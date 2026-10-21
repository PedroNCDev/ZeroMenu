using System;
using System.Collections;
using AmongUs.Data;
using BepInEx.Unity.IL2CPP.Utils.Collections;
using HarmonyLib;
using Hazel;
using UnityEngine;

namespace ZeroMenu.Functions
{
    public static class Self
    {
        public static bool UpdateStatsFreeplay = false;
        
        private static bool _immortalityEnabled = false;
        public static bool Immortality
        {
            get => _immortalityEnabled;
            set
            {
                if (value == _immortalityEnabled) return;
                if (PlayerControl.LocalPlayer != null && !PlayerControl.LocalPlayer.inVent)
                {
                    if (value) VentilationSystem.Update(VentilationSystem.Operation.Enter, 50);
                    else VentilationSystem.Update(VentilationSystem.Operation.Exit, 50);
                }
                _immortalityEnabled = value;
            }
        }

        public static bool FakeLag = false;
        public static bool AlwaysShowTaskAnimations = true;
        public static bool NoLadderCooldown = true;
        public static bool UnlimitedMeetings = true;
        public static bool Invisibility = false;
        public static bool SnipeColor = false;
        public static byte TargetColorId = 0;

        private static bool _showCustomizationUI = false;
        public static bool ShowCustomizationUI
        {
            get => _showCustomizationUI;
            set
            {
                if (value == _showCustomizationUI) return;
                _showCustomizationUI = value;

                if (value)
                {
                    if (PlayerCustomizationMenu.Instance == null)
                    {
                        try
                        {
                            var il2cppType = Il2CppInterop.Runtime.Il2CppType.Of<PlayerCustomizationMenu>();
                            var objects = Resources.FindObjectsOfTypeAll(il2cppType);
                            
                            if (objects != null && objects.Length > 0)
                            {
                                PlayerCustomizationMenu.Instance = (PlayerCustomizationMenu)objects[0];
                            }
                        }
                        catch { }
                    }

                    if (PlayerCustomizationMenu.Instance != null)
                    {
                        PlayerCustomizationMenu.Instance.Open();
                    }
                    else
                    {
                        ZeroMenu.Plugin.notifications.Send("Self", "Failed to open. Open it once in Main Menu first.", 3f);
                        _showCustomizationUI = false;
                    }
                }
                else
                {
                    if (PlayerCustomizationMenu.Instance != null)
                    {
                        PlayerCustomizationMenu.Instance.Close(true);
                    }
                }
            }
        }

        public static void CallMeeting()
        {
            if (PlayerControl.LocalPlayer == null) return;

            if (AmongUsClient.Instance.AmHost)
            {
                MeetingRoomManager.Instance.AssignSelf(PlayerControl.LocalPlayer, null);
                DestroyableSingleton<HudManager>.Instance.OpenMeetingRoom(PlayerControl.LocalPlayer);
                PlayerControl.LocalPlayer.RpcStartMeeting(null);
            }
            else
            {
                PlayerControl.LocalPlayer.CmdReportDeadBody(null);
            }
        }

        public static void StartCompleteAllTasks()
        {
            if (PlayerControl.LocalPlayer != null)
            {
                PlayerControl.LocalPlayer.StartCoroutine(BepInEx.Unity.IL2CPP.Utils.Collections.CollectionExtensions.WrapToIl2Cpp(CompleteAllTasks()));
            }
        }

        private static IEnumerator CompleteAllTasks()
        {
            foreach (PlayerTask task in PlayerControl.LocalPlayer.myTasks)
            {
                if (task != null && !task.IsComplete)
                {
                    var hostData = AmongUsClient.Instance.GetHost();
                    if (hostData != null && !hostData.Character.Data.Disconnected)
                    {
                        foreach (var player in PlayerControl.AllPlayerControls)
                        {
                            var writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)RpcCalls.CompleteTask, SendOption.Reliable, AmongUsClient.Instance.GetClientIdFromCharacter(player));
                            writer.WritePacked(task.Id);
                            AmongUsClient.Instance.FinishRpcImmediately(writer);
                        }
                    }
                    yield return new WaitForSeconds(0.1f);
                }
            }
        }

        public static void RestoreAvatar()
        {
            if (PlayerControl.LocalPlayer == null) return;

            PlayerControl.LocalPlayer.CmdCheckColor(DataManager.Player.Customization.Color);
            PlayerControl.LocalPlayer.RpcSetHat(DataManager.Player.Customization.Hat);
            PlayerControl.LocalPlayer.RpcSetVisor(DataManager.Player.Customization.Visor);
            PlayerControl.LocalPlayer.RpcSetSkin(DataManager.Player.Customization.Skin);
            PlayerControl.LocalPlayer.RpcSetPet(DataManager.Player.Customization.Pet);
            PlayerControl.LocalPlayer.RpcSetNamePlate(DataManager.Player.Customization.NamePlate);
        }
    }

    public static class SelfInvisibility
    {
        public static bool VisualReduction = true;
        private static SpriteRenderer[] _invisibleRendererCache;
        private static PlayerControl _invisibleCachedPlayer;
        private static bool _invisibleLastState;
        private static SpriteRenderer[] _invisiblePetRendererCache;
        private static PetBehaviour _invisibleCachedPet;
        private const float VoidCenter = 25000f;
        private const float VoidSpread = 100f;
        private static float _lastReinforceTime = 0f;

        private static bool ShouldRun() => Self.Invisibility;

        private static void Reinforce()
        {
            if (!ShouldRun() || PlayerControl.LocalPlayer == null) return;
            var netTransform = PlayerControl.LocalPlayer.NetTransform;
            if (netTransform != null) ReinforceOffMap(netTransform);
        }

        private static void ReinforceOffMap(CustomNetworkTransform netTransform)
        {
            try
            {
                ushort seq = (ushort)(netTransform.lastSequenceId + 1);
                netTransform.lastSequenceId = seq;
                
                var writer = AmongUsClient.Instance.StartRpcImmediately(netTransform.NetId, 21, SendOption.Reliable, -1);
                var pos = new Vector2(VoidCenter + UnityEngine.Random.Range(-VoidSpread, VoidSpread), VoidCenter + UnityEngine.Random.Range(-VoidSpread, VoidSpread));
                NetHelpers.WriteVector2(pos, writer);
                writer.Write(seq);
                AmongUsClient.Instance.FinishRpcImmediately(writer);
            }
            catch { }
        }

        public static void InvisibleVisualCheat()
        {
            if (!VisualReduction)
            {
                if (_invisibleLastState) { _invisibleLastState = false; ResetInvisibleVisual(); }
                return;
            }

            bool invisible = Self.Invisibility;
            PlayerControl lp = PlayerControl.LocalPlayer;

            if (!invisible)
            {
                if (_invisibleLastState) { _invisibleLastState = false; ResetInvisibleVisual(); }
            }
            else
            {
                _invisibleLastState = true;
                if (lp != null)
                {
                    if (_invisibleCachedPlayer != lp || _invisibleRendererCache == null)
                    {
                        _invisibleCachedPlayer = lp;
                        _invisibleRendererCache = lp.GetComponentsInChildren<SpriteRenderer>(true);
                    }
                    foreach (var sr in _invisibleRendererCache)
                    {
                        if (sr != null)
                        {
                            Color c = sr.color;
                            if (c.a != 0.5f) { c.a = 0.5f; sr.color = c; }
                        }
                    }
                    try
                    {
                        PetBehaviour currentPet = lp.cosmetics != null ? lp.cosmetics.CurrentPet : null;
                        if (currentPet != _invisibleCachedPet)
                        {
                            _invisibleCachedPet = currentPet;
                            _invisiblePetRendererCache = currentPet != null ? currentPet.GetComponentsInChildren<SpriteRenderer>(true) : null;
                        }
                        if (_invisiblePetRendererCache != null)
                        {
                            foreach (var sr2 in _invisiblePetRendererCache)
                            {
                                if (sr2 != null)
                                {
                                    Color c2 = sr2.color;
                                    if (c2.a != 0.5f) { c2.a = 0.5f; sr2.color = c2; }
                                }
                            }
                        }
                    }
                    catch { }
                }
            }
        }

        public static void ResetInvisibleVisual()
        {
            if (_invisibleRendererCache != null)
            {
                foreach (var sr in _invisibleRendererCache)
                {
                    if (sr != null) { Color c = sr.color; c.a = 1f; sr.color = c; }
                }
            }
            if (_invisiblePetRendererCache != null)
            {
                foreach (var sr2 in _invisiblePetRendererCache)
                {
                    if (sr2 != null) { Color c2 = sr2.color; c2.a = 1f; sr2.color = c2; }
                }
            }
            _invisibleLastState = false;
            _invisibleCachedPlayer = null;
            _invisibleRendererCache = null;
            _invisibleCachedPet = null;
            _invisiblePetRendererCache = null;
        }

        [HarmonyPatch(typeof(CustomNetworkTransform), "FixedUpdate")]
        private static class Invisibility_Patch
        {
            private static bool Prefix(CustomNetworkTransform __instance)
            {
                if (!ShouldRun()) return true;
                if (__instance.myPlayer == null || __instance.myPlayer != PlayerControl.LocalPlayer) return true;
                if (Time.time - _lastReinforceTime >= 0.8f)
                {
                    _lastReinforceTime = Time.time;
                    ReinforceOffMap(__instance);
                }
                return false;
            }
        }

        [HarmonyPatch(typeof(PlayerControl), "MurderPlayer")]
        private static class OnKill { private static void Postfix() { Reinforce(); } }

        [HarmonyPatch(typeof(PlayerPhysics), "RpcEnterVent")]
        private static class OnVentEnter { private static void Postfix() { Reinforce(); } }

        [HarmonyPatch(typeof(PlayerPhysics), "RpcExitVent")]
        private static class OnVentExit { private static void Postfix() { Reinforce(); } }

        [HarmonyPatch(typeof(MeetingHud), "Start")]
        private static class OnMeetingStart { private static void Postfix() { Reinforce(); } }

        [HarmonyPatch(typeof(MeetingHud), "Close")]
        private static class OnMeetingEnd { private static void Postfix() { Reinforce(); } }

        [HarmonyPatch(typeof(CustomNetworkTransform), "HandleRpc")]
        private static class OnSnapTo
        {
            private static void Postfix(CustomNetworkTransform __instance)
            {
                if (__instance.myPlayer == PlayerControl.LocalPlayer) Reinforce();
            }
        }

        [HarmonyPatch(typeof(PlayerPhysics), "RpcClimbLadder")]
        private static class OnClimbLadder { private static void Postfix() { Reinforce(); } }

        [HarmonyPatch(typeof(PlayerPhysics), "RpcBootFromVent")]
        private static class OnBootFromVent { private static void Postfix() { Reinforce(); } }

        [HarmonyPatch(typeof(PlayerControl), "RpcUsePlatform")]
        private static class OnUsePlatform { private static void Postfix() { Reinforce(); } }

        [HarmonyPatch(typeof(PlayerControl), "Exiled")]
        private static class OnExiled { private static void Postfix() { Reinforce(); } }

        [HarmonyPatch(typeof(PlayerPhysics), "HandleRpc")]
        private static class OnPhysicsRpc
        {
            private static void Postfix(PlayerPhysics __instance)
            {
                if (__instance != null && __instance.myPlayer == PlayerControl.LocalPlayer) Reinforce();
            }
        }

        [HarmonyPatch(typeof(PlayerControl), "HandleRpc")]
        private static class OnControlRpc
        {
            private static void Postfix(PlayerControl __instance)
            {
                if (__instance == PlayerControl.LocalPlayer) Reinforce();
            }
        }

        [HarmonyPatch(typeof(Vent), "EnterVent")]
        private static class OnVentEnterComponent
        {
            private static void Postfix(PlayerControl pc)
            {
                if (pc == PlayerControl.LocalPlayer) Reinforce();
            }
        }

        [HarmonyPatch(typeof(Vent), "ExitVent")]
        private static class OnVentExitComponent
        {
            private static void Postfix(PlayerControl pc)
            {
                if (pc == PlayerControl.LocalPlayer) Reinforce();
            }
        }

        [HarmonyPatch(typeof(PlayerControl), "Die")]
        private static class OnDie
        {
            private static void Postfix(PlayerControl __instance)
            {
                if (__instance == PlayerControl.LocalPlayer) Reinforce();
            }
        }
    }

    public static class SelfImmortality
    {
        private const int CustomVentId = 50;

        [HarmonyPatch(typeof(VentilationSystem), "Update")]
        private static class BlockSendingUpdates
        {
            private static bool Prefix(VentilationSystem.Operation op, int ventId)
            {
                if (ventId != CustomVentId && Self.Immortality && (op == VentilationSystem.Operation.Enter || op == VentilationSystem.Operation.Exit || op == VentilationSystem.Operation.BootImpostors))
                {
                    return false;
                }
                return true;
            }
        }

        [HarmonyPatch(typeof(ShipStatus), "Awake")]
        private static class OnShipStatusCreate
        {
            private static void Prefix()
            {
                if (Self.Immortality) VentilationSystem.Update(VentilationSystem.Operation.Enter, CustomVentId);
            }
        }

        [HarmonyPatch(typeof(MeetingHud), "Close")]
        private static class OnMeetingEnd
        {
            private static void Postfix()
            {
                if (Self.Immortality && PlayerControl.LocalPlayer != null && !PlayerControl.LocalPlayer.Data.IsDead)
                {
                    VentilationSystem.Update(VentilationSystem.Operation.Enter, CustomVentId);
                }
            }
        }
    }
}