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
    public partial class mort_Pion : Form
    {
        // ══════════════════════════════════════════════════════
        //  CONSTANTES
        // ══════════════════════════════════════════════════════
        public const int GRID_SIZE = 10;
        public const int WIN_LENGTH = 5;
        public const int CELL = 58;
        public const int MARGIN = 20;
        public const int HEADER = 75;

        // ══════════════════════════════════════════════════════
        //  ÉTAT DU JEU
        // ══════════════════════════════════════════════════════
        public int[,] board = new int[GRID_SIZE, GRID_SIZE];
        public bool xTurn = true;
        public bool gameOver = false;
        public int scoreX = 0;
        public int scoreO = 0;
        public List<Point> winCells = new List<Point>();
        
        // MULTI
        public bool IsMultiplayer = false;
        public bool AmISpectating = false; // True si ce n'est pas mon tour ou si je suis spectateur

        // ══════════════════════════════════════════════════════
        //  HOVER
        // ══════════════════════════════════════════════════════
        private int hoverCol = -1;
        private int hoverRow = -1;

        // ══════════════════════════════════════════════════════
        //  ANIMATION GLOW
        // ══════════════════════════════════════════════════════
        public float glowPhase = 0f;
        public Timer glowTimer = new Timer();

        // ══════════════════════════════════════════════════════
        //  COULEURS NEON
        // ══════════════════════════════════════════════════════
        public readonly Color COL_BG = Color.FromArgb(10, 10, 18);
        public readonly Color COL_GRID = Color.FromArgb(20, 20, 30);
        public readonly Color COL_HOVER = Color.FromArgb(25, 25, 40);
        public readonly Color COL_X = Color.FromArgb(255, 34, 68);       // Rouge Néon
        public readonly Color COL_O = Color.FromArgb(0, 238, 255);       // Cyan Néon
        public readonly Color COL_WIN_GLOW = Color.FromArgb(0, 255, 136); // Vert Néon
        public readonly Color COL_TITLE = Color.FromArgb(255, 230, 0);    // Jaune Néon

        // ══════════════════════════════════════════════════════
        //  POLICES
        // ══════════════════════════════════════════════════════
        public Font fontTitle = new Font("Segoe UI Black", 17, FontStyle.Bold);
        public Font fontScore = new Font("Segoe UI", 12, FontStyle.Bold);
        public Font fontStatus = new Font("Segoe UI", 11, FontStyle.Bold);

        // ══════════════════════════════════════════════════════
        //  COORDONNÉES GRILLE
        // ══════════════════════════════════════════════════════
        // ══════════════════════════════════════════════════════
        //  COORDONNÉES GRILLE
        // ══════════════════════════════════════════════════════
        public int GridLeft { get { return (ClientSize.Width - GRID_SIZE * CELL) / 2; } }
        public int GridTop { get { return (ClientSize.Height - GRID_SIZE * CELL) / 2; } }
        public int GridRight { get { return GridLeft + GRID_SIZE * CELL; } }
        public int GridBottom { get { return GridTop + GRID_SIZE * CELL; } }

        // ══════════════════════════════════════════════════════
        //  BOUTON
        // ══════════════════════════════════════════════════════
        public Button btnNouveau;
        public Button btnChanger;
        public Button btnChanger2;

        // ──────────────────────────────────────────────────────
        public string Joueur1 = "Joueur X";
        public string Joueur2 = "Joueur O";

        // ──────────────────────────────────────────────────────
        public mort_Pion(string joueur1, string joueur2, bool isMultiplayer = false)
        {
            InitializeComponent();
            if(!string.IsNullOrWhiteSpace(joueur1)) Joueur1 = joueur1;
            if(!string.IsNullOrWhiteSpace(joueur2)) Joueur2 = joueur2;
            this.IsMultiplayer = isMultiplayer;
            this.Text = "Morpion (Tic-Tac-Toe)";
            FormUtils.ApplyFullScreen(this);
        }

        // Compatibilité
        public mort_Pion() : this("Joueur X", "Joueur O") { }

        // ──────────────────────────────────────────────────────
        private Label _lblRole; // Field for multiplayer role label

        public void mort_Pion_Load(object sender, EventArgs e)
        {
            this.Text = $"Mort Pion - {Joueur1} vs {Joueur2}";
            this.DoubleBuffered = true;
            this.BackColor = COL_BG;
            
            // ── Bouton Retour ─────────────────────────────────
            var btnRetour = FormUtils.CreateBackButton(this, () => 
            {
                 var acc = new Accueil();
                 acc.Show();
                 this.Close();
            });
            // Position handled in Resize/RecalcLayout or fixed
            // Let's fix it top-left
            btnRetour.Location = new Point(20, 20);

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
            btnNouveau.Click += new EventHandler(btnNouveau_Click);
            this.Controls.Add(btnNouveau);

            // ── Événements souris ─────────────────────────────
            this.MouseMove += new MouseEventHandler(OnMouseMove);
            this.MouseClick += new MouseEventHandler(OnMouseClick);
            this.MouseLeave += new EventHandler(OnMouseLeave);

            // ── Timer glow ────────────────────────────────────
            glowTimer.Interval = 40;
            glowTimer.Tick += new EventHandler(glowTimer_Tick);

            NouvellePartie();

            // ── Bouton Changer de Jeu à gauche à coté de nouvelle partie ─────────────────────────
            btnChanger = new Button(); 
            btnChanger.Text = "PUISSANCE 4"; 
            btnChanger.Font = new Font("Segoe UI", 9, FontStyle.Bold); 
            btnChanger.ForeColor = COL_WIN_GLOW; 
            btnChanger.BackColor = Color.FromArgb(20, 20, 30); 
            btnChanger.FlatStyle = FlatStyle.Flat; 
            btnChanger.Size = new Size(140, 40); 
            btnChanger.Cursor = Cursors.Hand; 
            btnChanger.FlatAppearance.BorderColor = COL_WIN_GLOW; 
            btnChanger.FlatAppearance.BorderSize = 1;
            btnChanger.FlatAppearance.MouseOverBackColor = Color.FromArgb(40, COL_WIN_GLOW.R, COL_WIN_GLOW.G, COL_WIN_GLOW.B);
            btnChanger.Click += new EventHandler(btnChanger_Click); 
            this.Controls.Add(btnChanger);

            // ── En local: enlever les boutons de changement de jeu ────────────
            btnChanger.Visible = false;

            // ── MULTIPLAYER INIT ──────────────────────────────
            if (IsMultiplayer)
            {
                // Pseudos (déjà passés via constructeur ou NetworkManager)
                if (NetworkManager.Instance.IsHost)
                {
                    Joueur1 = NetworkManager.Instance.MyPseudo;
                    Joueur2 = NetworkManager.Instance.OpponentPseudo;
                    AmISpectating = false; // Host joue X, commence
                }
                else
                {
                    Joueur1 = NetworkManager.Instance.OpponentPseudo;
                    Joueur2 = NetworkManager.Instance.MyPseudo;
                    AmISpectating = true; // Client joue O, attend
                }
                this.Text = $"[LAN] {Joueur1} (X) vs {Joueur2} (O)";

                NetworkManager.Instance.OnPacketReceived += OnPacketReceived;
                NetworkManager.Instance.OnDisconnected   += OnDisconnected;

                // Masquer les boutons inutiles en multi
                btnNouveau.Visible  = false;
                btnChanger.Visible  = false;
            }

            // ── Bouton Changer de Jeu 2 à droite à coté de nouvelle partie ───────────────────────
            btnChanger2 = new Button(); 
            btnChanger2.Text = "SNAKE"; 
            btnChanger2.Font = new Font("Segoe UI", 9, FontStyle.Bold); 
            btnChanger2.ForeColor = COL_O; 
            btnChanger2.BackColor = Color.FromArgb(20, 20, 30); 
            btnChanger2.FlatStyle = FlatStyle.Flat; 
            btnChanger2.Size = new Size(140, 40); 
            btnChanger2.Cursor = Cursors.Hand; 
            btnChanger2.FlatAppearance.BorderColor = COL_O; 
            btnChanger2.FlatAppearance.BorderSize = 1;
            btnChanger2.FlatAppearance.MouseOverBackColor = Color.FromArgb(40, COL_O.R, COL_O.G, COL_O.B);
            btnChanger2.Click += new EventHandler(btnChanger2_Click); 
            this.Controls.Add(btnChanger2);

            // ── En local: enlever les boutons de changement de jeu ────────────
            btnChanger2.Visible = false;

            // Masquer btnChanger2 si multi (btnChanger déjà masqué plus haut)
            if (IsMultiplayer)
            {
                btnChanger2.Visible = false;

                _lblRole = new Label
                {
                    Text      = NetworkManager.Instance.IsHost
                                ? $"Vous jouez ✕ ({Joueur1})"
                                : $"Vous jouez ○ ({Joueur2})",
                    ForeColor = NetworkManager.Instance.IsHost ? COL_X : COL_O,
                    Font      = new Font("Segoe UI", 9, FontStyle.Bold),
                    AutoSize  = false,
                    TextAlign = ContentAlignment.MiddleCenter,
                    Size      = new Size(GridRight, 20),
                    Location  = new Point(0, 0), // Updated in RecalcLayout
                };
                this.Controls.Add(_lblRole);
            }

            // Petit "À vous de jouer" en bas à gauche (pas au milieu)
            _lblAVousDeJouer = new Label
            {
                Text     = "",
                Font     = new Font("Segoe UI", 9),
                AutoSize = true,
                Location = new Point(MARGIN, GridBottom + 14),
                BackColor = Color.Transparent,
            };
            this.Controls.Add(_lblAVousDeJouer);
            UpdateTourLabel();
            
            this.Resize += (s, ev) => RecalcLayout();
            RecalcLayout();
        }

        private void RecalcLayout()
        {
             // Reposition Buttons
             int btnY = GridBottom + 30;
             int centerX = ClientSize.Width / 2;
             
             if (btnNouveau != null) btnNouveau.Location = new Point(centerX - btnNouveau.Width / 2, btnY);
             if (btnChanger != null) btnChanger.Location = new Point(centerX - btnChanger.Width / 2 - 170, btnY);
             if (btnChanger2 != null) btnChanger2.Location = new Point(centerX - btnChanger2.Width / 2 + 170, btnY);
             
             if (_lblAVousDeJouer != null) 
             {
                 _lblAVousDeJouer.Location = new Point(GridLeft, GridBottom + 55);
                 _lblAVousDeJouer.BringToFront();
             }

             if (_lblRole != null)
             {
                 _lblRole.Size = new Size(ClientSize.Width, 20);
                 _lblRole.Location = new Point(0, GridBottom + 20);
                 _lblRole.BringToFront();
             }
             
             Invalidate();
        }

        // ══════════════════════════════════════════════════════
        //  GESTIONNAIRES D'ÉVÉNEMENTS
        // ══════════════════════════════════════════════════════
        public void btnNouveau_Click(object sender, EventArgs e)
        {
            NouvellePartie();
        }

        public void glowTimer_Tick(object sender, EventArgs e)
        {
            glowPhase += 0.12f;
            Invalidate();
        }

        public void OnMouseLeave(object sender, EventArgs e)
        {
            hoverCol = -1;
            hoverRow = -1;
            Invalidate();
        }

        // ══════════════════════════════════════════════════════
        //  LOGIQUE DU JEU
        // ══════════════════════════════════════════════════════
        public void NouvellePartie()
        {
            board = new int[GRID_SIZE, GRID_SIZE];
            xTurn = true;
            UpdateTourLabel();
            gameOver = false;
            winCells.Clear();
            glowTimer.Stop();
            Invalidate();
        }

        // Convertit pixels -> case, retourne col=-1 si hors grille
        public void PixelToCell(int px, int py, out int col, out int row)
        {
            col = (px - GridLeft) / CELL;
            row = (py - GridTop) / CELL;
            if (col < 0 || col >= GRID_SIZE || row < 0 || row >= GRID_SIZE)
            {
                col = -1;
                row = -1;
            }
        }

        public void OnMouseMove(object sender, MouseEventArgs e)
        {
            int col, row;
            PixelToCell(e.X, e.Y, out col, out row);
            if (col != hoverCol || row != hoverRow)
            {
                hoverCol = col;
                hoverRow = row;
                Invalidate();
            }
        }

        public void OnMouseClick(object sender, MouseEventArgs e)
        {
            if (gameOver) return;

            int col, row;
            PixelToCell(e.X, e.Y, out col, out row);
            if (col < 0 || board[row, col] != 0) return;

            if (IsMultiplayer)
            {
                // Vérifier si c'est mon tour
                // Host est X (1), Client est O (2)
                bool isMyTurn = NetworkManager.Instance.IsHost ? xTurn : !xTurn;
                
                if (!isMyTurn) 
                {
                    // Pas mon tour !
                    return; 
                }
            }

            int joueur = xTurn ? 1 : 2;
            board[row, col] = joueur;

            // Envoi réseau SI MULTI
            if (IsMultiplayer)
            {
                NetworkManager.Instance.SendPacket(new Packet("MOVE", NetworkManager.Instance.MyPseudo, $"{col},{row}"));
            }

            if (VerifierVictoire(row, col, joueur))
            {
                gameOver = true;
                if (xTurn) scoreX++;
                else scoreO++;
                glowTimer.Start();
                Invalidate();
                string nom = xTurn ? Joueur1 : Joueur2;
                try { _ = NetworkManager.Instance.Lobby.RecordWinAsync(nom, "MortPion"); } catch { }
                MessageBox.Show(
                    nom + " gagne la partie !",
                    "Mort Pion - Victoire !",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.None);
                if (IsMultiplayer) ShowRematchDialog();
                return;
            }

            if (PlateauPlein())
            {
                gameOver = true;
                Invalidate();
                MessageBox.Show("Match nul !", "Mort Pion",
                    MessageBoxButtons.OK, MessageBoxIcon.None);
                if (IsMultiplayer) ShowRematchDialog();
                return;
            }

            xTurn = !xTurn;
            UpdateTourLabel();
            Invalidate();
        }

        private void OnDisconnected()
        {
            if (this.IsDisposed || !this.IsHandleCreated) return;
            this.BeginInvoke(new Action(() =>
            {
                if (this.IsDisposed) return;
                glowTimer.Stop();
                // En ligne: retour direct à l'accueil, sans notification.
                try { NetworkManager.Instance.Disconnect(); } catch { }
                try
                {
                    var acc = new Accueil();
                    acc.Show();
                }
                catch { }
                this.Close();
            }));
        }

        private void OnPacketReceived(Packet p)
        {
            if (this.IsDisposed || !this.IsHandleCreated) return;
            if (this.InvokeRequired) { this.BeginInvoke(new Action<Packet>(OnPacketReceived), p); return; }

            if (p.Type == "MOVE")
            {
                string[] parts = p.Content.Split(',');
                if (parts.Length != 2) return;
                if (!int.TryParse(parts[0], out int c) || !int.TryParse(parts[1], out int r)) return;
                if (c < 0 || c >= GRID_SIZE || r < 0 || r >= GRID_SIZE) return;
                if (board[r, c] != 0) return; // case déjà prise (désync)

                // C'est le tour de l'adversaire → joueur courant (xTurn)
                int joueur = xTurn ? 1 : 2;
                board[r, c] = joueur;

                if (VerifierVictoire(r, c, joueur))
                {
                    gameOver = true;
                    if (joueur == 1) scoreX++; else scoreO++;
                    glowTimer.Start();
                    Invalidate();
                    string nom = joueur == 1 ? Joueur1 : Joueur2;
                    try { _ = NetworkManager.Instance.Lobby.RecordWinAsync(nom, "MortPion"); } catch { }
                    MessageBox.Show(nom + " gagne !", "Victoire", MessageBoxButtons.OK, MessageBoxIcon.None);
                    ShowRematchDialog();
                }
                else if (PlateauPlein())
                {
                    gameOver = true;
                    Invalidate();
                    MessageBox.Show("Match nul !", "Info", MessageBoxButtons.OK, MessageBoxIcon.None);
                    ShowRematchDialog();
                }
                else
                {
                    xTurn = !xTurn;
                    UpdateTourLabel();
                    Invalidate();
                }
            }
            else if (p.Type == "REMATCH_REQUEST")
            {
                var r = MessageBox.Show(p.Sender + " veut rejouer. Accepter ?", "Relancer la partie",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (r == DialogResult.Yes)
                    NetworkManager.Instance.SendPacket(new Packet("REMATCH_ACCEPT", NetworkManager.Instance.MyPseudo, ""));
                else
                {
                    NetworkManager.Instance.SendPacket(new Packet("REMATCH_REFUSE", NetworkManager.Instance.MyPseudo, ""));
                    NetworkManager.Instance.ReturnToLobby = true;
                    this.Close();
                }
            }
            else if (p.Type == "REMATCH_ACCEPT")
            {
                NouvellePartie();
            }
            else if (p.Type == "REMATCH_REFUSE")
            {
                NetworkManager.Instance.ReturnToLobby = true;
                this.Close();
            }
        }

        private void ShowRematchDialog()
        {
            // Seul l'hôte propose de relancer (évite que les 2 côtés demandent).
            if (IsMultiplayer && !NetworkManager.Instance.IsHost)
            {
                var wait = new Form
                {
                    Text = "Partie terminée",
                    Size = new Size(360, 150),
                    StartPosition = FormStartPosition.CenterParent,
                    FormBorderStyle = FormBorderStyle.FixedDialog,
                    BackColor = Color.FromArgb(20, 20, 32),
                    ForeColor = Color.White,
                };
                wait.Controls.Add(new Label
                {
                    Text = "En attente de la décision de l'hôte…",
                    Location = new Point(18, 18),
                    ForeColor = COL_TITLE,
                    AutoSize = true
                });
                var btnSalon = new Button
                {
                    Text = "Retour au salon",
                    Size = new Size(140, 36),
                    Location = new Point((360 - 140) / 2, 74),
                    FlatStyle = FlatStyle.Flat,
                    ForeColor = COL_X,
                };
                btnSalon.Click += (s, e) =>
                {
                    NetworkManager.Instance.ReturnToLobby = true;
                    wait.Close();
                    this.Close();
                };
                wait.Controls.Add(btnSalon);
                wait.ShowDialog(this);
                return;
            }

            var dlg = new Form
            {
                Text = "Partie terminée",
                Size = new Size(320, 140),
                StartPosition = FormStartPosition.CenterParent,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                BackColor = Color.FromArgb(20, 20, 32),
                ForeColor = Color.White,
            };
            var btnRelancer = new Button { Text = "Relancer la partie", Size = new Size(140, 36), Location = new Point(24, 64), FlatStyle = FlatStyle.Flat, ForeColor = COL_WIN_GLOW };
            var btnSalon = new Button { Text = "Retour au salon", Size = new Size(140, 36), Location = new Point(168, 64), FlatStyle = FlatStyle.Flat, ForeColor = COL_X };
            btnRelancer.Click += (s, e) =>
            {
                NetworkManager.Instance.SendPacket(new Packet("REMATCH_REQUEST", NetworkManager.Instance.MyPseudo, ""));
                dlg.Close();
            };
            btnSalon.Click += (s, e) =>
            {
                NetworkManager.Instance.SendPacket(new Packet("REMATCH_REFUSE", NetworkManager.Instance.MyPseudo, ""));
                NetworkManager.Instance.ReturnToLobby = true;
                dlg.Close();
                this.Close();
            };
            dlg.Controls.Add(btnRelancer);
            dlg.Controls.Add(btnSalon);
            dlg.Controls.Add(new Label { Text = "Que voulez-vous faire ?", Location = new Point(24, 20), ForeColor = COL_TITLE, AutoSize = true });
            dlg.ShowDialog(this);
        }


        public bool VerifierVictoire(int row, int col, int joueur)
        {
            int[] dr = { 0, 1, 1, 1 };
            int[] dc = { 1, 0, 1, -1 };

            for (int d = 0; d < 4; d++)
            {
                List<Point> cellules = new List<Point>();
                cellules.Add(new Point(col, row));

                for (int k = 1; k < WIN_LENGTH; k++)
                {
                    int nr = row + dr[d] * k;
                    int nc = col + dc[d] * k;
                    if (nr < 0 || nr >= GRID_SIZE || nc < 0 || nc >= GRID_SIZE) break;
                    if (board[nr, nc] != joueur) break;
                    cellules.Add(new Point(nc, nr));
                }
                for (int k = 1; k < WIN_LENGTH; k++)
                {
                    int nr = row - dr[d] * k;
                    int nc = col - dc[d] * k;
                    if (nr < 0 || nr >= GRID_SIZE || nc < 0 || nc >= GRID_SIZE) break;
                    if (board[nr, nc] != joueur) break;
                    cellules.Add(new Point(nc, nr));
                }

                if (cellules.Count >= WIN_LENGTH)
                {
                    winCells = cellules;
                    return true;
                }
            }
            return false;
        }

        public bool PlateauPlein()
        {
            foreach (int v in board)
                if (v == 0) return false;
            return true;
        }

        // ══════════════════════════════════════════════════════
        //  DESSIN
        // ══════════════════════════════════════════════════════
        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
            g.Clear(COL_BG);

            DessinerEntete(g);
            DessinerGrille(g);
            DessinerPieces(g);
            DessinerStatut(g);
        }

        public void DessinerEntete(Graphics g)
        {
            float centerY = 35f;
            string titre = "MORT  PION";
            SizeF szT = g.MeasureString(titre, fontTitle);
            g.DrawString(titre, fontTitle, new SolidBrush(COL_TITLE),
                (this.ClientSize.Width - szT.Width) / 2f, centerY - szT.Height / 2f);

            float scoreY = 80f;
            
            // Move text to x=140 to clearly avoid the Back button (width=100 + margin=20)
            string sx = $"{Joueur1} : {scoreX}";
            g.DrawString(sx, fontScore, new SolidBrush(COL_X), 140f, scoreY);

            string so = $"{Joueur2} : {scoreO}";
            SizeF szO = g.MeasureString(so, fontScore);
            g.DrawString(so, fontScore, new SolidBrush(COL_O),
                GridRight - szO.Width, scoreY);

            using (Pen pen = new Pen(Color.FromArgb(45, 50, 80), 1))
            {
                g.DrawLine(pen, MARGIN, HEADER - 6, GridRight, HEADER - 6);
            }
        }

        public void DessinerGrille(Graphics g)
        {
            g.FillRectangle(
                new SolidBrush(Color.FromArgb(20, 22, 38)),
                GridLeft, GridTop, GRID_SIZE * CELL, GRID_SIZE * CELL);

            if (hoverCol >= 0 && hoverRow >= 0 && !gameOver
                && board[hoverRow, hoverCol] == 0)
            {
                g.FillRectangle(new SolidBrush(COL_HOVER),
                    GridLeft + hoverCol * CELL + 1,
                    GridTop + hoverRow * CELL + 1,
                    CELL - 1, CELL - 1);
            }

            using (Pen pen = new Pen(COL_GRID, 1))
            {
                for (int i = 0; i <= GRID_SIZE; i++)
                {
                    int x = GridLeft + i * CELL;
                    int y = GridTop + i * CELL;
                    g.DrawLine(pen, x, GridTop, x, GridBottom);
                    g.DrawLine(pen, GridLeft, y, GridRight, y);
                }
            }

            using (Pen borderPen = new Pen(Color.FromArgb(70, 75, 110), 2))
            {
                g.DrawRectangle(borderPen,
                    GridLeft, GridTop, GRID_SIZE * CELL, GRID_SIZE * CELL);
            }
        }

        public void DessinerPieces(Graphics g)
        {
            int pad = 13;

            for (int r = 0; r < GRID_SIZE; r++)
            {
                for (int c = 0; c < GRID_SIZE; c++)
                {
                    if (board[r, c] == 0) continue;

                    bool estX = (board[r, c] == 1);
                    bool estWin = winCells.Contains(new Point(c, r));

                    int cx = GridLeft + c * CELL + CELL / 2;
                    int cy = GridTop + r * CELL + CELL / 2;

                    Color couleur = estX ? COL_X : COL_O;

                    if (estWin)
                    {
                        float glow = (float)(Math.Sin(glowPhase) * 0.5 + 0.5);
                        int alpha = (int)(40 + glow * 120);
                        int haloR = pad + 6 + (int)(glow * 5);
                        g.FillEllipse(
                            new SolidBrush(Color.FromArgb(alpha, COL_WIN_GLOW)),
                            cx - haloR, cy - haloR, haloR * 2, haloR * 2);
                        couleur = COL_WIN_GLOW;
                    }

                    if (estX)
                    {
                        using (Pen p = new Pen(couleur, 4.5f))
                        {
                            p.StartCap = LineCap.Round;
                            p.EndCap = LineCap.Round;
                            g.DrawLine(p, cx - pad, cy - pad, cx + pad, cy + pad);
                            g.DrawLine(p, cx + pad, cy - pad, cx - pad, cy + pad);
                        }
                    }
                    else
                    {
                        using (Pen p = new Pen(couleur, 4f))
                        {
                            g.DrawEllipse(p, cx - pad, cy - pad, pad * 2, pad * 2);
                        }
                    }
                }
            }
        }

        // Petit "À vous de jouer" en bas à gauche (pas au milieu)
        private Label _lblAVousDeJouer;

        public void DessinerStatut(Graphics g)
        {
            if (gameOver) return;
            // Statut affiché dans le label _lblAVousDeJouer, pas au centre
        }

        private void UpdateTourLabel()
        {
            if (_lblAVousDeJouer == null) return;
            if (gameOver) { _lblAVousDeJouer.Text = ""; return; }
            bool myTurn = !IsMultiplayer || (NetworkManager.Instance.IsHost ? xTurn : !xTurn);
            _lblAVousDeJouer.Text = myTurn ? "À vous de jouer" : "Attente adversaire...";
            _lblAVousDeJouer.ForeColor = xTurn ? COL_X : COL_O;
        }

        public void btnChanger_Click(object sender, EventArgs e)
        {
            new Puissance_4(Joueur1, Joueur2).Show();
        }

        public void btnChanger2_Click(object sender, EventArgs e)
        {
            new Snake(Joueur1).Show();
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            glowTimer.Stop();
            if (IsMultiplayer)
            {
                NetworkManager.Instance.OnPacketReceived -= OnPacketReceived;
                NetworkManager.Instance.OnDisconnected   -= OnDisconnected;
            }
            base.OnFormClosing(e);
        }
    }
}