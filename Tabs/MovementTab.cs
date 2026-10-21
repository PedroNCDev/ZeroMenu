using System;
using System.Collections.Generic;
using UnityEngine;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using ZeroMenu.Functions;

namespace ZeroMenu.Tabs
{
    public class MovementTab : ITab
    {
        public string Name => "Movement";
        private Vector2 _scrollPosition = Vector2.zero;

        public void Draw()
        {
            _scrollPosition = SmoothScrollView.Begin(_scrollPosition, System.Array.Empty<GUILayoutOption>());
            GUILayout.BeginVertical(new GUILayoutOption[] { GUILayout.Width((float)MainUI.windowWidth * 0.425f) });

            if (PlayerControl.LocalPlayer == null || PlayerControl.LocalPlayer.Data == null)
            {
                GUILayout.Label("You are not currently in a game, these options will not work.", MenuStyles.Label, System.Array.Empty<GUILayoutOption>());
                
                Movement.NoClip = CustomToggle.Draw(Movement.NoClip, "Noclip", "NoClip");
            }
            else
            {
                Vector2 pos = PlayerControl.LocalPlayer.transform.position;
                GUILayout.Label($"Current Position:\nX: {pos.x:F2}\nY: {pos.y:F2}", MenuStyles.Label, System.Array.Empty<GUILayoutOption>());

                Movement.NoClip = CustomToggle.Draw(Movement.NoClip, "Noclip", "NoClip");
            }

            Movement.InvertControls = CustomToggle.Draw(Movement.InvertControls, "Invert Controls", "InvertControls");

            GUILayout.Space(10f);
            GUILayout.Label($"Speed Modifier: {Movement.SpeedMultiplier:F2}x", MenuStyles.Label, System.Array.Empty<GUILayoutOption>());
            Movement.SpeedMultiplier = CustomSlider.DrawAuto(Movement.SpeedMultiplier, 0f, 5f, new Il2CppReferenceArray<GUILayoutOption>(new GUILayoutOption[] { GUILayout.Width(250f) }));

            GUILayout.Space(10f);
            GUILayout.Label("Teleport", MenuStyles.BoldLabel, System.Array.Empty<GUILayoutOption>());

            Movement.TeleportCursor = CustomToggle.Draw(Movement.TeleportCursor, "To Cursor (Right-Click)", "TeleportCursor");
            Movement.UseSnapToRpc = CustomToggle.Draw(Movement.UseSnapToRpc, "Use SnapTo RPC", "UseSnapToRpc");

            GUILayout.Space(5f);
            GUILayout.Label("Teleport To Location:", MenuStyles.Label, System.Array.Empty<GUILayoutOption>());

            foreach (var location in Movement.TeleportLocations)
            {
                if (CustomButton.Draw(location.Key))
                {
                    Movement.TeleportTo(location.Value);
                    ZeroMenu.Plugin.notifications.Send("Movement", $"Teleported to {location.Key}.", 3f);
                }
            }

            GUILayout.EndVertical();
            GUILayout.EndScrollView();
        }
    }
}