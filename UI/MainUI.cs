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
        private List<ITab> _tabs = new List<ITab>();
        private int _selectedTab = 0;

        public static int windowWidth = 800;
        public static int windowHeight = 550;

        private Vector2 _tabScrollPosition = Vector2.zero;
        private float _tabLinePosition = 0f;
        private float _tabLineVelocity = 0f;

        private Texture2D _backgroundTexture;
        private GUIStyle _bgStyle;

        private void Start()
        {
            _windowRect = new Rect(100, 100, windowWidth, windowHeight);
            LoadBackground();

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
                    _windowRect.position = new Vector2(mousePos.x, Screen.height - mousePos.y);
                }
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

            GUILayout.Space(10f);
            GUILayout.Label("ZeroMenu v1.0.4", MenuStyles.Title, new Il2CppReferenceArray<GUILayoutOption>(0));
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
        }
    }
}
