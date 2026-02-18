using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Jeux_Multiples
{
    public partial class Puissance_4 : Form
    {
        #region Constantes
        // ══════════════════════════════════════════════════════
        //  CONSTANTES
        // ══════════════════════════════════════════════════════
        public const int COLS = 7;
        public const int ROWS = 6;
        public const int CELL = 80;
        public const int MARGIN = 20;
        public const int HEADER = 80;
        #endregion

        #region Etat du jeu
        // ══════════════════════════════════════════════════════
        //  ÉTAT DU JEU
        // ══════════════════════════════════════════════════════
        public int[,] board = new int[ROWS, COLS]; // 0=vide 1=Rouge 2=Jaune
        public bool rouge_Tour = true;
        public bool gameOver = false;
        public int scoreRouge = 0;
        public int scoreJaune = 0;
        public List<Point> winCells = new List<Point>();
        #endregion

        #region Animation
        // ══════════════════════════════════════════════════════
        //  ANIMATION DE CHUTE
        // ══════════════════════════════════════════════════════
        public Timer dropTimer = new Timer();
        public int dropCol = -1;
        public int dropTarget = -1;   // ligne cible
        public float dropY = 0f;   // position Y courante en pixels
        public float dropVel = 0f;   // vitesse verticale
        public bool dropping = false;
        public int dropJoueur = 0;

        // ══════════════════════════════════════════════════════
        //  ANIMATION GLOW (cases gagnantes)
        // ══════════════════════════════════════════════════════
        public float glowPhase = 0f;
        public Timer glowTimer = new Timer();

        // ══════════════════════════════════════════════════════
        //  HOVER
        // ══════════════════════════════════════════════════════
        public int hoverCol = -1;

        // ══════════════════════════════════════════════════════
        //  COULEURS NEON
        // ══════════════════════════════════════════════════════
        public readonly Color COL_BG = Color.FromArgb(10, 10, 18);
        public readonly Color COL_PLATEAU = Color.FromArgb(0, 60, 180); // Bleu vibrant
        public readonly Color COL_TROU = Color.FromArgb(5, 5, 10);
        public readonly Color COL_ROUGE = Color.FromArgb(255, 34, 68);   // Rouge Néon
        public readonly Color COL_JAUNE = Color.FromArgb(255, 230, 0);   // Jaune Néon
        public readonly Color COL_TITLE = Color.FromArgb(0, 238, 255);   // Cyan Néon
        public readonly Color COL_WIN = Color.FromArgb(0, 255, 136);     // Vert Néon

        // ══════════════════════════════════════════════════════
        //  POLICES
        // ══════════════════════════════════════════════════════
        public Font fontTitle = new Font("Segoe UI Black", 17, FontStyle.Bold);
        public Font fontScore = new Font("Segoe UI", 12, FontStyle.Bold);
        public Font fontStatus = new Font("Segoe UI", 12, FontStyle.Bold);

        // ══════════════════════════════════════════════════════
        //  COORDONNÉES GRILLE
        // ══════════════════════════════════════════════════════
        public int GridLeft { get { return MARGIN; } }
        public int GridTop { get { return HEADER; } }
        public int GridRight { get { return GridLeft + COLS * CELL; } }
        public int GridBottom { get { return GridTop + ROWS * CELL; } }
        #endregion

        #region Buttons
        // ══════════════════════════════════════════════════════
        //  BOUTON
        // ══════════════════════════════════════════════════════
        public Button btnNouveau;

        // ──────────────────────────────────────────────────────
        public string Joueur1 = "Rouge";
        public string Joueur2 = "Jaune";
        public bool IsMultiplayer = false;

        // ──────────────────────────────────────────────────────
        public Puissance_4(string joueur1, string joueur2, bool isMultiplayer = false)
        {
            InitializeComponent();
            if (!string.IsNullOrWhiteSpace(joueur1)) Joueur1 = joueur1;
            if (!string.IsNullOrWhiteSpace(joueur2)) Joueur2 = joueur2;
            this.IsMultiplayer = isMultiplayer;
        }

        // Pour compatibilité si nécessaire, ou constructeur par défaut
        public Puissance_4() : this("Rouge", "Jaune", false) { }

        // ──────────────────────────────────────────────────────
        public void Puissance_4_Load(object sender, EventArgs e)
        {
            if (IsMultiplayer)
            {
                NetworkManager.Instance.OnPacketReceived += OnPacketReceived;
                NetworkManager.Instance.OnDisconnected += OnDisconnected;
                Joueur1 = NetworkManager.Instance.IsHost ? NetworkManager.Instance.MyPseudo : NetworkManager.Instance.OpponentPseudo;
                Joueur2 = NetworkManager.Instance.IsHost ? NetworkManager.Instance.OpponentPseudo : NetworkManager.Instance.MyPseudo;
            }
            
            this.Text = $"Puissance 4 - {Joueur1} vs {Joueur2}";
            this.ClientSize = new Size(GridRight + MARGIN, GridBottom + MARGIN + 55);
            this.DoubleBuffered = true;
            this.BackColor = COL_BG;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.StartPosition = FormStartPosition.CenterScreen;

            // ── Bouton Nouvelle Partie ────────────────────────
            btnNouveau = new Button();
            btnNouveau.Text = "NOUVELLE PARTIE";
            btnNouveau.Font = new Font("Segoe UI", 9, FontStyle.Bold);
            btnNouveau.ForeColor = COL_TITLE;
            btnNouveau.BackColor = Color.FromArgb(20, 20, 30);
            btnNouveau.FlatStyle = FlatStyle.Flat;
            btnNouveau.Size = new Size(160, 40);
            btnNouveau.Cursor = Cursors.Hand;
            btnNouveau.FlatAppearance.BorderColor = COL_TITLE;
            btnNouveau.FlatAppearance.BorderSize = 1;
            btnNouveau.FlatAppearance.MouseOverBackColor = Color.FromArgb(40, COL_TITLE.R, COL_TITLE.G, COL_TITLE.B);
            btnNouveau.Location = new Point(
                (this.ClientSize.Width - btnNouveau.Width) / 2,
                GridBottom + 15);
            btnNouveau.Click += new EventHandler(btnNouveau_Click);
            this.Controls.Add(btnNouveau);

            // ── Événements souris ─────────────────────────────
            this.MouseMove += new MouseEventHandler(OnMouseMove);
            this.MouseClick += new MouseEventHandler(OnMouseClick);
            this.MouseLeave += new EventHandler(OnMouseLeave);

            // ── Timer chute ───────────────────────────────────
            dropTimer.Interval = 16;
            dropTimer.Tick += new EventHandler(dropTimer_Tick);

            // ── Timer glow ────────────────────────────────────
            glowTimer.Interval = 40;
            glowTimer.Tick += new EventHandler(glowTimer_Tick);

            NouvellePartie();

            // ── Bouton Changer de jeux (Snake) à gauche ───────────────────────
            Button btnChanger2 = new Button();
            btnChanger2.Text = "SNAKE";
            btnChanger2.Font = new Font("Segoe UI", 9, FontStyle.Bold);
            btnChanger2.ForeColor = COL_WIN; // Vert
            btnChanger2.BackColor = Color.FromArgb(20, 20, 30);
            btnChanger2.FlatStyle = FlatStyle.Flat;
            btnChanger2.Size = new Size(140, 40);
            btnChanger2.Cursor = Cursors.Hand;
            btnChanger2.FlatAppearance.BorderColor = COL_WIN;
            btnChanger2.FlatAppearance.BorderSize = 1;
            btnChanger2.FlatAppearance.MouseOverBackColor = Color.FromArgb(40, COL_WIN.R, COL_WIN.G, COL_WIN.B);
            btnChanger2.Location = new Point(
                (this.ClientSize.Width - btnChanger2.Width) / 2 - 170,
                GridBottom + 15);
            btnChanger2.Click += new EventHandler(btnChanger2_Click);
            this.Controls.Add(btnChanger2);

            // ── Bouton Changer de jeux (Morpion) à droite ───────────────────────
            Button btnChanger = new Button();
            btnChanger.Text = "MORPION";
            btnChanger.Font = new Font("Segoe UI", 9, FontStyle.Bold);
            btnChanger.ForeColor = COL_ROUGE; // Rouge
            btnChanger.BackColor = Color.FromArgb(20, 20, 30);
            btnChanger.FlatStyle = FlatStyle.Flat;
            btnChanger.Size = new Size(140, 40);
            btnChanger.Cursor = Cursors.Hand;
            btnChanger.FlatAppearance.BorderColor = COL_ROUGE;
            btnChanger.FlatAppearance.BorderSize = 1;
            btnChanger.FlatAppearance.MouseOverBackColor = Color.FromArgb(40, COL_ROUGE.R, COL_ROUGE.G, COL_ROUGE.B);
            btnChanger.Location = new Point(
                (this.ClientSize.Width - btnChanger.Width) / 2 + 170,
                GridBottom + 15);
            btnChanger.Click += new EventHandler(btnChanger_Click);
            this.Controls.Add(btnChanger);

        }
        #endregion

        #region Gestionnaires d'événements
        // ══════════════════════════════════════════════════════
        //  GESTIONNAIRES D'ÉVÉNEMENTS
        // ══════════════════════════════════════════════════════
        public void btnNouveau_Click(object sender, EventArgs e)
        {
            NouvellePartie();
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
                int col = int.Parse(p.Content);
                this.Invoke(new Action(() => JouerCoup(col, false))); // False = Remote move
            }
        }

        public void glowTimer_Tick(object sender, EventArgs e)
        {
            glowPhase += 0.12f;
            Invalidate();
        }

        public void OnMouseLeave(object sender, EventArgs e)
        {
            hoverCol = -1;
            Invalidate();
        }

        public void OnMouseMove(object sender, MouseEventArgs e)
        {
            if (dropping || gameOver) return;
            int col = PixelToCol(e.X);
            if (col != hoverCol)
            {
                hoverCol = col;
                Invalidate();
            }
        }

        public void OnMouseClick(object sender, MouseEventArgs e)
        {
            if (gameOver || dropping) return;
            
            // MULTI: Vérifier tour
            if (IsMultiplayer)
            {
                bool isMyTurn = NetworkManager.Instance.IsHost ? rouge_Tour : !rouge_Tour;
                if (!isMyTurn) return; 
            }

            int col = PixelToCol(e.X);
            if (col < 0) return;

            JouerCoup(col, true); // True = Local move
        }

        public void JouerCoup(int col, bool isLocal)
        {
            if (gameOver || dropping) return;

            // Trouver la ligne la plus basse disponible
            int targetRow = -1;
            for (int r = ROWS - 1; r >= 0; r--)
            {
                if (board[r, col] == 0)
                {
                    targetRow = r;
                    break;
                }
            }
            if (targetRow < 0) return; // colonne pleine

            // Send Packet if Local
            if (IsMultiplayer && isLocal)
            {
                NetworkManager.Instance.SendPacket(new Packet("MOVE", NetworkManager.Instance.MyPseudo, col.ToString()));
            }

            // Lancer l'animation de chute
            dropping = true;
            dropCol = col;
            dropTarget = targetRow;
            dropJoueur = rouge_Tour ? 1 : 2;
            dropY = GridTop - CELL / 2f;
            dropVel = 4f;
            dropTimer.Start();
        }

        public void dropTimer_Tick(object sender, EventArgs e)
        {
            float targetY = GridTop + dropTarget * CELL + CELL / 2f;
            dropVel += 2.5f;   // gravité
            dropY += dropVel;

            if (dropY >= targetY)
            {
                dropY = targetY;
                dropping = false;
                dropTimer.Stop();

                // Poser le jeton
                board[dropTarget, dropCol] = dropJoueur;

                if (VerifierVictoire(dropTarget, dropCol, dropJoueur))
                {
                    gameOver = true;
                    if (dropJoueur == 1) scoreRouge++;
                    else scoreJaune++;
                    glowTimer.Start();
                    Invalidate();
                    string nom = dropJoueur == 1 ? Joueur1 : Joueur2;
                    MessageBox.Show(
                        "Le joueur " + nom + " gagne la partie !",
                        "Puissance 4 - Victoire !",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.None);
                    return;
                }

                if (PlateauPlein())
                {
                    gameOver = true;
                    Invalidate();
                    MessageBox.Show("Match nul !", "Puissance 4",
                        MessageBoxButtons.OK, MessageBoxIcon.None);
                    return;
                }

                rouge_Tour = !rouge_Tour;
            }
            Invalidate();
        }
        #endregion

        #region Logique du jeu
        // ══════════════════════════════════════════════════════
        //  LOGIQUE DU JEU
        // ══════════════════════════════════════════════════════
        public void NouvellePartie()
        {
            board = new int[ROWS, COLS];
            rouge_Tour = true;
            gameOver = false;
            dropping = false;
            winCells.Clear();
            dropTimer.Stop();
            glowTimer.Stop();
            Invalidate();
        }

        public int PixelToCol(int px)
        {
            if (px < GridLeft || px >= GridRight) return -1;
            return (px - GridLeft) / CELL;
        }

        public bool VerifierVictoire(int row, int col, int joueur)
        {
            int[] dr = { 0, 1, 1, 1 };
            int[] dc = { 1, 0, 1, -1 };

            winCells.Clear();

            for (int d = 0; d < 4; d++)
            {
                List<Point> cellules = new List<Point>();
                cellules.Add(new Point(col, row));

                for (int k = 1; k <= 3; k++)
                {
                    int nr = row + dr[d] * k;
                    int nc = col + dc[d] * k;
                    if (nr < 0 || nr >= ROWS || nc < 0 || nc >= COLS) break;
                    if (board[nr, nc] != joueur) break;
                    cellules.Add(new Point(nc, nr));
                }
                for (int k = 1; k <= 3; k++)
                {
                    int nr = row - dr[d] * k;
                    int nc = col - dc[d] * k;
                    if (nr < 0 || nr >= ROWS || nc < 0 || nc >= COLS) break;
                    if (board[nr, nc] != joueur) break;
                    cellules.Add(new Point(nc, nr));
                }

                if (cellules.Count >= 4)
                {
                    winCells = cellules;
                    return true;
                }
            }
            return false;
        }

        public bool PlateauPlein()
        {
            for (int c = 0; c < COLS; c++)
                if (board[0, c] == 0) return false;
            return true;
        }
        #endregion

        #region Dessin
        // ══════════════════════════════════════════════════════
        //  DESSIN
        // ══════════════════════════════════════════════════════
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (IsMultiplayer)
            {
                NetworkManager.Instance.OnPacketReceived -= OnPacketReceived;
                NetworkManager.Instance.OnDisconnected -= OnDisconnected;
            }
            base.OnFormClosing(e);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
            g.Clear(COL_BG);

            DessinerEntete(g);
            DessinerIndicateur(g);
            DessinerPlateau(g);
            DessinerJetons(g);
            DessinerJetonEnChute(g);
            DessinerStatut(g);
        }

        // ── En-tête ──────────────────────────────────────────
        public void DessinerEntete(Graphics g)
        {
            string titre = "PUISSANCE 4";
            SizeF szT = g.MeasureString(titre, fontTitle);
            g.DrawString(titre, fontTitle, new SolidBrush(COL_TITLE),
                (this.ClientSize.Width - szT.Width) / 2f, 8f);

            string sr = $"{Joueur1} : {scoreRouge}";
            g.DrawString(sr, fontScore, new SolidBrush(COL_ROUGE), (float)MARGIN, 12f);

            string sj = $"{Joueur2} : {scoreJaune}";
            SizeF szJ = g.MeasureString(sj, fontScore);
            g.DrawString(sj, fontScore, new SolidBrush(COL_JAUNE),
                GridRight - szJ.Width, 12f);
        }

        // ── Flèche indicatrice de colonne ────────────────────
        public void DessinerIndicateur(Graphics g)
        {
            if (hoverCol < 0 || gameOver || dropping) return;

            Color couleur = rouge_Tour ? COL_ROUGE : COL_JAUNE;
            int cx = GridLeft + hoverCol * CELL + CELL / 2;

            // Surbrillance de la colonne
            using (SolidBrush b = new SolidBrush(Color.FromArgb(40, couleur)))
            {
                g.FillRectangle(b,
                    GridLeft + hoverCol * CELL, HEADER,
                    CELL, ROWS * CELL);
            }

            // Triangle indicateur
            Point[] pts = new Point[]
            {
                new Point(cx,      HEADER - 5),
                new Point(cx - 11, HEADER - 20),
                new Point(cx + 11, HEADER - 20)
            };
            using (SolidBrush b = new SolidBrush(couleur))
            {
                g.FillPolygon(b, pts);
            }
        }

        // ── Plateau bleu avec trous ───────────────────────────
        public void DessinerPlateau(Graphics g)
        {
            // Fond du plateau arrondi
            using (SolidBrush b = new SolidBrush(COL_PLATEAU))
            {
                FillRoundedRect(g, b,
                    GridLeft - 6, GridTop - 6,
                    COLS * CELL + 12, ROWS * CELL + 12, 12);
            }

            // Trous vides
            for (int r = 0; r < ROWS; r++)
            {
                for (int c = 0; c < COLS; c++)
                {
                    if (board[r, c] == 0)
                    {
                        int cx = GridLeft + c * CELL + CELL / 2;
                        int cy = GridTop + r * CELL + CELL / 2;
                        int radius = CELL / 2 - 8;
                        using (SolidBrush b = new SolidBrush(COL_TROU))
                        {
                            g.FillEllipse(b, cx - radius, cy - radius,
                                radius * 2, radius * 2);
                        }
                    }
                }
            }
        }

        // ── Jetons posés ─────────────────────────────────────
        public void DessinerJetons(Graphics g)
        {
            int radius = CELL / 2 - 8;

            for (int r = 0; r < ROWS; r++)
            {
                for (int c = 0; c < COLS; c++)
                {
                    if (board[r, c] == 0) continue;

                    bool estRouge = (board[r, c] == 1);
                    bool estWin = winCells.Contains(new Point(c, r));
                    Color couleur = estRouge ? COL_ROUGE : COL_JAUNE;

                    int cx = GridLeft + c * CELL + CELL / 2;
                    int cy = GridTop + r * CELL + CELL / 2;

                    // Halo pulsant sur les jetons gagnants
                    if (estWin)
                    {
                        float glow = (float)(Math.Sin(glowPhase) * 0.5 + 0.5);
                        int alpha = (int)(50 + glow * 150);
                        int haloR = radius + 5 + (int)(glow * 6);
                        using (SolidBrush b = new SolidBrush(Color.FromArgb(alpha, COL_WIN)))
                        {
                            g.FillEllipse(b,
                                cx - haloR, cy - haloR,
                                haloR * 2, haloR * 2);
                        }
                        couleur = COL_WIN;
                    }

                    DessinerJeton(g, cx, cy, radius, couleur);
                }
            }
        }

        // ── Jeton en train de tomber ──────────────────────────
        public void DessinerJetonEnChute(Graphics g)
        {
            if (!dropping) return;
            int radius = CELL / 2 - 8;
            int cx = GridLeft + dropCol * CELL + CELL / 2;
            float cy = dropY;
            Color couleur = dropJoueur == 1 ? COL_ROUGE : COL_JAUNE;
            DessinerJeton(g, cx, (int)cy, radius, couleur);
        }

        // ── Dessin d'un seul jeton avec reflet 3D ────────────
        public void DessinerJeton(Graphics g, int cx, int cy, int radius, Color couleur)
        {
            // Base du jeton
            using (SolidBrush b = new SolidBrush(couleur))
            {
                g.FillEllipse(b, cx - radius, cy - radius, radius * 2, radius * 2);
            }

            // Reflet blanc en haut à gauche
            int refletR = radius / 2;
            int refletX = cx - radius / 2;
            int refletY = cy - radius / 2;
            using (GraphicsPath path = new GraphicsPath())
            {
                path.AddEllipse(cx - radius, cy - radius, radius * 2, radius * 2);
                using (PathGradientBrush grad = new PathGradientBrush(path))
                {
                    grad.CenterColor = Color.FromArgb(160, Color.White);
                    grad.SurroundColors = new Color[] { Color.FromArgb(0, couleur) };
                    grad.CenterPoint = new PointF(cx - radius / 3f, cy - radius / 3f);
                    g.FillEllipse(grad, cx - radius, cy - radius, radius * 2, radius * 2);
                }
            }

            // Contour léger
            using (Pen p = new Pen(Color.FromArgb(60, Color.White), 1.5f))
            {
                g.DrawEllipse(p, cx - radius, cy - radius, radius * 2, radius * 2);
            }
        }

        // ── Statut (tour en cours) ───────────────────────────
        public void DessinerStatut(Graphics g)
        {
            if (gameOver || dropping) return;

            string msg = rouge_Tour ? $"Tour de {Joueur1}" : $"Tour de {Joueur2}";
            Color couleur = rouge_Tour ? COL_ROUGE : COL_JAUNE;

            SizeF sz = g.MeasureString(msg, fontStatus);
            g.DrawString(msg, fontStatus, new SolidBrush(couleur),
                (this.ClientSize.Width - sz.Width) / 2f,
                GridBottom + 13f);
        }

        // ══════════════════════════════════════════════════════
        //  HELPER : rectangle arrondi
        // ══════════════════════════════════════════════════════
        public void FillRoundedRect(Graphics g, Brush b, int x, int y, int w, int h, int r)
        {
            using (GraphicsPath path = new GraphicsPath())
            {
                path.AddArc(x, y, r * 2, r * 2, 180, 90);
                path.AddArc(x + w - r * 2, y, r * 2, r * 2, 270, 90);
                path.AddArc(x + w - r * 2, y + h - r * 2, r * 2, r * 2, 0, 90);
                path.AddArc(x, y + h - r * 2, r * 2, r * 2, 90, 90);
                path.CloseFigure();
                g.FillPath(b, path);
            }
        }

        public void btnChanger_Click(object sender, EventArgs e)
        {
            mort_Pion formulaire = new mort_Pion(Joueur1, Joueur2); formulaire.Show();
        }

        public void btnChanger2_Click(object sender, EventArgs e)
        {
            Snake formulaire = new Snake(Joueur1); formulaire.Show();
        }
    }
    #endregion
}
