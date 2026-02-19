using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;

namespace Jeux_Multiples
{
    // ════════════════════════════════════════════════════════════
    //  MODÈLE D'UN SERVEUR LISTÉ
    // ════════════════════════════════════════════════════════════
    public class ServerInfo
    {
        public int    id              { get; set; }
        public string server_name    { get; set; }
        public string host_pseudo    { get; set; }
        public string ip_address     { get; set; }
        public string local_ip       { get; set; }
        public int    port           { get; set; }
        public string game_type      { get; set; } = "any";
        public int    current_players { get; set; } = 1;
        public int    max_players    { get; set; } = 2;

        public bool IsFull => current_players >= max_players;

        // Affichage dans la ListBox
        public override string ToString()
        {
            string full = IsFull ? " [PLEIN]" : "";
            return $"[{game_type}]  {server_name}  —  {host_pseudo}  ({current_players}/{max_players}){full}  [{ip_address}]";
        }
    }

    // ════════════════════════════════════════════════════════════
    //  CLIENT HTTP → lobby_api.php
    // ════════════════════════════════════════════════════════════
    public class LobbyClient
    {
        private const string API_URL = "https://cuencamathieu.com/lobby_api.php";

        private static readonly HttpClient _http = new HttpClient { Timeout = TimeSpan.FromSeconds(8) };
        private static readonly JavaScriptSerializer _json = new JavaScriptSerializer();

        // ── IP publique ───────────────────────────────────────────
        public async Task<string> GetPublicIPAsync()
        {
            try { return (await _http.GetStringAsync("https://api.ipify.org")).Trim(); }
            catch { }
            try { return (await _http.GetStringAsync("https://checkip.amazonaws.com")).Trim(); }
            catch { return null; }
        }

        // ── Enregistrer / mettre à jour le salon ─────────────────
        public async Task<int?> RegisterServerAsync(
            string name, string publicIp, string localIp, int port,
            string gameType = "any", int maxPlayers = 2, string hostPseudo = "")
        {
            try
            {
                var payload = new {
                    name        = name,
                    ip          = publicIp,
                    local_ip    = localIp,
                    port        = port,
                    game_type   = gameType,
                    max_players = maxPlayers,
                    host_pseudo = hostPseudo
                };
                var resp = await PostJsonAsync("?action=host", payload);
                if (resp != null && resp.ContainsKey("id"))
                    return Convert.ToInt32(resp["id"]);
            }
            catch (Exception ex) { Log("RegisterServer: " + ex.Message); }
            return null;
        }

        // ── Lister les salons actifs ──────────────────────────────
        public async Task<List<ServerInfo>> GetServersAsync(string gameTypeFilter = null)
        {
            try
            {
                string url = API_URL + "?action=list";
                if (!string.IsNullOrEmpty(gameTypeFilter) && gameTypeFilter != "Tous")
                    url += "&game_type=" + Uri.EscapeDataString(gameTypeFilter);

                string raw = await _http.GetStringAsync(url);
                return _json.Deserialize<List<ServerInfo>>(raw) ?? new List<ServerInfo>();
            }
            catch (Exception ex)
            {
                Log("GetServers: " + ex.Message);
                return new List<ServerInfo>();
            }
        }

        // ── Ping / heartbeat ──────────────────────────────────────
        public async Task<bool> PingAsync(string publicIp, int port)
        {
            try
            {
                var resp = await PostJsonAsync("?action=ping", new { ip = publicIp, port = port });
                return resp != null && resp.ContainsKey("success");
            }
            catch { return false; }
        }

        // ── Mettre à jour le nombre de joueurs ────────────────────
        public async Task UpdatePlayersAsync(string publicIp, int port, int currentPlayers)
        {
            try
            {
                await PostJsonAsync("?action=update_players",
                    new { ip = publicIp, port = port, current_players = currentPlayers });
            }
            catch (Exception ex) { Log("UpdatePlayers: " + ex.Message); }
        }

        // ── Retirer le salon ──────────────────────────────────────
        public async Task RemoveServerAsync(string publicIp, int port)
        {
            try
            {
                await PostJsonAsync("?action=remove", new { ip = publicIp, port = port });
            }
            catch (Exception ex) { Log("RemoveServer: " + ex.Message); }
        }

        // ════════════════════════════════════════════════════════
        //  LEADERBOARD (en ligne)
        // ════════════════════════════════════════════════════════
        public async Task<bool> RecordWinAsync(string pseudo, string gameType)
        {
            try
            {
                var resp = await PostJsonAsync("?action=lb_record_win", new { pseudo = pseudo, game_type = gameType });
                return resp != null && resp.ContainsKey("success");
            }
            catch (Exception ex) { Log("RecordWin: " + ex.Message); return false; }
        }

        public async Task<bool> RecordSnakeScoreAsync(string pseudo, int score)
        {
            try
            {
                var resp = await PostJsonAsync("?action=lb_record_snake", new { pseudo = pseudo, score = score });
                return resp != null && resp.ContainsKey("success");
            }
            catch (Exception ex) { Log("RecordSnake: " + ex.Message); return false; }
        }

        /// <summary>
        /// Retourne un dictionnaire: game_type -> liste d'items {pseudo, value}.
        /// </summary>
        public async Task<Dictionary<string, List<Dictionary<string, object>>>> GetLeaderboardAsync()
        {
            try
            {
                string raw = await _http.GetStringAsync(API_URL + "?action=lb_list");
                return _json.Deserialize<Dictionary<string, List<Dictionary<string, object>>>>(raw)
                       ?? new Dictionary<string, List<Dictionary<string, object>>>();
            }
            catch (Exception ex)
            {
                Log("GetLeaderboard: " + ex.Message);
                return new Dictionary<string, List<Dictionary<string, object>>>();
            }
        }

        // ── Helper HTTP POST JSON ─────────────────────────────────
        private async Task<Dictionary<string, object>> PostJsonAsync(string queryString, object payload)
        {
            string json    = _json.Serialize(payload);
            var    content = new StringContent(json, Encoding.UTF8, "application/json");
            var    resp    = await _http.PostAsync(API_URL + queryString, content);

            if (!resp.IsSuccessStatusCode) return null;

            string body = await resp.Content.ReadAsStringAsync();
            return _json.Deserialize<Dictionary<string, object>>(body);
        }

        private void Log(string msg) => System.Diagnostics.Debug.WriteLine("[LobbyClient] " + msg);
    }
}