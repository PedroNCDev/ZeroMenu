using System;
using System.Text.RegularExpressions;
using HarmonyLib;
using TMPro;
using UnityEngine;
using AmongUs.Data;
using AmongUs.QuickChat;
using Hazel; 

namespace ZeroMenu.Functions
{
    public static class Chat
    {
        public static bool EnableChat = false;
        public static bool BypassUrlBlock = false;
        public static bool LowerRateLimits = false;
        public static bool DarkMode = false;
        public static bool UnlockCharacters = false;
        public static bool LongerMessages = false;
        public static bool UnlockClipboard = false;
        public static bool ColorAsPlayer = false;
        public static bool ChangeChatColor = false;
        public static string MenuChatColor = "";

        [HarmonyPatch(typeof(ChatController), "Update")]
        public static class ChatController_Update
        {
            public static void Postfix(ChatController __instance)
            {
                if (__instance.freeChatField != null && __instance.freeChatField.textArea != null)
                {
                    __instance.freeChatField.textArea.characterLimit = LongerMessages ? 120 : 100;
                }
            }
        }


        public static void SendQuickChatById(int id)
     
    {
        try
        {
            if (PlayerControl.LocalPlayer == null) return;

          
            var writer = AmongUsClient.Instance.StartRpcImmediately(
                PlayerControl.LocalPlayer.NetId,
                (byte)33, 
                SendOption.Reliable,
                -1
            );
            writer.Write((byte)3);
            writer.Write((ushort)78);
            writer.Write((byte)1);
            writer.Write((byte)2);
            writer.Write((ushort)id); 
            AmongUsClient.Instance.FinishRpcImmediately(writer);

           
            var selfWriter = AmongUsClient.Instance.StartRpcImmediately(
                PlayerControl.LocalPlayer.NetId,
                (byte)33,
                SendOption.Reliable,
                (int)PlayerControl.LocalPlayer.OwnerId
            );
            selfWriter.Write((byte)3);
            selfWriter.Write((ushort)78);
            selfWriter.Write((byte)1);
            selfWriter.Write((byte)2);
            selfWriter.Write((ushort)id);
            AmongUsClient.Instance.FinishRpcImmediately(selfWriter);
        }
          catch (Exception ex)
           {
             Debug.LogError("error en sendquickchatbyid: " + ex.Message);
           }
    }
        [HarmonyPatch(typeof(ChatController), "SendChat")]
        public static class ChatController_SendChat
        {
            public static void Postfix(ChatController __instance)
            {
                if (!LowerRateLimits) return;
                if (__instance.timeSinceLastMessage == 0f)
                {
                    __instance.timeSinceLastMessage += 1f;
                }
            }
        }

        [HarmonyPatch(typeof(ChatController), "SendFreeChat")]
        public static class ChatController_SendFreeChat
        {
            public static bool Prefix(ChatController __instance)
            {
                if (!BypassUrlBlock) return true;

                string text = __instance.freeChatField.Text;
                string modifiedText = CensorUrlsAndEmails(text);

                ChatController.Logger.Debug("SendFreeChat () :: Sending message: '" + modifiedText + "'", null);
                PlayerControl.LocalPlayer.RpcSendChat(modifiedText);

                return false;
            }

            private static string CensorUrlsAndEmails(string text)
            {
                string pattern = @"(http[s]?://)?([a-zA-Z0-9-]+\.)+[a-zA-Z]{2,6}(/[\w-./?%&=]*)?|([a-zA-Z0-9_.+-]+@[a-zA-Z0-9-]+\.[a-zA-Z0-9-.]+)";
                Regex regex = new Regex(pattern);
                return regex.Replace(text, match =>
                {
                    var censored = match.Value;
                    censored = censored.Replace('.', ',');
                    return censored;
                });
            }
        }

        [HarmonyPatch(typeof(ChatController), "AddChat")]
        public static class ChatColorGlobalPatch
        {
            [HarmonyPriority(600)]
            public static void Prefix(PlayerControl sourcePlayer, ref string chatText)
            {
                if (ChangeChatColor && ColorAsPlayer)
                {
                    ChangeChatColor = false;
                    ColorAsPlayer = false;
                }
                else if (ChangeChatColor && !string.IsNullOrEmpty(MenuChatColor))
                {
                    chatText = $"<color={MenuChatColor}>{chatText}</color>";
                }
                else if (ColorAsPlayer && sourcePlayer != null && sourcePlayer.Data != null)
                {
                    if (sourcePlayer.Data.DefaultOutfit != null)
                    {
                        int colorId = sourcePlayer.Data.DefaultOutfit.ColorId;
                        if (colorId >= 0 && colorId < Palette.PlayerColors.Length)
                        {
                            Color pColor = Palette.PlayerColors[colorId];
                            string hex = "#" + ColorUtility.ToHtmlStringRGB(pColor);
                            chatText = $"<color={hex}>{chatText}</color>";
                        }
                    }
                }
            }
        }

        [HarmonyPatch(typeof(ChatController), "Update")]
        public static class DarkMode_ChatControllerPatch
        {
            [HarmonyPostfix]
            public static void Postfix(ChatController __instance)
            {
                if (!DarkMode) return;
                try
                {
                    if (__instance.freeChatField != null && __instance.freeChatField.background != null)
                    {
                        __instance.freeChatField.background.color = new Color32(40, 40, 40, byte.MaxValue);
                    }
                    if (__instance.freeChatField != null && __instance.freeChatField.textArea != null && __instance.freeChatField.textArea.outputText != null)
                    {
                        __instance.freeChatField.textArea.outputText.color = Color.white;
                    }
                    if (__instance.quickChatField != null && __instance.quickChatField.background != null)
                    {
                        __instance.quickChatField.background.color = new Color32(40, 40, 40, byte.MaxValue);
                    }
                    if (__instance.quickChatField != null && __instance.quickChatField.text != null)
                    {
                        __instance.quickChatField.text.color = Color.white;
                    }
                }
                catch { }
            }
        }

        [HarmonyPatch(typeof(ChatBubble), "SetText")]
        public static class DarkMode_ChatBubblePatch
        {
            [HarmonyPrefix]
            public static void Prefix(ChatBubble __instance, ref string chatText)
            {
                if (!DarkMode) return;
                try
                {
                    Transform backgroundTransform = __instance.transform.Find("Background");
                    SpriteRenderer sprite = backgroundTransform != null ? backgroundTransform.GetComponent<SpriteRenderer>() : null;
                    if (sprite != null)
                    {
                        sprite.color = new Color(0.15f, 0.15f, 0.15f, 1f);
                    }
                    
                    if (!chatText.Contains("░") && !chatText.Contains("▄") && !chatText.Contains("█") && !chatText.Contains("▌") && !chatText.Contains("▒"))
                    {
                        chatText = "<color=#FFFFFF>" + chatText.TrimEnd('\0') + "</color>";
                    }
                }
                catch { }
            }
        }
    }
}
