using UnityEngine;
using Il2CppSystem.Collections.Generic;

namespace ZeroMenu.Utilities
{
    public static class PlayerPickMenu
    {
        public static ShapeshifterMinigame playerpickMenu;
        public static bool isActive;
        public static NetworkedPlayerInfo targetPlayerData;
        public static Il2CppSystem.Action customAction;
        public static List<NetworkedPlayerInfo> customPlayerList;

        public static RoleBehaviour GetBehaviourByRoleType(AmongUs.GameOptions.RoleTypes roleType)
        {
            if (RoleManager.Instance == null || RoleManager.Instance.AllRoles == null) return null;
            
            foreach (var role in RoleManager.Instance.AllRoles)
            {
                if (role.Role == roleType)
                    return role;
            }
            return null;
        }

        public static ShapeshifterMinigame GetShapeshifterMenu()
        {
            var rolePrefab = GetBehaviourByRoleType(AmongUs.GameOptions.RoleTypes.Shapeshifter);
            if (rolePrefab == null) return null;
            return Object.Instantiate(rolePrefab.Cast<ShapeshifterRole>(), GameData.Instance.transform).ShapeshifterMenu;
        }

        public static void OpenPlayerPickMenu(List<NetworkedPlayerInfo> playerList, Il2CppSystem.Action action)
        {
            isActive = true;
            customPlayerList = playerList;
            customAction = action;

            var menu = GetShapeshifterMenu();
            if (menu == null) return;

            playerpickMenu = Object.Instantiate(menu, Camera.main.transform, false);

            playerpickMenu.transform.localPosition = new Vector3(0f, 0f, -50f);
            playerpickMenu.Begin(null);
        }

        public static NetworkedPlayerInfo CustomPPMChoice(string name, NetworkedPlayerInfo.PlayerOutfit outfit, RoleBehaviour role = null)
        {
            NetworkedPlayerInfo customChoice = Object.Instantiate<NetworkedPlayerInfo>(GameData.Instance.PlayerInfoPrefab);

            outfit.PlayerName = name;

            customChoice.Outfits[PlayerOutfitType.Default] = outfit;

            if (role != null)
            {
                customChoice.Role = role;
            }

            return customChoice;
        }
    }
}
