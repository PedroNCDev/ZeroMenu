using System.Collections.Generic;
using UnityEngine;
using Il2CppInterop.Runtime.InteropTypes.Arrays;

namespace ZeroMenu
{
    public static class CustomSlider
    {
        private const float TrackHeight = 8f;
        private const int ThumbDiameter = 18;
        private const float TrackRadius = 4f;
        private const float SmoothTime = 0.1f;
        private const float DragScaleTarget = 1.15f;

        private static readonly Color TrackColor = new Color(0.12f, 0.12f, 0.15f, 1f);
        private static readonly Color FillColor = new Color(0.65f, 0.75f, 0.85f, 1f);
        private static readonly Color TrackBorder = new Color(0.3f, 0.3f, 0.35f, 0.5f);
        private static readonly Color ThumbFill = new Color(1f, 1f, 1f, 1f);
        private static readonly Color ThumbBorder = new Color(0.65f, 0.75f, 0.85f, 1f);

        private class SliderState
        {
            public float VisualValue;
            public float Velocity;
            public float ThumbScale = 1f;
            public float ThumbScaleVelocity;
            public bool Initialized;
            public float LastMin;
            public float LastMax;
        }

        private static readonly Dictionary<int, SliderState> _statesById = new Dictionary<int, SliderState>();
        private static GUIStyle _fillStyle;
        private static GUIStyle _trackStyle;
        private static GUIStyle _thumbStyle;

        public static float DrawAuto(float value, float min, float max, Il2CppReferenceArray<GUILayoutOption> options)
        {
            if (options == null) options = new Il2CppReferenceArray<GUILayoutOption>(0);

            int extra = 1;
            Il2CppReferenceArray<GUILayoutOption> combined = new Il2CppReferenceArray<GUILayoutOption>(options.Length + extra);
            for (int i = 0; i < options.Length; i++) combined[i] = options[i];
            combined[options.Length] = GUILayout.Height(ThumbDiameter + 6f);

            Rect rect = GUILayoutUtility.GetRect(GUIContent.none, GUI.skin.horizontalSlider, combined);
            int id = GUIUtility.GetControlID(FocusType.Passive, rect);
            return DrawById(id, rect, value, min, max);
        }

        private static float DrawById(int id, Rect layoutRect, float value, float min, float max)
        {
            if (layoutRect.width < 1f) return value;

            if (!_statesById.TryGetValue(id, out var state))
            {
                state = new SliderState();
                _statesById[id] = state;
            }

            float visualHeight = ThumbDiameter + 4f;
            Rect rect = new Rect(layoutRect.x, layoutRect.y + layoutRect.height * 0.5f - visualHeight * 0.5f, layoutRect.width, visualHeight);

            float trackX = rect.x + ThumbDiameter * 0.5f;
            float trackWidth = rect.width - ThumbDiameter;
            if (trackWidth < 1f) trackWidth = 1f;

            float range = max - min;
            if (range <= 0f) range = 1f;

            if (!state.Initialized)
            {
                state.VisualValue = Mathf.Clamp01((value - min) / range);
                state.LastMin = min;
                state.LastMax = max;
                state.Initialized = true;
            }

            if (Mathf.Abs(state.LastMin - min) > 0.0001f || Mathf.Abs(state.LastMax - max) > 0.0001f)
            {
                state.VisualValue = Mathf.Clamp01((value - min) / range);
                state.Velocity = 0f;
                state.LastMin = min;
                state.LastMax = max;
            }

            Event e = Event.current;
            bool isDragging = GUIUtility.hotControl == id;

            if (e.type == EventType.MouseDown && e.button == 0 && rect.Contains(e.mousePosition))
            {
                GUIUtility.hotControl = id;
                isDragging = true;
                float t = Mathf.Clamp01((e.mousePosition.x - trackX) / trackWidth);
                value = min + t * range;
                e.Use();
            }
            else if (e.type == EventType.MouseDrag && GUIUtility.hotControl == id)
            {
                float t = Mathf.Clamp01((e.mousePosition.x - trackX) / trackWidth);
                value = min + t * range;
                e.Use();
            }
            else if (e.type == EventType.MouseUp && GUIUtility.hotControl == id)
            {
                GUIUtility.hotControl = 0;
                isDragging = false;
                e.Use();
            }

            if (value < min) value = min;
            if (value > max) value = max;

            float target01 = Mathf.Clamp01((value - min) / range);

            state.VisualValue = Mathf.SmoothDamp(state.VisualValue, target01, ref state.Velocity, SmoothTime, Mathf.Infinity, Time.unscaledDeltaTime);

            float thumbScaleTarget = isDragging ? DragScaleTarget : 1f;
            state.ThumbScale = Mathf.SmoothDamp(state.ThumbScale, thumbScaleTarget, ref state.ThumbScaleVelocity, SmoothTime, Mathf.Infinity, Time.unscaledDeltaTime);

            float trackY = rect.y + (rect.height - TrackHeight) * 0.5f;
            Rect trackRect = new Rect(trackX, trackY, trackWidth, TrackHeight);

            GUI.Box(trackRect, GUIContent.none, GetTrackStyle());

            float fillWidth = state.VisualValue * trackWidth;
            if (fillWidth > 1f)
            {
                Rect fillRect = new Rect(trackX, trackY, fillWidth, TrackHeight);
                GUI.Box(fillRect, GUIContent.none, GetFillStyle());
            }

            float thumbCenterX = trackX + state.VisualValue * trackWidth;
            float thumbCenterY = rect.y + rect.height * 0.5f;
            float scaledDiameter = ThumbDiameter * state.ThumbScale;
            Rect thumbRect = new Rect(thumbCenterX - scaledDiameter * 0.5f, thumbCenterY - scaledDiameter * 0.5f, scaledDiameter, scaledDiameter);

            GUI.Box(thumbRect, GUIContent.none, GetThumbStyle());

            return value;
        }

        private static GUIStyle GetTrackStyle()
        {
            if (_trackStyle != null) return _trackStyle;
            Texture2D tex = RoundedTexture.Get((int)TrackRadius, TrackColor, TrackBorder, 2);
            _trackStyle = new GUIStyle();
            _trackStyle.normal.background = tex;
            _trackStyle.hover.background = tex;
            _trackStyle.active.background = tex;
            _trackStyle.focused.background = tex;
            _trackStyle.border = new RectOffset { left = (int)TrackRadius, right = (int)TrackRadius, top = (int)TrackRadius, bottom = (int)TrackRadius };
            _trackStyle.padding = new RectOffset();
            _trackStyle.margin = new RectOffset();
            return _trackStyle;
        }

        private static GUIStyle GetFillStyle()
        {
            if (_fillStyle != null) return _fillStyle;
            Texture2D tex = RoundedTexture.Get((int)TrackRadius, FillColor, FillColor, 0);
            _fillStyle = new GUIStyle();
            _fillStyle.normal.background = tex;
            _fillStyle.hover.background = tex;
            _fillStyle.active.background = tex;
            _fillStyle.focused.background = tex;
            _fillStyle.border = new RectOffset { left = (int)TrackRadius, right = (int)TrackRadius, top = (int)TrackRadius, bottom = (int)TrackRadius };
            _fillStyle.padding = new RectOffset();
            _fillStyle.margin = new RectOffset();
            return _fillStyle;
        }

        private static GUIStyle GetThumbStyle()
        {
            if (_thumbStyle != null) return _thumbStyle;
            int radius = ThumbDiameter / 2;
            Texture2D tex = RoundedTexture.Get(radius, ThumbFill, ThumbBorder, 2);
            _thumbStyle = new GUIStyle();
            _thumbStyle.normal.background = tex;
            _thumbStyle.hover.background = tex;
            _thumbStyle.active.background = tex;
            _thumbStyle.focused.background = tex;
            _thumbStyle.border = new RectOffset { left = radius, right = radius, top = radius, bottom = radius };
            _thumbStyle.padding = new RectOffset();
            _thumbStyle.margin = new RectOffset();
            return _thumbStyle;
        }
    }
}