using System;
using System.IO;
using System.Reflection;
using System.Text.Json;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using UnityEngine;
using ZeroMenu.Functions;

namespace ZeroMenu.Functions
{
    public static class Config
    {
        public static KeyCode MenuKey = KeyCode.Delete;
        public static string MenuHtmlColor = "#FFFFFF";
        public static bool OpenOnMouse = false;
        public static bool AutoLoadProfile = false;
        public static bool LogChatMessages = false;

        private static readonly ManualLogSource Log = BepInEx.Logging.Logger.CreateLogSource("ZeroMenu.Config");
        private static string ConfigPath => Path.Combine(Paths.ConfigPath, "ZeroMenu.json");
        private static string ProfilePath => Path.Combine(Paths.ConfigPath, "ZeroMenu_Profile.json");

        public class ConfigData
        {
            public string MenuKey { get; set; } = "Delete";
            public string MenuHtmlColor { get; set; } = "#FFFFFF";
            public bool OpenOnMouse { get; set; } = false;
            public bool AutoLoadProfile { get; set; } = false;
            public bool LogChatMessages { get; set; } = false;

            public bool SpoofGuestMode { get; set; } = false;
            public bool SpoofFriendCode { get; set; } = false;
            public string SpoofLevel { get; set; } = "1";
            public bool SpoofDeviceId { get; set; } = false;
            public bool SpoofAprilFools { get; set; } = false;
            public string PlatformName { get; set; } = "TESTNAME";
            public bool SpoofVersion { get; set; } = false;
            public int SpoofedVersion { get; set; } = 50632950;
            public bool UseModdedProtocol { get; set; } = false;
            public int SpoofedPlatform { get; set; } = 1;
        }

        public static void SaveConfig(bool isProfile = false)
        {
            try
            {
                var data = new ConfigData
                {
                    MenuKey = MenuKey.ToString(),
                    MenuHtmlColor = MenuHtmlColor,
                    OpenOnMouse = OpenOnMouse,
                    AutoLoadProfile = AutoLoadProfile,
                    LogChatMessages = LogChatMessages,
                    SpoofGuestMode = Spoof.GuestMode,
                    SpoofFriendCode = Spoof.FriendCodeSpoof,
                    SpoofLevel = Spoof.SpoofLevelStr,
                    SpoofDeviceId = Passive.SpoofDeviceId,
                    SpoofAprilFools = Passive.SpoofAprilFoolsDate,
                    PlatformName = Spoof.PlatformNameStr,
                    SpoofVersion = Spoof.ShouldSpoofVersion,
                    SpoofedVersion = Spoof.SpoofedVersion,
                    UseModdedProtocol = Spoof.UseModdedProtocol,
                    SpoofedPlatform = (int)Spoof.SpoofedPlatform
                };
                string json = JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(isProfile ? ProfilePath : ConfigPath, json);
                ZeroMenu.Plugin.notifications.Send("Config", isProfile ? "Profile saved!" : "Config saved!", 3f);
            }
            catch (Exception e)
            {
                Log.LogError($"Failed to save config: {e.Message}");
            }
        }

        public static void LoadConfig(bool isProfile = false)
        {
            try
            {
                string path = isProfile ? ProfilePath : ConfigPath;
                if (!File.Exists(path)) return;

                string json = File.ReadAllText(path);
                var data = JsonSerializer.Deserialize<ConfigData>(json);
                if (data == null) return;

                MenuKey = (KeyCode)Enum.Parse(typeof(KeyCode), data.MenuKey, true);
                MenuHtmlColor = data.MenuHtmlColor;
                OpenOnMouse = data.OpenOnMouse;
                AutoLoadProfile = data.AutoLoadProfile;
                LogChatMessages = data.LogChatMessages;
                Spoof.GuestMode = data.SpoofGuestMode;
                Spoof.FriendCodeSpoof = data.SpoofFriendCode;
                Spoof.SpoofLevelStr = data.SpoofLevel;
                Passive.SpoofDeviceId = data.SpoofDeviceId;
                Passive.SpoofAprilFoolsDate = data.SpoofAprilFools;
                Spoof.PlatformNameStr = data.PlatformName;
                Spoof.ShouldSpoofVersion = data.SpoofVersion;
                Spoof.SpoofedVersion = data.SpoofedVersion;
                Spoof.UseModdedProtocol = data.UseModdedProtocol;
                Spoof.SpoofedPlatform = (Platforms)data.SpoofedPlatform;

                ZeroMenu.Plugin.notifications.Send("Config", isProfile ? "Profile loaded!" : "Config loaded!", 3f);
            }
            catch (Exception e)
            {
                Log.LogError($"Failed to load config: {e.Message}");
            }
        }

        public static void OpenConfigFile()
        {
            string path = Path.Combine(Paths.ConfigPath, "ZeroMenu.json");
            if (!File.Exists(path))
            {
                ZeroMenu.Plugin.notifications.Send("Config", "Config file doesn't exist yet. Save it first!", 3f);
                return;
            }
            try
            {
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = path,
                    UseShellExecute = true
                });
            }
            catch (Exception e)
            {
                Log.LogError($"Failed to open config: {e.Message}");
            }
        }
    }
}
