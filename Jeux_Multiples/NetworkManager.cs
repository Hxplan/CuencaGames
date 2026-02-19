using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Jeux_Multiples
{
    // ════════════════════════════════════════════════════════════
    //  STRUCTURE DES PAQUETS  (Type|Sender|Content)
    // ════════════════════════════════════════════════════════════
    public class Packet
    {
        public string Type    { get; set; }
        public string Sender  { get; set; }
        public string Content { get; set; }

        public Packet(string type, string sender, string content)
        { Type = type; Sender = sender; Content = content; }

        public override string ToString() => $"{Type}|{Sender}|{Content}";

        public static Packet FromString(string data)
        {
            string[] p = data.Split(new[] { '|' }, 3);
            return p.Length < 3 ? null : new Packet(p[0], p[1], p[2]);
        }
    }

    // ════════════════════════════════════════════════════════════
    //  NETWORK MANAGER (Singleton)
    // ════════════════════════════════════════════════════════════
    public class NetworkManager
    {
        // ── Singleton ────────────────────────────────────────────
        private static readonly Lazy<NetworkManager> _lazy =
            new Lazy<NetworkManager>(() => new NetworkManager());
        public static NetworkManager Instance => _lazy.Value;

        // ── Configuration ─────────────────────────────────────────
        private const int    PORT_TCP      = 8080;
        private const int    PORT_UDP      = 8081;
        private const string DISCOVER_MSG  = "CUENCAGAMES_DISCOVER";
        private const int    PING_INTERVAL = 30_000; // ms

        // ── État public ───────────────────────────────────────────
        public bool   IsHost           { get; private set; }
        public bool   IsConnected      { get; private set; }
        public string MyPseudo         { get; set; } = "Joueur";
        public string CurrentGameType  { get; set; } = "any";
        public string OpponentPseudo   { get; private set; } = "?";
        public string MyLocalIP        { get; private set; }
        public string MyPublicIP       { get; private set; }
        /// <summary>Si true, à la fermeture du jeu on revient au lobby sans déconnecter.</summary>
        public bool ReturnToLobby      { get; set; }

        // ── Services ──────────────────────────────────────────────
        public LobbyClient Lobby { get; } = new LobbyClient();

        // ── Événements ───────────────────────────────────────────
        public event Action<string>  OnLog;
        public event Action<Packet>  OnPacketReceived;
        public event Action          OnConnected;
        public event Action          OnDisconnected;

        // ── Sockets ───────────────────────────────────────────────
        private TcpListener   _listener;
        private TcpClient     _tcpClient;
        private NetworkStream _stream;
        private UdpClient     _udp;

        // ── Threading ─────────────────────────────────────────────
        private bool                   _running;
        private bool                   _registeredOnWeb;
        private string                 _myGuid = Guid.NewGuid().ToString();
        private CancellationTokenSource _cts;

        // ═════════════════════════════════════════════════════════
        private NetworkManager()
        {
            MyLocalIP = GetLocalIP();
            TryUnlockFirewall();
        }

        // ─────────────────────────────────────────────────────────
        private void Log(string msg) => OnLog?.Invoke(msg);

        // ════════════════════════════════════════════════════════
        //  HÉBERGEMENT D'UN SALON
        // ════════════════════════════════════════════════════════
        /// <summary>
        /// Crée un salon visible sur le Web + LAN.
        /// Appeler AVANT de démarrer le matchmaking.
        /// </summary>
        public void HostSalon(string salonName, string gameType = "any", int maxPlayers = 2)
        {
            if (_running) Disconnect();
            _running      = true;
            IsConnected   = false;
            MyPseudo      = salonName;
            CurrentGameType = gameType;
            _cts          = new CancellationTokenSource();

            Log($"🟢 Hébergement de '{salonName}' [{gameType}] …");
            StartTcpListener();
        }

        // ════════════════════════════════════════════════════════
        //  MATCHMAKING AUTO (LAN)
        // ════════════════════════════════════════════════════════
        public void StartMatchmaking()
        {
            if (_running) Disconnect();
            _running    = true;
            IsConnected = false;
            _cts        = new CancellationTokenSource();

            Log("🔍 Recherche automatique sur le LAN …");
            StartTcpListener();
            _ = Task.Run(() => UdpDiscoveryLoop(_cts.Token));
        }

        // ════════════════════════════════════════════════════════
        //  CONNEXION DIRECTE (IP ou IP:PORT)
        // ════════════════════════════════════════════════════════
        public async void ConnectDirectly(string address)
        {
            if (IsConnected) return;

            // Initialiser _cts si on rejoint directement sans passer par HostSalon/StartMatchmaking
            if (_cts == null || _cts.IsCancellationRequested)
            {
                _cts     = new CancellationTokenSource();
                _running = true;
            }

            string host = address;
            int    port = PORT_TCP;

            if (address.Contains(":"))
            {
                string[] parts = address.Split(':');
                if (parts.Length == 2 && int.TryParse(parts[1], out int p))
                { host = parts[0]; port = p; }
            }

            Log($"⏩ Connexion directe → {host}:{port} …");
            try
            {
                var client = new TcpClient();
                await client.ConnectAsync(host, port);
                IsHost = false;
                HandleConnection(client);
            }
            catch (Exception ex)
            {
                Log($"❌ Connexion échouée : {ex.Message}");
                System.Windows.Forms.MessageBox.Show(
                    $"Impossible de rejoindre {host}:{port}\n{ex.Message}",
                    "Erreur de connexion",
                    System.Windows.Forms.MessageBoxButtons.OK,
                    System.Windows.Forms.MessageBoxIcon.Warning);
            }
        }

        // ════════════════════════════════════════════════════════
        //  DÉCONNEXION
        // ════════════════════════════════════════════════════════
        public void Disconnect()
        {
            _running    = false;
            IsConnected = false;

            _cts?.Cancel();
            try { _tcpClient?.Close(); } catch { }
            try { _listener?.Stop();   } catch { }
            try { _udp?.Close();       } catch { }

            _tcpClient = null;
            _stream    = null;
            _listener  = null;
            _udp       = null;

            if (_registeredOnWeb) _ = UnregisterWebAsync();
            OnDisconnected?.Invoke();
            Log("🔌 Déconnecté.");
        }

        // ════════════════════════════════════════════════════════
        //  ENVOI DE PAQUETS
        // ════════════════════════════════════════════════════════
        public void SendPacket(Packet p)
        {
            if (_stream == null) return;
            try
            {
                byte[] data = Encoding.UTF8.GetBytes(p.ToString() + "\n");
                _stream.Write(data, 0, data.Length);
            }
            catch { Disconnect(); }
        }

        // ════════════════════════════════════════════════════════
        //  PRIVÉ — TCP LISTENER
        // ════════════════════════════════════════════════════════
        private void StartTcpListener()
        {
            try
            {
                _listener = new TcpListener(IPAddress.Any, PORT_TCP);
                _listener.Start();
                Log($"📡 Serveur TCP démarré (port {PORT_TCP})");
                _ = RegisterWebAsync();
                _ = AcceptLoopAsync(_cts.Token);
            }
            catch
            {
                Log($"⚠️ Port {PORT_TCP} déjà utilisé — mode client uniquement.");
            }
        }

        private async Task AcceptLoopAsync(CancellationToken ct)
        {
            while (_running && !IsConnected && !ct.IsCancellationRequested)
            {
                try
                {
                    var client = await _listener.AcceptTcpClientAsync();
                    if (IsConnected) { client.Close(); return; }
                    Log("📥 Connexion entrante acceptée !");
                    IsHost = true;
                    HandleConnection(client);
                }
                catch { break; }
            }
        }

        // ════════════════════════════════════════════════════════
        //  PRIVÉ — UDP DISCOVERY (LAN)
        // ════════════════════════════════════════════════════════
        private async Task UdpDiscoveryLoop(CancellationToken ct)
        {
            try
            {
                _udp = new UdpClient();
                _udp.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
                _udp.Client.Bind(new IPEndPoint(IPAddress.Any, PORT_UDP));
                _udp.EnableBroadcast = true;

                // Envoi broadcast en boucle
                _ = Task.Run(async () =>
                {
                    var ep = new IPEndPoint(IPAddress.Broadcast, PORT_UDP);
                    while (!ct.IsCancellationRequested && !IsConnected)
                    {
                        byte[] d = Encoding.UTF8.GetBytes($"{DISCOVER_MSG}|{MyPseudo}|{MyLocalIP}|{_myGuid}");
                        try { await _udp.SendAsync(d, d.Length, ep); } catch { break; }
                        await Task.Delay(1000, ct);
                    }
                }, ct);

                // Écoute broadcast
                while (!ct.IsCancellationRequested && !IsConnected)
                {
                    var result = await _udp.ReceiveAsync();
                    string msg = Encoding.UTF8.GetString(result.Buffer);
                    if (!msg.StartsWith(DISCOVER_MSG)) continue;

                    string[] parts = msg.Split('|');
                    if (parts.Length < 4 || parts[3] == _myGuid) continue;

                    Log($"🎯 Joueur LAN détecté : {parts[1]} ({parts[2]})");

                    // Déterminisme : le GUID le plus petit est client
                    if (string.Compare(_myGuid, parts[3], StringComparison.Ordinal) > 0)
                    {
                        Log("→ Je suis CLIENT (GUID)");
                        ConnectDirectly(parts[2]);
                        break;
                    }
                }
            }
            catch (Exception ex) { if (!ct.IsCancellationRequested) Log("UDP: " + ex.Message); }
        }

        // ════════════════════════════════════════════════════════
        //  PRIVÉ — GESTION CONNEXION ÉTABLIE
        // ════════════════════════════════════════════════════════
        private void HandleConnection(TcpClient client)
        {
            if (IsConnected) { client.Close(); return; }

            IsConnected = true;
            _tcpClient  = client;
            _stream     = client.GetStream();

            Log("✅ Connecté !");
            SendPacket(new Packet("HELLO", MyPseudo, CurrentGameType));
            OnConnected?.Invoke();

            _ = ReadLoopAsync(_cts.Token);
        }

        private async Task ReadLoopAsync(CancellationToken ct)
        {
            var buffer  = new byte[8192];
            var builder = new StringBuilder();

            try
            {
                while (!ct.IsCancellationRequested && _tcpClient?.Connected == true)
                {
                    int n = await _stream.ReadAsync(buffer, 0, buffer.Length, ct);
                    if (n == 0) break;

                    builder.Append(Encoding.UTF8.GetString(buffer, 0, n));
                    string buf = builder.ToString();

                    int nl;
                    while ((nl = buf.IndexOf('\n')) >= 0)
                    {
                        string line = buf.Substring(0, nl);
                        buf = buf.Substring(nl + 1);

                        var p = Packet.FromString(line);
                        if (p == null) continue;

                        if (p.Type == "HELLO")
                        {
                            OpponentPseudo = p.Sender;
                            Log($"👤 Adversaire : {OpponentPseudo}");
                        }
                        OnPacketReceived?.Invoke(p);
                    }
                    builder.Clear();
                    builder.Append(buf);
                }
            }
            catch { }
            finally { if (IsConnected) Disconnect(); }
        }

        // ════════════════════════════════════════════════════════
        //  WEB LOBBY — Enregistrement + Ping loop
        // ════════════════════════════════════════════════════════
        private async Task RegisterWebAsync()
        {
            Log("🌍 Enregistrement sur le Lobby Web …");
            MyPublicIP = await Lobby.GetPublicIPAsync();
            if (string.IsNullOrEmpty(MyPublicIP))
            { Log("⚠️ IP publique introuvable — lobby ignoré."); return; }

            Log($"IP publique : {MyPublicIP}");

            // Lire maxPlayers depuis le contexte courant (2 par défaut)
            int? id = await Lobby.RegisterServerAsync(
                name       : MyPseudo,
                publicIp   : MyPublicIP,
                localIp    : MyLocalIP,
                port       : PORT_TCP,
                gameType   : CurrentGameType,
                maxPlayers : 2,
                hostPseudo : MyPseudo);

            if (id == null) { Log("❌ Échec enregistrement."); return; }

            _registeredOnWeb = true;
            Log($"✅ Salon enregistré (ID {id})");
            _ = PingLoopAsync();
        }

        private async Task PingLoopAsync()
        {
            while (_registeredOnWeb && _running && !IsConnected)
            {
                await Task.Delay(PING_INTERVAL);
                if (!_registeredOnWeb || !_running || IsConnected) break;

                if (!string.IsNullOrEmpty(MyPublicIP))
                    await Lobby.PingAsync(MyPublicIP, PORT_TCP);
            }
        }

        private async Task UnregisterWebAsync()
        {
            _registeredOnWeb = false;
            if (!string.IsNullOrEmpty(MyPublicIP))
                await Lobby.RemoveServerAsync(MyPublicIP, PORT_TCP);
        }

        // ════════════════════════════════════════════════════════
        //  UTILITAIRES
        // ════════════════════════════════════════════════════════
        public string GetLocalIP()
        {
            try
            {
                using (var s = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, 0))
                {
                    s.Connect("8.8.8.8", 65530);
                    return ((IPEndPoint)s.LocalEndPoint).Address.ToString();
                }
            }
            catch { return "127.0.0.1"; }
        }

        private void TryUnlockFirewall()
        {
            try
            {
                string exe  = System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName;
                string rule = "CuencaGames_TCP";
                Run("netsh", $"advfirewall firewall delete rule name=\"{rule}\"");
                Run("netsh", $"advfirewall firewall add rule name=\"{rule}\" dir=in action=allow program=\"{exe}\" enable=yes protocol=tcp localport={PORT_TCP}");
                Run("netsh", $"advfirewall firewall add rule name=\"CuencaGames_UDP\" dir=in action=allow program=\"{exe}\" enable=yes protocol=udp localport={PORT_UDP}");
            }
            catch { }
        }

        private static void Run(string exe, string args)
        {
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
            {
                FileName = exe, Arguments = args,
                UseShellExecute = false, CreateNoWindow = true, Verb = "runas"
            });
        }
    }
}