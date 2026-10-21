using System;
using System.Collections.Generic;
using System.Linq;
using AmongUs.GameOptions;
using AmongUs.InnerNet.GameDataMessages;
using BepInEx.Logging;
using HarmonyLib;
using Hazel;
using Il2CppInterop.Runtime;
using InnerNet;
using UnityEngine;
using ZeroMenu.Functions;

namespace ZeroMenu.Functions
{
    public static class Anticheat
    {
        public static bool Enabled { get; set; } = true;
        public static bool CheckSpoofedPlatforms { get; set; } = true;
        public static Punishments punishment = Punishments.None;
        public static bool sendNotification = true;
        public static bool discardRpc = true;
        public static float NotificationDuration = 10.0f;

        private static readonly BepInEx.Logging.ManualLogSource Log = BepInEx.Logging.Logger.CreateLogSource("ZeroMenu.Anticheat");

        public enum Punishments { None, Kick, ErrorKick, Ban }

        public static readonly Dictionary<GameDataTypes, GameDataCheck> GameDataHandlers = new Dictionary<GameDataTypes, GameDataCheck>()
        {
            { GameDataTypes.SceneChangeFlag, new SceneChange() },
            { GameDataTypes.ReadyFlag, new ClientReady() }
        };

        public static readonly Dictionary<RpcCalls, RpcCheck> RpcHandlers = new Dictionary<RpcCalls, RpcCheck>()
        {
            { RpcCalls.PlayAnimation, new PlayAnimation() },
            { RpcCalls.CompleteTask, new CompleteTask() },
            { RpcCalls.Exiled, new Exiled() },
            { RpcCalls.CheckName, new CheckName() },
            { RpcCalls.SetName, new SetName() },
            { RpcCalls.SetColor, new SetColor() },
            { RpcCalls.ReportDeadBody, new ReportDeadBody() },
            { RpcCalls.SetScanner, new SetScanner() },
            { RpcCalls.SetStartCounter, new SetStartCounter() },
            { RpcCalls.EnterVent, new EnterVent() },
            { RpcCalls.ExitVent, new ExitVent() },
            { RpcCalls.SnapTo, new SnapTo() },
            { RpcCalls.AddVote, new AddVote() },
            { RpcCalls.CloseDoorsOfType, new CloseDoorsOfType() },
            { RpcCalls.ClimbLadder, new ClimbLadder() },
            { RpcCalls.UsePlatform, new UsePlatform() },
            { RpcCalls.UpdateSystem, new UpdateSystem() },
            { RpcCalls.SetLevel, new SetLevel() }
        };

        public static void Flag(PlayerControl player, string reason, bool shouldPunish = true)
        {
            if (player == PlayerControl.LocalPlayer) return;
            if (sendNotification) ZeroMenu.Plugin.notifications.Send("Anticheat", reason, NotificationDuration);
            if (AmongUsClient.Instance.AmHost && shouldPunish) Punish(player);
        }

        public static void Flag(string reason)
        {
            if (sendNotification) ZeroMenu.Plugin.notifications.Send("Anticheat", reason, NotificationDuration);
        }

        private static void Punish(PlayerControl player)
        {
            switch (punishment)
            {
                case Punishments.None: break;
                case Punishments.Kick:
                case Punishments.ErrorKick:
                    if (punishment == Punishments.Kick || AmongUsClient.Instance.GameState != InnerNetClient.GameStates.Started)
                        AmongUsClient.Instance.KickPlayer(player.OwnerId, false);
                    else
                        AmongUsClient.Instance.SendLateRejection(player.OwnerId, DisconnectReasons.ClientTimeout);
                    break;
                case Punishments.Ban:
                    AmongUsClient.Instance.KickPlayer(player.OwnerId, true);
                    break;
            }
        }

        public static bool IsHideAndSeek()
        {
            return GameOptionsManager.Instance.currentGameOptions.GameMode == GameModes.HideNSeek;
        }

        [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.HandleRpc))]
        class OnPlayerControlRPC
        {
            static bool Prefix(PlayerControl __instance, byte callId, MessageReader reader)
            {
                return HandleRpc(typeof(PlayerControl), __instance, (RpcCalls)callId, reader);
            }
        }

        [HarmonyPatch(typeof(PlayerPhysics), nameof(PlayerPhysics.HandleRpc))]
        class OnPlayerPhysicsRPC
        {
            static bool Prefix(PlayerPhysics __instance, byte callId, MessageReader reader)
            {
                return HandleRpc(typeof(PlayerPhysics), __instance.myPlayer, (RpcCalls)callId, reader);
            }
        }

        [HarmonyPatch(typeof(CustomNetworkTransform), nameof(CustomNetworkTransform.HandleRpc))]
        class OnNetTransformRPC
        {
            static bool Prefix(CustomNetworkTransform __instance, byte callId, MessageReader reader)
            {
                return HandleRpc(typeof(CustomNetworkTransform), __instance.myPlayer, (RpcCalls)callId, reader);
            }
        }

        [HarmonyPatch(typeof(ShipStatus), nameof(ShipStatus.HandleRpc))]
        class OnShipStatusRPC
        {
            static bool Prefix(byte callId, MessageReader reader)
            {
                return HandleRpc(typeof(ShipStatus), null, (RpcCalls)callId, reader);
            }
        }

        [HarmonyPatch(typeof(InnerNetClient), "HandleGameData")]
        class OnHandleGameData
        {
            static bool Prefix(InnerNetClient __instance, MessageReader parentReader)
            {
                if (!Enabled) return true;
                int position = parentReader.Position;
                try
                {
                    while (parentReader.BytesRemaining >= 3)
                    {
                        MessageReader reader = parentReader.ReadMessage();
                        if (reader.Tag == 6)
                        {
                            if (!HandleGameData(GameDataTypes.ReadyFlag, reader)) return false;
                        }
                        else if (reader.Tag == 10)
                        {
                            if (!HandleGameData(GameDataTypes.SceneChangeFlag, reader)) return false;
                        }
                        reader.Recycle();
                    }
                }
                catch (Exception ex)
                {
                    Log.LogWarning($"[Anticheat] HandleGameData error: {ex.Message}");
                }
                finally
                {
                    parentReader.Position = position;
                }
                return true;
            }
        }

        public static bool HandleGameData(GameDataTypes type, MessageReader reader)
        {
            GameDataHandlers.TryGetValue(type, out GameDataCheck gameDataCheck);
            if (!Enabled || gameDataCheck == null || !gameDataCheck.Enabled) return true;
            int oldReadPosition = reader.Position;
            bool isValid = gameDataCheck.Validate(reader);
            reader.Position = oldReadPosition;
            return isValid || !discardRpc;
        }

        private static bool HandleRpc(Type sourceNetObj, PlayerControl player, RpcCalls rpc, MessageReader reader)
        {
            RpcHandlers.TryGetValue(rpc, out RpcCheck rpcCheck);
            if (!Enabled || rpcCheck == null || !rpcCheck.Enabled) return true;

            if (sourceNetObj != rpcCheck.GetExpectedNetObject()) return false;

            if (player != null && AmongUsClient.Instance.AmHost && rpcCheck.IsHostOnly())
            {
                Flag(player, $"{player.Data.PlayerName} sent the {rpc} RPC while non-host.");
                return false;
            }

            int oldReadPosition = reader.Position;
            bool isValid = rpcCheck.Validate(player, reader);
            reader.Position = oldReadPosition;

            return isValid || !discardRpc;
        }
    }

    public abstract class GameDataCheck : ICheck
    {
        public bool Enabled { get; set; } = true;
        public virtual bool Validate(MessageReader reader) => true;
        public abstract GameDataTypes GetId();
    }

    public interface ICheck
    {
        bool Enabled { get; set; }
    }

    public abstract class RpcCheck : ICheck
    {
        public bool Enabled { get; set; } = true;
        public virtual bool Validate(PlayerControl player, MessageReader reader) => true;
        public abstract RpcCalls GetId();
        public virtual bool IsHostOnly() => false;
        public virtual Type GetExpectedNetObject() => typeof(PlayerControl);
    }

    public class ClientReady : GameDataCheck
    {
        public override bool Validate(MessageReader reader)
        {
            int clientId = reader.ReadPackedInt32();
            ClientData client = AmongUsClient.Instance.FindClientById(clientId);
            if (client == null)
            {
                Anticheat.Flag($"Received ClientReady message for unknown client: {clientId}.");
                return false;
            }
            if (client.IsReady)
            {
                Anticheat.Flag(client.Character, $"{client.Character.Data.PlayerName} sent a ClientReady message while already ready.");
                return false;
            }
            return true;
        }
        public override GameDataTypes GetId() => GameDataTypes.ReadyFlag;
    }

    public class SceneChange : GameDataCheck
    {
        public override bool Validate(MessageReader reader)
        {
            int clientId = reader.ReadPackedInt32();
            string scene = reader.ReadString();
            ClientData client = AmongUsClient.Instance.FindClientById(clientId);
            if (client == null)
            {
                Anticheat.Flag($"Received SceneChange message for unknown client: {clientId}.");
                return false;
            }
            if (scene == "Tutorial")
            {
                Anticheat.Flag(client.Character, $"{client.Character.Data.PlayerName} sent a scene change of Tutorial.");
                return false;
            }
            return true;
        }
        public override GameDataTypes GetId() => GameDataTypes.SceneChangeFlag;
    }

    public class PlayAnimation : RpcCheck
    {
        public override bool Validate(PlayerControl player, MessageReader reader)
        {
            TaskTypes animation = (TaskTypes)reader.ReadByte();
            if (LobbyBehaviour.Instance)
            {
                Anticheat.Flag(player, $"{player.Data.PlayerName} sent the PlayAnimation RPC for task {animation} inside the lobby.");
                return false;
            }
            if (player.Data.Role.IsImpostor)
            {
                Anticheat.Flag(player, $"{player.Data.PlayerName} sent the PlayAnimation RPC for task {animation} when they are an imposter.");
                return false;
            }
            if (!GameOptionsManager.Instance.currentGameOptions.GetBool(BoolOptionNames.VisualTasks))
            {
                Anticheat.Flag(player, $"{player.Data.PlayerName} sent the PlayAnimation RPC for task {animation} when visual tasks are off.");
                return false;
            }
            return true;
        }
        public override RpcCalls GetId() => RpcCalls.PlayAnimation;
    }

    public class CompleteTask : RpcCheck
    {
        public override bool Validate(PlayerControl player, MessageReader reader)
        {
            uint taskIndex = reader.ReadPackedUInt32();
            if (ShipStatus.Instance == null)
            {
                Anticheat.Flag(player, $"{player.Data.PlayerName} tried completing task {taskIndex} when there was no valid instance of ShipStatus.");
                return false;
            }
            if (player.Data.Role.IsImpostor)
            {
                Anticheat.Flag(player, $"{player.Data.PlayerName} tried completing task {taskIndex} while being an imposter.");
                return false;
            }
            if (taskIndex + 1 > player.Data.Tasks.Count)
            {
                Anticheat.Flag(player, $"{player.Data.PlayerName} tried completing task {taskIndex} when they only have {player.Data.Tasks.Count} tasks.");
                return false;
            }
            return true;
        }
        public override RpcCalls GetId() => RpcCalls.CompleteTask;
    }

    public class Exiled : RpcCheck
    {
        public override bool Validate(PlayerControl player, MessageReader reader)
        {
            Anticheat.Flag(player, $"{player.Data.PlayerName} sent an invalid Exiled RPC.");
            return false;
        }
        public override RpcCalls GetId() => RpcCalls.Exiled;
    }

    public class CheckName : RpcCheck
    {
        public readonly int MAX_NAME_LENGTH = 10;
        public override bool Validate(PlayerControl player, MessageReader reader)
        {
            string requestedName = reader.ReadString();
            if (requestedName.Length > MAX_NAME_LENGTH)
            {
                Anticheat.Flag(player, $"{requestedName} tried setting their name to something too long ({requestedName.Length}).");
                return false;
            }
            if (requestedName.Contains('<'))
            {
                Anticheat.Flag(player, $"{requestedName} requested a name with invalid characters.");
                return false;
            }
            return true;
        }
        public override RpcCalls GetId() => RpcCalls.CheckName;
    }

    public class SetName : RpcCheck
    {
        public readonly int MAX_NAME_LENGTH = 12;
        public override bool Validate(PlayerControl player, MessageReader reader)
        {
            uint netId = reader.ReadUInt32();
            string requestedName = reader.ReadString();
            if (netId != GetExpectedNetId(player))
            {
                Anticheat.Flag(player, $"SetName RPC sent for {requestedName} includes an invalid net id, received {netId}, expected {GetExpectedNetId(player)}.");
                return false;
            }
            if (requestedName.Length > MAX_NAME_LENGTH)
            {
                Anticheat.Flag(player, $"{requestedName} tried setting their name to something too long ({requestedName.Length}).");
                return false;
            }
            if (requestedName.Contains('<'))
            {
                Anticheat.Flag(player, $"{requestedName} requested a name with invalid characters.");
                return false;
            }
            return true;
        }
        private uint GetExpectedNetId(PlayerControl player) => player.Data.NetId;
        public override RpcCalls GetId() => RpcCalls.SetName;
        public override bool IsHostOnly() => true;
    }

    public class SetColor : RpcCheck
    {
        public override bool Validate(PlayerControl player, MessageReader reader)
        {
            uint netId = reader.ReadUInt32();
            byte color = reader.ReadByte();
            if (netId != player.Data.NetId)
            {
                Anticheat.Flag(player, $"SetColor RPC sent for {player.Data.PlayerName} contains invalid net id, expected {player.Data.NetId}, received {netId}", false);
                player.SetColor(0);
                return false;
            }
            if (color >= Palette.PlayerColors.Length)
            {
                Anticheat.Flag(player, $"SetColor RPC sent for {player.Data.PlayerName} contains an invalid color: {color}", false);
                player.SetColor(0);
                return false;
            }
            return true;
        }
        public override RpcCalls GetId() => RpcCalls.SetColor;
        public override bool IsHostOnly() => true;
    }

    public class ReportDeadBody : RpcCheck
    {
        public override bool Validate(PlayerControl player, MessageReader reader)
        {
            if (Anticheat.IsHideAndSeek())
            {
                Anticheat.Flag(player, $"{player.Data.PlayerName} attempted to call a meeting in Hide and Seek");
                return false;
            }
            return true;
        }
        public override RpcCalls GetId() => RpcCalls.ReportDeadBody;
    }

    public class SetScanner : RpcCheck
    {
        public override bool Validate(PlayerControl player, MessageReader reader)
        {
            bool scanning = reader.ReadBoolean();
            if (ShipStatus.Instance == null && scanning)
            {
                Anticheat.Flag(player, $"{player.Data.PlayerName} sent the SetScanner RPC while the map has not spawned in yet.");
                return false;
            }
            if (player.Data.Role.IsImpostor && scanning)
            {
                Anticheat.Flag(player, $"{player.Data.PlayerName} sent the SetScanner RPC when they are an imposter.");
                return false;
            }
            if (!GameOptionsManager.Instance.currentGameOptions.GetBool(BoolOptionNames.VisualTasks))
            {
                Anticheat.Flag(player, $"{player.Data.PlayerName} sent the SetScanner RPC while visual tasks were disabled.");
                return false;
            }
            bool hasMedbayScanTask = false;
            foreach (NetworkedPlayerInfo.TaskInfo task in player.Data.Tasks)
            {
                if (task.Id == (byte)TaskTypes.SubmitScan)
                {
                    hasMedbayScanTask = true;
                    break;
                }
            }
            if (!hasMedbayScanTask && scanning)
            {
                Anticheat.Flag(player, $"{player.Data.PlayerName} sent the SetScanner RPC without being assigned the medbay scan task.");
                return false;
            }
            return true;
        }
        public override RpcCalls GetId() => RpcCalls.SetScanner;
    }

    public class SetStartCounter : RpcCheck
    {
        public override bool Validate(PlayerControl player, MessageReader reader)
        {
            reader.ReadPackedInt32();
            sbyte counter = reader.ReadSByte();
            if (player.OwnerId != AmongUsClient.Instance.HostId && counter != -1)
            {
                Anticheat.Flag(player, $"{player.Data.PlayerName} sent a SetStartCounter RPC with an invalid value: {counter}.");
                if (AmongUsClient.Instance.AmHost)
                {
                    PlayerControl.LocalPlayer.RpcSetStartCounter(-1);
                }
                return false;
            }
            return true;
        }
        public override RpcCalls GetId() => RpcCalls.SetStartCounter;
    }

    public class EnterVent : RpcCheck
    {
        public override bool Validate(PlayerControl player, MessageReader reader)
        {
            if (ShipStatus.Instance == null)
            {
                Anticheat.Flag(player, $"{player.Data.PlayerName} tried to vent when there is no instance of ShipStatus.");
                return false;
            }
            if (!player.Data.IsDead && !player.Data.Role.CanVent)
            {
                Anticheat.Flag(player, $"{player.Data.PlayerName} tried to vent when their role ({player.Data.RoleType}) does not support venting.");
                return false;
            }
            if (Anticheat.IsHideAndSeek() && player.Data.Role.IsImpostor)
            {
                Anticheat.Flag(player, $"{player.Data.PlayerName} tried to vent while being the seeker.");
                return false;
            }
            return true;
        }
        public override RpcCalls GetId() => RpcCalls.EnterVent;
        public override Type GetExpectedNetObject() => typeof(PlayerPhysics);
    }

    public class ExitVent : RpcCheck
    {
        public override bool Validate(PlayerControl player, MessageReader reader)
        {
            if (ShipStatus.Instance == null)
            {
                Anticheat.Flag(player, $"{player.Data.PlayerName} tried to exit a vent when there is no instance of ShipStatus.");
                return false;
            }
            if (!player.Data.IsDead && !player.Data.Role.CanVent)
            {
                Anticheat.Flag(player, $"{player.Data.PlayerName} tried to exit a vent when their role ({player.Data.RoleType}) does not support venting.");
                return false;
            }
            if (Anticheat.IsHideAndSeek() && player.Data.Role.IsImpostor)
            {
                Anticheat.Flag(player, $"{player.Data.PlayerName} tried to exit a vent while being the seeker.");
                return false;
            }
            return true;
        }
        public override RpcCalls GetId() => RpcCalls.ExitVent;
        public override Type GetExpectedNetObject() => typeof(PlayerPhysics);
    }

    public class SnapTo : RpcCheck
    {
        public override bool Validate(PlayerControl player, MessageReader reader)
        {
            if (LobbyBehaviour.Instance != null)
            {
                Anticheat.Flag(player, $"{player.Data.PlayerName} sent the SnapTo RPC while inside the lobby.");
                if (AmongUsClient.Instance.AmHost)
                {
                    player.NetTransform.RpcSnapTo(player.transform.position);
                }
                return false;
            }
            return true;
        }
        public override RpcCalls GetId() => RpcCalls.SnapTo;
        public override Type GetExpectedNetObject() => typeof(CustomNetworkTransform);
    }

    public class AddVote : RpcCheck
    {
        public override bool Validate(PlayerControl player, MessageReader reader)
        {
            int source = reader.ReadInt32();
            int target = reader.ReadInt32();
            ClientData client = AmongUsClient.Instance.FindClientById(source);
            if (client == null || client.Character == null) return false;
            player = client.Character;
            if (player.Data.IsDead)
            {
                Anticheat.Flag(player, $"{player.Data.PlayerName} attempted to votekick a player while dead.");
                return false;
            }
            if (MeetingHud.Instance == null)
            {
                Anticheat.Flag(player, $"{player.Data.PlayerName} attempted to votekick a player outside of a meeting.");
                return false;
            }
            return true;
        }
        public override RpcCalls GetId() => RpcCalls.AddVote;
        public override Type GetExpectedNetObject() => typeof(VoteBanSystem);
    }

    public class CloseDoorsOfType : RpcCheck
    {
        public override bool Validate(PlayerControl player, MessageReader reader)
        {
            if (Anticheat.IsHideAndSeek())
            {
                Anticheat.Flag("Someone attempted to close doors while in Hide and Seek.");
                return false;
            }
            return true;
        }
        public override RpcCalls GetId() => RpcCalls.CloseDoorsOfType;
        public override Type GetExpectedNetObject() => typeof(ShipStatus);
    }

    public class ClimbLadder : RpcCheck
    {
        public override bool Validate(PlayerControl player, MessageReader reader)
        {
            if (ShipStatus.Instance == null)
            {
                Anticheat.Flag(player, $"{player.Data.PlayerName} tried to climb a ladder when there is no instance of ShipStatus.");
                return false;
            }
            byte mapId = (byte)Utils.GetCurrentMapID();
            if (mapId != 4 && mapId != 5)
            {
                Anticheat.Flag(player, $"{player.Data.PlayerName} tried to climb a ladder outside of the proper map.");
                return false;
            }
            if (player.Data.IsDead)
            {
                Anticheat.Flag(player, $"{player.Data.PlayerName} tried to climb a ladder while dead.");
                return false;
            }
            return true;
        }
        public override RpcCalls GetId() => RpcCalls.ClimbLadder;
        public override Type GetExpectedNetObject() => typeof(PlayerPhysics);
    }

    public class UsePlatform : RpcCheck
    {
        public override bool Validate(PlayerControl player, MessageReader reader)
        {
            byte mapId = (byte)Utils.GetCurrentMapID();
            if (mapId != 4)
            {
                Anticheat.Flag(player, $"{player.Data.PlayerName} tried to use a platform outside of the proper map.");
                return false;
            }
            if (ShipStatus.Instance == null)
            {
                Anticheat.Flag(player, $"{player.Data.PlayerName} tried to use a platform when there is no instance of ShipStatus.");
                return false;
            }
            if (Anticheat.IsHideAndSeek())
            {
                Anticheat.Flag(player, $"{player.Data.PlayerName} tried to use a platform while in Hide and Seek.");
                return false;
            }
            return true;
        }
        public override RpcCalls GetId() => RpcCalls.UsePlatform;
    }

    public class SetLevel : RpcCheck
    {
        public readonly uint MAX_PLAYER_LEVEL = 10000;
        public override bool Validate(PlayerControl player, MessageReader reader)
        {
            uint level = reader.ReadPackedUInt32();
            if (level > MAX_PLAYER_LEVEL)
            {
                Anticheat.Flag(player, $"{player.Data.PlayerName} sent SetLevel RPC with a level that is too high ({level}).");
                player.SetLevel(MAX_PLAYER_LEVEL);
                return false;
            }
            if (ShipStatus.Instance)
            {
                Anticheat.Flag(player, $"{player.Data.PlayerName} sent SetLevel RPC when the game has already started.");
                return false;
            }
            return true;
        }
        public override RpcCalls GetId() => RpcCalls.SetLevel;
    }

    public class UpdateSystem : RpcCheck
    {
        private static readonly SystemTypes[] SystemsThatCanBeUpdatedWhenDead = {
            SystemTypes.MedBay, SystemTypes.Sabotage, SystemTypes.Security, SystemTypes.Ventilation
        };

        private static readonly Dictionary<Il2CppSystem.Type, Func<PlayerControl, MessageReader, bool>> systemHandlers = new Dictionary<Il2CppSystem.Type, Func<PlayerControl, MessageReader, bool>>()
        {
            { Il2CppType.From(typeof(HudOverrideSystemType)), ValidateHudOverrideSystem },
            { Il2CppType.From(typeof(LifeSuppSystemType)), ValidateLifeSuppSystem },
            { Il2CppType.From(typeof(MushroomMixupSabotageSystem)), ValidateMushroomMixupSystem },
            { Il2CppType.From(typeof(ReactorSystemType)), ValidateReactorSystem },
            { Il2CppType.From(typeof(SabotageSystemType)), ValidateSabotageSystem },
            { Il2CppType.From(typeof(SwitchSystem)), ValidateSwitchSystem }
        };

        public override bool Validate(PlayerControl player, MessageReader reader)
        {
            SystemTypes system = (SystemTypes)reader.ReadByte();
            player = reader.ReadNetObject<PlayerControl>();

            if (ShipStatus.Instance == null) return true;
            ShipStatus.Instance.Systems.TryGetValue(system, out ISystemType systemInterface);
            if (systemInterface == null)
            {
                Anticheat.Flag(player, $"{player.Data.PlayerName} tried to update system {system} when the current map has no such system.");
                return false;
            }
            if (player.Data.IsDead && !SystemsThatCanBeUpdatedWhenDead.Contains(system))
            {
                Anticheat.Flag(player, $"{player.Data.PlayerName} tried to update system {system} while dead.");
                return false;
            }

            Il2CppSystem.Type systemType = Il2CppType.TypeFromPointer(systemInterface.ObjectClass);
            systemHandlers.TryGetValue(systemType, out Func<PlayerControl, MessageReader, bool> handler);
            if (handler == null) return true;
            return handler(player, reader);
        }

        private static bool ValidateHudOverrideSystem(PlayerControl player, MessageReader reader)
        {
            byte operation = reader.ReadByte();
            if ((operation & 128) != 0)
            {
                Anticheat.Flag(player, $"{player.Data.PlayerName} attempted to force call the Comms sabotage");
                return false;
            }
            return true;
        }

        private static bool ValidateLifeSuppSystem(PlayerControl player, MessageReader reader)
        {
            byte operation = reader.ReadByte();
            if ((operation & 16) != 0)
            {
                Anticheat.Flag(player, $"{player.Data.PlayerName} attempted to force repair the LifeSupp sabotage");
                return false;
            }
            if ((operation & 128) != 0)
            {
                Anticheat.Flag(player, $"{player.Data.PlayerName} attempted to force call the LifeSupp sabotage");
                return false;
            }
            return true;
        }

        private static bool ValidateMushroomMixupSystem(PlayerControl player, MessageReader reader)
        {
            byte operation = reader.ReadByte();
            Anticheat.Flag(player, $"{player.Data.PlayerName} attempted to update Mushroom Mixup system with operation {operation}.");
            return false;
        }

        private static bool ValidateReactorSystem(PlayerControl player, MessageReader reader)
        {
            byte operation = reader.ReadByte();
            if ((operation & 16) != 0)
            {
                Anticheat.Flag(player, $"{player.Data.PlayerName} attempted to forcefully fix the Reactor sabotage");
                return false;
            }
            if ((operation & 128) != 0)
            {
                Anticheat.Flag(player, $"{player.Data.PlayerName} attempted to force call the Reactor sabotage");
                return false;
            }
            return true;
        }

        private static bool ValidateSabotageSystem(PlayerControl player, MessageReader reader)
        {
            SystemTypes system = (SystemTypes)reader.ReadByte();
            if (!player.Data.Role.IsImpostor)
            {
                Anticheat.Flag(player, $"{player.Data.PlayerName} attempted to sabotage {system} when they are not an imposter.");
                return false;
            }
            if (Anticheat.IsHideAndSeek())
            {
                Anticheat.Flag(player, $"{player.Data.PlayerName} attempted to sabotage {system} while in Hide and Seek.");
                return false;
            }
            if (player.inVent)
            {
                Anticheat.Flag(player, $"{player.Data.PlayerName} attempted to sabotage {system} while in a vent.");
                return false;
            }
            return true;
        }

        private static bool ValidateSwitchSystem(PlayerControl player, MessageReader reader)
        {
            byte switches = reader.ReadByte();
            if ((switches & 128) != 0)
            {
                Anticheat.Flag(player, $"{player.Data.PlayerName} attempted to bulk-update switches: {switches}.");
                return false;
            }
            if (switches > 5)
            {
                Anticheat.Flag(player, $"{player.Data.PlayerName} attempted to toggle an invalid switch: {switches}.");
                return false;
            }
            if (ShipStatus.Instance != null)
            {
                var switchSystem = ShipStatus.Instance.Systems[SystemTypes.Electrical].Cast<SwitchSystem>();
                if (switchSystem == null || !switchSystem.IsActive)
                {
                    return false;
                }
            }
            if (MeetingHud.Instance != null)
            {
                return false;
            }
            return true;
        }

        public override RpcCalls GetId() => RpcCalls.UpdateSystem;
        public override Type GetExpectedNetObject() => typeof(ShipStatus);
    }

    public static class PlatformSpoofer
    {
        [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.Start))]
        class PlatformSpoof
        {
            static void Postfix(PlayerControl __instance)
            {
                if (!Anticheat.Enabled || !Anticheat.CheckSpoofedPlatforms) return;
                ClientData clientData = AmongUsClient.Instance.GetClientFromCharacter(__instance);
                if (clientData == null) return;
                PlatformSpecificData platformData = clientData.PlatformData;
                if (!IsValidPlatform(platformData))
                {
                    Anticheat.Flag(__instance, $"{clientData.PlayerName} was detected with spoofed platform information. Platform: {platformData.Platform}, Platform name: {platformData.PlatformName}, XUID: {platformData.XboxPlatformId}, PSID: {platformData.PsnPlatformId}.");
                }
            }
        }

        public static bool IsValidPlatform(PlatformSpecificData platform)
        {
            string platformName = platform.PlatformName;
            ulong xuid = platform.XboxPlatformId;
            ulong psid = platform.PsnPlatformId;

            switch (platform.Platform)
            {
                case Platforms.StandaloneEpicPC:
                case Platforms.StandaloneSteamPC:
                case Platforms.StandaloneMac:
                case Platforms.StandaloneItch:
                case Platforms.IPhone:
                case Platforms.Android:
                case (Platforms)112:
                    if (IsGenericPlatformName(platformName) && xuid == 0 && psid == 0) return true;
                    break;
                case Platforms.StandaloneWin10:
                    if (IsGenericPlatformName(platformName) && xuid != 0 && psid == 0) return true;
                    break;
                case Platforms.Xbox:
                    if (!IsGenericPlatformName(platformName) && platformName.Length >= 3 && platformName.Length <= 16 && xuid != 0 && psid == 0) return true;
                    break;
                case Platforms.Playstation:
                    if (!IsGenericPlatformName(platformName) && xuid == 0 && psid != 0) return true;
                    break;
                case Platforms.Switch:
                    if (!IsGenericPlatformName(platformName) && xuid == 0 && psid == 0) return true;
                    break;
                case (Platforms)255:
                    if (AmongUsClient.Instance.NetworkMode == NetworkModes.LocalGame) return true;
                    break;
            }
            return false;
        }

        public static bool IsGenericPlatformName(string platformName) => platformName == "TESTNAME";
    }
}