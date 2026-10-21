using System;
using UnityEngine;
using ZeroMenu.Functions;
using Il2CppInterop.Runtime.InteropTypes.Arrays;

namespace ZeroMenu.Tabs
{
    public class SabotageTab : ITab
    {
        public string Name => "Sabotage";
        private Vector2 _scrollPosition = Vector2.zero;

        public void Draw()
        {
            _scrollPosition = SmoothScrollView.Begin(_scrollPosition, System.Array.Empty<GUILayoutOption>());
            GUILayout.BeginVertical(new Il2CppReferenceArray<GUILayoutOption>(new GUILayoutOption[] { GUILayout.Width(500f) }));

            if (ShipStatus.Instance == null)
            {
                GUILayout.Label("You are not currently in a game, or the game has not started yet.", MenuStyles.Label, System.Array.Empty<GUILayoutOption>());
            }

            Sabotage.UpdateSystemsDirectly = CustomToggle.Draw(Sabotage.UpdateSystemsDirectly, "Update Sabotage Systems Directly", "UpdateSystems");

            var sabotages = Sabotage.GetSabotages();
            var doors = Sabotage.GetDoors();

            GUILayout.Space(10f);

            GUILayout.BeginHorizontal(System.Array.Empty<GUILayoutOption>());
            if (CustomButton.Draw("Sabotage All")) 
            {
                Sabotage.SabotageAll();
                ZeroMenu.Plugin.notifications.Send("Sabotage", "All sabotages enabled.", 3f);
            }
            if (CustomButton.Draw("Close All Doors")) 
            {
                Sabotage.LockAll();
                ZeroMenu.Plugin.notifications.Send("Sabotage", "All doors closed.", 3f);
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal(System.Array.Empty<GUILayoutOption>());
            if (CustomButton.Draw("Fix All Sabotages")) 
            {
                Sabotage.FixAllSabotages();
                ZeroMenu.Plugin.notifications.Send("Sabotage", "All sabotages repaired.", 3f);
            }
            if (CustomButton.Draw("Unlock All Doors")) 
            {
                if (Sabotage.CanUnlockDoors()) 
                {
                    Sabotage.UnlockAll();
                    ZeroMenu.Plugin.notifications.Send("Sabotage", "All doors unlocked.", 3f);
                }
                else 
                {
                    ZeroMenu.Plugin.notifications.Send("Sabotage", "Map does not support unlocking doors.", 3f);
                }
            }
            GUILayout.EndHorizontal();

            GUILayout.Space(10f);
            GUILayout.Label("Sabotages:", MenuStyles.BoldLabel, System.Array.Empty<GUILayoutOption>());

            foreach (var kvp in sabotages)
            {
                GUILayout.BeginHorizontal(System.Array.Empty<GUILayoutOption>());
                
                if (CustomButton.Draw($"Sabotage {kvp.Key}"))
                {
                    Sabotage.SabotageSystem(kvp.Value);
                    ZeroMenu.Plugin.notifications.Send("Sabotage", $"{kvp.Key} sabotaged.", 3f);
                }
                
                if (CustomButton.Draw($"Fix {kvp.Key}"))
                {
                    Sabotage.FixSabotage(kvp.Value);
                    ZeroMenu.Plugin.notifications.Send("Sabotage", $"{kvp.Key} fixed.", 3f);
                }
                
                GUILayout.EndHorizontal();
            }

            GUILayout.Space(10f);
            GUILayout.Label("Close Doors:", MenuStyles.BoldLabel, System.Array.Empty<GUILayoutOption>());

            if (doors.Count == 0)
            {
                GUILayout.Label("This map has no doors that can be closed.", MenuStyles.Label, System.Array.Empty<GUILayoutOption>());
            }
            else
            {
                foreach (var kvp in doors)
                {
                    GUILayout.BeginHorizontal(System.Array.Empty<GUILayoutOption>());
                    
                    if (CustomButton.Draw($"Close {kvp.Key}"))
                    {
                        Sabotage.LockDoor(kvp.Value);
                    }
                    
                    if (CustomButton.Draw($"Open {kvp.Key}"))
                    {
                        if (Sabotage.CanUnlockDoors()) Sabotage.UnlockDoor(kvp.Value);
                        else ZeroMenu.Plugin.notifications.Send("Sabotage", "Host only to unlock doors.", 3f);
                    }
                    
                    GUILayout.EndHorizontal();
                }
            }

            GUILayout.EndVertical();
            GUILayout.EndScrollView();
        }
    }
}