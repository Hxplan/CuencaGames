using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using System.Windows.Forms;

namespace Jeux_Multiples
{
    public static class UpdateManager
    {
        private class GithubRelease
        {
            public string tag_name { get; set; }
            public GithubAsset[] assets { get; set; }
        }

        private class GithubAsset
        {
            public string name { get; set; }
            public string browser_download_url { get; set; }
        }

        private static readonly JavaScriptSerializer _json = new JavaScriptSerializer();

        public static async Task CheckAndPromptAsync(IWin32Window owner)
        {
            try
            {
                // TLS 1.2 (important sur .NET Framework)
                ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls12;

                var cfg = UserConfig.Load() ?? new UserConfig();
                if (string.IsNullOrWhiteSpace(cfg.UpdateRepo))
                {
                    // Valeur par défaut (modifiable via config.json)
                    cfg.UpdateRepo = "CuencaGames/CuencaGames";
                }
                if (string.IsNullOrWhiteSpace(cfg.UpdateAssetName))
                {
                    cfg.UpdateAssetName = "Jeux_Multiples.exe";
                }
                UserConfig.Save(cfg);

                Version current = typeof(UpdateManager).Assembly.GetName().Version ?? new Version(1, 0, 0, 0);
                Version latest = await GetLatestReleaseVersionAsync(cfg.UpdateRepo);
                if (latest == null) return;
                if (latest <= current) return;

                var r = MessageBox.Show(owner,
                    $"Une mise à jour est disponible.\n\nVersion actuelle : {current}\nNouvelle version : {latest}\n\nTélécharger et installer maintenant ?",
                    "Mise à jour",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Information);
                if (r != DialogResult.Yes) return;

                string url = await GetLatestAssetUrlAsync(cfg.UpdateRepo, cfg.UpdateAssetName);
                if (string.IsNullOrWhiteSpace(url))
                {
                    MessageBox.Show(owner,
                        $"Impossible de trouver l'asset '{cfg.UpdateAssetName}' sur la dernière release GitHub.\n" +
                        $"Modifie '%AppData%\\CuencaGames\\config.json' si le nom est différent.",
                        "Mise à jour",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                    return;
                }

                string updateDir = Path.Combine(UserConfig.ConfigDir, "updates");
                Directory.CreateDirectory(updateDir);

                string newFile = Path.Combine(updateDir, cfg.UpdateAssetName);
                await DownloadAsync(url, newFile);

                ScheduleReplaceOnExit(newFile);

                MessageBox.Show(owner,
                    "La mise à jour est prête.\nL'application va se fermer pour terminer l'installation.",
                    "Mise à jour",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                Application.Exit();
            }
            catch
            {
                // best-effort: pas de popup d'erreur pour l'update auto
            }
        }

        private static async Task<Version> GetLatestReleaseVersionAsync(string repo)
        {
            var rel = await GetLatestReleaseAsync(repo);
            if (rel == null) return null;
            string tag = (rel.tag_name ?? "").Trim();
            if (tag.StartsWith("v", StringComparison.OrdinalIgnoreCase)) tag = tag.Substring(1);
            if (Version.TryParse(tag, out var v)) return v;
            return null;
        }

        private static async Task<string> GetLatestAssetUrlAsync(string repo, string assetName)
        {
            var rel = await GetLatestReleaseAsync(repo);
            if (rel?.assets == null) return null;
            foreach (var a in rel.assets)
            {
                if (a == null) continue;
                if (string.Equals(a.name, assetName, StringComparison.OrdinalIgnoreCase))
                    return a.browser_download_url;
            }
            return null;
        }

        private static async Task<GithubRelease> GetLatestReleaseAsync(string repo)
        {
            using (var http = new HttpClient())
            {
                http.Timeout = TimeSpan.FromSeconds(8);
                http.DefaultRequestHeaders.UserAgent.ParseAdd("CuencaGames-Updater");
                string raw = await http.GetStringAsync($"https://api.github.com/repos/{repo}/releases/latest");
                return _json.Deserialize<GithubRelease>(raw);
            }
        }

        private static async Task DownloadAsync(string url, string destPath)
        {
            using (var http = new HttpClient())
            {
                http.Timeout = TimeSpan.FromSeconds(30);
                http.DefaultRequestHeaders.UserAgent.ParseAdd("CuencaGames-Updater");
                using (var resp = await http.GetAsync(url))
                {
                    resp.EnsureSuccessStatusCode();
                    using (var fs = new FileStream(destPath, FileMode.Create, FileAccess.Write, FileShare.None))
                    {
                        await resp.Content.CopyToAsync(fs);
                    }
                }
            }
        }

        private static void ScheduleReplaceOnExit(string downloadedFile)
        {
            string currentExe = Process.GetCurrentProcess().MainModule.FileName;
            int pid = Process.GetCurrentProcess().Id;

            string batDir = Path.GetDirectoryName(downloadedFile) ?? UserConfig.ConfigDir;
            string batPath = Path.Combine(batDir, "apply_update.bat");

            // Script qui attend la fin du process, remplace l'exe, relance, puis s'auto-supprime.
            string bat = $@"@echo off
setlocal
set PID={pid}
:loop
tasklist /FI ""PID eq %PID%"" | find ""%PID%"" >nul
if not errorlevel 1 (
  timeout /t 1 >nul
  goto loop
)
copy /Y ""{downloadedFile}"" ""{currentExe}"" >nul
start """" ""{currentExe}""
del ""%~f0""
";
            File.WriteAllText(batPath, bat);

            Process.Start(new ProcessStartInfo
            {
                FileName = "cmd.exe",
                Arguments = $"/c start \"\" /min \"{batPath}\"",
                CreateNoWindow = true,
                UseShellExecute = false,
                WindowStyle = ProcessWindowStyle.Hidden
            });
        }
    }
}

