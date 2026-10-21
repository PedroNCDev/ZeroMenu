using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using AmongUs.GameOptions;
using ZeroMenu.Functions;
using ZeroMenu.UI;
using Il2CppInterop.Runtime.InteropTypes.Arrays;

namespace ZeroMenu.Tabs
{
    public class ConfigTab : ITab
    {
        public string Name => "Config";
        private Vector2 _scrollPosition = Vector2.zero;
        private bool _initialized = false;

        private TextField _menuKeyField;
        private TextField _menuColorField;
        private TextField _spoofLevelField;
        private TextField _platformNameField;

        private readonly Dictionary<string, int> _versions = new Dictionary<string, int>
        {
            { Constants.AddressablesVersion + " (Current)", Constants.GetBroadcastVersion() },
            { "16.1.0", 50632950 }, { "17.1", 50643450 },
            { "17.1.2", 50647000 }, { "17.2", 50645050 },
            { "17.2.1", 50652900 }, { "17.2.2", 50653700 }
        };

        private int _versionSelection = 0;

        public void Draw()
        {
            if (!_initialized)
            {
                _menuKeyField = new TextField(Config.MenuKey.ToString());
                _menuColorField = new TextField(Config.MenuHtmlColor);
                _spoofLevelField = new TextField(Spoof.SpoofLevelStr);
                _platformNameField = new TextField(Spoof.PlatformNameStr);
                _initialized = true;
            }

            _scrollPosition = SmoothScrollView.Begin(_scrollPosition, System.Array.Empty<GUILayoutOption>());
            GUILayout.BeginVertical(new Il2CppReferenceArray<GUILayoutOption>(new GUILayoutOption[] { GUILayout.Width((float)MainUI.windowWidth * 0.425f) }));

            DrawGeneral();
            GUILayout.Space(10f);
            DrawSpoofing();

            GUILayout.EndVertical();
            GUILayout.EndScrollView();
        }

        private void DrawGeneral()
        {
            GUILayout.Label("General Settings", MenuStyles.BoldLabel, System.Array.Empty<GUILayoutOption>());

            Config.LogChatMessages = CustomToggle.Draw(Config.LogChatMessages, "Log chat messages to console", "logChat");
            
            if (CustomButton.Draw("Clear Notifications"))
            {
                ZeroMenu.Plugin.notifications.ClearNotifications();
                ZeroMenu.Plugin.notifications.Send("Notifications", "All notifications cleared.", 3f);
            }

            GUILayout.BeginHorizontal(new Il2CppReferenceArray<GUILayoutOption>(0));
            GUILayout.Label("Menu Keybind:", MenuStyles.Label, new Il2CppReferenceArray<GUILayoutOption>(new GUILayoutOption[] { GUILayout.Width(150f) }));
            _menuKeyField.Draw(150, 22);
            if (CustomButton.Draw("Save"))
            {
                if (Enum.TryParse(_menuKeyField.Content, true, out KeyCode kc))
                {
                    Config.MenuKey = kc;
                    ZeroMenu.Plugin.notifications.Send("Config", $"Menu keybind set to {kc}.", 3f);
                }
                else
                {
                    ZeroMenu.Plugin.notifications.Send("Config", "Invalid key.", 3f);
                }
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal(new Il2CppReferenceArray<GUILayoutOption>(0));
            GUILayout.Label("Menu Color (HTML):", MenuStyles.Label, new Il2CppReferenceArray<GUILayoutOption>(new GUILayoutOption[] { GUILayout.Width(150f) }));
            _menuColorField.Draw(150, 22);
            if (CustomButton.Draw("Save"))
            {
                Config.MenuHtmlColor = _menuColorField.Content;
                ZeroMenu.Plugin.notifications.Send("Config", "Menu color saved.", 3f);
            }
            GUILayout.EndHorizontal();

            Config.OpenOnMouse = CustomToggle.Draw(Config.OpenOnMouse, "Open Menu on Mouse Position", "openOnMouse");
            Config.AutoLoadProfile = CustomToggle.Draw(Config.AutoLoadProfile, "Auto-Load Profile on Startup", "autoLoad");

            GUILayout.Space(10f);
            GUILayout.Label("Config Management", MenuStyles.BoldLabel, System.Array.Empty<GUILayoutOption>());
            if (CustomButton.Draw("Save Config")) Config.SaveConfig(false);
            if (CustomButton.Draw("Load Config")) Config.LoadConfig(false);
            if (CustomButton.Draw("Save to Profile")) Config.SaveConfig(true);
            if (CustomButton.Draw("Load from Profile")) Config.LoadConfig(true);
            if (CustomButton.Draw("Open Config File")) Config.OpenConfigFile();
        }

        private void DrawSpoofing()
        {
            GUILayout.Label("Spoofing Settings", MenuStyles.BoldLabel, System.Array.Empty<GUILayoutOption>());

            Spoof.ShouldSpoofVersion = CustomToggle.Draw(Spoof.ShouldSpoofVersion, "Enable Version Spoofing", "spoofVersion");
            
            if (Spoof.ShouldSpoofVersion)
            {
                var versionList = _versions.ToList();
                int selected = Mathf.Clamp(_versionSelection, 0, versionList.Count - 1);
                GUILayout.Label($"Spoofed Version: {versionList[selected].Key} ({Spoof.SpoofedVersion})", MenuStyles.Label, System.Array.Empty<GUILayoutOption>());
                _versionSelection = (int)CustomSlider.DrawAuto(_versionSelection, 0, versionList.Count - 1, new Il2CppReferenceArray<GUILayoutOption>(new GUILayoutOption[] { GUILayout.Width(250f) }));
                Spoof.SpoofedVersion = versionList[_versionSelection].Value;
                Spoof.UseModdedProtocol = CustomToggle.Draw(Spoof.UseModdedProtocol, "Use Modded Protocol", "moddedProtocol");
            }

            GUILayout.Label($"Spoofed Platform: {Spoof.SpoofedPlatform}", MenuStyles.Label, System.Array.Empty<GUILayoutOption>());
            float platformVal = CustomSlider.DrawAuto((float)Spoof.SpoofedPlatform, 0, 10, new Il2CppReferenceArray<GUILayoutOption>(new GUILayoutOption[] { GUILayout.Width(250f) }));
            Spoof.SpoofedPlatform = (Platforms)Mathf.RoundToInt(platformVal);

            GUILayout.BeginHorizontal(new Il2CppReferenceArray<GUILayoutOption>(0));
            GUILayout.Label("Spoof Level (1-100001):", MenuStyles.Label, new Il2CppReferenceArray<GUILayoutOption>(new GUILayoutOption[] { GUILayout.Width(150f) }));
            _spoofLevelField.Draw(150, 22);
            if (CustomButton.Draw("Save"))
            {
                if (int.TryParse(_spoofLevelField.Content, out int lvl) && lvl >= 1 && lvl <= 100001)
                {
                    Spoof.SpoofLevelStr = _spoofLevelField.Content;
                    Spoof.ApplyLevel();
                    ZeroMenu.Plugin.notifications.Send("Config", $"Spoofed level to {lvl}.", 3f);
                }
                else
                {
                    ZeroMenu.Plugin.notifications.Send("Config", "Invalid level.", 3f);
                }
            }
            GUILayout.EndHorizontal();

            Passive.SpoofDeviceId = CustomToggle.Draw(Passive.SpoofDeviceId, "Hide Device ID", "hideDeviceId");
        }
    }
}