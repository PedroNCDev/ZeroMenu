using System;
using UnityEngine;
using Il2CppInterop.Runtime.InteropTypes.Arrays;

namespace ZeroMenu.Tabs
{
    public class AboutTab : ITab
    {
        public string Name => "About";
        private Vector2 _scrollPosition = Vector2.zero;

        private static GUIStyle _bigTitleStyle;
        private static GUIStyle _subtitleStyle;
        private static GUIStyle _creditStyle;
        private static GUIStyle _featureStyle;
        private static GUIStyle _boxStyle;

        private const string GithubUrl = "https://github.com/PedroNCDev/ZeroMenu";

        private void InitStyles()
        {
            if (_bigTitleStyle != null) return;

            _bigTitleStyle = new GUIStyle(MenuStyles.Title);
            _bigTitleStyle.fontSize = 26;
            _bigTitleStyle.fontStyle = FontStyle.Bold;
            _bigTitleStyle.alignment = TextAnchor.MiddleCenter;
            _bigTitleStyle.normal.textColor = Color.white;

            _subtitleStyle = new GUIStyle(MenuStyles.Label);
            _subtitleStyle.fontSize = 12;
            _subtitleStyle.alignment = TextAnchor.MiddleCenter;
            _subtitleStyle.normal.textColor = new Color(0.65f, 0.75f, 0.85f);

            _creditStyle = new GUIStyle(MenuStyles.Label);
            _creditStyle.alignment = TextAnchor.MiddleCenter;
            _creditStyle.normal.textColor = new Color(0.85f, 0.85f, 0.85f);

            _featureStyle = new GUIStyle(MenuStyles.Label);
            _featureStyle.normal.textColor = new Color(0.8f, 0.8f, 0.8f);

            _boxStyle = new GUIStyle();
            _boxStyle.normal.background = RoundedTexture.Get(10, new Color(0.05f, 0.05f, 0.08f, 0.7f), new Color(0.2f, 0.2f, 0.25f, 0.5f), 1);
            _boxStyle.border = new RectOffset { left = 10, right = 10, top = 10, bottom = 10 };
            _boxStyle.padding = new RectOffset { left = 10, right = 10, top = 6, bottom = 6 };
        }

        public void Draw()
        {
            InitStyles();

            _scrollPosition = SmoothScrollView.Begin(_scrollPosition, Array.Empty<GUILayoutOption>());
            GUILayout.BeginVertical(new Il2CppReferenceArray<GUILayoutOption>(new GUILayoutOption[] { GUILayout.Width((float)MainUI.windowWidth * 0.9f) }));

            GUILayout.Space(20f);
            GUILayout.Label("ZeroMenu", _bigTitleStyle, new Il2CppReferenceArray<GUILayoutOption>(0));
            GUILayout.Label($"v{ZeroMenu.Plugin.PluginVersion}  •  AmongUs Mod Menu", _subtitleStyle, new Il2CppReferenceArray<GUILayoutOption>(0));
            GUILayout.Space(20f);

            DrawSection("Developers", () =>
            {
                GUILayout.Label("PedroCAZ  •  Osiris", _creditStyle, new Il2CppReferenceArray<GUILayoutOption>(0));
            });

            GUILayout.Space(10f);

            DrawSection("Links", () =>
            {
                GUILayout.BeginHorizontal(new Il2CppReferenceArray<GUILayoutOption>(0));
                GUILayout.FlexibleSpace();
                if (CustomButton.Draw("GitHub Repository")) Application.OpenURL(GithubUrl);
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();
            });

            GUILayout.Space(10f);

            DrawSection("Features", () =>
            {
                GUILayout.Label("• Host-Only tools (Kill All, Role Manager, Map Spawner)", _featureStyle, new Il2CppReferenceArray<GUILayoutOption>(0));
                GUILayout.Label("• Player tools (Teleport, Murder, Immortality, Frames)", _featureStyle, new Il2CppReferenceArray<GUILayoutOption>(0));
                GUILayout.Label("• ESP (Roles, Tracers, Ghosts, Reveal Votes)", _featureStyle, new Il2CppReferenceArray<GUILayoutOption>(0));
                GUILayout.Label("• Sabotage (All maps, doors, custom systems)", _featureStyle, new Il2CppReferenceArray<GUILayoutOption>(0));
                GUILayout.Label("• Roles (Endless vent, no cooldowns, reach)", _featureStyle, new Il2CppReferenceArray<GUILayoutOption>(0));
                GUILayout.Label("• Chat (Colors, Dark Mode, URL bypass)", _featureStyle, new Il2CppReferenceArray<GUILayoutOption>(0));
                GUILayout.Label("• Spoofing (Platform, Level, Version, Guest)", _featureStyle, new Il2CppReferenceArray<GUILayoutOption>(0));
                GUILayout.Label("• Protections & Anticheat", _featureStyle, new Il2CppReferenceArray<GUILayoutOption>(0));
            });

            GUILayout.Space(20f);
            GUILayout.Label("Made with ♥ by the Pedro And Osiris", _creditStyle, new Il2CppReferenceArray<GUILayoutOption>(0));
            GUILayout.Space(20f);

            GUILayout.EndVertical();
            GUILayout.EndScrollView();
        }

        private void DrawSection(string title, Action content)
        {
            GUILayout.BeginVertical(_boxStyle, new Il2CppReferenceArray<GUILayoutOption>(new GUILayoutOption[] { GUILayout.ExpandWidth(true) }));
            GUILayout.Label(title, MenuStyles.BoldLabel, new Il2CppReferenceArray<GUILayoutOption>(0));
            GUILayout.Space(4f);
            content();
            GUILayout.EndVertical();
        }
    }
}
