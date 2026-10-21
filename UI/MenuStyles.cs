using UnityEngine;

namespace ZeroMenu
{
    public static class MenuStyles
    {
        public static UnityEngine.GUIStyle Window;
        public static UnityEngine.GUIStyle Title;
        public static UnityEngine.GUIStyle TabActive;
        public static UnityEngine.GUIStyle TabInactive;
        public static UnityEngine.GUIStyle TabHover;
        public static UnityEngine.GUIStyle Label;
        public static UnityEngine.GUIStyle BoldLabel;
        public static UnityEngine.GUIStyle Line;
        public static UnityEngine.GUIStyle Toggle;
        public static UnityEngine.GUIStyle Button;

        private static UnityEngine.GUIStyle _invisibleScrollbar;
        public static UnityEngine.GUIStyle InvisibleScrollbar
        {
            get
            {
                if (_invisibleScrollbar == null)
                {
                    _invisibleScrollbar = new UnityEngine.GUIStyle();
                    _invisibleScrollbar.fixedWidth = 0;
                    _invisibleScrollbar.fixedHeight = 0;
                    _invisibleScrollbar.margin = new RectOffset { left = 0, right = 0, top = 0, bottom = 0 };
                    _invisibleScrollbar.padding = new RectOffset { left = 0, right = 0, top = 0, bottom = 0 };
                    
                    Texture2D tex = new Texture2D(1, 1);
                    tex.SetPixel(0, 0, Color.clear);
                    tex.Apply();
                    tex.hideFlags = HideFlags.HideAndDontSave;
                    
                    _invisibleScrollbar.normal.background = tex;
                    _invisibleScrollbar.hover.background = tex;
                    _invisibleScrollbar.active.background = tex;
                }
                return _invisibleScrollbar;
            }
        }

        private static bool _initialized = false;

        public static void Init()
        {
            if (_initialized) return;
            _initialized = true;

            Texture2D bgTex = MakeRoundedRect(256, 256, 8, 1, new Color(0.06f, 0.06f, 0.08f, 0.98f), new Color(0.2f, 0.2f, 0.2f));
            
            Window = new UnityEngine.GUIStyle();
            Window.normal.background = bgTex;
            Window.hover.background = bgTex;
            Window.active.background = bgTex;
            Window.focused.background = bgTex;
            Window.border = new RectOffset { left = 8, right = 8, top = 8, bottom = 8 };

            Title = new UnityEngine.GUIStyle(GUI.skin.label);
            Title.fontSize = 16;
            Title.alignment = TextAnchor.MiddleCenter;
            Title.fontStyle = FontStyle.Bold;
            Title.normal.textColor = Color.white;

            TabActive = new UnityEngine.GUIStyle(GUI.skin.label);
            TabActive.fontSize = 13;
            TabActive.alignment = TextAnchor.MiddleCenter;
            TabActive.normal.textColor = Color.white;
            TabActive.wordWrap = false;

            TabInactive = new UnityEngine.GUIStyle(TabActive);
            TabInactive.normal.textColor = new Color(0.6f, 0.6f, 0.6f);
            TabInactive.wordWrap = false;

            TabHover = new UnityEngine.GUIStyle(TabActive);
            TabHover.normal.textColor = new Color(0.85f, 0.85f, 0.85f);
            TabHover.wordWrap = false;

            Label = new UnityEngine.GUIStyle(GUI.skin.label);
            Label.fontSize = 13;
            Label.normal.textColor = new Color(0.8f, 0.8f, 0.8f);

            BoldLabel = new UnityEngine.GUIStyle(Label);
            BoldLabel.fontStyle = FontStyle.Bold;
            BoldLabel.normal.textColor = Color.white;

            Line = new UnityEngine.GUIStyle();
            Line.normal.background = Texture2D.whiteTexture;
            Line.margin = new RectOffset { left = 0, right = 0, top = 0, bottom = 0 };
            Line.padding = new RectOffset { left = 0, right = 0, top = 0, bottom = 0 };
            Line.fixedHeight = 2f;

            Toggle = new UnityEngine.GUIStyle(GUI.skin.toggle);
            Toggle.fontSize = 14;
            Toggle.fontStyle = FontStyle.Bold;
            Toggle.normal.textColor = Color.white;
            Toggle.margin = new RectOffset { left = 10, right = 10, top = 5, bottom = 5 };

            Button = new UnityEngine.GUIStyle(GUI.skin.button);
            Button.fontSize = 13;
            Button.normal.textColor = Color.white;
        }

        private static Texture2D MakeRoundedRect(int width, int height, int radius, int border, Color fillColor, Color borderColor)
        {
            Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
            Color[] pixels = new Color[width * height];

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    float dx = 0;
                    float dy = 0;

                    if (x < radius) dx = radius - x;
                    else if (x >= width - radius) dx = x - (width - radius - 1);

                    if (y < radius) dy = radius - y;
                    else if (y >= height - radius) dy = y - (height - radius - 1);

                    float dist = dx * dx + dy * dy;

                    if (dist <= radius * radius)
                    {
                        if (dist >= (radius - border) * (radius - border))
                        {
                            pixels[y * width + x] = borderColor;
                        }
                        else
                        {
                            pixels[y * width + x] = fillColor;
                        }
                    }
                    else
                    {
                        pixels[y * width + x] = Color.clear;
                    }
                }
            }

            tex.SetPixels(pixels);
            tex.Apply();
            tex.hideFlags = HideFlags.HideAndDontSave;
            return tex;
        }
    }
}