using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace Jeux_Multiples
{
    // ── Panel avec double-buffering natif → zéro flickering ──
    public class PanelDouble : Panel
    {
        public PanelDouble()
        {
            this.SetStyle(
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.UserPaint |
                ControlStyles.OptimizedDoubleBuffer,
                true);
            this.UpdateStyles();
        }
    }

    public partial class Dame : Form
    {
        // ─── Constantes ──────────────────────────────────────────
        private const int CASES = 8;
        private const int TAILLE = 70;
        private const int OFFSET = 45;
        private const int RAYON = 27;
        private const int COURONNE = 11;

        // ─── État du plateau ─────────────────────────────────────
        // 0=vide  1=blanc  2=noir  3=dame blanche  4=dame noire
        private int[,] plateau = new int[CASES, CASES];
        private int tourJoueur = 2;
        private Point selectionne = new Point(-1, -1);
        private List<Point> mouvementsPossibles = new List<Point>();
        private List<Point> capturesPossibles = new List<Point>();
        private int scoreNoir = 0;
        private int scoreBlanc = 0;
        private bool partieTerminee = false;

        private bool captureEnCours = false;
        private Point pionEnCapture = new Point(-1, -1);

        private List<string> historique = new List<string>();

        private Timer timerClignote;
        private bool clignoteVisible = true;

        // ─── Palette ─────────────────────────────────────────────
        // ─── Palette NEON CYBER ──────────────────────────────────
        private readonly Color CLAIR = Color.FromArgb(40, 40, 55);       // Case Claire
        private readonly Color FONCE = Color.FromArgb(15, 15, 20);       // Case Foncée
        private readonly Color BORD = Color.FromArgb(0, 238, 255);       // Bordure Cyan
        private readonly Color SURLINE = Color.FromArgb(100, 0, 255, 136); // Selection Vert
        private readonly Color POSSIBLE = Color.FromArgb(150, 0, 238, 255); // Possible Cyan
        private readonly Color CAPTURE = Color.FromArgb(180, 255, 34, 68);  // Capture Rouge
        private readonly Color BG = Color.FromArgb(10, 10, 18);          // Fond Principal
        private readonly Color PANEL_BG = Color.FromArgb(18, 18, 28);    // Panel Latéral

        // ─── Contrôles UI ────────────────────────────────────────
        private PanelDouble panelJeu;
        private Panel panelLateral;
        private Label lblTitre;
        private Label lblTour;
        private Label lblScoreNoir;
        private Label lblScoreBlanc;
        private Label lblHistoriqueTitre;
        private ListBox listHistorique;
        private Button btnNouvellePartie;
        private Button btnAccueil;
        private Panel panelIndicateur;

        public string Joueur1 = "Blancs";
        public string Joueur2 = "Noirs";
        public bool IsMultiplayer = false;

        // ════════════════════════════════════════════════════════
        // CONSTRUCTEUR
        // ════════════════════════════════════════════════════════
        public Dame(string joueur1, string joueur2, bool isMultiplayer = false)
        {
            InitializeComponent();
            if (!string.IsNullOrWhiteSpace(joueur1)) Joueur1 = joueur1;
            if (!string.IsNullOrWhiteSpace(joueur2)) Joueur2 = joueur2;
            this.IsMultiplayer = isMultiplayer;
            ConstruireUI();

            // Timer clignotement captures obligatoires
            timerClignote = new Timer { Interval = 600 };
            timerClignote.Tick += (s, e) => { clignoteVisible = !clignoteVisible; panelJeu?.Invalidate(); };
            timerClignote.Start();

            // Init plateau SANS passer par NouvellePartie (évite double-init depuis designer)
            ResetEtat();
            MettreAJourInfo();

            if (IsMultiplayer)
            {
                NetworkManager.Instance.OnPacketReceived += OnPacketReceived;
                NetworkManager.Instance.OnDisconnected += OnDisconnected;
                // Host = Blancs (Joueur 1), Client = Noirs (Joueur 2)
                if (NetworkManager.Instance.IsHost)
                {
                    Joueur1 = NetworkManager.Instance.MyPseudo;
                    Joueur2 = NetworkManager.Instance.OpponentPseudo;
                }
                else
                {
                    Joueur1 = NetworkManager.Instance.OpponentPseudo;
                    Joueur2 = NetworkManager.Instance.MyPseudo;
                }
                this.Text = $"Jeu de Dames - {Joueur1} vs {Joueur2}";
                MettreAJourInfo();
                panelJeu?.Invalidate();
            }
        }

        // Pour compatibilité ou designer
        public Dame() : this("Blancs", "Noirs", false) { }

        // ════════════════════════════════════════════════════════
        // RESET ÉTAT (sans toucher aux contrôles UI)
        // ════════════════════════════════════════════════════════
        private void ResetEtat()
        {
            plateau = new int[CASES, CASES];
            tourJoueur = 2;
            selectionne = new Point(-1, -1);
            scoreNoir = 0;
            scoreBlanc = 0;
            partieTerminee = false;
            captureEnCours = false;
            pionEnCapture = new Point(-1, -1);
            historique.Clear();
            mouvementsPossibles.Clear();
            capturesPossibles.Clear();

            for (int l = 0; l < CASES; l++)
                for (int c = 0; c < CASES; c++)
                    if ((l + c) % 2 == 1)
                    {
                        if (l < 3) plateau[l, c] = 2;      // noirs en haut
                        else if (l > 4) plateau[l, c] = 1; // blancs en bas
                    }
        }

        private void NouvellePartie()
        {
            ResetEtat();
            MettreAJourInfo();
            listHistorique?.Items.Clear();
            panelJeu?.Invalidate();
        }

        // ════════════════════════════════════════════════════════
        // CONSTRUCTION DE L'INTERFACE
        // ════════════════════════════════════════════════════════
        private void ConstruireUI()
        {
            int boardSize = CASES * TAILLE + OFFSET * 2;
            int panelLatW = 200;
            int formWidth = boardSize + panelLatW + 30;
            int formHeight = boardSize + 110;

            this.Text = $"Jeu de Dames - {Joueur1} vs {Joueur2}";
            this.ClientSize = new Size(formWidth, formHeight);
            this.MinimumSize = new Size(formWidth + 16, formHeight + 39);
            this.MaximumSize = this.MinimumSize;
            this.BackColor = BG;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Font = new Font("Segoe UI", 10f);
            this.SetStyle(ControlStyles.OptimizedDoubleBuffer |
                          ControlStyles.AllPaintingInWmPaint, true);

            // Titre
            lblTitre = new Label
            {
                Text = "♛  JEU DE DAMES  ♛",
                ForeColor = Color.FromArgb(220, 175, 80),
                Font = new Font("Georgia", 17f, FontStyle.Bold),
                AutoSize = false,
                TextAlign = ContentAlignment.MiddleCenter,
                BackColor = Color.Transparent,
                Bounds = new Rectangle(0, 6, formWidth, 36)
            };
            this.Controls.Add(lblTitre);

            // Plateau (PanelDouble = anti-flickering)
            panelJeu = new PanelDouble
            {
                Bounds = new Rectangle(10, 48, boardSize, boardSize),
                BackColor = Color.Transparent
            };
            panelJeu.Paint += PanelJeu_Paint;
            panelJeu.MouseClick += PanelJeu_MouseClick;
            this.Controls.Add(panelJeu);

            // Panneau latéral
            int latX = boardSize + 18;
            panelLateral = new Panel
            {
                Bounds = new Rectangle(latX, 48, panelLatW, boardSize),
                BackColor = PANEL_BG
            };
            panelLateral.Paint += (s, e) =>
            {
                using (var pen = new Pen(Color.FromArgb(100, 180, 130, 50), 1))
                    e.Graphics.DrawRectangle(pen, 0, 0, panelLateral.Width - 1, panelLateral.Height - 1);
            };
            this.Controls.Add(panelLateral);

            // Barre indicatrice de tour
            panelIndicateur = new Panel
            {
                Bounds = new Rectangle(latX, 48, panelLatW, 8),
                BackColor = Color.Gray
            };
            this.Controls.Add(panelIndicateur);

            // Label tour
            lblTour = new Label
            {
                Text = "",
                ForeColor = Color.FromArgb(230, 215, 170),
                Font = new Font("Segoe UI", 10f, FontStyle.Bold),
                AutoSize = false,
                TextAlign = ContentAlignment.MiddleCenter,
                BackColor = Color.Transparent,
                Bounds = new Rectangle(latX, 60, panelLatW, 46)
            };
            this.Controls.Add(lblTour);

            // Scores
            int scoreY = 112;
            CreerCaseScore($"⬜ {Joueur1}", latX, scoreY, out lblScoreBlanc, Color.FromArgb(240, 235, 215));
            CreerCaseScore($"⬛ {Joueur2}", latX, scoreY + 60, out lblScoreNoir, Color.FromArgb(160, 140, 100));

            // Historique titre
            lblHistoriqueTitre = new Label
            {
                Text = "─  Historique  ─",
                ForeColor = Color.FromArgb(150, 120, 60),
                Font = new Font("Segoe UI", 9f, FontStyle.Italic),
                AutoSize = false,
                TextAlign = ContentAlignment.MiddleCenter,
                BackColor = Color.Transparent,
                Bounds = new Rectangle(latX, scoreY + 132, panelLatW, 22)
            };
            this.Controls.Add(lblHistoriqueTitre);

            // Historique liste
            listHistorique = new ListBox
            {
                Bounds = new Rectangle(latX + 6, scoreY + 156,
                                          panelLatW - 12, boardSize - scoreY - 170),
                BackColor = Color.FromArgb(28, 18, 6),
                ForeColor = Color.FromArgb(180, 165, 120),
                Font = new Font("Consolas", 8f),
                BorderStyle = BorderStyle.None,
                ScrollAlwaysVisible = false,
                SelectionMode = SelectionMode.None
            };
            this.Controls.Add(listHistorique);

            // Boutons
            int btnY = boardSize + 58;

            btnAccueil = CréerBouton("🏠  Accueil", 10, btnY,
                Color.FromArgb(45, 30, 10), Color.FromArgb(220, 175, 80));
            btnAccueil.Click += BtnAccueil_Click;
            this.Controls.Add(btnAccueil);

            btnNouvellePartie = CréerBouton("↺  Nouvelle Partie", boardSize - 168, btnY,
                Color.FromArgb(60, 38, 10), Color.FromArgb(220, 175, 80));
            btnNouvellePartie.Click += (s, e) => NouvellePartie();
            this.Controls.Add(btnNouvellePartie);
        }

        private void CreerCaseScore(string titre, int x, int y, out Label lblValeur, Color couleurTitre)
        {
            var pnl = new Panel { Bounds = new Rectangle(x + 8, y, 184, 52), BackColor = Color.FromArgb(42, 27, 8) };
            pnl.Paint += (s, e) =>
            {
                using (var pen = new Pen(Color.FromArgb(80, 180, 130, 50), 1))
                    e.Graphics.DrawRectangle(pen, 0, 0, pnl.Width - 1, pnl.Height - 1);
            };
            this.Controls.Add(pnl);

            this.Controls.Add(new Label
            {
                Text = titre,
                ForeColor = couleurTitre,
                Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                AutoSize = false,
                TextAlign = ContentAlignment.MiddleCenter,
                BackColor = Color.Transparent,
                Bounds = new Rectangle(x + 8, y + 2, 184, 22)
            });

            lblValeur = new Label
            {
                Text = "0 capture",
                ForeColor = Color.FromArgb(255, 210, 100),
                Font = new Font("Georgia", 13f, FontStyle.Bold),
                AutoSize = false,
                TextAlign = ContentAlignment.MiddleCenter,
                BackColor = Color.Transparent,
                Bounds = new Rectangle(x + 8, y + 24, 184, 26)
            };
            this.Controls.Add(lblValeur);
        }

        private Button CréerBouton(string texte, int x, int y, Color bg, Color fg)
        {
            var btn = new Button
            {
                Text = texte.ToUpper(),
                Bounds = new Rectangle(x, y, 170, 36),
                BackColor = Color.FromArgb(30, 30, 45),
                ForeColor = fg,
                Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btn.FlatAppearance.BorderColor = fg;
            btn.FlatAppearance.BorderSize = 1;
            btn.FlatAppearance.MouseOverBackColor = Color.FromArgb(40, fg.R, fg.G, fg.B);
            btn.FlatAppearance.MouseDownBackColor = Color.FromArgb(60, fg.R, fg.G, fg.B);
            return btn;
        }

        private void OnDisconnected()
        {
             this.Invoke(new Action(() => 
             {
                 MessageBox.Show("Adversaire déconnecté.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                 this.Close();
             }));
        }

        private void OnPacketReceived(Packet p)
        {
            if (p.Type == "MOVE")
            {
                // Format: "nligne;ncol" (la sélection initiale est gérée par l'état du plateau côté client, 
                // mais pour simplifier ici on envoie juste le COUP final: FROM_L,FROM_C,TO_L,TO_C)
                // Wait, Dame logic is complex (multi-step capture). 
                // Let's assume we sync the CLICK events or the MOVE events.
                // Simpler: Send "L1,C1,L2,C2" -> Move [L1,C1] to [L2,C2].
                
                // Parsing:
                string[] parts = p.Content.Split(',');
                if (parts.Length == 4)
                {
                    int fromL = int.Parse(parts[0]);
                    int fromC = int.Parse(parts[1]);
                    int toL = int.Parse(parts[2]);
                    int toC = int.Parse(parts[3]);

                    this.Invoke(new Action(() => 
                    {
                        // Simulate local state
                        selectionne = new Point(fromL, fromC);
                        CalculerMouvements(fromL, fromC); // Populate lists
                        EffectuerMouvement(toL, toC, false); // False = remote
                    }));
                }
            }
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (IsMultiplayer)
            {
                NetworkManager.Instance.OnPacketReceived -= OnPacketReceived;
                NetworkManager.Instance.OnDisconnected -= OnDisconnected;
            }
            timerClignote?.Stop();
            base.OnFormClosing(e);
        }

        // ════════════════════════════════════════════════════════
        // BOUTON ACCUEIL
        // ════════════════════════════════════════════════════════
        private void BtnAccueil_Click(object sender, EventArgs e)
        {
            timerClignote?.Stop();
            try
            {
                var accueil = new Accueil();
                accueil.Show();
                this.Close();
            }
            catch
            {
                MessageBox.Show("Impossible d'ouvrir le formulaire Accueil.",
                    "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        // ════════════════════════════════════════════════════════
        // DESSIN
        // ════════════════════════════════════════════════════════
        private void PanelJeu_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;

            // Fond sombre
            using (var bgBrush = new SolidBrush(Color.FromArgb(5, 5, 10)))
                g.FillRectangle(bgBrush, 0, 0, panelJeu.Width, panelJeu.Height);

            // Double cadre
            using (var pen = new Pen(Color.FromArgb(80, 50, 15), 10))
                g.DrawRectangle(pen, OFFSET - 4, OFFSET - 4, CASES * TAILLE + 7, CASES * TAILLE + 7);
            using (var pen = new Pen(Color.FromArgb(160, 120, 40), 2))
                g.DrawRectangle(pen, OFFSET - 8, OFFSET - 8, CASES * TAILLE + 15, CASES * TAILLE + 15);

            // Coordonnées
            using (var fnt = new Font("Georgia", 11f, FontStyle.Italic))
            using (var br = new SolidBrush(Color.FromArgb(180, 150, 80)))
                for (int i = 0; i < CASES; i++)
                {
                    string lettre = ((char)('A' + i)).ToString();
                    SizeF sz = g.MeasureString(lettre, fnt);
                    g.DrawString(lettre, fnt, br, OFFSET + i * TAILLE + (TAILLE - sz.Width) / 2, 14);
                    g.DrawString((8 - i).ToString(), fnt, br, 12, OFFSET + i * TAILLE + (TAILLE - sz.Height) / 2);
                }

            bool captureObligatoire = ACapturePossible(tourJoueur);

            // Cases
            for (int l = 0; l < CASES; l++)
            {
                for (int c = 0; c < CASES; c++)
                {
                    Rectangle rect = CaseRect(l, c);
                    bool caseFoncee = (l + c) % 2 == 1;

                    using (var br = new SolidBrush(caseFoncee ? FONCE : CLAIR))
                        g.FillRectangle(br, rect);

                    // Légère texture diagonale sur les cases foncées
                    if (caseFoncee)
                        using (var pen = new Pen(Color.FromArgb(12, 255, 200, 100), 1))
                        {
                            g.DrawLine(pen, rect.X, rect.Y, rect.Right, rect.Bottom);
                            g.DrawLine(pen, rect.Right, rect.Y, rect.X, rect.Bottom);
                        }

                    // Case sélectionnée
                    if (selectionne.X == l && selectionne.Y == c)
                        using (var br = new SolidBrush(SURLINE))
                            g.FillRectangle(br, rect);

                    // Mouvements possibles
                    if (EstDansListe(mouvementsPossibles, l, c))
                    {
                        using (var br = new SolidBrush(POSSIBLE))
                            g.FillRectangle(br, rect);
                        int marge = 24;
                        using (var br = new SolidBrush(Color.FromArgb(140, 60, 190, 60)))
                            g.FillEllipse(br, rect.X + marge, rect.Y + marge,
                                TAILLE - marge * 2, TAILLE - marge * 2);
                    }

                    // Captures possibles (clignotent si obligatoires)
                    if (EstDansListe(capturesPossibles, l, c) &&
                        (!captureObligatoire || clignoteVisible))
                    {
                        using (var br = new SolidBrush(CAPTURE))
                            g.FillRectangle(br, rect);
                        int marge = 12;
                        using (var pen = new Pen(Color.FromArgb(220, 230, 60, 60), 3))
                            g.DrawEllipse(pen, rect.X + marge, rect.Y + marge,
                                TAILLE - marge * 2, TAILLE - marge * 2);
                    }
                }
            }

            // Grille
            using (var pen = new Pen(Color.FromArgb(35, BORD), 1))
                for (int i = 0; i <= CASES; i++)
                {
                    g.DrawLine(pen, OFFSET + i * TAILLE, OFFSET, OFFSET + i * TAILLE, OFFSET + CASES * TAILLE);
                    g.DrawLine(pen, OFFSET, OFFSET + i * TAILLE, OFFSET + CASES * TAILLE, OFFSET + i * TAILLE);
                }

            // Pions
            for (int l = 0; l < CASES; l++)
                for (int c = 0; c < CASES; c++)
                    if (plateau[l, c] != 0)
                        DessinerPion(g, l, c, plateau[l, c]);
        }

        private void DessinerPion(Graphics g, int ligne, int col, int type)
        {
            Rectangle rect = CaseRect(ligne, col);
            int cx = rect.X + TAILLE / 2;
            int cy = rect.Y + TAILLE / 2;

            bool estBlanc = (type == 1 || type == 3);
            bool estDame = (type == 3 || type == 4);

            Color couleurBase = estBlanc ? Color.FromArgb(0, 238, 255) : Color.FromArgb(255, 34, 68); // Cyan vs Rouge
            Color couleurBord = estBlanc ? Color.White : Color.FromArgb(50, 0, 0);

            // Ombre
            using (var br = new SolidBrush(Color.FromArgb(80, 0, 0, 0)))
                g.FillEllipse(br, cx - RAYON + 5, cy - RAYON + 7, RAYON * 2, RAYON * 2);

            // Corps dégradé
            using (var path = new GraphicsPath())
            {
                path.AddEllipse(cx - RAYON, cy - RAYON, RAYON * 2, RAYON * 2);
                using (var brush = new PathGradientBrush(path))
                {
                    brush.CenterPoint = new PointF(cx - RAYON / 3, cy - RAYON / 3);
                    brush.CenterColor = estBlanc ? Color.White : Color.FromArgb(255, 100, 100);
                    brush.SurroundColors = new[] { couleurBase };
                    g.FillEllipse(new SolidBrush(couleurBase), cx - RAYON, cy - RAYON, RAYON * 2, RAYON * 2);
                    g.FillEllipse(brush, cx - RAYON, cy - RAYON, RAYON * 2, RAYON * 2);
                }
            }

            // Anneau extérieur
            using (var pen = new Pen(couleurBord, 2.5f))
                g.DrawEllipse(pen, cx - RAYON, cy - RAYON, RAYON * 2, RAYON * 2);

            // Anneau intérieur décoratif
            int ri = RAYON - 6;
            using (var pen = new Pen(Color.FromArgb(60, estBlanc ? 160 : 255, estBlanc ? 140 : 200, estBlanc ? 80 : 80), 1f))
                g.DrawEllipse(pen, cx - ri, cy - ri, ri * 2, ri * 2);

            // Reflet
            using (var br = new SolidBrush(Color.FromArgb(
                estBlanc ? 160 : 70, estBlanc ? 255 : 200,
                estBlanc ? 255 : 180, estBlanc ? 255 : 100)))
                g.FillEllipse(br, cx - RAYON / 2 - 2, cy - RAYON + 5, RAYON * 3 / 4, RAYON * 2 / 5);

            // Couronne dame
            if (estDame)
            {
                Color cC = estBlanc ? Color.FromArgb(220, 170, 30) : Color.FromArgb(210, 230, 170);
                using (var pen = new Pen(cC, 2f))
                {
                    g.DrawEllipse(pen, cx - COURONNE, cy - COURONNE, COURONNE * 2, COURONNE * 2);
                    for (int i = 0; i < 5; i++)
                    {
                        double angle = -Math.PI / 2 + i * 2 * Math.PI / 5;
                        float px = cx + (float)(COURONNE * Math.Cos(angle));
                        float py = cy + (float)(COURONNE * Math.Sin(angle));
                        g.DrawLine(pen, cx, cy, px, py);
                        using (var brC = new SolidBrush(cC))
                            g.FillEllipse(brC, px - 3.5f, py - 3.5f, 7, 7);
                    }
                }
            }
        }

        // ════════════════════════════════════════════════════════
        // INTERACTION SOURIS
        // ════════════════════════════════════════════════════════
        private void PanelJeu_MouseClick(object sender, MouseEventArgs e)
        {
            if (partieTerminee) return;

            // MULTI CHECK
            if (IsMultiplayer)
            {
                // Blancs (1) = Host
                // Noirs (2) = Client
                bool isMyTurn = NetworkManager.Instance.IsHost ? (tourJoueur == 1) : (tourJoueur == 2);
                if (!isMyTurn) return;
            }

            int col = (e.X - OFFSET) / TAILLE;
            int ligne = (e.Y - OFFSET) / TAILLE;
            if (col < 0 || col >= CASES || ligne < 0 || ligne >= CASES) return;

            // Pendant une capture multiple : seules les destinations de capture sont valides
            if (captureEnCours)
            {
                if (EstDansListe(capturesPossibles, ligne, col))
                    EffectuerMouvement(ligne, col, true);
                panelJeu.Invalidate();
                return;
            }

            bool clicSurPionJoueur =
                (tourJoueur == 1 && (plateau[ligne, col] == 1 || plateau[ligne, col] == 3)) ||
                (tourJoueur == 2 && (plateau[ligne, col] == 2 || plateau[ligne, col] == 4));

            if (clicSurPionJoueur)
            {
                selectionne = new Point(ligne, col);
                CalculerMouvements(ligne, col);

                // Prise obligatoire : bloquer les mouvements simples si ce pion ne peut pas capturer
                if (ACapturePossible(tourJoueur) && capturesPossibles.Count == 0)
                    mouvementsPossibles.Clear();
            }
            else if (selectionne.X != -1)
            {
                if (EstDansListe(capturesPossibles, ligne, col) ||
                    EstDansListe(mouvementsPossibles, ligne, col))
                    EffectuerMouvement(ligne, col, true);
                else
                {
                    selectionne = new Point(-1, -1);
                    mouvementsPossibles.Clear();
                    capturesPossibles.Clear();
                }
            }

            panelJeu.Invalidate();
        }

        // ════════════════════════════════════════════════════════
        // CALCUL DES MOUVEMENTS
        // ════════════════════════════════════════════════════════
        private void CalculerMouvements(int l, int c)
        {
            mouvementsPossibles.Clear();
            capturesPossibles.Clear();
            int type = plateau[l, c];
            bool estDame = (type == 3 || type == 4);
            if (estDame) CalculerMouvementsDame(l, c, type);
            else CalculerMouvementsPion(l, c, type);
        }

        private void CalculerMouvementsPion(int l, int c, int type)
        {
            int dirLigne = (type == 2) ? 1 : -1;
            int adverse = (type == 2) ? 1 : 2;
            int adDame = (type == 2) ? 3 : 4;

            foreach (int dc in new[] { -1, 1 })
            {
                int nl = l + dirLigne, nc = c + dc;
                if (EstDansGrille(nl, nc) && plateau[nl, nc] == 0)
                    mouvementsPossibles.Add(new Point(nl, nc));
            }

            foreach (int dc in new[] { -1, 1 })
                foreach (int dir in new[] { 1, -1 })
                {
                    int nl = l + dir, nc = c + dc;
                    int nl2 = l + dir * 2, nc2 = c + dc * 2;
                    if (EstDansGrille(nl2, nc2) &&
                        (plateau[nl, nc] == adverse || plateau[nl, nc] == adDame) &&
                        plateau[nl2, nc2] == 0)
                        capturesPossibles.Add(new Point(nl2, nc2));
                }
        }

        private void CalculerMouvementsDame(int l, int c, int type)
        {
            int adverse = (type == 3) ? 2 : 1;
            int adDame = (type == 3) ? 4 : 3;

            foreach (int dl in new[] { -1, 1 })
                foreach (int dc in new[] { -1, 1 })
                {
                    bool aCapture = false;
                    for (int pas = 1; pas < CASES; pas++)
                    {
                        int nl = l + dl * pas, nc = c + dc * pas;
                        if (!EstDansGrille(nl, nc)) break;
                        if (plateau[nl, nc] == 0)
                        {
                            if (aCapture) capturesPossibles.Add(new Point(nl, nc));
                            else mouvementsPossibles.Add(new Point(nl, nc));
                        }
                        else if ((plateau[nl, nc] == adverse || plateau[nl, nc] == adDame) && !aCapture)
                            aCapture = true;
                        else
                            break;
                    }
                }
        }

        private bool ACapturePossible(int joueur)
        {
            for (int l = 0; l < CASES; l++)
                for (int c = 0; c < CASES; c++)
                {
                    int t = plateau[l, c];
                    if ((joueur == 1 && (t == 1 || t == 3)) || (joueur == 2 && (t == 2 || t == 4)))
                    {
                        var tmp = new List<Point>();
                        if (t == 3 || t == 4) CaptureDameLocale(l, c, t, tmp);
                        else CapturePionLocale(l, c, t, tmp);
                        if (tmp.Count > 0) return true;
                    }
                }
            return false;
        }

        private void CapturePionLocale(int l, int c, int type, List<Point> caps)
        {
            int adverse = (type == 2) ? 1 : 2;
            int adDame = (type == 2) ? 3 : 4;
            foreach (int dc in new[] { -1, 1 })
                foreach (int dir in new[] { 1, -1 })
                {
                    int nl = l + dir, nc = c + dc;
                    int nl2 = l + dir * 2, nc2 = c + dc * 2;
                    if (EstDansGrille(nl2, nc2) &&
                        (plateau[nl, nc] == adverse || plateau[nl, nc] == adDame) &&
                        plateau[nl2, nc2] == 0)
                        caps.Add(new Point(nl2, nc2));
                }
        }

        private void CaptureDameLocale(int l, int c, int type, List<Point> caps)
        {
            int adverse = (type == 3) ? 2 : 1;
            int adDame = (type == 3) ? 4 : 3;
            foreach (int dl in new[] { -1, 1 })
                foreach (int dc in new[] { -1, 1 })
                {
                    bool aCapture = false;
                    for (int pas = 1; pas < CASES; pas++)
                    {
                        int nl = l + dl * pas, nc = c + dc * pas;
                        if (!EstDansGrille(nl, nc)) break;
                        if (plateau[nl, nc] == 0)
                        { if (aCapture) caps.Add(new Point(nl, nc)); }
                        else if ((plateau[nl, nc] == adverse || plateau[nl, nc] == adDame) && !aCapture)
                            aCapture = true;
                        else break;
                    }
                }
        }

        // ════════════════════════════════════════════════════════
        // EFFECTUER UN MOUVEMENT
        // ════════════════════════════════════════════════════════
        private void EffectuerMouvement(int nligne, int ncol, bool isLocal = true)
        {
            int l = captureEnCours ? pionEnCapture.X : selectionne.X;
            int c = captureEnCours ? pionEnCapture.Y : selectionne.Y;

            // NET: Envoyer le coup
            if (IsMultiplayer && isLocal)
            {
                NetworkManager.Instance.SendPacket(new Packet("MOVE", NetworkManager.Instance.MyPseudo, $"{l},{c},{nligne},{ncol}"));
            }

            bool estCapture = EstDansListe(capturesPossibles, nligne, ncol);
            int type = plateau[l, c];

            // Historique
            string colLettre = ((char)('A' + c)).ToString();
            string nColLettre = ((char)('A' + ncol)).ToString();
            string coup = $"{(tourJoueur == 2 ? "⬛" : "⬜")} {colLettre}{8 - l} → {nColLettre}{8 - nligne}{(estCapture ? " ✂" : "")}";
            historique.Insert(0, coup);
            if (historique.Count > 40) historique.RemoveAt(historique.Count - 1);
            listHistorique.BeginUpdate();
            listHistorique.Items.Clear();
            foreach (var h in historique) listHistorique.Items.Add(h);
            listHistorique.EndUpdate();

            // Déplacer le pion
            plateau[nligne, ncol] = type;
            plateau[l, c] = 0;

            // Supprimer pion capturé
            if (estCapture)
            {
                int ml = (l + nligne) / 2;
                int mc = (c + ncol) / 2;

                if (type == 3 || type == 4)
                {
                    int dl = Math.Sign(nligne - l), dc2 = Math.Sign(ncol - c);
                    for (int pas = 1; pas < CASES; pas++)
                    {
                        int tl = l + dl * pas, tc = c + dc2 * pas;
                        if (tl == nligne && tc == ncol) break;
                        if (plateau[tl, tc] != 0) { ml = tl; mc = tc; break; }
                    }
                }

                if (tourJoueur == 2) scoreBlanc++;
                else scoreNoir++;
                plateau[ml, mc] = 0;
            }

            // Promotion
            if (type == 2 && nligne == CASES - 1) plateau[nligne, ncol] = 4;
            if (type == 1 && nligne == 0) plateau[nligne, ncol] = 3;

            // Capture multiple ?
            if (estCapture)
            {
                pionEnCapture = new Point(nligne, ncol);
                captureEnCours = true;
                selectionne = new Point(nligne, ncol);
                CalculerMouvements(nligne, ncol);

                if (capturesPossibles.Count > 0)
                {
                    mouvementsPossibles.Clear();
                    panelJeu.Invalidate();
                    MettreAJourInfo();
                    return;
                }
            }

            // Fin du tour
            captureEnCours = false;
            pionEnCapture = new Point(-1, -1);
            selectionne = new Point(-1, -1);
            mouvementsPossibles.Clear();
            capturesPossibles.Clear();
            tourJoueur = (tourJoueur == 1) ? 2 : 1;

            panelJeu.Invalidate();
            MettreAJourInfo();

            if (VerifierFinDePartie())
            {
                partieTerminee = true;
                string gagnant = (tourJoueur == 1) ? Joueur2 : Joueur1;
                MessageBox.Show(
                    $"⬛⬜  {gagnant} a gagné !  ⬜⬛\n\nScore final :\n  {Joueur1} : {scoreBlanc} captures\n  {Joueur2} : {scoreNoir} captures",
                    "Fin de partie", MessageBoxButtons.OK, MessageBoxIcon.Information);
                MettreAJourInfo();
            }
        }

        // ════════════════════════════════════════════════════════
        // FIN DE PARTIE
        // ════════════════════════════════════════════════════════
        private bool VerifierFinDePartie()
        {
            for (int l = 0; l < CASES; l++)
                for (int c = 0; c < CASES; c++)
                {
                    int t = plateau[l, c];
                    if ((tourJoueur == 1 && (t == 1 || t == 3)) ||
                        (tourJoueur == 2 && (t == 2 || t == 4)))
                    {
                        CalculerMouvements(l, c);
                        if (mouvementsPossibles.Count > 0 || capturesPossibles.Count > 0)
                        {
                            mouvementsPossibles.Clear(); capturesPossibles.Clear();
                            return false;
                        }
                    }
                }
            return true;
        }

        // ════════════════════════════════════════════════════════
        // UTILITAIRES
        // ════════════════════════════════════════════════════════
        private Rectangle CaseRect(int l, int c) =>
            new Rectangle(OFFSET + c * TAILLE, OFFSET + l * TAILLE, TAILLE, TAILLE);

        private bool EstDansGrille(int l, int c) =>
            l >= 0 && l < CASES && c >= 0 && c < CASES;

        private bool EstDansListe(List<Point> liste, int l, int c)
        {
            foreach (var p in liste)
                if (p.X == l && p.Y == c) return true;
            return false;
        }

        private void MettreAJourInfo()
        {
            if (panelIndicateur == null || lblTour == null) return;

            panelIndicateur.BackColor = partieTerminee
                ? Color.FromArgb(80, 60, 20)
                : (tourJoueur == 2 ? Color.FromArgb(40, 30, 18) : Color.FromArgb(200, 190, 160));

            if (partieTerminee)
            {
                lblTour.Text = "🏁  Partie terminée !";
                lblTour.ForeColor = Color.FromArgb(220, 175, 80);
            }
            else
            {
                string joueur = (tourJoueur == 2) ? $"⬛  Tour de {Joueur2.ToUpper()}" : $"⬜  Tour de {Joueur1.ToUpper()}";
                if (captureEnCours) joueur += "\n⚡ Capture multiple !";
                else if (ACapturePossible(tourJoueur)) joueur += "\n⚠ Prise obligatoire !";
                lblTour.Text = joueur;
                lblTour.ForeColor = (tourJoueur == 2)
                    ? Color.FromArgb(200, 180, 130)
                    : Color.FromArgb(240, 235, 210);
            }

            if (lblScoreBlanc != null)
                lblScoreBlanc.Text = $"{scoreBlanc} {(scoreBlanc == 1 ? "capture" : "captures")}";
            if (lblScoreNoir != null)
                lblScoreNoir.Text = $"{scoreNoir} {(scoreNoir == 1 ? "capture" : "captures")}";
        }

        // ════════════════════════════════════════════════════════
        // NETTOYAGE
        // ════════════════════════════════════════════════════════
        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            timerClignote?.Stop();
            timerClignote?.Dispose();
            base.OnFormClosed(e);
        }

        // IMPORTANT : NE PAS mettre NouvellePartie() ici !
        // Le designer peut déclencher Dame_Load plusieurs fois,
        // ce qui causait le rechargement du plateau au clic.
        private void Dame_Load(object sender, EventArgs e) { }
    }
}