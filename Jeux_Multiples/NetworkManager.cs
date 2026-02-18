using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Jeux_Multiples
{
    // ════════════════════════════════════════════════════════════════
    //  STRUCTURE DES PAQUETS
    // ════════════════════════════════════════════════════════════════
    public class Packet
    {
        public string Type { get; set; }    // ex: "CHAT", "MOVE", "HELLO"
        public string Sender { get; set; }  // Pseudo
        public string Content { get; set; } //

        public Packet(string type, string sender, string content)
        {
            Type = type;
            Sender = sender;
            Content = content;
        }

        public override string ToString()
        {
            return $"{Type}|{Sender}|{Content}";
        }

        public static Packet FromString(string data)
        {
            string[] parts = data.Split(new char[] { '|' }, 3);
            if (parts.Length < 3) return null;
            return new Packet(parts[0], parts[1], parts[2]);
        }
    }

    // ════════════════════════════════════════════════════════════════
    //  MANAGER RÉSEAU (AUTO-MATCH)
    // ════════════════════════════════════════════════════════════════
    public class NetworkManager
    {
        private static NetworkManager _instance;
        public static NetworkManager Instance
        {
            get
            {
                if (_instance == null) _instance = new NetworkManager();
                return _instance;
            }
        }

        // Configuration
        private const int PORT_UDP = 8081; // Was 45000
        private const int PORT_TCP = 8080; // Was 45001 - Using 8080 as "standard" port
        private const string DISCOVER_MSG = "JEUX_MULTIPLES_AUTO";

        // État
        public bool IsHost { get; private set; } = false;
        public bool IsConnected { get; private set; } = false;
        public string MyPseudo { get; set; } = "Joueur";
        public string OpponentPseudo { get; private set; } = "En attente...";
        public string MyIP { get; private set; }
        private string _myGuid; // Unique ID for this instance

        // Sockets
        private UdpClient udpSock;
        private TcpListener tcpListener;
        private TcpClient tcpClient;
        private NetworkStream listStream;

        // Events
        public event Action<string> OnLog;
        public event Action<Packet> OnPacketReceived;
        public event Action OnConnected; 
        public event Action OnDisconnected;

        // Threading
        private bool running = false;
        private CancellationTokenSource cts;

        private NetworkManager() 
        {
            MyIP = GetLocalIP();
            _myGuid = Guid.NewGuid().ToString();
            UnlockFirewall();
        }

        private void Log(string msg)
        {
            OnLog?.Invoke(msg);
        }

        private void UnlockFirewall()
        {
            try
            {
                string exePath = System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName;
                string ruleName = "Jeux_Multiples_LAN";
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = "netsh",
                    Arguments = $"advfirewall firewall delete rule name=\"{ruleName}\"",
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    Verb = "runas"
                });

                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = "netsh",
                    Arguments = $"advfirewall firewall add rule name=\"{ruleName}_TCP\" dir=in action=allow program=\"{exePath}\" enable=yes profile=any remoteip=any protocol=tcp localport={PORT_TCP}",
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    Verb = "runas"
                });

                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = "netsh",
                    Arguments = $"advfirewall firewall add rule name=\"{ruleName}_UDP\" dir=in action=allow program=\"{exePath}\" enable=yes profile=any remoteip=any protocol=udp localport={PORT_UDP}",
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    Verb = "runas"
                });
            }
            catch { /* Ignorer si pas admin */ }
        }

        // ════════════════════════════════════════════════════════════
        //  AUTO-MATCHMAKING
        // ════════════════════════════════════════════════════════════
        public void StartMatchmaking()
        {
            if (running) Disconnect();
            running = true;
            IsConnected = false;
            cts = new CancellationTokenSource();

            Log("🔍 Recherche d'adversaire...");

            // 1. Démarrer Serveur TCP (Pour Tunnel/DirectIP)
            StartListening();

            // 2. Broadcast UDP (LAN)
            _ = Task.Run(() => DiscoverLoop(cts.Token));

            // 3. Scan TCP (LAN Routé)
            _ = Task.Run(() => ScanSubnets(cts.Token));
        }

        private void StartListening()
        {
            try
            {
                if (tcpListener == null)
                {
                    tcpListener = new TcpListener(IPAddress.Any, PORT_TCP);
                    tcpListener.Start();
                    Log("Serveur TCP démarré (Port " + PORT_TCP + ")");
                    
                    // Enregistrement Web Lobby
                    RegisterToWebLobby();
                    
                    _ = Task.Run(async () =>
                    {
                        try
                        {
                            while (running && !IsConnected)
                            {
                                 var client = await tcpListener.AcceptTcpClientAsync();
                                 if (IsConnected) { client.Close(); return; }
                                 
                                 Log("Connexion entrante acceptée !");
                                 IsHost = true; // Je suis le serveur
                                 HandleConnection(client);
                            }
                        }
                        catch { }
                    });
                }
            }
            catch (Exception ex)
            {
                Log("Impossible de lier le port " + PORT_TCP + " (Déjà utilisé ?). Mode Client uniquement.");
            }
        }

        private async Task ScanSubnets(CancellationToken token)
        {
            Log("🚀 Scanning 10.138.8.x et 10.138.9.x...");
            
            List<string> subnets = new List<string>();
            string[] parts = MyIP.Split('.');
            if (parts.Length == 4)
            {
                string prefix = $"{parts[0]}.{parts[1]}";
                subnets.Add($"{prefix}.8");
                subnets.Add($"{prefix}.9");
                if (parts[2] != "8" && parts[2] != "9") subnets.Add($"{prefix}.{parts[2]}");
            }

            List<Task> scanTasks = new List<Task>();
            foreach(var subnet in subnets)
            {
                for (int i = 1; i < 255; i++)
                {
                    if (IsConnected) break;
                    string targetIP = $"{subnet}.{i}";
                    if (targetIP == MyIP) continue;

                    scanTasks.Add(CheckHostAsync(targetIP, token));
                    if (scanTasks.Count % 50 == 0) await Task.Delay(50); 
                }
            }
            await Task.WhenAll(scanTasks);
            if (!IsConnected) Log("Fin du scan.");
        }

        private async Task CheckHostAsync(string ip, CancellationToken token)
        {
            if (IsConnected || token.IsCancellationRequested) return;
            try
            {
                using (TcpClient tempClient = new TcpClient())
                {
                    var connectTask = tempClient.ConnectAsync(ip, PORT_TCP);
                    var timeoutTask = Task.Delay(200);
                    if (await Task.WhenAny(connectTask, timeoutTask) == connectTask)
                    {
                        if (tempClient.Connected)
                        {
                            Log($"🎯 HÔTE DÉTECTÉ: {ip}");
                            ConnectDirectly(ip); 
                        }
                    }
                }
            }
            catch { }
        }

        private async Task DiscoverLoop(CancellationToken token)
        {
            try
            {
                udpSock = new UdpClient();
                udpSock.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
                udpSock.Client.Bind(new IPEndPoint(IPAddress.Any, PORT_UDP));
                udpSock.EnableBroadcast = true;

                _ = Task.Run(async () => 
                {
                    IPEndPoint broadcastEp = new IPEndPoint(IPAddress.Broadcast, PORT_UDP);
                    while (!token.IsCancellationRequested && !IsConnected)
                    {
                        byte[] data = Encoding.UTF8.GetBytes($"{DISCOVER_MSG}|{MyPseudo}|{MyIP}|{_myGuid}");
                        await udpSock.SendAsync(data, data.Length, broadcastEp);
                        await Task.Delay(1000, token);
                    }
                }, token);

                while (!token.IsCancellationRequested && !IsConnected)
                {
                    var result = await udpSock.ReceiveAsync();
                    string msg = Encoding.UTF8.GetString(result.Buffer);
                    if (msg.StartsWith(DISCOVER_MSG))
                    {
                        var parts = msg.Split('|');
                        if (parts.Length < 4) continue;
                        if (parts[3] == _myGuid) continue;

                        Log($"Joueur trouvé : {parts[1]} ({parts[2]})");
                        int comparison = String.Compare(_myGuid, parts[3]);
                        if (comparison < 0)
                        {
                            Log("Je suis CLIENT (guid distant prioritaire).");
                            ConnectDirectly(parts[2]);
                            break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                if (!base.Equals(ex.ToString())) Log("Erreur Discovery: " + ex.Message);
            }
        }

        // Ancienne méthode pour UDP Role Resolution (Obsolète mais gardée pour compatibilité logique)
        private void StartHostTimeout()
        {
            if (IsHost) return;
            IsHost = true;
            Log("Rôle défini : HOST (via UDP GUID)");
        }

        public async void ConnectDirectly(string address)
        {
             if (IsConnected) return;

             try
             {
                 string host = address;
                 int port = PORT_TCP;

                 // Support syntaxe "IP:PORT" ou "Domaine:PORT"
                 if (address.Contains(":"))
                 {
                     string[] parts = address.Split(':');
                     if (parts.Length == 2 && int.TryParse(parts[1], out int p))
                     {
                         host = parts[0];
                         port = p;
                     }
                 }

                  tcpClient = new TcpClient();
                  await tcpClient.ConnectAsync(host, port);
                  IsHost = false; // Je suis client car j'ai initié la connexion
                  HandleConnection(tcpClient);
                  Log($"Connecté direct à {host}:{port}");
              }
             catch (Exception ex)
             {
                 Log("Echec connexion TCP : " + ex.Message);
                 System.Windows.Forms.MessageBox.Show($"Impossible de rejoindre {address}.\nErreur : {ex.Message}\n\nVérifiez le PORT et la reachabilité de l'adresse.", "Erreur Connexion", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
             }
        }

        private void HandleConnection(TcpClient client)
        {
            if (IsConnected) return; // Déjà connecté race condition
            
            IsConnected = true;
            tcpClient = client;
            listStream = tcpClient.GetStream();
            
            Log("✅ CONNECTÉ !");
            
            // Send Handshake immediately
            SendPacket(new Packet("HELLO", MyPseudo, "v1"));

            OnConnected?.Invoke();
            
            // Start reading
            _ = Task.Run(() => ReadLoop(cts.Token));
        }

        // ════════════════════════════════════════════════════════════
        //  COMM & TOOLS
        // ════════════════════════════════════════════════════════════
        public void SendPacket(Packet p)
        {
            if (tcpClient == null || !tcpClient.Connected) return;
            try
            {
                string raw = p.ToString() + "\n";
                byte[] data = Encoding.UTF8.GetBytes(raw);
                listStream.Write(data, 0, data.Length);
            }
            catch { Disconnect(); }
        }

        private async Task ReadLoop(CancellationToken token)
        {
            byte[] buffer = new byte[4096];
            StringBuilder msgBuilder = new StringBuilder();

            try
            {
                while (!token.IsCancellationRequested && tcpClient.Connected)
                {
                    int read = await listStream.ReadAsync(buffer, 0, buffer.Length, token);
                    if (read == 0) break;
                    
                    msgBuilder.Append(Encoding.UTF8.GetString(buffer, 0, read));
                    string content = msgBuilder.ToString();
                    
                    if (content.Contains("\n"))
                    {
                        string[] msgs = content.Split('\n');
                        for(int i=0; i<msgs.Length-1; i++)
                        {
                            var p = Packet.FromString(msgs[i]);
                            if (p != null)
                            {
                                if (p.Type == "HELLO")
                                {
                                    OpponentPseudo = p.Sender;
                                    Log($"Adversaire identifié : {OpponentPseudo}");
                                }
                                OnPacketReceived?.Invoke(p);
                            }
                        }
                        msgBuilder.Clear();
                        msgBuilder.Append(msgs[msgs.Length-1]);
                    }
                }
            }
            catch {}
            finally { Disconnect(); }
        }

        public void Disconnect()
        {
            running = false;
            IsConnected = false;
            cts?.Cancel();
            tcpClient?.Close();
            tcpListener?.Stop();
            udpSock?.Close();
            UnregisterFromWebLobby();
            OnDisconnected?.Invoke();
            Log("Déconnecté.");
        }

        public string GetLocalIP()
        {
            try
            {
                using (Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, 0))
                {
                    socket.Connect("8.8.8.8", 65530);
                    IPEndPoint endPoint = socket.LocalEndPoint as IPEndPoint;
                    return endPoint.Address.ToString();
                }
            }
            catch { return "127.0.0.1"; }
        }

        // Public helper to host a salon with a given name (sets pseudo and starts listening + registers to web lobby)
        public void HostSalon(string salonName)
        {
            if (running) Disconnect();
            running = true;
            IsConnected = false;
            MyPseudo = salonName;
            cts = new CancellationTokenSource();

            // Start listening (this will call RegisterToWebLobby internally)
            StartListening();
        }

        // ════════════════════════════════════════════════════════════
        //  WEB LOBBY (Hostinger)
        // ════════════════════════════════════════════════════════════
        public LobbyClient Lobby { get; private set; } = new LobbyClient();
        private bool _registeredToLobby = false;

        private async void RegisterToWebLobby()
        {
            if (_registeredToLobby) return;
            Log("🌍 Connexion au Lobby Web...");

            string publicIp = await Lobby.GetPublicIP();
            if (string.IsNullOrEmpty(publicIp))
            {
                Log("⚠️ Impossible de récupérer l'IP Publique. Lobby ignoré.");
                return;
            }

            Log($"Mon IP Publique : {publicIp}");
            
            int? id = await Lobby.RegisterServer(MyPseudo, publicIp, MyIP, PORT_TCP);
            
            if (id != null)
            {
                _registeredToLobby = true;
                Log("✅ Serveur enregistré sur le Web ! (ID: " + id + ")");
                
                // Lancer la boucle de Ping pour rester visible
                _ = LobbyPingLoop();
            }
            else
            {
                Log("❌ Échec enregistrement Web.");
            }
        }

        private async Task LobbyPingLoop()
        {
            while (_registeredToLobby && running && !IsConnected)
            {
                await Task.Delay(60000); // 60 secondes
                if (!_registeredToLobby || !running || IsConnected) break;

                string publicIp = await Lobby.GetPublicIP();
                if (!string.IsNullOrEmpty(publicIp))
                {
                    await Lobby.RegisterServer(MyPseudo, publicIp, MyIP, PORT_TCP);
                }
            }
        }

        private async void UnregisterFromWebLobby()
        {
            if (!_registeredToLobby) return;
            string publicIp = await Lobby.GetPublicIP();
            if (!string.IsNullOrEmpty(publicIp))
            {
                await Lobby.RemoveServer(publicIp, PORT_TCP);
                Log("Serveur retiré du Lobby Web.");
            }
            _registeredToLobby = false;
        }

    }
}
