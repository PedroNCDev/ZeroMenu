using System;
using System.Collections.Generic;
using UnityEngine;

namespace ZeroMenu.UI
{
    public class NotificationManager : MonoBehaviour
    {
        public static Vector2 BoxSize => new Vector2(325f, 90f);
        public static Vector2 BoxHeaderSize => new Vector2(BoxSize.x, 25f);
        public static Vector2 BoxContentPadding => new Vector2(10f, 0f);
        public static Vector2 BoxContentSize => new Vector2(BoxSize.x - BoxContentPadding.x, BoxSize.y - BoxHeaderSize.y - 4f);

        public List<Notification> notifications = new List<Notification>();
        public bool DisableNotifications = false;

        private GUIStyle _boxStyle;
        private GUIStyle _boxStyleDragging;
        private GUIStyle _titleStyle;
        private GUIStyle _messageStyle;
        private static GUIStyle _barBgStyle;
        private static GUIStyle _barFillStyle;

        private void EnsureBarStyles()
        {
            if (_barBgStyle != null) return;

            Texture2D bgTex = RoundedTexture.Get(2, new Color(0.2f, 0.2f, 0.2f), Color.clear, 0);
            _barBgStyle = new GUIStyle();
            _barBgStyle.normal.background = bgTex;
            _barBgStyle.border = new RectOffset { left = 2, right = 2, top = 2, bottom = 2 };

            Texture2D fillTex = new Texture2D(128, 1);
            for (int x = 0; x < 128; x++)
            {
                float t = (float)x / 127f;
                float a = 1f;
                if (t > 0.95f) a = 1f - ((t - 0.95f) / 0.05f);
                fillTex.SetPixel(x, 0, new Color(0.65f, 0.75f, 0.85f, a));
            }
            fillTex.Apply();
            fillTex.wrapMode = TextureWrapMode.Clamp;
            fillTex.hideFlags = HideFlags.HideAndDontSave;

            _barFillStyle = new GUIStyle();
            _barFillStyle.normal.background = fillTex;
            _barFillStyle.border = new RectOffset();
        }

        public void Update()
        {
            int notificationsCount = Math.Min(this.GetMaxNotifications(), this.notifications.Count);
            for (int i = 0; i < notificationsCount; i++)
            {
                Notification notification = this.notifications[i];
                notification.lifetime += Time.deltaTime;

                if (!notification.isDragging && notification.releaseTime < 0f)
                {
                    notification.visualOffset = Vector2.Lerp(notification.visualOffset, notification.targetOffset, Time.deltaTime * 15f);
                }

                if (notification.HasExpired)
                {
                    this.notifications.RemoveAt(i);
                    i--;
                    notificationsCount--;
                }
            }
        }

        private void InitStyles()
        {
            if (_boxStyle != null) return;
            EnsureBarStyles();

            _boxStyle = new GUIStyle();
            Texture2D bg = RoundedTexture.Get(12, new Color(0.06f, 0.06f, 0.08f, 0.98f), new Color(0.2f, 0.2f, 0.2f), 2);
            _boxStyle.normal.background = bg;
            _boxStyle.border = new RectOffset { left = 12, right = 12, top = 12, bottom = 12 };
            _boxStyle.padding = new RectOffset();

            _boxStyleDragging = new GUIStyle(_boxStyle);
            Texture2D bgDrag = RoundedTexture.Get(12, new Color(0.06f, 0.06f, 0.08f, 0.98f), new Color(0.65f, 0.75f, 0.85f), 2);
            _boxStyleDragging.normal.background = bgDrag;

            _titleStyle = new GUIStyle(GUI.skin.label);
            _titleStyle.fontSize = 14;
            _titleStyle.fontStyle = FontStyle.Bold;
            _titleStyle.normal.textColor = Color.white;
            _titleStyle.alignment = TextAnchor.MiddleCenter;

            _messageStyle = new GUIStyle(GUI.skin.label);
            _messageStyle.fontSize = 12;
            _messageStyle.normal.textColor = new Color(0.8f, 0.8f, 0.8f);
            _messageStyle.alignment = TextAnchor.MiddleCenter;
            _messageStyle.wordWrap = true;
        }

        public void OnGUI()
        {
            if (DisableNotifications) return;
            InitStyles();

            int notificationsCount = Math.Min(this.GetMaxNotifications(), this.notifications.Count);
            for (int i = 0; i < notificationsCount; i++)
            {
                this.RenderNotification(i, this.notifications[i]);
            }
        }

        private void RenderNotification(int position, Notification notification)
        {
            float timeElapsed = Time.time - notification.spawnTime;
            float animProgress = Mathf.Clamp01(timeElapsed / 0.7f);
            float eased = animProgress == 1f ? 1f : 1f - Mathf.Pow(2f, -10f * animProgress);

            float baseX = (Screen.width - BoxSize.x) / 2f;
            float baseY = 20f + (position * (BoxSize.y + 10f));

            float currentX = baseX + notification.visualOffset.x;
            float currentY = baseY + notification.visualOffset.y;

            if (timeElapsed < 0.7f)
            {
                currentY -= (1f - eased) * (BoxSize.y + 50f);
            }

            Rect boxRect = new Rect(currentX, currentY, BoxSize.x, BoxSize.y);

            Event e = Event.current;
            if (e.type == EventType.MouseDown && e.button == 0 && boxRect.Contains(e.mousePosition) && notification.releaseTime < 0f)
            {
                notification.isDragging = true;
                e.Use();
            }
            else if (e.type == EventType.MouseDrag && e.button == 0 && notification.isDragging)
            {
                notification.targetOffset += e.delta;
                notification.visualOffset = notification.targetOffset;
                e.Use();
            }
            else if (e.type == EventType.MouseUp && e.button == 0 && notification.isDragging)
            {
                notification.isDragging = false;
                notification.releaseTime = Time.time;
                e.Use();
            }

            float timeRemaining = notification.ttl - notification.lifetime;
            float alpha = 1f;

            if (notification.releaseTime > 0f)
            {
                float timeSinceRelease = Time.time - notification.releaseTime;
                alpha = Mathf.Clamp01(1f - (timeSinceRelease / 0.1f));
            }
            else if (timeRemaining < 0.2f)
            {
                alpha = Mathf.Clamp01(timeRemaining / 0.2f);
            }

            GUI.color = new Color(1f, 1f, 1f, alpha);

            GUIStyle currentStyle = notification.isDragging ? _boxStyleDragging : _boxStyle;
            GUI.Box(boxRect, "", currentStyle);

            Rect titleRect = new Rect(currentX, currentY + 5f, BoxSize.x, BoxHeaderSize.y);
            GUI.Label(titleRect, notification.title, _titleStyle);

            Rect msgRect = new Rect(currentX + BoxContentPadding.x, currentY + BoxHeaderSize.y, BoxContentSize.x, BoxContentSize.y);
            GUI.Label(msgRect, notification.message, _messageStyle);

            float barHeight = 4f;
            float barPadding = 15f;
            float barWidth = BoxSize.x - (barPadding * 2);
            float barX = currentX + barPadding;
            float barY = currentY + BoxSize.y - barHeight - 8f;
            float timeLeftPct = Mathf.Clamp01(timeRemaining / notification.ttl);

            Rect bgBarRect = new Rect(barX, barY, barWidth, barHeight);
            GUI.color = new Color(0.3f, 0.3f, 0.3f, alpha);
            GUI.Box(bgBarRect, GUIContent.none, _barBgStyle);

            if (timeLeftPct > 0.01f)
            {
                Rect fillBarRect = new Rect(barX, barY, barWidth * timeLeftPct, barHeight);
                GUI.color = new Color(1f, 1f, 1f, alpha);
                GUI.Box(fillBarRect, GUIContent.none, _barFillStyle);
            }

            GUI.color = Color.white;
        }

        public int GetMaxNotifications() => Screen.height / 2 / (int)BoxSize.y;

        public void Send(string title, string message, float ttl = 10f)
        {
            if (!DisableNotifications)
            {
                Notification notification = new Notification(title, message, ttl);
                this.notifications.Add(notification);
            }
        }

        public void ClearNotifications() => this.notifications.Clear();
    }
}