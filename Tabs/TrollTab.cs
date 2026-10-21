using UnityEngine;
using ZeroMenu.Functions;
using Il2CppInterop.Runtime.InteropTypes.Arrays;

namespace ZeroMenu.Tabs
{
    public class TrollTab : ITab
    {
        public string Name => "Troll";
        private Vector2 _scrollPosition = Vector2.zero;
        private static int _selectedVent = 0;

        public void Draw()
        {
            _scrollPosition = SmoothScrollView.Begin(_scrollPosition, System.Array.Empty<GUILayoutOption>());
            GUILayout.BeginVertical(new Il2CppReferenceArray<GUILayoutOption>(new GUILayoutOption[] { GUILayout.Width((float)MainUI.windowWidth * 0.425f) }));

            if (PlayerControl.LocalPlayer == null || PlayerControl.LocalPlayer.Data == null)
            {
                GUILayout.Label("You are not currently in a game, these options will not work.", MenuStyles.Label, new Il2CppReferenceArray<GUILayoutOption>(0));
            }

            Troll.AutoReportBodies = CustomToggle.Draw(Troll.AutoReportBodies, "Automatically Report Bodies", "AutoReportBodies");
            Troll.CrashLobby = CustomToggle.Draw(Troll.CrashLobby, "Queue Lobby Crash", "CrashLobby");
            Troll.DisableVents = CustomToggle.Draw(Troll.DisableVents, "Disable Vents", "DisableVents");
            Troll.BlockSabotages = CustomToggle.Draw(Troll.BlockSabotages, "Block Sabotages", "BlockSabotages");
            Troll.DoorTrollerEnabled = CustomToggle.Draw(Troll.DoorTrollerEnabled, "Door Troller", "DoorTroller");
            Troll.AutoExposeEnabled = CustomToggle.Draw(Troll.AutoExposeEnabled, "Auto Expose Impostors", "AutoExpose");

            if (Troll.DoorTrollerEnabled)
            {
                GUILayout.BeginHorizontal(new Il2CppReferenceArray<GUILayoutOption>(0));
                GUILayout.Label($"Delay: {Troll.DoorTrollerDelay:F2}s", MenuStyles.Label, new Il2CppReferenceArray<GUILayoutOption>(new GUILayoutOption[] { GUILayout.Width(100f) }));
                Troll.DoorTrollerDelay = CustomSlider.DrawAuto(Troll.DoorTrollerDelay, 0.1f, 2f, new Il2CppReferenceArray<GUILayoutOption>(new GUILayoutOption[] { GUILayout.Width(150f) }));
                GUILayout.EndHorizontal();
            }

            if (Troll.AutoExposeEnabled)
            {
                Troll.ExposeOnMurder = CustomToggle.Draw(Troll.ExposeOnMurder, "Expose On Murder", "ExposeOnMurder");
                Troll.ExposeOnShapeshift = CustomToggle.Draw(Troll.ExposeOnShapeshift, "Expose On Shapeshift", "ExposeOnShapeshift");
                Troll.ExposeOnPhantom = CustomToggle.Draw(Troll.ExposeOnPhantom, "Expose On Phantom", "ExposeOnPhantom");
            }

            GUILayout.Space(15f);
            GUILayout.Label("Vent Teleport", MenuStyles.BoldLabel, System.Array.Empty<GUILayoutOption>());

            if (ShipStatus.Instance != null)
            {
                int maxVents = ShipStatus.Instance.AllVents.Count - 1;
                if (_selectedVent > maxVents) _selectedVent = maxVents;
                if (_selectedVent < 0) _selectedVent = 0;

                GUILayout.Label($"Target Vent: {Utils.GetVentName(_selectedVent)}", MenuStyles.Label, new Il2CppReferenceArray<GUILayoutOption>(0));
                _selectedVent = (int)CustomSlider.DrawAuto(_selectedVent, 0f, maxVents, new Il2CppReferenceArray<GUILayoutOption>(new GUILayoutOption[] { GUILayout.Width(200f) }));
                
                if (CustomButton.Draw("Teleport Everyone To Vent")) Troll.TeleportAllToVent(_selectedVent);
                if (CustomButton.Draw("Teleport Everyone To Random Vent")) Troll.TeleportAllToRandomVent();
            }

            GUILayout.Space(15f);
            if (CustomButton.Draw("Disconnect All (non-host)")) Troll.DisconnectAllNonHost();
            if (CustomButton.Draw("Trigger All Spores")) Troll.TriggerAllSpores();
            if (CustomButton.Draw("Copy Random Player")) Troll.CopyRandomPlayer();
            if (CustomButton.Draw("Disable Killing")) Troll.DisableKilling();
            if (CustomButton.Draw("Deplete HnS Timer")) Troll.DepleteHnSTimer();

            GUILayout.EndVertical();
            GUILayout.EndScrollView();
        }
    }
}