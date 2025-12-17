using System.IO;
using System.Text.Json;

namespace ChatClient
{
    public static class AppSettings
    {
        private static string _settingsFile = "chatclient_settings.json";
        private static SettingsData _settings;

        public class SettingsData
        {
            public string ServerIP { get; set; } = "127.0.0.1";
            public bool RememberIP { get; set; } = true;
            public string LastUsername { get; set; } = "";
        }

        static AppSettings()
        {
            LoadSettings();
        }

        public static string ServerIP
        {
            get => _settings.ServerIP;
            set
            {
                _settings.ServerIP = value;
                SaveSettings();
            }
        }

        public static bool RememberIP
        {
            get => _settings.RememberIP;
            set
            {
                _settings.RememberIP = value;
                SaveSettings();
            }
        }

        public static string LastUsername
        {
            get => _settings.LastUsername;
            set
            {
                _settings.LastUsername = value;
                SaveSettings();
            }
        }

        private static void LoadSettings()
        {
            try
            {
                if (File.Exists(_settingsFile))
                {
                    var json = File.ReadAllText(_settingsFile);
                    _settings = JsonSerializer.Deserialize<SettingsData>(json) ?? new SettingsData();
                }
                else
                {
                    _settings = new SettingsData();
                }
            }
            catch
            {
                _settings = new SettingsData();
            }
        }

        private static void SaveSettings()
        {
            try
            {
                var json = JsonSerializer.Serialize(_settings, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(_settingsFile, json);
            }
            catch { }
        }
    }
}