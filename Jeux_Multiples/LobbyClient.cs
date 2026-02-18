using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;

namespace Jeux_Multiples
{
    public class ServerInfo
    {
        public int id { get; set; }
        public string server_name { get; set; }
        public string ip_address { get; set; }
        public string local_ip { get; set; }
        public int port { get; set; }
        public string created_at { get; set; }
    }

    public class LobbyClient
    {
        // URL publique de l'API Lobby (mettre à jour si changé)
        private const string API_URL = "https://cuencamathieu.com/lobby_api.php";
        
        private static readonly HttpClient client = new HttpClient();
        private static readonly JavaScriptSerializer serializer = new JavaScriptSerializer();

        public async Task<string> GetPublicIP()
        {
            try
            {
                return await client.GetStringAsync("https://api.ipify.org");
            }
            catch 
            {
                return null;
            }
        }

        public async Task<int?> RegisterServer(string name, string publicIp, string localIp, int port)
        {
            try
            {
            var data = new { name = name, ip = publicIp, local_ip = localIp, port = port };
                var json = serializer.Serialize(data);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await client.PostAsync(API_URL + "?action=host", content);
                if (response.IsSuccessStatusCode)
                {
                    string resJson = await response.Content.ReadAsStringAsync();
                    var dict = serializer.Deserialize<Dictionary<string, object>>(resJson);
                    
                    if (dict.ContainsKey("id"))
                        return Convert.ToInt32(dict["id"]);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Lobby Register Error: " + ex.Message);
            }
            return null;
        }

        public async Task<List<ServerInfo>> GetServers()
        {
            try
            {
                var response = await client.GetStringAsync(API_URL + "?action=list");
                return serializer.Deserialize<List<ServerInfo>>(response);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Lobby List Error: " + ex.Message);
                return new List<ServerInfo>();
            }
        }

        public async Task RemoveServer(string ip, int port)
        {
            try
            {
                var data = new { ip = ip, port = port };
                var json = serializer.Serialize(data);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                await client.PostAsync(API_URL + "?action=remove", content);
            }
            catch { }
        }
    }
}
