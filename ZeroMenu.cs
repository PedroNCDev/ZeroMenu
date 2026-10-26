using BepInEx;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;
using UnityEngine;
using ZeroMenu.Functions;
using ZeroMenu.UI;

namespace ZeroMenu
{
    [BepInPlugin(PluginGuid, PluginName, PluginVersion)]
    public class Plugin : BasePlugin
    {
        public const string PluginGuid = "com.mies.zeromenu";
        public const string PluginName = "ZeroMenu";
        public const string PluginVersion = "1.0.4";

        public static MainUI menuUI;
        public static NotificationManager notifications;
        private Harmony _harmony;

        public override void Load()
        {
            Log.LogInfo("ZeroMenu cargado correctamente!");

            HostSettings.Initialize(Log);

            _harmony = new Harmony(PluginGuid);
            _harmony.PatchAll();

            menuUI = AddComponent<MainUI>();
            notifications = AddComponent<NotificationManager>();
        }

        public override bool Unload()
        {
            _harmony?.UnpatchSelf();
            return true;
        }
    }
}
