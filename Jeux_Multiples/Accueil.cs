using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace Jeux_Multiples
{
    public partial class Accueil : Form
    {
        // ── Couleurs ────────────────────────────────────────────────
        private readonly Color colFond = Color.FromArgb(10, 10, 18);
        private readonly Color colAccent1 = Color.FromArgb(0, 255, 136); // Vert néon
        private readonly Color colAccent2 = Color.FromArgb(0, 238, 255); // Cyan néon
        private readonly Color colAccent3 = Color.FromArgb(255, 34, 68); // Rouge néon
        private readonly Color colPanel = Color.FromArgb(18, 18, 28);
        private readonly Color colText = Color.White;

        // ── Contrôles ───────────────────────────────────────────────
        private TextBox txtJoueur1;
        private TextBox txtJoueur2;
        private Label lblTitre;

        public Accueil()
        {
            InitializeComponent();
            ConfigForm();
            // Force load event logic to clear old designer controls
            Acceuil_Load(this, EventArgs.Empty);
        }

        private void ConfigForm()
        {
            this.Text = "ARCADE MULTIPLAYER";
            this.Size = new Size(900, 600);
            this.MinimumSize = new Size(900, 600);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = colFond;
            this.ForeColor = colText;
            this.DoubleBuffered = true;
        }

        private void Acceuil_Load(object sender, EventArgs e)
        {
            // Nettoyer les vieux contrôles du Designer pour une refonte totale
            this.Controls.Clear();
            ConstruireInterface();
        }

        private void ConstruireInterface()
        {
            // ── Titre ──────────────────────────────────────────────
            lblTitre = new Label
            {
                Text = "ARCADE ZONE",
                Font = new Font("Segoe UI", 36, FontStyle.Bold),
                ForeColor = colAccent2,
                AutoSize = true,
                Location = new Point(0, 30),
                TextAlign = ContentAlignment.MiddleCenter
            };
            // Centrage horizontal après création
            lblTitre.Location = new Point((this.ClientSize.Width - lblTitre.PreferredWidth) / 2, 30);
            this.Controls.Add(lblTitre);

            // ── Panneau Identité ───────────────────────────────────
            Panel panelJoueurs = new Panel
            {
                Size = new Size(400, 100), // Plus petit, centré
                Location = new Point((this.ClientSize.Width - 400) / 2, 120),
                BackColor = colPanel,
            };
            panelJoueurs.Paint += (s, e) =>
            {
                ControlPaint.DrawBorder(e.Graphics, panelJoueurs.ClientRectangle, colAccent1, ButtonBorderStyle.Solid);
            };
            this.Controls.Add(panelJoueurs);

            // Input Pseudo
            Label lblJ1 = NewLabel("VOTRE PSEUDO", 0, 25, colAccent2);
            lblJ1.AutoSize = false;
            lblJ1.Size = new Size(400, 25);
            lblJ1.TextAlign = ContentAlignment.MiddleCenter;
            panelJoueurs.Controls.Add(lblJ1);

            txtJoueur1 = NewTextBox(50, 50); // Centré dans le panel 400 wide -> 300 wide text box à x=50
            txtJoueur1.Text = Environment.UserName; // Auto-fill windows username
            txtJoueur1.TextAlign = HorizontalAlignment.Center;
            panelJoueurs.Controls.Add(txtJoueur1);

            // Pas de Joueur 2 (Auto pour local)
            txtJoueur2 = new TextBox(); // Hidden dummy for logic compatibility if needed, or just remove
            txtJoueur2.Text = "Invité"; 
            
            // ── Mode En Ligne Toggle ───────────────────────────────
            CheckBox chkOnline = new CheckBox();
            chkOnline.Text = "JEU EN RÉSEAU (LAN)";
            chkOnline.Appearance = Appearance.Button;
            chkOnline.TextAlign = ContentAlignment.MiddleCenter;
            chkOnline.Size = new Size(240, 40);
            chkOnline.Location = new Point((this.ClientSize.Width - 240) / 2, 190);
            chkOnline.FlatStyle = FlatStyle.Flat;
            chkOnline.BackColor = Color.FromArgb(40, 20, 60);
            chkOnline.ForeColor = Color.Gray;
            chkOnline.Font = new Font("Segoe UI Black", 11, FontStyle.Bold);
            chkOnline.Cursor = Cursors.Hand;
            
            chkOnline.CheckedChanged += (s, e) => 
            {
                if (chkOnline.Checked)
                {
                    chkOnline.BackColor = Color.Magenta;
                    chkOnline.ForeColor = Color.White;
                }
                else
                {
                    chkOnline.BackColor = Color.FromArgb(40, 20, 60);
                    chkOnline.ForeColor = Color.Gray;
                }
            };
            this.Controls.Add(chkOnline);

            // ── Grille de jeux ─────────────────────────────────────
            int gridY = 250;
            int btnW = 200;
            int btnH = 60;
            int gapX = 30;
            int gapY = 30;
            // 3 colonnes centré
            int startX = (this.ClientSize.Width - (btnW * 3 + gapX * 2)) / 2;

            // Ligne 1
            AddGameBtn("PUISSANCE 4", startX, gridY, colAccent1, (s, e) => LancerJeu(() => new Puissance_4(GetJ1(), GetJ2(), NetworkManager.Instance.IsConnected)));
            AddGameBtn("JEU DE DAMES", startX + btnW + gapX, gridY, colAccent2, (s, e) => LancerJeu(() => new Dame(GetJ1(), GetJ2(), NetworkManager.Instance.IsConnected)));
            AddGameBtn("MORT PION", startX + (btnW + gapX) * 2, gridY, colAccent3, (s, e) => LancerJeu(() => new mort_Pion(GetJ1(), GetJ2(), NetworkManager.Instance.IsConnected)));

            // Ligne 2
            AddGameBtn("SNAKE", startX, gridY + btnH + gapY, Color.Lime, (s, e) => LancerJeu(() => new Snake(GetJ1())));
            AddGameBtn("BLACKJACK", startX + btnW + gapX, gridY + btnH + gapY, Color.Gold, (s, e) => LancerJeu(() => new BlackJack(GetJ1())));
            AddGameBtn("POKER", startX + (btnW + gapX) * 2, gridY + btnH + gapY, Color.Orange, (s, e) => LancerJeu(() => new Poker(GetJ1())));

            // ── Pied de page ───────────────────────────────────────
            Button btnQuitter = new Button
            {
                Text = "QUITTER",
                Size = new Size(150, 40),
                Location = new Point((this.ClientSize.Width - 150) / 2, this.ClientSize.Height - 60),
                FlatStyle = FlatStyle.Flat,
                ForeColor = Color.Gray,
                Font = new Font("Segoe UI", 10),
                Cursor = Cursors.Hand
            };
            btnQuitter.FlatAppearance.BorderSize = 0;
            btnQuitter.Click += (s, e) => Application.Exit();
            this.Controls.Add(btnQuitter);
        }

        // ── Helpers ────────────────────────────────────────────────
        private string GetJ1() => string.IsNullOrWhiteSpace(txtJoueur1.Text) ? "Joueur" : txtJoueur1.Text;
        private string GetJ2() => "Invité"; // Default for local play

        private void LancerJeu(Func<Form> createForm)
        {
            // Vérifier si Mode En Ligne est activé
            bool online = false;
            foreach(Control c in this.Controls) 
                if (c is CheckBox chk && chk.Text.Contains("RÉSEAU") && chk.Checked) online = true;

            if (online)
            {
                // 1. Ouvrir WaitingForm
                NetworkManager.Instance.MyPseudo = GetJ1();
                WaitingForm wait = new WaitingForm();
                if (wait.ShowDialog() == DialogResult.OK)
                {
                    // 2. Connecté ! Lancer le jeu
                    try
                    {
                        Form jeu = createForm();
                        jeu.FormClosed += (s, args) => 
                        {
                            NetworkManager.Instance.Disconnect(); // Déco quand on quitte le jeu
                            this.Show(); 
                        };
                        jeu.Show();
                        this.Hide();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Erreur lancement jeu : " + ex.Message);
                    }
                }
                // Si Cancel, on ne fait rien (reste sur Accueil)
            }
            else
            {
                // LOCAL
                try
                {
                    Form jeu = createForm();
                    jeu.FormClosed += (s, args) => this.Show(); 
                    jeu.Show();
                    this.Hide();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Erreur lancement jeu : " + ex.Message);
                    this.Show();
                }
            }
        }

        private TextBox NewTextBox(int x, int y)
        {
            return new TextBox
            {
                Location = new Point(x, y),
                Size = new Size(300, 30),
                Font = new Font("Segoe UI", 12),
                BackColor = Color.FromArgb(30, 30, 40),
                ForeColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };
        }

        private Label NewLabel(string text, int x, int y, Color color)
        {
            return new Label
            {
                Text = text,
                Location = new Point(x, y),
                AutoSize = true,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = color
            };
        }

        private void AddGameBtn(string text, int x, int y, Color accentColor, EventHandler onClick)
        {
            Button btn = new Button
            {
                Text = text,
                Location = new Point(x, y),
                Size = new Size(200, 60),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(20, 20, 30),
                ForeColor = accentColor,
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            
            btn.FlatAppearance.BorderColor = accentColor;
            btn.FlatAppearance.BorderSize = 1;
            btn.FlatAppearance.MouseOverBackColor = Color.FromArgb(40, accentColor.R, accentColor.G, accentColor.B);
            
            btn.Click += onClick;
            this.Controls.Add(btn);
        }

        // Garder les anciens event handlers vides si le designer en a besoin pour ne pas crash
        // (Bien que le designer ne compile pas le code C#, le runtime via InitializeComponent pourrait chercher ces méthodes)
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
