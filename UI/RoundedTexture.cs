using System.Collections.Generic;
using UnityEngine;

namespace ZeroMenu
{
    public static class RoundedTexture
    {
        private static readonly Dictionary<string, Texture2D> Cache = new Dictionary<string, Texture2D>();
        private const int Supersample = 6;

        public static Texture2D Get(int radius, Color fill, Color border, int borderWidth)
        {
            return GetGradient(radius, fill, fill, border, borderWidth);
        }

        public static Texture2D GetGradient(int radius, Color left, Color right, Color border, int borderWidth)
        {
            string key = "grad_" + radius + "_" + ColorUtility.ToHtmlStringRGBA(left) + "_" + ColorUtility.ToHtmlStringRGBA(right) + "_" + ColorUtility.ToHtmlStringRGBA(border) + "_" + borderWidth;

            Texture2D tex;
            if (Cache.TryGetValue(key, out tex) && tex != null) return tex;

            int smallSize = radius * 2 + 16;
            int bigSize = smallSize * Supersample;

            Color[] bigPixels = new Color[bigSize * bigSize];
            Color transparent = new Color(0f, 0f, 0f, 0f);
            float outerR = radius;
            float innerR = radius - borderWidth;

            for (int y = 0; y < bigSize; y++)
            {
                for (int x = 0; x < bigSize; x++)
                {
                    float fx = (x + 0.5f) / Supersample;
                    float fy = (y + 0.5f) / Supersample;

                    float dx = 0f;
                    float dy = 0f;
                    if (fx < radius) dx = radius - fx;
                    else if (fx >= smallSize - radius) dx = fx - (smallSize - radius);
                    if (fy < radius) dy = radius - fy;
                    else if (fy >= smallSize - radius) dy = fy - (smallSize - radius);

                    float dist = Mathf.Sqrt(dx * dx + dy * dy);

                    Color pixel;
                    if (dist <= innerR)
                    {
                        float t = Mathf.Clamp01(fx / (float)smallSize);
                        t = t * t * (3f - 2f * t);
                        pixel = Color.Lerp(left, right, t);
                    }
                    else if (dist <= outerR) pixel = border;
                    else pixel = transparent;

                    bigPixels[y * bigSize + x] = pixel;
                }
            }

            Color[] smallPixels = new Color[smallSize * smallSize];
            int samples = Supersample * Supersample;

            for (int y = 0; y < smallSize; y++)
            {
                for (int x = 0; x < smallSize; x++)
                {
                    float r = 0f, g = 0f, b = 0f, a = 0f;
                    for (int sy = 0; sy < Supersample; sy++)
                    {
                        for (int sx = 0; sx < Supersample; sx++)
                        {
                            Color c = bigPixels[(y * Supersample + sy) * bigSize + (x * Supersample + sx)];
                            r += c.r; g += c.g; b += c.b; a += c.a;
                        }
                    }
                    smallPixels[y * smallSize + x] = new Color(r / samples, g / samples, b / samples, a / samples);
                }
            }

            tex = new Texture2D(smallSize, smallSize, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;
            tex.wrapMode = TextureWrapMode.Clamp;
            tex.SetPixels(smallPixels);
            tex.Apply();
            tex.hideFlags = HideFlags.HideAndDontSave;
            Cache[key] = tex;
            return tex;
        }
    }
}