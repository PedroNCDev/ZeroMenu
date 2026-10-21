using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using AmongUs.GameOptions;
using BepInEx.Unity.IL2CPP.Utils.Collections;
using HarmonyLib;
using Hazel;
using InnerNet;
using UnityEngine;
using ZeroMenu.Functions;
using ZeroMenu.Utilities;

namespace ZeroMenu.Functions
{
    public static class Troll
    {
        public static bool AutoReportBodies = false;
        public static bool CrashLobby = false;
        public static bool DisableVents = false;
        public static bool BlockSabotages = false;
        
        public static bool AutoExposeEnabled = false;
        public static bool ExposeOnMurder = true;
        public static bool ExposeOnShapeshift = true;
        public static bool ExposeOnPhantom = true;

        public static bool DoorTrollerEnabled = false;
        public static float DoorTrollerDelay = 0.5f;
        private static float _doorTrollerTimer = 0f;
        private static bool _doorsClosedByTroller = false;

        public static bool DisableGameEndNonHost = false;

        public static void SetDisableGameEndNonHost(bool enabled)
        {
            DisableGameEndNonHost = enabled;

            if (enabled && AmongUsClient.Instance != null && !AmongUsClient.Instance.AmHost
                && GameManager.Instance != null && ShipStatus.Instance != null)
            {
                try
                {
                    GameManager.Instance.Despawn();
                    ZeroMenu.Plugin.notifications.Send("No Game End", "GameManager despawned. Game will not end.", 5f);
                }
                catch
                {
                    ZeroMenu.Plugin.notifications.Send("No Game End", "Failed to despawn GameManager.", 5f);
                }
            }
        }

        [HarmonyPatch(typeof(GameManager), nameof(GameManager.StartGame))]
        public static class NoGameEndOnLoadPatch
        {
            public static void Postfix()
            {
                if (!DisableGameEndNonHost) return;
                if (AmongUsClient.Instance == null || AmongUsClient.Instance.AmHost) return;

                try
                {
                    GameManager.Instance.Despawn();
                }
                catch { }
            }
        }

        [HarmonyPatch(typeof(InnerNetObjectCollection), nameof(InnerNetObjectCollection.TryAddNetObject))]
        public static class NoGameEndOnSpawnPatch
        {
            public static void Postfix(InnerNetObject obj)
            {
                if (!DisableGameEndNonHost) return;
                if (AmongUsClient.Instance == null || AmongUsClient.Instance.AmHost) return;

                try
                {
                    if (obj != null && obj.TryCast<GameManager>() != null)
                        obj.Despawn();
                }
                catch { }
            }
        }

        public static void TickDoorTroller()
        {
            if (!DoorTrollerEnabled || ShipStatus.Instance == null || AmongUsClient.Instance == null) return;

            _doorTrollerTimer += Time.deltaTime;
            if (_doorTrollerTimer >= DoorTrollerDelay)
            {
                _doorTrollerTimer = 0f;
                _doorsClosedByTroller = !_doorsClosedByTroller;

                if (_doorsClosedByTroller)
                {
                    foreach (var door in ShipStatus.Instance.AllDoors)
                        ShipStatus.Instance.RpcCloseDoorsOfType(door.Room);
                }
                else
                {
                    foreach (var door in ShipStatus.Instance.AllDoors)
                        ShipStatus.Instance.RpcUpdateSystem(SystemTypes.Doors, (byte)(door.Id | 64));
                }
            }
        }

        public static void DisconnectAllNonHost()
        {
            if (AmongUsClient.Instance == null || PlayerControl.LocalPlayer == null || ShipStatus.Instance == null)
            {
                ZeroMenu.Plugin.notifications.Send("Disconnect All", "You must be inside a live match.", 5f);
                return;
            }

            var targets = new List<PlayerControl>();
            foreach (var p in PlayerControl.AllPlayerControls)
            {
                if (p != null && p.Data != null && p != PlayerControl.LocalPlayer && !p.Data.Disconnected)
                {
                    if (p.OwnerId != AmongUsClient.Instance.HostId)
                    {
                        targets.Add(p);
                    }
                }
            }

            if (targets.Count == 0)
            {
                ZeroMenu.Plugin.notifications.Send("Disconnect All", "No clients found.", 5f);
                return;
            }

            AmongUsClient.Instance.StartCoroutine(DisconnectAllRoutine(targets).WrapToIl2Cpp());
        }

        private static IEnumerator DisconnectAllRoutine(List<PlayerControl> targets)
        {
            int disconnected = 0;
            foreach (var player in targets)
            {
                if (player == null || player.Data == null || player.Data.Disconnected) continue;

                try
                {
                    Utils.KickPlayer(player, false, true);
                    disconnected++;
                }
                catch { }
                
                yield return new WaitForSeconds(0.1f);
            }
            ZeroMenu.Plugin.notifications.Send("Disconnect All", "$Disconnect all player with rpc mlfor.", 5f);
        }

        public static void TriggerAllSpores()
        {
            if (Utils.GetCurrentMapID() != 5)
            {
                ZeroMenu.Plugin.notifications.Send("Trigger Spores", "This option only works on the Fungle map.", 5f);
                return;
            }
            var ship = ShipStatus.Instance.TryCast<FungleShipStatus>();
            if (ship == null) return;
            
            foreach (var mushroom in ship.sporeMushrooms.Values)
            {
                PlayerControl.LocalPlayer.CmdCheckSporeTrigger(mushroom);
            }
            ZeroMenu.Plugin.notifications.Send("Trigger Spores", "All spores have been triggered.", 5f);
        }

        public static void CopyRandomPlayer()
        {
            var players = PlayerControl.AllPlayerControls.ToArray();
            if (players.Length == 0) return;
            var random = players[UnityEngine.Random.Range(0, players.Length)];
            Players.CopyAvatar(random);
        }

        public static void DisableKilling()
        {
            if (AmongUsClient.Instance == null || ShipStatus.Instance == null) return;
            int count = 0;
            foreach (var p in PlayerControl.AllPlayerControls)
            {
                if (p == null || p.Data == null || p.Data.IsDead || p == PlayerControl.LocalPlayer) continue;
                try
                {
                    PlayerImmortality.Toggle(p);
                    count++;
                }
                catch { }
            }
            ZeroMenu.Plugin.notifications.Send("Disable Killing", $"The impostors no killing today", 5f);
        }

        public static void DepleteHnSTimer()
        {
            if (AmongUsClient.Instance == null || ShipStatus.Instance == null) return;
            if (GameOptionsManager.Instance.currentGameOptions.GameMode != GameModes.HideNSeek)
            {
                ZeroMenu.Plugin.notifications.Send("Deplete HnS Timer", "This feature can only be used in Hide and Seek.", 5f);
                return;
            }
            if (AmongUsClient.Instance.AmHost)
            {
                var gameFlow = GameManager.Instance.LogicFlow.Cast<LogicGameFlowHnS>();
                gameFlow.AdjustEscapeTimer(gameFlow.currentHideTime, true);
                return;
            }
            AmongUsClient.Instance.StartCoroutine(DepleteHnSTimerCoroutine().WrapToIl2Cpp());
        }

        private static IEnumerator DepleteHnSTimerCoroutine()
        {
            var gameFlow = GameManager.Instance.LogicFlow.Cast<LogicGameFlowHnS>();
            if (gameFlow.IsFinalCountdown) yield break;
            
            NormalPlayerTask applicableTask = null;
            foreach (PlayerTask task in PlayerControl.LocalPlayer.myTasks)
            {
                var normalTask = task.TryCast<NormalPlayerTask>();
                if (normalTask != null) { applicableTask = normalTask; break; }
            }
            if (applicableTask == null) yield break;

            var logicOptions = GameManager.Instance.LogicOptions.Cast<LogicOptionsHnS>();
            float deduction = logicOptions.GetShortTaskTimeValue();
            
            int required = (int)Math.Ceiling(gameFlow.currentHideTime / deduction);
            
            for (int i = 0; i <= required; i++)
            {
                try
                {
                    PlayerControl.LocalPlayer.RpcCompleteTask(applicableTask.Id);
                }
                catch { }
                
                if (i % 6 == 0)
                    yield return new WaitForSeconds(0.05f);
            }
        }

        public static void TeleportAllToVent(int ventId)
        {
            foreach (var player in PlayerControl.AllPlayerControls)
            {
                if (player == null || player.Data == null || player == PlayerControl.LocalPlayer) continue;
                Players.TeleportToVent(player, ventId);
            }
        }

        public static void TeleportAllToRandomVent()
        {
            foreach (var player in PlayerControl.AllPlayerControls)
            {
                if (player == null || player.Data == null || player == PlayerControl.LocalPlayer) continue;
                int randomVent = UnityEngine.Random.Range(0, ShipStatus.Instance.AllVents.Count);
                Players.TeleportToVent(player, randomVent);
            }
        }

        public static void ExposeImpostors(PlayerControl murderer)
        {
            if (ShipStatus.Instance == null) return;
            Vent selectedVent = FindClosestVent(murderer, 1.0f, 5.0f);
            if (selectedVent == null) return;

            foreach (var player in PlayerControl.AllPlayerControls)
            {
                if (player == murderer || player.Data == null || player.Data.IsDead || player.Data.Role.IsImpostor || player == PlayerControl.LocalPlayer) continue;
                Players.TeleportToVent(player, selectedVent.Id);
            }
        }

        private static Vent FindClosestVent(PlayerControl player, float minDist, float maxDist)
        {
            foreach (var vent in ShipStatus.Instance.AllVents)
            {
                float dist = Vector2.Distance(player.transform.position, vent.transform.position);
                if (dist < minDist || dist > maxDist) continue;
                return vent;
            }
            return null;
        }

        [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.MurderPlayer))]
        public static class AutoReportAndExposePatch
        {
            public static void Postfix(PlayerControl __instance, PlayerControl target, MurderResultFlags resultFlags)
            {
                if (!resultFlags.HasFlag(MurderResultFlags.Succeeded)) return;

                if (AutoReportBodies)
                {
                    if (AmongUsClient.Instance.AmHost)
                    {
                        MeetingRoomManager.Instance.AssignSelf(PlayerControl.LocalPlayer, target.Data);
                        PlayerControl.LocalPlayer.RpcStartMeeting(target.Data);
                        DestroyableSingleton<HudManager>.Instance.OpenMeetingRoom(PlayerControl.LocalPlayer);
                    }
                    else if (!PlayerControl.LocalPlayer.Data.IsDead)
                    {
                        PlayerControl.LocalPlayer.CmdReportDeadBody(target.Data);
                    }
                }

                if (AutoExposeEnabled && ExposeOnMurder && !__instance.shapeshiftTargetPlayerId.Equals(-1))
                {
                    ExposeImpostors(__instance);
                }
            }
        }

        [HarmonyPatch(typeof(PlayerPhysics), nameof(PlayerPhysics.HandleRpc))]
        public static class DisableVentsPatch
        {
            public static void Prefix(PlayerPhysics __instance, [HarmonyArgument(0)] byte callId, [HarmonyArgument(1)] MessageReader reader)
            {
                if (!DisableVents || callId != (byte)RpcCalls.EnterVent) return;
                if (ShipStatus.Instance == null) return;
                
                try
                {
                    int pos = reader.Position;
                    reader.Position = pos + 1;
                    byte ventId = reader.ReadByte();
                    reader.Position = pos;
                    __instance.myPlayer.MyPhysics.RpcBootFromVent(ventId);
                }
                catch { }
            }
        }

        [HarmonyPatch(typeof(SabotageSystemType), nameof(SabotageSystemType.UpdateSystem))]
        public static class BlockSabotagesPatch
        {
            public static bool Prefix() => !BlockSabotages;
        }

        [HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.CoStartGame))]
        public static class CrashLobbyPatch
        {
            public static void Postfix()
            {
                if (!CrashLobby) return;
                PlayerControl.LocalPlayer.CmdReportDeadBody(null);
                ZeroMenu.Plugin.notifications.Send("Lobby Crasher", "The lobby has been crashed.", 5f);
                CrashLobby = false;
            }
        }

        [HarmonyPatch(typeof(InnerNetClient), nameof(InnerNetClient.Update))]
        public static class TrollUpdatePatch
        {
            public static void Postfix() => TickDoorTroller();
        }
    }
}
