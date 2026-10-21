using System;
using System.Collections.Generic;
using UnityEngine;
using AmongUs.GameOptions;
using ZeroMenu.Functions;
using ZeroMenu.UI;
using Il2CppInterop.Runtime.InteropTypes.Arrays;

namespace ZeroMenu.Tabs
{
    public class HostOnlyTab : ITab
    {
        public string Name => "Host-Only";

        private const float SubTabWidth = 100f;
        private const float SubTabSpacing = 5f;
        private const float SubTabBarHeight = 35f;

        private static readonly string[] _subtabNames = { "General", "Role Assign", "Settings" };
        private int _subtab = 0;

        private float _subtabLinePosition = 0f;
        private float _subtabLineVelocity = 0f;
        private float _lastRepaintTime = 0f;

        private Vector2 _generalScroll = Vector2.zero;
        private Vector2 _roleScroll = Vector2.zero;
        private Vector2 _settingsScroll = Vector2.zero;

        private static byte _selectedMap = 0;

        private static int _globalRoleIndex = 0;
        private static readonly Dictionary<byte, int> _playerRoleIndex = new Dictionary<byte, int>();

        private static Dictionary<FloatOptionNames, TextField> _floatFields = new Dictionary<FloatOptionNames, TextField>();
        private static Dictionary<Int32OptionNames, TextField> _intFields = new Dictionary<Int32OptionNames, TextField>();

        public void Draw()
        {
            DrawSubTabBar();

            GUILayout.Space(15f);

            switch (_subtab)
            {
                case 0: DrawGeneral(); break;
                case 1: DrawRoleAssign(); break;
                case 2: DrawSettings(); break;
            }
        }

        private void DrawSubTabBar()
        {
            float viewWidth = MainUI.windowWidth - 45f;
            Rect tabViewRect = GUILayoutUtility.GetRect(viewWidth, SubTabBarHeight);

            if (Event.current.type == EventType.Repaint)
            {
                float now = Time.unscaledTime;
                float dt = now - _lastRepaintTime;
                if (dt <= 0f || dt > 0.5f) dt = Time.unscaledDeltaTime;
                _lastRepaintTime = now;

                float targetX = _subtab * (SubTabWidth + SubTabSpacing);
                if (Mathf.Abs(_subtabLinePosition - targetX) > 0.1f)
                {
                    _subtabLinePosition = Mathf.SmoothDamp(
                        _subtabLinePosition, targetX, ref _subtabLineVelocity,
                        0.2f, Mathf.Infinity, dt);
                }
                else
                {
                    _subtabLinePosition = targetX;
                }
            }

            GUI.BeginGroup(tabViewRect);

            for (int i = 0; i < _subtabNames.Length; i++)
            {
                float xPos = i * (SubTabWidth + SubTabSpacing);
                Rect btnRect = new Rect(xPos, 0f, SubTabWidth, 25f);

                var style = MenuStyles.TabInactive;
                if (i == _subtab) style = MenuStyles.TabActive;
                else if (btnRect.Contains(Event.current.mousePosition)) style = MenuStyles.TabHover;

                if (GUI.Button(btnRect, _subtabNames[i], style))
                {
                    _subtab = i;
                }
            }

            GUI.Box(new Rect(_subtabLinePosition, 27f, SubTabWidth, 2f), "", MenuStyles.Line);

            GUI.EndGroup();
        }

        private void DrawGeneral()
        {
            _generalScroll = SmoothScrollView.Begin(_generalScroll, System.Array.Empty<GUILayoutOption>());
            GUILayout.BeginVertical(new Il2CppReferenceArray<GUILayoutOption>(new GUILayoutOption[] { GUILayout.Width((float)MainUI.windowWidth * 0.425f) }));

            if (PlayerControl.LocalPlayer == null || PlayerControl.LocalPlayer.Data == null)
            {
                GUILayout.Label("You are not currently in a game, these options will not work.", MenuStyles.Label, new Il2CppReferenceArray<GUILayoutOption>(0));
            }
            else if (!AmongUsClient.Instance.AmHost)
            {
                GUILayout.Label("You are not the host. These may fail or ban you.", MenuStyles.Label, new Il2CppReferenceArray<GUILayoutOption>(0));
            }

            GUILayout.Space(10f);
            GUILayout.Label("General", MenuStyles.BoldLabel, System.Array.Empty<GUILayoutOption>());
            Host.KillVanished = CustomToggle.Draw(Host.KillVanished, "Kill While Vanished", "KillVanished");
            Host.KillAnyone = CustomToggle.Draw(Host.KillAnyone, "Kill Anyone", "KillAnyone");
            Host.KillGhosts = CustomToggle.Draw(Host.KillGhosts, "Kill Ghosts", "KillGhosts");
            Host.NoKillCooldown = CustomToggle.Draw(Host.NoKillCooldown, "No Kill Cooldown", "NoKillCooldown");
            Host.NoTaskMode = CustomToggle.Draw(Host.NoTaskMode, "No Task Mode", "NoTaskMode");
            Host.NoOptionsLimits = CustomToggle.Draw(Host.NoOptionsLimits, "No Settings Limit", "NoOptionsLimits");
            Host.BanMidGame = CustomToggle.Draw(Host.BanMidGame, "Ban Players Mid-Game", "BanMidGame");
            Host.SetSkeldFlip(CustomToggle.Draw(Host.IsSkeldFlipped, "Use Flipped Skeld Map", "FlipSkeld"));

            GUILayout.Space(10f);
            GUILayout.Label("Protections", MenuStyles.BoldLabel, System.Array.Empty<GUILayoutOption>());
            Host.GodMode = CustomToggle.Draw(Host.GodMode, "God Mode", "GodMode");
            Host.GodModeAll = CustomToggle.Draw(Host.GodModeAll, "God Mode Everyone", "GodModeAll");
            Host.AutoAngel = CustomToggle.Draw(Host.AutoAngel, "Auto Angel", "AutoAngel");

            GUILayout.BeginHorizontal(new Il2CppReferenceArray<GUILayoutOption>(0));
            GUILayout.Label($"Angel Delay: {Host.AutoAngelInterval:F1}s", MenuStyles.Label, new Il2CppReferenceArray<GUILayoutOption>(new GUILayoutOption[] { GUILayout.Width(120f) }));
            Host.AutoAngelInterval = CustomSlider.DrawAuto(Host.AutoAngelInterval, 0.1f, 2f, new Il2CppReferenceArray<GUILayoutOption>(new GUILayoutOption[] { GUILayout.Width(150f) }));
            GUILayout.EndHorizontal();

            if (CustomButton.Draw("Protect Everyone")) Host.ProtectEveryone(true);

            GUILayout.Space(10f);
            GUILayout.Label("Murder", MenuStyles.BoldLabel, System.Array.Empty<GUILayoutOption>());
            if (CustomButton.Draw("Kill All Crewmates")) Host.KillAll(RoleTeamTypes.Crewmate);
            if (CustomButton.Draw("Kill All Impostors")) Host.KillAll(RoleTeamTypes.Impostor);
            if (CustomButton.Draw("Kill Everyone")) Host.KillAll(null);
            Host.KillInLobbies = CustomToggle.Draw(Host.KillInLobbies, "Allow Killing in Lobbies", "KillInLobbies");

            GUILayout.Space(10f);
            GUILayout.Label("Game State", MenuStyles.BoldLabel, System.Array.Empty<GUILayoutOption>());
            if (CustomButton.Draw("Force Start Game")) Host.ForceStartGame = true;
            if (CustomButton.Draw("Cancel Start of Game")) Host.CancelStartGame = true;

            Host.ModifyStartCountdown = CustomToggle.Draw(Host.ModifyStartCountdown, "Modify Start Countdown", "ModifyStartCountdown");
            if (Host.ModifyStartCountdown)
            {
                GUILayout.BeginHorizontal(new Il2CppReferenceArray<GUILayoutOption>(0));
                GUILayout.Label($"Countdown: {Host.StartCountdown}s", MenuStyles.Label, new Il2CppReferenceArray<GUILayoutOption>(new GUILayoutOption[] { GUILayout.Width(130f) }));
                Host.StartCountdown = Mathf.RoundToInt(CustomSlider.DrawAuto(Host.StartCountdown, 1f, 127f, new Il2CppReferenceArray<GUILayoutOption>(new GUILayoutOption[] { GUILayout.Width(150f) })));
                GUILayout.EndHorizontal();
            }

            string[] gameModes = { "Default", "Task Speedrun", "Battle Royale" };
            GUILayout.BeginHorizontal(new Il2CppReferenceArray<GUILayoutOption>(0));
            if (CustomButton.Draw("<")) Host.GameMode = (Host.GameMode + 2) % 3;
            GUILayout.Label($"Mode: {gameModes[Host.GameMode]}", MenuStyles.Label, new Il2CppReferenceArray<GUILayoutOption>(new GUILayoutOption[] { GUILayout.Width(120f) }));
            if (CustomButton.Draw(">")) Host.GameMode = (Host.GameMode + 1) % 3;
            GUILayout.EndHorizontal();

            Host.DisableMeetings = CustomToggle.Draw(Host.DisableMeetings, "Disable Meetings", "DisableMeetings");
            Host.DisableSabotages = CustomToggle.Draw(Host.DisableSabotages, "Disable Sabotages", "DisableSabotages");
            Host.DisableCloseDoors = CustomToggle.Draw(Host.DisableCloseDoors, "Disable Close Doors", "DisableCloseDoors");
            Host.DisableCameras = CustomToggle.Draw(Host.DisableCameras, "Disable Security Cameras", "DisableCameras");
            Host.DisableGameEnd = CustomToggle.Draw(Host.DisableGameEnd, "Disable Game End", "DisableGameEnd");
            Host.DisableVentClean = CustomToggle.Draw(Host.DisableVentClean, "Disable Vent Clean", "DisableVentClean");
            Host.DisableAllVotekicks = CustomToggle.Draw(Host.DisableAllVotekicks, "Disable All Votekicks", "DisableAllVotekicks");

            Host.ForceColorEveryone = CustomToggle.Draw(Host.ForceColorEveryone, "Force Color For Everyone", "ForceColorEveryone");
            if (Host.ForceColorEveryone)
            {
                GUILayout.BeginHorizontal(new Il2CppReferenceArray<GUILayoutOption>(0));
                GUILayout.Label($"Color ID: {Host.ForceColorId}", MenuStyles.Label, new Il2CppReferenceArray<GUILayoutOption>(new GUILayoutOption[] { GUILayout.Width(100f) }));
                Host.ForceColorId = (byte)CustomSlider.DrawAuto(Host.ForceColorId, 0f, 17f, new Il2CppReferenceArray<GUILayoutOption>(new GUILayoutOption[] { GUILayout.Width(150f) }));
                GUILayout.EndHorizontal();
            }

            GUILayout.Space(10f);
            GUILayout.Label("Meetings", MenuStyles.BoldLabel, System.Array.Empty<GUILayoutOption>());
            Host.SkipMeeting = CustomToggle.Draw(Host.SkipMeeting, "Skip Meeting", "SkipMeeting");
            Host.VoteImmune = CustomToggle.Draw(Host.VoteImmune, "Vote Immune", "VoteImmune");
            Host.JudgeImmune = CustomToggle.Draw(Host.JudgeImmune, "Judge Immune", "JudgeImmune");
            Host.EjectPlayer = CustomToggle.Draw(Host.EjectPlayer, "Eject Player", "EjectPlayer");

            GUILayout.Space(10f);
            GUILayout.Label("Players", MenuStyles.BoldLabel, System.Array.Empty<GUILayoutOption>());
            Host.ReportBodySpam = CustomToggle.Draw(Host.ReportBodySpam, "Spam Report Bodies", "ReportBodySpam");
            Host.BlockLowLevels = CustomToggle.Draw(Host.BlockLowLevels, "Block Low Levels", "BlockLowLevels");
            if (Host.BlockLowLevels)
            {
                GUILayout.BeginHorizontal(new Il2CppReferenceArray<GUILayoutOption>(0));
                GUILayout.Label($"Min Level: {Host.BlockLowLevelsMin}", MenuStyles.Label, new Il2CppReferenceArray<GUILayoutOption>(new GUILayoutOption[] { GUILayout.Width(100f) }));
                Host.BlockLowLevelsMin = (uint)CustomSlider.DrawAuto(Host.BlockLowLevelsMin, 0f, 100f, new Il2CppReferenceArray<GUILayoutOption>(new GUILayoutOption[] { GUILayout.Width(150f) }));
                GUILayout.EndHorizontal();
            }

            GUILayout.Space(10f);
            GUILayout.Label("End Game", MenuStyles.BoldLabel, System.Array.Empty<GUILayoutOption>());
            GUILayout.BeginHorizontal(new Il2CppReferenceArray<GUILayoutOption>(0));
            if (CustomButton.Draw("Crewmate Win")) Host.ForceVictory(GameOverReason.CrewmatesByTask);
            if (CustomButton.Draw("Impostor Win")) Host.ForceVictory(GameOverReason.ImpostorsByKill);
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal(new Il2CppReferenceArray<GUILayoutOption>(0));
            if (CustomButton.Draw("Imp Disconnect")) Host.ForceEndImpDisconnect();
            if (CustomButton.Draw("Kick All")) Host.KickAll();
            GUILayout.EndHorizontal();

            GUILayout.Space(10f);
            GUILayout.Label("Shapeshift Controls", MenuStyles.BoldLabel, System.Array.Empty<GUILayoutOption>());
            if (CustomButton.Draw("Shapeshift Everyone Into Me")) Host.ShapeshiftAll(PlayerControl.LocalPlayer);
            if (CustomButton.Draw("Shapeshift Everyone Into Random"))
            {
                var all = PlayerControl.AllPlayerControls.ToArray();
                if (all.Length > 0) Host.ShapeshiftAll(all[UnityEngine.Random.Range(0, all.Length)]);
            }
            if (CustomButton.Draw("Revert All Shapeshifts")) Host.RevertAllShapeshifts();
            Host.FakeShapeshiftBubble = CustomToggle.Draw(Host.FakeShapeshiftBubble, "Fake Shapeshift Bubble", "FakeShapeshiftBubble");

            GUILayout.Space(10f);
            GUILayout.Label("Role Manager", MenuStyles.BoldLabel, System.Array.Empty<GUILayoutOption>());
            Host.PreGameRoleForce = CustomToggle.Draw(Host.PreGameRoleForce, "Pre-Game Role Forcing", "PreGameRoleForce");
            if (Host.PreGameRoleForce)
            {
                GUILayout.BeginHorizontal(new Il2CppReferenceArray<GUILayoutOption>(0));
                GUILayout.Label($"Random Impostors: {Host.PreGameImpCount}", MenuStyles.Label, new Il2CppReferenceArray<GUILayoutOption>(new GUILayoutOption[] { GUILayout.Width(160f) }));
                Host.PreGameImpCount = Mathf.RoundToInt(CustomSlider.DrawAuto(Host.PreGameImpCount, 1f, 3f, new Il2CppReferenceArray<GUILayoutOption>(new GUILayoutOption[] { GUILayout.Width(100f) })));
                GUILayout.EndHorizontal();
            }

            GUILayout.Space(10f);
            GUILayout.Label("Map Spawner", MenuStyles.BoldLabel, System.Array.Empty<GUILayoutOption>());
            GUILayout.BeginHorizontal(new Il2CppReferenceArray<GUILayoutOption>(0));
            if (CustomButton.Draw("Despawn Lobby")) { if (LobbyBehaviour.Instance != null) LobbyBehaviour.Instance.Despawn(); }
            if (CustomButton.Draw("Spawn Lobby")) Host.SpawnLobby();
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal(new Il2CppReferenceArray<GUILayoutOption>(0));
            GUILayout.Label($"Map: {(MapNames)_selectedMap}", MenuStyles.Label, new Il2CppReferenceArray<GUILayoutOption>(new GUILayoutOption[] { GUILayout.Width(80f) }));
            _selectedMap = (byte)CustomSlider.DrawAuto(_selectedMap, 0f, 5f, new Il2CppReferenceArray<GUILayoutOption>(new GUILayoutOption[] { GUILayout.Width(100f) }));
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal(new Il2CppReferenceArray<GUILayoutOption>(0));
            if (CustomButton.Draw("Despawn Map")) { if (ShipStatus.Instance != null) ShipStatus.Instance.Despawn(); }
            if (CustomButton.Draw("Spawn Map")) Host.SpawnMap(_selectedMap);
            GUILayout.EndHorizontal();

            GUILayout.Space(10f);
            GUILayout.Label("Disco Party", MenuStyles.BoldLabel, System.Array.Empty<GUILayoutOption>());
            Host.DiscoParty = CustomToggle.Draw(Host.DiscoParty, "Enable Disco Party", "DiscoParty");
            GUILayout.BeginHorizontal(new Il2CppReferenceArray<GUILayoutOption>(0));
            GUILayout.Label($"Delay: {Host.DiscoDelay:F2}s", MenuStyles.Label, new Il2CppReferenceArray<GUILayoutOption>(new GUILayoutOption[] { GUILayout.Width(80f) }));
            Host.DiscoDelay = CustomSlider.DrawAuto(Host.DiscoDelay, 0.1f, 2f, new Il2CppReferenceArray<GUILayoutOption>(new GUILayoutOption[] { GUILayout.Width(150f) }));
            GUILayout.EndHorizontal();

            GUILayout.EndVertical();
            GUILayout.EndScrollView();
        }

        private void DrawRoleAssign()
        {
            _roleScroll = SmoothScrollView.Begin(_roleScroll, System.Array.Empty<GUILayoutOption>());
            GUILayout.BeginVertical(new Il2CppReferenceArray<GUILayoutOption>(new GUILayoutOption[] { GUILayout.Width((float)MainUI.windowWidth * 0.425f) }));

            if (PlayerControl.AllPlayerControls == null || PlayerControl.AllPlayerControls.Count == 0)
            {
                GUILayout.Label("No players available.", MenuStyles.Label, new Il2CppReferenceArray<GUILayoutOption>(0));
                GUILayout.EndVertical();
                GUILayout.EndScrollView();
                return;
            }

            GUILayout.Label("Global Role Selector", MenuStyles.BoldLabel, System.Array.Empty<GUILayoutOption>());
            GUILayout.BeginHorizontal(new Il2CppReferenceArray<GUILayoutOption>(0));
            if (CustomButton.Draw("<")) _globalRoleIndex = (_globalRoleIndex - 1 + Host.ValidRoles.Length) % Host.ValidRoles.Length;
            GUILayout.Label(Host.ValidRoles[_globalRoleIndex].ToString(), MenuStyles.Label, new Il2CppReferenceArray<GUILayoutOption>(new GUILayoutOption[] { GUILayout.Width(120f) }));
            if (CustomButton.Draw(">")) _globalRoleIndex = (_globalRoleIndex + 1) % Host.ValidRoles.Length;
            GUILayout.EndHorizontal();

            if (CustomButton.Draw("Set All Players To Selected Role"))
            {
                Host.SetAllPlayersRole(Host.ValidRoles[_globalRoleIndex]);
            }

            GUILayout.Space(15f);
            GUILayout.Label("Per-Player Role Assignment", MenuStyles.BoldLabel, System.Array.Empty<GUILayoutOption>());
            GUILayout.Space(5f);

            foreach (var player in PlayerControl.AllPlayerControls)
            {
                if (player?.Data == null) continue;

                if (!_playerRoleIndex.ContainsKey(player.PlayerId))
                    _playerRoleIndex[player.PlayerId] = 0;

                int idx = _playerRoleIndex[player.PlayerId];
                if (idx < 0 || idx >= Host.ValidRoles.Length) idx = 0;

                GUILayout.BeginVertical(new Il2CppReferenceArray<GUILayoutOption>(new GUILayoutOption[] { GUILayout.Width(360f) }));

                string nameColor = ColorUtility.ToHtmlStringRGB(player.Data.Color);
                GUILayout.BeginHorizontal(new Il2CppReferenceArray<GUILayoutOption>(0));
                GUILayout.Label($"<color=#{nameColor}>{player.Data.PlayerName}</color>", MenuStyles.Label, new Il2CppReferenceArray<GUILayoutOption>(new GUILayoutOption[] { GUILayout.Width(150f) }));
                GUILayout.Label($"Current: {Host.GetPlayerCurrentRole(player)}", MenuStyles.Label, new Il2CppReferenceArray<GUILayoutOption>(0));
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal(new Il2CppReferenceArray<GUILayoutOption>(0));
                if (CustomButton.Draw("<")) _playerRoleIndex[player.PlayerId] = (idx - 1 + Host.ValidRoles.Length) % Host.ValidRoles.Length;
                GUILayout.Label(Host.ValidRoles[idx].ToString(), MenuStyles.Label, new Il2CppReferenceArray<GUILayoutOption>(new GUILayoutOption[] { GUILayout.Width(110f) }));
                if (CustomButton.Draw(">")) _playerRoleIndex[player.PlayerId] = (idx + 1) % Host.ValidRoles.Length;
                if (CustomButton.Draw("Apply Role"))
                {
                    Host.SetPlayerRole(player, Host.ValidRoles[_playerRoleIndex[player.PlayerId]]);
                }
                GUILayout.EndHorizontal();

                GUILayout.EndVertical();
                GUILayout.Space(5f);
            }

            GUILayout.EndVertical();
            GUILayout.EndScrollView();
        }

        private void DrawSettings()
        {
            if (!Host.IsHostReady())
            {
                GUILayout.Label("HostSettings not initialized. Call HostSettings.Initialize(Log) in Plugin.Load().",
                    MenuStyles.Label, new Il2CppReferenceArray<GUILayoutOption>(0));
                return;
            }

            var mode = GameOptionsManager.Instance.currentGameOptions.GameMode;

            _settingsScroll = SmoothScrollView.Begin(_settingsScroll, System.Array.Empty<GUILayoutOption>());
            GUILayout.BeginVertical(new Il2CppReferenceArray<GUILayoutOption>(new GUILayoutOption[] { GUILayout.Width((float)MainUI.windowWidth * 0.425f) }));

            if (CustomButton.Draw("Refresh All Fields"))
            {
                foreach (var def in Host.NormalFloats) if (_floatFields.ContainsKey(def.Enum)) _floatFields[def.Enum].Content = Host.FormatFloatSetting(def.Enum);
                foreach (var def in Host.NormalInts) if (_intFields.ContainsKey(def.Enum)) _intFields[def.Enum].Content = Host.FormatIntSetting(def.Enum);
                
                foreach (var def in Host.RoleFloats) if (_floatFields.ContainsKey(def.Enum)) _floatFields[def.Enum].Content = Host.FormatFloatSetting(def.Enum);
                
                foreach (var def in Host.HnSFloats) if (_floatFields.ContainsKey(def.Enum)) _floatFields[def.Enum].Content = Host.FormatFloatSetting(def.Enum);
                foreach (var def in Host.HnSInts) if (_intFields.ContainsKey(def.Enum)) _intFields[def.Enum].Content = Host.FormatIntSetting(def.Enum);
            }

            GUILayout.Space(10f);
            DrawIntRow(Int32OptionNames.NumImpostors, "Number of Impostors");

            GUILayout.Space(10f);
            GUILayout.Label("General Settings", MenuStyles.BoldLabel, System.Array.Empty<GUILayoutOption>());

            byte curMap = Host.GetByteSetting(ByteOptionNames.MapId);
            GUILayout.BeginHorizontal(new Il2CppReferenceArray<GUILayoutOption>(0));
            GUILayout.Label("Map", MenuStyles.Label, new Il2CppReferenceArray<GUILayoutOption>(new GUILayoutOption[] { GUILayout.Width(230f) }));
            GUILayout.Label($"{(MapNames)curMap} (ID: {curMap})", MenuStyles.Label, new Il2CppReferenceArray<GUILayoutOption>(new GUILayoutOption[] { GUILayout.Width(150f) }));
            if (CustomButton.Draw("<")) Host.ApplyByteSetting(ByteOptionNames.MapId, (byte)Math.Max(0, curMap - 1));
            if (CustomButton.Draw(">")) Host.ApplyByteSetting(ByteOptionNames.MapId, (byte)Math.Min(5, curMap + 1));
            GUILayout.EndHorizontal();

            if (mode == GameModes.Normal)
            {
                DrawFloatList(Host.NormalFloats);
                DrawIntList(Host.NormalInts);
                DrawBoolList(Host.NormalBools);

                DrawRoleSettings();
            }
            else if (mode == GameModes.HideNSeek)
            {
                GUILayout.Space(10f);
                GUILayout.Label("Hide & Seek Settings", MenuStyles.BoldLabel, System.Array.Empty<GUILayoutOption>());
                DrawFloatList(Host.HnSFloats);
                DrawIntList(Host.HnSInts);
                DrawBoolList(Host.HnSBools);
            }
            else
            {
                GUILayout.Label("Unknown game mode. No settings available.", MenuStyles.Label, System.Array.Empty<GUILayoutOption>());
            }

            GUILayout.EndVertical();
            GUILayout.EndScrollView();
        }

        private void DrawRoleSettings()
        {
            GUILayout.Space(15f);
            GUILayout.Label("Role Settings", MenuStyles.BoldLabel, System.Array.Empty<GUILayoutOption>());

            GUILayout.Space(5f);
            GUILayout.Label("<color=#00FFFF>Scientist</color>", MenuStyles.BoldLabel);
            DrawFloatRow(FloatOptionNames.ScientistCooldown);
            DrawFloatRow(FloatOptionNames.ScientistBatteryCharge);

            GUILayout.Label("<color=#00FFFF>Engineer</color>", MenuStyles.BoldLabel);
            DrawFloatRow(FloatOptionNames.EngineerCooldown);
            DrawFloatRow(FloatOptionNames.EngineerInVentMaxTime);

            GUILayout.Label("<color=#FF0000>Shapeshifter</color>", MenuStyles.BoldLabel);
            DrawFloatRow(FloatOptionNames.ShapeshifterDuration);
            DrawFloatRow(FloatOptionNames.ShapeshifterCooldown);
            DrawBoolRow(BoolOptionNames.ShapeshifterLeaveSkin);

            GUILayout.Label("<color=#8AFCFF>Guardian Angel</color>", MenuStyles.BoldLabel);
            DrawFloatRow(FloatOptionNames.GuardianAngelCooldown);
            DrawFloatRow(FloatOptionNames.ProtectionDurationSeconds);
            DrawBoolRow(BoolOptionNames.ImpostorsCanSeeProtect);

            GUILayout.Label("<color=#00FFFF>Noisemaker</color>", MenuStyles.BoldLabel);
            DrawFloatRow(FloatOptionNames.NoisemakerAlertDuration);
            DrawBoolRow(BoolOptionNames.NoisemakerImpostorAlert);

            GUILayout.Label("<color=#00FFFF>Tracker</color>", MenuStyles.BoldLabel);
            DrawFloatRow(FloatOptionNames.TrackerDuration);
            DrawFloatRow(FloatOptionNames.TrackerCooldown);
            DrawFloatRow(FloatOptionNames.TrackerDelay);

            GUILayout.Label("<color=#FF0000>Phantom</color>", MenuStyles.BoldLabel);
            DrawFloatRow(FloatOptionNames.PhantomDuration);
            DrawFloatRow(FloatOptionNames.PhantomCooldown);

            GUILayout.Label("<color=#00FFFF>Detective</color>", MenuStyles.BoldLabel);
            DrawFloatRow(FloatOptionNames.DetectiveSuspectLimit);

            GUILayout.Label("<color=#FF0000>Viper</color>", MenuStyles.BoldLabel);
            DrawFloatRow(FloatOptionNames.ViperDissolveTime);

            GUILayout.Label("<color=#00FFFF>Judge</color>", MenuStyles.BoldLabel);
            DrawFloatRow(FloatOptionNames.JudgeTaskRequirementPercentage);

            GUILayout.Label("<color=#8AFCFF>Influencer</color>", MenuStyles.BoldLabel);
            DrawFloatRow(FloatOptionNames.SpiritGuideCooldownSeconds);
        }

        private void DrawFloatList(Host.FloatSettingDef[] settings)
        {
            foreach (var def in settings)
                DrawFloatRow(def.Enum, def.Name);
        }

        private void DrawIntList(Host.IntSettingDef[] settings)
        {
            foreach (var def in settings)
                DrawIntRow(def.Enum, def.Name);
        }

        private void DrawBoolList(Host.BoolSettingDef[] settings)
        {
            foreach (var def in settings)
                DrawBoolRow(def.Enum, def.Name);
        }

        private void DrawFloatRow(FloatOptionNames enumVal, string labelName = null)
        {
            if (labelName == null)
            {
                labelName = enumVal.ToString();
                foreach (var def in Host.RoleFloats) if (def.Enum == enumVal) { labelName = def.Name; break; }
                foreach (var def in Host.NormalFloats) if (def.Enum == enumVal) { labelName = def.Name; break; }
                foreach (var def in Host.HnSFloats) if (def.Enum == enumVal) { labelName = def.Name; break; }
            }

            if (!_floatFields.ContainsKey(enumVal))
                _floatFields[enumVal] = new TextField(Host.FormatFloatSetting(enumVal));

            var field = _floatFields[enumVal];
            GUILayout.BeginHorizontal(new Il2CppReferenceArray<GUILayoutOption>(0));
            GUILayout.Label(labelName, MenuStyles.Label, new Il2CppReferenceArray<GUILayoutOption>(new GUILayoutOption[] { GUILayout.Width(230f) }));
            field.Draw(100, 22);

            if (field.Submitted)
            {
                Host.TryApplyFloatSetting(enumVal, field.Content);
                field.Content = Host.FormatFloatSetting(enumVal);
                field.ClearSubmitted();
            }
            else if (!field.IsFocused)
            {
                string cur = Host.FormatFloatSetting(enumVal);
                if (field.Content != cur) field.Content = cur;
            }

            GUILayout.EndHorizontal();
        }

        private void DrawIntRow(Int32OptionNames enumVal, string labelName = null)
        {
            if (labelName == null)
            {
                labelName = enumVal.ToString();
                foreach (var def in Host.NormalInts) if (def.Enum == enumVal) { labelName = def.Name; break; }
                foreach (var def in Host.HnSInts) if (def.Enum == enumVal) { labelName = def.Name; break; }
            }

            if (!_intFields.ContainsKey(enumVal))
                _intFields[enumVal] = new TextField(Host.FormatIntSetting(enumVal));

            var field = _intFields[enumVal];
            GUILayout.BeginHorizontal(new Il2CppReferenceArray<GUILayoutOption>(0));
            GUILayout.Label(labelName, MenuStyles.Label, new Il2CppReferenceArray<GUILayoutOption>(new GUILayoutOption[] { GUILayout.Width(230f) }));
            field.Draw(100, 22);

            if (field.Submitted)
            {
                Host.TryApplyIntSetting(enumVal, field.Content);
                field.Content = Host.FormatIntSetting(enumVal);
                field.ClearSubmitted();
            }
            else if (!field.IsFocused)
            {
                string cur = Host.FormatIntSetting(enumVal);
                if (field.Content != cur) field.Content = cur;
            }

            GUILayout.EndHorizontal();
        }

        private void DrawBoolRow(BoolOptionNames enumVal, string labelName = null)
        {
            if (labelName == null)
            {
                labelName = enumVal.ToString();
                foreach (var def in Host.RoleBools) if (def.Enum == enumVal) { labelName = def.Name; break; }
                foreach (var def in Host.NormalBools) if (def.Enum == enumVal) { labelName = def.Name; break; }
                foreach (var def in Host.HnSBools) if (def.Enum == enumVal) { labelName = def.Name; break; }
            }

            bool cur = Host.GetBoolSetting(enumVal);
            bool newVal = CustomToggle.Draw(cur, labelName, "settings_" + enumVal);
            if (newVal != cur) Host.ApplyBoolSetting(enumVal, newVal);
        }
    }
}