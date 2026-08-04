using System;
using System.IO;
using Tidepool.Domain;
using UnityEngine;

namespace Tidepool.Runtime
{
    public static class TidepoolSettingsService
    {
        private const string SettingsFileName = "settings.json";

        private static SettingsData settings;

        public static bool Muted
        {
            get
            {
                EnsureLoaded();
                return settings.muted;
            }
        }

        public static float MasterVolume
        {
            get
            {
                EnsureLoaded();
                return settings.masterVolume;
            }
        }

        private static string SettingsPath => Path.Combine(Application.persistentDataPath, SettingsFileName);

        public static void SetMuted(bool muted)
        {
            EnsureLoaded();
            if (settings.muted == muted)
            {
                ApplyGlobalAudio();
                return;
            }

            settings.muted = muted;
            Save();
            ApplyGlobalAudio();
        }

        public static void SetMasterVolume(float masterVolume)
        {
            EnsureLoaded();
            float clampedVolume = Mathf.Clamp01(masterVolume);
            if (Mathf.Approximately(settings.masterVolume, clampedVolume))
            {
                ApplyGlobalAudio();
                return;
            }

            settings.masterVolume = clampedVolume;
            Save();
            ApplyGlobalAudio();
        }

        public static void ApplyGlobalAudio()
        {
            EnsureLoaded();
            AudioListener.volume = settings.muted ? 0f : settings.masterVolume;
        }

        private static void EnsureLoaded()
        {
            if (settings != null)
            {
                return;
            }

            Load();
        }

        private static void Load()
        {
            if (!File.Exists(SettingsPath))
            {
                settings = new SettingsData();
                ApplyGlobalAudio();
                return;
            }

            try
            {
                string json = File.ReadAllText(SettingsPath);
                settings = string.IsNullOrWhiteSpace(json)
                    ? new SettingsData()
                    : JsonUtility.FromJson<SettingsData>(json);
                NormalizeLoadedSettings();
            }
            catch (Exception exception)
            {
                Debug.LogWarning($"Could not load Tidepool settings. Using defaults. {exception.Message}");
                settings = new SettingsData();
            }

            ApplyGlobalAudio();
        }

        private static void Save()
        {
            Directory.CreateDirectory(Path.GetDirectoryName(SettingsPath));
            string json = JsonUtility.ToJson(settings, true);
            File.WriteAllText(SettingsPath, json);
        }

        private static void NormalizeLoadedSettings()
        {
            if (settings == null || settings.schemaVersion <= 0)
            {
                settings = new SettingsData();
                return;
            }

            settings.masterVolume = Mathf.Clamp01(settings.masterVolume);
        }
    }
}
