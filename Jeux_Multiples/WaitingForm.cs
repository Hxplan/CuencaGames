using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Jeux_Multiples
{
    // ════════════════════════════════════════════════════════════
    //  FORMULAIRE DE LOBBY — Recherche & Création de salon
    // ════════════════════════════════════════════════════════════
    public class WaitingForm : Form
    {
        // ── Couleurs ─────────────────────────────────────────────
        private readonly Color C_BG      = Color.FromArgb(10, 10, 18);
        private readonly Color C_PANEL   = Color.FromArgb(16, 16, 26);
        private readonly Color C_BORDER  = Color.FromArgb(30, 30, 50);
        private readonly Color C_CYAN    = Color.FromArgb(0, 238, 255);
        private readonly Color C_GREEN   = Color.FromArgb(0, 255, 136);
        private readonly Color C_RED     = Color.FromArgb(255, 50, 80);
        private readonly Color C_YELLOW  = Color.FromArgb(255, 200, 0);
        private readonly Color C_GRAY    = Color.FromArgb(100, 100, 120);

        // ── Contrôles principaux ─────────────────────────────────
        private Label     _lblTitle;
        private Label     _lblMyInfo;
        private Label     _lblCount;
        private ListView  _listView;
        private ComboBox  _cboFilter;
        private Button    _btnRefresh;
        private Button    _btnCreate;
        private Button    _btnJoinDirect;
        private Button    _btnCancel;
        private Label     _lblStatus;
        private Timer     _animTimer;
        private Timer     _autoRefreshTimer;

        // ── État ─────────────────────────────────────────────────
        private string  _myPublicIp;
        private int     _dotCount;
        private bool    _connecting;

        private static readonly string[] GAME_TYPES = {
            "Tous", "Puissance4", "Dames", "MortPion", "Snake", "BlackJack", "Poker"
        };

        // ════════════════════════════════════════════════════════
        public WaitingForm()
        {
            BuildUI();
            WireEvents();
            FormUtils.ApplyFullScreen(this);
        }

        // ════════════════════════════════════════════════════════
        //  CONSTRUCTION DE L'INTERFACE
        // ════════════════════════════════════════════════════════
        private void BuildUI()
        {
            this.Text             = "CuencaGames — Lobby";
            this.BackColor        = C_BG;
            this.ForeColor        = Color.White;
            this.DoubleBuffered   = true;

            // ── Titre ────────────────────────────────────────────
            _lblTitle = MakeLabel("⚡ LOBBY MULTIJOUEUR", 0, 0, new Font("Segoe UI Black", 18, FontStyle.Bold), C_CYAN);
            _lblTitle.AutoSize    = false;
            _lblTitle.Dock        = DockStyle.Top;
            _lblTitle.Height      = 52;
            _lblTitle.TextAlign   = ContentAlignment.MiddleCenter;
            this.Controls.Add(_lblTitle);

            // ── Infos ────────────────────────────────────────────
            _lblMyInfo = MakeLabel("", 12, 56, null, C_GRAY);
            _lblMyInfo.AutoSize = false;
            _lblMyInfo.Size     = new Size(400, 20);
            _lblMyInfo.Anchor   = AnchorStyles.Top | AnchorStyles.Left;
            this.Controls.Add(_lblMyInfo);

            _lblCount = MakeLabel("", 0, 56, null, C_GREEN);
            _lblCount.AutoSize   = false;
            _lblCount.Size       = new Size(this.Width - 24, 20);
            _lblCount.TextAlign  = ContentAlignment.MiddleRight;
            _lblCount.Anchor     = AnchorStyles.Top | AnchorStyles.Right | AnchorStyles.Left;
            this.Controls.Add(_lblCount);

            // ── Barre de filtres ─────────────────────────────────
            var filterPanel = new Panel {
                Location  = new Point(12, 82),
                Size      = new Size(this.Width - 24, 32),
                BackColor = C_PANEL,
                Anchor    = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };
            this.Controls.Add(filterPanel);

            MakeLabel("Jeu :", 8, 7, null, C_GRAY).Parent = filterPanel;

            _cboFilter = new ComboBox {
                Location      = new Point(45, 4),
                Size          = new Size(160, 24),
                DropDownStyle = ComboBoxStyle.DropDownList,
                BackColor     = Color.FromArgb(22, 22, 35),
                ForeColor     = Color.White,
                FlatStyle     = FlatStyle.Flat,
            };
            _cboFilter.Items.AddRange(GAME_TYPES);
            _cboFilter.SelectedIndex = 0;
            filterPanel.Controls.Add(_cboFilter);

            _btnRefresh = MakeButton("↻ Actualiser", 220, 3, 110, 26, C_CYAN);
            filterPanel.Controls.Add(_btnRefresh);

            // ── ListView des salons ───────────────────────────────
            _listView = new ListView {
                Location      = new Point(12, 122),
                Size          = new Size(this.Width - 24, this.Height - 122 - 70), // Initial size approximation
                BackColor     = C_PANEL,
                ForeColor     = Color.White,
                BorderStyle   = BorderStyle.FixedSingle,
                FullRowSelect = true,
                GridLines     = false,
                View          = View.Details,
                Font          = new Font("Segoe UI", 9.5f),
                Anchor        = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom,
                HideSelection = false,
            };
            _listView.Columns.Add("Salon",   180);
            _listView.Columns.Add("Hôte",    110);
            _listView.Columns.Add("Jeu",     110);
            _listView.Columns.Add("Joueurs",  70);
            _listView.Columns.Add("IP",      130);
            this.Controls.Add(_listView);

            // ── Barre de statut ───────────────────────────────────
            _lblStatus = MakeLabel("Chargement…", 12, 0, new Font("Segoe UI", 9), C_GRAY);
            _lblStatus.AutoSize  = false;
            _lblStatus.Size      = new Size(this.Width - 24, 22);
            _lblStatus.Anchor    = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            _lblStatus.Location  = new Point(12, this.ClientSize.Height - 90);
            this.Controls.Add(_lblStatus);

            // ── Boutons d'action ──────────────────────────────────
            int btnY = this.ClientSize.Height - 62;

            _btnCreate     = MakeButton("🟢 CRÉER UN SALON",   12,             btnY, 180, 44, C_GREEN);
            _btnJoinDirect = MakeButton("⚡ REJOINDRE IP:PORT", 204,            btnY, 180, 44, C_YELLOW);
            _btnCancel     = MakeButton("✖ ANNULER",            this.Width-204, btnY, 180, 44, C_RED);

            _btnCreate.Anchor     = AnchorStyles.Bottom | AnchorStyles.Left;
            _btnJoinDirect.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            _btnCancel.Anchor     = AnchorStyles.Bottom | AnchorStyles.Right;

            this.Controls.Add(_btnCreate);
            this.Controls.Add(_btnJoinDirect);
            this.Controls.Add(_btnCancel);

            // ── Timers ────────────────────────────────────────────
            _animTimer = new Timer { Interval = 500 };
            _animTimer.Tick += (s, e) => {
                _dotCount = (_dotCount + 1) % 4;
                if (!_connecting) return;
                _lblStatus.Text = "Connexion en cours" + new string('.', _dotCount);
            };
            _animTimer.Start();

            _autoRefreshTimer = new Timer { Interval = 15_000 };
            _autoRefreshTimer.Tick += async (s, e) => await RefreshListAsync();
        }

        // ════════════════════════════════════════════════════════
        //  CÂBLAGE DES ÉVÉNEMENTS
        // ════════════════════════════════════════════════════════
        private void WireEvents()
        {
            this.Load += async (s, e) => {
                NetworkManager.Instance.OnConnected    += HandleConnected;
                NetworkManager.Instance.OnDisconnected += HandleDisconnected;

                _myPublicIp = await NetworkManager.Instance.Lobby.GetPublicIPAsync();
                _lblMyInfo.Text = $"IP locale : {NetworkManager.Instance.MyLocalIP}   |   IP publique : {_myPublicIp ?? "?"}";

                await RefreshListAsync();
                _autoRefreshTimer.Start();
            };

            this.FormClosing += (s, e) => {
                _autoRefreshTimer.Stop();
                _animTimer.Stop();
                NetworkManager.Instance.OnConnected    -= HandleConnected;
                NetworkManager.Instance.OnDisconnected -= HandleDisconnected;
                if (this.DialogResult != DialogResult.OK)
                    NetworkManager.Instance.Disconnect();
            };

            this.Resize += (s, e) => {
                // Adjust ListView to fill available space
                int topSpace = 122; // Title + Info + Filter
                int bottomSpace = 100; // Status + Buttons
                if (this.ClientSize.Height > topSpace + bottomSpace)
                {
                    _listView.Height = this.ClientSize.Height - topSpace - bottomSpace;
                }
                
                // Adjust status and buttons Y position
                _lblStatus.Location = new Point(12, this.ClientSize.Height - 90);
                int btnY = this.ClientSize.Height - 62;
                _btnCreate.Location = new Point(12, btnY);
                _btnJoinDirect.Location = new Point(204, btnY);
                _btnCancel.Location = new Point(this.ClientSize.Width - 204, btnY);
            };

            _btnRefresh.Click  += async (s, e) => await RefreshListAsync();
            _cboFilter.SelectedIndexChanged += async (s, e) => await RefreshListAsync();
            _listView.DoubleClick += (s, e) => JoinSelectedServer();
            _listView.KeyDown     += (s, e) => { if (e.KeyCode == Keys.Return) JoinSelectedServer(); };
            _btnCreate.Click      += BtnCreate_Click;
            _btnJoinDirect.Click  += BtnJoinDirect_Click;
            _btnCancel.Click      += (s, e) => { this.DialogResult = DialogResult.Cancel; this.Close(); };
        }

        // ════════════════════════════════════════════════════════
        //  ACTUALISATION DE LA LISTE
        // ════════════════════════════════════════════════════════
        private async Task RefreshListAsync()
        {
            if (InvokeRequired) { Invoke(new Action(async () => await RefreshListAsync())); return; }

            SetStatus("Actualisation…", C_GRAY);
            _btnRefresh.Enabled = false;

            try
            {
                string filter  = _cboFilter.SelectedItem?.ToString() == "Tous" ? null : _cboFilter.SelectedItem?.ToString();
                var    servers = await NetworkManager.Instance.Lobby.GetServersAsync(filter);

                _listView.Items.Clear();

                foreach (var srv in servers)
                {
                    // Sur le même réseau (même IP publique) → préférer IP locale
                    bool   sameNet  = !string.IsNullOrEmpty(_myPublicIp) && srv.ip_address == _myPublicIp;
                    string displayIp = sameNet && !string.IsNullOrEmpty(srv.local_ip)
                                     ? srv.local_ip + " (LAN)"
                                     : srv.ip_address;

                    var item = new ListViewItem(srv.server_name) { Tag = srv };
                    item.SubItems.Add(srv.host_pseudo ?? "?");
                    item.SubItems.Add(srv.game_type   ?? "any");
                    item.SubItems.Add($"{srv.current_players}/{srv.max_players}");
                    item.SubItems.Add(displayIp);

                    if (srv.IsFull)
                        item.ForeColor = C_GRAY;
                    else if (sameNet)
                        item.ForeColor = C_GREEN; // LAN → vert
                    else
                        item.ForeColor = Color.White;

                    _listView.Items.Add(item);
                }

                int n = servers.Count;
                _lblCount.Text  = n > 0 ? $"{n} salon(s) en ligne" : "Aucun salon actif";
                SetStatus(n > 0 ? "Double-clic pour rejoindre un salon." : "Aucun salon — créez le vôtre !", C_GRAY);
            }
            catch (Exception ex) { SetStatus("Erreur : " + ex.Message, C_RED); }
            finally { _btnRefresh.Enabled = true; }
        }

        // ════════════════════════════════════════════════════════
        //  REJOINDRE LE SALON SÉLECTIONNÉ
        // ════════════════════════════════════════════════════════
        private void JoinSelectedServer()
        {
            if (_listView.SelectedItems.Count == 0) return;
            var srv = _listView.SelectedItems[0].Tag as ServerInfo;
            if (srv == null) return;

            if (srv.IsFull)
            {
                SetStatus("Ce salon est plein !", C_RED);
                return;
            }

            // Choisir IP LAN si même réseau
            bool   sameNet = !string.IsNullOrEmpty(_myPublicIp) && srv.ip_address == _myPublicIp;
            string ip      = sameNet && !string.IsNullOrEmpty(srv.local_ip) ? srv.local_ip : srv.ip_address;
            string address = $"{ip}:{srv.port}";

            SetStatus($"Connexion à {srv.server_name} ({address})…", C_CYAN);
            _connecting = true;
            _autoRefreshTimer.Stop();
            NetworkManager.Instance.ConnectDirectly(address);
        }

        // ════════════════════════════════════════════════════════
        //  CRÉER UN SALON
        // ════════════════════════════════════════════════════════
        private async void BtnCreate_Click(object sender, EventArgs e)
        {
            using (var dlg = new CreateSalonDialog(NetworkManager.Instance.MyPseudo, GAME_TYPES))
            {
                if (dlg.ShowDialog(this) != DialogResult.OK) return;

                _btnCreate.Enabled = false;
                SetStatus($"Hébergement de '{dlg.SalonName}' [{dlg.GameType}]…", C_GREEN);

                NetworkManager.Instance.CurrentGameType = dlg.GameType;
                NetworkManager.Instance.HostSalon(dlg.SalonName, dlg.GameType, dlg.MaxPlayers);

                await Task.Delay(600); // Laisser le temps à l'enregistrement web
                await RefreshListAsync();
                _autoRefreshTimer.Start();
                _btnCreate.Enabled = true;
            }
        }

        // ════════════════════════════════════════════════════════
        //  REJOINDRE PAR IP DIRECTE
        // ════════════════════════════════════════════════════════
        private void BtnJoinDirect_Click(object sender, EventArgs e)
        {
            string ip = Microsoft.VisualBasic.Interaction.InputBox(
                "Entrez l'adresse IP:PORT\n(ex: 1.2.3.4:8080 ou 192.168.1.5:8080)",
                "Connexion directe", "");
            if (string.IsNullOrWhiteSpace(ip)) return;

            SetStatus($"Connexion à {ip}…", C_CYAN);
            _connecting = true;
            _autoRefreshTimer.Stop();
            NetworkManager.Instance.ConnectDirectly(ip.Trim());
        }

        // ════════════════════════════════════════════════════════
        //  CALLBACKS RÉSEAU
        // ════════════════════════════════════════════════════════
        private void HandleConnected()
        {
            if (InvokeRequired) { Invoke(new Action(HandleConnected)); return; }

            _connecting = false;
            _animTimer.Stop();
            SetStatus("✅ CONNECTÉ !", C_GREEN);
            _lblTitle.ForeColor = C_GREEN;

            var t = new Timer { Interval = 800 };
            t.Tick += (s, e2) => {
                t.Stop();
                this.DialogResult = DialogResult.OK;
                this.Close();
            };
            t.Start();
        }

        private void HandleDisconnected()
        {
            if (InvokeRequired) { Invoke(new Action(HandleDisconnected)); return; }
            if (_connecting)
            {
                _connecting = false;
                SetStatus("Connexion échouée. Réessayez.", C_RED);
                _autoRefreshTimer.Start();
            }
        }

        // ════════════════════════════════════════════════════════
        //  HELPERS UI
        // ════════════════════════════════════════════════════════
        private void SetStatus(string msg, Color color)
        {
            _lblStatus.Text      = msg;
            _lblStatus.ForeColor = color;
        }

        private Label MakeLabel(string text, int x, int y, Font font = null, Color? color = null)
        {
            return new Label {
                Text      = text,
                Location  = new Point(x, y),
                AutoSize  = true,
                Font      = font ?? new Font("Segoe UI", 9),
                ForeColor = color ?? Color.White,
            };
        }

        private Button MakeButton(string text, int x, int y, int w, int h, Color accent)
        {
            var btn = new Button {
                Text      = text,
                Location  = new Point(x, y),
                Size      = new Size(w, h),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(22, 22, 35),
                ForeColor = accent,
                Font      = new Font("Segoe UI", 9, FontStyle.Bold),
                Cursor    = Cursors.Hand,
            };
            btn.FlatAppearance.BorderColor = Color.FromArgb(60, accent.R, accent.G, accent.B);
            btn.FlatAppearance.BorderSize  = 1;
            btn.FlatAppearance.MouseOverBackColor = Color.FromArgb(30, accent.R, accent.G, accent.B);
            return btn;
        }
    }

    // ════════════════════════════════════════════════════════════
    //  DIALOGUE DE CRÉATION DE SALON
    // ════════════════════════════════════════════════════════════
    public class CreateSalonDialog : Form
    {
        public string SalonName  { get; private set; }
        public string GameType   { get; private set; }
        public int    MaxPlayers { get; private set; }

        private TextBox  _txtName;
        private ComboBox _cboGame;
        private NumericUpDown _numPlayers;

        public CreateSalonDialog(string defaultName, string[] gameTypes)
        {
            this.Text             = "Créer un salon";
            this.Size             = new Size(380, 260);
            this.StartPosition    = FormStartPosition.CenterParent;
            this.FormBorderStyle  = FormBorderStyle.FixedDialog;
            this.MaximizeBox      = false;
            this.MinimizeBox      = false;
            this.BackColor        = Color.FromArgb(14, 14, 22);
            this.ForeColor        = Color.White;

            int y = 20;
            AddLabel("Nom du salon :", 20, y);
            _txtName = new TextBox {
                Text      = defaultName + "'s game",
                Location  = new Point(20, y + 22),
                Size      = new Size(330, 24),
                BackColor = Color.FromArgb(22, 22, 35),
                ForeColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
            };
            this.Controls.Add(_txtName);

            y += 62;
            AddLabel("Type de jeu :", 20, y);
            _cboGame = new ComboBox {
                Location      = new Point(20, y + 22),
                Size          = new Size(330, 24),
                DropDownStyle = ComboBoxStyle.DropDownList,
                BackColor     = Color.FromArgb(22, 22, 35),
                ForeColor     = Color.White,
                FlatStyle     = FlatStyle.Flat,
            };
            // Exclure "Tous" de la liste
            foreach (var g in gameTypes) if (g != "Tous") _cboGame.Items.Add(g);
            _cboGame.SelectedIndex = 0;
            this.Controls.Add(_cboGame);

            y += 62;
            AddLabel("Joueurs max :", 20, y);
            _numPlayers = new NumericUpDown {
                Location  = new Point(20, y + 22),
                Size      = new Size(80, 24),
                Minimum   = 2, Maximum = 8, Value = 2,
                BackColor = Color.FromArgb(22, 22, 35),
                ForeColor = Color.White,
            };
            this.Controls.Add(_numPlayers);

            var btnOk = new Button {
                Text      = "CRÉER",
                Location  = new Point(200, y + 18),
                Size      = new Size(150, 34),
                FlatStyle = FlatStyle.Flat,
                ForeColor = Color.FromArgb(0, 255, 136),
                BackColor = Color.FromArgb(20, 20, 35),
                Font      = new Font("Segoe UI", 10, FontStyle.Bold),
                Cursor    = Cursors.Hand,
            };
            btnOk.FlatAppearance.BorderColor = Color.FromArgb(0, 255, 136);
            btnOk.Click += (s, e) => {
                SalonName  = _txtName.Text.Trim();
                GameType   = _cboGame.SelectedItem?.ToString() ?? "any";
                MaxPlayers = (int)_numPlayers.Value;
                if (string.IsNullOrEmpty(SalonName)) { MessageBox.Show("Entrez un nom."); return; }
                this.DialogResult = DialogResult.OK;
                this.Close();
            };
            this.Controls.Add(btnOk);
        }

        private void AddLabel(string text, int x, int y)
        {
            this.Controls.Add(new Label {
                Text      = text,
                Location  = new Point(x, y),
                AutoSize  = true,
                ForeColor = Color.FromArgb(0, 238, 255),
                Font      = new Font("Segoe UI", 9, FontStyle.Bold),
            });
        }
    }

    // ════════════════════════════════════════════════════════════
    //  EN ATTENTE D'UN JOUEUR (hôte)
    // ════════════════════════════════════════════════════════════
    public class WaitingForPlayerForm : Form
    {
        private readonly Color C_BG   = Color.FromArgb(10, 10, 18);
        private readonly Color C_CYAN = Color.FromArgb(0, 238, 255);
        private readonly Color C_RED  = Color.FromArgb(255, 50, 80);
        private Label _lblStatus;
        private Button _btnCancel;

        public WaitingForPlayerForm(string gameType)
        {
            this.Text             = "En attente…";
            this.Size             = new Size(400, 160);
            this.StartPosition    = FormStartPosition.CenterParent;
            this.FormBorderStyle  = FormBorderStyle.FixedDialog;
            this.BackColor        = C_BG;
            this.ForeColor        = Color.White;

            _lblStatus = new Label {
                Text      = $"Partie {gameType} — En attente d'un joueur…",
                Font      = new Font("Segoe UI", 11),
                ForeColor = C_CYAN,
                AutoSize  = false,
                Size      = new Size(360, 50),
                Location  = new Point(20, 24),
                TextAlign = ContentAlignment.MiddleCenter,
            };
            this.Controls.Add(_lblStatus);

            _btnCancel = new Button {
                Text      = "Annuler",
                Size      = new Size(120, 36),
                Location  = new Point((400 - 120) / 2, 80),
                FlatStyle = FlatStyle.Flat,
                ForeColor = C_RED,
                BackColor = Color.FromArgb(30, 30, 45),
                Cursor    = Cursors.Hand,
            };
            _btnCancel.FlatAppearance.BorderColor = C_RED;
            _btnCancel.Click += (s, e) => {
                NetworkManager.Instance.Disconnect();
                this.DialogResult = DialogResult.Cancel;
                this.Close();
            };
            this.Controls.Add(_btnCancel);

            this.Load += (s, e) => {
                NetworkManager.Instance.OnConnected += OnConnected;
            };
            this.FormClosing += (s, e) => {
                NetworkManager.Instance.OnConnected -= OnConnected;
            };
        }

        private void OnConnected()
        {
            if (InvokeRequired) { Invoke(new Action(OnConnected)); return; }
            this.DialogResult = DialogResult.OK;
            this.Close();
        }
    }

    // ════════════════════════════════════════════════════════════
    //  LISTE DES SALONS (rejoindre uniquement)
    // ════════════════════════════════════════════════════════════
    public class SalonListForm : Form
    {
        private readonly Color C_BG     = Color.FromArgb(10, 10, 18);
        private readonly Color C_PANEL  = Color.FromArgb(16, 16, 26);
        private readonly Color C_CYAN   = Color.FromArgb(0, 238, 255);
        private readonly Color C_GREEN   = Color.FromArgb(0, 255, 136);
        private readonly Color C_RED     = Color.FromArgb(255, 50, 80);
        private readonly Color C_GRAY    = Color.FromArgb(100, 100, 120);

        private ListView _listView;
        private ComboBox _cboFilter;
        private Button _btnRefresh, _btnJoin, _btnCancel;
        private Label _lblStatus, _lblCount;
        private Timer _autoRefreshTimer;
        private string _myPublicIp;
        private bool _connecting;

        private static readonly string[] GAME_TYPES = { "Tous", "Puissance4", "Dames", "MortPion", "Snake", "BlackJack", "Poker" };

        public SalonListForm()
        {
            this.Text             = "Rejoindre une partie";
            this.Size             = new Size(640, 440);
            this.StartPosition    = FormStartPosition.CenterParent;
            this.BackColor        = C_BG;
            this.ForeColor        = Color.White;

            var lblTitle = new Label {
                Text = "Liste des salons",
                Font = new Font("Segoe UI Black", 14, FontStyle.Bold),
                ForeColor = C_CYAN,
                Location = new Point(12, 10),
                AutoSize = true,
            };
            this.Controls.Add(lblTitle);

            _cboFilter = new ComboBox {
                Location = new Point(12, 38),
                Size = new Size(140, 24),
                DropDownStyle = ComboBoxStyle.DropDownList,
                BackColor = Color.FromArgb(22, 22, 35),
                ForeColor = Color.White,
            };
            _cboFilter.Items.AddRange(GAME_TYPES);
            _cboFilter.SelectedIndex = 0;
            this.Controls.Add(_cboFilter);

            _btnRefresh = new Button {
                Text = "Actualiser",
                Location = new Point(160, 36),
                Size = new Size(90, 26),
                FlatStyle = FlatStyle.Flat,
                ForeColor = C_CYAN,
                BackColor = Color.FromArgb(22, 22, 35),
            };
            this.Controls.Add(_btnRefresh);

            _listView = new ListView {
                Location = new Point(12, 72),
                Size = new Size(this.ClientSize.Width - 24, 260),
                BackColor = C_PANEL,
                ForeColor = Color.White,
                FullRowSelect = true,
                View = View.Details,
                Font = new Font("Segoe UI", 9f),
            };
            _listView.Columns.Add("Salon", 140);
            _listView.Columns.Add("Hôte", 100);
            _listView.Columns.Add("Jeu", 100);
            _listView.Columns.Add("Joueurs", 60);
            _listView.Columns.Add("IP", 120);
            this.Controls.Add(_listView);

            _lblCount = new Label { Location = new Point(12, 338), AutoSize = true, ForeColor = C_GRAY };
            this.Controls.Add(_lblCount);

            _lblStatus = new Label {
                Location = new Point(12, 358),
                Size = new Size(400, 20),
                ForeColor = C_GRAY,
            };
            this.Controls.Add(_lblStatus);

            int btnY = 384;
            _btnJoin = new Button {
                Text = "Rejoindre",
                Location = new Point(12, btnY),
                Size = new Size(120, 36),
                FlatStyle = FlatStyle.Flat,
                ForeColor = C_GREEN,
                BackColor = Color.FromArgb(22, 22, 35),
            };
            _btnCancel = new Button {
                Text = "Annuler",
                Location = new Point(this.ClientSize.Width - 132, btnY),
                Size = new Size(120, 36),
                FlatStyle = FlatStyle.Flat,
                ForeColor = C_RED,
                BackColor = Color.FromArgb(22, 22, 35),
            };
            _btnJoin.Anchor = AnchorStyles.Left | AnchorStyles.Bottom;
            _btnCancel.Anchor = AnchorStyles.Right | AnchorStyles.Bottom;
            this.Controls.Add(_btnJoin);
            this.Controls.Add(_btnCancel);

            _autoRefreshTimer = new Timer { Interval = 15_000 };

            this.Load += async (s, e) => {
                NetworkManager.Instance.OnConnected += HandleConnected;
                NetworkManager.Instance.OnDisconnected += HandleDisconnected;
                _myPublicIp = await NetworkManager.Instance.Lobby.GetPublicIPAsync();
                await RefreshListAsync();
                _autoRefreshTimer.Tick += async (ss, ee) => await RefreshListAsync();
                _autoRefreshTimer.Start();
            };
            this.FormClosing += (s, e) => {
                _autoRefreshTimer.Stop();
                NetworkManager.Instance.OnConnected -= HandleConnected;
                NetworkManager.Instance.OnDisconnected -= HandleDisconnected;
                if (this.DialogResult != DialogResult.OK)
                    NetworkManager.Instance.Disconnect();
            };

            _btnRefresh.Click += async (s, e) => await RefreshListAsync();
            _cboFilter.SelectedIndexChanged += async (s, e) => await RefreshListAsync();
            _listView.DoubleClick += (s, e) => JoinSelected();
            _listView.KeyDown += (s, e) => { if (e.KeyCode == Keys.Return) JoinSelected(); };
            _btnJoin.Click += (s, e) => JoinSelected();
            _btnCancel.Click += (s, e) => { this.DialogResult = DialogResult.Cancel; this.Close(); };

            // Layout responsive: éviter que les boutons sortent de l'écran
            this.Resize += (s, e) =>
            {
                int bottomPadding = 16;
                int buttonsH = 36;
                int btnY2 = this.ClientSize.Height - buttonsH - bottomPadding;
                _btnJoin.Location = new Point(12, btnY2);
                _btnCancel.Location = new Point(this.ClientSize.Width - 12 - _btnCancel.Width, btnY2);

                _lblStatus.Location = new Point(12, btnY2 - 26);
                _lblStatus.Width = Math.Max(200, this.ClientSize.Width - 24);

                int listTop = 72;
                int listBottom = (btnY2 - 10) - listTop;
                _listView.Size = new Size(this.ClientSize.Width - 24, Math.Max(160, listBottom));
            };
            // Force first layout
            this.PerformLayout();
        }

        private async Task RefreshListAsync()
        {
            if (InvokeRequired) { Invoke(new Action(async () => await RefreshListAsync())); return; }
            _btnRefresh.Enabled = false;
            try
            {
                string filter = _cboFilter.SelectedItem?.ToString() == "Tous" ? null : _cboFilter.SelectedItem?.ToString();
                var servers = await NetworkManager.Instance.Lobby.GetServersAsync(filter);
                _listView.Items.Clear();
                foreach (var srv in servers)
                {
                    bool sameNet = !string.IsNullOrEmpty(_myPublicIp) && srv.ip_address == _myPublicIp;
                    string displayIp = sameNet && !string.IsNullOrEmpty(srv.local_ip) ? srv.local_ip + " (LAN)" : srv.ip_address;
                    var item = new ListViewItem(srv.server_name ?? "?") { Tag = srv };
                    item.SubItems.Add(srv.host_pseudo ?? "?");
                    item.SubItems.Add(srv.game_type ?? "?");
                    item.SubItems.Add($"{srv.current_players}/{srv.max_players}");
                    item.SubItems.Add(displayIp);
                    if (!srv.IsFull) item.ForeColor = sameNet ? C_GREEN : Color.White;
                    else item.ForeColor = C_GRAY;
                    _listView.Items.Add(item);
                }
                _lblCount.Text = _listView.Items.Count > 0 ? $"{_listView.Items.Count} salon(s)" : "Aucun salon";
                _lblStatus.Text = "Double-clic ou Rejoindre pour entrer.";
            }
            catch (Exception ex) { _lblStatus.Text = "Erreur : " + ex.Message; }
            finally { _btnRefresh.Enabled = true; }
        }

        private void JoinSelected()
        {
            if (_listView.SelectedItems.Count == 0) return;
            var srv = _listView.SelectedItems[0].Tag as ServerInfo;
            if (srv == null || srv.IsFull) return;

            NetworkManager.Instance.CurrentGameType = srv.game_type ?? "any";
            bool sameNet = !string.IsNullOrEmpty(_myPublicIp) && srv.ip_address == _myPublicIp;
            string ip = sameNet && !string.IsNullOrEmpty(srv.local_ip) ? srv.local_ip : srv.ip_address;
            string address = $"{ip}:{srv.port}";

            _lblStatus.Text = "Connexion…";
            _connecting = true;
            _autoRefreshTimer.Stop();
            NetworkManager.Instance.ConnectDirectly(address);
        }

        private void HandleConnected()
        {
            if (InvokeRequired) { Invoke(new Action(HandleConnected)); return; }
            _connecting = false;
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void HandleDisconnected()
        {
            if (InvokeRequired) { Invoke(new Action(HandleDisconnected)); return; }
            if (_connecting) { _connecting = false; _lblStatus.Text = "Connexion échouée."; _autoRefreshTimer.Start(); }
        }
    }
}