using UnityEngine;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using ZeroMenu.Functions;

namespace ZeroMenu.Tabs
{
    public class SelfTab : ITab
    {
        public string Name => "Self";
        private Vector2 _scrollPosition = Vector2.zero;

        public void Draw()
        {
            _scrollPosition = SmoothScrollView.Begin(_scrollPosition, System.Array.Empty<GUILayoutOption>());
            GUILayout.BeginVertical(new Il2CppReferenceArray<GUILayoutOption>(new GUILayoutOption[] { GUILayout.Width(500f) }));

            if (PlayerControl.LocalPlayer == null || PlayerControl.LocalPlayer.Data == null)
            {
                GUILayout.Label("You are not currently in a game, these options will not work.", MenuStyles.Label, new Il2CppReferenceArray<GUILayoutOption>(0));
            }
            else
            {
                GUILayout.Label("Role: " + PlayerControl.LocalPlayer.Data.RoleType.ToString(), MenuStyles.BoldLabel, new Il2CppReferenceArray<GUILayoutOption>(0));
            }

            Self.UpdateStatsFreeplay = CustomToggle.Draw(Self.UpdateStatsFreeplay, "Update Stats in Freeplay", "UpdateStatsFreeplay");
            Self.Immortality = CustomToggle.Draw(Self.Immortality, "Become Immortal", "Immortality");
            
            Self.Invisibility = CustomToggle.Draw(Self.Invisibility, "Become Phantom", "Invisibility");
            SelfInvisibility.InvisibleVisualCheat();

            Self.ShowCustomizationUI = CustomToggle.Draw(Self.ShowCustomizationUI, "Show Player Customization", "ShowCustomizationUI");

            Self.FakeLag = CustomToggle.Draw(Self.FakeLag, "Fake Lag", "FakeLag");
            Self.AlwaysShowTaskAnimations = CustomToggle.Draw(Self.AlwaysShowTaskAnimations, "Always Show Task Animations", "AlwaysShowTaskAnimations");
            Self.NoLadderCooldown = CustomToggle.Draw(Self.NoLadderCooldown, "No Ladder Cooldown", "NoLadderCooldown");
            Self.UnlimitedMeetings = CustomToggle.Draw(Self.UnlimitedMeetings, "Unlimited Meetings", "UnlimitedMeetings");

            GUILayout.Space(10f);

            if (CustomButton.Draw("Call Meeting"))
            {
                Self.CallMeeting();
            }

            if (CustomButton.Draw("Complete All Tasks"))
            {
                Self.StartCompleteAllTasks();
            }

            GUILayout.Space(10f);
            GUILayout.Label("Avatar Controls:", MenuStyles.BoldLabel, new Il2CppReferenceArray<GUILayoutOption>(0));
            
            Self.SnipeColor = CustomToggle.Draw(Self.SnipeColor, "Snipe Color", "SnipeColor");

            GUILayout.BeginHorizontal(new Il2CppReferenceArray<GUILayoutOption>(0));
            GUILayout.Label("Target Color ID: " + Self.TargetColorId, MenuStyles.Label, new Il2CppReferenceArray<GUILayoutOption>(new GUILayoutOption[] { GUILayout.Width(130f) }));
            float colorFloat = CustomSlider.DrawAuto(Self.TargetColorId, 0f, 17f, new Il2CppReferenceArray<GUILayoutOption>(new GUILayoutOption[] { GUILayout.Width(200f) }));
            Self.TargetColorId = (byte)Mathf.RoundToInt(colorFloat);
            GUILayout.EndHorizontal();

            GUILayout.Space(5f);

            if (CustomButton.Draw("Restore Avatar"))
            {
                Self.RestoreAvatar();
            }

            GUILayout.EndVertical();
            GUILayout.EndScrollView();
        }
    }
}