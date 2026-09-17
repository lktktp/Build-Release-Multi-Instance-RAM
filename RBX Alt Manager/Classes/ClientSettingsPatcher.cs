using System;
using Microsoft.Win32;
using Newtonsoft.Json.Linq;
using System.IO;

namespace RBX_Alt_Manager.Classes
{
    public static class ClientSettingsPatcher
    {
        public static void PatchSettings()
        {
            DirectoryInfo VersionFolder = null;

            try
            {
                using (var key = Registry.CurrentUser.OpenSubKey(@"Software\Classes\roblox-player\shell\open\command") ?? Registry.ClassesRoot.OpenSubKey(@"roblox-player\shell\open\command"))
                {
                    if (key != null && key.GetValue("") is string cmd && !string.IsNullOrEmpty(cmd))
                    {
                        var match = System.Text.RegularExpressions.Regex.Match(cmd, "\"([^\"]+RobloxPlayerBeta\\.exe)\"");
                        if (match.Success && File.Exists(match.Groups[1].Value))
                        {
                            VersionFolder = new DirectoryInfo(Path.GetDirectoryName(match.Groups[1].Value));
                        }
                    }
                }
            }
            catch { }

            if (VersionFolder == null || !VersionFolder.Exists)
            {
                string localApp = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                string versionsDir = Path.Combine(localApp, @"Roblox\Versions");
                if (Directory.Exists(versionsDir))
                {
                    foreach (string dir in Directory.GetDirectories(versionsDir))
                    {
                        if (File.Exists(Path.Combine(dir, "RobloxPlayerBeta.exe")))
                        {
                            VersionFolder = new DirectoryInfo(dir);
                            break;
                        }
                    }
                }
            }

            if (VersionFolder == null || !VersionFolder.Exists) { Program.Logger.Warn("Can't patch ClientAppSettings, Roblox version folder not found"); return; }
            if (!File.Exists(Path.Combine(VersionFolder.FullName, "RobloxPlayerBeta.exe"))) { Program.Logger.Warn("Can't patch ClientAppSettings, RobloxPlayerBeta.exe not found"); return; }

            DirectoryInfo SettingsFolder = new DirectoryInfo(Path.Combine(VersionFolder.FullName, "ClientSettings"));

            if (!SettingsFolder.Exists) SettingsFolder.Create();

            string CustomFN = AccountManager.General.Exists("CustomClientSettings") ? AccountManager.General.Get<string>("CustomClientSettings") : string.Empty;
            string SettingsFN = Path.Combine(SettingsFolder.FullName, "ClientAppSettings.json");

            if (!string.IsNullOrEmpty(CustomFN) && File.Exists(CustomFN))
                File.Copy(CustomFN, SettingsFN);
            else if (AccountManager.General.Get<bool>("UnlockFPS"))
            {
                if (File.Exists(SettingsFN) && File.ReadAllText(SettingsFN).TryParseJson(out JObject Settings))
                {
                    Settings["DFIntTaskSchedulerTargetFps"] = AccountManager.General.Exists("MaxFPSValue") ? AccountManager.General.Get<int>("MaxFPSValue") : 240;
                    File.WriteAllText(SettingsFN, Settings.ToString(Newtonsoft.Json.Formatting.None));
                }
                else
                    File.WriteAllText(SettingsFN, "{\"DFIntTaskSchedulerTargetFps\":240}");
            }
        }
    }
}