using UnityEngine;
using Il2CppInterop.Runtime.InteropTypes.Arrays;

namespace ZeroMenu.Tabs
{
    public class AboutTab : ITab
    {
        public string Name => "About";

        public void Draw()
        {
            GUILayout.Label("ZeroMenu JEJE\n Created By PedroCAZ And Osiris", ZeroMenu.MenuStyles.Label, new Il2CppReferenceArray<GUILayoutOption>(0));
        }
    }
}