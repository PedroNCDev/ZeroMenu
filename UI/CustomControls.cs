using System.Collections.Generic;
using UnityEngine;
using Il2CppInterop.Runtime.InteropTypes.Arrays;

namespace ZeroMenu
{
    public static class CustomToggle
    {
        private const float ToggleWidth = 36f;
        private const float ToggleHeight = 22f;
        private const float KnobSize = 16f;
        private const float KnobPadding = 3f;
        private const float AnimationDuration = 0.2f;

        private static GUIStyle _trackOffStyle;
        private static GUIStyle _trackOnStyle;
        private static GUIStyle _knobStyle;
        private static GUIStyle _knobGlowStyle;

        private static readonly Dictionary<string, float> _animState = new Dictionary<string, float>();
        private static readonly Dictionary<string, float> _lastUpdateTime = new Dictionary<string, float>();

        private static void ApplyAllStates(GUIStyle s, Texture2D bg)
        {
            s.normal.background = bg;
            s.hover.background = bg;
            s.active.background = bg;
            s.focused.background = bg;
            s.onNormal.background = bg;
            s.onHover.background = bg;
            s.onActive.background = bg;
            s.onFocused.background = bg;
        }

        private static Texture2D CreateGlowTexture(int size)
        {
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            int center = size / 2;
            float radius = center;
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dist = Vector2.Distance(new Vector2(x, y), new Vector2(center, center));
                    float alpha = Mathf.Clamp01(1f - (dist / radius));
                    alpha = Mathf.Pow(alpha, 2.5f) * 0.8f;
                    tex.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
                }
            }
            tex.Apply();
            tex.hideFlags = HideFlags.HideAndDontSave;
            return tex;
        }

        private static void EnsureStyles()
        {
            if (_trackOffStyle == null)
            {
                _trackOffStyle = new GUIStyle();
                Texture2D bg = RoundedTexture.Get(11, new Color(0.15f, 0.15f, 0.20f, 1f), Color.clear, 0);
                ApplyAllStates(_trackOffStyle, bg);
                _trackOffStyle.border = new RectOffset { left = 13, right = 13, top = 13, bottom = 13 };
                _trackOffStyle.padding = new RectOffset();
                _trackOffStyle.margin = new RectOffset();
            }
            if (_trackOnStyle == null)
            {
                _trackOnStyle = new GUIStyle();
                Texture2D bg = RoundedTexture.Get(11, new Color(0.65f, 0.75f, 0.85f, 1f), Color.clear, 0);
                ApplyAllStates(_trackOnStyle, bg);
                _trackOnStyle.border = new RectOffset { left = 13, right = 13, top = 13, bottom = 13 };
                _trackOnStyle.padding = new RectOffset();
                _trackOnStyle.margin = new RectOffset();
            }
            if (_knobStyle == null)
            {
                _knobStyle = new GUIStyle();
                Texture2D bg = RoundedTexture.Get(8, Color.white, Color.white, 0);
                ApplyAllStates(_knobStyle, bg);
                _knobStyle.border = new RectOffset { left = 10, right = 10, top = 10, bottom = 10 };
                _knobStyle.padding = new RectOffset();
                _knobStyle.margin = new RectOffset();
            }
            if (_knobGlowStyle == null)
            {
                _knobGlowStyle = new GUIStyle();
                Texture2D glowTex = CreateGlowTexture(32);
                ApplyAllStates(_knobGlowStyle, glowTex);
                _knobGlowStyle.border = new RectOffset { left = 16, right = 16, top = 16, bottom = 16 };
                _knobGlowStyle.padding = new RectOffset();
                _knobGlowStyle.margin = new RectOffset();
            }
        }

        private static float GetAnimatedValue(string key, bool target)
        {
            float targetValue = target ? 1f : 0f;
            float currentValue;

            if (!_animState.TryGetValue(key, out currentValue))
            {
                _animState[key] = targetValue;
                _lastUpdateTime[key] = Time.unscaledTime;
                return targetValue;
            }

            float now = Time.unscaledTime;
            float lastTime;
            if (!_lastUpdateTime.TryGetValue(key, out lastTime)) lastTime = now;

            float dt = now - lastTime;
            if (dt > 0.1f) dt = 0.1f;
            _lastUpdateTime[key] = now;

            float step = dt / AnimationDuration;
            currentValue = Mathf.MoveTowards(currentValue, targetValue, step);
            _animState[key] = currentValue;

            return currentValue;
        }

        public static bool Draw(bool value, string label, string toggleName = null)
        {
            EnsureStyles();

            string key = toggleName ?? label ?? "default";
            float anim = GetAnimatedValue(key, value);

            GUILayout.BeginHorizontal(new Il2CppReferenceArray<GUILayoutOption>(0));

            Il2CppReferenceArray<GUILayoutOption> fixedSize = new Il2CppReferenceArray<GUILayoutOption>(new GUILayoutOption[] { GUILayout.Width(ToggleWidth), GUILayout.Height(ToggleHeight) });
            Rect rect = GUILayoutUtility.GetRect(ToggleWidth, ToggleHeight, fixedSize);

            Event e = Event.current;

            if (e.type == EventType.MouseDown && e.button == 0 && rect.Contains(e.mousePosition))
            {
                value = !value;
                e.Use();
            }

            GUIContent emptyContent = new GUIContent("");
            GUIStyle trackStyle = (anim >= 0.5f) ? _trackOnStyle : _trackOffStyle;
            GUI.Box(rect, emptyContent, trackStyle);

            float knobY = rect.y + (rect.height - KnobSize) / 2f;
            float knobXLeft = rect.x + KnobPadding;
            float knobXRight = rect.x + rect.width - KnobSize - KnobPadding;
            float knobX = Mathf.Lerp(knobXLeft, knobXRight, anim);

            Rect thumbRect = new Rect(knobX, knobY, KnobSize, KnobSize);
            
            Rect glowRect = new Rect(thumbRect.x - 8f, thumbRect.y - 8f, thumbRect.width + 16f, thumbRect.height + 16f);
            GUI.Box(glowRect, emptyContent, _knobGlowStyle);

            GUI.Box(thumbRect, emptyContent, _knobStyle);

            if (!string.IsNullOrEmpty(label))
            {
                GUILayout.Label(label, MenuStyles.Label, new Il2CppReferenceArray<GUILayoutOption>(0));
            }

            GUILayout.EndHorizontal();
            return value;
        }
    }

    public static class CustomButton
    {
        private static GUIStyle _style;
        private static readonly Dictionary<string, float> _fadeStates = new Dictionary<string, float>();

        private static void InitStyle()
        {
            if (_style != null) return;
            
            _style = new GUIStyle(GUI.skin.button);
            Texture2D normalTex = RoundedTexture.Get(8, new Color(0.15f, 0.15f, 0.18f, 1f), Color.clear, 0);
            Texture2D hoverTex = RoundedTexture.Get(8, new Color(0.25f, 0.25f, 0.28f, 1f), Color.clear, 0);
            
            _style.normal.background = normalTex;
            _style.hover.background = hoverTex;
            _style.active.background = hoverTex;
            _style.normal.textColor = Color.white;
            _style.hover.textColor = Color.white;
            _style.active.textColor = Color.white;
            _style.border = new RectOffset { left = 8, right = 8, top = 8, bottom = 8 };
            _style.alignment = TextAnchor.MiddleCenter;
            _style.fontSize = 13;
        }

        public static bool Draw(string text)
        {
            InitStyle();

            if (!_fadeStates.ContainsKey(text)) _fadeStates[text] = 0f;
            float fade = _fadeStates[text];

            Rect rect = GUILayoutUtility.GetRect(new GUIContent(text), _style, new Il2CppReferenceArray<GUILayoutOption>(new GUILayoutOption[] { GUILayout.Height(30f) }));
            
            bool clicked = false;
            if (Event.current.type == EventType.MouseDown && Event.current.button == 0 && rect.Contains(Event.current.mousePosition))
            {
                clicked = true;
                _fadeStates[text] = 1f;
                fade = 1f;
            }

            GUI.Box(rect, text, _style);

            return clicked;
        }
    }
}