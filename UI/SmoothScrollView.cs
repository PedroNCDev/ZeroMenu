using UnityEngine;
using Il2CppInterop.Runtime.InteropTypes.Arrays;

namespace ZeroMenu
{
    public static class SmoothScrollView
    {
        public static Vector2 Begin(Vector2 scrollPosition, GUILayoutOption[] options)
        {
            if (options == null) options = new GUILayoutOption[0];

            var il2CppOptions = new Il2CppReferenceArray<GUILayoutOption>(options.Length);
            for (int i = 0; i < options.Length; i++)
            {
                il2CppOptions[i] = options[i];
            }

            return GUILayout.BeginScrollView(scrollPosition, false, false, MenuStyles.InvisibleScrollbar, MenuStyles.InvisibleScrollbar, il2CppOptions);
        }
    }
}