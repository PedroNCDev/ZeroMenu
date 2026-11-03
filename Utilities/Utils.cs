using System;
using System.Collections.Generic;
using HarmonyLib;
using InnerNet;
using UnityEngine;

namespace ZeroMenu.Functions
{
    public static class DummyManager
    {
        public static readonly Dictionary<byte, List<PlayerControl>> PlayerDummies = new Dictionary<byte, List<PlayerControl>>();
        public static PlayerControl TrailTarget = null;
        public static float TrailInterval = 0.05f;
        private static float _lastTrailTime = 0f;

        private static readonly Dictionary<char, string[]> Font5x5 = new Dictionary<char, string[]>()
        {
            { 'A', new[] { "01110", "10001", "11111", "10001", "10001" } },
            { 'B', new[] { "11110", "10001", "11110", "10001", "11110" } },
            { 'C', new[] { "01111", "10000", "10000", "10000", "01111" } },
            { 'D', new[] { "11110", "10001", "10001", "10001", "11110" } },
            { 'E', new[] { "11111", "10000", "11110", "10000", "11111" } },
            { 'F', new[] { "11111", "10000", "11110", "10000", "10000" } },
            { 'G', new[] { "01111", "10000", "10011", "10001", "01111" } },
            { 'H', new[] { "10001", "10001", "11111", "10001", "10001" } },
            { 'I', new[] { "11111", "00100", "00100", "00100", "11111" } },
            { 'J', new[] { "00111", "00010", "00010", "10010", "01100" } },
            { 'K', new[] { "10001", "10010", "11100", "10010", "10001" } },
            { 'L', new[] { "10000", "10000", "10000", "10000", "11111" } },
            { 'M', new[] { "10001", "11011", "10101", "10001", "10001" } },
            { 'N', new[] { "10001", "11001", "10101", "10011", "10001" } },
            { 'O', new[] { "01110", "10001", "10001", "10001", "01110" } },
            { 'P', new[] { "11110", "10001", "11110", "10000", "10000" } },
            { 'Q', new[] { "01110", "10001", "10101", "10010", "01101" } },
            { 'R', new[] { "11110", "10001", "11110", "10010", "10001" } },
            { 'S', new[] { "01111", "10000", "01110", "00001", "11110" } },
            { 'T', new[] { "11111", "00100", "00100", "00100", "00100" } },
            { 'U', new[] { "10001", "10001", "10001", "10001", "01110" } },
            { 'V', new[] { "10001", "10001", "10001", "01010", "00100" } },
            { 'W', new[] { "10001", "10001", "10101", "11011", "10001" } },
            { 'X', new[] { "10001", "01010", "00100", "01010", "10001" } },
            { 'Y', new[] { "10001", "01010", "00100", "00100", "00100" } },
            { 'Z', new[] { "11111", "00010", "00100", "01000", "11111" } },
            { '0', new[] { "01110", "10011", "10101", "11001", "01110" } },
            { '1', new[] { "00100", "01100", "00100", "00100", "01110" } },
            { '2', new[] { "01110", "10001", "00110", "01000", "11111" } },
            { '3', new[] { "11111", "00010", "00110", "00001", "11110" } },
            { '4', new[] { "00010", "00110", "01010", "11111", "00010" } },
            { '5', new[] { "11111", "10000", "11110", "00001", "11110" } },
            { '6', new[] { "01110", "10000", "11110", "10001", "01110" } },
            { '7', new[] { "11111", "00001", "00010", "00100", "01000" } },
            { '8', new[] { "01110", "10001", "01110", "10001", "01110" } },
            { '9', new[] { "01110", "10001", "01111", "00001", "01110" } },
            { '?', new[] { "01110", "10001", "00110", "00000", "00100" } },
            { ' ', new[] { "00000", "00000", "00000", "00000", "00000" } }
        };

        public static void SpawnSingleDummy(Vector2 pos, byte playerId)
        {
            try
            {
                if (AmongUsClient.Instance == null || AmongUsClient.Instance.PlayerPrefab == null) return;

                PlayerControl dummy = UnityEngine.Object.Instantiate<PlayerControl>(
                    AmongUsClient.Instance.PlayerPrefab,
                    pos,
                    Quaternion.identity
                );
                
                dummy.PlayerId = playerId;
                AmongUsClient.Instance.Spawn(dummy, -2, 0);

                if (!PlayerDummies.ContainsKey(playerId))
                    PlayerDummies[playerId] = new List<PlayerControl>();
                
                PlayerDummies[playerId].Add(dummy);
            }
            catch { }
        }

        public static void SpawnText(string text, Vector2 origin, byte playerId)
        {
            ClearDummies(playerId);
            
            if (string.IsNullOrEmpty(text)) return;
            text = text.ToUpperInvariant();
            if (text.Length > 64) text = text.Substring(0, 64);

            float pixelSize = 0.4f;
            List<Vector2> positions = new List<Vector2>();

            int totalWidth = 0;
            foreach (char c in text)
            {
                if (c == ' ') totalWidth += 3;
                else totalWidth += 6;
            }

            float halfW = totalWidth * pixelSize * 0.5f;
            int currentX = 0;

            foreach (char c in text)
            {
                if (c == ' ')
                {
                    currentX += 3;
                    continue;
                }

                if (!Font5x5.TryGetValue(c, out string[] pattern))
                    pattern = Font5x5['?'];

                for (int row = 0; row < 5; row++)
                {
                    for (int col = 0; col < 5; col++)
                    {
                        if (pattern[row][col] == '1')
                        {
                            Vector2 pos = new Vector2(
                                origin.x - halfW + (currentX + col) * pixelSize,
                                origin.y + 1.5f - row * pixelSize
                            );
                            positions.Add(pos);
                        }
                    }
                }
                currentX += 6;
            }

            foreach (var pos in positions)
            {
                SpawnSingleDummy(pos, playerId);
            }
        }

        public static void ClearDummies(byte playerId)
        {
            if (!PlayerDummies.TryGetValue(playerId, out List<PlayerControl> dummies)) return;

            foreach (var dummy in dummies)
            {
                try
                {
                    if (dummy != null)
                    {
                        AmongUsClient.Instance.Despawn(dummy);
                        UnityEngine.Object.Destroy(dummy.gameObject);
                    }
                }
                catch { }
            }
            dummies.Clear();
            PlayerDummies.Remove(playerId);
        }

        [HarmonyPatch(typeof(HudManager), nameof(HudManager.Update))]
        public static class UpdatePatch
        {
            public static void Postfix()
            {
                if (TrailTarget == null || TrailTarget.Data == null || TrailTarget.Data.Disconnected)
                {
                    TrailTarget = null;
                    return;
                }

                if (Time.time - _lastTrailTime >= TrailInterval)
                {
                    _lastTrailTime = Time.time;
                    SpawnSingleDummy(TrailTarget.transform.position, TrailTarget.PlayerId);
                }
            }
        }
    }
}
