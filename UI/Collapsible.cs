using UnityEngine;
using Il2CppInterop.Runtime.InteropTypes.Arrays;

namespace ZeroMenu.UI
{
    public class CollapsibleSection
    {
        public string Title { get; set; }
        public bool Expanded { get; private set; }

        private static Texture2D _gradientTex;
        private static Texture2D _whiteTex;
        private static GUIStyle _headerStyle;
        private static GUIStyle _gradientBoxStyle;
        private static GUIStyle _whiteBoxStyle;
        
        private float _targetHeight = 0f;
        private float _currentHeight = 0f;
        private float _velocity = 0f;
        private const float AnimDuration = 0.2f;

        public CollapsibleSection(string title, bool defaultExpanded = false)
        {
            Title = title;
            Expanded = defaultExpanded;
            _targetHeight = defaultExpanded ? 1f : 0f;
            _currentHeight = _targetHeight;
        }

        private static void InitStyles()
        {
            if (_headerStyle == null)
            {
                _headerStyle = new GUIStyle(GUI.skin.label);
                _headerStyle.fontSize = 14;
                _headerStyle.fontStyle = FontStyle.Bold;
                _headerStyle.alignment = TextAnchor.MiddleLeft;
                _headerStyle.normal.textColor = Color.white;
                _headerStyle.padding = new RectOffset { left = 10, right = 10, top = 0, bottom = 0 };
            }
            
            if (_whiteTex == null)
            {
                _whiteTex = new Texture2D(1, 1);
                _whiteTex.SetPixel(0, 0, Color.white);
                _whiteTex.Apply();
                _whiteTex.hideFlags = HideFlags.HideAndDontSave;

                _whiteBoxStyle = new GUIStyle();
                _whiteBoxStyle.normal.background = _whiteTex;
                _whiteBoxStyle.border = new RectOffset { left = 0, right = 0, top = 0, bottom = 0 };
                _whiteBoxStyle.margin = new RectOffset { left = 0, right = 0, top = 0, bottom = 0 };
                _whiteBoxStyle.padding = new RectOffset { left = 0, right = 0, top = 0, bottom = 0 };
            }

            if (_gradientTex == null)
            {
                _gradientTex = new Texture2D(1, 64);
                for (int i = 0; i < 64; i++)
                {
                    float a = 1f - ((float)i / 63f); 
                    _gradientTex.SetPixel(0, i, new Color(1f, 1f, 1f, a));
                }
                _gradientTex.Apply();
                _gradientTex.hideFlags = HideFlags.HideAndDontSave;

                _gradientBoxStyle = new GUIStyle();
                _gradientBoxStyle.normal.background = _gradientTex;
                _gradientBoxStyle.border = new RectOffset { left = 0, right = 0, top = 0, bottom = 0 };
                _gradientBoxStyle.margin = new RectOffset { left = 0, right = 0, top = 0, bottom = 0 };
                _gradientBoxStyle.padding = new RectOffset { left = 0, right = 0, top = 0, bottom = 0 };
            }
        }

        public bool Draw()
        {
            InitStyles();

            Rect rect = GUILayoutUtility.GetRect(0, 30, GUILayout.ExpandWidth(true));

            Event e = Event.current;
            if (e.type == EventType.MouseDown && e.button == 0 && rect.Contains(e.mousePosition))
            {
                Expanded = !Expanded;
                _targetHeight = Expanded ? 1f : 0f;
                _velocity = 0f; 
                e.Use();
            }

            _currentHeight = Mathf.SmoothDamp(_currentHeight, _targetHeight, ref _velocity, AnimDuration);

            if (_currentHeight > 0.01f)
            {
                float drawHeight = rect.height * _currentHeight;
                Rect gradRect = new Rect(rect.x, rect.yMax - drawHeight, rect.width, drawHeight);
                GUI.Box(gradRect, GUIContent.none, _gradientBoxStyle);
            }

            GUI.Box(new Rect(rect.x, rect.y, rect.width, 1), GUIContent.none, _whiteBoxStyle);
            GUI.Box(new Rect(rect.x, rect.yMax - 1, rect.width, 1), GUIContent.none, _whiteBoxStyle);
            GUI.Box(new Rect(rect.x, rect.y, 1, rect.height), GUIContent.none, _whiteBoxStyle);
            GUI.Box(new Rect(rect.xMax - 1, rect.y, 1, rect.height), GUIContent.none, _whiteBoxStyle);

            GUI.Label(rect, Title, _headerStyle);

            string arrow = Expanded ? "▲" : "▼";
            GUI.Label(new Rect(rect.xMax - 20, rect.y, 20, rect.height), arrow, _headerStyle);

            return Expanded;
        }

        public bool DrawWithIndent(float indentWidth = 8f)
        {
            bool open = Draw();
            if (open)
            {
                GUILayout.BeginHorizontal(new Il2CppReferenceArray<GUILayoutOption>(0));
                GUILayout.Space(indentWidth);
                GUILayout.BeginVertical(new Il2CppReferenceArray<GUILayoutOption>(0));
            }
            return open;
        }

        public void EndIndent()
        {
            GUILayout.EndVertical();
            GUILayout.EndHorizontal();
        }
    }
}