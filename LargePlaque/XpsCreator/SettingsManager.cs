using System;
using System.IO;
using System.Text.Json;

namespace XpsCreator
{
    public class AppSettings
    {
        public string LastDatabasePath { get; set; } = string.Empty;
    }

    public static class SettingsManager
    {
        private static readonly string SettingsFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "appsettings.json");

        public static AppSettings LoadSettings()
        {
            if (File.Exists(SettingsFilePath))
            {
                try
                {
                    string json = File.ReadAllText(SettingsFilePath);
                    return JsonSerializer.Deserialize<AppSettings>(json) ?? new AppSettings();
                }
                catch
                {
                    return new AppSettings();
                }
            }
            return new AppSettings();
        }

        public static void SaveSettings(AppSettings settings)
        {
            try
            {
                string json = JsonSerializer.Serialize(settings, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(SettingsFilePath, json);
            }
            catch
            {
                // Ignore save errors
            }
        }
    }
}
