using System;
using System.Linq;
using HarmonyLib;
using Hazel;
using Il2CppInterop.Runtime;
using InnerNet;
using UnityEngine;
using AmongUs.GameOptions;

namespace ZeroMenu.Utilities
{
    public static class EventCoordinator
    {
        private static readonly BepInEx.Logging.ManualLogSource Log = BepInEx.Logging.Logger.CreateLogSource("ZeroMenu.Events");

        public static event Action OnGameStart;
        public static event Action OnGameLoad;
        public static event Action OnDisconnect;
        public static event Action<MeetingHud.VoterState[], NetworkedPlayerInfo, bool, bool> OnVotingComplete;
        public static event Action OnMeetingEnd;
        public static event Action<Minigame> OnOpenMinigame;
        public static event Action<Ladder> OnUseLadder;
        public static event Action<ZiplineConsole> OnUseZipline;

        public static event Action<PlayerControl, ClientData> OnPlayerJoin;
        public static event Action<ClientData, DisconnectReasons> OnPlayerDisconnect;
        public static event Action<PlayerControl, string> OnPlayerChat;
        public static event Action<PlayerControl, byte> OnPlayerEnterVent;
        public static event Action<PlayerControl, byte> OnPlayerExitVent;
        public static event Action<PlayerControl, byte, byte> OnPlayerMoveVent;
        public static event Action<PlayerControl> OnPlayerEnterCameras;
        public static event Action<PlayerControl> OnPlayerExitCameras;
        public static event Action<PlayerControl, PlayerControl, MurderResultFlags> OnPlayerMurder;
        public static event Action<PlayerControl, PlayerControl, bool> OnPlayerShapeshift;
        public static event Action<PlayerControl> OnPlayerPhantom;
        public static event Action<ClientData, ClientData> OnPlayerVotekick;
        public static event Action<NetworkedPlayerInfo, NetworkedPlayerInfo> OnPlayerCastVote;

        public static event Action OnHudOverrideSabotage;
        public static event Action OnHudOverrideRepair;
        public static event Action<InnerNetObject> OnNetObjectSpawn;

        public static readonly System.Collections.Generic.Queue<InnerNetObject> LobbyList = new System.Collections.Generic.Queue<InnerNetObject>();
        public static readonly System.Collections.Generic.Queue<InnerNetObject> ShipList = new System.Collections.Generic.Queue<InnerNetObject>();

        private static readonly System.Collections.Generic.List<Il2CppSystem.Type> ShipNetObjects = new System.Collections.Generic.List<Il2CppSystem.Type>
        {
            Il2CppType.From(typeof(ShipStatus)),
            Il2CppType.From(typeof(SkeldShipStatus)),
            Il2CppType.From(typeof(MiraShipStatus)),
            Il2CppType.From(typeof(PolusShipStatus)),
            Il2CppType.From(typeof(AirshipStatus)),
            Il2CppType.From(typeof(FungleShipStatus))
        };

        private static string GetPlatformName(Platforms platform)
        {
            switch (platform)
            {
                case Platforms.StandaloneEpicPC: return "Epic Games";
                case Platforms.StandaloneSteamPC: return "Steam";
                case Platforms.StandaloneMac: return "Mac";
                case Platforms.StandaloneWin10: return "Microsoft Store";
                case Platforms.StandaloneItch: return "Itch.io";
                case Platforms.IPhone: return "iPhone / iPad";
                case Platforms.Android: return "Android";
                case Platforms.Switch: return "Nintendo Switch";
                case Platforms.Xbox: return "Xbox";
                case Platforms.Playstation: return "PlayStation";
                default: return "Unknown";
            }
        }

        [HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.CoStartGame))]
        class GameStart
        {
            static void Prefix() => PublishEvent(OnGameStart);
        }

        [HarmonyPatch(typeof(GameManager), nameof(GameManager.StartGame))]
        class GameLoad
        {
            static void Prefix() => PublishEvent(OnGameLoad);
        }

        [HarmonyPatch(typeof(GameData), nameof(GameData.OnDisconnected))]
        class Disconnect
        {
            static void Prefix()
            {
                Log.LogInfo("[EventCoordinator] Local player disconnected.");
                LobbyList.Clear();
                ShipList.Clear();
                PublishEvent(OnDisconnect);
            }
        }

        [HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.VotingComplete))]
        class VotingComplete
        {
            static void Prefix(MeetingHud.VoterState[] states, NetworkedPlayerInfo exiled, bool tie, bool wasOverruled)
            {
                PublishEvent(OnVotingComplete, states, exiled, tie, wasOverruled);
            }
        }

        [HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.Close))]
        class MeetingEnd
        {
            static void Prefix() => PublishEvent(OnMeetingEnd);
        }

        [HarmonyPatch(typeof(Minigame), nameof(Minigame.Begin))]
        class MinigameOpen
        {
            static void Prefix(Minigame __instance)
            {
                Log.LogInfo($"Minigame opened: {__instance.GetIl2CppType().Name}");
                PublishEvent(OnOpenMinigame, __instance);
            }
        }

        [HarmonyPatch(typeof(Ladder), nameof(Ladder.SetDestinationCooldown))]
        class LadderUsed
        {
            static void Postfix(Ladder __instance)
            {
                Log.LogInfo($"Ladder {__instance.Id} used");
                PublishEvent(OnUseLadder, __instance.Destination);
            }
        }

        [HarmonyPatch(typeof(ZiplineBehaviour), nameof(ZiplineBehaviour.ResetTarget))]
        class ZiplineUsed
        {
            static void Postfix(ZiplineBehaviour __instance)
            {
                ZiplineConsole console = __instance.lastUsedConsole;
                if (console == null) return;
                Log.LogInfo($"Zipline used at {(console.atTop ? "top" : "bottom")}");
                PublishEvent(OnUseZipline, console);
            }
        }

        [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.Start))]
        class PlayerJoin
        {
            static void Postfix(PlayerControl __instance)
            {
                if (__instance == PlayerControl.LocalPlayer || AmongUsClient.Instance.NetworkMode == NetworkModes.FreePlay) return;

                ClientData clientData = AmongUsClient.Instance.GetClientFromCharacter(__instance);
                if (clientData != null)
                {
                    PlatformSpecificData platformData = clientData.PlatformData;
                    Log.LogInfo($"[PlayerLogger] {clientData.PlayerName} ({__instance.NetId}) joined on {GetPlatformName(platformData.Platform)}. FC: {clientData.FriendCode}, PUID: {clientData.ProductUserId}");
                }
                else
                {
                    Log.LogInfo($"[PlayerLogger] {__instance.name} ({__instance.NetId}) joined.");
                }

                PublishEvent(OnPlayerJoin, __instance, clientData);
            }
        }

        [HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.OnPlayerLeft))]
        class PlayerDisconnect
        {
            static void Prefix(ClientData data, DisconnectReasons reason)
            {
                if (data.Character == null || data.Character.Data == null) return;
                Log.LogInfo($"[Disconnect Logger] {data.Character.Data.PlayerName} disconnected. Reason: {reason}");
                PublishEvent(OnPlayerDisconnect, data, reason);
            }
        }

        [HarmonyPatch(typeof(ChatController), nameof(ChatController.AddChat))]
        class PlayerChat
        {
            static void Prefix(PlayerControl sourcePlayer, string chatText)
            {
                PublishEvent(OnPlayerChat, sourcePlayer, chatText);
            }
        }

        [HarmonyPatch(typeof(VentilationSystem), nameof(VentilationSystem.Deserialize))]
        class PlayerVentNonHost
        {
            static void Prefix(VentilationSystem __instance, MessageReader reader)
            {
                int oldReadPosition = reader.Position;

                int ventCleans = reader.ReadPackedInt32();
                if (ventCleans > PlayerControl.AllPlayerControls.Count || ventCleans > reader.BytesRemaining) goto end;
                reader.Position += 2 * ventCleans;

                int ventedPlayers = reader.ReadPackedInt32();
                if (ventedPlayers > PlayerControl.AllPlayerControls.Count || ventedPlayers > reader.BytesRemaining) goto end;

                System.Collections.Generic.Dictionary<byte, byte> ventData = new System.Collections.Generic.Dictionary<byte, byte>();
                for (int i = 0; i < ventedPlayers; i++)
                {
                    byte playerId = reader.ReadByte();
                    byte ventId = reader.ReadByte();
                    ventData[playerId] = ventId;
                }

                foreach (PlayerControl player in PlayerControl.AllPlayerControls)
                {
                    byte playerId = player.PlayerId;
                    bool inOld = __instance.PlayersInsideVents.TryGetValue(playerId, out byte oldVent);
                    bool inNew = ventData.TryGetValue(playerId, out byte newVent);

                    if (!inOld && inNew) PublishEvent(OnPlayerEnterVent, player, newVent);
                    else if (inOld && !inNew) PublishEvent(OnPlayerExitVent, player, oldVent);
                    else if (oldVent != newVent) PublishEvent(OnPlayerMoveVent, player, oldVent, newVent);
                }

                end:
                reader.Position = oldReadPosition;
            }
        }

        [HarmonyPatch(typeof(VentilationSystem), nameof(VentilationSystem.UpdateSystem))]
        class PlayerVentHost
        {
            static void Prefix(VentilationSystem __instance, PlayerControl player, MessageReader msgReader)
            {
                int oldReadPosition = msgReader.Position;
                msgReader.ReadUInt16();
                VentilationSystem.Operation operation = (VentilationSystem.Operation)msgReader.ReadByte();
                byte ventId = msgReader.ReadByte();

                switch (operation)
                {
                    case VentilationSystem.Operation.Enter:
                        PublishEvent(OnPlayerEnterVent, player, ventId);
                        break;
                    case VentilationSystem.Operation.Exit:
                        PublishEvent(OnPlayerExitVent, player, ventId);
                        break;
                    case VentilationSystem.Operation.Move:
                        byte oldVent = __instance.PlayersInsideVents[player.PlayerId];
                        PublishEvent(OnPlayerMoveVent, player, oldVent, ventId);
                        break;
                }

                msgReader.Position = oldReadPosition;
            }
        }

        [HarmonyPatch(typeof(SecurityCameraSystemType), nameof(SecurityCameraSystemType.UpdateSystem))]
        class UpdateCamerasHost
        {
            static void Postfix(PlayerControl player, MessageReader msgReader)
            {
                msgReader.Position--;
                byte operation = msgReader.ReadByte();
                msgReader.Position++;

                if (operation == 1) PublishEvent(OnPlayerEnterCameras, player);
                else PublishEvent(OnPlayerExitCameras, player);
            }
        }

        [HarmonyPatch(typeof(SecurityCameraSystemType), nameof(SecurityCameraSystemType.Deserialize))]
        class UpdateCamerasNonHost
        {
            static void Prefix(SecurityCameraSystemType __instance, MessageReader reader)
            {
                int oldReadPosition = reader.Position;
                int playerCount = reader.ReadPackedInt32();
                if (playerCount > PlayerControl.AllPlayerControls.Count || playerCount > reader.BytesRemaining) goto end;

                System.Collections.Generic.List<byte> players = new System.Collections.Generic.List<byte>();
                for (int i = 0; i < playerCount; i++) players.Add(reader.ReadByte());

                foreach (PlayerControl player in PlayerControl.AllPlayerControls)
                {
                    bool inOld = __instance.PlayersUsing.Contains(player.PlayerId);
                    bool inNew = players.Contains(player.PlayerId);

                    if (!inOld && inNew) PublishEvent(OnPlayerEnterCameras, player);
                    else if (inOld && !inNew) PublishEvent(OnPlayerExitCameras, player);
                }

                end:
                reader.Position = oldReadPosition;
            }
        }

        [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.MurderPlayer))]
        class PlayerMurder
        {
            static void Prefix(PlayerControl __instance, PlayerControl target, MurderResultFlags resultFlags)
            {
                PublishEvent(OnPlayerMurder, __instance, target, resultFlags);
            }
        }

        [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.Shapeshift))]
        class PlayerShapeshift
        {
            static void Prefix(PlayerControl __instance, PlayerControl targetPlayer, bool animate)
            {
                PublishEvent(OnPlayerShapeshift, __instance, targetPlayer, animate);
            }
        }

        [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.HandleServerVanish))]
        class PlayerPhantom
        {
            static void Prefix(PlayerControl __instance) => PublishEvent(OnPlayerPhantom, __instance);
        }

        [HarmonyPatch(typeof(VoteBanSystem), nameof(VoteBanSystem.AddVote))]
        class PlayerVotekick
        {
            static void Postfix(int srcClient, int clientId)
            {
                ClientData source = AmongUsClient.Instance.FindClientById(srcClient);
                ClientData target = AmongUsClient.Instance.FindClientById(clientId);
                if (source == null || target == null) return;

                if (clientId == AmongUsClient.Instance.ClientId)
                {
                    ZeroMenu.Plugin.notifications.Send("Votekick Logger", $"{source.PlayerName} has voted to kick you out.");
                }

                PublishEvent(OnPlayerVotekick, source, target);
            }
        }

        [HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.CastVote))]
        class PlayerCastVoteHost
        {
            static void Postfix(byte srcPlayerId, byte suspectPlayerId)
            {
                NetworkedPlayerInfo voter = GameData.Instance.GetPlayerById(srcPlayerId);
                NetworkedPlayerInfo votee = GameData.Instance.GetPlayerById(suspectPlayerId);
                if (voter == null) return;
                PublishEvent(OnPlayerCastVote, voter, votee);
            }
        }

        [HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.Deserialize))]
        class PlayerCastVoteNonHost
        {
            static void Prefix(MeetingHud __instance, MessageReader reader)
            {
                if (AmongUsClient.Instance.AmHost) return;

                int oldReadPosition = reader.Position;
                int playerStateCount = reader.ReadPackedInt32();
                if (playerStateCount > 1024 || playerStateCount > reader.BytesRemaining) goto end;

                System.Collections.Generic.Dictionary<byte, byte> playerVotes = new System.Collections.Generic.Dictionary<byte, byte>();
                for (int i = 0; i < playerStateCount; i++)
                {
                    MessageReader msg = reader.ReadMessage();
                    byte voteeId = msg.ReadByte();
                    playerVotes[msg.Tag] = voteeId;
                }

                foreach (PlayerControl player in PlayerControl.AllPlayerControls)
                {
                    if (player.Data == null) continue;
                    PlayerVoteArea oldState = __instance.playerStates.FirstOrDefault(state => state.PlayerId == player.PlayerId);

                    bool inOld = oldState != null && oldState.VotedForId != 255;
                    bool inNew = playerVotes.TryGetValue(player.PlayerId, out byte voteeId) && voteeId != 255;

                    if (!inOld && inNew)
                    {
                        NetworkedPlayerInfo votee = GameData.Instance.GetPlayerById(voteeId);
                        PublishEvent(OnPlayerCastVote, player.Data, votee);
                    }
                }

                end:
                reader.Position = oldReadPosition;
            }
        }

        [HarmonyPatch(typeof(HudOverrideSystemType), nameof(HudOverrideSystemType.UpdateSystem))]
        class UpdateHudOverrideHost
        {
            static void Postfix(MessageReader msgReader)
            {
                msgReader.Position--;
                byte operation = msgReader.ReadByte();
                msgReader.Position++;

                if ((operation & 128) != 0) PublishEvent(OnHudOverrideSabotage);
                else PublishEvent(OnHudOverrideRepair);
            }
        }

        [HarmonyPatch(typeof(HudOverrideSystemType), nameof(HudOverrideSystemType.Deserialize))]
        class UpdateHudOverrideNonHost
        {
            static void Prefix(HudOverrideSystemType __instance, MessageReader reader)
            {
                bool isActive = reader.ReadBoolean();
                bool wasActive = __instance.IsActive;
                reader.Position--;

                if (!wasActive && isActive) PublishEvent(OnHudOverrideSabotage);
                else if (wasActive && !isActive) PublishEvent(OnHudOverrideRepair);
            }
        }

        [HarmonyPatch(typeof(InnerNetObjectCollection), nameof(InnerNetObjectCollection.TryAddNetObject))]
        class NetObjectAdd
        {
            static void Postfix(InnerNetObject obj)
            {
                if (obj == null) return;
                Il2CppSystem.Type type = obj.GetIl2CppType();

                if (type == Il2CppType.From(typeof(LobbyBehaviour))) LobbyList.Enqueue(obj);
                else if (ShipNetObjects.Contains(type)) ShipList.Enqueue(obj);

                PublishEvent(OnNetObjectSpawn, obj);
            }
        }

        private static void PublishEvent(Action evt)
        {
            if (evt == null) return;
            evt();
        }

        private static void PublishEvent<T1>(Action<T1> evt, T1 arg1)
        {
            if (evt == null) return;
            evt(arg1);
        }

        private static void PublishEvent<T1, T2>(Action<T1, T2> evt, T1 arg1, T2 arg2)
        {
            if (evt == null) return;
            evt(arg1, arg2);
        }

        private static void PublishEvent<T1, T2, T3>(Action<T1, T2, T3> evt, T1 arg1, T2 arg2, T3 arg3)
        {
            if (evt == null) return;
            evt(arg1, arg2, arg3);
        }

        private static void PublishEvent<T1, T2, T3, T4>(Action<T1, T2, T3, T4> evt, T1 arg1, T2 arg2, T3 arg3, T4 arg4)
        {
            if (evt == null) return;
            evt(arg1, arg2, arg3, arg4);
        }
    }
}