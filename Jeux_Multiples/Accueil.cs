using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Jeux_Multiples
{
    // ════════════════════════════════════════════════════════════
    //  ACCUEIL — Écran principal
    // ════════════════════════════════════════════════════════════
    public partial class Accueil : Form
    {
        // ── Palette ──────────────────────────────────────────────
        private readonly Color C_BG     = Color.FromArgb(10, 10, 18);
        private readonly Color C_PANEL  = Color.FromArgb(16, 16, 26);
        private readonly Color C_CYAN   = Color.FromArgb(0, 238, 255);
        private readonly Color C_GREEN  = Color.FromArgb(0, 255, 136);
        private readonly Color C_RED    = Color.FromArgb(255, 34, 68);

        // ── Contrôles ────────────────────────────────────────────
        private TextBox   _txtPseudo;
        private List<(Button btn, bool supportsMulti)> _gameButtons;
        private Panel     _pnlLeaderboard;
        private ListBox   _listLeaderboard;
        private Timer     _leaderboardRefreshTimer;
        private UserConfig _cfg;

        public Accueil()
        {
            InitializeComponent();
            ConfigForm();
            _cfg = UserConfig.Load();
            Acceuil_Load(this, EventArgs.Empty);
        }

        private void ConfigForm()
        {
            this.Text = "Jeux Multiples - Accueil";
            
            // Full Screen Mode
            FormUtils.ApplyFullScreen(this);
            
            this.BackColor     = C_BG;
            this.ForeColor     = Color.White;
            this.DoubleBuffered = true;
        }

        private void Acceuil_Load(object sender, EventArgs e)
        {
            this.Controls.Clear();
            BuildUI();
            // Start leaderboard loading
            LoadLeaderboard();

            _leaderboardRefreshTimer?.Stop();
            _leaderboardRefreshTimer = new Timer { Interval = 30_000 };
            _leaderboardRefreshTimer.Tick += (s, ev) => LoadLeaderboard();
            _leaderboardRefreshTimer.Start();

            // Check update in background (best-effort)
            _ = Task.Run(async () =>
            {
                await Task.Delay(800);
                try { await UpdateManager.CheckAndPromptAsync(this); } catch { }
            });
        }

        private void BuildUI()
        {
            int W = this.ClientSize.Width;
            int H = this.ClientSize.Height;

            // ── Titre ────────────────────────────────────────────
            int titleY = H / 10; // 10% from top
            var lblTitle = new Label {
                Text      = "CUENCAGAMES",
                Font      = new Font("Segoe UI Black", 32, FontStyle.Bold),
                ForeColor = C_CYAN,
                AutoSize  = false,
                Size      = new Size(W, 60),
                Location  = new Point(0, titleY),
                TextAlign = ContentAlignment.MiddleCenter,
                Anchor    = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };
            this.Controls.Add(lblTitle);

            var lblSub = new Label {
                Text      = "ARCADE MULTIJOUEUR",
                Font      = new Font("Segoe UI", 12, FontStyle.Bold),
                ForeColor = Color.FromArgb(60, 60, 80),
                AutoSize  = false,
                Size      = new Size(W, 30),
                Location  = new Point(0, titleY + 60),
                TextAlign = ContentAlignment.MiddleCenter,
                Anchor    = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };
            this.Controls.Add(lblSub);

            // ── Panneau pseudo ───────────────────────────────────
            int panW = 420, panH = 56;
            int panY = titleY + 110; 
            int panX = (W - panW) / 2;
            
            var panPseudo = new Panel {
                Location  = new Point(panX, panY),
                Size      = new Size(panW, panH),
                BackColor = C_PANEL,
                Anchor    = AnchorStyles.Top
            };
            panPseudo.Paint += (s, e) =>
                ControlPaint.DrawBorder(e.Graphics, panPseudo.ClientRectangle,
                    Color.FromArgb(40, C_CYAN.R, C_CYAN.G, C_CYAN.B), ButtonBorderStyle.Solid);
            this.Controls.Add(panPseudo);

            panPseudo.Controls.Add(new Label {
                Text      = "PSEUDO",
                Location  = new Point(14, 8),
                AutoSize  = true,
                Font      = new Font("Segoe UI", 8, FontStyle.Bold),
                ForeColor = Color.FromArgb(80, 80, 100),
            });

            _txtPseudo = new TextBox {
                Text        = string.IsNullOrWhiteSpace(_cfg?.Pseudo) ? Environment.UserName : _cfg.Pseudo,
                Location    = new Point(14, 26),
                Size        = new Size(panW - 28, 22),
                Font        = new Font("Segoe UI", 11),
                BackColor   = C_PANEL,
                ForeColor   = Color.White,
                BorderStyle = BorderStyle.None,
            };
            _txtPseudo.TextChanged += (s, e) =>
            {
                // Sauvegarde simple en continu (best-effort)
                if (_cfg == null) _cfg = new UserConfig();
                _cfg.Pseudo = GetPseudo();
                UserConfig.Save(_cfg);
            };
            panPseudo.Controls.Add(_txtPseudo);

            // ── Grille de jeux ────────────────────────────────────
            
            // Calculate center vertical position for the grid
            int btnW   = 220, btnH = 80; // Slightly larger buttons
            int gapX   = 20, gapY = 20;
            int cols   = 3;
            // 6 games total -> 2 rows
            int rows = 2; 

            int gridW  = cols * btnW + (cols - 1) * gapX;
            int gridH  = rows * btnH + (rows - 1) * gapY;

            int gridX  = (W - gridW) / 2;
            // Center vertically between pseudo panel and bottom (approx)
            int availableH = H - (panY + panH); 
            int gridY  = (panY + panH) + (availableH - gridH) / 2 - 40; // Shift up a bit to leave room for quit
            
            if (gridY < panY + panH + 20) gridY = panY + panH + 20; // Minimum gap

            // supportsMulti = true → au clic on propose "Local" ou "Réseau"
            var games = new (string Label, Color Color, string Type, bool SupportsMulti, Func<string, string, bool, Form> Factory)[] {
                ("⬡  PUISSANCE 4",  C_CYAN,                         "Puissance4", true,  (j1, j2, mp) => new Puissance_4(j1, j2, mp)),
                ("♟  JEU DE DAMES", Color.FromArgb(180, 120, 255),  "Dames",      true,  (j1, j2, mp) => new Dame(j1, j2, mp)),
                ("☠  MORT PION",    C_RED,                           "MortPion",   true,  (j1, j2, mp) => new mort_Pion(j1, j2, mp)),
                ("🐍  SNAKE",        Color.FromArgb(0, 200, 80),     "Snake",      false, (j1, j2, mp) => new Snake(j1)),
                ("🃏  BLACKJACK",    Color.FromArgb(255, 200, 0),    "BlackJack",  false, (j1, j2, mp) => new BlackJack(j1, j2, false)),
                ("🎰  POKER",        Color.FromArgb(255, 130, 0),    "Poker",      false, (j1, j2, mp) => new Poker(j1, j2, false)),
            };

            _gameButtons = new List<(Button, bool)>();
            for (int i = 0; i < games.Length; i++)
            {
                int col = i % cols, row = i / cols;
                int bx  = gridX + col * (btnW + gapX);
                int by  = gridY + row * (btnH + gapY);
                var g   = games[i];

                var btn = MakeGameButton(g.Label, bx, by, btnW, btnH, g.Color);
                var type = g.Type;
                var factory = g.Factory;
                var supportsMulti = g.SupportsMulti;
                btn.Click += (s, e) => LaunchGameWithModeChoice(type, factory, supportsMulti);
                btn.Anchor = AnchorStyles.None; 
                this.Controls.Add(btn);
                _gameButtons.Add((btn, supportsMulti));
            }

            // ── Leaderboard (Floating Right) ──────────────────────
            // Only show if there is space
            if (W > 1200) 
            {
                int lbW = 250;
                // Garder le leaderboard entièrement visible (éviter bouton Quitter + marges)
                int bottomReserved = 90;
                int topReserved = 110;
                int lbH = Math.Max(220, H - topReserved - bottomReserved);
                int lbX = Math.Max(10, W - lbW - 40);
                int lbY = topReserved;
                
                BuildLeaderboardPanel(lbX, lbY, lbW, lbH);
            }
            else
            {
                // On smaller screens, maybe hide it or put it below? 
                // For now, let's just make it smaller/simpler or skip it to keep "Clean"
                // Or standardized bottom-right
                 int lbW = Math.Min(240, Math.Max(180, W / 5));
                 int lbH = Math.Min(320, Math.Max(200, H / 3));
                 int lbX = Math.Max(10, W - lbW - 20);
                 int lbY = Math.Max(10, H - lbH - 90);
                 BuildLeaderboardPanel(lbX, lbY, lbW, lbH);
            }

            // ── Bouton Quitter ────────────────────────────────────
            var btnQuit = new Button {
                Text      = "QUITTER",
                Location  = new Point((W - 140) / 2, H - 60),
                Size      = new Size(140, 40),
                FlatStyle = FlatStyle.Flat,
                ForeColor = Color.FromArgb(100, 100, 120),
                Font      = new Font("Segoe UI", 10),
                Cursor    = Cursors.Hand,
                Anchor    = AnchorStyles.Bottom
            };
            btnQuit.FlatAppearance.BorderSize = 0;
            btnQuit.Click += (s, e) => Application.Exit();
            this.Controls.Add(btnQuit);
        }

        private void BuildLeaderboardPanel(int x, int y, int w, int h)
        {
            _pnlLeaderboard = new Panel
            {
                Location = new Point(x, y),
                Size = new Size(w, h),
                BackColor = C_PANEL,
                Anchor = AnchorStyles.Right | AnchorStyles.Top | AnchorStyles.Bottom,
            };
            _pnlLeaderboard.Paint += (s, e) =>
                ControlPaint.DrawBorder(e.Graphics, _pnlLeaderboard.ClientRectangle,
                    Color.FromArgb(60, C_CYAN.R, C_CYAN.G, C_CYAN.B), ButtonBorderStyle.Solid);
            this.Controls.Add(_pnlLeaderboard);

            var lblLbTitle = new Label
            {
                Text = "🏆 CLASSEMENT",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = C_CYAN,
                Location = new Point(8, 6),
                AutoSize = true,
                Anchor = AnchorStyles.Top | AnchorStyles.Left,
            };
            _pnlLeaderboard.Controls.Add(lblLbTitle);

            _listLeaderboard = new ListBox
            {
                Location = new Point(6, 28),
                Size = new Size(w - 12, h - 34),
                BackColor = Color.FromArgb(12, 12, 22),
                ForeColor = Color.FromArgb(200, 220, 240),
                Font = new Font("Consolas", 8.5f),
                BorderStyle = BorderStyle.None,
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
            };
            _pnlLeaderboard.Controls.Add(_listLeaderboard);
        }


        private async void LoadLeaderboard()
        {
            if (_listLeaderboard == null) return;
            _listLeaderboard.Items.Clear();
            _listLeaderboard.Items.Add("Chargement...");

            try
            {
                var all = await NetworkManager.Instance.Lobby.GetLeaderboardAsync();
                _listLeaderboard.Items.Clear();

                var gameNames = new[] { ("Puissance4", "P4"), ("Dames", "Dames"), ("MortPion", "M.Pion"), ("BlackJack", "BJ"), ("Poker", "Poker"), ("Snake", "Snake") };
                foreach (var (key, name) in gameNames)
                {
                    if (!all.TryGetValue(key, out var list) || list == null || list.Count == 0) continue;
                    _listLeaderboard.Items.Add("── " + name + " ──");
                    int rank = 1;
                    foreach (var item in list.Take(5))
                    {
                        string pseudo = item.ContainsKey("pseudo") ? (item["pseudo"]?.ToString() ?? "?") : "?";
                        string valS = item.ContainsKey("value") ? (item["value"]?.ToString() ?? "0") : "0";
                        string suffix = key == "Snake" ? " pts" : " V";
                        _listLeaderboard.Items.Add(rank + ") " + pseudo + " - " + valS + suffix);
                        rank++;
                    }
                }

                if (_listLeaderboard.Items.Count == 0)
                    _listLeaderboard.Items.Add("(Aucun score)");
            }
            catch
            {
                _listLeaderboard.Items.Clear();
                _listLeaderboard.Items.Add("(Leaderboard indisponible)");
            }
        }

        // ════════════════════════════════════════════════════════
        //  LANCEMENT D'UN JEU (choix du mode par jeu : local ou réseau)
        // ════════════════════════════════════════════════════════
        private void LaunchGameWithModeChoice(string gameType, Func<string, string, bool, Form> factory, bool supportsMulti)
        {
            string pseudo = GetPseudo();
            NetworkManager.Instance.MyPseudo       = pseudo;
            NetworkManager.Instance.CurrentGameType = gameType;

            bool useNetwork = false;
            if (supportsMulti)
            {
                var dlg = new Form
                {
                    Text            = "Mode de jeu",
                    Size            = new Size(320, 120),
                    StartPosition   = FormStartPosition.CenterParent,
                    FormBorderStyle = FormBorderStyle.FixedDialog,
                    BackColor       = C_PANEL,
                    ForeColor       = Color.White,
                };
                var btnLocal = new Button
                {
                    Text = "Jouer en local",
                    Size = new Size(120, 36),
                    Location = new Point(24, 36),
                    FlatStyle = FlatStyle.Flat,
                    ForeColor = C_CYAN,
                    BackColor = Color.FromArgb(30, 30, 45),
                };
                var btnReseau = new Button
                {
                    Text = "Jouer en ligne",
                    Size = new Size(120, 36),
                    Location = new Point(160, 36),
                    FlatStyle = FlatStyle.Flat,
                    ForeColor = C_GREEN,
                    BackColor = Color.FromArgb(30, 30, 45),
                };
                btnLocal.Click += (s, e) => { dlg.DialogResult = DialogResult.OK; useNetwork = false; dlg.Close(); };
                btnReseau.Click += (s, e) => { dlg.DialogResult = DialogResult.OK; useNetwork = true; dlg.Close(); };
                dlg.Controls.Add(btnLocal);
                dlg.Controls.Add(btnReseau);
                dlg.Controls.Add(new Label { Text = "Choisir le mode", Location = new Point(24, 10), ForeColor = C_CYAN });
                if (dlg.ShowDialog(this) != DialogResult.OK) return;
            }

            if (useNetwork)
            {
                var choice = new Form
                {
                    Text            = "Multijoueur",
                    Size            = new Size(420, 170),
                    StartPosition   = FormStartPosition.CenterParent,
                    FormBorderStyle = FormBorderStyle.FixedDialog,
                    BackColor       = C_PANEL,
                    ForeColor       = Color.White,
                    AutoScaleMode   = AutoScaleMode.Dpi,
                };
                var lbl = new Label { Text = "Choisir une action", Dock = DockStyle.Top, Height = 34, TextAlign = ContentAlignment.MiddleCenter, ForeColor = C_CYAN, Font = new Font("Segoe UI", 10, FontStyle.Bold) };
                choice.Controls.Add(lbl);

                var layout = new TableLayoutPanel
                {
                    Dock = DockStyle.Fill,
                    ColumnCount = 2,
                    RowCount = 1,
                    Padding = new Padding(18, 14, 18, 18),
                };
                layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
                layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
                choice.Controls.Add(layout);

                var btnCreate = new Button { Text = "Créer une partie", Dock = DockStyle.Fill, Height = 44, FlatStyle = FlatStyle.Flat, ForeColor = C_GREEN, BackColor = Color.FromArgb(30, 30, 45) };
                var btnJoin   = new Button { Text = "Rejoindre une partie", Dock = DockStyle.Fill, Height = 44, FlatStyle = FlatStyle.Flat, ForeColor = C_CYAN, BackColor = Color.FromArgb(30, 30, 45) };
                layout.Controls.Add(btnCreate, 0, 0);
                layout.Controls.Add(btnJoin,   1, 0);
                bool create = false;
                btnCreate.Click += (s, e) => { create = true;  choice.DialogResult = DialogResult.OK; choice.Close(); };
                btnJoin.Click   += (s, e) => { create = false; choice.DialogResult = DialogResult.OK; choice.Close(); };
                if (choice.ShowDialog(this) != DialogResult.OK) return;

                try
                {
                    if (create)
                    {
                        NetworkManager.Instance.HostSalon(GetPseudo(), gameType, 2);
                        using (var waitForm = new WaitingForPlayerForm(gameType))
                        {
                            if (waitForm.ShowDialog(this) != DialogResult.OK) return;
                        }
                    }
                    else
                    {
                        using (var listForm = new SalonListForm())
                        {
                            if (listForm.ShowDialog(this) != DialogResult.OK) return;
                        }
                    }
                    string j1 = NetworkManager.Instance.IsHost ? NetworkManager.Instance.MyPseudo : NetworkManager.Instance.OpponentPseudo;
                    string j2 = NetworkManager.Instance.IsHost ? NetworkManager.Instance.OpponentPseudo : NetworkManager.Instance.MyPseudo;
                    Form jeu = CreateGameFormForCurrentType(j1, j2);
                    if (jeu == null) { MessageBox.Show("Type de jeu inconnu.", "Erreur"); return; }
                    jeu.FormClosed += GameFormClosed;
                    jeu.Show();
                    this.Hide();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Erreur lancement : " + ex.Message, "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                try
                {
                    Form jeu = factory(pseudo, "Invité", false);
                    jeu.FormClosed += (s, e) => this.Show();
                    jeu.Show();
                    this.Hide();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Erreur lancement : " + ex.Message);
                }
            }
        }

        private void GameFormClosed(object sender, FormClosedEventArgs e)
        {
            if (!NetworkManager.Instance.ReturnToLobby)
                NetworkManager.Instance.Disconnect();
            else
                NetworkManager.Instance.ReturnToLobby = false;
            this.Show();
            LoadLeaderboard();
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            _leaderboardRefreshTimer?.Stop();
            _leaderboardRefreshTimer?.Dispose();
            _leaderboardRefreshTimer = null;
            base.OnFormClosed(e);
        }

        // ── Plus de checkbox global : le mode est choisi par jeu au clic ──

        // ════════════════════════════════════════════════════════
        //  HELPERS
        // ════════════════════════════════════════════════════════
        private string GetPseudo() =>
            string.IsNullOrWhiteSpace(_txtPseudo?.Text) ? "Joueur" : _txtPseudo.Text.Trim();

        /// <summary>Crée le formulaire de jeu correspondant à CurrentGameType (pour host ou join).</summary>
        private Form CreateGameFormForCurrentType(string j1, string j2)
        {
            string gt = NetworkManager.Instance.CurrentGameType ?? "any";
            switch (gt)
            {
                case "Puissance4":  return new Puissance_4(j1, j2, true);
                case "Dames":      return new Dame(j1, j2, true);
                case "MortPion":   return new mort_Pion(j1, j2, true);
                case "BlackJack":  return new BlackJack(j1, j2, true);
                case "Poker":      return new Poker(j1, j2, true);
                default:           return null;
            }
        }

        private Button MakeGameButton(string text, int x, int y, int w, int h, Color accent)
        {
            var btn = new Button {
                Text      = text,
                Location  = new Point(x, y),
                Size      = new Size(w, h),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(16, 16, 26),
                ForeColor = accent,
                Font      = new Font("Segoe UI Black", 11, FontStyle.Bold),
                Cursor    = Cursors.Hand,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding   = new Padding(14, 0, 0, 0),
            };
            btn.FlatAppearance.BorderColor = Color.FromArgb(35, accent.R, accent.G, accent.B);
            btn.FlatAppearance.BorderSize  = 1;
            btn.FlatAppearance.MouseOverBackColor = Color.FromArgb(22, accent.R, accent.G, accent.B);
            btn.MouseEnter += (s, e) => btn.FlatAppearance.BorderColor = accent;
            btn.MouseLeave += (s, e) => btn.FlatAppearance.BorderColor = Color.FromArgb(35, accent.R, accent.G, accent.B);
            return btn;
        }

        // ── Stubs designer (ne pas toucher) ──────────────────────
        private void btnPuissance4_Click(object sender, EventArgs e) { }
        private void btnMortPion_Click(object sender, EventArgs e) { }
        private void btnSnake_Click(object sender, EventArgs e) { }
        private void quitterToolStripMenuItem_Click(object sender, EventArgs e) { }
        private void btnPker_Click(object sender, EventArgs e) { }
        private void btnBJ_Click(object sender, EventArgs e) { }
        private void button1_Click(object sender, EventArgs e) { }
        private void label1_Click(object sender, EventArgs e) { }
        private void btnHost_Click(object sender, EventArgs e) { }
        private void btnJoin_Click(object sender, EventArgs e) { }
        private void txtPseudo_GotFocus(object sender, EventArgs e) { }
        private void txtPseudo_LostFocus(object sender, EventArgs e) { }
        private void txtIP_GotFocus(object sender, EventArgs e) { }
        private void txtIP_LostFocus(object sender, EventArgs e) { }
    }
}