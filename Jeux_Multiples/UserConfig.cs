using System;
using System.IO;
using System.Web.Script.Serialization;

namespace Jeux_Multiples
{
    public class UserConfig
    {
        public string Pseudo { get; set; }

        /// <summary>
        /// Repo GitHub au format "owner/repo" (optionnel).
        /// Exemple: "CuencaGames/Jeux_Multiples"
        /// </summary>
        public string UpdateRepo { get; set; }

        /// <summary>
        /// Nom exact de l'asset à télécharger sur la release (optionnel).
        /// Exemple: "Jeux_Multiples.exe" ou "Jeux_Multiples.zip"
        /// </summary>
        public string UpdateAssetName { get; set; }

        public static string ConfigDir =>
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "CuencaGames");

        public static string ConfigPath => Path.Combine(ConfigDir, "config.json");

        private static readonly object _lock = new object();
        private static readonly JavaScriptSerializer _json = new JavaScriptSerializer();

        public static UserConfig Load()
        {
            lock (_lock)
            {
                try
                {
                    if (!File.Exists(ConfigPath)) return new UserConfig();
                    string raw = File.ReadAllText(ConfigPath);
                    return _json.Deserialize<UserConfig>(raw) ?? new UserConfig();
                }
                catch
                {
                    return new UserConfig();
                }
            }
        }

        public static void Save(UserConfig cfg)
        {
            if (cfg == null) return;
            lock (_lock)
            {
                try
                {
                    if (!Directory.Exists(ConfigDir)) Directory.CreateDirectory(ConfigDir);
                    File.WriteAllText(ConfigPath, _json.Serialize(cfg));
                }
                catch { }
            }
        }
    }
}

