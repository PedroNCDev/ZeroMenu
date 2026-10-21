using System;
using UnityEngine;

namespace ZeroMenu.UI
{
    public class Notification
    {
        public readonly string title;
        public readonly string message;
        public readonly float ttl;
        public float lifetime;
        public float spawnTime;
        public float releaseTime = -1f;

        public Vector2 visualOffset = Vector2.zero;
        public Vector2 targetOffset = Vector2.zero;
        public bool isDragging = false;

        public Notification(string title, string message, float ttl)
        {
            this.title = title;
            this.message = message;
            this.ttl = ttl;
            this.lifetime = 0f;
            this.spawnTime = Time.time;
        }

        public bool HasExpired
        {
            get 
            { 
                if (releaseTime > 0f && Time.time - releaseTime >= 0.1f) return true;
                return this.lifetime > this.ttl; 
            }
        }
    }
}