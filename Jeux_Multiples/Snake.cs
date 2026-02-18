using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Windows.Forms;

namespace Jeux_Multiples
{
    public partial class Snake : Form
    { 
        #region Variables et constantes
        // ══════════════════════════════════════════
        //  VARIABLES DU JEU
        // ══════════════════════════════════════════
        private List<Point> serpent = new List<Point>();
        private Point nourriture;
        private int direction;          // 0=haut, 1=droite, 2=bas, 3=gauche
        private int prochaineDirection; // Direction buffered pour éviter les inversions
        private int tailleCase = 20;
        private int score = 0;
        private int highScore = 0;
        private int niveauVitesse = 1;
        private bool jeuEnCours = false;
        private bool jeuDemarre = false;
        private Timer timer;
        private int ticksEcoules = 0;   // Pour le chrono

        // ══════════════════════════════════════════
        //  COULEURS NÉON
        // ══════════════════════════════════════════
        private readonly Color couleurFond = Color.FromArgb(5, 10, 14);
        private readonly Color couleurGrille = Color.FromArgb(12, 30, 20);
        private readonly Color couleurNeonVert = Color.FromArgb(0, 255, 136);
        private readonly Color couleurVertFonce = Color.FromArgb(0, 180, 90);
        private readonly Color couleurNeonCyan = Color.FromArgb(0, 238, 255);
        private readonly Color couleurNeonRouge = Color.FromArgb(255, 34, 68);
        private readonly Color couleurNeonJaune = Color.FromArgb(255, 238, 0);
        private readonly Color couleurBordure = Color.FromArgb(0, 200, 100);

        // ══════════════════════════════════════════
        //  DIMENSIONS
        // ══════════════════════════════════════════
        private const int PANNEAU_HAUT = 60;   // Barre de score en haut
        private const int PANNEAU_DROITE = 170;  // Panneau latéral droit
        private const int MARGE = 10;
        private int zonjeJeuLargeur;
        private int zoneJeuHauteur;

        // Animation nourriture
        private float angleNourriture = 0f;
        private Timer timerAnimation;

        // ══════════════════════════════════════════
        //  BOUTONS DE NAVIGATION
        // ══════════════════════════════════════════
        private Button btnAccueil;
        private Button btnPuissance4;
        private Button btnMortPion;

        public string Joueur = "Joueur";

        public Snake(string joueur)
        {
            InitializeComponent();
            if (!string.IsNullOrWhiteSpace(joueur)) Joueur = joueur;
            InitialiserJeu();
        }

        public Snake() : this("Joueur") { }

        // ══════════════════════════════════════════
        //  INITIALISATION
        // ══════════════════════════════════════════
        private void InitialiserJeu()
        {
            this.Text = $"SNAKE — ARCADE ({Joueur})";
            this.Width = 700;
            this.Height = 700;
            this.MinimumSize = new Size(700, 700);
            this.MaximumSize = new Size(700, 700);
            this.DoubleBuffered = true;
            this.KeyPreview = true;
            this.BackColor = couleurFond;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;

            // Calcul des zones
            zonjeJeuLargeur = this.ClientSize.Width - PANNEAU_DROITE - MARGE * 3;
            zoneJeuHauteur = this.ClientSize.Height - PANNEAU_HAUT - MARGE * 2;

            // Timer principal
            timer = new Timer();
            timer.Interval = 130;
            timer.Tick += GameLoop;

            // Timer animation (nourriture pulsante)
            timerAnimation = new Timer();
            timerAnimation.Interval = 30;
            timerAnimation.Tick += (s, e) => { angleNourriture += 0.08f; this.Invalidate(); };
            timerAnimation.Start();

            // Événements
            this.Paint += Snake_Paint;
            this.KeyDown += Snake_KeyDown;

            // Créer les boutons de navigation
            CreerBoutonsNavigation();

            DemarrerNouveauJeu();
        }

        private void CreerBoutonsNavigation()
        {
            // Position X de départ : dans le panneau droit
            int bx = MARGE + zonjeJeuLargeur + MARGE * 2;
            int bLargeur = PANNEAU_DROITE - MARGE - 20;
            int bHauteur = 34;

            // Les boutons sont placés en bas du panneau latéral
            int byBase = PANNEAU_HAUT + MARGE + zoneJeuHauteur - (bHauteur * 3 + 30);

            // ── Bouton Accueil ──
            btnAccueil = CreerBoutonNeon("⌂  ACCUEIL",
                Color.FromArgb(0, 238, 255),   // cyan
                bx, byBase, bLargeur, bHauteur);
            btnAccueil.Click += (s, e) =>
            {
                timer.Stop();
                timerAnimation.Stop();
                Accueil accueil = new Accueil();
                accueil.Show();
                this.Close();
            };
            this.Controls.Add(btnAccueil);

            // ── Bouton Puissance 4 ──
            btnPuissance4 = CreerBoutonNeon("◉  PUISSANCE 4",
                Color.FromArgb(255, 180, 0),   // jaune/orange
                bx, byBase + bHauteur + 10, bLargeur, bHauteur);
            btnPuissance4.Click += (s, e) =>
            {
                timer.Stop();
                timerAnimation.Stop();
                Puissance_4 p4 = new Puissance_4 (Joueur, "Joueur 2");
                p4.Show();
                this.Close();
            };
            this.Controls.Add(btnPuissance4);

            // ── Bouton Mort Pion ──
            btnMortPion = CreerBoutonNeon("♟  MORT PION",
                Color.FromArgb(255, 34, 68),   // rouge néon
                bx, byBase + (bHauteur + 10) * 2, bLargeur, bHauteur);
            btnMortPion.Click += (s, e) =>
            {
                timer.Stop();
                timerAnimation.Stop();
                mort_Pion mp = new mort_Pion(Joueur, "Joueur 2");
                mp.Show();
                this.Close();
            };
            this.Controls.Add(btnMortPion);
        }

        private Button CreerBoutonNeon(string texte, Color couleurNeon, int x, int y, int w, int h)
        {
            Button btn = new Button
            {
                Text = texte,
                Location = new Point(x, y),
                Size = new Size(w, h),
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Consolas", 9, FontStyle.Bold),
                ForeColor = couleurNeon,
                BackColor = Color.FromArgb(15, couleurNeon.R, couleurNeon.G, couleurNeon.B),
                Cursor = Cursors.Hand,
            };
            btn.FlatAppearance.BorderColor = couleurNeon;
            btn.FlatAppearance.BorderSize = 1;
            btn.FlatAppearance.MouseOverBackColor = Color.FromArgb(40, couleurNeon.R, couleurNeon.G, couleurNeon.B);
            btn.FlatAppearance.MouseDownBackColor = Color.FromArgb(70, couleurNeon.R, couleurNeon.G, couleurNeon.B);

            // Garder le focus sur le formulaire pour les touches clavier
            btn.TabStop = false;
            btn.Click += (s, e) => this.Focus();

            return btn;
        }

        private void DemarrerNouveauJeu()
        {
            // Réinitialiser le serpent au centre de la zone de jeu
            serpent.Clear();
            int centreX = (zonjeJeuLargeur / tailleCase) / 2;
            int centreY = (zoneJeuHauteur / tailleCase) / 2;
            serpent.Add(new Point(centreX, centreY));
            serpent.Add(new Point(centreX, centreY + 1));
            serpent.Add(new Point(centreX, centreY + 2));

            direction = 0;
            prochaineDirection = 0;
            score = 0;
            niveauVitesse = 1;
            ticksEcoules = 0;
            jeuEnCours = true;
            jeuDemarre = false;
            timer.Interval = 130;

            GenererNourriture();
            this.Invalidate();
        }

        private void GenererNourriture()
        {
            Random rand = new Random();
            int maxX = (zonjeJeuLargeur / tailleCase) - 1;
            int maxY = (zoneJeuHauteur / tailleCase) - 1;

            do
            {
                nourriture = new Point(rand.Next(1, maxX), rand.Next(1, maxY));
            } while (serpent.Contains(nourriture));
        }
#endregion

        #region BOUCLE DE JEU
        // ══════════════════════════════════════════
        //  BOUCLE DE JEU
        // ══════════════════════════════════════════
        private void GameLoop(object sender, EventArgs e)
        {
            if (!jeuEnCours) return;
            ticksEcoules++;

            // Appliquer la direction bufferisée
            direction = prochaineDirection;

            Point tete = serpent[0];
            Point nouvelleTete = new Point(tete.X, tete.Y);

            switch (direction)
            {
                case 0: nouvelleTete.Y--; break;
                case 1: nouvelleTete.X++; break;
                case 2: nouvelleTete.Y++; break;
                case 3: nouvelleTete.X--; break;
            }

            int maxX = zonjeJeuLargeur / tailleCase;
            int maxY = zoneJeuHauteur / tailleCase;

            // Collision murs
            if (nouvelleTete.X < 0 || nouvelleTete.X >= maxX ||
                nouvelleTete.Y < 0 || nouvelleTete.Y >= maxY)
            {
                GameOver(); return;
            }

            // Collision corps
            if (serpent.Contains(nouvelleTete))
            {
                GameOver(); return;
            }

            serpent.Insert(0, nouvelleTete);

            if (nouvelleTete == nourriture)
            {
                score += 10;
                if (score > highScore) highScore = score;

                // Accélération progressive
                niveauVitesse = Math.Min(5, 1 + score / 50);
                timer.Interval = Math.Max(60, 130 - niveauVitesse * 14);

                GenererNourriture();
            }
            else
            {
                serpent.RemoveAt(serpent.Count - 1);
            }

            this.Invalidate();
        }
        #endregion

        #region Dessin du jeu
        // ══════════════════════════════════════════
        //  DESSIN PRINCIPAL
        // ══════════════════════════════════════════
        private void Snake_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;

            // Origines des zones
            int zoneX = MARGE;
            int zoneY = PANNEAU_HAUT + MARGE;

            // 1) Fond général
            g.Clear(couleurFond);

            // 2) Grille de jeu
            DessinerGrille(g, zoneX, zoneY);

            // 3) Bordure de la zone de jeu
            DessinerBordureNeon(g, zoneX - 2, zoneY - 2, zonjeJeuLargeur + 4, zoneJeuHauteur + 4, couleurNeonVert);

            // 4) Panneau du haut (score)
            DessinerPanneauHaut(g);

            // 5) Panneau latéral droit
            DessinerPanneauDroit(g, zoneX + zonjeJeuLargeur + MARGE * 2, zoneY);

            // 6) Nourriture
            DessinerNourriture(g, zoneX, zoneY);

            // 7) Serpent
            DessinerSerpent(g, zoneX, zoneY);

            // 8) Overlay démarrage
            if (!jeuDemarre && jeuEnCours)
                DessinerOverlayDemarrage(g, zoneX, zoneY);

            // 9) Overlay Game Over
            if (!jeuEnCours)
                DessinerOverlayGameOver(g, zoneX, zoneY);
        }

        // ──────────────────────────────────────────
        private void DessinerGrille(Graphics g, int ox, int oy)
        {
            using (Pen styloGrille = new Pen(couleurGrille, 1))
            {
                for (int x = 0; x <= zonjeJeuLargeur; x += tailleCase)
                    g.DrawLine(styloGrille, ox + x, oy, ox + x, oy + zoneJeuHauteur);
                for (int y = 0; y <= zoneJeuHauteur; y += tailleCase)
                    g.DrawLine(styloGrille, ox, oy + y, ox + zonjeJeuLargeur, oy + y);
            }
        }

        // ──────────────────────────────────────────
        private void DessinerSerpent(Graphics g, int ox, int oy)
        {
            for (int i = serpent.Count - 1; i >= 0; i--)
            {
                Point seg = serpent[i];
                Rectangle rect = new Rectangle(
                    ox + seg.X * tailleCase + 2,
                    oy + seg.Y * tailleCase + 2,
                    tailleCase - 4,
                    tailleCase - 4
                );

                if (i == 0)
                {
                    // ── Tête ──
                    using (SolidBrush br = new SolidBrush(couleurNeonVert))
                        g.FillRectangle(br, rect);

                    // Halo tête
                    DessinerHalo(g, rect, couleurNeonVert, 8);

                    // Yeux
                    DessinerYeux(g, ox + seg.X * tailleCase, oy + seg.Y * tailleCase);
                }
                else
                {
                    // ── Corps avec dégradé ──
                    float t = (float)i / serpent.Count;
                    int vert = (int)(255 - t * 130);
                    Color couleurSeg = Color.FromArgb(0, Math.Max(60, vert), 70);

                    using (SolidBrush br = new SolidBrush(couleurSeg))
                        g.FillRectangle(br, rect);

                    // Bordure subtile
                    using (Pen p = new Pen(Color.FromArgb(60, couleurNeonVert), 1))
                        g.DrawRectangle(p, rect);
                }
            }
        }

        private void DessinerYeux(Graphics g, int x, int y)
        {
            // Positions selon la direction
            Point oeil1, oeil2;
            int d = 4;
            switch (direction)
            {
                case 0: oeil1 = new Point(x + 4, y + 4); oeil2 = new Point(x + 13, y + 4); break;
                case 2: oeil1 = new Point(x + 4, y + 13); oeil2 = new Point(x + 13, y + 13); break;
                case 1: oeil1 = new Point(x + 13, y + 4); oeil2 = new Point(x + 13, y + 13); break;
                default: oeil1 = new Point(x + 4, y + 4); oeil2 = new Point(x + 4, y + 13); break;
            }
            g.FillRectangle(Brushes.Black, oeil1.X, oeil1.Y, 3, 3);
            g.FillRectangle(Brushes.Black, oeil2.X, oeil2.Y, 3, 3);
        }

        // ──────────────────────────────────────────
        private void DessinerNourriture(Graphics g, int ox, int oy)
        {
            float cx = ox + nourriture.X * tailleCase + tailleCase / 2f;
            float cy = oy + nourriture.Y * tailleCase + tailleCase / 2f;
            float r = tailleCase / 2f - 2;

            // Halo externe pulsant
            float pulse = 1f + 0.25f * (float)Math.Sin(angleNourriture);
            float haloR = r * pulse + 6;
            using (GraphicsPath haloPath = new GraphicsPath())
            {
                haloPath.AddEllipse(cx - haloR, cy - haloR, haloR * 2, haloR * 2);
                using (PathGradientBrush haloBrush = new PathGradientBrush(haloPath))
                {
                    haloBrush.CenterColor = Color.FromArgb(80, couleurNeonRouge);
                    haloBrush.SurroundColors = new[] { Color.FromArgb(0, couleurNeonRouge) };
                    g.FillEllipse(haloBrush, cx - haloR, cy - haloR, haloR * 2, haloR * 2);
                }
            }

            // Corps de la nourriture
            using (GraphicsPath path = new GraphicsPath())
            {
                path.AddEllipse(cx - r, cy - r, r * 2, r * 2);
                using (PathGradientBrush brush = new PathGradientBrush(path))
                {
                    brush.CenterPoint = new PointF(cx - 2, cy - 2);
                    brush.CenterColor = Color.FromArgb(255, 180, 190);
                    brush.SurroundColors = new[] { Color.FromArgb(180, 0, 20) };
                    g.FillEllipse(brush, cx - r, cy - r, r * 2, r * 2);
                }
            }

            // Reflet
            g.FillEllipse(
                new SolidBrush(Color.FromArgb(120, 255, 255, 255)),
                cx - r * 0.5f - 1, cy - r * 0.6f - 1, r * 0.5f, r * 0.4f
            );
        }

        // ──────────────────────────────────────────
        private void DessinerPanneauHaut(Graphics g)
        {
            Rectangle panRect = new Rectangle(0, 0, this.ClientSize.Width, PANNEAU_HAUT);

            // Fond du panneau
            using (LinearGradientBrush br = new LinearGradientBrush(
                panRect, Color.FromArgb(10, 20, 15), Color.FromArgb(5, 10, 14),
                LinearGradientMode.Vertical))
                g.FillRectangle(br, panRect);

            // Ligne de séparation néon
            using (Pen p = new Pen(couleurNeonVert, 1))
                g.DrawLine(p, 0, PANNEAU_HAUT - 1, this.ClientSize.Width, PANNEAU_HAUT - 1);

            // Titre
            using (Font fTitre = new Font("Consolas", 22, FontStyle.Bold))
            {
                string titre1 = "SN";
                string titre2 = "AK";
                string titre3 = "E";
                float tx = 20;
                float ty = (PANNEAU_HAUT - 30) / 2f;

                g.DrawString(titre1, fTitre, new SolidBrush(couleurNeonVert), tx, ty);
                float w1 = g.MeasureString(titre1, fTitre).Width - 4;
                g.DrawString(titre2, fTitre, new SolidBrush(couleurNeonCyan), tx + w1, ty);
                float w2 = g.MeasureString(titre2, fTitre).Width - 4;
                g.DrawString(titre3, fTitre, new SolidBrush(couleurNeonVert), tx + w1 + w2, ty);
            }

            // Score
            DessinerMiniStat(g, 180, 10, "SCORE", score.ToString("D3"), couleurNeonJaune);
            DessinerMiniStat(g, 340, 10, $"RECORD ({Joueur.ToUpper()})", highScore.ToString("D3"), couleurNeonCyan);
            DessinerMiniStat(g, 500, 10, "TAILLE", serpent.Count.ToString(), couleurNeonVert);

            // Chrono
            int totalSec = ticksEcoules;
            string chrono = $"{totalSec / 60:D2}:{totalSec % 60:D2}";
            DessinerMiniStat(g, 600, 10, "TEMPS", chrono, Color.FromArgb(0, 200, 100));
        }

        private void DessinerMiniStat(Graphics g, int x, int y, string label, string valeur, Color couleur)
        {
            using (Font fLabel = new Font("Consolas", 7, FontStyle.Regular))
            using (Font fVal = new Font("Consolas", 16, FontStyle.Bold))
            {
                g.DrawString(label, fLabel, new SolidBrush(Color.FromArgb(120, couleur)), x, y + 2);
                g.DrawString(valeur, fVal, new SolidBrush(couleur), x, y + 14);
            }
        }

        // ──────────────────────────────────────────
        private void DessinerPanneauDroit(Graphics g, int ox, int oy)
        {
            int largeur = PANNEAU_DROITE - MARGE;
            int hauteur = zoneJeuHauteur;

            Rectangle panRect = new Rectangle(ox, oy, largeur, hauteur);

            // Fond
            using (SolidBrush br = new SolidBrush(Color.FromArgb(8, 15, 12)))
                g.FillRectangle(br, panRect);

            DessinerBordureNeon(g, ox, oy, largeur, hauteur, Color.FromArgb(0, 150, 80));

            int cy = oy + 16;

            // ── Bloc CONTRÔLES ──
            cy = DessinerBloc(g, ox, cy, largeur, "CONTRÔLES");
            cy += 8;

            string[,] touches = {
                {"↑ / Z",  "Haut"},
                {"↓ / S",  "Bas"},
                {"← / Q",  "Gauche"},
                {"→ / D",  "Droite"},
                {"ESPACE", "Restart"},
            };

            using (Font fTouche = new Font("Consolas", 9, FontStyle.Bold))
            using (Font fAction = new Font("Consolas", 8, FontStyle.Regular))
            {
                for (int i = 0; i < touches.GetLength(0); i++)
                {
                    // Fond de la touche
                    Rectangle keyRect = new Rectangle(ox + 10, cy, 55, 18);
                    using (SolidBrush br = new SolidBrush(Color.FromArgb(25, couleurNeonVert)))
                        g.FillRectangle(br, keyRect);
                    using (Pen p = new Pen(Color.FromArgb(80, couleurNeonVert), 1))
                        g.DrawRectangle(p, keyRect);

                    g.DrawString(touches[i, 0], fTouche, new SolidBrush(couleurNeonVert), ox + 12, cy + 2);
                    g.DrawString(touches[i, 1], fAction, new SolidBrush(Color.FromArgb(160, couleurNeonVert)), ox + 70, cy + 3);
                    cy += 24;
                }
            }

            cy += 10;

            // ── Bloc VITESSE ──
            cy = DessinerBloc(g, ox, cy, largeur, "VITESSE");
            cy += 12;

            int segLargeur = (largeur - 24) / 5 - 3;
            for (int i = 0; i < 5; i++)
            {
                bool actif = i < niveauVitesse;
                Color couleurSeg = actif ? couleurNeonVert : Color.FromArgb(20, 60, 35);
                Rectangle seg = new Rectangle(ox + 10 + i * (segLargeur + 4), cy, segLargeur, 12);
                using (SolidBrush br = new SolidBrush(couleurSeg))
                    g.FillRectangle(br, seg);
                if (actif)
                    DessinerHalo(g, seg, couleurNeonVert, 4);
            }

            using (Font fNiveau = new Font("Consolas", 8))
                g.DrawString($"NIVEAU {niveauVitesse}", fNiveau,
                    new SolidBrush(Color.FromArgb(150, couleurNeonVert)), ox + 10, cy + 18);

            cy += 42;

            // ── Bloc INFO ──
            cy = DessinerBloc(g, ox, cy, largeur, "SYSTÈME");
            cy += 10;

            using (Font fInfo = new Font("Consolas", 7))
            {
                // Point clignotant
                Color clignotant = (DateTime.Now.Millisecond < 500) ? couleurNeonVert : Color.FromArgb(40, couleurNeonVert);
                g.FillEllipse(new SolidBrush(clignotant), ox + 10, cy + 3, 7, 7);
                g.DrawString("SYSTÈME ACTIF", fInfo, new SolidBrush(Color.FromArgb(120, couleurNeonVert)), ox + 22, cy);
                cy += 16;
                g.DrawString($"GRILLE {zonjeJeuLargeur / tailleCase}×{zoneJeuHauteur / tailleCase}", fInfo, new SolidBrush(Color.FromArgb(80, couleurNeonVert)), ox + 10, cy);
                cy += 14;
                g.DrawString("VER. 1.0.0", fInfo, new SolidBrush(Color.FromArgb(60, couleurNeonVert)), ox + 10, cy);
            }

            cy += 30;

            // ── Bloc NAVIGATION ──
            cy = DessinerBloc(g, ox, cy, largeur, "NAVIGATION");
            cy += 10;

            using (Font fNav = new Font("Consolas", 7))
            {
                g.DrawString("Quitter et aller vers :", fNav,
                    new SolidBrush(Color.FromArgb(80, couleurNeonVert)), ox + 10, cy);
            }
        }

        private int DessinerBloc(Graphics g, int ox, int oy, int largeur, string titre)
        {
            // Ligne supérieure
            using (Pen p = new Pen(Color.FromArgb(50, couleurNeonVert), 1))
                g.DrawLine(p, ox + 10, oy, ox + largeur - 10, oy);

            // Titre
            using (Font f = new Font("Consolas", 7, FontStyle.Bold))
            {
                SizeF sz = g.MeasureString(titre, f);
                g.DrawString(titre, f, new SolidBrush(couleurNeonCyan), ox + 12, oy - 7);
            }

            return oy + 8;
        }

        // ──────────────────────────────────────────
        private void DessinerOverlayDemarrage(Graphics g, int ox, int oy)
        {
            // Fond semi-transparent
            using (SolidBrush br = new SolidBrush(Color.FromArgb(200, 5, 10, 14)))
                g.FillRectangle(br, ox, oy, zonjeJeuLargeur, zoneJeuHauteur);

            string msg1 = "PRÊT ?";
            string msg2 = "Appuyez sur une flèche";
            string msg3 = "pour commencer";

            using (Font f1 = new Font("Consolas", 30, FontStyle.Bold))
            using (Font f2 = new Font("Consolas", 13))
            {
                float cx = ox + zonjeJeuLargeur / 2f;
                float cy = oy + zoneJeuHauteur / 2f;

                // Titre clignotant
                Color c = (DateTime.Now.Millisecond < 600) ? couleurNeonCyan : Color.FromArgb(60, couleurNeonCyan);
                SizeF sz1 = g.MeasureString(msg1, f1);
                g.DrawString(msg1, f1, new SolidBrush(c), cx - sz1.Width / 2, cy - 60);

                // Sous-titre
                SizeF sz2 = g.MeasureString(msg2, f2);
                SizeF sz3 = g.MeasureString(msg3, f2);
                g.DrawString(msg2, f2, new SolidBrush(Color.FromArgb(180, couleurNeonVert)), cx - sz2.Width / 2, cy);
                g.DrawString(msg3, f2, new SolidBrush(Color.FromArgb(180, couleurNeonVert)), cx - sz3.Width / 2, cy + 22);
            }
        }

        // ──────────────────────────────────────────
        private void DessinerOverlayGameOver(Graphics g, int ox, int oy)
        {
            // Fond semi-transparent
            using (SolidBrush br = new SolidBrush(Color.FromArgb(210, 5, 10, 14)))
                g.FillRectangle(br, ox, oy, zonjeJeuLargeur, zoneJeuHauteur);

            // Cadre central
            int cw = 320, ch = 200;
            int cx = ox + (zonjeJeuLargeur - cw) / 2;
            int cy = oy + (zoneJeuHauteur - ch) / 2;
            Rectangle cadre = new Rectangle(cx, cy, cw, ch);

            using (SolidBrush br = new SolidBrush(Color.FromArgb(220, 8, 15, 12)))
                g.FillRectangle(br, cadre);

            DessinerBordureNeon(g, cx, cy, cw, ch, couleurNeonRouge);

            // "GAME OVER"
            using (Font fGO = new Font("Consolas", 26, FontStyle.Bold))
            {
                string go = "GAME OVER";
                SizeF sz = g.MeasureString(go, fGO);
                g.DrawString(go, fGO, new SolidBrush(couleurNeonRouge),
                    cx + (cw - sz.Width) / 2, cy + 20);
            }

            // Score final
            using (Font fScore = new Font("Consolas", 14))
            {
                string sScore = $"SCORE : {score:D3}";
                SizeF sz = g.MeasureString(sScore, fScore);
                g.DrawString(sScore, fScore, new SolidBrush(couleurNeonJaune),
                    cx + (cw - sz.Width) / 2, cy + 80);

                // Nouveau record
                if (score == highScore && score > 0)
                {
                    string sRecord = "★ NOUVEAU RECORD ! ★";
                    SizeF szR = g.MeasureString(sRecord, fScore);
                    g.DrawString(sRecord, fScore, new SolidBrush(couleurNeonCyan),
                        cx + (cw - szR.Width) / 2, cy + 108);
                }
            }

            // Bouton RECOMMENCER
            int bw = 200, bh = 36;
            Rectangle bouton = new Rectangle(cx + (cw - bw) / 2, cy + 148, bw, bh);
            using (SolidBrush br = new SolidBrush(Color.FromArgb(40, couleurNeonVert)))
                g.FillRectangle(br, bouton);
            DessinerBordureNeon(g, bouton.X, bouton.Y, bouton.Width, bouton.Height, couleurNeonVert);

            using (Font fBtn = new Font("Consolas", 12, FontStyle.Bold))
            {
                string txtBtn = "↵ RECOMMENCER";
                SizeF sz = g.MeasureString(txtBtn, fBtn);
                g.DrawString(txtBtn, fBtn, new SolidBrush(couleurNeonVert),
                    bouton.X + (bw - sz.Width) / 2, bouton.Y + (bh - sz.Height) / 2 + 1);
            }
        }

        // ══════════════════════════════════════════
        //  UTILITAIRES DESSIN
        // ══════════════════════════════════════════
        private void DessinerBordureNeon(Graphics g, int x, int y, int w, int h, Color couleur)
        {
            // Bordure principale
            using (Pen p = new Pen(couleur, 2))
                g.DrawRectangle(p, x, y, w, h);

            // Halo extérieur subtil
            using (Pen p2 = new Pen(Color.FromArgb(40, couleur), 4))
                g.DrawRectangle(p2, x - 1, y - 1, w + 2, h + 2);

            // Coins accentués en cyan
            int coin = 10;
            using (Pen pCoin = new Pen(couleurNeonCyan, 2))
            {
                // Coin haut-gauche
                g.DrawLine(pCoin, x, y, x + coin, y);
                g.DrawLine(pCoin, x, y, x, y + coin);
                // Coin haut-droit
                g.DrawLine(pCoin, x + w - coin, y, x + w, y);
                g.DrawLine(pCoin, x + w, y, x + w, y + coin);
                // Coin bas-gauche
                g.DrawLine(pCoin, x, y + h - coin, x, y + h);
                g.DrawLine(pCoin, x, y + h, x + coin, y + h);
                // Coin bas-droit
                g.DrawLine(pCoin, x + w, y + h - coin, x + w, y + h);
                g.DrawLine(pCoin, x + w - coin, y + h, x + w, y + h);
            }
        }

        private void DessinerHalo(Graphics g, Rectangle rect, Color couleur, int rayon)
        {
            Rectangle haloRect = new Rectangle(
                rect.X - rayon, rect.Y - rayon,
                rect.Width + rayon * 2, rect.Height + rayon * 2
            );
            using (GraphicsPath path = new GraphicsPath())
            {
                path.AddRectangle(haloRect);
                using (PathGradientBrush br = new PathGradientBrush(path))
                {
                    br.CenterColor = Color.FromArgb(60, couleur);
                    br.SurroundColors = new[] { Color.FromArgb(0, couleur) };
                    g.FillRectangle(br, haloRect);
                }
            }
        }
        #endregion

        #region Contrôles clavier
        // ══════════════════════════════════════════
        //  CONTRÔLES CLAVIER
        // ══════════════════════════════════════════
        private void Snake_KeyDown(object sender, KeyEventArgs e)
        {
            // Redémarrer avec Espace si Game Over
            if (!jeuEnCours && e.KeyCode == Keys.Space)
            {
                DemarrerNouveauJeu();
                return;
            }

            // Démarrer au premier appui sur une touche directionnelle
            if (!jeuDemarre && jeuEnCours)
            {
                bool estDirectionnel =
                    e.KeyCode == Keys.Up || e.KeyCode == Keys.Down ||
                    e.KeyCode == Keys.Left || e.KeyCode == Keys.Right ||
                    e.KeyCode == Keys.Z || e.KeyCode == Keys.S ||
                    e.KeyCode == Keys.Q || e.KeyCode == Keys.D ||
                    e.KeyCode == Keys.A;

                if (estDirectionnel)
                {
                    jeuDemarre = true;
                    timer.Start();
                }
            }

            // Changer de direction (sans demi-tour)
            switch (e.KeyCode)
            {
                case Keys.Up:
                case Keys.Z:
                    if (direction != 2) prochaineDirection = 0;
                    break;
                case Keys.Right:
                case Keys.D:
                    if (direction != 3) prochaineDirection = 1;
                    break;
                case Keys.Down:
                case Keys.S:
                    if (direction != 0) prochaineDirection = 2;
                    break;
                case Keys.Left:
                case Keys.Q:
                case Keys.A:
                    if (direction != 1) prochaineDirection = 3;
                    break;
            }
        }
        #endregion

        #region Game Over
        // ══════════════════════════════════════════
        //  GAME OVER
        // ══════════════════════════════════════════
        private void GameOver()
        {
            jeuEnCours = false;
            jeuDemarre = false;
            timer.Stop();
            this.Invalidate();
            // Pas de MessageBox — le Game Over est dessiné directement sur le canvas
        }

        private void Form3_Load(object sender, EventArgs e) { }
    }
    #endregion
}