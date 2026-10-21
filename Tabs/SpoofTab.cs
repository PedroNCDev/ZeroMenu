using System;
using System.Collections.Generic;
using UnityEngine;
using ZeroMenu.Functions;
using ZeroMenu.UI;
using Il2CppInterop.Runtime.InteropTypes.Arrays;

namespace ZeroMenu.Tabs
{
    public class SpoofTab : ITab
    {
        public string Name => "Spoof";
        private Vector2 _scrollPosition = Vector2.zero;
        private bool _initialized = false;

        private TextField _levelField;
        private TextField _platformNameField;
        private TextField _friendCodeField;
        private TextField _discordDetailsField;
        private TextField _discordStateField;

        private static readonly CollapsibleSection _levelSection = new CollapsibleSection("Player Level", true);
        private static readonly CollapsibleSection _platformSection = new CollapsibleSection("Platform", true);
        private static readonly CollapsibleSection _versionSection = new CollapsibleSection("Version", true);
        private static readonly CollapsibleSection _privacySection = new CollapsibleSection("Privacy & Account", true);
        private static readonly CollapsibleSection _discordSection = new CollapsibleSection("Discord Activity", true);

        private readonly Dictionary<string, Platforms> _platformList = new Dictionary<string, Platforms>
        {
            { "Epic Games", (Platforms)1 }, { "Steam", (Platforms)2 },
            { "Mac", (Platforms)3 }, { "Win10", (Platforms)4 },
            { "Itch.io", (Platforms)5 }, { "iPhone", (Platforms)6 },
            { "Android", (Platforms)7 }, { "Switch", (Platforms)8 },
            { "Xbox", (Platforms)9 }, { "PlayStation", (Platforms)10 }
        };

        private readonly Dictionary<string, int> _versionList = new Dictionary<string, int>
        {
            { Constants.AddressablesVersion + " (Current)", Constants.GetBroadcastVersion() },
            { "16.1.0", 50632950 }, { "17.1", 50643450 },
            { "17.1.2", 50647000 }, { "17.2", 50645050 },
            { "17.2.1", 50652900 }, { "17.2.2", 50653700 },
            { "18", 50663350 }, { "19", 50663600 }
        };

        public void Draw()
        {
            if (!_initialized)
            {
                _levelField = new TextField(Spoof.SpoofLevelStr);
                _platformNameField = new TextField(Spoof.PlatformNameStr);
                _friendCodeField = new TextField(Spoof.FriendCodeStr);
                _discordDetailsField = new TextField(Spoof.DiscordDetails);
                _discordStateField = new TextField(Spoof.DiscordState);
                _initialized = true;
            }

            _scrollPosition = SmoothScrollView.Begin(_scrollPosition, System.Array.Empty<GUILayoutOption>());
            GUILayout.BeginVertical(new Il2CppReferenceArray<GUILayoutOption>(new GUILayoutOption[] { GUILayout.Width((float)MainUI.windowWidth * 0.425f) }));

            if (_levelSection.DrawWithIndent())
            {
                DrawLevel();
                _levelSection.EndIndent();
            }
            GUILayout.Space(10f);

            if (_platformSection.DrawWithIndent())
            {
                DrawPlatform();
                _platformSection.EndIndent();
            }
            GUILayout.Space(10f);

            if (_versionSection.DrawWithIndent())
            {
                DrawVersion();
                _versionSection.EndIndent();
            }
            GUILayout.Space(10f);

            if (_privacySection.DrawWithIndent())
            {
                DrawPrivacy();
                _privacySection.EndIndent();
            }
            GUILayout.Space(10f);

            if (_discordSection.DrawWithIndent())
            {
                DrawDiscord();
                _discordSection.EndIndent();
            }

            GUILayout.EndVertical();
            GUILayout.EndScrollView();
        }

        private void DrawLevel()
        {
            GUILayout.BeginHorizontal(new Il2CppReferenceArray<GUILayoutOption>(0));
            GUILayout.Label("Level (1-99999):", MenuStyles.Label, new Il2CppReferenceArray<GUILayoutOption>(new GUILayoutOption[] { GUILayout.Width(120f) }));
            _levelField.Draw(120, 22);
            if (CustomButton.Draw("Apply"))
            {
                Spoof.SpoofLevelStr = _levelField.Content;
                Spoof.ApplyLevel();
                ZeroMenu.Plugin.notifications.Send("Spoof Level", "Level set to " + _levelField.Content + ".", 5f);
            }
            GUILayout.EndHorizontal();

            GUILayout.Space(5f);

            GUILayout.BeginHorizontal(new Il2CppReferenceArray<GUILayoutOption>(0));
            if (CustomButton.Draw("-1")) AdjustLevel(-1);
            if (CustomButton.Draw("+1")) AdjustLevel(1);
            if (CustomButton.Draw("-10")) AdjustLevel(-10);
            if (CustomButton.Draw("+10")) AdjustLevel(10);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal(new Il2CppReferenceArray<GUILayoutOption>(0));
            if (CustomButton.Draw("-100")) AdjustLevel(-100);
            if (CustomButton.Draw("+100")) AdjustLevel(100);
            if (CustomButton.Draw("-1000")) AdjustLevel(-1000);
            if (CustomButton.Draw("+1000")) AdjustLevel(1000);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal(new Il2CppReferenceArray<GUILayoutOption>(0));
            if (CustomButton.Draw("-10000")) AdjustLevel(-10000);
            if (CustomButton.Draw("+10000")) AdjustLevel(10000);
            GUILayout.EndHorizontal();
        }

        private void AdjustLevel(int delta)
        {
            if (!int.TryParse(_levelField.Content, out int currentLevel) || currentLevel < 1) currentLevel = 1;
            int newLevel = Mathf.Clamp(currentLevel + delta, 1, 99999);
            _levelField.Content = newLevel.ToString();
            Spoof.SpoofLevelStr = _levelField.Content;
            Spoof.ApplyLevel();
        }

        private void DrawPlatform()
        {
            int i = 0;
            foreach (var kv in _platformList)
            {
                if (i % 2 == 0) GUILayout.BeginHorizontal(new Il2CppReferenceArray<GUILayoutOption>(0));

                bool selected = Spoof.SpoofedPlatform == kv.Value;
                string label = selected ? $"[{kv.Key}]" : kv.Key;

                if (CustomButton.Draw(label))
                {
                    Spoof.SpoofedPlatform = kv.Value;
                    ZeroMenu.Plugin.notifications.Send("Spoof Platform", "Spoofed as " + kv.Key + ".", 5f);
                }

                if (i % 2 == 1) GUILayout.EndHorizontal();
                i++;
            }
            if (i % 2 == 1) GUILayout.EndHorizontal();

            GUILayout.Space(5f);
            GUILayout.BeginHorizontal(new Il2CppReferenceArray<GUILayoutOption>(0));
            GUILayout.Label("Platform Name:", MenuStyles.Label, new Il2CppReferenceArray<GUILayoutOption>(new GUILayoutOption[] { GUILayout.Width(120f) }));
            _platformNameField.Draw(120, 22);
            if (CustomButton.Draw("Save"))
            {
                Spoof.PlatformNameStr = _platformNameField.Content;
            }
            GUILayout.EndHorizontal();

            GUILayout.Label("Current: " + Spoof.SpoofedPlatform, MenuStyles.Label, new Il2CppReferenceArray<GUILayoutOption>(0));
        }

        private void DrawVersion()
        {
            Spoof.ShouldSpoofVersion = CustomToggle.Draw(Spoof.ShouldSpoofVersion, "Spoof Version", "spoofVersion");

            if (Spoof.ShouldSpoofVersion)
            {
                GUILayout.Space(3f);
                foreach (var kv in _versionList)
                {
                    bool selected = Spoof.SpoofedVersion == kv.Value;
                    string label = selected ? $"[{kv.Key} ({kv.Value})]" : $"{kv.Key} ({kv.Value})";

                    if (CustomButton.Draw(label))
                    {
                        Spoof.SpoofedVersion = kv.Value;
                        ZeroMenu.Plugin.notifications.Send("Spoof Version", "Version set to " + kv.Key + ".", 5f);
                    }
                }
                GUILayout.Space(3f);
                Spoof.UseModdedProtocol = CustomToggle.Draw(Spoof.UseModdedProtocol, "Use Modded Protocol", "moddedProtocol");
            }
        }

        private void DrawPrivacy()
        {
            Spoof.GuestMode = CustomToggle.Draw(Spoof.GuestMode, "Spoof Guest Account", "guestMode");
            Spoof.FriendCodeSpoof = CustomToggle.Draw(Spoof.FriendCodeSpoof, "Spoof Friend Code", "friendCodeSpoof");
            Passive.SpoofDeviceId = CustomToggle.Draw(Passive.SpoofDeviceId, "Hide Device ID", "hideDeviceId");
            Passive.SpoofAprilFoolsDate = CustomToggle.Draw(Passive.SpoofAprilFoolsDate, "Spoof Date to April 1st", "aprilFools");

            GUILayout.BeginHorizontal(new Il2CppReferenceArray<GUILayoutOption>(0));
            GUILayout.Label("Friend Code:", MenuStyles.Label, new Il2CppReferenceArray<GUILayoutOption>(new GUILayoutOption[] { GUILayout.Width(100f) }));
            _friendCodeField.Draw(140, 22);
            if (CustomButton.Draw("Set"))
            {
                Spoof.FriendCodeStr = _friendCodeField.Content;
                ZeroMenu.Plugin.notifications.Send("Friend Code", "Friend code set.", 5f);
            }
            GUILayout.EndHorizontal();
        }

        private void DrawDiscord()
        {
            Spoof.DiscordEnabled = CustomToggle.Draw(Spoof.DiscordEnabled, "Enable Discord Activity", "discordActivity");

            if (Spoof.DiscordEnabled)
            {
                GUILayout.Space(4f);

                GUILayout.Label("Details:", MenuStyles.Label, System.Array.Empty<GUILayoutOption>());
                _discordDetailsField.Draw(300, 22);
                Spoof.DiscordDetails = _discordDetailsField.Content;

                GUILayout.Label("State:", MenuStyles.Label, System.Array.Empty<GUILayoutOption>());
                _discordStateField.Draw(300, 22);
                Spoof.DiscordState = _discordStateField.Content;
            }
        }
    }
}