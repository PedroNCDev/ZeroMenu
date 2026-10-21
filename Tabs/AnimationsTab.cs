using System;
using UnityEngine;
using ZeroMenu.Functions;
using ZeroMenu.Utilities;
using Il2CppInterop.Runtime.InteropTypes.Arrays;

namespace ZeroMenu.Tabs
{
    public class AnimationsTab : ITab
    {
        public string Name => "Animations";
        private Vector2 _scrollPosition = Vector2.zero;

        public void Draw()
        {
            _scrollPosition = SmoothScrollView.Begin(_scrollPosition, System.Array.Empty<GUILayoutOption>());
            GUILayout.BeginVertical(new Il2CppReferenceArray<GUILayoutOption>(new GUILayoutOption[] { GUILayout.Width((float)MainUI.windowWidth * 0.425f) }));

            Animations.Shields = CustomToggle.Draw(Animations.Shields, "Shields", "animShields");
            Animations.Asteroids = CustomToggle.Draw(Animations.Asteroids, "Asteroids", "animAsteroids");
            Animations.EmptyGarbage = CustomToggle.Draw(Animations.EmptyGarbage, "Empty Garbage", "animEmptyGarbage");
            Animations.MedScan = CustomToggle.Draw(Animations.MedScan, "Medbay Scan", "animMedScan");
            Animations.CamsInUse = CustomToggle.Draw(Animations.CamsInUse, "Cams In Use", "animCamsInUse");

            GUILayout.Space(15f);
            GUILayout.Label("Client-Sided", MenuStyles.BoldLabel, System.Array.Empty<GUILayoutOption>());
            
            Animations.Moonwalk = CustomToggle.Draw(Animations.Moonwalk, "Moonwalk", "moonWalk");

            GUILayout.Space(15f);
            GUILayout.Label("Fake Tasks", MenuStyles.BoldLabel, System.Array.Empty<GUILayoutOption>());
            
            FakeTasks.FakeScanTaskEnabled = CustomToggle.Draw(FakeTasks.FakeScanTaskEnabled, "Fake Scan Task", "FakeScanTaskEnabled");
            FakeTasks.FakeGarbageTaskEnabled = CustomToggle.Draw(FakeTasks.FakeGarbageTaskEnabled, "Fake Empty Garbage", "FakeGarbageTaskEnabled");

            GUILayout.EndVertical();
            GUILayout.EndScrollView();
        }
    }
}