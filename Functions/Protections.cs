using System;
using System.Collections.Generic;
using AmongUs.GameOptions;
using HarmonyLib;
using Hazel;
using InnerNet;
using UnityEngine;
using UObject = UnityEngine.Object;

namespace ZeroMenu.Functions
{
    public static class Protections
    {
        public static bool ForceDTLS = true;
        public static bool BlockServerTeleports = false;
        public static bool HardenedReadPackedUInt = true;
        public static bool BlockInvalidLadderOverload = true;
        public static bool BlockLargeGameMessages = true;
        public static bool BlockInvalidGameDataMessages = true;
        public static bool BlockUnauthorizedSystemUpdates = true;
        public static bool ProtectAgainstNonHostKickExploit = true;
        public static bool BlockZiplineForce = true;
        public static bool BlockVentTpForce = true;
        public static bool Votekicks = true;
        public static bool MemoryAllocationOverload = true;
        public static bool BypassShapeshiftRatelimits = true;
        public static bool AntiCrash = true;
        public static bool AntiExploits = false;
        public static bool ShowProtections = true;

        private static readonly BepInEx.Logging.ManualLogSource Log = BepInEx.Logging.Logger.CreateLogSource("ZeroMenu.Protections");

        [HarmonyPatch(typeof(InnerNetClient), nameof(InnerNetClient.SetEndpoint))]
        public static class Patch_ForceDTLS
        {
            public static void Prefix(ref bool dtls)
            {
                if (ForceDTLS) dtls = true;
            }
        }

        [HarmonyPatch(typeof(CustomNetworkTransform), nameof(CustomNetworkTransform.HandleRpc))]
        public static class Patch_BlockServerTeleports
        {
            public static bool Prefix(CustomNetworkTransform __instance, byte callId)
            {
                if (!BlockServerTeleports || callId != (byte)RpcCalls.SnapTo || __instance.myPlayer.PlayerId != PlayerControl.LocalPlayer.PlayerId) return true;
                Log.LogMessage("Received SnapTo RPC for our player, blocking...");
                return false;
            }
        }

        [HarmonyPatch(typeof(MessageReader), nameof(MessageReader.ReadPackedUInt32))]
        public static class Patch_HardenedReadPackedUInt
        {
            public static bool Prefix(MessageReader __instance, ref uint __result)
            {
                if (!HardenedReadPackedUInt) return true;

                bool readMore = true;
                int shift = 0;
                uint output = 0;

                while (readMore)
                {
                    if (__instance.BytesRemaining < 1) break;
                    byte b = __instance.ReadByte();
                    if (b >= 0x80)
                    {
                        readMore = true;
                        b ^= 0x80;
                    }
                    else
                    {
                        readMore = false;
                    }
                    output |= (uint)(b << shift);
                    shift += 7;
                }

                __result = output;
                return false;
            }
        }

        [HarmonyPatch(typeof(PlayerPhysics), nameof(PlayerPhysics.HandleRpc))]
        public static class Patch_OnPlayerPhysicsRpc
        {
            public static bool Prefix(byte callId, MessageReader reader)
            {
                int oldReadPosition = reader.Position;
                RpcCalls rpcId = (RpcCalls)callId;

                if (rpcId == RpcCalls.ClimbLadder)
                {
                    byte ladderId = reader.ReadByte();
                    if (BlockInvalidLadderOverload && (!ShipStatus.Instance || ladderId > ShipStatus.Instance.Ladders.Length - 1))
                    {
                        return false;
                    }
                }

                reader.Position = oldReadPosition;
                return true;
            }
        }

        [HarmonyPatch(typeof(AmongUsClient), nameof(InnerNetClient.CoStartGame))]
        public static class Patch_BypassShapeshiftRatelimits
        {
            public static void Postfix()
            {
                if (!BypassShapeshiftRatelimits || !AmongUsClient.Instance.AmHost) return;
                try { GameOptionsManager.Instance.currentGameOptions.SetFloat(FloatOptionNames.ShapeshifterCooldown, 0f); } catch { }
            }
        }

        [HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.HandleRpc))]
        public static class Patch_MemoryAllocationOverload
        {
            public static bool Prefix(byte callId, MessageReader reader)
            {
                if (!MemoryAllocationOverload || callId != (byte)RpcCalls.VotingComplete) return true;
                int oldReadPosition = reader.Position;
                int arrayLength = reader.ReadPackedInt32();
                if (arrayLength > 1024 || arrayLength > reader.BytesRemaining) return false;
                reader.Position = oldReadPosition;
                return true;
            }
        }

        [HarmonyPatch(typeof(VoteBanSystem), nameof(VoteBanSystem.AddVote))]
        public static class Patch_Votekicks
        {
            public static bool Prefix(int srcClient, int clientId)
            {
                Log.LogInfo($"[VotekickLogger] {srcClient} voted to kick out {clientId}");
                if (clientId != PlayerControl.LocalPlayer.OwnerId) return true;

                ClientData player = AmongUsClient.Instance.GetClient(srcClient);
                if (player != null)
                    ZeroMenu.Plugin.notifications.Send("Votekick Logger", $"{player.PlayerName} has voted to kick you out.", 3f);

                return !(Votekicks && AmongUsClient.Instance.AmHost);
            }
        }

        [HarmonyPatch(typeof(InnerNetClient), nameof(InnerNetClient.HandleMessage))]
        public static class Patch_OnHandleMessage
        {
            public static bool Prefix(MessageReader reader)
            {
                if (BlockLargeGameMessages && reader.Length > 1024 * 64) return false;
                if (BlockInvalidGameDataMessages && reader.Tag != 5 && reader.Tag != 6 && reader.Tag != 10 && reader.Tag != 12 && reader.Tag != 13) return true;
                return true;
            }
        }

        [HarmonyPatch(typeof(ShipStatus), nameof(ShipStatus.HandleRpc))]
        public static class Patch_OnShipStatusRPC
        {
            public static bool Prefix(byte callId, MessageReader reader)
            {
                int oldReadPosition = reader.Position;

                switch ((RpcCalls)callId)
                {
                    case RpcCalls.CloseDoorsOfType:
                        if (BlockUnauthorizedSystemUpdates && !AmongUsClient.Instance.AmHost) return false;
                        break;

                    case RpcCalls.UpdateSystem:
                        SystemTypes system = (SystemTypes)reader.ReadByte();
                        try
                        {
                            PlayerControl player = reader.ReadNetObject<PlayerControl>();
                            if (ProtectAgainstNonHostKickExploit && system == SystemTypes.Ventilation && !AmongUsClient.Instance.AmHost)
                            {
                                if (player?.Data != null)
                                    ZeroMenu.Plugin.notifications.Send("Protections Alert", $"{player.Data.PlayerName} attempted the VentilationSystem kick exploit on you!", 3f);
                                return false;
                            }
                        }
                        catch { }
                        if (BlockUnauthorizedSystemUpdates && !AmongUsClient.Instance.AmHost) return false;
                        break;
                }

                reader.Position = oldReadPosition;
                return true;
            }
        }

        public static class ZiplineProtect
        {
            private static float _okUntil;
            private static float _lastToast;
            public static bool IsForced(PlayerControl pc)
            {
                if (!BlockZiplineForce) return false;
                if (pc == null || pc != PlayerControl.LocalPlayer) return false;
                if (Time.unscaledTime < _okUntil) return false;
                if (Time.unscaledTime - _lastToast >= 2f)
                {
                    _lastToast = Time.unscaledTime;
                    ZeroMenu.Plugin.notifications.Send("Protection", "Forced zipline blocked", 2.5f);
                }
                return true;
            }
            public static void MarkOk() { _okUntil = Time.unscaledTime + 1.5f; }
        }

        [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.CmdCheckUseZipline))]
        public static class Patch_OnCmdZipline
        {
            public static void Prefix(PlayerControl __instance)
            {
                if (__instance == PlayerControl.LocalPlayer) ZiplineProtect.MarkOk();
            }
        }

        [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.RpcUseZipline))]
        public static class Patch_OnRpcZipline
        {
            public static void Prefix(PlayerControl __instance)
            {
                if (__instance == PlayerControl.LocalPlayer) ZiplineProtect.MarkOk();
            }
        }

        [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.HandleRpc))]
        public static class Patch_OnZiplineHandleRpc
        {
            public static bool Prefix(PlayerControl __instance, byte callId)
            {
                try
                {
                    if (!BlockZiplineForce || callId != (byte)RpcCalls.UseZipline) return true;
                    return !ZiplineProtect.IsForced(__instance);
                }
                catch { return true; }
            }
        }

        public static class VentTpProtect
        {
            private static float _lastToast;
            public static void Notify(string who)
            {
                if (!BlockVentTpForce) return;
                if (Time.unscaledTime - _lastToast < 2f) return;
                _lastToast = Time.unscaledTime;
                ZeroMenu.Plugin.notifications.Send("Protection", "Vent TP blocked: " + who, 2.5f);
            }
        }

        [HarmonyPatch(typeof(PlayerPhysics), nameof(PlayerPhysics.HandleRpc))]
        public static class Patch_OnVentBootProtect
        {
            public static bool Prefix(PlayerPhysics __instance, byte callId)
            {
                try
                {
                    if (__instance == null || callId != (byte)RpcCalls.BootFromVent) return true;
                    if (!BlockVentTpForce || AmongUsClient.Instance == null) return true;
                    VentTpProtect.Notify("vent boot");
                    return false;
                }
                catch { return true; }
            }
        }

        [HarmonyPatch(typeof(ShipStatus), nameof(ShipStatus.HandleRpc))]
        public static class Patch_OnVentUpdateSystemProtect
        {
            public static bool Prefix(byte callId, MessageReader reader)
            {
                try
                {
                    if (callId != (byte)RpcCalls.UpdateSystem || reader == null) return true;
                    if (!BlockVentTpForce || AmongUsClient.Instance == null) return true;
                    int pos = reader.Position;
                    PlayerControl actor = null;
                    bool block = false;
                    try
                    {
                        int sys = reader.ReadByte();
                        if (sys != (int)SystemTypes.Ventilation) return true;
                        actor = reader.ReadNetObject<PlayerControl>();
                        reader.ReadUInt16();
                        int op = reader.Position < reader.Length ? reader.ReadByte() : -1;
                        if (op == 1 || op == 2) block = true;
                    }
                    finally { try { reader.Position = pos; } catch { } }
                    if (!block) return true;
                    string name = (actor != null && actor.Data != null && actor.Data.PlayerName != null) ? actor.Data.PlayerName : "?";
                    VentTpProtect.Notify(name);
                    return false;
                }
                catch { return true; }
            }
        }

        [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.ReportDeadBody))]
        public static class Patch_AntiCrash
        {
            internal static bool gameFullyLoaded = false;

            public static bool Prefix()
            {
                if (!AntiCrash) return true;
                return gameFullyLoaded;
            }

            [HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.CoStartGame))]
            class OnGameStart { static void Postfix() { gameFullyLoaded = false; } }

            [HarmonyPatch(typeof(GameManager), nameof(GameManager.StartGame))]
            class OnGameLoad { static void Postfix() { gameFullyLoaded = true; } }
        }

        [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.FixedUpdate))]
        public static class Patch_ProtectionKeeper
        {
            public static void Postfix(PlayerControl __instance)
            {
                if (!ShowProtections) return;
                if (PlayerControl.LocalPlayer == null || AmongUsClient.Instance == null) return;
                if (AmongUsClient.Instance.GameState != InnerNetClient.GameStates.Started) return;

                foreach (PlayerControl pc in PlayerControl.AllPlayerControls)
                {
                    if (pc == null || pc.protectedByGuardianId < 0) continue;
                    try { KeepProtection(pc); } catch { }
                }
            }

            private static void KeepProtection(PlayerControl pc)
            {
                var options = GameOptionsManager.Instance.CurrentGameOptions;
                float full = options.GetFloat(FloatOptionNames.ProtectionDurationSeconds);
                
                foreach (RoleEffectAnimation anim in pc.currentRoleAnimations)
                {
                    if (anim != null && anim.effectType == RoleEffectAnimation.EffectType.ProtectLoop)
                        return;
                }

                options.SetFloat(FloatOptionNames.ProtectionDurationSeconds, 10f);
                try { pc.TurnOnProtection(true, pc.Data.DefaultOutfit.ColorId, pc.protectedByGuardianId); }
                finally { options.SetFloat(FloatOptionNames.ProtectionDurationSeconds, full); }
            }
        }
    }

    public class AntiExploitsPlayerTracker : MonoBehaviour
    {
        public PlayerControl player;
        public float joinTime = 0f;

        private Dictionary<byte, Queue<float>> rpcTimestamps = new Dictionary<byte, Queue<float>>();

        private static readonly Dictionary<byte, (int max, float window)> RpcLimits = new Dictionary<byte, (int, float)>
        {
            { 18, (100, 0.1f) }, { 49, (100, 0.1f) }, { 44, (50, 0.1f) }, { 50, (100, 0.1f) },
            { 21, (20, 0.1f) }, { 8, (30, 0.1f) }, { 6, (30, 0.1f) }, { 39, (30, 0.1f) },
            { 40, (30, 0.1f) }, { 42, (30, 0.1f) }, { 41, (30, 0.1f) }, { 33, (1, 1f) },
            { 54, (5, 1f) }, { 7, (100, 0.1f) }
        };

        private static readonly HashSet<byte> ImmediatelyRPCs = new HashSet<byte>
            { 51, 54, 5, 7, 14, 47, 48, 12, 52, 53, 45, 46, 62, 64, 55, 56, 2, 63, 65, 21 };

        private static readonly HashSet<byte> SusRPCs = new HashSet<byte> { 101, 164, 154, 85 };

        private static readonly HashSet<byte> ExcludedNumMsgCallIds = new HashSet<byte> { 41, 39, 40, 42, 43, 38 };

        private static readonly HashSet<byte> ExcludedForLobby = new HashSet<byte>
            { 2, 5, 7, 9, 10, 13, 17, 18, 21, 33, 36, 37, 38, 39, 40, 41, 42, 43, 49, 50, 60, 61, 80, 78, 70, 210, 81, 176 };

        private static readonly HashSet<byte> NormalCustomCallIds = new HashSet<byte> { 80, 78, 70, 210, 81, 176 };

        public void Awake()
        {
            player = GetComponent<PlayerControl>();
            joinTime = 0f;
        }

        public void FixedUpdate()
        {
            joinTime += Time.fixedDeltaTime;
        }

        public bool RpcCheck(byte callId, int numData)
        {
            if (joinTime < 3f) return true;
            if (NormalCustomCallIds.Contains(callId)) return true;

            if (SusRPCs.Contains(callId))
            {
                AntiExploitsNotifier.NotifyExploit(player, callId, "Suspicious RPC (instantly flagged)");
                return false;
            }

            if (!CheckSpam(callId)) return false;

            if (LobbyBehaviour.Instance != null && !ExcludedForLobby.Contains(callId))
            {
                AntiExploitsNotifier.NotifyExploit(player, callId, "Blocked RPC in lobby");
                return false;
            }

            if (!ExcludedNumMsgCallIds.Contains(callId))
            {
                if (ImmediatelyRPCs.Contains(callId) && numData > 1)
                {
                    AntiExploitsNotifier.NotifyExploit(player, callId, "Big Data RPC (ImmediateRPC flood)");
                    return false;
                }
                if (numData > 10)
                {
                    AntiExploitsNotifier.NotifyExploit(player, callId, "Big Data RPC (message flood)");
                    return false;
                }
            }

            return true;
        }

        public bool CheckSpam(byte callId)
        {
            if (!rpcTimestamps.ContainsKey(callId))
                rpcTimestamps[callId] = new Queue<float>();

            Queue<float> queue = rpcTimestamps[callId];
            float fixedTime = Time.fixedTime;
            int max = 10;
            float window = 1f;

            if (RpcLimits.TryGetValue(callId, out (int, float) value))
                (max, window) = value;

            while (queue.Count > 0 && queue.Peek() < fixedTime - window)
                queue.Dequeue();

            queue.Enqueue(fixedTime);

            if (queue.Count > max)
            {
                AntiExploitsNotifier.NotifyExploit(player, callId, "RPC Spam (" + queue.Count + "/" + max + ")");
                return false;
            }

            return true;
        }
    }

    public static class AntiExploitsNotifier
    {
        private static readonly BepInEx.Logging.ManualLogSource Log = BepInEx.Logging.Logger.CreateLogSource("ZeroMenu.AntiExploits");

        public static void NotifyExploit(PlayerControl player, byte callId, string reason)
        {
            string name = player?.Data?.PlayerName ?? "Unknown";
            Log.LogWarning($"[ANTI-EXPLOITS] {name} (RPC {callId}): {reason}");
            ZeroMenu.Plugin.notifications.Send("Anti-Exploits", $"{name} -> {reason}", 3f);
        }

        public static void NotifyHazel(string reason)
        {
            Log.LogWarning($"[ANTI-EXPLOITS] {reason}");
            ZeroMenu.Plugin.notifications.Send("Anti-Exploits", reason, 3f);
        }
    }

    [HarmonyPatch(typeof(InnerNetClient), "HandleGameData")]
    public static class AntiExploits_HandleGameDataPatch
    {
        private static int _numData;

        public static bool Prefix(InnerNetClient __instance, [HarmonyArgument(0)] MessageReader parentReader)
        {
            if (!Protections.AntiExploits) return true;
            if (!__instance.InOnlineScene) return true;

            int position = parentReader.Position;
            _numData = 0;

            try
            {
                InnerNetObject val2 = default(InnerNetObject);
                while (parentReader.Position < parentReader.Length && parentReader.BytesRemaining >= 3)
                {
                    _numData++;
                    if (_numData > 100)
                    {
                        AntiExploitsNotifier.NotifyHazel("Blocked data flood (" + _numData + " sub-messages)");
                        parentReader.Recycle();
                        return false;
                    }

                    MessageReader val = parentReader.ReadMessage();

                    if (val.Tag > 8 || val.Tag == 3)
                    {
                        AntiExploitsNotifier.NotifyHazel("Blocked invalid GameData tag: " + val.Tag + " (overload)");
                        parentReader.Recycle();
                        return false;
                    }

                    if (val.Tag == 1)
                    {
                        uint num = val.ReadPackedUInt32();
                        if (!__instance.allObjects.AllObjectsFast.ContainsKey(num) && !__instance.DestroyedObjects.Contains(num))
                        {
                            AntiExploitsNotifier.NotifyHazel("Blocked invalid NetId in DATA: " + num);
                            parentReader.Recycle();
                            return false;
                        }
                    }

                    if (val.Tag != 2) continue;

                    uint num2 = val.ReadPackedUInt32();
                    byte callId = val.ReadByte();

                    if (__instance.allObjects.AllObjectsFast.TryGetValue(num2, out val2))
                    {
                        PlayerControl val3 = val2.TryCast<PlayerControl>();
                        if (val3 != null && !val3.AmOwner)
                        {
                            AntiExploitsPlayerTracker tracker = val3.gameObject.GetComponent<AntiExploitsPlayerTracker>();
                            if (tracker == null)
                                tracker = val3.gameObject.AddComponent<AntiExploitsPlayerTracker>();

                            if (!tracker.RpcCheck(callId, _numData))
                            {
                                parentReader.Position = position;
                                return false;
                            }
                        }
                    }
                    else if (!__instance.DestroyedObjects.Contains(num2))
                    {
                        AntiExploitsNotifier.NotifyHazel("Blocked invalid NetId in RPC: " + num2);
                        parentReader.Recycle();
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                BepInEx.Logging.Logger.CreateLogSource("ZeroMenu.AntiExploits").LogDebug("[ANTI-EXPLOITS] HandleGameData check error: " + ex.Message);
            }
            finally
            {
                parentReader.Position = position;
            }

            return true;
        }
    }

    [HarmonyPatch(typeof(PlayerControl), "HandleRpc")]
    public static class AntiExploits_PlayerControlRpcPatch
    {
        public static bool Prefix(PlayerControl __instance, [HarmonyArgument(0)] byte callId, [HarmonyArgument(1)] MessageReader reader)
        {
            if (!Protections.AntiExploits) return true;
            if (__instance == null || __instance.AmOwner) return true;

            try
            {
                AntiExploitsPlayerTracker tracker = __instance.gameObject.GetComponent<AntiExploitsPlayerTracker>();
                if (tracker == null)
                    tracker = __instance.gameObject.AddComponent<AntiExploitsPlayerTracker>();

                return tracker.RpcCheck(callId, 1);
            }
            catch (Exception ex)
            {
                BepInEx.Logging.Logger.CreateLogSource("ZeroMenu.AntiExploits").LogDebug("[ANTI-EXPLOITS] PlayerControl RPC check error: " + ex.Message);
                return true;
            }
        }
    }

    [HarmonyPatch(typeof(InnerNetClient), "OnMessageReceived")]
    public static class AntiExploits_PacketValidatorPatch
    {
        public static bool Prefix(DataReceivedEventArgs e)
        {
            if (!Protections.AntiExploits) return true;

            MessageReader message = e.Message;
            int position = message.Position;

            try
            {
                if (message.BytesRemaining >= 3 && !ValidatePacketStructure(message))
                {
                    AntiExploitsNotifier.NotifyHazel("Blocked malformed Hazel packet (Mix/SDF overload)");
                    return false;
                }
            }
            catch (Exception ex)
            {
                BepInEx.Logging.Logger.CreateLogSource("ZeroMenu.AntiExploits").LogDebug("[ANTI-EXPLOITS] Packet check error: " + ex.Message);
            }
            finally
            {
                message.Position = position;
            }

            return true;
        }

        private static bool ValidatePacketStructure(MessageReader msg)
        {
            int position = msg.Position;
            try
            {
                while (msg.BytesRemaining >= 3)
                {
                    ushort length = msg.ReadUInt16();
                    msg.ReadByte();
                    if (msg.BytesRemaining < length) return false;
                    msg.Position += length;
                }
                return true;
            }
            catch
            {
                return false;
            }
            finally
            {
                msg.Position = position;
            }
        }
    }
}