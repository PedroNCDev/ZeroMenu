using System;
using HarmonyLib;
using UnityEngine;
using AmongUs.GameOptions;

namespace ZeroMenu.Functions
{
    public static class Animations
    {
        public static bool Shields = false;
        public static bool Asteroids = false;
        public static bool EmptyGarbage = false;
        public static bool MedScan = false;
        public static bool CamsInUse = false;
        public static bool Moonwalk = false;

        private static bool _isScanActive = false;
        private static bool _isCamsActive = false;

        public static void ProcessAnimations()
        {
            if (PlayerControl.LocalPlayer == null || ShipStatus.Instance == null) return;

            byte mapId = (byte)Utils.GetCurrentMapID();
            if (mapId == byte.MaxValue) return;

            if (Shields)
            {
                if (mapId == 0 || mapId == 3) 
                {
                    Utils.ForcePlayAnimation((byte)TaskTypes.PrimeShields);
                }
                Shields = false;
            }

            if (Asteroids)
            {
                if (mapId == 0 || mapId == 3 || mapId == 2) 
                {
                    Utils.ForcePlayAnimation((byte)TaskTypes.ClearAsteroids);
                }
                else 
                {
                    Asteroids = false;
                }
            }

            if (EmptyGarbage)
            {
                if (mapId == 0 || mapId == 3) 
                {
                    Utils.ForcePlayAnimation((byte)TaskTypes.EmptyGarbage);
                }
                EmptyGarbage = false;
            }

            if (MedScan && !_isScanActive)
            {
                Utils.ForceSetScanner(PlayerControl.LocalPlayer, true);
                _isScanActive = true;
            }
            else if (!MedScan && _isScanActive)
            {
                Utils.ForceSetScanner(PlayerControl.LocalPlayer, false);
                _isScanActive = false;
            }

            if (mapId != 1 && mapId != 5)
            {
                if (CamsInUse && !_isCamsActive)
                {
                    ShipStatus.Instance.RpcUpdateSystem(SystemTypes.Security, 1);
                    _isCamsActive = true;
                }
                else if (!CamsInUse && _isCamsActive)
                {
                    ShipStatus.Instance.RpcUpdateSystem(SystemTypes.Security, 0);
                    _isCamsActive = false;
                }
            }
            else
            {
                CamsInUse = false;
            }
        }

        [HarmonyPatch(typeof(PlayerPhysics), "HandleAnimation")]
        public static class HandleAnimation_Patch
        {
            public static bool Prefix(PlayerPhysics __instance)
            {
                if (Moonwalk && __instance.AmOwner)
                {
                    __instance.ResetAnimState();
                    return false;
                }
                return true;
            }
        }

        [HarmonyPatch(typeof(PlayerPhysics), "LateUpdate")]
        public static class LateUpdate_Patch
        {
            public static void Postfix()
            {
                ProcessAnimations();
            }
        }
    }
}