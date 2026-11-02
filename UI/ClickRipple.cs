using System;
using System.Collections.Generic;
using UnityEngine;

namespace ZeroMenu.UI
{
    public static class ClickRipple
    {
        private struct Ripple
        {
            public Vector2 Position;
            public float StartTime;
        }

        private static readonly List<Ripple> _ripples = new List<Ripple>();
        private static Texture2D _circleTexture;
        private static GUIStyle _circleStyle;

        private const float Duration = 0.2f;
        private const float StartRadius = 6f;
        private const float EndRadius = 44f;
        private const float StartAlpha = 0.50f;
        private const float EndAlpha = 0.00f;

        public static void Spawn(Vector2 guiPosition)
        {
            _ripples.Add(new Ripple
            {
                Position = guiPosition,
                StartTime = Time.unscaledTime
            });
        }

        public static void Draw()
        {
            if (_ripples.Count == 0) return;

            float now = Time.unscaledTime;
            GUIStyle style = GetCircleStyle();

            for (int i = _ripples.Count - 1; i >= 0; i--)
            {
                Ripple r = _ripples[i];
                float elapsed = now - r.StartTime;

                if (elapsed >= Duration)
                {
                    _ripples.RemoveAt(i);
                    continue;
                }

                float t = elapsed / Duration;
                t = Mathf.Clamp01(t);
                t = t * t * (3f - 2f * t);

                float radius = Mathf.Lerp(StartRadius, EndRadius, t);
                float alpha = Mathf.Lerp(StartAlpha, EndAlpha, t);

                Color saved = GUI.color;
                GUI.color = new Color(1f, 1f, 1f, alpha);

                Rect rect = new Rect(
                    r.Position.x - radius,
                    r.Position.y - radius,
                    radius * 2f,
                    radius * 2f
                );

                GUI.Box(rect, GUIContent.none, style);

                GUI.color = saved;
            }
        }

        public static void Clear()
        {
            _ripples.Clear();
        }

        private static GUIStyle GetCircleStyle()
        {
            if (_circleStyle != null) return _circleStyle;

            Texture2D tex = GetCircleTexture();

            _circleStyle = new GUIStyle();
            _circleStyle.normal.background = tex;
            _circleStyle.hover.background = tex;
            _circleStyle.active.background = tex;
            _circleStyle.focused.background = tex;
            _circleStyle.border = new RectOffset { left = 0, right = 0, top = 0, bottom = 0 };
            _circleStyle.padding = new RectOffset { left = 0, right = 0, top = 0, bottom = 0 };
            _circleStyle.margin = new RectOffset { left = 0, right = 0, top = 0, bottom = 0 };
            _circleStyle.overflow = new RectOffset { left = 0, right = 0, top = 0, bottom = 0 };

            return _circleStyle;
        }

        private static Texture2D GetCircleTexture()
        {
            if (_circleTexture != null) return _circleTexture;

            int size = 64;
            _circleTexture = new Texture2D(size, size, TextureFormat.RGBA32, false);

            Color[] pixels = new Color[size * size];
            float center = size / 2f;
            float maxR = center - 1f;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dx = x - center + 0.5f;
                    float dy = y - center + 0.5f;
                    float dist = Mathf.Sqrt(dx * dx + dy * dy);

                    float a = Mathf.Clamp01(1f - dist / maxR);
                    a = a * a;

                    pixels[y * size + x] = new Color(1f, 1f, 1f, a);
                }
            }

            _circleTexture.SetPixels(pixels);
            _circleTexture.Apply();
            _circleTexture.filterMode = FilterMode.Bilinear;
            _circleTexture.wrapMode = TextureWrapMode.Clamp;
            _circleTexture.hideFlags = HideFlags.HideAndDontSave;

            return _circleTexture;
        }
    }
}
