using System.Collections.Generic;
using UnityEngine;
using Il2CppInterop.Runtime.InteropTypes.Arrays;

namespace ZeroMenu
{
    public static class CustomToggle
    {
        private const float ToggleWidth = 45f;
        private const float ToggleHeight = 22f;
        private const float KnobSize = 18f;
        private const float KnobPadding = 2f;
        private const float AnimationSpeed = 8f;

        private static readonly Dictionary<string, float> _animState = new Dictionary<string, float>();

        private static readonly Color _offColor = new Color(0.25f, 0.25f, 0.25f, 1f);
        private static readonly Color _onColor = new Color(0.65f, 0.75f, 0.85f, 1f);
        private static readonly Color _thumbColor = Color.white;

        private static Texture2D _offTex;
        private static Texture2D _onTex;
        private static Texture2D _thumbTex;

        private static void EnsureTextures()
        {
            if (_offTex == null)
            {
                _offTex = MakeSolidTex(_offColor);
                _onTex = MakeSolidTex(_onColor);
                _thumbTex = MakeSolidTex(_thumbColor);
            }
        }

        private static Texture2D MakeSolidTex(Color c)
        {
            Texture2D tex = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            tex.SetPixel(0, 0, c);
            tex.SetPixel(0, 1, c);
            tex.SetPixel(1, 0, c);
            tex.SetPixel(1, 1, c);
            tex.Apply();
            tex.filterMode = FilterMode.Point;
            tex.hideFlags = HideFlags.HideAndDontSave;
            return tex;
        }

        public static bool Draw(bool value, string label, string toggleName = null)
        {
            EnsureTextures();

            string key = toggleName ?? label ?? "default";
            if (!_animState.ContainsKey(key))
                _animState[key] = value ? 1f : 0f;

            float target = value ? 1f : 0f;
            _animState[key] = Mathf.Lerp(_animState[key], target, Time.deltaTime * AnimationSpeed);
            float anim = _animState[key];

            GUILayout.BeginHorizontal(new Il2CppReferenceArray<GUILayoutOption>(0));

            if (!string.IsNullOrEmpty(label))
            {
                GUILayout.Label(label, MenuStyles.Label, new Il2CppReferenceArray<GUILayoutOption>(new GUILayoutOption[] { GUILayout.Width(220f) }));
            }

            Il2CppReferenceArray<GUILayoutOption> fixedSize = new Il2CppReferenceArray<GUILayoutOption>(new GUILayoutOption[] { GUILayout.Width(ToggleWidth), GUILayout.Height(ToggleHeight) });
            Rect rect = GUILayoutUtility.GetRect(ToggleWidth, ToggleHeight, fixedSize);
            
            bool isHover = rect.Contains(Event.current.mousePosition);

            Color bgColor = Color.Lerp(_offColor, _onColor, anim);
            if (isHover) bgColor = Color.Lerp(bgColor, Color.white, 0.3f);

            GUIStyle trackStyle = new GUIStyle(GUI.skin.box);
            trackStyle.normal.background = anim < 0.5f ? _offTex : _onTex;

            Color savedColor = GUI.color;
            GUI.color = bgColor;
            GUI.Box(rect, "", trackStyle);
            GUI.color = savedColor;

            if (GUI.Button(rect, "", GUIStyle.none))
            {
                value = !value;
            }

            float knobXLeft = rect.x + KnobPadding;
            float knobXRight = rect.x + rect.width - KnobSize - KnobPadding;
            float knobX = Mathf.Lerp(knobXLeft, knobXRight, anim);

            GUIStyle knobStyle = new GUIStyle(GUI.skin.box);
            knobStyle.normal.background = _thumbTex;
            
            GUI.Box(new Rect(knobX, rect.y + KnobPadding, KnobSize, KnobSize), "", knobStyle);

            GUILayout.EndHorizontal();
            GUILayout.Space(2f);

            return value;
        }
    }

    public static class CustomButton
    {
        private class ButtonState
        {
            public float AnimProgress = 0f;
            public float Scale = 1f;
            public float ScaleVelocity = 0f;
            public bool IsAnimating = false;
        }

        private static GUIStyle _baseStyle;
        private static readonly Dictionary<string, ButtonState> _states = new Dictionary<string, ButtonState>();

        private const float AnimationDuration = 0.4f;

        private static void InitStyles()
        {
            if (_baseStyle != null) return;

            _baseStyle = new GUIStyle(GUI.skin.button);
            Texture2D normalTex = RoundedTexture.Get(8, new Color(0.15f, 0.15f, 0.18f, 1f), Color.clear, 0);
            Texture2D hoverTex = RoundedTexture.Get(8, new Color(0.25f, 0.25f, 0.28f, 1f), Color.clear, 0);
            
            _baseStyle.normal.background = normalTex;
            _baseStyle.hover.background = hoverTex;
            _baseStyle.active.background = hoverTex;
            _baseStyle.normal.textColor = Color.white;
            _baseStyle.hover.textColor = Color.white;
            _baseStyle.active.textColor = Color.white;
            _baseStyle.border = new RectOffset { left = 8, right = 8, top = 8, bottom = 8 };
            _baseStyle.alignment = TextAnchor.MiddleCenter;
            _baseStyle.fontSize = 13;
        }

        private static ButtonState GetState(string key)
        {
            if (!_states.TryGetValue(key, out ButtonState state))
            {
                state = new ButtonState();
                _states[key] = state;
            }
            return state;
        }

        public static bool Draw(string text)
        {
            InitStyles();

            string key = text;
            ButtonState state = GetState(key);

            Rect rect = GUILayoutUtility.GetRect(new GUIContent(text), _baseStyle, new Il2CppReferenceArray<GUILayoutOption>(new GUILayoutOption[] { GUILayout.Height(30f) }));

            Event e = Event.current;
            bool clicked = false;

            if (e.type == EventType.MouseDown && e.button == 0 && rect.Contains(e.mousePosition))
            {
                clicked = true;
                state.AnimProgress = 0f;
                state.IsAnimating = true;
                e.Use();
            }

            if (state.IsAnimating)
            {
                state.AnimProgress += Time.unscaledDeltaTime / AnimationDuration;
                if (state.AnimProgress >= 1f)
                {
                    state.AnimProgress = 1f;
                    state.IsAnimating = false;
                }
            }

            float smoothProgress = Mathf.SmoothStep(0f, 1f, state.AnimProgress);
            
            float targetScale = 1f;
            if (smoothProgress > 0.5f)
            {
                targetScale = Mathf.Lerp(1f, 1.05f, (smoothProgress - 0.5f) * 2f);
                if (smoothProgress > 0.8f)
                {
                    targetScale = Mathf.Lerp(1.05f, 1f, (smoothProgress - 0.8f) * 5f);
                }
            }

            state.Scale = Mathf.SmoothDamp(state.Scale, targetScale, ref state.ScaleVelocity, 0.1f);

            float alpha = 1f;
            if (state.IsAnimating && smoothProgress > 0.7f)
            {
                alpha = Mathf.Lerp(1f, 0.5f, (smoothProgress - 0.7f) / 0.3f);
            }
            else if (!state.IsAnimating && state.AnimProgress >= 1f)
            {
                state.AnimProgress = 0f;
            }

            Matrix4x4 savedMatrix = GUI.matrix;
            Color savedColor = GUI.color;

            Vector2 center = new Vector2(rect.x + rect.width / 2f, rect.y + rect.height / 2f);
            GUIUtility.ScaleAroundPivot(new Vector2(state.Scale, state.Scale), center);
            GUI.color = new Color(1f, 1f, 1f, alpha);

            GUI.Box(rect, text, _baseStyle);

            GUI.matrix = savedMatrix;
            GUI.color = savedColor;

            return clicked;
        }
    }
}
