using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Jeux_Multiples
{
    /// <summary>
    /// Classement local : victoires par mode de jeu et pseudo, score Snake (solo).
    /// Fichier : %AppData%\CuencaGames\leaderboard.txt (lignes "gameType|pseudo|value").
    /// </summary>
    public static class Leaderboard
    {
        private static readonly string Dir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "CuencaGames");
        private static readonly string FilePath = Path.Combine(Dir, "leaderboard.txt");
        private static readonly object _lock = new object();

        private static Dictionary<string, Dictionary<string, int>> Load()
        {
            var data = new Dictionary<string, Dictionary<string, int>>(StringComparer.OrdinalIgnoreCase);
            if (!File.Exists(FilePath)) return data;
            try
            {
                foreach (var line in File.ReadAllLines(FilePath))
                {
                    if (string.IsNullOrWhiteSpace(line)) continue;
                    var parts = line.Split('|');
                    if (parts.Length != 3) continue;
                    string game = parts[0].Trim();
                    string pseudo = parts[1].Trim();
                    if (!int.TryParse(parts[2], out int val)) continue;
                    if (!data.ContainsKey(game)) data[game] = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
                    data[game][pseudo] = val;
                }
            }
            catch { }
            return data;
        }

        private static void Save(Dictionary<string, Dictionary<string, int>> data)
        {
            try
            {
                if (!Directory.Exists(Dir)) Directory.CreateDirectory(Dir);
                var lines = new List<string>();
                foreach (var kv in data.OrderBy(k => k.Key))
                    foreach (var kv2 in kv.Value.OrderBy(k => k.Key))
                        lines.Add($"{kv.Key}|{kv2.Key}|{kv2.Value}");
                File.WriteAllLines(FilePath, lines);
            }
            catch { }
        }

        /// <summary>Enregistre une victoire pour un jeu multijoueur (Puissance4, Dames, MortPion, etc.).</summary>
        public static void RecordVictory(string gameType, string pseudo)
        {
            if (string.IsNullOrWhiteSpace(pseudo)) return;
            lock (_lock)
            {
                var data = Load();
                if (!data.ContainsKey(gameType)) data[gameType] = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
                if (!data[gameType].ContainsKey(pseudo)) data[gameType][pseudo] = 0;
                data[gameType][pseudo]++;
                Save(data);
            }
        }

        /// <summary>Enregistre le meilleur score Snake (solo) pour un pseudo.</summary>
        public static void RecordSnakeScore(string pseudo, int score)
        {
            if (string.IsNullOrWhiteSpace(pseudo) || score <= 0) return;
            lock (_lock)
            {
                var data = Load();
                const string key = "Snake";
                if (!data.ContainsKey(key)) data[key] = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
                if (!data[key].ContainsKey(pseudo) || data[key][pseudo] < score)
                    data[key][pseudo] = score;
                Save(data);
            }
        }

        /// <summary>Retourne le classement : par jeu, puis liste (pseudo, valeur) triée par valeur décroissante.</summary>
        public static Dictionary<string, List<(string pseudo, int value)>> GetAll()
        {
            lock (_lock)
            {
                var data = Load();
                var result = new Dictionary<string, List<(string, int)>>(StringComparer.OrdinalIgnoreCase);
                foreach (var kv in data)
                    result[kv.Key] = kv.Value.Select(p => (p.Key, p.Value)).OrderByDescending(x => x.Item2).ToList();
                return result;
            }
        }
    }
}
