using UnityEngine;
using System.Collections.Generic;
using System.IO;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using ZeroMenu.Tabs;
using ZeroMenu.UI;
using ZeroMenu.Functions;

namespace ZeroMenu
{
    public class MainUI : MonoBehaviour
    {
        private static readonly BepInEx.Logging.ManualLogSource Log = BepInEx.Logging.Logger.CreateLogSource("ZeroMenu");

        private bool isMenuOpen = false;
        private Rect _windowRect;
        private Vector2 _targetWindowPos;
        private Vector2 _windowVelocity;
        
        private List<ITab> _tabs = new List<ITab>();
        private int _selectedTab = 0;

        public static int windowWidth = 800;
        public static int windowHeight = 550;

        private Vector2 _tabScrollPosition = Vector2.zero;
        private float _tabLinePosition = 0f;
        private float _tabLineVelocity = 0f;

        private Texture2D _backgroundTexture;
        private GUIStyle _bgStyle;

        private GUIStyle _poleStyle;
        private GUIStyle _poleGlowStyle;
        private float _poleScale = 1f;
        private float _poleScaleVelocity = 0f;
        private bool _isDraggingPole = false;
        private Vector2 _poleDragOffset = Vector2.zero;

        private const float PoleWidth = 220f;
        private const float PoleHeight = 8f;

        private void Start()
        {
            _windowRect = new Rect(100, 100, windowWidth, windowHeight);
            _targetWindowPos = _windowRect.position;
            
            LoadBackground();
            InitPoleStyles();

            _tabs.Add(new AboutTab());
            _tabs.Add(new SelfTab());
            _tabs.Add(new ESPTab());
            _tabs.Add(new MovementTab());
            _tabs.Add(new RolesTab());
            _tabs.Add(new PlayersTab());
            _tabs.Add(new SabotageTab());
            _tabs.Add(new ShipTab());
            _tabs.Add(new ChatTab());
            _tabs.Add(new AnimationsTab());
            _tabs.Add(new HostOnlyTab());
            _tabs.Add(new PassiveTab());
            _tabs.Add(new TrollTab());
            _tabs.Add(new TuffTab());
            _tabs.Add(new SpoofTab());
            _tabs.Add(new ProtectionsTab());
            _tabs.Add(new AnticheatTab());
            _tabs.Add(new ConfigTab());
        }

        private void InitPoleStyles()
        {
            _poleStyle = new GUIStyle();
            _poleStyle.normal.background = RoundedTexture.Get(4, Color.white, Color.white, 0);
            _poleStyle.hover.background = _poleStyle.normal.background;

            _poleGlowStyle = new GUIStyle();
            _poleGlowStyle.normal.background = CreateGlowTexture(64, 16, 4);
            _poleGlowStyle.border = new RectOffset { left = 4, right = 4, top = 4, bottom = 4 };
        }

        private Texture2D CreateGlowTexture(int width, int height, int fadeRadius)
        {
            Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
            Color[] pixels = new Color[width * height];
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    float fx = Mathf.Clamp01((float)Mathf.Min(x, width - 1 - x) / fadeRadius);
                    float fy = Mathf.Clamp01((float)Mathf.Min(y, height - 1 - y) / fadeRadius);
                    float alpha = Mathf.Min(fx, fy);
                    alpha = Mathf.SmoothStep(0f, 1f, alpha);
                    pixels[y * width + x] = new Color(1f, 1f, 1f, alpha);
                }
            }
            tex.SetPixels(pixels);
            tex.Apply();
            tex.hideFlags = HideFlags.HideAndDontSave;
            return tex;
        }

        private void LoadBackground()
        {
            try
            {
                var assembly = typeof(MainUI).Assembly;
                using var stream = assembly.GetManifestResourceStream("ZeroMenu.zerobackground.png");
                if (stream != null)
                {
                    using var ms = new MemoryStream();
                    stream.CopyTo(ms);
                    _backgroundTexture = new Texture2D(2, 2, TextureFormat.RGBA32, false);
                    _backgroundTexture.LoadImage(ms.ToArray());
                    
                    Color[] pixels = _backgroundTexture.GetPixels();
                    for (int i = 0; i < pixels.Length; i++)
                    {
                        pixels[i].a *= 0.30f;
                    }
                    _backgroundTexture.SetPixels(pixels);
                    _backgroundTexture.Apply();

                    _backgroundTexture.hideFlags = HideFlags.HideAndDontSave;
                }
            }
            catch (System.Exception e)
            {
                Log.LogError("Failed to load background: " + e.Message);
            }
        }

        private void Update()
        {
            TuffTab.Update();

            if (HudManager.InstanceExists && HudManager.Instance.Chat && HudManager.Instance.Chat.IsOpenOrOpening) return;

            if (Input.GetKeyDown(Config.MenuKey))
            {
                isMenuOpen = !isMenuOpen;
                if (isMenuOpen && Config.OpenOnMouse)
                {
                    Vector2 mousePos = Input.mousePosition;
                    _targetWindowPos = new Vector2(mousePos.x, Screen.height - mousePos.y);
                    _windowRect.position = _targetWindowPos;
                }
                ClickRipple.Clear();
            }

            if (isMenuOpen)
            {
                if (_isDraggingPole)
                {
                    _windowRect.position = Vector2.SmoothDamp(_windowRect.position, _targetWindowPos, ref _windowVelocity, 0.2f);
                }
                else
                {
                    _targetWindowPos = _windowRect.position;
                    _windowVelocity = Vector2.zero;
                }
                
                _poleScale = Mathf.SmoothDamp(_poleScale, _isDraggingPole ? 1.05f : 1.0f, ref _poleScaleVelocity, 0.1f);
            }

            float tabWidth = 75f;
            float tabSpacing = 5f;
            float targetLineX = _selectedTab * (tabWidth + tabSpacing);

            if (Mathf.Abs(_tabLinePosition - targetLineX) > 0.1f)
            {
                _tabLinePosition = Mathf.SmoothDamp(_tabLinePosition, targetLineX, ref _tabLineVelocity, 0.2f, Mathf.Infinity, Time.unscaledDeltaTime);
            }
            else
            {
                _tabLinePosition = targetLineX;
            }
        }

        private void OnGUI()
        {
            if (!isMenuOpen) return;

            MenuStyles.Init();

            GUI.color = Color.white;
            GUI.backgroundColor = Color.white;
            GUI.contentColor = Color.white;

            _windowRect = GUI.Window(0, _windowRect, (GUI.WindowFunction)DrawWindow, "", MenuStyles.Window);

            DrawPole();
        }

        private void DrawPole()
        {
            float scaledWidth = PoleWidth * _poleScale;
            float scaledHeight = PoleHeight * _poleScale;
            
            float xPos = _windowRect.x + (windowWidth / 2f) - (scaledWidth / 2f);
            float yPos = _windowRect.y + windowHeight + 10f;

            Rect poleRect = new Rect(xPos, yPos, scaledWidth, scaledHeight);
            Rect glowRect = new Rect(poleRect.x - 4f, poleRect.y - 4f, poleRect.width + 8f, poleRect.height + 8f);

            Event e = Event.current;

            if (e.type == EventType.MouseDown && e.button == 0 && poleRect.Contains(e.mousePosition))
            {
                _isDraggingPole = true;
                _poleDragOffset = e.mousePosition - _windowRect.position;
                e.Use();
            }
            else if (e.type == EventType.MouseDrag && _isDraggingPole)
            {
                _targetWindowPos = e.mousePosition - _poleDragOffset;
                e.Use();
            }
            else if (e.type == EventType.MouseUp && e.button == 0 && _isDraggingPole)
            {
                _isDraggingPole = false;
                e.Use();
            }

            GUI.Box(glowRect, GUIContent.none, _poleGlowStyle);
            GUI.Box(poleRect, GUIContent.none, _poleStyle);
        }

        private void DrawWindow(int windowID)
        {
            if (_bgStyle == null && _backgroundTexture != null)
            {
                _bgStyle = new GUIStyle();
                _bgStyle.normal.background = _backgroundTexture;
            }

            if (_bgStyle != null)
            {
                GUI.Box(new Rect(0, 0, windowWidth, windowHeight), GUIContent.none, _bgStyle);
            }

            if (Event.current.type == EventType.MouseDown && Event.current.button == 0)
            {
                ClickRipple.Spawn(Event.current.mousePosition);
            }

            GUILayout.Space(10f);
            GUILayout.Label("ZeroMenu v1.0.8", MenuStyles.Title, new Il2CppReferenceArray<GUILayoutOption>(0));
            GUILayout.Space(10f);

            float tabWidth = 75f;
            float tabSpacing = 5f;
            float viewWidth = windowWidth - 45f;
            float totalTabsWidth = _tabs.Count * (tabWidth + tabSpacing);

            GUILayout.BeginHorizontal(new Il2CppReferenceArray<GUILayoutOption>(0));
            GUILayout.Space(15f);

            Rect tabViewRect = GUILayoutUtility.GetRect(viewWidth, 35f);

            GUILayout.EndHorizontal();

            if (Event.current.type == EventType.ScrollWheel && tabViewRect.Contains(Event.current.mousePosition))
            {
                _tabScrollPosition.x += Event.current.delta.y * 30f;
                Event.current.Use();
            }

            if (Event.current.type == EventType.MouseDrag && tabViewRect.Contains(Event.current.mousePosition))
            {
                _tabScrollPosition.x += Event.current.delta.x;
                Event.current.Use();
            }

            float minX = Mathf.Min(0f, viewWidth - totalTabsWidth);
            _tabScrollPosition.x = Mathf.Clamp(_tabScrollPosition.x, minX, 0f);

            GUI.BeginGroup(tabViewRect);

            for (int i = 0; i < _tabs.Count; i++)
            {
                float xPos = _tabScrollPosition.x + i * (tabWidth + tabSpacing);
                Rect btnRect = new Rect(xPos, 0f, tabWidth, 25f);

                var style = MenuStyles.TabInactive;
                if (i == _selectedTab) style = MenuStyles.TabActive;
                else if (btnRect.Contains(Event.current.mousePosition)) style = MenuStyles.TabHover;

                if (GUI.Button(btnRect, _tabs[i].Name, style))
                {
                    _selectedTab = i;
                }
            }

            GUI.Box(new Rect(_tabScrollPosition.x + _tabLinePosition, 27f, tabWidth, 2f), "", MenuStyles.Line);

            GUI.EndGroup();

            GUILayout.Space(15f);

            GUILayout.BeginHorizontal(new Il2CppReferenceArray<GUILayoutOption>(0));
            GUILayout.Space(15f);

            GUILayout.BeginVertical(new Il2CppReferenceArray<GUILayoutOption>(new GUILayoutOption[] { GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true) }));

            if (_selectedTab >= 0 && _selectedTab < _tabs.Count)
            {
                _tabs[_selectedTab].Draw();
            }

            GUILayout.EndVertical();
            GUILayout.EndHorizontal();

            GUI.DragWindow(new Rect(0, 0, windowWidth, 45f));

            ClickRipple.Draw();
        }
    }
}
