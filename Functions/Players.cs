using System;
using System.Collections;
using System.Collections.Generic;
using AmongUs.GameOptions;
using BepInEx.Unity.IL2CPP.Utils.Collections;
using HarmonyLib;
using Hazel;
using InnerNet;
using UnityEngine;
using ZeroMenu.Utilities;

namespace ZeroMenu.Functions
{
    public static class Players
    {
        public static PlayerControl SelectedPlayer = null;
        public static bool IsSpectating = false;
        public static int SelectedVent = 0;

        public enum PlayerColors
        {
            Red, Blue, Green, Pink, Orange, Yellow, Black, White, Purple, Brown,
            Cyan, Lime, Maroon, Rose, Banana, Gray, Tan, Coral, Fortegreen
        }

        public static int SelectedColorIndex = 0;
        public static byte SelectedColor => (byte)SelectedColorIndex;

        public static readonly RoleTypes[] ValidRoles = new RoleTypes[]
        {
            RoleTypes.Crewmate,
            RoleTypes.Impostor,
            RoleTypes.Scientist,
            RoleTypes.Engineer,
            RoleTypes.GuardianAngel,
            RoleTypes.Shapeshifter,
            RoleTypes.CrewmateGhost,
            RoleTypes.ImpostorGhost,
            RoleTypes.Noisemaker,
            RoleTypes.Phantom,
            RoleTypes.Tracker,
            RoleTypes.Detective,
            RoleTypes.Viper,
            RoleTypes.Judge,
            RoleTypes.SpiritGuide
        };

        public static int SelectedRoleIndex = 0;
        public static RoleTypes SelectedRole => ValidRoles[SelectedRoleIndex];

        public static PlayerControl FollowTarget = null;
        public static PlayerControl PetTarget = null;
        public static PlayerControl JailTarget = null;
        
        private static float _petTimer = 0f;
        private static float _jailTimer = 0f;
        private static readonly Dictionary<PlayerControl, ushort> _ventSeqIds = new Dictionary<PlayerControl, ushort>();

        public static PlayerControl WhisperTarget = null;
        public static PlayerControl PendingWhisperDisplayTarget = null;

        public static void TeleportTo(PlayerControl target)
        {
            if (target == null || PlayerControl.LocalPlayer == null) return;
            PlayerControl.LocalPlayer.NetTransform.RpcSnapTo(target.transform.position);
            ZeroMenu.Plugin.notifications.Send("Players", $"Teleported to {target.Data.PlayerName}.", 3f);
        }

        public static void TeleportToVent(PlayerControl target, int ventId)
        {
            if (target == null || ShipStatus.Instance == null) return;
            
            if (AmongUsClient.Instance.AmHost)
            {
                target.MyPhysics.RpcBootFromVent(ventId);
            }
            else
            {
                if (!_ventSeqIds.ContainsKey(target)) _ventSeqIds.Add(target, 10000);
                
                MessageWriter enterVent = MessageWriter.Get((SendOption)0);
                _ventSeqIds[target]++;
                enterVent.Write(_ventSeqIds[target]);
                enterVent.Write(2);
                enterVent.Write((byte)ventId);

                MessageWriter bootFromVent = MessageWriter.Get((SendOption)0);
                _ventSeqIds[target]++;
                bootFromVent.Write(_ventSeqIds[target]);
                bootFromVent.Write(5);
                bootFromVent.Write((byte)ventId);

                BatchedMessage batch = new BatchedMessage(AmongUsClient.Instance.HostId);
                batch.QueueUpdateSystem(target, (SystemTypes)37, enterVent);
                batch.QueueUpdateSystem(target, (SystemTypes)37, bootFromVent);
                batch.FinishBatch();
                
                enterVent.Recycle();
                bootFromVent.Recycle();
            }
            ZeroMenu.Plugin.notifications.Send("Players", $"Moved {target.Data.PlayerName} to vent {ventId}.", 3f);
        }

        public static void Spectate(PlayerControl target)
        {
            if (target == null || Camera.main == null) return;
            var follower = Camera.main.GetComponent<FollowerCamera>();
            if (follower != null)
            {
                if (IsSpectating)
                {
                    follower.SetTarget(PlayerControl.LocalPlayer);
                    IsSpectating = false;
                    ZeroMenu.Plugin.notifications.Send("Players", "Stopped spectating.", 3f);
                }
                else
                {
                    follower.SetTarget(target);
                    IsSpectating = true;
                    ZeroMenu.Plugin.notifications.Send("Players", $"Spectating {target.Data.PlayerName}.", 3f);
                }
            }
        }

        public static void Follow(PlayerControl target)
        {
            if (target == null) return;
            FollowTarget = FollowTarget == target ? null : target;
            if (FollowTarget != null)
            {
                PlayerControl.LocalPlayer.moveable = false;
                PlayerControl.LocalPlayer.NetTransform.body.velocity = Vector2.zero;
            }
            else
            {
                PlayerControl.LocalPlayer.moveable = true;
            }
        }

        public static void PetPlayer(PlayerControl target)
        {
            if (target == null) return;
            PetTarget = PetTarget == target ? null : target;
            if (PetTarget != null)
            {
                PlayerControl.LocalPlayer.moveable = false;
                PlayerControl.LocalPlayer.NetTransform.body.velocity = Vector2.zero;
            }
            else
            {
                PlayerControl.LocalPlayer.moveable = true;
                PlayerControl.LocalPlayer.cosmetics.PettingHand.StopPetting();
            }
        }

        public static void JailPlayer(PlayerControl target)
        {
            if (target == null) return;
            JailTarget = JailTarget == target ? null : target;
        }

        public static void Whisper(PlayerControl target)
        {
            if (target == null) return;
            WhisperTarget = WhisperTarget == target ? null : target;
            ZeroMenu.Plugin.notifications.Send("Whisper", WhisperTarget != null ? $"Whispering to {target.Data.PlayerName}." : "Stopped whispering.", 3f);
        }

        public static void Murder(PlayerControl target)
        {
            if (target == null || PlayerControl.LocalPlayer == null) return;
            if (AmongUsClient.Instance.AmHost)
                PlayerControl.LocalPlayer.RpcMurderPlayer(target, true);
            else
                PlayerControl.LocalPlayer.CmdCheckMurder(target);
            ZeroMenu.Plugin.notifications.Send("Players", $"Attempted to kill {target.Data.PlayerName}.", 3f);
        }

        public static void StartTeleMurder(PlayerControl target)
        {
            if (AmongUsClient.Instance != null && PlayerControl.LocalPlayer != null && target != null)
            {
                AmongUsClient.Instance.StartCoroutine(TeleMurder(target).WrapToIl2Cpp());
            }
        }

        private static IEnumerator TeleMurder(PlayerControl target)
        {
            Vector2 savedPos = PlayerControl.LocalPlayer.NetTransform.transform.position;
            if (AmongUsClient.Instance.AmHost)
                PlayerControl.LocalPlayer.RpcMurderPlayer(target, true);
            else
                PlayerControl.LocalPlayer.CmdCheckMurder(target);

            float elapsed = 0f;
            float duration = Mathf.Max((AmongUsClient.Instance.Ping + 26) / 1000f, 0.320f);
            while (elapsed < duration)
            {
                PlayerControl.LocalPlayer.NetTransform.RpcSnapTo(savedPos);
                elapsed += Time.deltaTime;
                yield return null;
            }
        }

        public static void ReportBody(PlayerControl target)
        {
            if (target == null || PlayerControl.LocalPlayer == null) return;
            if (AmongUsClient.Instance.AmHost)
            {
                MeetingRoomManager.Instance.AssignSelf(PlayerControl.LocalPlayer, target.Data);
                PlayerControl.LocalPlayer.RpcStartMeeting(target.Data);
                DestroyableSingleton<HudManager>.Instance.OpenMeetingRoom(PlayerControl.LocalPlayer);
            }
            else
            {
                PlayerControl.LocalPlayer.CmdReportDeadBody(target.Data);
            }
            ZeroMenu.Plugin.notifications.Send("Players", $"Reported {target.Data.PlayerName}.", 3f);
        }

        public static void Eject(PlayerControl target)
        {
            if (target == null || MeetingHud.Instance == null) return;
            if (!AmongUsClient.Instance.AmHost) return;
            
            MeetingHud.Instance.RpcVotingComplete(new Il2CppInterop.Runtime.InteropTypes.Arrays.Il2CppStructArray<MeetingHud.VoterState>(0L), target.Data, false, false, 0);
            MeetingHud.Instance.RpcClose();
            ZeroMenu.Plugin.notifications.Send("Players", $"Ejected {target.Data.PlayerName}.", 3f);
        }

        public static void ForceMeeting(PlayerControl target)
        {
            if (target == null || !AmongUsClient.Instance.AmHost) return;
            MeetingRoomManager.Instance.AssignSelf(target, null);
            target.RpcStartMeeting(null);
            DestroyableSingleton<HudManager>.Instance.OpenMeetingRoom(target);
            ZeroMenu.Plugin.notifications.Send("Players", $"Forced meeting as {target.Data.PlayerName}.", 3f);
        }

        public static void ForceVotes(PlayerControl target)
        {
            if (target == null || MeetingHud.Instance == null || !AmongUsClient.Instance.AmHost) return;
            foreach (var player in PlayerControl.AllPlayerControls)
            {
                var voteArea = MeetingHud.Instance.playerStates[player.PlayerId];
                voteArea.SetVote(target.PlayerId);
            }
            MeetingHud.Instance.SetDirtyBit(1U);
            MeetingHud.Instance.CheckForEndVoting();
            ZeroMenu.Plugin.notifications.Send("Players", $"Forced votes on {target.Data.PlayerName}.", 3f);
        }

        public static void Revive(PlayerControl target)
        {
            if (target == null || !AmongUsClient.Instance.AmHost) return;
            if (!target.Data.IsDead) return;
            
            try
            {
                target.Data.IsDead = false;
                if (target.Collider != null) target.Collider.enabled = true;
                if (target.MyPhysics != null) target.MyPhysics.gameObject.layer = LayerMask.NameToLayer("Players");
                
                foreach (var db in UnityEngine.Object.FindObjectsOfType<DeadBody>())
                {
                    if (db != null && db.ParentId == target.PlayerId) db.gameObject.SetActive(false);
                }
                
                target.RpcSetRole(target.Data.RoleType, true);
                var netObj = GameData.Instance.GetComponent<InnerNetObject>();
                if (netObj != null) netObj.SetDirtyBit(uint.MaxValue);
                ZeroMenu.Plugin.notifications.Send("Players", $"{target.Data.PlayerName} revived!", 3f);
            }
            catch { }
        }

        public static void ShapeshiftInto(PlayerControl target)
        {
            if (target == null || PlayerControl.LocalPlayer == null) return;
            PlayerControl.LocalPlayer.RpcShapeshift(target, true);
            ZeroMenu.Plugin.notifications.Send("Players", $"Shapeshifted into {target.Data.PlayerName}.", 3f);
        }

        public static void CopyAvatar(PlayerControl target)
        {
            if (target == null) return;
            var outfit = target.CurrentOutfit;
            var batch = new BatchedMessage(-1);
            batch.QueueSetColor(PlayerControl.LocalPlayer, (byte)outfit.ColorId);
            batch.QueueSetHatStr(PlayerControl.LocalPlayer, outfit.HatId, ++outfit.HatSequenceId);
            batch.QueueSetVisorStr(PlayerControl.LocalPlayer, outfit.VisorId, ++outfit.VisorSequenceId);
            batch.QueueSetSkinStr(PlayerControl.LocalPlayer, outfit.SkinId, ++outfit.SkinSequenceId);
            batch.QueueSetPetStr(PlayerControl.LocalPlayer, outfit.PetId, ++outfit.PetSequenceId);
            batch.QueueSetNameplateStr(PlayerControl.LocalPlayer, outfit.NamePlateId, ++outfit.NamePlateSequenceId);
            batch.FinishBatch();
            ZeroMenu.Plugin.notifications.Send("Players", $"Copied {target.Data.PlayerName}'s avatar.", 3f);
        }

        public static void Votekick(PlayerControl target)
        {
            try
            {
                var client = AmongUsClient.Instance.GetClientFromCharacter(target);
                if (client != null) 
                {
                    VoteBanSystem.Instance.AddVote(AmongUsClient.Instance.ClientId, client.Id);
                    ZeroMenu.Plugin.notifications.Send("Players", $"Votekicked {target.Data.PlayerName}.", 3f);
                }
            } catch { }
        }

        public static void BanExploit(PlayerControl target) => Utils.KickPlayer(target);

        public static void FrameShapeshift(PlayerControl target)
        {
            if (target == null || !AmongUsClient.Instance.AmHost) return;
            var random = PlayerControl.AllPlayerControls[UnityEngine.Random.Range(0, PlayerControl.AllPlayerControls.Count)];
            if (random != target)
            {
                var batch = new BatchedMessage(-1);
                batch.QueueShapeshift(target, random, true);
                batch.FinishBatch();
                ZeroMenu.Plugin.notifications.Send("Players", $"Framed {target.Data.PlayerName} shapeshifting.", 3f);
            }
        }

        public static void FrameForKillingAll(PlayerControl target)
        {
            if (target == null || !AmongUsClient.Instance.AmHost) return;
            foreach (var player in PlayerControl.AllPlayerControls)
                if (player != target) PlayerControl.LocalPlayer.RpcMurderPlayer(player, true);
            ZeroMenu.Plugin.notifications.Send("Players", $"Framed {target.Data.PlayerName} for killing all.", 3f);
        }

        public static void FloodTasks(PlayerControl target)
        {
            if (target == null || !AmongUsClient.Instance.AmHost) return;
            byte[] tasks = new byte[255];
            for (int i = 0; i < 255; i++) tasks[i] = (byte)i;
            target.Data.RpcSetTasks(tasks);
            ZeroMenu.Plugin.notifications.Send("Players", $"Flooded {target.Data.PlayerName} with tasks.", 3f);
        }

        public static void ClearTasks(PlayerControl target)
        {
            if (target == null || !AmongUsClient.Instance.AmHost) return;
            target.Data.RpcSetTasks(Array.Empty<byte>());
            ZeroMenu.Plugin.notifications.Send("Players", $"Cleared tasks for {target.Data.PlayerName}.", 3f);
        }

        public static void SetColor(PlayerControl target)
        {
            if (target == null) return;
            target.RpcSetColor(SelectedColor);
            ZeroMenu.Plugin.notifications.Send("Players", $"Set color to {((PlayerColors)SelectedColorIndex).ToString()}.", 3f);
        }

        public static void SetRole(PlayerControl target)
        {
            if (target == null) return;
            target.RpcSetRole(SelectedRole, true);
            ZeroMenu.Plugin.notifications.Send("Players", $"Set role to {SelectedRole}.", 3f);
        }

        public static void SendFakeLights(PlayerControl target)
        {
            if (target == null || ShipStatus.Instance == null) return;
            int targetClientId = Utils.GetClientIdByPlayer(target);
            if (targetClientId < 0) return;
            byte switches = 4;
            for (int i = 0; i < 5; i++) if (UnityEngine.Random.Range(0f, 1f) > 0.5f) switches |= (byte)(1 << i);
            switches |= 128;
            SendUpdateSystemToClient(SystemTypes.Electrical, PlayerControl.LocalPlayer.NetId, new byte[] { switches }, targetClientId);
            ZeroMenu.Plugin.notifications.Send("Fake Lights", $"Sent to {target.Data.PlayerName}.", 3f);
        }

        public static void SendFakeOxygen(PlayerControl target)
        {
            if (target == null || ShipStatus.Instance == null) return;
            int targetClientId = Utils.GetClientIdByPlayer(target);
            if (targetClientId < 0) return;
            SendUpdateSystemToClient(SystemTypes.LifeSupp, PlayerControl.LocalPlayer.NetId, new byte[] { 128 }, targetClientId);
            ZeroMenu.Plugin.notifications.Send("Fake O2", $"Sent to {target.Data.PlayerName}.", 3f);
        }

        public static void SendFakeReactor(PlayerControl target)
        {
            if (target == null || ShipStatus.Instance == null) return;
            int targetClientId = Utils.GetClientIdByPlayer(target);
            if (targetClientId < 0) return;
            SendUpdateSystemToClient(SystemTypes.Reactor, PlayerControl.LocalPlayer.NetId, new byte[] { 128 }, targetClientId);
            ZeroMenu.Plugin.notifications.Send("Fake Reactor", $"Sent to {target.Data.PlayerName}.", 3f);
        }

        public static void DoorHallucination(PlayerControl target)
        {
            if (target == null || ShipStatus.Instance == null) return;
            int targetClientId = Utils.GetClientIdByPlayer(target);
            if (targetClientId < 0) return;
            foreach (var door in ShipStatus.Instance.AllDoors)
                SendUpdateSystemToClient(SystemTypes.Doors, PlayerControl.LocalPlayer.NetId, new byte[] { (byte)(door.Id | 64) }, targetClientId);
            ZeroMenu.Plugin.notifications.Send("Door Hallucination", $"Sent to {target.Data.PlayerName}.", 3f);
        }

        public static void SendGameOptions(PlayerControl target, float speed, float light) => Utils.SendGameOptions(target, speed, light);
        public static void NerfPlayer(PlayerControl target) => Utils.SendGameOptions(target, 1f, 0.5f);
        public static void BuffPlayer(PlayerControl target) => Utils.SendGameOptions(target, 3f, 500f);
        public static void ResetOptions(PlayerControl target) => Utils.SendGameOptions(target, 2.5f, 1f);

        public static void ToggleImmortality(PlayerControl target)
        {
            if (target == null || target.Data == null) return;
            if (AmongUsClient.Instance == null || ShipStatus.Instance == null) return;
            if (target == PlayerControl.LocalPlayer) return;

            bool on = !PlayerImmortality.IsImmortal(target.PlayerId);
            PlayerImmortality.Toggle(target);
            ZeroMenu.Plugin.notifications.Send("Immortality", on ? $"Granted to {target.Data.PlayerName}." : $"Removed from {target.Data.PlayerName}.", 3f);
        }

        public static bool IsImmortal(byte pid) => PlayerImmortality.IsImmortal(pid);

        public static void Protect(PlayerControl target)
        {
            if (target == null || PlayerControl.LocalPlayer == null) return;
            PlayerControl.LocalPlayer.RpcProtectPlayer(target, PlayerControl.LocalPlayer.cosmetics.ColorId);
            ZeroMenu.Plugin.notifications.Send("Players", $"Protected {target.Data.PlayerName}.", 3f);
        }

        private static void SendUpdateSystemToClient(SystemTypes systemType, uint senderNetId, byte[] extraBytes, int targetClientId)
        {
            MessageWriter w = AmongUsClient.Instance.StartRpcImmediately(ShipStatus.Instance.NetId, 35, (SendOption)1, targetClientId);
            w.Write((byte)systemType);
            w.WritePacked(senderNetId);
            foreach (byte b in extraBytes) w.Write(b);
            AmongUsClient.Instance.FinishRpcImmediately(w);
        }

        public static void UpdateRoutines()
        {
            if (PlayerControl.LocalPlayer == null) return;

            if (FollowTarget != null)
            {
                if (FollowTarget.Data == null || FollowTarget.Data.Disconnected)
                {
                    FollowTarget = null;
                    PlayerControl.LocalPlayer.moveable = true;
                }
                else
                {
                    PlayerControl.LocalPlayer.transform.position = FollowTarget.transform.position;
                }
            }

            if (PetTarget != null)
            {
                if (PetTarget.Data == null || PetTarget.Data.Disconnected)
                {
                    PetTarget = null;
                    PlayerControl.LocalPlayer.moveable = true;
                }
                else
                {
                    _petTimer += Time.deltaTime;
                    if (_petTimer >= 0.6f)
                    {
                        _petTimer = 0f;
                        Vector2 petPosition = PetTarget.transform.position;
                        petPosition.y -= PlayerControl.LocalPlayer.cosmetics.currentPet.yOffset * 2f;
                        PlayerControl.LocalPlayer.cosmetics.CurrentPet.SetGettingPet(true, petPosition);
                        PlayerControl.LocalPlayer.cosmetics.PettingHand.StartPet(PlayerControl.LocalPlayer.cosmetics.currentPet);
                        MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.MyPhysics.NetId, 49, (SendOption)1, -1);
                        NetHelpers.WriteVector2(PlayerControl.LocalPlayer.GetTruePosition(), writer);
                        NetHelpers.WriteVector2(petPosition, writer);
                        AmongUsClient.Instance.FinishRpcImmediately(writer);
                    }
                }
            }

            if (JailTarget != null && ShipStatus.Instance != null)
            {
                if (JailTarget.Data == null || JailTarget.Data.Disconnected)
                {
                    JailTarget = null;
                }
                else
                {
                    _jailTimer += Time.deltaTime;
                    if (_jailTimer >= 0.5f)
                    {
                        _jailTimer = 0f;
                        SystemTypes jailRoom = SystemTypes.Storage;
                        int ventId = 12;
                        
                        switch ((int)Utils.GetCurrentMapID())
                        {
                            case 1: jailRoom = SystemTypes.Laboratory; ventId = 9; break;
                            case 2: jailRoom = SystemTypes.Storage; ventId = 8; break;
                            case 4: jailRoom = (SystemTypes)44; ventId = 7; break;
                            case 5: jailRoom = SystemTypes.Kitchen; ventId = 4; break;
                        }

                        bool isInJail = false;
                        foreach (var room in ShipStatus.Instance.AllRooms)
                        {
                            if (room.RoomId == jailRoom && room.roomArea.OverlapPoint(JailTarget.GetTruePosition()))
                            {
                                isInJail = true;
                                break;
                            }
                        }

                        if (!isInJail)
                        {
                            TeleportToVent(JailTarget, ventId);
                        }
                    }
                }
            }
        }

        [HarmonyPatch(typeof(HudManager), "Update")]
        public static class HudManager_Update_Patch
        {
            public static void Postfix() => UpdateRoutines();
        }

        [HarmonyPatch(typeof(PlayerControl), "RpcSendChat")]
        public static class ChatMimic_RpcSendChat_Patch
        {
            public static bool Prefix(ref string chatText, PlayerControl __instance)
            {
                if (WhisperTarget != null && __instance.AmOwner)
                {
                    if (string.IsNullOrWhiteSpace(chatText)) return false;

                    int targetClientId = -1;
                    try { targetClientId = AmongUsClient.Instance.GetClientIdFromCharacter(WhisperTarget); } catch { }

                    if (targetClientId >= 0)
                    {
                        if (AmongUsClient.Instance.AmClient && DestroyableSingleton<HudManager>.Instance != null)
                        {
                            PendingWhisperDisplayTarget = WhisperTarget;
                            DestroyableSingleton<HudManager>.Instance.Chat.AddChat(__instance, chatText, false);
                        }
                        MessageWriter mw = AmongUsClient.Instance.StartRpcImmediately(__instance.NetId, 13, (SendOption)1, targetClientId);
                        mw.Write(chatText);
                        AmongUsClient.Instance.FinishRpcImmediately(mw);
                        WhisperTarget = null;
                        return false;
                    }
                }
                return !string.IsNullOrWhiteSpace(chatText);
            }
        }

        [HarmonyPatch(typeof(ChatBubble), "SetName")]
        public static class ChatBubble_SetName_Whisper
        {
            public static void Postfix(ChatBubble __instance)
            {
                if (PendingWhisperDisplayTarget != null && __instance.playerInfo != null && __instance.playerInfo.Object == PlayerControl.LocalPlayer)
                {
                    PlayerControl target = PendingWhisperDisplayTarget;
                    PendingWhisperDisplayTarget = null;
                    string theirName = target?.Data?.PlayerName ?? "???";
                    __instance.NameText.text = "[You] -> [" + theirName + "]";
                    __instance.NameText.ForceMeshUpdate(true, true);
                }
            }
        }
    }

    public static class PlayerImmortality
    {
        private const byte GrantVent = 231;
        private const int ReapplyInterval = 10;
        private static readonly HashSet<byte> _immortal = new HashSet<byte>();
        private static int _tickCount = 0;

        public static bool IsImmortal(byte pid) => _immortal.Contains(pid);

        public static void Toggle(PlayerControl target)
        {
            bool on = !_immortal.Contains(target.PlayerId);
            if (!SendVentRpc(target, on ? 2 : 3)) return;
            if (on) _immortal.Add(target.PlayerId);
            else _immortal.Remove(target.PlayerId);
        }

        private static bool SendVentRpc(PlayerControl target, int op)
        {
            MessageWriter body = null;
            try
            {
                var net = (InnerNetClient)AmongUsClient.Instance;
                body = MessageWriter.Get(SendOption.Reliable);
                body.Write((ushort)0);
                body.Write((byte)op);
                body.Write(GrantVent);

                MessageWriter w = net.StartRpcImmediately(((InnerNetObject)ShipStatus.Instance).NetId, 35, SendOption.Reliable, net.HostId);
                if (w == null) return false;
                w.Write((byte)SystemTypes.Ventilation);
                w.WriteNetObject(target);
                w.Write(body, false);
                net.FinishRpcImmediately(w);
                return true;
            }
            catch { return false; }
            finally { try { body?.Recycle(); } catch { } }
        }

        private static void ReapplyAll()
        {
            foreach (byte pid in _immortal)
                foreach (PlayerControl pc in PlayerControl.AllPlayerControls)
                    if (pc != null && pc.PlayerId == pid && !pc.Data.IsDead && !pc.inVent)
                        SendVentRpc(pc, 2);
        }

        [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.FixedUpdate))]
        public static class TickImmortality
        {
            public static void Postfix(PlayerControl __instance)
            {
                if (__instance != PlayerControl.LocalPlayer || _immortal.Count == 0 || ShipStatus.Instance == null || MeetingHud.Instance != null) return;
                _tickCount++;
                if (_tickCount >= ReapplyInterval) { ReapplyAll(); _tickCount = 0; }
            }
        }
    }
}