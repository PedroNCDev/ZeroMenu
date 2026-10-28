using UnityEngine;
using AmongUs.GameOptions;
using Il2CppSystem.Collections.Generic;
using InnerNet;
using ZeroMenu.Functions;

namespace ZeroMenu.Utilities
{
    public static class RolesPPM
    {
        public static void SetFakeRolePPM()
        {
            if (PlayerPickMenu.playerpickMenu != null)
                PlayerPickMenu.playerpickMenu.Close();

            List<NetworkedPlayerInfo> playerDataList = new List<NetworkedPlayerInfo>();

            RoleTypes oldRole = PlayerControl.LocalPlayer.Data.RoleType;
            bool isDead = PlayerControl.LocalPlayer.Data.IsDead;
            bool isHost = AmongUsClient.Instance.AmHost;

            if (!isDead)
            {
                if (oldRole == RoleTypes.Shapeshifter || Utils.IsFreePlay || isHost)
                    playerDataList.Add(PlayerPickMenu.CustomPPMChoice("Shapeshifter", OutfitPreset.Shapeshifter, PlayerPickMenu.GetBehaviourByRoleType(RoleTypes.Shapeshifter)));

                if (oldRole == RoleTypes.Phantom || Utils.IsFreePlay || isHost)
                    playerDataList.Add(PlayerPickMenu.CustomPPMChoice("Phantom", OutfitPreset.Phantom, PlayerPickMenu.GetBehaviourByRoleType(RoleTypes.Phantom)));

                if (oldRole == RoleTypes.Viper || Utils.IsFreePlay || isHost)
                {
                    playerDataList.Add(PlayerPickMenu.CustomPPMChoice("Viper", OutfitPreset.Viper, PlayerPickMenu.GetBehaviourByRoleType(RoleTypes.Viper)));
                    playerDataList.Add(PlayerPickMenu.CustomPPMChoice("Vipershifter", OutfitPreset.Vipershifter, PlayerPickMenu.GetBehaviourByRoleType(RoleTypes.Viper)));
                }

                if (PlayerControl.LocalPlayer.Data.Role.TeamType == RoleTeamTypes.Impostor || Utils.IsFreePlay || isHost)
                    playerDataList.Add(PlayerPickMenu.CustomPPMChoice("Impostor", OutfitPreset.Impostor, PlayerPickMenu.GetBehaviourByRoleType(RoleTypes.Impostor)));

                if (oldRole == RoleTypes.Judge || Utils.IsFreePlay || isHost)
                    playerDataList.Add(PlayerPickMenu.CustomPPMChoice("Judge", OutfitPreset.Judge, PlayerPickMenu.GetBehaviourByRoleType(RoleTypes.Judge)));

                playerDataList.Add(PlayerPickMenu.CustomPPMChoice("Tracker", OutfitPreset.Tracker, PlayerPickMenu.GetBehaviourByRoleType(RoleTypes.Tracker)));
                playerDataList.Add(PlayerPickMenu.CustomPPMChoice("Noisemaker", OutfitPreset.Noisemaker, PlayerPickMenu.GetBehaviourByRoleType(RoleTypes.Noisemaker)));
                playerDataList.Add(PlayerPickMenu.CustomPPMChoice("Engineer", OutfitPreset.Engineer, PlayerPickMenu.GetBehaviourByRoleType(RoleTypes.Engineer)));
                playerDataList.Add(PlayerPickMenu.CustomPPMChoice("Scientist", OutfitPreset.Scientist, PlayerPickMenu.GetBehaviourByRoleType(RoleTypes.Scientist)));
                playerDataList.Add(PlayerPickMenu.CustomPPMChoice("Detective", OutfitPreset.Detective, PlayerPickMenu.GetBehaviourByRoleType(RoleTypes.Detective)));
                playerDataList.Add(PlayerPickMenu.CustomPPMChoice("Crewmate", OutfitPreset.Crewmate, PlayerPickMenu.GetBehaviourByRoleType(RoleTypes.Crewmate)));
            }
            else
            {
                if (oldRole == RoleTypes.GuardianAngel || Utils.IsFreePlay || isHost)
                    playerDataList.Add(PlayerPickMenu.CustomPPMChoice("Guardian Angel", OutfitPreset.GuardianAngel, PlayerPickMenu.GetBehaviourByRoleType(RoleTypes.GuardianAngel)));

                if (oldRole == RoleTypes.SpiritGuide || Utils.IsFreePlay || isHost)
                    playerDataList.Add(PlayerPickMenu.CustomPPMChoice("Influencer", OutfitPreset.SpiritGuide, PlayerPickMenu.GetBehaviourByRoleType(RoleTypes.SpiritGuide)));
            }

            PlayerPickMenu.OpenPlayerPickMenu(playerDataList, (Il2CppSystem.Action)(() =>
            {
                var chosenName = PlayerPickMenu.targetPlayerData.DefaultOutfit.PlayerName;

                if (isHost)
                {
                    if (PlayerControl.LocalPlayer.Data.IsDead)
                    {
                        if (PlayerPickMenu.targetPlayerData.Role.Role == RoleTypes.GuardianAngel || PlayerPickMenu.targetPlayerData.Role.Role == RoleTypes.SpiritGuide)
                            PlayerControl.LocalPlayer.RpcSetRole(PlayerPickMenu.targetPlayerData.Role.Role, true);
                        else
                            PlayerControl.LocalPlayer.RpcSetRole(PlayerPickMenu.targetPlayerData.Role.TeamType == RoleTeamTypes.Impostor ? RoleTypes.ImpostorGhost : RoleTypes.CrewmateGhost, true);
                    }
                    else
                    {
                        PlayerControl.LocalPlayer.RpcSetRole(PlayerPickMenu.targetPlayerData.Role.Role, true);
                    }
                }
                else
                {
                    if (PlayerControl.LocalPlayer.Data.IsDead)
                    {
                        if (PlayerPickMenu.targetPlayerData.Role.Role == RoleTypes.GuardianAngel || PlayerPickMenu.targetPlayerData.Role.Role == RoleTypes.SpiritGuide)
                            RoleManager.Instance.SetRole(PlayerControl.LocalPlayer, PlayerPickMenu.targetPlayerData.Role.Role);
                        else
                            RoleManager.Instance.SetRole(PlayerControl.LocalPlayer, PlayerPickMenu.targetPlayerData.Role.Role == RoleTypes.Impostor ? RoleTypes.ImpostorGhost : RoleTypes.CrewmateGhost);
                    }
                    else
                    {
                        RoleManager.Instance.SetRole(PlayerControl.LocalPlayer, PlayerPickMenu.targetPlayerData.Role.Role);
                    }
                }

                if (chosenName == "Vipershifter")
                {
                    Vipershifter.Enabled = true;
                    Vipershifter.LoadSprites();
                    ZeroMenu.Plugin.notifications.Send("Vipershifter", "yes", 3f);
                }
                else
                {
                    Vipershifter.Enabled = false;
                }
            }));
        }

        public static void SetFakeAlivePPM()
        {
            if (PlayerPickMenu.playerpickMenu != null)
                PlayerPickMenu.playerpickMenu.Close();

            List<NetworkedPlayerInfo> playerDataList = new List<NetworkedPlayerInfo>();
            playerDataList.Add(PlayerPickMenu.CustomPPMChoice("Alive", OutfitPreset.Crewmate, PlayerPickMenu.GetBehaviourByRoleType(RoleTypes.Crewmate)));
            playerDataList.Add(PlayerPickMenu.CustomPPMChoice("Dead", OutfitPreset.Dead, PlayerPickMenu.GetBehaviourByRoleType(RoleTypes.CrewmateGhost)));

            PlayerPickMenu.OpenPlayerPickMenu(playerDataList, (Il2CppSystem.Action)(() =>
            {
                if (PlayerPickMenu.targetPlayerData.Role.IsDead)
                {
                    PlayerControl.LocalPlayer.Die(DeathReason.Exile, true);
                }
                else
                {
                    PlayerControl.LocalPlayer.Revive();
                }
            }));
        }
    }
}
