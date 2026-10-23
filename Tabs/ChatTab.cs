using System;
using UnityEngine;
using ZeroMenu.Functions;
using ZeroMenu.UI;
using Il2CppInterop.Runtime.InteropTypes.Arrays;

namespace ZeroMenu.Tabs
{
    public class ChatTab : ITab
    {
        public string Name => "Chat";
        private Vector2 _scrollPosition = Vector2.zero;
        private TextField _chatColorField;
        private bool _initialized = false;

        public void Draw()
        {
            if (!_initialized)
            {
                _chatColorField = new TextField(Chat.MenuChatColor ?? "");
                _initialized = true;
            }

            _scrollPosition = SmoothScrollView.Begin(_scrollPosition, (Il2CppReferenceArray<GUILayoutOption>)null);
            GUILayout.BeginVertical(new Il2CppReferenceArray<GUILayoutOption>(new GUILayoutOption[] { GUILayout.Width((float)MainUI.windowWidth * 0.425f) }));

            Chat.EnableChat = CustomToggle.Draw(Chat.EnableChat, "Enable Chat", "EnableChat");
            Chat.BypassUrlBlock = CustomToggle.Draw(Chat.BypassUrlBlock, "Bypass URL Block", "BypassUrlBlock");
            Chat.LowerRateLimits = CustomToggle.Draw(Chat.LowerRateLimits, "Lower Rate Limits", "LowerRateLimits");
            Chat.DarkMode = CustomToggle.Draw(Chat.DarkMode, "Dark Mode", "DarkMode");

            GUILayout.Space(10f);
            GUILayout.Label("Textbox", MenuStyles.BoldLabel, new Il2CppReferenceArray<GUILayoutOption>(0));
            
            Chat.UnlockCharacters = CustomToggle.Draw(Chat.UnlockCharacters, "Unlock Extra Characters", "UnlockCharacters");
            Chat.LongerMessages = CustomToggle.Draw(Chat.LongerMessages, "Allow Longer Messages", "LongerMessages");
            Chat.UnlockClipboard = CustomToggle.Draw(Chat.UnlockClipboard, "Unlock Clipboard", "UnlockClipboard");

            GUILayout.Space(10f);
            GUILayout.Label("Chat Color", MenuStyles.BoldLabel, new Il2CppReferenceArray<GUILayoutOption>(0));
            
            Chat.ColorAsPlayer = CustomToggle.Draw(Chat.ColorAsPlayer, "Color as Player", "ColorAsPlayer");
            Chat.ChangeChatColor = CustomToggle.Draw(Chat.ChangeChatColor, "Enable Custom Chat Color", "ChangeChatColor");
            
            GUILayout.BeginHorizontal(new Il2CppReferenceArray<GUILayoutOption>(0));
            GUILayout.Label("HTML Color:", MenuStyles.Label, new Il2CppReferenceArray<GUILayoutOption>(new GUILayoutOption[] { GUILayout.Width(90f) }));
            
            _chatColorField.Draw(150, 20);
            
            if (CustomButton.Draw("Save")) 
            {
                Chat.MenuChatColor = _chatColorField.Content;
                ZeroMenu.Plugin.notifications.Send("Chat", "Color saved.", 3f);
            }
            GUILayout.EndHorizontal();

            GUILayout.Space(15f);
            GUILayout.Label("Coloured Chat (send emotes)", MenuStyles.BoldLabel, new Il2CppReferenceArray<GUILayoutOption>(0));
            GUILayout.Label("Each button sends a coloured quick-chat entry to the lobby.", new GUIStyle(GUI.skin.label) { fontSize = 10, normal = { textColor = Color.gray } });
            GUILayout.Space(5f);

            var quickChatButtons = new (string label, Color color, int id)[]
            {
                ("1",  new Color(0.8f, 0.2f, 0.2f), 1912),
                ("2",  new Color(0.2f, 0.6f, 0.8f), 197),
                ("3",  new Color(0.2f, 0.7f, 0.5f), 155),
                ("4",  new Color(0.6f, 0.4f, 0.8f), 156),
                ("5",  new Color(0.8f, 0.2f, 0.2f), 1913),
                ("6",  new Color(0.2f, 0.6f, 0.8f), 198),
                ("7",  new Color(0.2f, 0.7f, 0.5f), 157),
                ("8",  new Color(0.6f, 0.4f, 0.8f), 158),
                ("9",  new Color(0.8f, 0.2f, 0.2f), 1914),
                ("10", new Color(0.2f, 0.6f, 0.8f), 199),
                ("11", new Color(0.2f, 0.7f, 0.5f), 159),
                ("12", new Color(0.6f, 0.4f, 0.8f), 160),
                ("13", new Color(0.8f, 0.2f, 0.2f), 1915),
                ("14", new Color(0.2f, 0.6f, 0.8f), 200),
                ("15", new Color(0.2f, 0.7f, 0.5f), 161)
            };

            int currentColumn = 0;
            GUILayout.BeginHorizontal(new Il2CppReferenceArray<GUILayoutOption>(0));

            foreach (var btn in quickChatButtons)
            {
                if (currentColumn >= 5)
                {
                    GUILayout.EndHorizontal();
                    GUILayout.BeginHorizontal(new Il2CppReferenceArray<GUILayoutOption>(0));
                    currentColumn = 0;
                }

                var defaultBackground = GUI.backgroundColor;
                GUI.backgroundColor = btn.color;

                if (GUILayout.Button(btn.label, GUILayout.Width(40), GUILayout.Height(30)))
                {
                    ColouredChat.SendQuickChatById(btn.id);
                }

                GUI.backgroundColor = defaultBackground;
                GUILayout.Space(3f);
                currentColumn++;
            }

            GUILayout.EndHorizontal();

            GUILayout.EndVertical();
            GUILayout.EndScrollView();
        }
    }
}

