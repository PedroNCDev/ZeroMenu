using System;
using System.Collections;
using System.Collections.Generic;
using HarmonyLib;
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

        public static void SetPing()
        {
            if (AmongUsClient.Instance == null || PlayerControl.LocalPlayer == null) return;
            try
            {
                Hazel.MessageWriter mw = Hazel.MessageWriter.Get((Hazel.SendOption)1);
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

                try
                {
                    Utils.KickPlayer(player, false, true);
                }
                catch { }

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
