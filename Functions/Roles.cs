using System;
using System.Linq;
using AmongUs.GameOptions;
using HarmonyLib;
using UnityEngine;

namespace ZeroMenu.Functions
{
    public static class Roles
    {
        public static bool KillReach = false;
        public static bool KillAnyone = false;
        public static bool KillVanished = false;
        public static bool KillOtherImpostors = false;
        
        public static bool NoSsAnim = false;
        public static bool EndlessSsDuration = false;
        
        public static bool EndlessVentTime = false;
        public static bool NoVentCooldown = false;
        
        public static bool EndlessBattery = false;
        public static bool NoVitalsCooldown = false;
        
        public static bool EndlessTracking = false;
        public static bool NoTrackDelay = false;
        public static bool NoTrackCooldown = false;
        public static bool TrackReach = false;
        
        public static bool NoVanishCooldown = false;
        public static bool EndlessVanishDuration = false;
        
        public static bool InterrogateReach = false;
        public static bool GaInfiniteRange = false;

        public static bool JudgeInstantUnlock = false;
        public static bool JudgeInfiniteGavels = false;
        
        public static bool NoSpiritGuideCooldown = false;

        [HarmonyPatch(typeof(JudgeRole), nameof(JudgeRole.IsBlockedByTasks))]
        public static class JudgeRole_IsBlockedByTasks
        {
            public static bool Prefix(ref bool __result)
            {
                if (!JudgeInstantUnlock) return true;
                __result = false;
                return false;
            }
        }

        [HarmonyPatch(typeof(JudgeRole), nameof(JudgeRole.ConsumeOverruleVotesUsage))]
        public static class JudgeRole_ConsumeOverrule
        {
            public static bool Prefix()
            {
                return !JudgeInfiniteGavels;
            }
        }

        [HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.Update))]
        public static class JudgeRole_Refill
        {
            public static void Postfix()
            {
                if (!JudgeInfiniteGavels) return;
                try
                {
                    var role = PlayerControl.LocalPlayer?.Data?.Role;
                    if (role == null || role.GetIl2CppType().Name != "JudgeRole") return;
                    var jr = role.TryCast<JudgeRole>();
                    if (jr == null) return;
                    jr.HasAnOverruleUse = true;
                    jr.HasAlreadyOverruledThisMeeting = false;
                }
                catch { }
            }
        }

        // --- INFLUENCER (SPIRIT GUIDE) PATCHES ---

        [HarmonyPatch(typeof(SpiritGuideRole), "SetCooldown")]
        public static class SpiritGuideRole_SetCooldown_Patch
        {
            public static bool Prefix(SpiritGuideRole __instance)
            {
                if (!NoSpiritGuideCooldown) return true;
                __instance.cooldownSecondsRemaining = 0f;
                __instance.selectionCooldown = false;
                return false;
            }
        }

        [HarmonyPatch(typeof(SpiritGuideRole), "SetInitialCooldown")]
        public static class SpiritGuideRole_SetInitialCooldown_Patch
        {
            public static bool Prefix(SpiritGuideRole __instance)
            {
                if (!NoSpiritGuideCooldown) return true;
                __instance.cooldownSecondsRemaining = 0f;
                return false;
            }
        }

        [HarmonyPatch(typeof(SpiritGuideRole), "SetRefreshCooldown")]
        public static class SpiritGuideRole_SetRefreshCooldown_Patch
        {
            public static bool Prefix(SpiritGuideRole __instance)
            {
                if (!NoSpiritGuideCooldown) return true;
                __instance.cooldownSecondsRemaining = 0f;
                return false;
            }
        }

        [HarmonyPatch(typeof(ActionButton), "SetCoolDown")]
        public static class ActionButton_SetCoolDown_Patch
        {
            public static bool Prefix(ActionButton __instance)
            {
                if (!NoSpiritGuideCooldown) return true;
                if (PlayerControl.LocalPlayer == null || PlayerControl.LocalPlayer.Data == null) return true;

                // 21 es el ID del Influencer en el enum RoleTypes
                if (PlayerControl.LocalPlayer.Data.RoleType != (RoleTypes)21) return true;

                try
                {
                    __instance.ResetCoolDown();
                    __instance.SetCooldownFill(0f);
                }
                catch { }

                return false;
            }
        }

        // --- OTROS ROLES ---

        [HarmonyPatch(typeof(EngineerRole), nameof(EngineerRole.FixedUpdate))]
        public static class EngineerRole_FixedUpdate
        {
            public static void Postfix(EngineerRole __instance)
            {
                if (__instance.Player.AmOwner)
                {
                    if (EndlessVentTime) __instance.inVentTimeRemaining = float.MaxValue;
                    if (NoVentCooldown) __instance.cooldownSecondsRemaining = 0f;
                }
            }
        }

        [HarmonyPatch(typeof(ShapeshifterRole), nameof(ShapeshifterRole.FixedUpdate))]
        public static class ShapeshifterRole_FixedUpdate
        {
            public static void Postfix(ShapeshifterRole __instance)
            {
                if (__instance.Player.AmOwner)
                {
                    if (EndlessSsDuration) __instance.durationSecondsRemaining = float.MaxValue;
                }
            }
        }

        [HarmonyPatch(typeof(ScientistRole), nameof(ScientistRole.Update))]
        public static class ScientistRole_Update
        {
            public static void Postfix(ScientistRole __instance)
            {
                if (__instance.Player.AmOwner)
                {
                    if (NoVitalsCooldown) __instance.currentCooldown = 0f;
                    if (EndlessBattery) __instance.currentCharge = float.MaxValue;
                }
            }
        }

        [HarmonyPatch(typeof(TrackerRole), nameof(TrackerRole.FixedUpdate))]
        public static class TrackerRole_FixedUpdate
        {
            public static void Postfix(TrackerRole __instance)
            {
                if (__instance.Player.AmOwner)
                {
                    if (NoTrackCooldown)
                    {
                        __instance.cooldownSecondsRemaining = 0f;
                        __instance.delaySecondsRemaining = 0f;
                    }
                    if (EndlessTracking) __instance.durationSecondsRemaining = float.MaxValue;
                }
            }
        }

        [HarmonyPatch(typeof(PhantomRole), nameof(PhantomRole.FixedUpdate))]
        public static class PhantomRole_FixedUpdate
        {
            public static void Postfix(PhantomRole __instance)
            {
                if (__instance.Player.AmOwner)
                {
                    if (NoVanishCooldown) __instance.cooldownSecondsRemaining = 0f;
                    if (EndlessVanishDuration) __instance.durationSecondsRemaining = float.MaxValue;
                }
            }
        }

        [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.CmdCheckShapeshift))]
        public static class PlayerControl_CmdCheckShapeshift
        {
            public static void Prefix(ref bool shouldAnimate)
            {
                if (NoSsAnim) shouldAnimate = false;
            }
        }

        [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.CmdCheckRevertShapeshift))]
        public static class PlayerControl_CmdCheckRevertShapeshift
        {
            public static void Prefix(ref bool shouldAnimate)
            {
                if (NoSsAnim) shouldAnimate = false;
            }
        }

        [HarmonyPatch(typeof(PhantomRole), nameof(PhantomRole.IsValidTarget))]
        public static class PhantomRole_IsValidTarget
        {
            public static void Postfix(NetworkedPlayerInfo target, ref bool __result)
            {
                if (KillVanished) __result = target != null && !target.Disconnected && target.Object != null;
            }
        }

        [HarmonyPatch(typeof(ImpostorRole), nameof(ImpostorRole.IsValidTarget))]
        public static class ImpostorRole_IsValidTarget
        {
            public static void Postfix(NetworkedPlayerInfo target, ref bool __result)
            {
                if (target == null || target.IsDead || target.Disconnected) return;
                if (target.PlayerId == PlayerControl.LocalPlayer.PlayerId) return;

                if (KillAnyone)
                {
                    __result = true;
                }
                else if (KillOtherImpostors)
                {
                    if (target.RoleType == RoleTypes.Impostor || target.RoleType == RoleTypes.Shapeshifter || target.RoleType == RoleTypes.Phantom || target.RoleType == RoleTypes.Viper)
                    {
                        __result = true;
                    }
                }
            }
        }

        [HarmonyPatch(typeof(ImpostorRole), nameof(ImpostorRole.FindClosestTarget))]
        public static class ImpostorRole_FindClosestTarget
        {
            public static bool Prefix(ImpostorRole __instance, ref PlayerControl __result)
            {
                if (!KillReach) return true;

                var playerList = PlayerControl.AllPlayerControls.ToArray()
                    .Where(p => p != null && !p.Data.IsDead && !p.Data.Disconnected && p.Collider.enabled && __instance.IsValidTarget(p.Data))
                    .OrderBy(p => Vector2.Distance(PlayerControl.LocalPlayer.transform.position, p.transform.position))
                    .ToList();

                if (playerList.Count > 0)
                {
                    __result = playerList[0];
                    return false;
                }
                return true;
            }
        }

        [HarmonyPatch(typeof(DetectiveRole), nameof(DetectiveRole.FindClosestTarget))]
        public static class DetectiveRole_FindClosestTarget
        {
            public static bool Prefix(DetectiveRole __instance, ref PlayerControl __result)
            {
                if (!InterrogateReach) return true;
                
                var playerList = PlayerControl.AllPlayerControls.ToArray()
                    .Where(p => p != null && !p.Data.IsDead && !p.Data.Disconnected && p.Collider.enabled && __instance.IsValidTarget(p.Data))
                    .OrderBy(p => Vector2.Distance(PlayerControl.LocalPlayer.transform.position, p.transform.position))
                    .ToList();

                if (playerList.Count > 0)
                {
                    __result = playerList[0];
                    return false;
                }
                return true;
            }
        }

        [HarmonyPatch(typeof(GuardianAngelRole), nameof(GuardianAngelRole.FindClosestTarget))]
        public static class GuardianAngelRole_FindClosestTarget
        {
            public static void Postfix(ref PlayerControl __result)
            {
                if (!GaInfiniteRange) return;

                PlayerControl best = null;
                float bestDist = float.MaxValue;

                foreach (var player in PlayerControl.AllPlayerControls)
                {
                    if (player == null || player.Data == null || player.Data.IsDead || player == PlayerControl.LocalPlayer) continue;
                    float dist = Vector2.Distance(PlayerControl.LocalPlayer.transform.position, player.transform.position);
                    if (dist < bestDist)
                    {
                        bestDist = dist;
                        best = player;
                    }
                }

                if (best != null) __result = best;
            }
        }

        [HarmonyPatch(typeof(TrackerRole), nameof(TrackerRole.FindClosestTarget))]
        public static class TrackerRole_FindClosestTarget
        {
            public static bool Prefix(TrackerRole __instance, ref PlayerControl __result)
            {
                if (!TrackReach) return true;

                var playerList = PlayerControl.AllPlayerControls.ToArray()
                    .Where(p => p != null && !p.Data.IsDead && !p.Data.Disconnected && p.Collider.enabled && __instance.IsValidTarget(p.Data))
                    .OrderBy(p => Vector2.Distance(PlayerControl.LocalPlayer.transform.position, p.transform.position))
                    .ToList();

                if (playerList.Count > 0)
                {
                    __result = playerList[0];
                    return false;
                }
                return true;
            }
        }
    }
}