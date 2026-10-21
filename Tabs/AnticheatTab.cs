using UnityEngine;
using ZeroMenu.Functions;
using Il2CppInterop.Runtime.InteropTypes.Arrays;

namespace ZeroMenu.Tabs
{
    public class AnticheatTab : ITab
    {
        public string Name => "Anticheat";
        private Vector2 _scrollPosition = Vector2.zero;

        public void Draw()
        {
            _scrollPosition = SmoothScrollView.Begin(_scrollPosition, System.Array.Empty<GUILayoutOption>());
            GUILayout.BeginVertical(new Il2CppReferenceArray<GUILayoutOption>(new GUILayoutOption[] { GUILayout.Width((float)MainUI.windowWidth * 0.425f) }));

            Anticheat.Enabled = CustomToggle.Draw(Anticheat.Enabled, "Enable Anticheat", "ac_enabled");
            Anticheat.CheckSpoofedPlatforms = CustomToggle.Draw(Anticheat.CheckSpoofedPlatforms, "Flag Spoofed Platform Data", "ac_platforms");

            GUILayout.Space(10f);
            GUILayout.Label("RPCs that should be checked by the anticheat:", MenuStyles.BoldLabel, System.Array.Empty<GUILayoutOption>());
            foreach (var kvp in Anticheat.RpcHandlers)
            {
                kvp.Value.Enabled = CustomToggle.Draw(kvp.Value.Enabled, kvp.Key.ToString(), "ac_rpc_" + kvp.Key);
            }

            GUILayout.Space(10f);
            GUILayout.Label("When a cheater is detected:", MenuStyles.BoldLabel, System.Array.Empty<GUILayoutOption>());
            Anticheat.sendNotification = CustomToggle.Draw(Anticheat.sendNotification, "Send notification", "ac_notify");
            Anticheat.discardRpc = CustomToggle.Draw(Anticheat.discardRpc, "Discard RPC", "ac_discard");

            GUILayout.BeginHorizontal(new Il2CppReferenceArray<GUILayoutOption>(0));
            GUILayout.Label($"Punish: {Anticheat.punishment}", MenuStyles.Label, new Il2CppReferenceArray<GUILayoutOption>(new GUILayoutOption[] { GUILayout.Width(150f) }));
            float punishValue = CustomSlider.DrawAuto((float)Anticheat.punishment, 0, 3, new Il2CppReferenceArray<GUILayoutOption>(new GUILayoutOption[] { GUILayout.Width(150f) }));
            Anticheat.punishment = (Anticheat.Punishments)Mathf.RoundToInt(punishValue);
            GUILayout.EndHorizontal();

            GUILayout.EndVertical();
            GUILayout.EndScrollView();
        }
    }
}