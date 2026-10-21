using System;
using UnityEngine;
using ZeroMenu.Functions;
using Il2CppInterop.Runtime.InteropTypes.Arrays;

namespace ZeroMenu.Tabs
{
    public class ShipTab : ITab
    {
        public string Name => "Ship";
        private Vector2 _scrollPosition = Vector2.zero;

        public void Draw()
        {
            _scrollPosition = SmoothScrollView.Begin(_scrollPosition, System.Array.Empty<GUILayoutOption>());
            GUILayout.BeginVertical(new Il2CppReferenceArray<GUILayoutOption>(new GUILayoutOption[] { GUILayout.Width((float)MainUI.windowWidth * 0.425f) }));

            if (ShipStatus.Instance == null)
            {
                GUILayout.Label("You are not currently in a game.", MenuStyles.Label, System.Array.Empty<GUILayoutOption>());
            }

            Ship.UnfixableLights = CustomToggle.Draw(Ship.UnfixableLights, "Unfixable Lights", "UnfixableLights");
            Ship.CallMeeting = CustomToggle.Draw(Ship.CallMeeting, "Call Meeting", "CallMeeting");
            Ship.CloseMeeting = CustomToggle.Draw(Ship.CloseMeeting, "Close Meeting", "CloseMeeting");
            Ship.AutoReportBodies = CustomToggle.Draw(Ship.AutoReportBodies, "Auto-Report Dead Bodies", "AutoReportBodies");
            Ship.AutoOpenDoors = CustomToggle.Draw(Ship.AutoOpenDoors, "Auto-Open Doors On Use", "AutoOpenDoors");

            GUILayout.Space(10f);
            GUILayout.Label("Sabotage", MenuStyles.BoldLabel, System.Array.Empty<GUILayoutOption>());
            
            Ship.ReactorSab = CustomToggle.Draw(Ship.ReactorSab, "Reactor", "ReactorSab");
            Ship.OxygenSab = CustomToggle.Draw(Ship.OxygenSab, "Oxygen", "OxygenSab");
            Ship.LightsSab = CustomToggle.Draw(Ship.LightsSab, "Lights", "LightsSab");
            Ship.CommsSab = CustomToggle.Draw(Ship.CommsSab, "Comms", "CommsSab");
            Ship.MushMixupSab = CustomToggle.Draw(Ship.MushMixupSab, "Mushroom Mixup", "MushMixupSab");
            Ship.TriggerSpores = CustomToggle.Draw(Ship.TriggerSpores, "Trigger Spores", "TriggerSpores");

            GUILayout.Space(10f);
            GUILayout.Label("Vents", MenuStyles.BoldLabel, System.Array.Empty<GUILayoutOption>());
            
            Ship.UnlockVents = CustomToggle.Draw(Ship.UnlockVents, "Unlock Vents", "UnlockVents");
            Ship.KickVents = CustomToggle.Draw(Ship.KickVents, "Kick All From Vents", "KickVents");
            Ship.WalkInVents = CustomToggle.Draw(Ship.WalkInVents, "Walk In Vents", "WalkInVents");

            GUILayout.EndVertical();
            GUILayout.EndScrollView();
        }
    }
}