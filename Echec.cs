using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;

namespace Jeux_Multiples
{
    public partial class Echec : Form
    {
        private Button[,] cases;
        private Piece[,] plateau;
        private bool tourBlanc = true;
        private Button caseSelectionnee = null;
        private Point positionSelectionnee;
        private List<Point> mouvementsPossibles;

        private static readonly Color CouleurClaire = Color.FromArgb(240, 217, 181);
        private static readonly Color CouleurFoncee = Color.FromArgb(181, 136, 99);
        private static readonly Color CouleurSurbrillance = Color.FromArgb(200, 230, 75, 180);
        private static readonly Color CouleurDeplacement = Color.FromArgb(150, 100, 200, 100);
        private static readonly Color CouleurCapture = Color.FromArgb(200, 220, 50, 50);
        private static readonly Color CouleurPanel = Color.FromArgb(58, 30, 8);
        private static readonly Color CouleurEchec = Color.FromArgb(255, 50, 50);

        private const int TAILLE_CASE = 76;
        private const int OFFSET_PLATEAU = 50;
        private const int MARGE_TOP = 80;

        public Echec()
        {
            InitializeComponent();
            InitialiserPlateau();
            InitialiserPieces();
        }

        private void Echec_Load(object sender, EventArgs e)
        {
            this.Text = "♔  Jeu d'Échecs  ♚";
            int largeur = TAILLE_CASE * 8 + OFFSET_PLATEAU * 2 + 20;
            int hauteur = TAILLE_CASE * 8 + OFFSET_PLATEAU * 2 + MARGE_TOP + 40;
            this.ClientSize = new Size(largeur, hauteur);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.BackColor = CouleurPanel;
            this.DoubleBuffered = true;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            Rectangle cadre = new Rectangle(
                OFFSET_PLATEAU - 8, MARGE_TOP - 8,
                TAILLE_CASE * 8 + 16, TAILLE_CASE * 8 + 16);

            using (LinearGradientBrush boisBrush = new LinearGradientBrush(
                cadre, Color.FromArgb(120, 70, 20), Color.FromArgb(60, 30, 5), 45f))
                g.FillRoundedRect(boisBrush, cadre, 8);

            using (Pen pen = new Pen(Color.FromArgb(180, 140, 60), 2))
                g.DrawRoundedRect(pen, cadre, 8);

            Font coordFont = new Font("Georgia", 10, FontStyle.Bold);
            for (int c = 0; c < 8; c++)
            {
                string lettre = ((char)('a' + c)).ToString();
                int x = OFFSET_PLATEAU + c * TAILLE_CASE + TAILLE_CASE / 2 - 5;
                g.DrawString(lettre, coordFont, Brushes.BurlyWood, x, MARGE_TOP + 8 * TAILLE_CASE + 4);
                g.DrawString(lettre, coordFont, Brushes.BurlyWood, x, MARGE_TOP - 20);
            }
            for (int l = 0; l < 8; l++)
            {
                string chiffre = (8 - l).ToString();
                int y = MARGE_TOP + l * TAILLE_CASE + TAILLE_CASE / 2 - 8;
                g.DrawString(chiffre, coordFont, Brushes.BurlyWood, OFFSET_PLATEAU - 20, y);
                g.DrawString(chiffre, coordFont, Brushes.BurlyWood, OFFSET_PLATEAU + 8 * TAILLE_CASE + 6, y);
            }
            coordFont.Dispose();
        }

        private void InitialiserPlateau()
        {
            cases = new Button[8, 8];
            plateau = new Piece[8, 8];
            mouvementsPossibles = new List<Point>();

            Panel header = new Panel
            {
                Size = new Size(TAILLE_CASE * 8 + OFFSET_PLATEAU * 2, MARGE_TOP - 10),
                Location = new Point(0, 0),
                BackColor = Color.Transparent
            };
            header.Controls.Add(new Label
            {
                Text = "ÉCHECS",
                Font = new Font("Georgia", 22, FontStyle.Bold | FontStyle.Italic),
                ForeColor = Color.FromArgb(220, 180, 80),
                AutoSize = true,
                Location = new Point(12, 10)
            });
            header.Controls.Add(new Label
            {
                Text = "● Tour des BLANCS",
                Font = new Font("Georgia", 12, FontStyle.Bold),
                ForeColor = Color.FromArgb(240, 230, 210),
                AutoSize = true,
                Name = "labelTour",
                Location = new Point(12, 42)
            });
            this.Controls.Add(header);

            for (int ligne = 0; ligne < 8; ligne++)
                for (int col = 0; col < 8; col++)
                {
                    Button btn = new Button
                    {
                        Size = new Size(TAILLE_CASE, TAILLE_CASE),
                        Location = new Point(OFFSET_PLATEAU + col * TAILLE_CASE, MARGE_TOP + ligne * TAILLE_CASE),
                        Font = new Font("Segoe UI Symbol", 36, FontStyle.Regular),
                        Tag = new Point(ligne, col),
                        FlatStyle = FlatStyle.Flat,
                        Cursor = Cursors.Hand,
                        TextAlign = ContentAlignment.MiddleCenter,
                        BackColor = ((ligne + col) % 2 == 0) ? CouleurClaire : CouleurFoncee
                    };
                    btn.FlatAppearance.BorderSize = 0;
                    btn.FlatAppearance.MouseOverBackColor = Color.Empty;
                    btn.Click += Case_Click;
                    cases[ligne, col] = btn;
                    this.Controls.Add(btn);
                }
        }

        private void InitialiserPieces()
        {
            PlacerRangee(0, CouleurPiece.Noir);
            PlacerRangee(7, CouleurPiece.Blanc);
            for (int i = 0; i < 8; i++)
            {
                PlacerPiece(1, i, new Piece(TypePiece.Pion, CouleurPiece.Noir));
                PlacerPiece(6, i, new Piece(TypePiece.Pion, CouleurPiece.Blanc));
            }
        }

        private void PlacerRangee(int rangee, CouleurPiece couleur)
        {
            TypePiece[] ordre = { TypePiece.Tour, TypePiece.Cavalier, TypePiece.Fou, TypePiece.Dame, TypePiece.Roi, TypePiece.Fou, TypePiece.Cavalier, TypePiece.Tour };
            for (int i = 0; i < 8; i++)
                PlacerPiece(rangee, i, new Piece(ordre[i], couleur));
        }

        private static readonly Dictionary<(TypePiece, CouleurPiece), string> Symboles =
            new Dictionary<(TypePiece, CouleurPiece), string>
        {
            {(TypePiece.Roi,      CouleurPiece.Blanc), "♔"}, {(TypePiece.Roi,      CouleurPiece.Noir), "♚"},
            {(TypePiece.Dame,     CouleurPiece.Blanc), "♕"}, {(TypePiece.Dame,     CouleurPiece.Noir), "♛"},
            {(TypePiece.Tour,     CouleurPiece.Blanc), "♖"}, {(TypePiece.Tour,     CouleurPiece.Noir), "♜"},
            {(TypePiece.Fou,      CouleurPiece.Blanc), "♗"}, {(TypePiece.Fou,      CouleurPiece.Noir), "♝"},
            {(TypePiece.Cavalier, CouleurPiece.Blanc), "♘"}, {(TypePiece.Cavalier, CouleurPiece.Noir), "♞"},
            {(TypePiece.Pion,     CouleurPiece.Blanc), "♙"}, {(TypePiece.Pion,     CouleurPiece.Noir), "♟"},
        };

        private void PlacerPiece(int l, int c, Piece piece)
        {
            plateau[l, c] = piece;
            cases[l, c].Text = Symboles[(piece.Type, piece.Couleur)];
            cases[l, c].ForeColor = (piece.Couleur == CouleurPiece.Blanc)
                ? Color.FromArgb(255, 250, 240)
                : Color.FromArgb(20, 10, 5);
        }

        // ── Clic ─────────────────────────────────────────────────────────────
        private void Case_Click(object sender, EventArgs e)
        {
            Button caseCliquee = sender as Button;
            Point pos = (Point)caseCliquee.Tag;
            int ligne = pos.X, col = pos.Y;

            if (caseSelectionnee == null)
            {
                if (plateau[ligne, col] != null)
                {
                    CouleurPiece cp = plateau[ligne, col].Couleur;
                    if ((tourBlanc && cp == CouleurPiece.Blanc) || (!tourBlanc && cp == CouleurPiece.Noir))
                    {
                        caseSelectionnee = caseCliquee;
                        positionSelectionnee = pos;
                        mouvementsPossibles = CalculerMouvementsLegaux(ligne, col);
                        caseSelectionnee.BackColor = CouleurSurbrillance;
                        AfficherMouvementsPossibles();
                    }
                }
            }
            else
            {
                if (EstMouvementValide(ligne, col))
                {
                    DeplacerPiece(positionSelectionnee.X, positionSelectionnee.Y, ligne, col);

                    // ── Promotion du pion ─────────────────────────────────────
                    VerifierPromotion(ligne, col);

                    tourBlanc = !tourBlanc;
                    MettreAJourLabelTour();

                    CouleurPiece couleurAdversaire = tourBlanc ? CouleurPiece.Blanc : CouleurPiece.Noir;
                    if (EstEnEchec(couleurAdversaire))
                    {
                        if (EstEchecEtMat(couleurAdversaire))
                        {
                            ResetterSelection();
                            AfficherEtatEchec(couleurAdversaire);
                            string gagnant = (couleurAdversaire == CouleurPiece.Blanc) ? "Noirs" : "Blancs";
                            var result = MessageBox.Show(
                                $"♚  ÉCHEC ET MAT !\n\nLes {gagnant} ont gagné la partie.\n\nVoulez-vous rejouer ?",
                                "Partie terminée", MessageBoxButtons.YesNo, MessageBoxIcon.Information);
                            if (result == DialogResult.Yes) ReinitialisierPartie();
                            else this.Close();
                            return;
                        }
                        else
                        {
                            AfficherEtatEchec(couleurAdversaire);
                            string joueur = (couleurAdversaire == CouleurPiece.Blanc) ? "Blancs" : "Noirs";
                            MessageBox.Show(
                                $"⚠  ÉCHEC !\n\nLe roi des {joueur} est en danger.\nVous devez le protéger !",
                                "Échec au Roi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        }
                    }
                }
                ResetterSelection();
            }
        }

        // ── Promotion du pion ─────────────────────────────────────────────────
        private void VerifierPromotion(int ligne, int col)
        {
            Piece piece = plateau[ligne, col];
            if (piece == null || piece.Type != TypePiece.Pion) return;

            bool promotionBlanc = (piece.Couleur == CouleurPiece.Blanc && ligne == 0);
            bool promotionNoir = (piece.Couleur == CouleurPiece.Noir && ligne == 7);

            if (!promotionBlanc && !promotionNoir) return;

            // Afficher le dialog de choix
            TypePiece choix = AfficherDialogPromotion(piece.Couleur);
            plateau[ligne, col] = new Piece(choix, piece.Couleur);
            cases[ligne, col].Text = Symboles[(choix, piece.Couleur)];
        }

        private TypePiece AfficherDialogPromotion(CouleurPiece couleur)
        {
            // ── Fenêtre de promotion style bois ──────────────────────────────
            Form dialog = new Form
            {
                Text = "Promotion du pion !",
                Size = new Size(420, 180),
                StartPosition = FormStartPosition.CenterParent,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                MaximizeBox = false,
                MinimizeBox = false,
                BackColor = CouleurPanel,
                ShowInTaskbar = false
            };

            Label titre = new Label
            {
                Text = "✨ Choisissez la pièce de promotion :",
                Font = new Font("Georgia", 11, FontStyle.Bold | FontStyle.Italic),
                ForeColor = Color.FromArgb(220, 180, 80),
                AutoSize = true,
                Location = new Point(12, 12)
            };
            dialog.Controls.Add(titre);

            // Pièces disponibles pour la promotion
            var options = new[]
            {
                (TypePiece.Dame,     couleur == CouleurPiece.Blanc ? "♕" : "♛", "Dame"),
                (TypePiece.Tour,     couleur == CouleurPiece.Blanc ? "♖" : "♜", "Tour"),
                (TypePiece.Fou,      couleur == CouleurPiece.Blanc ? "♗" : "♝", "Fou"),
                (TypePiece.Cavalier, couleur == CouleurPiece.Blanc ? "♘" : "♞", "Cavalier"),
            };

            TypePiece choixFinal = TypePiece.Dame;

            for (int i = 0; i < options.Length; i++)
            {
                var opt = options[i];
                Button btn = new Button
                {
                    Size = new Size(88, 88),
                    Location = new Point(10 + i * 98, 45),
                    Font = new Font("Segoe UI Symbol", 30, FontStyle.Regular),
                    Text = opt.Item2,
                    FlatStyle = FlatStyle.Flat,
                    BackColor = Color.FromArgb(240, 217, 181),
                    ForeColor = (couleur == CouleurPiece.Blanc)
                        ? Color.FromArgb(255, 250, 240)
                        : Color.FromArgb(20, 10, 5),
                    Cursor = Cursors.Hand,
                    Tag = opt.Item1,
                    TextAlign = ContentAlignment.MiddleCenter
                };
                btn.FlatAppearance.BorderColor = Color.FromArgb(180, 140, 60);
                btn.FlatAppearance.BorderSize = 2;

                // Tooltip avec le nom
                ToolTip tt = new ToolTip();
                tt.SetToolTip(btn, opt.Item3);

                btn.Click += (s, e) =>
                {
                    choixFinal = (TypePiece)((Button)s).Tag;
                    dialog.DialogResult = DialogResult.OK;
                    dialog.Close();
                };

                // Hover effect
                btn.MouseEnter += (s, e) => ((Button)s).BackColor = Color.FromArgb(220, 190, 130);
                btn.MouseLeave += (s, e) => ((Button)s).BackColor = Color.FromArgb(240, 217, 181);

                dialog.Controls.Add(btn);
            }

            dialog.ShowDialog(this);
            return choixFinal;
        }

        // ── Détection d'échec ─────────────────────────────────────────────────
        private bool EstEnEchec(CouleurPiece couleur)
        {
            Point posRoi = TrouverRoi(couleur);
            return EstCaseAttaquee(posRoi.X, posRoi.Y, couleur, plateau);
        }

        private bool EstEnEchecSurPlateau(CouleurPiece couleur, Piece[,] etat)
        {
            Point posRoi = TrouverRoiSurPlateau(couleur, etat);
            return EstCaseAttaquee(posRoi.X, posRoi.Y, couleur, etat);
        }

        private Point TrouverRoi(CouleurPiece couleur) => TrouverRoiSurPlateau(couleur, plateau);

        private Point TrouverRoiSurPlateau(CouleurPiece couleur, Piece[,] etat)
        {
            for (int l = 0; l < 8; l++)
                for (int c = 0; c < 8; c++)
                    if (etat[l, c] != null && etat[l, c].Type == TypePiece.Roi && etat[l, c].Couleur == couleur)
                        return new Point(l, c);
            return new Point(-1, -1);
        }

        private bool EstCaseAttaquee(int l, int c, CouleurPiece couleur, Piece[,] etat)
        {
            CouleurPiece adv = (couleur == CouleurPiece.Blanc) ? CouleurPiece.Noir : CouleurPiece.Blanc;
            for (int al = 0; al < 8; al++)
                for (int ac = 0; ac < 8; ac++)
                    if (etat[al, ac] != null && etat[al, ac].Couleur == adv)
                        if (CalculerAttaquesBrutes(al, ac, adv, etat).Any(p => p.X == l && p.Y == c))
                            return true;
            return false;
        }

        private List<Point> CalculerAttaquesBrutes(int l, int c, CouleurPiece couleur, Piece[,] etat)
        {
            switch (etat[l, c].Type)
            {
                case TypePiece.Pion: return AttaquesPion(l, c, couleur);
                case TypePiece.Tour: return GlissementEtat(l, c, couleur, etat, new int[,] { { -1, 0 }, { 1, 0 }, { 0, -1 }, { 0, 1 } });
                case TypePiece.Fou: return GlissementEtat(l, c, couleur, etat, new int[,] { { -1, -1 }, { -1, 1 }, { 1, -1 }, { 1, 1 } });
                case TypePiece.Dame: return GlissementEtat(l, c, couleur, etat, new int[,] { { -1, 0 }, { 1, 0 }, { 0, -1 }, { 0, 1 }, { -1, -1 }, { -1, 1 }, { 1, -1 }, { 1, 1 } });
                case TypePiece.Cavalier: return SautsEtat(l, c, couleur, etat);
                case TypePiece.Roi: return RoiEtat(l, c, couleur, etat);
                default: return new List<Point>();
            }
        }

        private List<Point> AttaquesPion(int l, int c, CouleurPiece couleur)
        {
            var pts = new List<Point>();
            int dir = (couleur == CouleurPiece.Blanc) ? -1 : 1;
            foreach (int dc in new[] { -1, 1 })
                if (OK(l + dir, c + dc)) pts.Add(new Point(l + dir, c + dc));
            return pts;
        }

        private List<Point> GlissementEtat(int l, int c, CouleurPiece couleur, Piece[,] etat, int[,] dirs)
        {
            var mvts = new List<Point>();
            for (int d = 0; d < dirs.GetLength(0); d++)
                for (int i = 1; i < 8; i++)
                {
                    int nl = l + dirs[d, 0] * i, nc = c + dirs[d, 1] * i;
                    if (!OK(nl, nc)) break;
                    if (etat[nl, nc] == null) { mvts.Add(new Point(nl, nc)); continue; }
                    if (etat[nl, nc].Couleur != couleur) mvts.Add(new Point(nl, nc));
                    break;
                }
            return mvts;
        }

        private List<Point> SautsEtat(int l, int c, CouleurPiece couleur, Piece[,] etat)
        {
            var mvts = new List<Point>();
            int[,] sauts = { { -2, -1 }, { -2, 1 }, { -1, -2 }, { -1, 2 }, { 1, -2 }, { 1, 2 }, { 2, -1 }, { 2, 1 } };
            for (int i = 0; i < 8; i++)
            {
                int nl = l + sauts[i, 0], nc = c + sauts[i, 1];
                if (OK(nl, nc) && (etat[nl, nc] == null || etat[nl, nc].Couleur != couleur))
                    mvts.Add(new Point(nl, nc));
            }
            return mvts;
        }

        private List<Point> RoiEtat(int l, int c, CouleurPiece couleur, Piece[,] etat)
        {
            var mvts = new List<Point>();
            int[,] dirs = { { -1, -1 }, { -1, 0 }, { -1, 1 }, { 0, -1 }, { 0, 1 }, { 1, -1 }, { 1, 0 }, { 1, 1 } };
            for (int i = 0; i < 8; i++)
            {
                int nl = l + dirs[i, 0], nc = c + dirs[i, 1];
                if (OK(nl, nc) && (etat[nl, nc] == null || etat[nl, nc].Couleur != couleur))
                    mvts.Add(new Point(nl, nc));
            }
            return mvts;
        }

        private List<Point> CalculerMouvementsLegaux(int l, int c)
        {
            Piece piece = plateau[l, c];
            List<Point> bruts = CalculerMouvementsBruts(l, c, piece.Couleur);
            var legaux = new List<Point>();
            foreach (Point dest in bruts)
            {
                Piece[,] copie = CopierPlateau();
                copie[dest.X, dest.Y] = copie[l, c];
                copie[l, c] = null;
                if (!EstEnEchecSurPlateau(piece.Couleur, copie))
                    legaux.Add(dest);
            }
            return legaux;
        }

        private List<Point> CalculerMouvementsBruts(int l, int c, CouleurPiece couleur)
        {
            switch (plateau[l, c].Type)
            {
                case TypePiece.Pion: return MouvementsPion(l, c, couleur);
                case TypePiece.Tour: return GlissementEtat(l, c, couleur, plateau, new int[,] { { -1, 0 }, { 1, 0 }, { 0, -1 }, { 0, 1 } });
                case TypePiece.Fou: return GlissementEtat(l, c, couleur, plateau, new int[,] { { -1, -1 }, { -1, 1 }, { 1, -1 }, { 1, 1 } });
                case TypePiece.Dame: return GlissementEtat(l, c, couleur, plateau, new int[,] { { -1, 0 }, { 1, 0 }, { 0, -1 }, { 0, 1 }, { -1, -1 }, { -1, 1 }, { 1, -1 }, { 1, 1 } });
                case TypePiece.Cavalier: return SautsEtat(l, c, couleur, plateau);
                case TypePiece.Roi: return RoiEtat(l, c, couleur, plateau);
                default: return new List<Point>();
            }
        }

        private List<Point> MouvementsPion(int l, int c, CouleurPiece couleur)
        {
            var mvts = new List<Point>();
            int dir = (couleur == CouleurPiece.Blanc) ? -1 : 1;
            if (OK(l + dir, c) && plateau[l + dir, c] == null)
            {
                mvts.Add(new Point(l + dir, c));
                if (((couleur == CouleurPiece.Blanc && l == 6) || (couleur == CouleurPiece.Noir && l == 1))
                    && plateau[l + 2 * dir, c] == null)
                    mvts.Add(new Point(l + 2 * dir, c));
            }
            foreach (int dc in new[] { -1, 1 })
                if (OK(l + dir, c + dc) && plateau[l + dir, c + dc] != null && plateau[l + dir, c + dc].Couleur != couleur)
                    mvts.Add(new Point(l + dir, c + dc));
            return mvts;
        }

        private bool EstEchecEtMat(CouleurPiece couleur)
        {
            for (int l = 0; l < 8; l++)
                for (int c = 0; c < 8; c++)
                    if (plateau[l, c] != null && plateau[l, c].Couleur == couleur)
                        if (CalculerMouvementsLegaux(l, c).Count > 0)
                            return false;
            return true;
        }

        private Piece[,] CopierPlateau()
        {
            Piece[,] copie = new Piece[8, 8];
            for (int l = 0; l < 8; l++)
                for (int c = 0; c < 8; c++)
                    copie[l, c] = plateau[l, c];
            return copie;
        }

        private void AfficherEtatEchec(CouleurPiece couleur)
        {
            Point posRoi = TrouverRoi(couleur);
            if (posRoi.X >= 0) cases[posRoi.X, posRoi.Y].BackColor = CouleurEchec;
        }

        private void AfficherMouvementsPossibles()
        {
            foreach (Point m in mouvementsPossibles)
                cases[m.X, m.Y].BackColor = plateau[m.X, m.Y] != null ? CouleurCapture : CouleurDeplacement;
        }

        private void ResetterSelection()
        {
            for (int l = 0; l < 8; l++)
                for (int c = 0; c < 8; c++)
                    cases[l, c].BackColor = ((l + c) % 2 == 0) ? CouleurClaire : CouleurFoncee;

            CouleurPiece couleurCourante = tourBlanc ? CouleurPiece.Blanc : CouleurPiece.Noir;
            if (EstEnEchec(couleurCourante)) AfficherEtatEchec(couleurCourante);

            caseSelectionnee = null;
            mouvementsPossibles?.Clear();
        }

        private void MettreAJourLabelTour()
        {
            Label lbl = this.Controls.Find("labelTour", true)[0] as Label;
            lbl.Text = tourBlanc ? "● Tour des BLANCS" : "● Tour des NOIRS";
            lbl.ForeColor = tourBlanc ? Color.FromArgb(240, 230, 210) : Color.FromArgb(180, 140, 80);
        }

        private void DeplacerPiece(int al, int ac, int nl, int nc)
        {
            plateau[nl, nc] = plateau[al, ac];
            plateau[al, ac] = null;
            cases[nl, nc].Text = cases[al, ac].Text;
            cases[nl, nc].ForeColor = cases[al, ac].ForeColor;
            cases[al, ac].Text = "";
        }

        private bool OK(int l, int c) => l >= 0 && l < 8 && c >= 0 && c < 8;
        private bool EstMouvementValide(int l, int c) => mouvementsPossibles.Any(p => p.X == l && p.Y == c);

        private void ReinitialisierPartie()
        {
            tourBlanc = true;
            caseSelectionnee = null;
            mouvementsPossibles = new List<Point>();
            for (int l = 0; l < 8; l++)
                for (int c = 0; c < 8; c++)
                {
                    plateau[l, c] = null;
                    cases[l, c].Text = "";
                    cases[l, c].BackColor = ((l + c) % 2 == 0) ? CouleurClaire : CouleurFoncee;
                }
            InitialiserPieces();
            MettreAJourLabelTour();
        }
    }

    // ── Extensions Graphics ───────────────────────────────────────────────────
    public static class GraphicsExtensions
    {
        public static void FillRoundedRect(this Graphics g, Brush brush, Rectangle r, int radius)
        {
            using (GraphicsPath path = RoundedPath(r, radius)) g.FillPath(brush, path);
        }
        public static void DrawRoundedRect(this Graphics g, Pen pen, Rectangle r, int radius)
        {
            using (GraphicsPath path = RoundedPath(r, radius)) g.DrawPath(pen, path);
        }
        private static GraphicsPath RoundedPath(Rectangle r, int rad)
        {
            GraphicsPath p = new GraphicsPath();
            p.AddArc(r.Left, r.Top, rad * 2, rad * 2, 180, 90);
            p.AddArc(r.Right - rad * 2, r.Top, rad * 2, rad * 2, 270, 90);
            p.AddArc(r.Right - rad * 2, r.Bottom - rad * 2, rad * 2, rad * 2, 0, 90);
            p.AddArc(r.Left, r.Bottom - rad * 2, rad * 2, rad * 2, 90, 90);
            p.CloseFigure();
            return p;
        }
    }

    public class Piece
    {
        public TypePiece Type { get; set; }
        public CouleurPiece Couleur { get; set; }
        public Piece(TypePiece type, CouleurPiece couleur) { Type = type; Couleur = couleur; }
    }

    public enum TypePiece { Pion, Tour, Cavalier, Fou, Dame, Roi }
    public enum CouleurPiece { Blanc, Noir }
}