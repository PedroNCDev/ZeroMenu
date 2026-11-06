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
        private static bool _doCopyCode = false;
        private static bool _doForceHost = false;
        private static bool _doStopForceHost = false;

        public void Draw()
        {
            _scrollPosition = SmoothScrollView.Begin(_scrollPosition, System.Array.Empty<GUILayoutOption>());
            GUILayout.BeginVertical(new Il2CppReferenceArray<GUILayoutOption>(new GUILayoutOption[] { GUILayout.Width((float)MainUI.windowWidth * 0.425f) }));

            Tuff.Enabled = CustomToggle.Draw(Tuff.Enabled, "Votekick Everyone", "TuffEnabled");
            Tuff.OnlyHost = CustomToggle.Draw(Tuff.OnlyHost, "Votekick Only Host", "TuffOnlyHost");
            Tuff.AutoRejoinOnVotekick = CustomToggle.Draw(Tuff.AutoRejoinOnVotekick, "Auto-Rejoin on Votekick", "TuffAutoRejoin");
            Tuff.AlwaysRed = CustomToggle.Draw(Tuff.AlwaysRed, "Always Red", "TuffAlwaysRed");

            GUILayout.Space(15f);

            GUILayout.BeginHorizontal(new Il2CppReferenceArray<GUILayoutOption>(0));
            GUILayout.Label($"Delay of Force Host: {Tuff.ForceHostExitDelay:F2}s", MenuStyles.Label, new Il2CppReferenceArray<GUILayoutOption>(new GUILayoutOption[] { GUILayout.Width(180f) }));
            Tuff.ForceHostExitDelay = CustomSlider.DrawAuto(Tuff.ForceHostExitDelay, 0.05f, 1.5f, new Il2CppReferenceArray<GUILayoutOption>(new GUILayoutOption[] { GUILayout.Width(150f) }));
            GUILayout.EndHorizontal();

            if (CustomButton.Draw(Tuff.ForceHostRunning ? "Force Host" : "Force Host"))
            {
                if (Tuff.ForceHostRunning) _doStopForceHost = true;
                else _doForceHost = true;
            }

            if (CustomButton.Draw("Copy Last Game Code"))
            {
                _doCopyCode = true;
            }

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
            if (_doCopyCode)
            {
                Tuff.CopyLastGameCode();
                _doCopyCode = false;
            }
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
            if (_doForceHost)
            {
                Tuff.StartForceHost();
                _doForceHost = false;
            }
            if (_doStopForceHost)
            {
                Tuff.StopForceHost();
                _doStopForceHost = false;
            }
        }
    }
}
