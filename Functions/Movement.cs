using System;
using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;

namespace ZeroMenu.Functions
{
    public static class Movement
    {
        public static bool NoClip = false;
        public static bool InvertControls = false;
        public static float SpeedMultiplier = 1f;
        public static bool TeleportCursor = false;
        public static bool UseSnapToRpc = true;

        public static Dictionary<string, Vector2> TeleportLocations = new Dictionary<string, Vector2>
        {
            { "Cafeteria", new Vector2(-1.2f, 3.5f) },
            { "MedBay", new Vector2(-9.5f, -3.5f) },
            { "Weapons", new Vector2(8.5f, 1.5f) },
            { "Navigation", new Vector2(16.5f, -4.5f) },
            { "Admin", new Vector2(4.5f, -8.5f) },
            { "Electrical", new Vector2(-7.5f, -9.5f) }
        };

        public static void TeleportTo(Vector2 location)
        {
            if (PlayerControl.LocalPlayer == null) return;

            if (UseSnapToRpc)
            {
                PlayerControl.LocalPlayer.NetTransform.RpcSnapTo(location);
            }
            else
            {
                PlayerControl.LocalPlayer.transform.position = location;
            }
        }

        [HarmonyPatch(typeof(PlayerPhysics), "TrueSpeed", MethodType.Getter)]
        public static class PlayerPhysics_TrueSpeed
        {
            public static void Postfix(ref float __result)
            {
                if (SpeedMultiplier != 1f)
                {
                    __result *= SpeedMultiplier;
                }
            }
        }

        [HarmonyPatch(typeof(PlayerPhysics), "LateUpdate")]
        public static class PlayerPhysics_LateUpdate
        {
            public static void Postfix(PlayerPhysics __instance)
            {
                if (PlayerControl.LocalPlayer == null) return;

                if (__instance.AmOwner)
                {
                    PlayerControl.LocalPlayer.Collider.enabled = !NoClip;

                    if (InvertControls)
                    {
                        __instance.Speed = -Mathf.Abs(__instance.Speed);
                        __instance.GhostSpeed = -Mathf.Abs(__instance.GhostSpeed);
                    }
                    else
                    {
                        __instance.Speed = Mathf.Abs(__instance.Speed);
                        __instance.GhostSpeed = Mathf.Abs(__instance.GhostSpeed);
                    }

                    if (TeleportCursor && Input.GetMouseButtonDown(1) && Camera.main != null)
                    {
                        Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                        TeleportTo(mousePos);
                    }
                }
            }
        }
    }
}