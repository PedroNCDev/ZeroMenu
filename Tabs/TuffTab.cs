using UnityEngine;
using ZeroMenu.Functions;
using Il2CppInterop.Runtime.InteropTypes.Arrays;

namespace ZeroMenu.Tabs
{
    public class TuffTab : ITab
    {
        public string Name => "Tuff";
        private Vector2 _scrollPosition = Vector2.zero;

        private static bool _doRejoin = false;
        private static bool _doSetPing = false;
        private static bool _doDisconnectAll = false;

        public void Draw()
        {
            _scrollPosition = SmoothScrollView.Begin(_scrollPosition, System.Array.Empty<GUILayoutOption>());
            GUILayout.BeginVertical(new Il2CppReferenceArray<GUILayoutOption>(new GUILayoutOption[] { GUILayout.Width((float)MainUI.windowWidth * 0.425f) }));

            Tuff.Enabled = CustomToggle.Draw(Tuff.Enabled, "Votekick Everyone", "TuffEnabled");
            Tuff.OnlyHost = CustomToggle.Draw(Tuff.OnlyHost, "Votekick Only Host", "TuffOnlyHost");
            Tuff.AlwaysRed = CustomToggle.Draw(Tuff.AlwaysRed, "Always Red", "TuffAlwaysRed");

            GUILayout.Space(15f);

            if (CustomButton.Draw("Rejoin Last Game"))
            {
                _doRejoin = true;
            }

            if (CustomButton.Draw("Set Ping"))
            {
                _doSetPing = true;
            }

            if (CustomButton.Draw("Disconnect All Players"))
            {
                _doDisconnectAll = true;
            }

            GUILayout.EndVertical();
            GUILayout.EndScrollView();
        }

        public static void Update()
        {
            if (_doRejoin)
            {
                Tuff.Rejoin();
                _doRejoin = false;
            }
            if (_doSetPing)
            {
                Tuff.SetPing();
                _doSetPing = false;
            }
            if (_doDisconnectAll)
            {
                Tuff.DisconnectAllPlayers();
                _doDisconnectAll = false;
            }
        }
    }
}
