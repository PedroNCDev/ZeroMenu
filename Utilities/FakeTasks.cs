using System;
using System.Collections;
using BepInEx.Unity.IL2CPP.Utils.Collections;
using HarmonyLib;
using UnityEngine;
using AmongUs.GameOptions;
using ZeroMenu.Functions;

namespace ZeroMenu.Utilities
{
    public static class FakeTasks
    {
        public static bool FakeScanTaskEnabled = false;
        public static bool FakeGarbageTaskEnabled = false;

        private static bool _runningScan = false;
        private static bool _runningGarbage = false;

        private static readonly Vector2 ScanPos = new Vector2(-7.21f, -4.78f);
        private static readonly Vector2 SecondPos = new Vector2(-8.57f, -4.58f);
        private static readonly Vector2 GarbagePos = new Vector2(0.75f, -16.56f);

        private const float WalkSpeedMultiplier = 0.27f;
        private const float ScanDuration = 9.1f;
        private const float ArriveTolerance = 0.15f;

        [HarmonyPatch(typeof(Console), nameof(Console.CanUse))]
        static class ConsoleCanUsePatch
        {
            static void Postfix(Console __instance, ref float __result, ref bool canUse, ref bool couldUse)
            {
                if (PlayerControl.LocalPlayer == null || __instance == null) return;

                if (FakeScanTaskEnabled && !_runningScan && IsScanConsole(__instance))
                {
                    canUse = true;
                    couldUse = true;
                    __result = 0f;
                }
                
                if (FakeGarbageTaskEnabled && !_runningGarbage && IsGarbageConsole(__instance))
                {
                    canUse = true;
                    couldUse = true;
                    __result = 0f;
                }
            }
        }

        [HarmonyPatch(typeof(Console), nameof(Console.Use))]
        static class ConsoleUsePatch
        {
            static bool Prefix(Console __instance)
            {
                if (PlayerControl.LocalPlayer == null || __instance == null) return true;

                if (FakeScanTaskEnabled && !_runningScan && IsScanConsole(__instance))
                {
                    PlayerControl.LocalPlayer.StartCoroutine(BepInEx.Unity.IL2CPP.Utils.Collections.CollectionExtensions.WrapToIl2Cpp(FakeScanRoutine()));
                    return false;
                }

                if (FakeGarbageTaskEnabled && !_runningGarbage && IsGarbageConsole(__instance))
                {
                    PlayerControl.LocalPlayer.StartCoroutine(BepInEx.Unity.IL2CPP.Utils.Collections.CollectionExtensions.WrapToIl2Cpp(FakeGarbageRoutine()));
                    return false;
                }

                return true;
            }
        }

        private static IEnumerator FakeScanRoutine()
        {
            var player = PlayerControl.LocalPlayer;
            if (player == null) yield break;

            _runningScan = true;
            player.moveable = false;

            var physics = player.MyPhysics;
            var body = physics?.body;
            if (body == null) { FinishScan(player); yield break; }

            float baseSpeed = physics.Speed;
            if (baseSpeed <= 0f) baseSpeed = 2.5f;

            while (Vector2.Distance((Vector2)player.transform.position, ScanPos) > ArriveTolerance)
            {
                if (player == null || !FakeScanTaskEnabled) { FinishScan(player); yield break; }
                if (PlayerMoved()) { FinishScan(player); yield break; }

                Vector2 dir = (ScanPos - (Vector2)player.transform.position).normalized;
                body.velocity = dir * baseSpeed * WalkSpeedMultiplier;
                yield return null;
            }

            body.velocity = Vector2.zero;
            Utils.ForceSetScanner(player, true);

            float scanStart = Time.time;
            while (Time.time - scanStart < ScanDuration)
            {
                if (player == null || !FakeScanTaskEnabled)
                {
                    Utils.ForceSetScanner(player, false);
                    FinishScan(player);
                    yield break;
                }

                if (PlayerMoved())
                {
                    Utils.ForceSetScanner(player, false);
                    FinishScan(player);
                    yield break;
                }

                yield return null;
            }

            Utils.ForceSetScanner(player, false);

            while (Vector2.Distance((Vector2)player.transform.position, SecondPos) > ArriveTolerance)
            {
                if (player == null || !FakeScanTaskEnabled) { FinishScan(player); yield break; }
                if (PlayerMoved()) { FinishScan(player); yield break; }

                Vector2 dir = (SecondPos - (Vector2)player.transform.position).normalized;
                body.velocity = dir * baseSpeed;
                yield return null;
            }

            FinishScan(player);
        }

        private static IEnumerator FakeGarbageRoutine()
        {
            var player = PlayerControl.LocalPlayer;
            if (player == null) yield break;

            _runningGarbage = true;
            player.moveable = false;

            var physics = player.MyPhysics;
            var body = physics?.body;
            if (body == null) { FinishGarbage(player); yield break; }

            float baseSpeed = physics.Speed;
            if (baseSpeed <= 0f) baseSpeed = 2.5f;

            while (Vector2.Distance((Vector2)player.transform.position, GarbagePos) > ArriveTolerance)
            {
                if (player == null || !FakeGarbageTaskEnabled) { FinishGarbage(player); yield break; }
                if (PlayerMoved()) { FinishGarbage(player); yield break; }

                Vector2 dir = (GarbagePos - (Vector2)player.transform.position).normalized;
                body.velocity = dir * baseSpeed;
                yield return null;
            }

            body.velocity = Vector2.zero;
            yield return new WaitForSeconds(3f);

            if (player == null || !FakeGarbageTaskEnabled) { FinishGarbage(player); yield break; }

            Utils.ForcePlayAnimation((byte)TaskTypes.EmptyGarbage);

            FinishGarbage(player);
        }

        private static bool PlayerMoved()
        {
            float h = Input.GetAxis("Horizontal");
            float v = Input.GetAxis("Vertical");
            return Mathf.Abs(h) > 0.1f || Mathf.Abs(v) > 0.1f;
        }

        private static void FinishScan(PlayerControl player)
        {
            _runningScan = false;
            try
            {
                if (player != null)
                {
                    player.moveable = true;
                    if (player.MyPhysics != null && player.MyPhysics.body != null)
                        player.MyPhysics.body.velocity = Vector2.zero;
                }
            }
            catch { }
        }

        private static void FinishGarbage(PlayerControl player)
        {
            _runningGarbage = false;
            try
            {
                if (player != null)
                {
                    player.moveable = true;
                    if (player.MyPhysics != null && player.MyPhysics.body != null)
                        player.MyPhysics.body.velocity = Vector2.zero;
                }
            }
            catch { }
        }

        private static bool IsScanConsole(Console console)
        {
            try
            {
                string typeName = console.GetIl2CppType().Name ?? "";
                if (typeName.IndexOf("MedScan", StringComparison.OrdinalIgnoreCase) >= 0) return true;
            }
            catch { }

            try
            {
                string goName = console.gameObject?.name ?? "";
                if (goName.IndexOf("MedScan", StringComparison.OrdinalIgnoreCase) >= 0) return true;
            }
            catch { }

            return false;
        }

        private static bool IsGarbageConsole(Console console)
        {
            if (console == null) return false;

            try
            {
                if (console.TaskTypes != null)
                {
                    foreach (var taskType in console.TaskTypes)
                    {
                        if (taskType == TaskTypes.EmptyGarbage || taskType == TaskTypes.EmptyChute)
                            return true;
                    }
                }
            }
            catch { }

            try
            {
                string typeName = console.GetIl2CppType().Name ?? "";
                if (typeName.IndexOf("Garbage", StringComparison.OrdinalIgnoreCase) >= 0 ||
                    typeName.IndexOf("Chute", StringComparison.OrdinalIgnoreCase) >= 0)
                    return true;
            }
            catch { }

            try
            {
                string goName = console.gameObject?.name ?? "";
                if (goName.IndexOf("Garbage", StringComparison.OrdinalIgnoreCase) >= 0 ||
                    goName.IndexOf("Chute", StringComparison.OrdinalIgnoreCase) >= 0)
                    return true;
            }
            catch { }

            try
            {
                var task = console.GetComponentInParent<NormalPlayerTask>();
                if (task != null && (task.TaskType == TaskTypes.EmptyGarbage || task.TaskType == TaskTypes.EmptyChute))
                    return true;
            }
            catch { }

            return false;
        }
    }
}