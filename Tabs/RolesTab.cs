using System;
using UnityEngine;
using ZeroMenu.Functions;
using ZeroMenu.Utilities;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using AmongUs.GameOptions;

namespace ZeroMenu.Tabs
{
    public class RolesTab : ITab
    {
        public string Name => "Roles";
        private Vector2 _scrollPosition = Vector2.zero;

        public void Draw()
        {
            _scrollPosition = SmoothScrollView.Begin(_scrollPosition, System.Array.Empty<GUILayoutOption>());
            GUILayout.BeginVertical(new GUILayoutOption[] { GUILayout.Width((float)MainUI.windowWidth * 0.425f) });

            if (PlayerControl.LocalPlayer == null || PlayerControl.LocalPlayer.Data == null)
            {
                GUILayout.Label("You are not currently in a game.", MenuStyles.Label, System.Array.Empty<GUILayoutOption>());
            }
            else
            {
                GUILayout.Label("Player Status", MenuStyles.BoldLabel, System.Array.Empty<GUILayoutOption>());
                
                if (AmongUsClient.Instance.AmHost)
                {
                    if (CustomButton.Draw("Set Role")) 
                    {
                        RolesPPM.SetFakeRolePPM();
                    }
                }
                else
                {
                    if (CustomButton.Draw("Set Role (visual)")) 
                    {
                        RolesPPM.SetFakeRolePPM();
                    }
                }

                if (CustomButton.Draw("Set Alive (visual)")) 
                {
                    RolesPPM.SetFakeAlivePPM();
                }

                GUILayout.Space(10f);
            }

            GUILayout.Label("Impostor", MenuStyles.BoldLabel, System.Array.Empty<GUILayoutOption>());
            Roles.KillReach = CustomToggle.Draw(Roles.KillReach, "Kill Reach", "KillReach");
            Roles.KillAnyone = CustomToggle.Draw(Roles.KillAnyone, "Kill Anyone", "KillAnyone");
            Roles.KillOtherImpostors = CustomToggle.Draw(Roles.KillOtherImpostors, "Kill Other Impostors", "KillOtherImpostors");
            Roles.KillVanished = CustomToggle.Draw(Roles.KillVanished, "Kill While Vanished", "KillVanished");

            GUILayout.Space(10f);
            GUILayout.Label("Shapeshifter", MenuStyles.BoldLabel, System.Array.Empty<GUILayoutOption>());
            Roles.NoSsAnim = CustomToggle.Draw(Roles.NoSsAnim, "No Shapeshift Anim", "NoSsAnim");
            Roles.EndlessSsDuration = CustomToggle.Draw(Roles.EndlessSsDuration, "Endless Shift Duration", "EndlessSsDuration");

            GUILayout.Space(10f);
            GUILayout.Label("Engineer", MenuStyles.BoldLabel, System.Array.Empty<GUILayoutOption>());
            Roles.EndlessVentTime = CustomToggle.Draw(Roles.EndlessVentTime, "Endless Vent Time", "EndlessVentTime");
            Roles.NoVentCooldown = CustomToggle.Draw(Roles.NoVentCooldown, "No Vent Cooldown", "NoVentCooldown");

            GUILayout.Space(10f);
            GUILayout.Label("Scientist", MenuStyles.BoldLabel, System.Array.Empty<GUILayoutOption>());
            Roles.EndlessBattery = CustomToggle.Draw(Roles.EndlessBattery, "Endless Battery", "EndlessBattery");
            Roles.NoVitalsCooldown = CustomToggle.Draw(Roles.NoVitalsCooldown, "No Vitals Cooldown", "NoVitalsCooldown");

            GUILayout.Space(10f);
            GUILayout.Label("Tracker", MenuStyles.BoldLabel, System.Array.Empty<GUILayoutOption>());
            Roles.TrackReach = CustomToggle.Draw(Roles.TrackReach, "Track Reach", "TrackReach");
            Roles.EndlessTracking = CustomToggle.Draw(Roles.EndlessTracking, "Endless Tracking", "EndlessTracking");
            Roles.NoTrackDelay = CustomToggle.Draw(Roles.NoTrackDelay, "No Track Delay", "NoTrackDelay");
            Roles.NoTrackCooldown = CustomToggle.Draw(Roles.NoTrackCooldown, "No Track Cooldown", "NoTrackCooldown");

            GUILayout.Space(10f);
            GUILayout.Label("Phantom", MenuStyles.BoldLabel, System.Array.Empty<GUILayoutOption>());
            Roles.NoVanishCooldown = CustomToggle.Draw(Roles.NoVanishCooldown, "No Vanish Cooldown", "NoVanishCooldown");
            Roles.EndlessVanishDuration = CustomToggle.Draw(Roles.EndlessVanishDuration, "Endless Vanish Time", "EndlessVanishDuration");

            GUILayout.Space(10f);
            GUILayout.Label("Detective", MenuStyles.BoldLabel, System.Array.Empty<GUILayoutOption>());
            Roles.InterrogateReach = CustomToggle.Draw(Roles.InterrogateReach, "Interrogate Reach", "InterrogateReach");

            GUILayout.Space(10f);
            GUILayout.Label("Guardian Angel", MenuStyles.BoldLabel, System.Array.Empty<GUILayoutOption>());
            Roles.GaInfiniteRange = CustomToggle.Draw(Roles.GaInfiniteRange, "Infinite Protect Range", "GaInfiniteRange");

            GUILayout.Space(10f);
            GUILayout.Label("Judge", MenuStyles.BoldLabel, System.Array.Empty<GUILayoutOption>());
            Roles.JudgeInstantUnlock = CustomToggle.Draw(Roles.JudgeInstantUnlock, "Instant Overrule Unlock", "JudgeInstantUnlock");
            Roles.JudgeInfiniteGavels = CustomToggle.Draw(Roles.JudgeInfiniteGavels, "Infinite Gavels", "JudgeInfiniteGavels");

            GUILayout.Space(10f);
            GUILayout.Label("Influencer", MenuStyles.BoldLabel, System.Array.Empty<GUILayoutOption>());
            Roles.NoSpiritGuideCooldown = CustomToggle.Draw(Roles.NoSpiritGuideCooldown, "No Refresh Cooldown", "NoSpiritGuideCooldown");

            GUILayout.EndVertical();
            GUILayout.EndScrollView();
        }
    }
}
