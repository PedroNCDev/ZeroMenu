using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using AmongUs.GameOptions;
using BepInEx.Unity.IL2CPP.Utils.Collections;
using BepInEx.Logging;
using HarmonyLib;
using Hazel;
using InnerNet;
using UnityEngine;
using ZeroMenu.Utilities;

namespace ZeroMenu.Functions
{
    public static class Host
    {
        public static bool DisableMeetings = false;
        public static bool DisableSabotages = false;
        public static bool DisableCloseDoors = false;
        public static bool BanMidGame = true;
        public static bool DisableCameras = false;
        public static bool DisableGameEnd = false;
        public static bool NoKillCooldown = false;
        public static bool BlockLowLevels = false;
        public static uint BlockLowLevelsMin = 20;
        public static bool DisconnectEveryone = false;
        public static bool DisableVentClean = false;
        public static bool FakeShapeshiftBubble = false;
        public static bool IsSkeldFlipped = false;
        public static bool KillAnyone = false;
        public static bool KillVanished = false;
        public static bool KillGhosts = false;
        public static bool NoOptionsLimits = false;
        public static bool VoteImmune = false;
        public static bool JudgeImmune = false;
        public static bool ReportBodySpam = false;
        public static bool DiscoParty = false;
        public static float DiscoDelay = 0.2f;
        public static bool DisableAllVotekicks = false;
        public static bool ForceColorEveryone = false;
        public static byte ForceColorId = 0;
        public static bool KillInLobbies = false;
        public static bool GodMode = false;
        public static bool GodModeAll = false;
        public static bool AutoAngel = false;
        public static float AutoAngelInterval = 0.5f;
        public static bool NoTaskMode = false;
        public static bool PreGameRoleForce = false;
        public static int PreGameImpCount = 2;
        public static int SelectedRoleIndex = 0;
        public static bool AlwaysAllowStart = false;
        public static bool ModifyStartCountdown = false;
        public static int StartCountdown = 5;
        public static bool CancelStartGame = false;
        public static int GameMode = 0;
        public static int GameModeDuration = 240;
        public static bool SkipMeeting = false;
        public static bool ForceStartGame = false;
        public static bool SwitchGameMode = false;
        public static bool EjectPlayer = false;
        public static bool SpawnMeetingHud = false;
        public static bool DespawnMeetingHud = false;

        public static readonly RoleTypes[] ValidRoles = {
            RoleTypes.Crewmate, RoleTypes.Impostor, RoleTypes.Scientist,
            RoleTypes.Engineer, RoleTypes.GuardianAngel, RoleTypes.Shapeshifter,
            RoleTypes.Tracker, RoleTypes.Noisemaker, RoleTypes.Phantom,
            RoleTypes.Detective, RoleTypes.Judge, RoleTypes.Viper
        };

        public static readonly Dictionary<byte, RoleTypes> AssignedRoles = new Dictionary<byte, RoleTypes>();

        public struct FloatSettingDef { public FloatOptionNames Enum; public string Name; }
        public struct IntSettingDef { public Int32OptionNames Enum; public string Name; }
        public struct BoolSettingDef { public BoolOptionNames Enum; public string Name; }

        public static readonly FloatSettingDef[] NormalFloats = {
            new() { Enum = FloatOptionNames.PlayerSpeedMod, Name = "Player Speed" },
            new() { Enum = FloatOptionNames.CrewLightMod, Name = "Crewmate Vision" },
            new() { Enum = FloatOptionNames.ImpostorLightMod, Name = "Impostor Vision" },
            new() { Enum = FloatOptionNames.KillCooldown, Name = "Kill Cooldown" }
        };

        public static readonly IntSettingDef[] NormalInts = {
            new() { Enum = Int32OptionNames.NumImpostors, Name = "Number of Impostors" },
            new() { Enum = Int32OptionNames.KillDistance, Name = "Kill Distance" },
            new() { Enum = Int32OptionNames.NumEmergencyMeetings, Name = "Emergency Meetings" },
            new() { Enum = Int32OptionNames.EmergencyCooldown, Name = "Emergency Cooldown" },
            new() { Enum = Int32OptionNames.DiscussionTime, Name = "Discussion Time" },
            new() { Enum = Int32OptionNames.VotingTime, Name = "Voting Time" },
            new() { Enum = Int32OptionNames.TaskBarMode, Name = "Task Bar Updates" },
            new() { Enum = Int32OptionNames.NumCommonTasks, Name = "Common Tasks" },
            new() { Enum = Int32OptionNames.NumShortTasks, Name = "Short Tasks" },
            new() { Enum = Int32OptionNames.NumLongTasks, Name = "Long Tasks" }
        };

        public static readonly BoolSettingDef[] NormalBools = {
            new() { Enum = BoolOptionNames.ConfirmImpostor, Name = "Confirm Ejects" },
            new() { Enum = BoolOptionNames.AnonymousVotes, Name = "Anonymous Votes" },
            new() { Enum = BoolOptionNames.VisualTasks, Name = "Visual Tasks" }
        };

        public static readonly FloatSettingDef[] RoleFloats = {
            new() { Enum = FloatOptionNames.ScientistCooldown, Name = "Vitals Display Cooldown" },
            new() { Enum = FloatOptionNames.ScientistBatteryCharge, Name = "Battery Duration" },
            new() { Enum = FloatOptionNames.EngineerCooldown, Name = "Vent Use Cooldown" },
            new() { Enum = FloatOptionNames.EngineerInVentMaxTime, Name = "Max Time in Vents" },
            new() { Enum = FloatOptionNames.GuardianAngelCooldown, Name = "Protect Cooldown" },
            new() { Enum = FloatOptionNames.ProtectionDurationSeconds, Name = "Protection Duration" },
            new() { Enum = FloatOptionNames.ShapeshifterDuration, Name = "Shapeshift Duration" },
            new() { Enum = FloatOptionNames.ShapeshifterCooldown, Name = "Shapeshift Cooldown" },
            new() { Enum = FloatOptionNames.NoisemakerAlertDuration, Name = "Alert Duration" },
            new() { Enum = FloatOptionNames.TrackerDuration, Name = "Tracker Duration" },
            new() { Enum = FloatOptionNames.TrackerCooldown, Name = "Tracker Cooldown" },
            new() { Enum = FloatOptionNames.TrackerDelay, Name = "Tracker Delay" },
            new() { Enum = FloatOptionNames.PhantomDuration, Name = "Phantom Duration" },
            new() { Enum = FloatOptionNames.PhantomCooldown, Name = "Phantom Cooldown" },
            new() { Enum = FloatOptionNames.DetectiveSuspectLimit, Name = "Detective Suspect Limit" },
            new() { Enum = FloatOptionNames.ViperDissolveTime, Name = "Viper Dissolve Time" },
            new() { Enum = FloatOptionNames.JudgeTaskRequirementPercentage, Name = "Tasks Required %" },
            new() { Enum = FloatOptionNames.SpiritGuideCooldownSeconds, Name = "Influencer Message Cooldown" }
        };

        public static readonly BoolSettingDef[] RoleBools = {
            new() { Enum = BoolOptionNames.ImpostorsCanSeeProtect, Name = "Protect Visible to Impostors" },
            new() { Enum = BoolOptionNames.ShapeshifterLeaveSkin, Name = "Leave Shapeshifting Evidence" },
            new() { Enum = BoolOptionNames.NoisemakerImpostorAlert, Name = "Noisemakers Alert Impostors" }
        };

        public static readonly FloatSettingDef[] HnSFloats = {
            new() { Enum = FloatOptionNames.CrewLightMod, Name = "Hider Vision" },
            new() { Enum = FloatOptionNames.ImpostorLightMod, Name = "Seeker Vision" },
            new() { Enum = FloatOptionNames.KillCooldown, Name = "Kill Cooldown" },
            new() { Enum = FloatOptionNames.PlayerSpeedMod, Name = "Player Speed" },
            new() { Enum = FloatOptionNames.EscapeTime, Name = "Hiding Time" },
            new() { Enum = FloatOptionNames.FinalEscapeTime, Name = "Final Hide Time" },
            new() { Enum = FloatOptionNames.CrewmateTimeInVent, Name = "Max Time in Vent" },
            new() { Enum = FloatOptionNames.CrewmateFlashlightSize, Name = "Hider Flashlight Size" },
            new() { Enum = FloatOptionNames.ImpostorFlashlightSize, Name = "Seeker Flashlight Size" },
            new() { Enum = FloatOptionNames.SeekerFinalSpeed, Name = "Final Hide Seeker Speed" },
            new() { Enum = FloatOptionNames.MaxPingTime, Name = "Ping Interval" }
        };

        public static readonly IntSettingDef[] HnSInts = {
            new() { Enum = Int32OptionNames.KillDistance, Name = "Kill Distance" },
            new() { Enum = Int32OptionNames.NumShortTasks, Name = "Short Tasks" },
            new() { Enum = Int32OptionNames.NumCommonTasks, Name = "Common Tasks" },
            new() { Enum = Int32OptionNames.NumLongTasks, Name = "Long Tasks" },
            new() { Enum = Int32OptionNames.CrewmateVentUses, Name = "Max Vent Uses" }
        };

        public static readonly BoolSettingDef[] HnSBools = {
            new() { Enum = BoolOptionNames.UseFlashlight, Name = "Flashlight Mode" },
            new() { Enum = BoolOptionNames.SeekerFinalMap, Name = "Final Hide Seeker Map" },
            new() { Enum = BoolOptionNames.SeekerPings, Name = "Final Hide Pings" },
            new() { Enum = BoolOptionNames.ShowCrewmateNames, Name = "Show Names" }
        };

        private static float _discoTimer = 0f;
        private static float _spamTimer = 0f;
        private static float _godModeTimer = -1f;
        private static float _godModeAllTimer = -1f;
        private static float _autoAngelTimer = -1f;
        private static byte _lastAngelTarget = byte.MaxValue;
        private static float _forceColorTimer = 0f;
        private static float _modeTimer = 0f;
        private static bool _modeOver = false;
        private static bool _brApplied = false;
        private static float _speedrunTimer = 0f;

        public static void SetSkeldFlip(bool value)
        {
            if (AmongUsClient.Instance == null || IsSkeldFlipped == value) return;
            var temp = AmongUsClient.Instance.ShipPrefabs[3];
            AmongUsClient.Instance.ShipPrefabs[3] = AmongUsClient.Instance.ShipPrefabs[0];
            AmongUsClient.Instance.ShipPrefabs[0] = temp;
            IsSkeldFlipped = value;
        }

        public static void ForceVictory(GameOverReason reason)
        {
            DisableGameEnd = false;
            GameManager.Instance.RpcEndGame(reason, false);
        }

        public static void SpawnMap(byte mapId)
        {
            if (ShipStatus.Instance != null) ShipStatus.Instance.Despawn();
            try
            {
                if (AmongUsClient.Instance != null && mapId < AmongUsClient.Instance.ShipPrefabs.Count)
                {
                    var prefab = AmongUsClient.Instance.ShipPrefabs[mapId];
                    var go = prefab.Asset.Cast<GameObject>();
                    if (go != null)
                    {
                        var ship = UnityEngine.Object.Instantiate(go).GetComponent<ShipStatus>();
                        AmongUsClient.Instance.Spawn(ship, -2, 0);
                    }
                }
            }
            catch { }
        }

        public static void SpawnLobby()
        {
            if (LobbyBehaviour.Instance == null)
            {
                LobbyBehaviour.Instance = UnityEngine.Object.Instantiate(DestroyableSingleton<GameStartManager>.Instance.LobbyPrefab);
                AmongUsClient.Instance.Spawn(LobbyBehaviour.Instance, -2, 0);
            }
        }

        public static void ShapeshiftAll(PlayerControl target)
        {
            if (target == null) return;
            AmongUsClient.Instance.StartCoroutine(ShapeshiftAllRoutine(target).WrapToIl2Cpp());
        }

        private static IEnumerator ShapeshiftAllRoutine(PlayerControl target)
        {
            foreach (var player in PlayerControl.AllPlayerControls)
            {
                if (player != null && !player.AmOwner)
                {
                    var batch = new BatchedMessage(-1);
                    byte realColor = (byte)player.Data.DefaultOutfit.ColorId;
                    bool fakeBubble = FakeShapeshiftBubble && AmongUsClient.Instance.AmHost;
                    byte bubbleColor = realColor;

                    if (fakeBubble)
                    {
                        bubbleColor = (byte)UnityEngine.Random.Range(0, 18);
                        if (bubbleColor == realColor) bubbleColor = (byte)((realColor + 1) % 18);
                        batch.QueueSetColor(player, bubbleColor);
                    }

                    if (player.Data.RoleType != RoleTypes.Shapeshifter)
                    {
                        var prev = player.Data.RoleType;
                        batch.QueueSetRole(player, RoleTypes.Shapeshifter, true);
                        batch.QueueShapeshift(player, target, true);
                        batch.QueueSetRole(player, prev, true);
                    }
                    else batch.QueueShapeshift(player, target, true);

                    if (fakeBubble) batch.QueueSetColor(player, realColor);
                    batch.FinishBatch();
                    yield return new WaitForSeconds(0.15f);
                }
            }
        }

        public static void RevertAllShapeshifts()
        {
            AmongUsClient.Instance.StartCoroutine(RevertAllShapeshiftsRoutine().WrapToIl2Cpp());
        }

        private static IEnumerator RevertAllShapeshiftsRoutine()
        {
            foreach (var player in PlayerControl.AllPlayerControls)
            {
                if (player != null && !player.AmOwner)
                {
                    var batch = new BatchedMessage(-1);
                    batch.QueueShapeshift(player, player, false);
                    batch.FinishBatch();
                    yield return new WaitForSeconds(0.15f);
                }
            }
        }

        public static void KillAll(RoleTeamTypes? team = null)
        {
            foreach (var player in PlayerControl.AllPlayerControls)
            {
                if (player == null || player.Data == null || player.Data.IsDead || player == PlayerControl.LocalPlayer) continue;
                if (team != null && player.Data.Role.TeamType != team) continue;
                try { player.RpcMurderPlayer(player, true); } catch { }
            }
        }

        public static void KickAll()
        {
            if (!AmongUsClient.Instance.AmHost || PlayerControl.AllPlayerControls == null) return;
            foreach (var pc in PlayerControl.AllPlayerControls)
                if (pc != null && !pc.AmOwner && pc.Data != null)
                    AmongUsClient.Instance.KickPlayer(pc.OwnerId, false);
        }

        public static void ForceEndImpDisconnect()
        {
            if (GameManager.Instance == null || !AmongUsClient.Instance.AmHost) return;
            GameManager.Instance.RpcEndGame((GameOverReason)5, false);
        }

        public static void ProtectEveryone(bool notify)
        {
            if (AmongUsClient.Instance == null || !AmongUsClient.Instance.AmHost) return;
            if (ShipStatus.Instance == null || LobbyBehaviour.Instance != null) return;

            PlayerControl local = PlayerControl.LocalPlayer;
            if (local == null || local.Data == null) return;

            foreach (PlayerControl target in PlayerControl.AllPlayerControls)
            {
                if (target == null || target.Data == null || target.PlayerId >= 100) continue;
                if (target.Data.Disconnected || target.Data.IsDead || target.protectedByGuardianId >= 0) continue;
                try { local.RpcProtectPlayer(target, target.Data.DefaultOutfit.ColorId); } catch { }
            }
        }

        public static void IncreaseOption(Int32OptionNames option, int amount) { GameOptionsManager.Instance.currentHostOptions.SetInt(option, GameOptionsManager.Instance.currentHostOptions.GetInt(option) + amount); }
        public static void DecreaseOption(Int32OptionNames option, int amount) { GameOptionsManager.Instance.currentHostOptions.SetInt(option, Mathf.Max(1, GameOptionsManager.Instance.currentHostOptions.GetInt(option) - amount)); }
        public static void IncreaseFloatOption(FloatOptionNames option, float amount) { GameOptionsManager.Instance.currentHostOptions.SetFloat(option, GameOptionsManager.Instance.currentHostOptions.GetFloat(option) + amount); }
        public static void DecreaseFloatOption(FloatOptionNames option, float amount) { GameOptionsManager.Instance.currentHostOptions.SetFloat(option, Mathf.Max(0.1f, GameOptionsManager.Instance.currentHostOptions.GetFloat(option) - amount)); }

        public static void ApplyZeroCd() { GameOptionsManager.Instance.currentHostOptions.SetFloat(FloatOptionNames.KillCooldown, 0f); }
        public static void BreakSettings()
        {
            GameOptionsManager.Instance.currentHostOptions.SetFloat(FloatOptionNames.PlayerSpeedMod, 0f);
            GameOptionsManager.Instance.currentHostOptions.SetInt(Int32OptionNames.NumImpostors, 67);
            GameOptionsManager.Instance.currentHostOptions.SetInt(Int32OptionNames.MaxPlayers, 67);
        }

        public static void SetAllPlayersRole(RoleTypes role)
        {
            if (PlayerControl.AllPlayerControls == null) return;
            int queued = 0;
            foreach (var pc in PlayerControl.AllPlayerControls)
            {
                if (pc?.Data == null || pc.Data.Disconnected) continue;
                AssignedRoles[pc.PlayerId] = role;
                queued++;
            }
            ZeroMenu.Plugin.notifications.Send("Role Assign", $"Queued {role} for {queued} players (applies next game).", 3f);
        }

        public static void SetPlayerRole(PlayerControl player, RoleTypes role)
        {
            if (player?.Data == null) return;
            AssignedRoles[player.PlayerId] = role;
            ZeroMenu.Plugin.notifications.Send("Role Assign", $"Queued {role} for {player.Data.PlayerName} (applies next game).", 3f);
        }

        public static string GetPlayerCurrentRole(PlayerControl player)
        {
            if (player?.Data == null) return "?";
            if (AssignedRoles.TryGetValue(player.PlayerId, out RoleTypes queuedRole))
            {
                return $"{player.Data.RoleType} -> {queuedRole}";
            }
            return player.Data.RoleType.ToString();
        }

        public static bool TryApplyFloatSetting(FloatOptionNames option, string input)
        {
            if (HostSettings.Instance == null) return false;
            if (float.TryParse(input, NumberStyles.Float, CultureInfo.InvariantCulture, out float v))
            {
                HostSettings.Instance.SetFloat(option, v);
                return true;
            }
            return false;
        }

        public static bool TryApplyIntSetting(Int32OptionNames option, string input)
        {
            if (HostSettings.Instance == null) return false;
            if (int.TryParse(input, NumberStyles.Integer, CultureInfo.InvariantCulture, out int v))
            {
                HostSettings.Instance.SetInt(option, v);
                return true;
            }
            return false;
        }

        public static void ApplyBoolSetting(BoolOptionNames option, bool value)
        {
            HostSettings.Instance?.SetBool(option, value);
        }

        public static void ApplyByteSetting(ByteOptionNames option, byte value)
        {
            HostSettings.Instance?.SetByte(option, value);
        }

        public static string FormatFloatSetting(FloatOptionNames option)
        {
            return HostSettings.Instance?.GetFloat(option).ToString("F2", CultureInfo.InvariantCulture) ?? "0.00";
        }

        public static string FormatIntSetting(Int32OptionNames option)
        {
            return HostSettings.Instance?.GetInt(option).ToString(CultureInfo.InvariantCulture) ?? "0";
        }

        public static bool GetBoolSetting(BoolOptionNames option)
        {
            return HostSettings.Instance != null && HostSettings.Instance.GetBool(option);
        }

        public static byte GetByteSetting(ByteOptionNames option)
        {
            return HostSettings.Instance?.GetByte(option) ?? 0;
        }

        public static bool IsHostReady()
        {
            return HostSettings.Instance != null 
                && AmongUsClient.Instance != null 
                && GameOptionsManager.Instance != null 
                && GameOptionsManager.Instance.currentGameOptions != null;
        }

        public static void Tick()
        {
            if (!AmongUsClient.Instance) return;

            if (DiscoParty && AmongUsClient.Instance.AmHost)
            {
                _discoTimer += Time.deltaTime;
                if (_discoTimer >= DiscoDelay)
                {
                    _discoTimer = 0f;
                    foreach (var p in PlayerControl.AllPlayerControls)
                        if (p.Data != null) p.RpcSetColor((byte)UnityEngine.Random.Range(0, 18));
                }
            }

            if (ForceColorEveryone && AmongUsClient.Instance.AmHost)
            {
                _forceColorTimer += Time.deltaTime;
                if (_forceColorTimer >= 0.5f)
                {
                    _forceColorTimer = 0f;
                    foreach (var p in PlayerControl.AllPlayerControls)
                        if (p.Data != null && p.Data.DefaultOutfit.ColorId != ForceColorId)
                            p.RpcSetColor(ForceColorId);
                }
            }

            if (GodMode && AmongUsClient.Instance.AmHost && ShipStatus.Instance != null && LobbyBehaviour.Instance == null)
            {
                var local = PlayerControl.LocalPlayer;
                if (local != null && local.Data != null && !local.Data.IsDead && local.protectedByGuardianId < 0)
                {
                    float now = Time.unscaledTime;
                    if (_godModeTimer < 0 || now >= _godModeTimer)
                    {
                        _godModeTimer = now + 0.2f;
                        try { local.RpcProtectPlayer(local, local.Data.DefaultOutfit.ColorId); } catch { }
                    }
                }
            }

            if (GodModeAll && AmongUsClient.Instance.AmHost && ShipStatus.Instance != null && LobbyBehaviour.Instance == null)
            {
                float now = Time.unscaledTime;
                if (_godModeAllTimer < 0 || now >= _godModeAllTimer)
                {
                    _godModeAllTimer = now + 0.2f;
                    ProtectEveryone(false);
                }
            }

            if (AutoAngel && PlayerControl.LocalPlayer != null && PlayerControl.LocalPlayer.Data != null && PlayerControl.LocalPlayer.Data.Role.Role == RoleTypes.GuardianAngel)
            {
                float now = Time.unscaledTime;
                if (_autoAngelTimer < 0 || now >= _autoAngelTimer)
                {
                    _autoAngelTimer = now + Mathf.Clamp(AutoAngelInterval, 0.1f, 2f);
                    var targets = PlayerControl.AllPlayerControls.ToArray().Where(p => p != null && p != PlayerControl.LocalPlayer && p.Data != null && !p.Data.IsDead && !p.Data.Disconnected && p.PlayerId != _lastAngelTarget).ToList();
                    if (targets.Count > 0)
                    {
                        var target = targets[UnityEngine.Random.Range(0, targets.Count)];
                        try { PlayerControl.LocalPlayer.CmdCheckProtect(target); _lastAngelTarget = target.PlayerId; } catch { }
                    }
                }
            }

            if (GameMode != 0 && ShipStatus.Instance != null && !_modeOver && AmongUsClient.Instance.AmHost)
            {
                if (GameMode == 2 && !_brApplied)
                {
                    _brApplied = true;
                    foreach (var p in PlayerControl.AllPlayerControls) p.RpcSetRole(RoleTypes.Impostor, true);
                }
                if (GameMode == 1)
                {
                    _speedrunTimer += Time.deltaTime;
                    if (_speedrunTimer >= 1f)
                    {
                        _speedrunTimer = 0f;
                        foreach (var p in PlayerControl.AllPlayerControls)
                        {
                            if (p == null || p.Data == null || p.Data.IsDead || p.Data.Disconnected) continue;
                            foreach (var task in p.myTasks)
                            {
                                if (task != null && !task.IsComplete) { p.RpcCompleteTask(task.Id); break; }
                            }
                        }
                    }
                }
                _modeTimer += Time.deltaTime;
                if (_modeTimer >= GameModeDuration)
                {
                    _modeOver = true;
                    GameManager.Instance.RpcEndGame(GameOverReason.CrewmatesByTask, false);
                }
            }

            if (ReportBodySpam && AmongUsClient.Instance.AmHost)
            {
                _spamTimer += Time.deltaTime;
                if (_spamTimer >= 0.1f)
                {
                    _spamTimer = 0f;
                    PlayerControl.LocalPlayer.CmdReportDeadBody(null);
                }
            }

            if (SkipMeeting && MeetingHud.Instance != null)
            {
                MeetingHud.Instance.RpcVotingComplete(new Il2CppInterop.Runtime.InteropTypes.Arrays.Il2CppStructArray<MeetingHud.VoterState>(0L), null, true, false, 0);
                SkipMeeting = false;
            }

            if (ForceStartGame && Utils.IsLobby && AmongUsClient.Instance.AmHost)
            {
                AmongUsClient.Instance.SendStartGame();
                ForceStartGame = false;
            }
        }

        [HarmonyPatch(typeof(PlayerControl), "ReportDeadBody")]
        public static class DisableMeetings_Patch { public static bool Prefix() => !DisableMeetings; }

        [HarmonyPatch(typeof(SabotageSystemType), "UpdateSystem")]
        public static class DisableSabotages_Patch { public static bool Prefix() => !DisableSabotages; }

        [HarmonyPatch(typeof(ShipStatus), "CloseDoorsOfType")]
        public static class DisableCloseDoors_Patch { public static bool Prefix() => !DisableCloseDoors; }

        [HarmonyPatch(typeof(PlayerControl), "SetLevel")]
        public static class BlockLowLevels_Patch
        {
            public static void Prefix(PlayerControl __instance, uint level)
            {
                if (!BlockLowLevels || !AmongUsClient.Instance.AmHost || __instance == PlayerControl.LocalPlayer || level > BlockLowLevelsMin) return;
                AmongUsClient.Instance.KickPlayer(__instance.OwnerId, false);
                ZeroMenu.Plugin.notifications.Send("Host", $"{__instance.Data.PlayerName} kicked for low level ({level}).", 5f);
            }
        }

        [HarmonyPatch(typeof(InnerNetClient), "CanBan")]
        public static class BanMidGame_Patch
        {
            public static bool Prefix(InnerNetClient __instance, ref bool __result)
            {
                if (!BanMidGame) return true;
                __result = __instance.AmHost;
                return false;
            }
        }

        [HarmonyPatch(typeof(PlayerControl), "SetKillTimer")]
        public static class NoKillCooldown_Patch
        {
            public static void Prefix(PlayerControl __instance, ref float time)
            {
                if (!NoKillCooldown || __instance != PlayerControl.LocalPlayer) return;
                time = 0f;
            }
        }

        [HarmonyPatch(typeof(SecurityCameraSystemType), "UpdateSystem")]
        public static class DisableCameras_Patch
        {
            public static void Postfix(PlayerControl player, MessageReader msgReader)
            {
                if (!DisableCameras || !AmongUsClient.Instance.AmHost || player.OwnerId == AmongUsClient.Instance.HostId) return;
                if (ShipStatus.Instance != null)
                {
                    var comms = ShipStatus.Instance.Systems[SystemTypes.Comms].Cast<HudOverrideSystemType>();
                    if (comms != null && comms.IsActive) return;

                    msgReader.Position--;
                    byte operation = msgReader.ReadByte();
                    var writer = MessageWriter.Get(SendOption.Reliable);
                    writer.StartMessage(14);
                    writer.Write(operation == 1);
                    writer.EndMessage();
                    var batch = new BatchedMessage(player.OwnerId);
                    batch.QueueDataFlag(ShipStatus.Instance.NetId, writer);
                    batch.FinishBatch();
                    writer.Recycle();
                }
            }
        }

        [HarmonyPatch(typeof(GameManager), "RpcEndGame")]
        public static class DisableGameEnd_Patch { public static bool Prefix() => !DisableGameEnd; }

        [HarmonyPatch(typeof(VentilationSystem), "PerformVentOp")]
        public static class DisableVentClean_Patch { public static bool Prefix(VentilationSystem.Operation op) { if (!DisableVentClean) return true; return op != VentilationSystem.Operation.StartCleaning && op != VentilationSystem.Operation.StopCleaning && op != VentilationSystem.Operation.BootImpostors; } }

        [HarmonyPatch(typeof(PlayerControl), "CmdCheckShapeshift")]
        public static class FakeShapeshiftBubble_Patch
        {
            public static bool Prefix(PlayerControl __instance, PlayerControl target, bool shouldAnimate)
            {
                if (!FakeShapeshiftBubble || !shouldAnimate || __instance != PlayerControl.LocalPlayer) return true;
                int origColor = __instance.Data.DefaultOutfit.ColorId;
                int newColor = UnityEngine.Random.Range(0, 18);
                var batch = new BatchedMessage(-1);
                batch.QueueSetColor(__instance, (byte)newColor);
                batch.QueueShapeshift(__instance, target, true);
                batch.QueueSetColor(__instance, (byte)origColor);
                batch.FinishBatch();
                return false;
            }
        }

        [HarmonyPatch(typeof(ImpostorRole), "IsValidTarget")]
        public static class KillAnyone_Patch
        {
            public static void Postfix(NetworkedPlayerInfo target, ref bool __result)
            {
                if (KillAnyone && target != null && !target.IsDead && !target.Disconnected) __result = true;
                if (KillGhosts && target != null && target.IsDead && !target.Disconnected) __result = true;
            }
        }

        [HarmonyPatch(typeof(PhantomRole), "IsValidTarget")]
        public static class KillVanished_Patch
        {
            public static void Postfix(NetworkedPlayerInfo target, ref bool __result)
            {
                if (KillVanished) __result = target != null && !target.Disconnected && target.Object != null;
            }
        }

        [HarmonyPatch(typeof(IGameOptionsExtensions), "GetAdjustedNumImpostors")]
        public static class NoOptionsLimits_Patch
        {
            public static bool Prefix(ref int __result)
            {
                if (!NoOptionsLimits) return true;
                __result = GameOptionsManager.Instance.CurrentGameOptions.NumImpostors;
                return false;
            }
        }

        [HarmonyPatch(typeof(MeetingHud), "CheckForEndVoting")]
        public static class VoteImmune_Patch
        {
            public static bool Prefix(MeetingHud __instance)
            {
                if (!VoteImmune) return true;
                if (!__instance.playerStates.All(ps => ps.AmDead || ps.DidVote)) return true;
                var max = __instance.CalculateVotes().MaxPair(out var tie);
                var exiled = GameData.Instance.AllPlayers.ToArray().FirstOrDefault(v => !tie && v.PlayerId == max.Key);
                if (exiled != null && exiled == PlayerControl.LocalPlayer.Data) exiled = null;
                var states = new MeetingHud.VoterState[__instance.playerStates.Length];
                for (var index = 0; __instance.playerStates.Length > index; ++index)
                {
                    var playerState = __instance.playerStates[index];
                    states[index] = new MeetingHud.VoterState { VoterId = playerState.PlayerId, VotedForId = playerState.VotedForId };
                }
                __instance.RpcVotingComplete(states, exiled, tie, false, 0);
                return false;
            }
        }

        [HarmonyPatch(typeof(LogicRoleSelectionNormal), "AssignRolesFromList")]
        public static class PreGameRoleForce_Patch
        {
            public static bool Prefix(ref Il2CppSystem.Collections.Generic.List<NetworkedPlayerInfo> players, ref Il2CppSystem.Collections.Generic.List<RoleTypes> roleList, ref int rolesAssigned)
            {
                if (!PreGameRoleForce || !AmongUsClient.Instance.AmHost) return true;
                try
                {
                    var all = PlayerControl.AllPlayerControls.ToArray().Where(p => p?.Data != null && !p.Data.Disconnected && !p.Data.IsDead).ToList();
                    int impCount = Mathf.Clamp(PreGameImpCount, 1, all.Count - 1);
                    var rng = new System.Random();
                    var imps = all.OrderBy(_ => rng.Next()).Take(impCount).ToList();
                    var crew = all.Where(p => !imps.Contains(p)).ToList();

                    var impData = new Il2CppSystem.Collections.Generic.List<NetworkedPlayerInfo>();
                    foreach (var i in imps) impData.Add(i.Data);
                    var crewData = new Il2CppSystem.Collections.Generic.List<NetworkedPlayerInfo>();
                    foreach (var c in crew) crewData.Add(c.Data);

                    var opts = GameOptionsManager.Instance.CurrentGameOptions;
                    GameManager.Instance.LogicRoleSelection.AssignRolesForTeam(impData, opts, (RoleTeamTypes)1, int.MaxValue, new Il2CppSystem.Nullable<RoleTypes>());
                    GameManager.Instance.LogicRoleSelection.AssignRolesForTeam(crewData, opts, (RoleTeamTypes)0, int.MaxValue, new Il2CppSystem.Nullable<RoleTypes>((RoleTypes)0));
                    foreach (var pc in all) pc.Data.Role?.Initialize(pc);
                    return false;
                }
                catch { return true; }
            }
        }

        [HarmonyPatch(typeof(RoleManager), nameof(RoleManager.SelectRoles))]
        public static class RoleManagerHook
        {
            [HarmonyPrefix]
            public static bool Prefix(RoleManager __instance)
            {
                if (!AmongUsClient.Instance.AmHost) return true;

                var allPlayers = PlayerControl.AllPlayerControls.ToArray()
                    .Where(p => p != null && p.Data != null)
                    .ToList();
                var assignedPlayers = new List<byte>();

                var roleOptions = GameOptionsManager.Instance.currentGameOptions.RoleOptions;

                AssignRolesByChance(roleOptions.GetChancePerGame(RoleTypes.Shapeshifter), RoleTypes.Shapeshifter, allPlayers, assignedPlayers);
                AssignRolesByChance(roleOptions.GetChancePerGame(RoleTypes.Phantom), RoleTypes.Phantom, allPlayers, assignedPlayers);
                AssignRolesByChance(roleOptions.GetChancePerGame(RoleTypes.Viper), RoleTypes.Viper, allPlayers, assignedPlayers);
                AssignRolesByChance(100, RoleTypes.Impostor, allPlayers, assignedPlayers);
                AssignRolesByChance(roleOptions.GetChancePerGame(RoleTypes.Scientist), RoleTypes.Scientist, allPlayers, assignedPlayers);
                AssignRolesByChance(roleOptions.GetChancePerGame(RoleTypes.Tracker), RoleTypes.Tracker, allPlayers, assignedPlayers);
                AssignRolesByChance(roleOptions.GetChancePerGame(RoleTypes.Noisemaker), RoleTypes.Noisemaker, allPlayers, assignedPlayers);
                AssignRolesByChance(roleOptions.GetChancePerGame(RoleTypes.Detective), RoleTypes.Detective, allPlayers, assignedPlayers);
                AssignRolesByChance(roleOptions.GetChancePerGame(RoleTypes.Judge), RoleTypes.Judge, allPlayers, assignedPlayers);
                AssignRolesByChance(roleOptions.GetChancePerGame(RoleTypes.Engineer), RoleTypes.Engineer, allPlayers, assignedPlayers);
                AssignRolesByChance(100, RoleTypes.Crewmate, allPlayers, assignedPlayers);

                foreach (var p in allPlayers)
                {
                    if (AssignedRoles.TryGetValue(p.PlayerId, out RoleTypes trueRole))
                    {
                        if (IsHideAndSeek())
                        {
                            trueRole = trueRole switch
                            {
                                RoleTypes.Shapeshifter => RoleTypes.Impostor,
                                RoleTypes.Phantom      => RoleTypes.Impostor,
                                RoleTypes.Viper        => RoleTypes.Impostor,
                                RoleTypes.Tracker      => RoleTypes.Engineer,
                                RoleTypes.Noisemaker   => RoleTypes.Engineer,
                                RoleTypes.Detective    => RoleTypes.Engineer,
                                RoleTypes.Scientist    => RoleTypes.Engineer,
                                RoleTypes.Judge        => RoleTypes.Engineer,
                                RoleTypes.Crewmate     => RoleTypes.Engineer,
                                _ => trueRole
                            };
                        }

                        p.RpcSetRole(trueRole, false);
                    }
                }

                return false;
            }

            private static void AssignRolesByChance(int roleChance, RoleTypes role, List<PlayerControl> allPlayers, List<byte> assignedPlayers)
            {
                if (roleChance <= 0) return;
                int maxCount = GameOptionsManager.Instance.currentGameOptions.RoleOptions.GetNumPerGame(role);
                if (maxCount <= 0) return;

                int assigned = 0;
                foreach (var player in allPlayers)
                {
                    if (assigned >= maxCount) break;
                    if (player == null || player.Data == null) continue;
                    if (assignedPlayers.Contains(player.PlayerId)) continue;

                    if (roleChance >= 100 || UnityEngine.Random.Range(0, 100) < roleChance)
                    {
                        player.RpcSetRole(role, false);
                        assignedPlayers.Add(player.PlayerId);
                        assigned++;
                    }
                }
            }

            private static bool IsHideAndSeek()
            {
                var opts = GameOptionsManager.Instance.currentGameOptions;
                return opts != null && opts.GameMode == GameModes.HideNSeek;
            }
        }

        [HarmonyPatch(typeof(InnerNetClient), "Update")]
        public static class HostUpdate_Patch
        {
            public static void Postfix()
            {
                Tick();
            }
        }
    }

    public sealed class HostSettings
    {
        public static HostSettings Instance { get; private set; }

        private readonly ManualLogSource _log;
        private readonly Queue<Action> _syncQueue = new Queue<Action>();

        public sealed class RolePreset
        {
            public int Count;
            public int Chance;
        }

        public sealed class HostPreset
        {
            public string Name = "Preset";

            public float PlayerSpeed;
            public float CrewmateVision;
            public float ImpostorVision;
            public float KillCooldown;
            public int KillDistance;
            public int NumImpostors;
            public int MaxPlayers;
            public byte MapId;
            public bool VisualTasks;
            public bool ConfirmImpostor;
            public bool AnonymousVotes;
            public int NumEmergencyMeetings;
            public int EmergencyCooldown;
            public int DiscussionTime;
            public int VotingTime;
            public int TaskBarMode;
            public int NumCommonTasks;
            public int NumLongTasks;
            public int NumShortTasks;

            public float ShapeshifterCooldown;
            public float ShapeshifterDuration;
            public bool ShapeshifterLeaveSkin;
            public float GuardianAngelCooldown;
            public bool GuardianAngelProtectVisible;
            public float GuardianAngelProtectDuration;
            public float ScientistCooldown;
            public float ScientistBatteryCharge;
            public float EngineerCooldown;
            public float EngineerInVentMaxTime;
            public float PhantomCooldown;
            public float PhantomDuration;
            public float TrackerCooldown;
            public float TrackerDuration;
            public float TrackerDelay;
            public float NoisemakerAlertDuration;
            public bool NoisemakerImpostorAlert;
            public float ViperDissolveTime;
            public float DetectiveSuspectLimit;
            public float JudgeTaskRequirement;
            public float InfluencerMessageCooldown;

            public Dictionary<RoleTypes, RolePreset> RoleRates = new Dictionary<RoleTypes, RolePreset>();
        }

        public List<HostPreset> Presets { get; } = new List<HostPreset>();
        public int SelectedPreset { get; set; } = 0;

        private HostSettings(ManualLogSource log)
        {
            _log = log;
        }

        public static void Initialize(ManualLogSource log)
        {
            if (Instance != null) return;
            Instance = new HostSettings(log);
        }

        public void SyncAllSettings()
        {
            if (!AmongUsClient.Instance || PlayerControl.LocalPlayer == null) return;

            _syncQueue.Enqueue(() =>
            {
                try
                {
                    if (AmongUsClient.Instance.GameState == InnerNetClient.GameStates.Joined)
                    {
                        var opts = GameOptionsManager.Instance.currentGameOptions;
                        if (opts != null)
                        {
                            AmongUsClient.Instance.StartRpcImmediately(
                                PlayerControl.LocalPlayer.NetId,
                                (byte)RpcCalls.SyncSettings,
                                SendOption.Reliable,
                                -1);
                        }
                    }
                    else if (AmongUsClient.Instance.GameState == InnerNetClient.GameStates.Started)
                    {
                        AmongUsClient.Instance.StartRpcImmediately(
                            PlayerControl.LocalPlayer.NetId,
                            (byte)RpcCalls.SyncSettings,
                            SendOption.Reliable,
                            -1);
                    }
                }
                catch (Exception e)
                {
                    _log.LogError($"[SyncAllSettings] {e}");
                }
            });
        }

        public void FlushSyncQueue()
        {
            while (_syncQueue.Count > 0)
                _syncQueue.Dequeue()?.Invoke();
        }

        public int GetInt(Int32OptionNames n)     
        {
            if (GameOptionsManager.Instance == null || GameOptionsManager.Instance.currentGameOptions == null) return 0;
            return GameOptionsManager.Instance.currentGameOptions.GetInt(n);
        }
        
        public float GetFloat(FloatOptionNames n) 
        {
            if (GameOptionsManager.Instance == null || GameOptionsManager.Instance.currentGameOptions == null) return 0f;
            return GameOptionsManager.Instance.currentGameOptions.GetFloat(n);
        }
        
        public bool GetBool(BoolOptionNames n)    
        {
            if (GameOptionsManager.Instance == null || GameOptionsManager.Instance.currentGameOptions == null) return false;
            return GameOptionsManager.Instance.currentGameOptions.GetBool(n);
        }
        
        public byte GetByte(ByteOptionNames n)    
        {
            if (GameOptionsManager.Instance == null || GameOptionsManager.Instance.currentGameOptions == null) return 0;
            return GameOptionsManager.Instance.currentGameOptions.GetByte(n);
        }

        public void SetInt(Int32OptionNames n, int v)
        {
            if (GameOptionsManager.Instance == null || GameOptionsManager.Instance.currentGameOptions == null) return;
            GameOptionsManager.Instance.currentGameOptions.SetInt(n, v);
            SyncAllSettings();
        }
        
        public void SetFloat(FloatOptionNames n, float v)
        {
            if (GameOptionsManager.Instance == null || GameOptionsManager.Instance.currentGameOptions == null) return;
            GameOptionsManager.Instance.currentGameOptions.SetFloat(n, v);
            SyncAllSettings();
        }
        
        public void SetBool(BoolOptionNames n, bool v)
        {
            if (GameOptionsManager.Instance == null || GameOptionsManager.Instance.currentGameOptions == null) return;
            GameOptionsManager.Instance.currentGameOptions.SetBool(n, v);
            SyncAllSettings();
        }
        
        public void SetByte(ByteOptionNames n, byte v)
        {
            if (GameOptionsManager.Instance == null || GameOptionsManager.Instance.currentGameOptions == null) return;
            GameOptionsManager.Instance.currentGameOptions.SetByte(n, v);
            SyncAllSettings();
        }

        public void SetRoleAmount(RoleTypes type, int amount)
        {
            var opts = GameOptionsManager.Instance.currentGameOptions;
            if (opts == null) return;
            var roleOpts = opts.RoleOptions;
            int maxCount = roleOpts.GetNumPerGame(type);

            if (amount > maxCount) roleOpts.SetRoleRate(type, amount, 100);
            else if (amount > 0)   roleOpts.SetRoleRate(type, maxCount, 100);
            SyncAllSettings();
        }

        public int GetRoleCount(RoleTypes role, bool excludeSelf = false)
        {
            if (!PlayerControl.LocalPlayer) return 0;

            if (excludeSelf)
            {
                var all = PlayerControl.AllPlayerControls.ToArray();
                return all.Count(p => p != PlayerControl.LocalPlayer
                                   && p.Data != null
                                   && p.Data.Role != null
                                   && p.Data.Role.Role == role);
            }
            return PlayerControl.AllPlayerControls.ToArray()
                .Count(p => p.Data?.Role != null && p.Data.Role.Role == role);
        }

        public bool CapturePreset(HostPreset p)
        {
            var opts = GameOptionsManager.Instance.currentGameOptions;
            if (opts == null) return false;

            p.PlayerSpeed        = opts.GetFloat(FloatOptionNames.PlayerSpeedMod);
            p.CrewmateVision     = opts.GetFloat(FloatOptionNames.CrewLightMod);
            p.ImpostorVision     = opts.GetFloat(FloatOptionNames.ImpostorLightMod);
            p.KillCooldown       = opts.GetFloat(FloatOptionNames.KillCooldown);
            p.KillDistance       = opts.GetInt(Int32OptionNames.KillDistance);
            p.NumImpostors       = opts.GetInt(Int32OptionNames.NumImpostors);
            p.MaxPlayers         = opts.GetInt(Int32OptionNames.MaxPlayers);
            p.MapId              = opts.GetByte(ByteOptionNames.MapId);
            p.VisualTasks        = opts.GetBool(BoolOptionNames.VisualTasks);
            p.ConfirmImpostor    = opts.GetBool(BoolOptionNames.ConfirmImpostor);
            p.AnonymousVotes     = opts.GetBool(BoolOptionNames.AnonymousVotes);
            p.NumEmergencyMeetings = opts.GetInt(Int32OptionNames.NumEmergencyMeetings);
            p.EmergencyCooldown  = opts.GetInt(Int32OptionNames.EmergencyCooldown);
            p.DiscussionTime     = opts.GetInt(Int32OptionNames.DiscussionTime);
            p.VotingTime         = opts.GetInt(Int32OptionNames.VotingTime);
            p.TaskBarMode        = opts.GetInt(Int32OptionNames.TaskBarMode);
            p.NumCommonTasks     = opts.GetInt(Int32OptionNames.NumCommonTasks);
            p.NumLongTasks       = opts.GetInt(Int32OptionNames.NumLongTasks);
            p.NumShortTasks      = opts.GetInt(Int32OptionNames.NumShortTasks);

            p.ShapeshifterCooldown         = opts.GetFloat(FloatOptionNames.ShapeshifterCooldown);
            p.ShapeshifterDuration         = opts.GetFloat(FloatOptionNames.ShapeshifterDuration);
            p.ShapeshifterLeaveSkin        = opts.GetBool(BoolOptionNames.ShapeshifterLeaveSkin);
            p.GuardianAngelCooldown        = opts.GetFloat(FloatOptionNames.GuardianAngelCooldown);
            p.GuardianAngelProtectVisible  = opts.GetBool(BoolOptionNames.ImpostorsCanSeeProtect);
            p.GuardianAngelProtectDuration = opts.GetFloat(FloatOptionNames.ProtectionDurationSeconds);
            p.ScientistCooldown            = opts.GetFloat(FloatOptionNames.ScientistCooldown);
            p.ScientistBatteryCharge       = opts.GetFloat(FloatOptionNames.ScientistBatteryCharge);
            p.EngineerCooldown             = opts.GetFloat(FloatOptionNames.EngineerCooldown);
            p.EngineerInVentMaxTime        = opts.GetFloat(FloatOptionNames.EngineerInVentMaxTime);
            p.PhantomCooldown              = opts.GetFloat(FloatOptionNames.PhantomCooldown);
            p.PhantomDuration              = opts.GetFloat(FloatOptionNames.PhantomDuration);
            p.TrackerCooldown              = opts.GetFloat(FloatOptionNames.TrackerCooldown);
            p.TrackerDuration              = opts.GetFloat(FloatOptionNames.TrackerDuration);
            p.TrackerDelay                 = opts.GetFloat(FloatOptionNames.TrackerDelay);
            p.NoisemakerAlertDuration      = opts.GetFloat(FloatOptionNames.NoisemakerAlertDuration);
            p.NoisemakerImpostorAlert      = opts.GetBool(BoolOptionNames.NoisemakerImpostorAlert);
            p.ViperDissolveTime            = opts.GetFloat(FloatOptionNames.ViperDissolveTime);
            p.DetectiveSuspectLimit        = opts.GetFloat(FloatOptionNames.DetectiveSuspectLimit);
            p.JudgeTaskRequirement         = opts.GetFloat(FloatOptionNames.JudgeTaskRequirementPercentage);
            p.InfluencerMessageCooldown    = opts.GetFloat(FloatOptionNames.SpiritGuideCooldownSeconds);

            var roles = new[]
            {
                RoleTypes.Scientist, RoleTypes.Engineer, RoleTypes.GuardianAngel,
                RoleTypes.Shapeshifter, RoleTypes.Noisemaker, RoleTypes.Phantom,
                RoleTypes.Tracker, RoleTypes.Detective, RoleTypes.Viper, RoleTypes.Judge
            };
            p.RoleRates.Clear();
            foreach (var r in roles)
            {
                p.RoleRates[r] = new RolePreset
                {
                    Count  = opts.RoleOptions.GetNumPerGame(r),
                    Chance = opts.RoleOptions.GetChancePerGame(r)
                };
            }
            return true;
        }

        public void ApplyPreset(HostPreset p)
        {
            if (p == null) return;
            var opts = GameOptionsManager.Instance.currentGameOptions;
            if (opts == null) return;

            opts.SetFloat(FloatOptionNames.PlayerSpeedMod,      p.PlayerSpeed);
            opts.SetFloat(FloatOptionNames.CrewLightMod,        p.CrewmateVision);
            opts.SetFloat(FloatOptionNames.ImpostorLightMod,    p.ImpostorVision);
            opts.SetFloat(FloatOptionNames.KillCooldown,        p.KillCooldown);
            opts.SetInt(Int32OptionNames.KillDistance,          p.KillDistance);
            opts.SetInt(Int32OptionNames.NumImpostors,          p.NumImpostors);
            opts.SetInt(Int32OptionNames.MaxPlayers,            p.MaxPlayers);
            opts.SetByte(ByteOptionNames.MapId,                 p.MapId);
            opts.SetBool(BoolOptionNames.VisualTasks,           p.VisualTasks);
            opts.SetBool(BoolOptionNames.ConfirmImpostor,       p.ConfirmImpostor);
            opts.SetBool(BoolOptionNames.AnonymousVotes,        p.AnonymousVotes);
            opts.SetInt(Int32OptionNames.NumEmergencyMeetings,  p.NumEmergencyMeetings);
            opts.SetInt(Int32OptionNames.EmergencyCooldown,     p.EmergencyCooldown);
            opts.SetInt(Int32OptionNames.DiscussionTime,        p.DiscussionTime);
            opts.SetInt(Int32OptionNames.VotingTime,            p.VotingTime);
            opts.SetInt(Int32OptionNames.TaskBarMode,           p.TaskBarMode);
            opts.SetInt(Int32OptionNames.NumCommonTasks,        p.NumCommonTasks);
            opts.SetInt(Int32OptionNames.NumLongTasks,          p.NumLongTasks);
            opts.SetInt(Int32OptionNames.NumShortTasks,         p.NumShortTasks);

            opts.SetFloat(FloatOptionNames.ShapeshifterCooldown,       p.ShapeshifterCooldown);
            opts.SetFloat(FloatOptionNames.ShapeshifterDuration,       p.ShapeshifterDuration);
            opts.SetBool(BoolOptionNames.ShapeshifterLeaveSkin,        p.ShapeshifterLeaveSkin);
            opts.SetFloat(FloatOptionNames.GuardianAngelCooldown,      p.GuardianAngelCooldown);
            opts.SetBool(BoolOptionNames.ImpostorsCanSeeProtect,       p.GuardianAngelProtectVisible);
            opts.SetFloat(FloatOptionNames.ProtectionDurationSeconds,  p.GuardianAngelProtectDuration);
            opts.SetFloat(FloatOptionNames.ScientistCooldown,          p.ScientistCooldown);
            opts.SetFloat(FloatOptionNames.ScientistBatteryCharge,     p.ScientistBatteryCharge);
            opts.SetFloat(FloatOptionNames.EngineerCooldown,           p.EngineerCooldown);
            opts.SetFloat(FloatOptionNames.EngineerInVentMaxTime,      p.EngineerInVentMaxTime);
            opts.SetFloat(FloatOptionNames.PhantomCooldown,            p.PhantomCooldown);
            opts.SetFloat(FloatOptionNames.PhantomDuration,            p.PhantomDuration);
            opts.SetFloat(FloatOptionNames.TrackerCooldown,            p.TrackerCooldown);
            opts.SetFloat(FloatOptionNames.TrackerDuration,            p.TrackerDuration);
            opts.SetFloat(FloatOptionNames.TrackerDelay,               p.TrackerDelay);
            opts.SetFloat(FloatOptionNames.NoisemakerAlertDuration,    p.NoisemakerAlertDuration);
            opts.SetBool(BoolOptionNames.NoisemakerImpostorAlert,      p.NoisemakerImpostorAlert);
            opts.SetFloat(FloatOptionNames.ViperDissolveTime,          p.ViperDissolveTime);
            opts.SetFloat(FloatOptionNames.DetectiveSuspectLimit,      p.DetectiveSuspectLimit);
            opts.SetFloat(FloatOptionNames.JudgeTaskRequirementPercentage, p.JudgeTaskRequirement);
            opts.SetFloat(FloatOptionNames.SpiritGuideCooldownSeconds,     p.InfluencerMessageCooldown);

            foreach (var kv in p.RoleRates)
                opts.RoleOptions.SetRoleRate(kv.Key, kv.Value.Count, kv.Value.Chance);

            SyncAllSettings();
        }

        public void RequestApplyPreset(int idx)
        {
            if (idx < 0 || idx >= Presets.Count) return;
            ApplyPreset(Presets[idx]);
        }
    }
}