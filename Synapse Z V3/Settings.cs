using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace Synapse_Z_V3.Settings
{
    public static class GlobalSettings
    {
        public static bool Minimap { get; set; } = true;
        public static bool Transparency { get; set; } = false;
        public static bool Topmost { get; set; } = true; // Added Topmost property
        public static bool Randomize { get; set; } = true; // Added Randomize property
        public static bool AutoInject { get; set; } = false; // Added AutoInject property

        private static readonly string settingsFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "bin", "settings.json");

        // Updated SettingsModel to include Topmost, Randomize, and AutoInject
        private class SettingsModel
        {
            public bool Minimap { get; set; }
            public bool Transparency { get; set; }
            public bool Topmost { get; set; }
            public bool Randomize { get; set; } // Added Randomize property
            public bool AutoInject { get; set; } // Added AutoInject property
        }

        static GlobalSettings()
        {
            LoadSettings();
        }

        public static void SaveSettings()
        {
            // Ensure the 'bin' directory exists
            string binDirectory = Path.GetDirectoryName(settingsFilePath);
            if (!Directory.Exists(binDirectory))
            {
                Directory.CreateDirectory(binDirectory);
            }

            // Create an instance of SettingsModel with current settings
            var settings = new SettingsModel
            {
                Minimap = Minimap,
                Transparency = Transparency,
                Topmost = Topmost, // Save Topmost setting
                Randomize = Randomize, // Save Randomize setting
                AutoInject = AutoInject // Save AutoInject setting
            };

            // Serialize the settings to JSON and save it to the file
            string json = JsonSerializer.Serialize(settings, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(settingsFilePath, json);
        }

        public static void LoadSettings()
        {
            // Check if the settings file exists
            if (File.Exists(settingsFilePath))
            {
                // Read the JSON from the file and deserialize it into SettingsModel
                string json = File.ReadAllText(settingsFilePath);
                var settings = JsonSerializer.Deserialize<SettingsModel>(json);

                if (settings != null)
                {
                    Minimap = settings.Minimap;
                    Transparency = settings.Transparency; // Load Transparency from settings
                    Topmost = settings.Topmost; // Load Topmost from settings
                    Randomize = settings.Randomize; // Load Randomize from settings
                    AutoInject = settings.AutoInject; // Load AutoInject from settings
                }
            }
        }
    }
}
