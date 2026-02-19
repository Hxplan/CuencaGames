using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;

namespace Jeux_Multiples
{
    // ═══════════════════════════════════════════════════════════════════════════
    //  ÉCRAN D'ACCUEIL – Sélection du nombre de joueurs
    // ═══════════════════════════════════════════════════════════════════════════
    public class EcranAccueil : Form
    {
        public int NombreJoueurs { get; private set; } = 0;

        private static readonly Color[] CouleursJoueur = {
            Color.FromArgb(220, 50,  50),
            Color.FromArgb( 30,120, 220),
            Color.FromArgb( 40,170,  60),
            Color.FromArgb(230,180,  20),
        };
        private static readonly string[] NomsJoueur = { "Rouge", "Bleu", "Vert", "Jaune" };

        public EcranAccueil()
        {
            this.Text = "🐴 Les 4 Petits Chevaux";
            this.ClientSize = new Size(480, 420);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.BackColor = Color.FromArgb(35, 30, 45);
            this.DoubleBuffered = true;

            // Titre
            Label titre = new Label
            {
                Text = "🐴  LES 4 PETITS CHEVAUX",
                Font = new Font("Georgia", 18, FontStyle.Bold | FontStyle.Italic),
                ForeColor = Color.FromArgb(220, 180, 80),
                AutoSize = false,
                Size = new Size(460, 50),
                Location = new Point(10, 20),
                TextAlign = ContentAlignment.MiddleCenter
            };
            this.Controls.Add(titre);

            Label sousTitre = new Label
            {
                Text = "Combien de joueurs participent ?",
                Font = new Font("Georgia", 13, FontStyle.Italic),
                ForeColor = Color.FromArgb(200, 200, 200),
                AutoSize = false,
                Size = new Size(460, 35),
                Location = new Point(10, 75),
                TextAlign = ContentAlignment.MiddleCenter
            };
            this.Controls.Add(sousTitre);

            // Boutons 2, 3, 4 joueurs
            int[] choix = { 2, 3, 4 };
            for (int idx = 0; idx < choix.Length; idx++)
            {
                int nb = choix[idx];
                Button btn = new Button
                {
                    Size = new Size(120, 120),
                    Location = new Point(30 + idx * 145, 125),
                    FlatStyle = FlatStyle.Flat,
                    BackColor = Color.FromArgb(55, 45, 75),
                    ForeColor = Color.White,
                    Cursor = Cursors.Hand,
                    Tag = nb
                };
                btn.FlatAppearance.BorderColor = Color.FromArgb(160, 130, 220);
                btn.FlatAppearance.BorderSize = 2;
                btn.Click += (s, e) =>
                {
                    NombreJoueurs = (int)((Button)s).Tag;
                    this.DialogResult = DialogResult.OK;
                    this.Close();
                };
                btn.MouseEnter += (s, e) => ((Button)s).BackColor = Color.FromArgb(80, 65, 110);
                btn.MouseLeave += (s, e) => ((Button)s).BackColor = Color.FromArgb(55, 45, 75);
                btn.Paint += (s, pe) => DessinerBoutonJoueurs(pe.Graphics, (Button)s, nb);
                this.Controls.Add(btn);
            }

            // Preview des pions par configuration
            Label info = new Label
            {
                Text = "Les joueurs inactifs sont contrôlés automatiquement par l'ordinateur.",
                Font = new Font("Georgia", 9, FontStyle.Italic),
                ForeColor = Color.FromArgb(150, 150, 150),
                AutoSize = false,
                Size = new Size(440, 40),
                Location = new Point(20, 265),
                TextAlign = ContentAlignment.MiddleCenter
            };
            this.Controls.Add(info);

            // Légende couleurs
            string[] ordre = { "Rouge", "Bleu", "Vert", "Jaune" };
            for (int j = 0; j < 4; j++)
            {
                Panel p = new Panel
                {
                    Size = new Size(18, 18),
                    Location = new Point(60 + j * 95, 320),
                    BackColor = CouleursJoueur[j]
                };
                this.Controls.Add(p);
                Label l = new Label
                {
                    Text = ordre[j],
                    Font = new Font("Georgia", 10, FontStyle.Bold),
                    ForeColor = CouleursJoueur[j],
                    AutoSize = true,
                    Location = new Point(82 + j * 95, 318)
                };
                this.Controls.Add(l);
            }

            Label legendInfo = new Label
            {
                Text = "Ordre de jeu :",
                Font = new Font("Georgia", 9),
                ForeColor = Color.FromArgb(180, 180, 180),
                AutoSize = true,
                Location = new Point(20, 322)
            };
            this.Controls.Add(legendInfo);
        }

        private void DessinerBoutonJoueurs(Graphics g, Button btn, int nb)
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            int cx = btn.Width / 2, cy = btn.Height / 2;

            // Nombre grand
            using (Font f = new Font("Georgia", 36, FontStyle.Bold))
            using (StringFormat sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center })
                g.DrawString(nb.ToString(), f, Brushes.White, new RectangleF(0, 0, btn.Width, btn.Height - 30), sf);

            // Texte "joueurs"
            using (Font f2 = new Font("Georgia", 10))
            using (StringFormat sf2 = new StringFormat { Alignment = StringAlignment.Center })
                g.DrawString("joueur" + (nb > 1 ? "s" : ""), f2,
                    new SolidBrush(Color.FromArgb(180, 180, 180)),
                    new RectangleF(0, btn.Height - 30, btn.Width, 25), sf2);

            // Petits ronds couleurs
            int r2 = 8;
            var positions = new (int x, int y)[] { (cx - 12, cy + 20), (cx + 12, cy + 20), (cx - 12, cy + 38), (cx + 12, cy + 38) };
            for (int i = 0; i < nb && i < 4; i++)
            {
                g.FillEllipse(new SolidBrush(CouleursJoueur[i]),
                    positions[i].x - r2, positions[i].y - r2, r2 * 2, r2 * 2);
            }
        }
    }

    // ═══════════════════════════════════════════════════════════════════════════
    //  FORMULAIRE PRINCIPAL – Jeu des 4 petits chevaux
    // ═══════════════════════════════════════════════════════════════════════════
    public partial class _4Cheveaux : Form
    {
        // ── Constantes de layout ──────────────────────────────────────────────
        private const int T = 52;   // taille case
        private const int MG = 10;   // marge
        private const int TP = 20;   // taille pion (rayon = TP/2)

        // ── Couleurs ──────────────────────────────────────────────────────────
        private static readonly Color[] CJ = {
            Color.FromArgb(220, 50,  50),
            Color.FromArgb( 30,120, 220),
            Color.FromArgb( 40,170,  60),
            Color.FromArgb(230,180,  20),
        };
        private static readonly string[] NJ = { "Rouge", "Bleu", "Vert", "Jaune" };
        private static readonly Color FondForm = Color.FromArgb(35, 30, 45);

        // ── Règles ────────────────────────────────────────────────────────────
        // Circuit : 52 cases, sens horaire
        // Sortie par joueur : 0(Rouge), 13(Bleu), 26(Vert), 39(Jaune)
        private static readonly int[] CaseSortie = { 0, 13, 26, 39 };
        private static readonly int[] CaseEntreeFinale = { 51, 12, 25, 38 };
        private const int NB_CIRCUIT = 52;
        private const int NB_COULOIR = 6;

        // ── Coordonnées pixel pré-calculées ───────────────────────────────────
        private Point[] coordCircuit = new Point[NB_CIRCUIT];
        private Point[,] coordCouloir = new Point[4, NB_COULOIR];
        private Point[,] coordEcurie = new Point[4, 4];

        // ── État du jeu ───────────────────────────────────────────────────────
        private int nbJoueurs = 4;
        private int joueurCourant = 0;
        private int valeurDe = 0;
        private bool dejaLance = false;
        private int[,] posPions;      // [4,4] : -1=écurie, 0..51=circuit, 100..=couloir
        private bool[,] pionArrive;    // [4,4] : arrivé à la maison
        private List<int> pionsJouables = new List<int>();
        private Random rng = new Random();

        // ── Widgets ───────────────────────────────────────────────────────────
        private Panel panPlateau;
        private Button btnLancer;
        private Label lblTour, lblDe, lblMsg;

        // ══════════════════════════════════════════════════════════════════════
        public _4Cheveaux()
        {
            InitializeComponent();
        }

        private void _4Cheveaux_Load(object sender, EventArgs e)
        {
            // Écran de sélection du nombre de joueurs
            using (EcranAccueil accueil = new EcranAccueil())
            {
                if (accueil.ShowDialog() != DialogResult.OK)
                { this.Close(); return; }
                nbJoueurs = accueil.NombreJoueurs;
            }

            ConfigForm();
            PreCalcCoord();
            BuildUI();
            InitJeu();
            MettreAJourTour();
        }

        private void ConfigForm()
        {
            this.Text = "🐴  Les 4 Petits Chevaux";
            this.BackColor = FondForm;
            this.DoubleBuffered = true;
            FormUtils.ApplyFullScreen(this);
        }

        // ══════════════════════════════════════════════════════════════════════
        //  PRÉ-CALCUL COORDONNÉES
        // ══════════════════════════════════════════════════════════════════════
        private void PreCalcCoord()
        {
            // 52 cases du circuit (sens horaire), grille 11×11
            var c52 = new (int col, int lig)[]
            {
                (4,10),(4,9),(4,8),(4,7),(4,6),       // 0-4   Rouge sort: 0
                (3,6),(2,6),(1,6),(0,6),               // 5-8
                (0,5),(0,4),                           // 9-10
                (1,4),(2,4),(3,4),                     // 11-13  Bleu sort: 13
                (4,4),(4,3),(4,2),(4,1),(4,0),         // 14-18
                (5,0),(6,0),                           // 19-20
                (6,1),(6,2),(6,3),(6,4),               // 21-24
                (7,4),(8,4),                           // 25-26  Vert sort: 26
                (9,4),(10,4),(10,5),(10,6),            // 27-30
                (9,6),(8,6),(7,6),(6,6),               // 31-34
                (6,7),(6,8),(6,9),(6,10),              // 35-38
                (5,10),                                // 39    Jaune sort: 39
                (5,9),(5,8),(5,7),(5,6),               // 40-43
                (5,5),(4,5),(3,5),(2,5),(1,5),(0,5),   // 44-49
                (5,4),(5,3),(5,2),(5,1),               // 50-53 → on prend 50-51
            };
            for (int i = 0; i < NB_CIRCUIT && i < c52.Length; i++)
                coordCircuit[i] = CP(c52[i].col, c52[i].lig);

            // Couloirs finaux (6 cases chacun)
            var coul = new (int c, int l)[4, NB_COULOIR]
            {
                // Rouge : monte col 5, lignes 9→4
                { (5,9),(5,8),(5,7),(5,6),(5,5),(5,4) },
                // Bleu : ligne 5, cols 1→5, puis centre
                { (1,5),(2,5),(3,5),(4,5),(5,5),(5,4) },
                // Vert : descend col 5, lignes 1→6
                { (5,1),(5,2),(5,3),(5,4),(5,5),(5,6) },
                // Jaune : ligne 5, cols 9→5
                { (9,5),(8,5),(7,5),(6,5),(5,5),(5,6) },
            };
            for (int j = 0; j < 4; j++)
                for (int k = 0; k < NB_COULOIR; k++)
                    coordCouloir[j, k] = CP(coul[j, k].c, coul[j, k].l);

            // Écuries (4 cases dans chaque coin 3×3)
            // Rouge  : bas-gauche
            coordEcurie[0, 0] = CP(1, 9); coordEcurie[0, 1] = CP(2, 9);
            coordEcurie[0, 2] = CP(1, 10); coordEcurie[0, 3] = CP(2, 10);
            // Bleu   : haut-gauche
            coordEcurie[1, 0] = CP(1, 1); coordEcurie[1, 1] = CP(2, 1);
            coordEcurie[1, 2] = CP(1, 2); coordEcurie[1, 3] = CP(2, 2);
            // Vert   : haut-droit
            coordEcurie[2, 0] = CP(8, 1); coordEcurie[2, 1] = CP(9, 1);
            coordEcurie[2, 2] = CP(8, 2); coordEcurie[2, 3] = CP(9, 2);
            // Jaune  : bas-droit
            coordEcurie[3, 0] = CP(8, 9); coordEcurie[3, 1] = CP(9, 9);
            coordEcurie[3, 2] = CP(8, 10); coordEcurie[3, 3] = CP(9, 10);
        }

        private Point CP(int col, int lig) =>
            new Point(MG + col * T + T / 2, MG + lig * T + T / 2);

        // ══════════════════════════════════════════════════════════════════════
        //  UI
        // ══════════════════════════════════════════════════════════════════════
        private void BuildUI()
        {
            // Calculate content dimensions
            int platW = T * 11 + MG * 2;
            int sidePanelW = 280;
            int totalContentW = platW + sidePanelW + 20; // 20px gap
            int totalContentH = platW; // Height is determined by the board

            // Center positions
            int startX = (this.ClientSize.Width - totalContentW) / 2;
            int startY = (this.ClientSize.Height - totalContentH) / 2;
            
            // Ensure minimums
            if (startX < 20) startX = 20;
            if (startY < 20) startY = 20;

            // Back Button
            FormUtils.CreateBackButton(this, () => this.Close());

            // Board Panel
            panPlateau = new Panel
            {
                Location = new Point(startX, startY),
                Size = new Size(platW, platW), // Square
                BackColor = Color.Transparent
            };
            panPlateau.Paint += Plateau_Paint;
            panPlateau.MouseClick += Plateau_MouseClick;
            this.Controls.Add(panPlateau);

            int sx = startX + platW + 20;
            int sy = startY;

            lblTour = new Label
            {
                Location = new Point(sx, sy + 20),
                Size = new Size(sidePanelW, 45),
                Font = new Font("Georgia", 15, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = Color.Transparent,
                Text = "",
                TextAlign = ContentAlignment.MiddleCenter
            };
            this.Controls.Add(lblTour);

            lblDe = new Label
            {
                Location = new Point(sx + (sidePanelW - 70)/2, sy + 72),
                Size = new Size(70, 70),
                Font = new Font("Segoe UI Symbol", 40),
                ForeColor = Color.White,
                BackColor = Color.Transparent,
                Text = "⚀",
                TextAlign = ContentAlignment.MiddleCenter
            };
            this.Controls.Add(lblDe);

            btnLancer = new Button
            {
                Location = new Point(sx + (sidePanelW - 170)/2, sy + 150),
                Size = new Size(170, 48),
                Text = "🎲  Lancer le dé",
                Font = new Font("Georgia", 11, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                ForeColor = Color.White,
                Cursor = Cursors.Hand
            };
            btnLancer.FlatAppearance.BorderSize = 2;
            btnLancer.FlatAppearance.BorderColor = Color.FromArgb(160, 130, 220);
            btnLancer.Click += BtnLancer_Click;
            this.Controls.Add(btnLancer);

            lblMsg = new Label
            {
                Location = new Point(sx, sy + 215),
                Size = new Size(sidePanelW, 130),
                Font = new Font("Georgia", 10, FontStyle.Italic),
                ForeColor = Color.FromArgb(205, 205, 205),
                BackColor = Color.Transparent,
                TextAlign = ContentAlignment.TopCenter
            };
            this.Controls.Add(lblMsg);

            // Légende joueurs
            int legendStartY = sy + 360;
            for (int j = 0; j < nbJoueurs; j++)
            {
                Panel pastille = new Panel
                {
                    Location = new Point(sx + 20, legendStartY + j * 42),
                    Size = new Size(20, 20),
                    BackColor = CJ[j]
                };
                this.Controls.Add(pastille);

                Label lj = new Label
                {
                    Location = new Point(sx + 48, legendStartY - 2 + j * 42),
                    Size = new Size(220, 26),
                    Font = new Font("Georgia", 11, FontStyle.Bold),
                    ForeColor = CJ[j],
                    BackColor = Color.Transparent,
                    Text = NJ[j] + " — 0/4 pions",
                    Name = $"lblJoueur{j}"
                };
                this.Controls.Add(lj);
            }
        }

        // ══════════════════════════════════════════════════════════════════════
        //  INIT JEU
        // ══════════════════════════════════════════════════════════════════════
        private void InitJeu()
        {
            posPions = new int[4, 4];
            pionArrive = new bool[4, 4];
            for (int j = 0; j < 4; j++)
                for (int p = 0; p < 4; p++)
                {
                    posPions[j, p] = -1;
                    pionArrive[j, p] = false;
                }
        }

        // ══════════════════════════════════════════════════════════════════════
        //  DESSIN
        // ══════════════════════════════════════════════════════════════════════
        private void Plateau_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;

            // Fond crème
            g.FillRectangle(new SolidBrush(Color.FromArgb(245, 238, 215)), 0, 0, panPlateau.Width, panPlateau.Height);
            DessinerCases(g);
            DessinerPions(g);
        }

        private void DessinerCases(Graphics g)
        {
            for (int lig = 0; lig < 11; lig++)
                for (int col = 0; col < 11; col++)
                {
                    Color bg = CouleurCase(col, lig);
                    if (bg == Color.Empty) continue;
                    Rectangle r = new Rectangle(MG + col * T, MG + lig * T, T, T);
                    g.FillRectangle(new SolidBrush(bg), r);
                    g.DrawRectangle(new Pen(Color.FromArgb(170, 150, 120), 1f), r);
                }

            // Étoiles sur cases de sortie
            for (int j = 0; j < 4; j++)
                DessinerEtoile(g, coordCircuit[CaseSortie[j]], CJ[j], 16);

            // Centre
            DessinerEtoile(g, CP(5, 5), Color.FromArgb(200, 175, 50), T - 8);
        }

        private Color CouleurCase(int col, int lig)
        {
            if (col <= 2 && lig >= 8) return Color.FromArgb(255, 200, 200); // Rouge
            if (col <= 2 && lig <= 2) return Color.FromArgb(180, 210, 255); // Bleu
            if (col >= 8 && lig <= 2) return Color.FromArgb(180, 245, 190); // Vert
            if (col >= 8 && lig >= 8) return Color.FromArgb(255, 245, 160); // Jaune

            // Couloirs intérieurs colorés
            if (col == 5 && lig >= 1 && lig <= 4) return Color.FromArgb(255, 210, 210);
            if (lig == 5 && col >= 1 && col <= 4) return Color.FromArgb(190, 215, 255);
            if (col == 5 && lig >= 6 && lig <= 9) return Color.FromArgb(190, 245, 195);
            if (lig == 5 && col >= 6 && col <= 9) return Color.FromArgb(255, 248, 170);
            if (col == 5 && lig == 5) return Color.FromArgb(230, 220, 195);

            // Cases du circuit
            bool circuit =
                (col == 4 && lig >= 4 && lig <= 10) || (col == 6 && lig >= 0 && lig <= 6) ||
                (lig == 4 && col >= 0 && col <= 4) || (lig == 6 && col >= 6 && col <= 10) ||
                (col == 4 && lig >= 0 && lig <= 3) || (lig == 0 && col >= 4 && col <= 6) ||
                (col == 10 && lig >= 4 && lig <= 6) || (lig == 10 && col >= 4 && col <= 6) ||
                (col == 0 && lig >= 4 && lig <= 6) || (lig == 4 && col >= 6 && col <= 10) ||
                (lig == 6 && col >= 0 && col <= 4) || (col == 6 && lig >= 7 && lig <= 10);

            if (circuit) return Color.FromArgb(245, 238, 215);
            return Color.Empty;
        }

        private void DessinerEtoile(Graphics g, Point c, Color coul, int taille)
        {
            int outer = taille / 2 - 2, inner = outer / 2;
            var pts = new PointF[10];
            for (int i = 0; i < 10; i++)
            {
                double a = Math.PI / 5 * i - Math.PI / 2;
                double r = (i % 2 == 0) ? outer : inner;
                pts[i] = new PointF((float)(c.X + r * Math.Cos(a)), (float)(c.Y + r * Math.Sin(a)));
            }
            using (SolidBrush b = new SolidBrush(Color.FromArgb(100, coul)))
                g.FillPolygon(b, pts);
        }

        private void DessinerPions(Graphics g)
        {
            for (int j = 0; j < nbJoueurs; j++)
                for (int p = 0; p < 4; p++)
                {
                    if (pionArrive[j, p]) continue;
                    Point px = PixelPion(j, p);
                    bool jouable = (j == joueurCourant) && dejaLance && pionsJouables.Contains(p);
                    DessinerUnPion(g, px, CJ[j], jouable, p + 1);
                }
        }

        private Point PixelPion(int j, int p)
        {
            int pos = posPions[j, p];
            if (pos == -1)
            {
                // Décalage subtil pour que les 4 pions ne se superposent pas
                Point e = coordEcurie[j, p];
                return e;
            }
            if (pos >= 100)
            {
                int idx = Math.Min(pos - 100, NB_COULOIR - 1);
                Point c = coordCouloir[j, idx];
                // offset si plusieurs pions au même endroit
                int off = OffsetMeme(j, p, pos);
                return new Point(c.X + off * 5, c.Y + off * 5);
            }
            Point cp = coordCircuit[pos % NB_CIRCUIT];
            int off2 = OffsetMeme(j, p, pos);
            return new Point(cp.X + off2 * 5, cp.Y + off2 * 5);
        }

        private int OffsetMeme(int j, int p, int pos)
        {
            int count = 0;
            for (int p2 = 0; p2 < p; p2++)
                if (posPions[j, p2] == pos) count++;
            return count;
        }

        private void DessinerUnPion(Graphics g, Point c, Color coul, bool jouable, int num)
        {
            int r = TP / 2;
            // Ombre
            g.FillEllipse(new SolidBrush(Color.FromArgb(70, 0, 0, 0)),
                c.X - r + 2, c.Y - r + 2, TP, TP);
            // Corps dégradé
            using (GraphicsPath path = new GraphicsPath())
            {
                path.AddEllipse(c.X - r, c.Y - r, TP, TP);
                using (PathGradientBrush pb = new PathGradientBrush(path))
                {
                    pb.CenterColor = ControlPaint.Light(coul, 0.5f);
                    pb.SurroundColors = new[] { coul };
                    g.FillPath(pb, path);
                }
            }
            // Contour
            using (Pen pen = new Pen(jouable ? Color.Gold : Color.FromArgb(80, 0, 0, 0), jouable ? 2.5f : 1.5f))
                g.DrawEllipse(pen, c.X - r, c.Y - r, TP, TP);
            // Numéro
            using (Font f = new Font("Georgia", 7, FontStyle.Bold))
            using (StringFormat sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center })
                g.DrawString(num.ToString(), f, Brushes.White, new RectangleF(c.X - r, c.Y - r, TP, TP), sf);
            // Halo si jouable
            if (jouable)
                for (int k = 1; k <= 3; k++)
                    using (Pen h = new Pen(Color.FromArgb(55 * (4 - k), Color.Gold), k * 2))
                        g.DrawEllipse(h, c.X - r - k * 2, c.Y - r - k * 2, TP + k * 4, TP + k * 4);
        }

        // ══════════════════════════════════════════════════════════════════════
        //  LOGIQUE
        // ══════════════════════════════════════════════════════════════════════
        private void BtnLancer_Click(object sender, EventArgs e)
        {
            if (dejaLance) { lblMsg.Text = "Déjà lancé ! Sélectionnez un pion."; return; }

            btnLancer.Enabled = false;
            string[] faces = { "⚀", "⚁", "⚂", "⚃", "⚄", "⚅" };
            int ticks = 0;
            Timer anim = new Timer { Interval = 75 };
            anim.Tick += (s, ev) =>
            {
                lblDe.Text = faces[rng.Next(6)];
                if (++ticks >= 10)
                {
                    anim.Stop();
                    valeurDe = rng.Next(1, 7);
                    lblDe.Text = faces[valeurDe - 1];
                    ApresLancer();
                    btnLancer.Enabled = true;
                }
            };
            anim.Start();
        }

        private void ApresLancer()
        {
            dejaLance = true;
            pionsJouables = TrouverPionsJouables(joueurCourant, valeurDe);

            if (pionsJouables.Count == 0)
            {
                lblMsg.Text = $"Dé : {valeurDe} — Aucun mouvement possible.";
                PasserTour(false);
                return;
            }

            // ── Si dé = 6 et il y a un pion en écurie → sortie AUTOMATIQUE ──
            if (valeurDe == 6)
            {
                int pionEnEcurie = -1;
                for (int p = 0; p < 4; p++)
                    if (posPions[joueurCourant, p] == -1) { pionEnEcurie = p; break; }

                if (pionEnEcurie >= 0)
                {
                    // Sortie automatique du premier pion en écurie
                    SortirPionEcurie(joueurCourant, pionEnEcurie);
                    return;
                }
            }

            // Sinon : attendre le clic du joueur
            string msg = $"Dé : {valeurDe}\n";
            msg += (pionsJouables.Count == 1)
                ? "Cliquez le pion pour le déplacer."
                : "Cliquez le pion à déplacer.";
            lblMsg.Text = msg;
            panPlateau.Invalidate();
        }

        private void SortirPionEcurie(int joueur, int pion)
        {
            posPions[joueur, pion] = CaseSortie[joueur];
            lblMsg.Text = $"🐴 Un 6 ! Le pion {pion + 1} de {NJ[joueur]} sort de l'écurie !\n🎲 Relancez le dé (bonus 6).";
            MettreAJourScore();
            panPlateau.Invalidate();

            // Le 6 donne droit à un nouveau lancer
            dejaLance = false;
            pionsJouables.Clear();
            // Laisser le bouton actif pour relancer
        }

        private List<int> TrouverPionsJouables(int j, int de)
        {
            var liste = new List<int>();
            for (int p = 0; p < 4; p++)
            {
                if (pionArrive[j, p]) continue;
                int pos = posPions[j, p];
                if (pos == -1) { if (de == 6) liste.Add(p); }
                else if (pos >= 100) { if ((pos - 100) + de <= NB_COULOIR - 1) liste.Add(p); }
                else liste.Add(p);
            }
            return liste;
        }

        private void Plateau_MouseClick(object sender, MouseEventArgs e)
        {
            if (!dejaLance || pionsJouables.Count == 0) return;

            // Si un seul pion jouable → déplacement automatique au clic n'importe où
            if (pionsJouables.Count == 1)
            {
                JouerPion(joueurCourant, pionsJouables[0]);
                return;
            }

            // Plusieurs pions jouables : chercher lequel est cliqué
            for (int p = 0; p < 4; p++)
            {
                if (!pionsJouables.Contains(p)) continue;
                Point px = PixelPion(joueurCourant, p);
                double dist = Math.Sqrt(Math.Pow(e.X - px.X, 2) + Math.Pow(e.Y - px.Y, 2));
                if (dist <= TP + 3)
                {
                    JouerPion(joueurCourant, p);
                    return;
                }
            }
        }

        private void JouerPion(int j, int p)
        {
            int pos = posPions[j, p];

            if (pos == -1)
            {
                // Ne devrait pas arriver ici (géré dans ApresLancer) mais sécurité
                SortirPionEcurie(j, p);
                return;
            }

            if (pos >= 100)
            {
                // Avance dans le couloir
                int nouvelIdx = (pos - 100) + valeurDe;
                if (nouvelIdx >= NB_COULOIR - 1)
                {
                    pionArrive[j, p] = true;
                    posPions[j, p] = 100 + NB_COULOIR - 1;
                    lblMsg.Text = $"🏠 Pion {p + 1} de {NJ[j]} est arrivé à la maison !";
                    MettreAJourScore();
                    if (VerifierVictoire(j)) return;
                }
                else
                {
                    posPions[j, p] = 100 + nouvelIdx;
                    lblMsg.Text = $"Pion {p + 1} avance dans le couloir final.";
                }
            }
            else
            {
                // Avance sur le circuit
                int nvPos = AvancerCircuit(j, pos, valeurDe);
                string capture = "";
                if (nvPos < 100) capture = VerifierCapture(j, nvPos);
                posPions[j, p] = nvPos;
                lblMsg.Text = nvPos >= 100
                    ? $"Pion {p + 1} entre dans son couloir final !"
                    : $"Pion {p + 1} en case {nvPos}. {capture}";
            }

            MettreAJourScore();
            panPlateau.Invalidate();

            if (valeurDe == 6)
            {
                dejaLance = false;
                pionsJouables.Clear();
                lblMsg.Text += "\n🎲 Bonus 6 : relancez !";
            }
            else
            {
                PasserTour(false);
            }
        }

        private int AvancerCircuit(int j, int posActuelle, int de)
        {
            int entree = CaseEntreeFinale[j];
            for (int i = 1; i <= de; i++)
            {
                if (posActuelle == entree)
                {
                    int reste = de - i;
                    return 100 + Math.Min(reste, NB_COULOIR - 1);
                }
                posActuelle = (posActuelle + 1) % NB_CIRCUIT;
            }
            return posActuelle;
        }

        private string VerifierCapture(int attaquant, int dest)
        {
            // Cases protégées (cases de sortie)
            if (CaseSortie.Contains(dest)) return "";
            for (int j2 = 0; j2 < nbJoueurs; j2++)
            {
                if (j2 == attaquant) continue;
                for (int p2 = 0; p2 < 4; p2++)
                    if (posPions[j2, p2] == dest && !pionArrive[j2, p2])
                    {
                        posPions[j2, p2] = -1;
                        return $"💥 Pion de {NJ[j2]} renvoyé à l'écurie !";
                    }
            }
            return "";
        }

        private bool VerifierVictoire(int j)
        {
            int cnt = 0;
            for (int p = 0; p < 4; p++) if (pionArrive[j, p]) cnt++;
            if (cnt < 4) return false;

            panPlateau.Invalidate();
            var res = MessageBox.Show(
                $"🏆  VICTOIRE !\n\nLes {NJ[j]} ont gagné la partie !\n\nNouvelle partie ?",
                "Fin de partie", MessageBoxButtons.YesNo, MessageBoxIcon.Information);

            if (res == DialogResult.Yes)
            {
                InitJeu();
                joueurCourant = 0;
                dejaLance = false;
                pionsJouables.Clear();
                MettreAJourTour();
                panPlateau.Invalidate();
            }
            else this.Close();
            return true;
        }

        private void PasserTour(bool bonus6)
        {
            dejaLance = false;
            pionsJouables.Clear();
            if (!bonus6) joueurCourant = (joueurCourant + 1) % nbJoueurs;
            MettreAJourTour();
            panPlateau.Invalidate();
        }

        private void MettreAJourTour()
        {
            lblTour.Text = $"Tour : {NJ[joueurCourant]}";
            lblTour.ForeColor = CJ[joueurCourant];

            Color cb = CJ[joueurCourant];
            btnLancer.BackColor = Color.FromArgb(
                (int)(cb.R * 0.35f + 25),
                (int)(cb.G * 0.35f + 15),
                (int)(cb.B * 0.35f + 55));
            btnLancer.FlatAppearance.BorderColor = cb;

            if (!dejaLance)
                lblMsg.Text = $"C'est au tour des {NJ[joueurCourant]}.\nLancez le dé !";
        }

        private void MettreAJourScore()
        {
            for (int j = 0; j < nbJoueurs; j++)
            {
                int cnt = 0;
                for (int p = 0; p < 4; p++) if (pionArrive[j, p]) cnt++;
                var lbl = this.Controls.Find($"lblJoueur{j}", false).FirstOrDefault() as Label;
                if (lbl != null) lbl.Text = $"{NJ[j]} — {cnt}/4 pions";
            }
        }
    }
}