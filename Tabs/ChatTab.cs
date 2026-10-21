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

            _scrollPosition = SmoothScrollView.Begin(_scrollPosition, new GUILayoutOption[0]);
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

            GUILayout.EndVertical();
            GUILayout.EndScrollView();
        }
    }
}