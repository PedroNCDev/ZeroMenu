using AmongUs.GameOptions;
using AmongUs.InnerNet.GameDataMessages;
using Hazel;
using InnerNet;
using UnityEngine;

namespace ZeroMenu.Utilities
{
    public class BatchedMessage
    {
        public readonly MessageWriter writer;
        public readonly int targetClientId;
        public int msgCount = 0;
        private static readonly BepInEx.Logging.ManualLogSource Log = BepInEx.Logging.Logger.CreateLogSource("ZeroMenu");

        public BatchedMessage(int targetClientId = -1)
        {
            writer = MessageWriter.Get(SendOption.Reliable);
            this.targetClientId = targetClientId;

            if (targetClientId == -1)
            {
                writer.StartMessage(InnerNet.Tags.GameData);
                writer.Write(AmongUsClient.Instance.GameId);
            }
            else
            {
                writer.StartMessage(InnerNet.Tags.GameDataTo);
                writer.Write(AmongUsClient.Instance.GameId);
                writer.WritePacked(targetClientId);
            }
        }

        private bool IsGlobal => targetClientId == -1;
        private bool AmTarget => targetClientId == AmongUsClient.Instance.ClientId;

        public void QueueDataFlag(uint netId, MessageWriter msg)
        {
            writer.StartMessage((byte)GameDataTypes.DataFlag);
            writer.WritePacked(netId);
            writer.Write(msg, false);
            writer.EndMessage();
            msgCount++;
        }

        public void QueueSetColor(PlayerControl source, byte color)
        {
            if (IsGlobal || AmTarget) { source.SetColor(color); if (AmTarget) return; }
            writer.StartMessage((byte)GameDataTypes.RpcFlag);
            writer.WritePacked(source.NetId);
            writer.Write((byte)RpcCalls.SetColor);
            writer.Write(source.Data.NetId);
            writer.Write(color);
            writer.EndMessage();
            msgCount++;
        }

        public void QueueSetHatStr(PlayerControl source, string hat, byte seqId)
        {
            if (IsGlobal || AmTarget) { source.SetHat(hat, source.Data.DefaultOutfit.ColorId); if (AmTarget) return; }
            writer.StartMessage((byte)GameDataTypes.RpcFlag);
            writer.WritePacked(source.NetId);
            writer.Write((byte)RpcCalls.SetHatStr);
            writer.Write(hat);
            writer.Write(seqId);
            writer.EndMessage();
            msgCount++;
        }

        public void QueueSetVisorStr(PlayerControl source, string visor, byte seqId)
        {
            if (IsGlobal || AmTarget) { source.SetVisor(visor, source.Data.DefaultOutfit.ColorId); if (AmTarget) return; }
            writer.StartMessage((byte)GameDataTypes.RpcFlag);
            writer.WritePacked(source.NetId);
            writer.Write((byte)RpcCalls.SetVisorStr);
            writer.Write(visor);
            writer.Write(seqId);
            writer.EndMessage();
            msgCount++;
        }

        public void QueueSetSkinStr(PlayerControl source, string skin, byte seqId)
        {
            if (IsGlobal || AmTarget) { source.SetSkin(skin, source.Data.DefaultOutfit.ColorId); if (AmTarget) return; }
            writer.StartMessage((byte)GameDataTypes.RpcFlag);
            writer.WritePacked(source.NetId);
            writer.Write((byte)RpcCalls.SetSkinStr);
            writer.Write(skin);
            writer.Write(seqId);
            writer.EndMessage();
            msgCount++;
        }

        public void QueueSetPetStr(PlayerControl source, string pet, byte seqId)
        {
            if (IsGlobal || AmTarget) { source.SetPet(pet, source.Data.DefaultOutfit.ColorId); if (AmTarget) return; }
            writer.StartMessage((byte)GameDataTypes.RpcFlag);
            writer.WritePacked(source.NetId);
            writer.Write((byte)RpcCalls.SetPetStr);
            writer.Write(pet);
            writer.Write(seqId);
            writer.EndMessage();
            msgCount++;
        }

        public void QueueSetNameplateStr(PlayerControl source, string nameplate, byte seqId)
        {
            if (IsGlobal || AmTarget) { source.SetNamePlate(nameplate); if (AmTarget) return; }
            writer.StartMessage((byte)GameDataTypes.RpcFlag);
            writer.WritePacked(source.NetId);
            writer.Write((byte)RpcCalls.SetNamePlateStr);
            writer.Write(nameplate);
            writer.Write(seqId);
            writer.EndMessage();
            msgCount++;
        }

        public void QueueSetRole(PlayerControl source, RoleTypes role, bool canOverride = false)
        {
            if (IsGlobal || AmTarget) { source.StartCoroutine(source.CoSetRole(role, canOverride)); if (AmTarget) return; }
            writer.StartMessage((byte)GameDataTypes.RpcFlag);
            writer.WritePacked(source.NetId);
            writer.Write((byte)RpcCalls.SetRole);
            writer.Write((ushort)role);
            writer.Write(canOverride);
            writer.EndMessage();
            msgCount++;
        }

        public void QueueShapeshift(PlayerControl source, PlayerControl target, bool shouldAnimate)
        {
            if (IsGlobal || AmTarget) { source.Shapeshift(target, shouldAnimate); if (AmTarget) return; }
            writer.StartMessage((byte)GameDataTypes.RpcFlag);
            writer.WritePacked(source.NetId);
            writer.Write((byte)RpcCalls.Shapeshift);
            writer.WriteNetObject(target);
            writer.Write(shouldAnimate);
            writer.EndMessage();
            msgCount++;
        }

        public void QueueUpdateSystem(PlayerControl source, SystemTypes system, byte value)
        {
            if (IsGlobal || AmTarget)
            {
                ShipStatus.Instance.UpdateSystem(system, source, value);
                if (AmTarget) return;
            }
            writer.StartMessage((byte)GameDataTypes.RpcFlag);
            writer.WritePacked(ShipStatus.Instance.NetId);
            writer.Write((byte)RpcCalls.UpdateSystem);
            writer.Write((byte)system);
            writer.WriteNetObject(source);
            writer.Write(value);
            writer.EndMessage();
            msgCount++;
        }

        public void QueueUpdateSystem(PlayerControl source, SystemTypes system, MessageWriter msg)
        {
            if (IsGlobal || AmTarget)
            {
                MessageReader reader = MessageReader.Get(msg.ToByteArray(false));
                ShipStatus.Instance.UpdateSystem(system, source, reader);
                if (AmTarget) return;
            }
            writer.StartMessage((byte)GameDataTypes.RpcFlag);
            writer.WritePacked(ShipStatus.Instance.NetId);
            writer.Write((byte)RpcCalls.UpdateSystem);
            writer.Write((byte)system);
            writer.WriteNetObject(source);
            writer.Write(msg, false);
            writer.EndMessage();
            msgCount++;
        }

        public void QueueCloseDoors(SystemTypes door)
        {
            if (IsGlobal || AmTarget)
            {
                ShipStatus.Instance.CloseDoorsOfType(door);
                if (AmTarget) return;
            }
            writer.StartMessage((byte)GameDataTypes.RpcFlag);
            writer.WritePacked(ShipStatus.Instance.NetId);
            writer.Write((byte)RpcCalls.CloseDoorsOfType);
            writer.Write((byte)door);
            writer.EndMessage();
            msgCount++;
        }

        public void FinishBatch()
        {
            writer.EndMessage();
            if (msgCount > 0)
            {
                AmongUsClient.Instance.SendOrDisconnect(writer);
            }
            writer.Recycle();
        }
    }
}