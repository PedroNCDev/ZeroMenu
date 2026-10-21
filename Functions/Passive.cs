using System;
using System.Security.Cryptography;
using HarmonyLib;
using InnerNet;
using AmongUs.Data;
using AmongUs.Data.Player;
using UnityEngine;
using ZeroMenu.Functions;

namespace ZeroMenu.Functions
{
    public static class Passive
    {
        public static bool FreeCosmetics = false;
        public static bool AvoidPenalties = false;
        public static bool UnlockFeatures = false;
        public static bool CopyLobbyCodeOnDisconnect = false;
        public static bool SpoofAprilFoolsDate = false;
        public static bool SpoofDeviceId = false;

        [HarmonyPatch(typeof(PlayerPurchasesData), nameof(PlayerPurchasesData.GetPurchase))]
        public static class PlayerPurchasesData_GetPurchase
        {
            public static void Postfix(ref bool __result)
            {
                if (FreeCosmetics) __result = true;
            }
        }

        [HarmonyPatch(typeof(PlayerBanData), nameof(PlayerBanData.BanMinutesLeft), MethodType.Getter)]
        public static class PlayerBanData_BanMinutesLeft_Getter
        {
            public static void Postfix(PlayerBanData __instance, ref int __result)
            {
                if (!AvoidPenalties) return;
                __instance.BanPoints = 0f;
                __result = 0;
            }
        }

        [HarmonyPatch(typeof(FullAccount), nameof(FullAccount.CanSetCustomName))]
        public static class FullAccount_CanSetCustomName
        {
            public static void Prefix(ref bool canSetName)
            {
                if (UnlockFeatures) canSetName = true;
            }
        }

        [HarmonyPatch(typeof(AccountManager), nameof(AccountManager.CanPlayOnline))]
        public static class AccountManager_CanPlayOnline
        {
            public static void Postfix(ref bool __result)
            {
                if (UnlockFeatures) __result = true;
            }
        }

        [HarmonyPatch(typeof(InnerNetClient), nameof(InnerNetClient.JoinGame))]
        public static class InnerNetClient_JoinGame
        {
            public static void Prefix()
            {
                if (UnlockFeatures) DataManager.Player.Account.LoginStatus = EOSManager.AccountLoginStatus.LoggedIn;
            }
        }

        [HarmonyPatch(typeof(EOSManager), nameof(EOSManager.IsFreechatAllowed))]
        public static class EOSManager_IsFreechatAllowed
        {
            public static void Postfix(ref bool __result)
            {
                if (UnlockFeatures) __result = true;
            }
        }

        [HarmonyPatch(typeof(EOSManager), nameof(EOSManager.IsFriendsListAllowed))]
        public static class EOSManager_IsFriendsListAllowed
        {
            public static void Postfix(ref bool __result)
            {
                if (UnlockFeatures) __result = true;
            }
        }

        [HarmonyPatch(typeof(EOSManager), nameof(EOSManager.IsAllowedOnline))]
        public static class EOSManager_IsAllowedOnline
        {
            public static void Prefix(ref bool canOnline)
            {
                if (UnlockFeatures) canOnline = true;
            }
        }

        [HarmonyPatch(typeof(EOSManager), nameof(EOSManager.IsMinorOrWaiting))]
        public static class EOSManager_IsMinorOrWaiting
        {
            public static void Postfix(ref bool __result)
            {
                if (UnlockFeatures) __result = false;
            }
        }

        [HarmonyPatch(typeof(EOSManager), nameof(EOSManager.HasServerTimestamp), MethodType.Getter)]
        public static class EOSManager_HasServerTimestamp_Getter
        {
            public static void Postfix(ref bool __result)
            {
                if (SpoofAprilFoolsDate) __result = true;
            }
        }

        [HarmonyPatch(typeof(EOSManager), nameof(EOSManager.ApproximateServerTime), MethodType.Getter)]
        public static class EOSManager_ApproximateServerTime_Getter
        {
            public static void Postfix(ref Il2CppSystem.DateTime __result)
            {
                if (!SpoofAprilFoolsDate) return;
                var managedDate = new DateTime(DateTime.UtcNow.Year, 4, 1, 7, 1, 0, DateTimeKind.Utc);
                __result = new Il2CppSystem.DateTime(managedDate.Ticks);
            }
        }

        [HarmonyPatch(typeof(SystemInfo), nameof(SystemInfo.deviceUniqueIdentifier), MethodType.Getter)]
        public static class SystemInfo_deviceUniqueIdentifier_Getter
        {
            public static void Postfix(ref string __result)
            {
                if (!SpoofDeviceId) return;
                var bytes = new byte[16];
                using (var rng = RandomNumberGenerator.Create())
                {
                    rng.GetBytes(bytes);
                }
                __result = BitConverter.ToString(bytes).Replace("-", "").ToLower();
            }
        }

        [HarmonyPatch(typeof(DisconnectPopup), nameof(DisconnectPopup.DoShow))]
        public static class DisconnectPopup_DoShow
        {
            public static void Postfix(DisconnectPopup __instance)
            {
                if (!CopyLobbyCodeOnDisconnect || Tuff.LastGameId == 0) return;
                
                GUIUtility.systemCopyBuffer = GameCode.IntToGameName(Tuff.LastGameId);
                __instance.SetText(__instance._textArea.text + "\n\n<size=60%>Lobby code has been copied to the clipboard</size>");
            }
        }
    }
}