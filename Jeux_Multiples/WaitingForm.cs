using System;
using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Jeux_Multiples
{
    public class WaitingForm : Form
    {
        private Label lblStatus;
        private Label lblInfo;
        private Timer animTimer;
        private int dotCount = 0;
        private Button btnCancel;

        private ListBox lstServers;
        private Button btnRefresh;
        private string myPublicIp = null;

        public WaitingForm()
        {
            this.Size = new Size(500, 450); // Agrandir pour la liste
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.None;
            this.BackColor = Color.FromArgb(10, 10, 18);
            
            lblStatus = new Label();
            lblStatus.Text = "RECHERCHE D'ADVERSAIRE";
            lblStatus.Font = new Font("Segoe UI", 20, FontStyle.Bold);
            lblStatus.ForeColor = Color.Cyan;
            lblStatus.AutoSize = false;
            lblStatus.TextAlign = ContentAlignment.MiddleCenter;
            lblStatus.Dock = DockStyle.Top;
            lblStatus.Height = 60;
            this.Controls.Add(lblStatus);

            lblInfo = new Label();
            lblInfo.Text = "Votre IP : " + NetworkManager.Instance.MyIP + "\nEn attente d'adversaire...";
            lblInfo.Font = new Font("Segoe UI", 10);
            lblInfo.ForeColor = Color.Gray;
            lblInfo.AutoSize = false;
            lblInfo.TextAlign = ContentAlignment.MiddleCenter;
            lblInfo.Dock = DockStyle.Top;
            lblInfo.Height = 50;
            this.Controls.Add(lblInfo);

            // Liste des serveurs
            Label lblList = new Label();
            lblList.Text = "PARTIES EN LIGNE (WEB)";
            lblList.ForeColor = Color.White;
            lblList.Location = new Point(50, 120);
            lblList.AutoSize = true;
            this.Controls.Add(lblList);

            lstServers = new ListBox();
            lstServers.Location = new Point(50, 140);
            lstServers.Size = new Size(400, 150);
            lstServers.BackColor = Color.FromArgb(20, 20, 30);
            lstServers.ForeColor = Color.Cyan;
            lstServers.BorderStyle = BorderStyle.FixedSingle;
            lstServers.Font = new Font("Segoe UI", 10);
            lstServers.DoubleClick += LstServers_DoubleClick;
            this.Controls.Add(lstServers);

            btnRefresh = new Button();
            btnRefresh.Text = "ACTUALISER";
            btnRefresh.Size = new Size(100, 30);
            btnRefresh.Location = new Point(350, 110); // Juste au dessus de la liste
            btnRefresh.FlatStyle = FlatStyle.Flat;
            btnRefresh.ForeColor = Color.Yellow;
            btnRefresh.Click += async (s, e) => await RefreshServerList();
            this.Controls.Add(btnRefresh);

            btnCancel = new Button();
            btnCancel.Text = "ANNULER";
            btnCancel.Size = new Size(120, 40);
            btnCancel.Location = new Point(50, 350);
            btnCancel.FlatStyle = FlatStyle.Flat;
            btnCancel.ForeColor = Color.Red;
            btnCancel.Click += (s, e) => { this.DialogResult = DialogResult.Cancel; this.Close(); };
            this.Controls.Add(btnCancel);

            Button btnDirect = new Button();
            btnDirect.Text = "REJOINDRE IP";
            btnDirect.Size = new Size(120, 40);
            btnDirect.Location = new Point(330, 350);
            btnDirect.FlatStyle = FlatStyle.Flat;
            btnDirect.ForeColor = Color.Cyan;
            btnDirect.Click += BtnDirect_Click;
            this.Controls.Add(btnDirect);

            Button btnCreate = new Button();
            btnCreate.Text = "CRÉER SALON";
            btnCreate.Size = new Size(120, 40);
            btnCreate.Location = new Point(190, 350);
            btnCreate.FlatStyle = FlatStyle.Flat;
            btnCreate.ForeColor = Color.Lime;
            btnCreate.Click += async (s, e) =>
            {
                string name = Microsoft.VisualBasic.Interaction.InputBox("Nom du salon (pseudo affiché) :", "Créer Salon", NetworkManager.Instance.MyPseudo);
                if (!string.IsNullOrWhiteSpace(name))
                {
                    lblStatus.Text = "HÉBERGEMENT SALON...";
                    NetworkManager.Instance.HostSalon(name);
                    btnCreate.Enabled = false;
                    await Task.Delay(200); // small UI breath
                    await RefreshServerList();
                }
            };
            this.Controls.Add(btnCreate);

            // Paint border
            this.Paint += (s, e) => ControlPaint.DrawBorder(e.Graphics, this.ClientRectangle, Color.Cyan, ButtonBorderStyle.Solid);

            animTimer = new Timer();
            animTimer.Interval = 500;
            animTimer.Tick += (s, e) => 
            {
                dotCount = (dotCount + 1) % 4;
                string dots = new string('.', dotCount);
                if (!lblStatus.Text.StartsWith("CONN")) // Ne pas écraser "Connecté"
                     lblStatus.Text = "RECHERCHE" + dots;
            };
            animTimer.Start();

            // Auto Start
            this.Load += WaitingForm_Load;
        }

        private async void WaitingForm_Load(object sender, EventArgs e)
        {
            NetworkManager.Instance.OnConnected += HandleConnected;
            await RefreshServerList();
        }

        private async Task RefreshServerList()
        {
            lstServers.Items.Clear();
            lstServers.Items.Add("Chargement...");
            
            myPublicIp = await NetworkManager.Instance.Lobby.GetPublicIP();
            var servers = await NetworkManager.Instance.Lobby.GetServers();

            lstServers.Items.Clear();
            bool any = false;
            foreach (var server in servers)
            {
                // Skip salons created with the same pseudo as current user
                if (!string.IsNullOrEmpty(NetworkManager.Instance.MyPseudo) && server.server_name == NetworkManager.Instance.MyPseudo) continue;
                lstServers.Items.Add(new ServerListItem(server));
                any = true;
            }
            if (!any) lstServers.Items.Add("Aucun serveur trouvé.");
        }

        private void LstServers_DoubleClick(object sender, EventArgs e)
        {
            if (lstServers.SelectedItem is ServerListItem item)
            {
                string dest = item.Info.ip_address;
                // If the host reports the same public IP as us and provided a local_ip, try local IP first
                if (!string.IsNullOrEmpty(myPublicIp) && item.Info.ip_address == myPublicIp && !string.IsNullOrEmpty(item.Info.local_ip))
                {
                    dest = item.Info.local_ip;
                }
                string address = $"{dest}:{item.Info.port}";
                lblStatus.Text = "CONNEXION LOBBY...";
                NetworkManager.Instance.ConnectDirectly(address);
            }
        }

        // Wrapper pour l'affichage
        private class ServerListItem
        {
            public ServerInfo Info;
            public ServerListItem(ServerInfo info) { Info = info; }
            public override string ToString() { return $"{Info.server_name} [{Info.ip_address}]"; }
        }

        private void HandleConnected()
        {
            if (this.InvokeRequired) { this.Invoke(new Action(HandleConnected)); return; }
            
            lblStatus.ForeColor = Color.Lime;
            lblStatus.Text = "CONNECTÉ !";
            animTimer.Stop();
            
            // Delay court pour voir le succès
            Timer t = new Timer();
            t.Interval = 1000;
            t.Tick += (s, e) => 
            {
                t.Stop();
                this.DialogResult = DialogResult.OK;
                this.Close();
            };
            t.Start();
        }

        private void BtnDirect_Click(object sender, EventArgs e)
        {
            string ip = Microsoft.VisualBasic.Interaction.InputBox("Entrez l'IP:PORT (ex: 192.168.1.5:8080) :", "Connexion Directe", "192.168.1.");
            if (!string.IsNullOrWhiteSpace(ip))
            {
                lblStatus.Text = "CONNEXION À " + ip + "...";
                NetworkManager.Instance.ConnectDirectly(ip);
            }
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            NetworkManager.Instance.OnConnected -= HandleConnected; 
            // Si annulé :
            if (this.DialogResult != DialogResult.OK)
                NetworkManager.Instance.Disconnect();
        }
    }
}
