using System;
using UnityEngine;
using ZeroMenu.Functions;

namespace ZeroMenu.Tabs
{
    public class ESPTab : ITab
    {
        public string Name => "ESP";
        private Vector2 _scrollPosition = Vector2.zero;

        public void Draw()
        {
            this._scrollPosition = SmoothScrollView.Begin(this._scrollPosition, System.Array.Empty<GUILayoutOption>());
            
            GUILayout.BeginVertical(new GUILayoutOption[] { GUILayout.Width((float)MainUI.windowWidth * 0.425f) });

            ESP.SeeRoles = CustomToggle.Draw(ESP.SeeRoles, "See Roles", "SeeRoles");
            ESP.SeePlayerInfo = CustomToggle.Draw(ESP.SeePlayerInfo, "See Player Info", "SeePlayerInfo");
            ESP.SeeGhosts = CustomToggle.Draw(ESP.SeeGhosts, "See Ghosts", "SeeGhosts");
            ESP.NoShadows = CustomToggle.Draw(ESP.NoShadows, "No Shadows", "NoShadows");
            ESP.RevealVotes = CustomToggle.Draw(ESP.RevealVotes, "Reveal Votes", "RevealVotes");

            GUILayout.Space(10f);
            GUILayout.Label("Camera", MenuStyles.BoldLabel, System.Array.Empty<GUILayoutOption>());
            ESP.ZoomOut = CustomToggle.Draw(ESP.ZoomOut, "Zoom Out", "ZoomOut");
            ESP.Freecam = CustomToggle.Draw(ESP.Freecam, "Freecam", "Freecam");

            GUILayout.Space(10f);
            GUILayout.Label("Tracers", MenuStyles.BoldLabel, System.Array.Empty<GUILayoutOption>());

            ESP.TracersCrew = CustomToggle.Draw(ESP.TracersCrew, "Crewmates", "TracersCrew");
            ESP.TracersImps = CustomToggle.Draw(ESP.TracersImps, "Impostors", "TracersImps");
            ESP.TracersGhosts = CustomToggle.Draw(ESP.TracersGhosts, "Ghosts", "TracersGhosts");
            ESP.TracersBodies = CustomToggle.Draw(ESP.TracersBodies, "Dead Bodies", "TracersBodies");
            ESP.ColorBasedTracers = CustomToggle.Draw(ESP.ColorBasedTracers, "Color-based", "ColorBasedTracers");

            GUILayout.Space(10f);
            GUILayout.Label("Minimap", MenuStyles.BoldLabel, System.Array.Empty<GUILayoutOption>());

            ESP.MapCrew = CustomToggle.Draw(ESP.MapCrew, "Crewmates", "MapCrew");
            ESP.MapImps = CustomToggle.Draw(ESP.MapImps, "Impostors", "MapImps");
            ESP.MapGhosts = CustomToggle.Draw(ESP.MapGhosts, "Ghosts", "MapGhosts");
            ESP.ColorBasedMap = CustomToggle.Draw(ESP.ColorBasedMap, "Color-based", "ColorBasedMap");

            GUILayout.EndVertical();
            GUILayout.EndScrollView();
        }
    }
}