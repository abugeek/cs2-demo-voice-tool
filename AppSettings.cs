using System;
using System.IO;
using System.Text.Json;

namespace DemoPulse
{
    public class AppSettings
    {
        public string ConfigFileName { get; set; } = "demopulse";
        public string Cs2CfgFolder { get; set; } = "";
        public string KeyBindT { get; set; } = "b";
        public string KeyBindCT { get; set; } = "n";
        public string KeyBindAll { get; set; } = "v";
        public string KeyBindMute { get; set; } = "m";

        public string KeyBindSpeedUp { get; set; } = "shift";
        public string KeyBindSlowMo { get; set; } = "ctrl";
        public string KeyBindPause { get; set; } = "space";
        public string KeyBindResetSpeed { get; set; } = "r";

        public bool AutoSaveToCs2 { get; set; } = true;
        public bool AutoCopyDemoToCs2 { get; set; } = true;

        private static readonly string SettingsDir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "DemoPulse"
        );

        private static readonly string SettingsFilePath = Path.Combine(SettingsDir, "settings.json");

        public static AppSettings Load()
        {
            try
            {
                if (File.Exists(SettingsFilePath))
                {
                    string json = File.ReadAllText(SettingsFilePath);
                    var settings = JsonSerializer.Deserialize<AppSettings>(json);
                    if (settings != null)
                    {
                        settings.SanitizeAll();
                        if (string.IsNullOrWhiteSpace(settings.Cs2CfgFolder))
                            settings.Cs2CfgFolder = AutoDetectCs2CfgFolder();
                        return settings;
                    }
                }
            }
            catch
            {
                // Fall back to default
            }

            var defaultSettings = new AppSettings();
            defaultSettings.Cs2CfgFolder = AutoDetectCs2CfgFolder();
            defaultSettings.SanitizeAll();
            defaultSettings.Save();
            return defaultSettings;
        }

        public void Save()
        {
            SanitizeAll();
            if (!Directory.Exists(SettingsDir))
                Directory.CreateDirectory(SettingsDir);

            string json = JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(SettingsFilePath, json);
        }

        public static string SanitizeKeyBind(string? rawKey, string defaultKey)
        {
            if (string.IsNullOrWhiteSpace(rawKey)) return defaultKey;
            string key = rawKey.Trim().ToLowerInvariant();
            key = System.Text.RegularExpressions.Regex.Replace(key, @"[^a-z0-9_]", "");
            if (string.IsNullOrWhiteSpace(key) || key.Length > 15) return defaultKey;
            return key;
        }

        public void EnsureUniqueKeyBindings()
        {
            var fallbackPool = new[] { "b", "n", "v", "m", "shift", "ctrl", "space", "r", "f5", "f6", "f7", "f8", "f9", "f10" };
            var used = new HashSet<string>();

            string ResolveKey(string currentKey, string defaultKey)
            {
                if (!used.Contains(currentKey))
                {
                    used.Add(currentKey);
                    return currentKey;
                }

                foreach (var fb in fallbackPool)
                {
                    if (!used.Contains(fb))
                    {
                        used.Add(fb);
                        return fb;
                    }
                }
                return defaultKey;
            }

            KeyBindT = ResolveKey(KeyBindT, "b");
            KeyBindCT = ResolveKey(KeyBindCT, "n");
            KeyBindAll = ResolveKey(KeyBindAll, "v");
            KeyBindMute = ResolveKey(KeyBindMute, "m");
            KeyBindSpeedUp = ResolveKey(KeyBindSpeedUp, "shift");
            KeyBindSlowMo = ResolveKey(KeyBindSlowMo, "ctrl");
            KeyBindPause = ResolveKey(KeyBindPause, "space");
            KeyBindResetSpeed = ResolveKey(KeyBindResetSpeed, "r");
        }

        public static string SanitizeConfigFileName(string? rawName)
        {
            if (string.IsNullOrWhiteSpace(rawName)) return "demopulse";

            string fileName = Path.GetFileName(rawName.Trim());
            if (fileName.EndsWith(".cfg", StringComparison.OrdinalIgnoreCase))
            {
                fileName = fileName.Substring(0, fileName.Length - 4);
            }

            foreach (char invalidChar in Path.GetInvalidFileNameChars())
            {
                fileName = fileName.Replace(invalidChar.ToString(), "");
            }
            fileName = fileName.Replace("/", "").Replace("\\", "").Replace("..", "");

            fileName = fileName.Trim();
            if (string.IsNullOrWhiteSpace(fileName)) return "demopulse";
            return fileName;
        }

        public void SanitizeAll()
        {
            KeyBindT = SanitizeKeyBind(KeyBindT, "b");
            KeyBindCT = SanitizeKeyBind(KeyBindCT, "n");
            KeyBindAll = SanitizeKeyBind(KeyBindAll, "v");
            KeyBindMute = SanitizeKeyBind(KeyBindMute, "m");
            KeyBindSpeedUp = SanitizeKeyBind(KeyBindSpeedUp, "shift");
            KeyBindSlowMo = SanitizeKeyBind(KeyBindSlowMo, "ctrl");
            KeyBindPause = SanitizeKeyBind(KeyBindPause, "space");
            KeyBindResetSpeed = SanitizeKeyBind(KeyBindResetSpeed, "r");
            ConfigFileName = SanitizeConfigFileName(ConfigFileName);

            EnsureUniqueKeyBindings();
        }

        public static string AutoDetectCs2CfgFolder()
        {
            string[] possiblePaths = new[]
            {
                @"C:\Program Files (x86)\Steam\steamapps\common\Counter-Strike Global Offensive\game\csgo\cfg",
                @"C:\Program Files\Steam\steamapps\common\Counter-Strike Global Offensive\game\csgo\cfg",
                @"D:\SteamLibrary\steamapps\common\Counter-Strike Global Offensive\game\csgo\cfg",
                @"E:\SteamLibrary\steamapps\common\Counter-Strike Global Offensive\game\csgo\cfg"
            };

            foreach (string p in possiblePaths)
            {
                if (Directory.Exists(p))
                    return p;
            }

            return "";
        }

        public string GetCs2GameFolder()
        {
            if (!string.IsNullOrWhiteSpace(Cs2CfgFolder) && Directory.Exists(Cs2CfgFolder))
            {
                var parent = Directory.GetParent(Cs2CfgFolder.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar));
                if (parent != null && parent.Exists && parent.Name.Equals("csgo", StringComparison.OrdinalIgnoreCase))
                {
                    return parent.FullName;
                }
            }

            string cfg = AutoDetectCs2CfgFolder();
            if (!string.IsNullOrWhiteSpace(cfg))
            {
                var parent = Directory.GetParent(cfg.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar));
                if (parent != null && parent.Exists) return parent.FullName;
            }

            return "";
        }
    }
}
