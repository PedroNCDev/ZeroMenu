using System;
using System.Reflection;
using System.Text;
using AmongUs.Data;
using AmongUs.Data.Player;
using Discord;
using HarmonyLib;
using Il2CppInterop.Runtime;
using Il2CppInterop.Runtime.InteropTypes;
using UnityEngine;

namespace ZeroMenu.Functions
{
    public static class Spoof
    {
        public static bool GuestMode = false;
        public static bool FriendCodeSpoof = false;
        public static string SpoofLevelStr = "1";
        public static string PlatformNameStr = "TESTNAME";
        public static string FriendCodeStr = "";
        
        public static bool ShouldSpoofVersion = false;
        public static int SpoofedVersion = Constants.GetBroadcastVersion();
        public static bool UseModdedProtocol = false;
        public static Platforms SpoofedPlatform = Constants.GetPlatformType();

        public static bool DiscordEnabled = false;
        public static string DiscordDetails = "Using Zero? ye really bro";
        public static string DiscordState = "Coca Cola Zero Prime";
        public static string DiscordLargeImage = "impostor";
        public static string DiscordLargeText = "ZeroMenu";

        public static void ApplyLevel()
        {
            if (string.IsNullOrEmpty(SpoofLevelStr)) return;
            if (!int.TryParse(SpoofLevelStr, out int parsedLevel)) return;
            if (parsedLevel < 1 || parsedLevel > 99999) return;

            uint internalLevel = (uint)(parsedLevel - 1);
            if (internalLevel == DataManager.Player.Stats.Level) return;

            DataManager.Player.Stats.Level = internalLevel;
            DataManager.Player.Save();

            if (PlayerControl.LocalPlayer != null)
            {
                try { PlayerControl.LocalPlayer.RpcSetLevel(internalLevel); } catch { }
            }
        }
    }

    public static class SpoofPatches
    {
        [HarmonyPatch(typeof(Constants), nameof(Constants.GetBroadcastVersion))]
        public static class SpoofVersion
        {
            public static bool Prefix(ref int __result)
            {
                if (!Spoof.ShouldSpoofVersion || !AmongUsClient.Instance || (int)AmongUsClient.Instance.NetworkMode != 1)
                    return true;

                __result = Spoof.SpoofedVersion;
                if (Spoof.UseModdedProtocol) __result += 25;
                return false;
            }
        }

        [HarmonyPatch(typeof(Constants), nameof(Constants.IsVersionModded))]
        public static class MarkVersionModded
        {
            public static bool Prefix(ref bool __result)
            {
                if (Spoof.ShouldSpoofVersion && Spoof.UseModdedProtocol)
                {
                    __result = true;
                    return false;
                }
                return true;
            }
        }

        [HarmonyPatch(typeof(PlatformSpecificData), nameof(PlatformSpecificData.Serialize))]
        public static class SpoofPlatform
        {
            public static void Prefix(PlatformSpecificData __instance)
            {
                __instance.Platform = Spoof.SpoofedPlatform;
                switch ((int)Spoof.SpoofedPlatform)
                {
                    case 4:
                        __instance.XboxPlatformId = 2584878536129841UL;
                        break;
                    case 8:
                        __instance.PlatformName = "Sus";
                        break;
                    case 9:
                        __instance.PlatformName = "Major Nelson";
                        __instance.XboxPlatformId = 2584878536129841UL;
                        break;
                    case 10:
                        __instance.PlatformName = "";
                        __instance.PsnPlatformId = 0UL;
                        break;
                    default:
                        __instance.PlatformName = Spoof.PlatformNameStr;
                        __instance.XboxPlatformId = 0UL;
                        __instance.PsnPlatformId = 0UL;
                        break;
                }
            }
        }
    }

    public static class SpoofGuestAccountPatches
    {
        private static bool _isGuestAccount = false;

        private static bool IsSpoofEnabled() => Spoof.GuestMode;

        private static void FakeSuccessfulLogin(EOSManager eosManager)
        {
            var player = DataManager.Player;
            if (player == null) return;
            var accountField = player.GetIl2CppType().GetField("account", Il2CppSystem.Reflection.BindingFlags.Instance | Il2CppSystem.Reflection.BindingFlags.Public | Il2CppSystem.Reflection.BindingFlags.NonPublic);
            if (accountField == null) return;
            var account = accountField.GetValue(player)?.TryCast<PlayerAccountData>();
            if (account == null) return;
            var loginField = account.GetIl2CppType().GetField("loginStatus", Il2CppSystem.Reflection.BindingFlags.Instance | Il2CppSystem.Reflection.BindingFlags.Public | Il2CppSystem.Reflection.BindingFlags.NonPublic);
            if (loginField == null) return;
            loginField.SetValue(account, (Il2CppSystem.Object)(int)EOSManager.AccountLoginStatus.LoggedIn);
        }

        [HarmonyPatch(typeof(EOSManager), nameof(EOSManager.BeginLoginFlowWithDeviceID))]
        public static class BeginLoginFlowWithDeviceID_Patch
        {
            public static bool Prefix(EOSManager __instance)
            {
                try { __instance.DeleteDeviceID(null); } catch { }
                if (!IsSpoofEnabled()) return true;
                __instance.StartTempAccountFlow();
                _isGuestAccount = true;
                __instance.CloseStartupWaitScreen();
                return false;
            }
        }

        [HarmonyPatch(typeof(EOSManager), nameof(EOSManager.LoginFromAccountTab))]
        public static class LoginFromAccountTab_Patch
        {
            public static void Postfix(EOSManager __instance)
            {
                if (IsSpoofEnabled()) FakeSuccessfulLogin(__instance);
            }
        }

        [HarmonyPatch(typeof(EOSManager), nameof(EOSManager.Update))]
        public static class Update_Patch
        {
            public static void Postfix(EOSManager __instance)
            {
                if (IsSpoofEnabled()) FakeSuccessfulLogin(__instance);
            }
        }

        [HarmonyPatch(typeof(EOSManager), nameof(EOSManager.IsFreechatAllowed))]
        public static class IsFreechatAllowed_Patch
        {
            public static bool Prefix(ref bool __result)
            {
                __result = !_isGuestAccount || IsInGame() || IsInLobby();
                return false;
            }
            private static bool IsInGame() => AmongUsClient.Instance != null && AmongUsClient.Instance.GameState == InnerNet.InnerNetClient.GameStates.Started;
            private static bool IsInLobby() => AmongUsClient.Instance != null && AmongUsClient.Instance.GameState == InnerNet.InnerNetClient.GameStates.Joined;
        }

        [HarmonyPatch(typeof(EOSManager), "get_ProductUserId")]
        public static class GetProductUserId_Patch
        {
            public static void Postfix(ref string __result)
            {
                if (IsSpoofEnabled() && !string.IsNullOrEmpty(Spoof.FriendCodeStr))
                    __result = Spoof.FriendCodeStr;
            }
        }
    }

    public static class SpoofFriendCodePatches
    {
        private static bool IsSpoofEnabled() => Spoof.FriendCodeSpoof;
        private static string GetFriendCode() => !string.IsNullOrEmpty(Spoof.FriendCodeStr) ? Spoof.FriendCodeStr : null;

        private static void SetUsernameEAU(EditAccountUsername eau)
        {
            if (eau == null || !IsSpoofEnabled()) return;
            string code = GetFriendCode();
            bool isFriendCodeValid = !string.IsNullOrEmpty(code) && !code.Contains(" ") && code.Length <= 10;
            var tmpText = eau.UsernameText;
            if (tmpText == null) return;
            string newFriendCode = isFriendCodeValid ? Normalize(code) : GenerateRandomString();
            tmpText.text = newFriendCode;
        }

        private static string Normalize(string input)
        {
            var sb = new StringBuilder(input.Length);
            foreach (var c in input)
            {
                if (sb.Length > 0 && sb[sb.Length - 1] == ' ') break;
                sb.Append(char.ToLowerInvariant(c));
            }
            return sb.ToString();
        }

        private static string GenerateRandomString()
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var rng = new System.Random();
            var sb = new StringBuilder(10);
            for (int i = 0; i < 10; i++) sb.Append(chars[rng.Next(chars.Length)]);
            return sb.ToString();
        }

        [HarmonyPatch(typeof(EditAccountUsername), nameof(EditAccountUsername.SaveUsername))]
        public static class SaveUsername_Patch
        {
            public static void Prefix(EditAccountUsername __instance)
            {
                try { SetUsernameEAU(__instance); } catch { }
            }
        }

        [HarmonyPatch(typeof(AccountManager), "get_FriendCode")]
        public static class SpoofFriendCodeGetter
        {
            public static bool Prepare() => typeof(AccountManager).GetMethod("get_FriendCode", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic) != null;
            public static void Postfix(ref string __result)
            {
                if (Spoof.GuestMode) __result = Spoof.FriendCodeStr;
            }
        }
    }

    public static class DiscordPatches
    {
        private static readonly BepInEx.Logging.ManualLogSource _log = BepInEx.Logging.Logger.CreateLogSource("ZeroMenu.Discord");

        [HarmonyPatch(typeof(ActivityManager), "UpdateActivity")]
        public static class ActivityManager_UpdateActivity
        {
            public static void Prefix(Activity activity) { UpdateDiscord(activity); }
            public static void Postfix(Activity activity) { UpdateDiscord(activity); }

            private static void UpdateDiscord(Activity activity)
            {
                if (activity == null || !Spoof.DiscordEnabled) return;
                try
                {
                    activity.Details = Spoof.DiscordDetails;
                    activity.State = Spoof.DiscordState;
                    if (activity.Assets == null) activity.Assets = new ActivityAssets();
                    activity.Assets.LargeImage = Spoof.DiscordLargeImage;
                    activity.Assets.LargeText = Spoof.DiscordLargeText;
                }
                catch (Exception ex) { _log.LogWarning("[Discord] " + ex.Message); }
            }
        }
    }
}