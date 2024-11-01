﻿using System;
using System.IO;
using System.Text.Json;

namespace Synapse_Z_V3.Settings
{
    public static class GlobalSettings
    {
        public static bool Minimap { get; set; } = true;
        public static bool Transparency { get; set; } = false;
        public static bool Topmost { get; set; } = true;
        public static bool Randomize { get; set; } = true;
        public static bool AutoInject { get; set; } = false;
        public static bool TabConfirmation { get; set; } = true; // Added TabConfirmation property

        private static readonly string settingsFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "bin", "settings.json");

        private class SettingsModel
        {
            public bool Minimap { get; set; }
            public bool Transparency { get; set; }
            public bool Topmost { get; set; }
            public bool Randomize { get; set; }
            public bool AutoInject { get; set; }
            public bool TabConfirmation { get; set; } // Added TabConfirmation property
        }

        static GlobalSettings()
        {
            LoadSettings();
        }

        public static void SaveSettings()
        {
            string binDirectory = Path.GetDirectoryName(settingsFilePath);
            if (!Directory.Exists(binDirectory))
            {
                Directory.CreateDirectory(binDirectory);
            }

            var settings = new SettingsModel
            {
                Minimap = Minimap,
                Transparency = Transparency,
                Topmost = Topmost,
                Randomize = Randomize,
                AutoInject = AutoInject,
                TabConfirmation = TabConfirmation // Save TabConfirmation setting
            };

            string json = JsonSerializer.Serialize(settings, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(settingsFilePath, json);
        }

        public static void LoadSettings()
        {
            if (File.Exists(settingsFilePath))
            {
                string json = File.ReadAllText(settingsFilePath);
                var settings = JsonSerializer.Deserialize<SettingsModel>(json);

                if (settings != null)
                {
                    Minimap = settings.Minimap;
                    Transparency = settings.Transparency;
                    Topmost = settings.Topmost;
                    Randomize = settings.Randomize;
                    AutoInject = settings.AutoInject;
                    TabConfirmation = settings.TabConfirmation; // Load TabConfirmation from settings
                }
            }
        }
    }
}
