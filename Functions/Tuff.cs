using System;
using System.Collections;
using System.Collections.Generic;
using HarmonyLib;
using Hazel;
using InnerNet;
using UnityEngine;
using BepInEx.Unity.IL2CPP.Utils.Collections;

namespace ZeroMenu.Functions
{
    public static class Tuff
    {
        public static bool Enabled = false;
        public static bool OnlyHost = false;
        public static bool AlwaysRed = false;
        public static int LastGameId = 0;

        public static bool ForceHostRunning = false;
        public static int ForceHostVoteCount = 3;
        public static float ForceHostVoteDelay = 0.05f;
        public static float ForceHostExitDelay = 0.3f;
        public static bool ForceHostStopOnJoinFailure = true;
        public static int ForceHostHostsKicked = 0;

        public static bool AutoRejoinOnVotekick = false;
        public static bool DestroyLobbyToggle = false;
        private static bool _destroyLobbyRunning = false;
        private static Coroutine _destroyLobbyCoroutine = null;

        private static string _forceHostCurrentCode = "";
        private static int _forceHostCurrentGameId = 0;
        private static Coroutine _forceHostCoroutine = null;
        private static bool _isRejoining = false;

        private static bool _hasSentNonHostVotes;
        private static bool _hasSentHostVote;
        private static float _nextHostVoteTime;
        private static int _nonHostVotesSent;
        private static int _hostVotesSent;

        public static HashSet<byte> AnnouncedDisconnects = new HashSet<byte>();
        public static bool IsShowingCustomMessage = false;

        private static void ResetVoteState()
        {
            _hasSentNonHostVotes = false;
            _hasSentHostVote = false;
            _nextHostVoteTime = 0f;
            _nonHostVotesSent = 0;
            _hostVotesSent = 0;
        }

        public static void StartDestroyLobby()
        {
            if (_destroyLobbyRunning || AmongUsClient.Instance == null || PlayerControl.LocalPlayer == null) return;
            _destroyLobbyRunning = true;
            if (ZeroMenu.Plugin.menuUI != null)
            {
                _destroyLobbyCoroutine = ZeroMenu.Plugin.menuUI.StartCoroutine(DestroyLobbyRoutine().WrapToIl2Cpp());
            }
        }

        public static void StopDestroyLobby()
        {
            if (!_destroyLobbyRunning) return;
            _destroyLobbyRunning = false;
            if (_destroyLobbyCoroutine != null && ZeroMenu.Plugin.menuUI != null)
            {
                try { ZeroMenu.Plugin.menuUI.StopCoroutine(_destroyLobbyCoroutine); } catch { }
                _destroyLobbyCoroutine = null;
            }
            DestroyLobbyToggle = false;
        }

        private static IEnumerator DestroyLobbyRoutine()
        {
            PlayerControl.LocalPlayer.CmdReportDeadBody(null);
            yield return new WaitForSeconds(0.5f);
            
            Rejoin();
            yield return new WaitForSeconds(5f);

            float timeout = 0f;
            while (PlayerControl.AllPlayerControls.Count > 3 && timeout < 30f && _destroyLobbyRunning)
            {
                yield return new WaitForSeconds(1f);
                timeout += 1f;
            }

            for (int i = 0; i < 3; i++)
            {
                if (!_destroyLobbyRunning) yield break;

                foreach (var p in PlayerControl.AllPlayerControls)
                {
                    if (p != null && p != PlayerControl.LocalPlayer && p.OwnerId != AmongUsClient.Instance.ClientId)
                    {
                        if (VoteBanSystem.Instance != null && !VoteBanSystem.Instance.HasMyVote(p.OwnerId))
                        {
                            VoteBanSystem.Instance.CmdAddVote(p.OwnerId);
                        }
                    }
                }
                
                yield return new WaitForSeconds(1.5f);
                Rejoin();
                yield return new WaitForSeconds(4f);
            }

            _destroyLobbyRunning = false;
            DestroyLobbyToggle = false;
            ZeroMenu.Plugin.notifications.Send("Tuff", "Destroy Lobby sequence finished!", 3f);
        }

        public static void StartForceHost()
        {
            if (ForceHostRunning) return;
            if (AmongUsClient.Instance == null) return;

            if (AmongUsClient.Instance.AmHost) return;

            int gameId = AmongUsClient.Instance.GameId;
            string roomCode = (gameId != 0) ? GameCode.IntToGameName(gameId) : "";

            if (string.IsNullOrEmpty(roomCode)) return;

            if (gameId == 0)
            {
                try { gameId = GameCode.GameNameToIntV2(roomCode); } catch { gameId = 0; }
            }

            if (gameId == 0) return;

            _forceHostCurrentCode = roomCode;
            _forceHostCurrentGameId = gameId;
            LastGameId = gameId;
            ForceHostHostsKicked = 0;
            ForceHostRunning = true;

            if (ZeroMenu.Plugin.menuUI != null)
            {
                _forceHostCoroutine = ZeroMenu.Plugin.menuUI.StartCoroutine(ForceHostLoop().WrapToIl2Cpp());
            }
        }

        public static void StopForceHost()
        {
            if (!ForceHostRunning) return;
            ForceHostRunning = false;
            if (_forceHostCoroutine != null && ZeroMenu.Plugin.menuUI != null)
            {
                try { ZeroMenu.Plugin.menuUI.StopCoroutine(_forceHostCoroutine); } catch { }
                _forceHostCoroutine = null;
            }
        }

        private static IEnumerator ForceHostLoop()
        {
            while (ForceHostRunning)
            {
                if (AmongUsClient.Instance == null)
                {
                    yield return new WaitForSeconds(ForceHostExitDelay);
                    continue;
                }

                if (AmongUsClient.Instance.AmHost)
                {
                    ForceHostRunning = false;
                    _forceHostCoroutine = null;
                    yield break;
                }

                if (!IsInGameOrLobby())
                {
                    int gameId = _forceHostCurrentGameId;
                    if (gameId == 0)
                    {
                        try { gameId = GameCode.GameNameToIntV2(_forceHostCurrentCode); } catch { gameId = 0; }
                    }

                    if (gameId == 0)
                    {
                        ForceHostRunning = false;
                        _forceHostCoroutine = null;
                        yield break;
                    }

                    bool joinFailed = false;
                    try
                    {
                        AmongUsClient.Instance.GameId = gameId;
                        var joinRoutine = AmongUsClient.Instance.CoJoinOnlineGameFromCode(gameId, true);
                        if (joinRoutine != null)
                            AmongUsClient.Instance.StartCoroutine(joinRoutine);
                        else
                            joinFailed = true;
                    }
                    catch
                    {
                        joinFailed = true;
                    }

                    if (joinFailed)
                    {
                        if (ForceHostStopOnJoinFailure)
                        {
                            ForceHostRunning = false;
                            _forceHostCoroutine = null;
                            yield break;
                        }
                        yield return new WaitForSeconds(ForceHostExitDelay);
                        continue;
                    }

                    float wait = 0f;
                    while (wait < 5f && (AmongUsClient.Instance == null || (int)AmongUsClient.Instance.GameState == 0))
                    {
                        yield return null;
                        wait += Time.deltaTime;
                    }

                    if (!ForceHostRunning)
                    {
                        _forceHostCoroutine = null;
                        yield break;
                    }

                    if (AmongUsClient.Instance != null && AmongUsClient.Instance.AmHost)
                    {
                        ForceHostRunning = false;
                        _forceHostCoroutine = null;
                        yield break;
                    }

                    yield return new WaitForSeconds(0.5f);
                }

                if (VoteBanSystem.Instance != null && AmongUsClient.Instance != null)
                {
                    int hostId = AmongUsClient.Instance.HostId;
                    if (hostId == AmongUsClient.Instance.ClientId || AmongUsClient.Instance.AmHost)
                    {
                        ForceHostRunning = false;
                        _forceHostCoroutine = null;
                        yield break;
                    }

                    for (int i = 0; i < ForceHostVoteCount; i++)
                    {
                        if (!ForceHostRunning)
                        {
                            _forceHostCoroutine = null;
                            yield break;
                        }
                        try { VoteBanSystem.Instance.CmdAddVote(hostId); } catch { }
                        yield return new WaitForSeconds(ForceHostVoteDelay);
                    }

                    ForceHostHostsKicked++;
                }

                yield return new WaitForSeconds(0.3f);
                if (!ForceHostRunning)
                {
                    _forceHostCoroutine = null;
                    yield break;
                }

                if (AmongUsClient.Instance != null && !AmongUsClient.Instance.AmHost && IsInGameOrLobby())
                {
                    try { AmongUsClient.Instance.ExitGame(DisconnectReasons.ExitGame); } catch { }
                }

                yield return new WaitForSeconds(ForceHostExitDelay);
            }

            ForceHostRunning = false;
            _forceHostCoroutine = null;
        }

        public static bool IsInGameOrLobby()
        {
            try
            {
                if (AmongUsClient.Instance == null) return false;
                if ((int)AmongUsClient.Instance.GameState > 0) return true;

                string sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
                return sceneName != "MainMenu" && sceneName != "MatchMaking";
            }
            catch { return false; }
        }

        private static IEnumerator AutoRejoinRoutine()
        {
            if (_isRejoining) yield break;
            _isRejoining = true;
            yield return new WaitForSeconds(0.2f);
            
            if (AutoRejoinOnVotekick && LastGameId != 0 && AmongUsClient.Instance != null)
            {
                Rejoin();
            }
            _isRejoining = false;
        }

        [HarmonyPatch(typeof(VoteBanSystem), nameof(VoteBanSystem.CmdAddVote))]
        public static class VoteBanSystem_CmdAddVote_Patch
        {
            public static void Postfix([HarmonyArgument(0)] int clientId)
            {
                if (!AutoRejoinOnVotekick || ForceHostRunning) return;
                if (ZeroMenu.Plugin.menuUI != null)
                {
                    ZeroMenu.Plugin.menuUI.StartCoroutine(AutoRejoinRoutine().WrapToIl2Cpp());
                }
            }
        }

        public static void Rejoin()
        {
            if (LastGameId != 0)
            {
                ZeroMenu.Plugin.notifications.Send("Tuff", "Rejoining last game...", 3f);
                AmongUsClient instance = AmongUsClient.Instance;
                instance.ExitGame((DisconnectReasons)0);
                instance.StartCoroutine(instance.CoJoinOnlineGameFromCode(LastGameId, false));
            }
        }

        public static void CopyLastGameCode()
        {
            if (LastGameId == 0)
            {
                ZeroMenu.Plugin.notifications.Send("Tuff", "No game code has been saved yet.", 3f);
                return;
            }
            try
            {
                string code = GameCode.IntToGameName(LastGameId);
                if (!string.IsNullOrEmpty(code))
                {
                    GUIUtility.systemCopyBuffer = code;
                    ZeroMenu.Plugin.notifications.Send("Tuff", $"Copied lobby code to clipboard: {code}", 3f);
                }
            }
            catch (Exception e)
            {
                ZeroMenu.Plugin.notifications.Send("Tuff", "Failed to copy code.", 3f);
                UnityEngine.Debug.LogError("[ZeroMenu] CopyLastGameCode error: " + e);
            }
        }

        public static void SetPing()
        {
            if (AmongUsClient.Instance == null || PlayerControl.LocalPlayer == null) return;
            try
            {
                MessageWriter mw = MessageWriter.Get((SendOption)1);
                mw.StartMessage((byte)5);
                mw.Write(AmongUsClient.Instance.GameId);
                mw.StartMessage((byte)2);
                mw.WritePacked(PlayerControl.LocalPlayer.NetId);
                mw.Write((byte)230);
                mw.EndMessage();
                mw.EndMessage();
                AmongUsClient.Instance.SendOrDisconnect(mw);
                mw.Recycle();
                ZeroMenu.Plugin.notifications.Send("Tuff", "Ping sent.", 2f);
            }
            catch { }
        }

        public static void DisconnectAllPlayers()
        {
            if (AmongUsClient.Instance == null || PlayerControl.LocalPlayer == null || ShipStatus.Instance == null) return;

            var targets = new System.Collections.Generic.List<PlayerControl>();
            foreach (var p in PlayerControl.AllPlayerControls)
            {
                if (p != null && p.Data != null && p != PlayerControl.LocalPlayer && !p.Data.Disconnected)
                {
                    targets.Add(p);
                }
            }

            ZeroMenu.Plugin.menuUI.StartCoroutine(BepInEx.Unity.IL2CPP.Utils.Collections.CollectionExtensions.WrapToIl2Cpp(DisconnectAllRoutine(targets)));
        }

        private static IEnumerator DisconnectAllRoutine(System.Collections.Generic.List<PlayerControl> targets)
        {
            foreach (var player in targets)
            {
                if (player == null || player.Data == null || player.Data.Disconnected) continue;
                try { Utils.KickPlayer(player, false, true); } catch { }
                yield return new WaitForSeconds(0.1f);
            }
        }

        [HarmonyPatch(typeof(AmongUsClient), "OnGameJoined")]
        public static class CaptureGameId
        {
            public static void Postfix(string gameIdString)
            {
                try
                {
                    if (!string.IsNullOrEmpty(gameIdString))
                    {
                        LastGameId = GameCode.GameNameToIntV2(gameIdString);
                    }
                }
                catch { }
                try
                {
                    if (AmongUsClient.Instance != null && AmongUsClient.Instance.GameId != 0)
                    {
                        LastGameId = AmongUsClient.Instance.GameId;
                    }
                }
                catch { }
            }
        }

        [HarmonyPatch(typeof(PlayerControl), "CmdCheckColor")]
        public static class CmdCheckColor_Patch
        {
            public static bool Prefix(PlayerControl __instance)
            {
                if (AlwaysRed && __instance != null && __instance.AmOwner)
                {
                    __instance.SetColor(0);
                    return false;
                }
                return true;
            }
        }

        [HarmonyPatch(typeof(GameData), nameof(GameData.HandleDisconnect), new Type[] { typeof(PlayerControl), typeof(DisconnectReasons) })]
        public static class GameData_HandleDisconnect_Patch
        {
            public static void Prefix(PlayerControl player, DisconnectReasons reason)
            {
                if (player == null || player.Data == null) return;
                if (player == PlayerControl.LocalPlayer) return;
                if (player.OwnerId == AmongUsClient.Instance.HostId) return;

                if (!AnnouncedDisconnects.Contains(player.PlayerId))
                {
                    AnnouncedDisconnects.Add(player.PlayerId);

                    IsShowingCustomMessage = true;
                    if (DestroyableSingleton<HudManager>.InstanceExists && DestroyableSingleton<HudManager>.Instance.Notifier != null)
                    {
                        DestroyableSingleton<HudManager>.Instance.Notifier.AddDisconnectMessage($"{player.Data.PlayerName} left the game.");
                    }
                }
            }
        }

        [HarmonyPatch(typeof(NotificationPopper), nameof(NotificationPopper.AddDisconnectMessage))]
        public static class NotificationPopper_AddDisconnectMessage_Patch
        {
            public static bool Prefix()
            {
                if (AmongUsClient.Instance != null && AmongUsClient.Instance.AmHost) return false;

                if (IsShowingCustomMessage)
                {
                    IsShowingCustomMessage = false;
                    return true;
                }

                return false;
            }
        }

        [HarmonyPatch(typeof(InnerNetClient), "Update")]
        public static class UpdatePatch
        {
            public static void Postfix()
            {
                if (!Utils.IsLobby && !Utils.IsInGame)
                {
                    AnnouncedDisconnects.Clear();
                }

                bool flag = !Enabled || !AmongUsClient.Instance || !Utils.IsPlayer || (!Utils.IsLobby && !Utils.IsInGame) || Utils.IsMeeting;
                if (flag)
                {
                    ResetVoteState();
                }
                else
                {
                    if (!_hasSentNonHostVotes)
                    {
                        foreach (PlayerControl player in PlayerControl.AllPlayerControls)
                        {
                            if (player == null || player == PlayerControl.LocalPlayer || player.OwnerId == AmongUsClient.Instance.ClientId) continue;
                            if (player.OwnerId == AmongUsClient.Instance.HostId) continue;
                            if (VoteBanSystem.Instance != null && !VoteBanSystem.Instance.HasMyVote(player.OwnerId))
                            {
                                VoteBanSystem.Instance.CmdAddVote(player.OwnerId);
                                DestroyableSingleton<HudManager>.Instance.Notifier.AddDisconnectMessage($"<#FFFFFF>[ZeroMenu]</color>: Votekick sent to <color=black>{player.Data.PlayerName}</color>!.");
                                _nonHostVotesSent++;
                            }
                        }
                        _hasSentNonHostVotes = true;
                        _nextHostVoteTime = Time.time + 0.25f;
                    }
                    else
                    {
                        if (_hasSentHostVote || Time.time < _nextHostVoteTime) return;
                        foreach (PlayerControl player2 in PlayerControl.AllPlayerControls)
                        {
                            if (player2 == null || player2 == PlayerControl.LocalPlayer || player2.OwnerId == AmongUsClient.Instance.ClientId) continue;
                            if (player2.OwnerId == AmongUsClient.Instance.HostId)
                            {
                                if (VoteBanSystem.Instance != null && !VoteBanSystem.Instance.HasMyVote(player2.OwnerId))
                                {
                                    VoteBanSystem.Instance.CmdAddVote(player2.OwnerId);
                                    DestroyableSingleton<HudManager>.Instance.Notifier.AddDisconnectMessage($"<#FFFFFF>[ZeroMenu]</color>: Votekick sent to <color=black>{player2.Data.PlayerName}</color>!.");
                                    _hostVotesSent++;
                                }
                                break;
                            }
                        }
                        _hasSentHostVote = true;
                        int total = _nonHostVotesSent + _hostVotesSent;
                        ZeroMenu.Plugin.notifications.Send("Votekick Everyone", $"Votekicked {total} players", 5f);
                    }
                }
            }
        }

        [HarmonyPatch(typeof(InnerNetClient), "Update")]
        public static class VotekickOnlyHostFeature
        {
            private static bool _hasVotedHost = false;

            public static void Postfix()
            {
                bool flag = !OnlyHost || !AmongUsClient.Instance || !Utils.IsPlayer || (!Utils.IsLobby && !Utils.IsInGame) || Utils.IsMeeting;
                if (flag)
                {
                    _hasVotedHost = false;
                }
                else
                {
                    if (_hasVotedHost) return;
                    foreach (PlayerControl player in PlayerControl.AllPlayerControls)
                    {
                        if (player == null || player == PlayerControl.LocalPlayer || player.OwnerId == AmongUsClient.Instance.ClientId) continue;
                        if (player.OwnerId != AmongUsClient.Instance.HostId) continue;
                        if (VoteBanSystem.Instance != null && !VoteBanSystem.Instance.HasMyVote(player.OwnerId))
                        {
                            VoteBanSystem.Instance.CmdAddVote(player.OwnerId);
                            DestroyableSingleton<HudManager>.Instance.Notifier.AddDisconnectMessage($"<#FFFFFF>[ZeroMenu]</color>: Votekick sent to <color=black>{player.Data.PlayerName}</color>!.");
                            ZeroMenu.Plugin.notifications.Send("Votekick Only Host", $"Votekick sent to host: {player.Data.PlayerName}", 5f);
                        }
                        break;
                    }
                    _hasVotedHost = true;
                }
            }
        }
    }
}
