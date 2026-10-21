using UnityEngine;
using ZeroMenu.Functions;
using Il2CppInterop.Runtime.InteropTypes.Arrays;

namespace ZeroMenu.Tabs
{
    public class PassiveTab : ITab
    {
        public string Name => "Passive";
        private Vector2 _scrollPosition = Vector2.zero;

        public void Draw()
        {
            _scrollPosition = SmoothScrollView.Begin(_scrollPosition, System.Array.Empty<GUILayoutOption>());
            GUILayout.BeginVertical(new Il2CppReferenceArray<GUILayoutOption>(new GUILayoutOption[] { GUILayout.Width((float)MainUI.windowWidth * 0.425f) }));

            GUILayout.Label("Account & Cosmetics", MenuStyles.BoldLabel, System.Array.Empty<GUILayoutOption>());
            Passive.FreeCosmetics = CustomToggle.Draw(Passive.FreeCosmetics, "Free Cosmetics", "freeCosmetics");
            Passive.AvoidPenalties = CustomToggle.Draw(Passive.AvoidPenalties, "Avoid Penalties", "avoidPenalties");
            Passive.UnlockFeatures = CustomToggle.Draw(Passive.UnlockFeatures, "Unlock Extra Features", "unlockFeatures");

            GUILayout.Space(15f);
            GUILayout.Label("Privacy & Spoofing", MenuStyles.BoldLabel, System.Array.Empty<GUILayoutOption>());
            Passive.SpoofDeviceId = CustomToggle.Draw(Passive.SpoofDeviceId, "Spoof Device ID", "spoofDeviceId");
            Passive.SpoofAprilFoolsDate = CustomToggle.Draw(Passive.SpoofAprilFoolsDate, "Spoof Date to April 1st", "spoofAprilFoolsDate");

            GUILayout.Space(15f);
            GUILayout.Label("Misc", MenuStyles.BoldLabel, System.Array.Empty<GUILayoutOption>());
            Passive.CopyLobbyCodeOnDisconnect = CustomToggle.Draw(Passive.CopyLobbyCodeOnDisconnect, "Copy Lobby Code on Disconnect", "copyLobbyCodeOnDisconnect");

            GUILayout.EndVertical();
            GUILayout.EndScrollView();
        }
    }
}