using System;
using System.Collections.Generic;
using HarmonyLib;
using TMPro;
using UnityEngine;
using AmongUs.GameOptions;

namespace ZeroMenu.Functions
{
    public static class ESP
    {
        public static bool SeeRoles = false;
        public static bool SeePlayerInfo = false;
        public static bool SeeGhosts = false;
        public static bool NoShadows = false;
        public static bool TaskArrows = false;
        public static bool RevealVotes = false;
        
        public static bool TracersCrew = false;
        public static bool TracersImps = false;
        public static bool TracersGhosts = false;
        public static bool TracersBodies = false;
        public static bool ColorBasedTracers = false;
        
        public static bool MapCrew = false;
        public static bool MapImps = false;
        public static bool MapGhosts = false;
        public static bool ColorBasedMap = false;

        public static bool ZoomOut = false;
        public static bool Freecam = false;
        private static bool _freecamActive = false;
        private static bool _resolutionChangeNeeded = false;

        public static List<HerePoint> HerePoints = new List<HerePoint>();
        public static bool MinimapActive;

        public static bool IsFullbrightActive()
        {
            if (NoShadows) return true;
            if (PlayerControl.LocalPlayer != null && PlayerControl.LocalPlayer.Data != null && PlayerControl.LocalPlayer.Data.IsDead) return true;
            if (Camera.main != null && Camera.main.orthographicSize > 3f) return true;
            if (Camera.main != null)
            {
                var follower = Camera.main.gameObject.GetComponent<FollowerCamera>();
                if (follower != null && follower.Target != PlayerControl.LocalPlayer) return true;
            }
            return false;
        }

        public static void PlayerNametags(PlayerPhysics playerPhysics)
        {
            try
            {
                if (playerPhysics == null || playerPhysics.myPlayer == null) return;
                playerPhysics.myPlayer.cosmetics.SetName(Utils.GetNameTag(playerPhysics.myPlayer.Data, playerPhysics.myPlayer.CurrentOutfit.PlayerName));
                if (SeeRoles && SeePlayerInfo) playerPhysics.myPlayer.cosmetics.nameText.transform.localPosition = new Vector3(0f, 0.186f, 0f);
                else if (SeeRoles || SeePlayerInfo) playerPhysics.myPlayer.cosmetics.nameText.transform.localPosition = new Vector3(0f, 0.093f, 0f);
                else playerPhysics.myPlayer.cosmetics.nameText.transform.localPosition = new Vector3(0f, 0f, 0f);
            } catch { }
        }

        public static void MeetingNametags(MeetingHud meetingHud)
        {
            try
            {
                if (meetingHud == null || meetingHud.playerStates == null) return;
                foreach (var playerState in meetingHud.playerStates)
                {
                    var data = GameData.Instance.GetPlayerById(playerState.PlayerId);
                    if (data == null || data.Disconnected) continue;

                    playerState.NameText.text = Utils.GetNameTag(data, data.DefaultOutfit.PlayerName);

                    if (SeeRoles && SeePlayerInfo)
                    {
                        playerState.NameText.transform.localPosition = new Vector3(0.33f, 0.08f, 0f);
                        playerState.NameText.transform.localScale = new Vector3(0.75f, 0.75f, 0.75f);
                    }
                    else if (SeeRoles || SeePlayerInfo)
                    {
                        playerState.NameText.transform.localPosition = new Vector3(0.3384f, 0.1125f, -0.1f);
                        playerState.NameText.transform.localScale = new Vector3(0.9f, 1f, 1f);
                    }
                    else
                    {
                        playerState.NameText.transform.localPosition = new Vector3(0.3384f, 0.0311f, -0.1f);
                        playerState.NameText.transform.localScale = new Vector3(0.9f, 1f, 1f);
                    }
                }
            } catch { }
        }

        public static void ChatNametags(ChatBubble chatBubble)
        {
            try
            {
                chatBubble.NameText.text = Utils.GetNameTag(chatBubble.playerInfo, chatBubble.NameText.text, true);
                chatBubble.NameText.ForceMeshUpdate(true, true);

                float nameHeight = chatBubble.NameText.preferredHeight;
                float textHeight = chatBubble.TextArea.preferredHeight;

                chatBubble.Background.size = new Vector2(5.52f, 0.2f + nameHeight + textHeight);
                chatBubble.MaskArea.size = chatBubble.Background.size - new Vector2(0f, 0.03f);
            } catch { }
        }

        public static void SeeGhostsCheat(PlayerPhysics playerPhysics)
        {
            try
            {
                if (playerPhysics == null || playerPhysics.myPlayer == null) return;
                if (playerPhysics.myPlayer.Data.IsDead && !PlayerControl.LocalPlayer.Data.IsDead)
                {
                    playerPhysics.myPlayer.Visible = SeeGhosts;
                }
            } catch { }
        }

        public static void DrawPlayerTracer(PlayerPhysics playerPhysics)
        {
            try
            {
                if (playerPhysics == null || playerPhysics.myPlayer == null) return;
                var color = Color.clear;
                if (!playerPhysics.myPlayer.Data.IsDead)
                {
                    if (TracersCrew && !playerPhysics.myPlayer.Data.Role.IsImpostor || TracersImps && playerPhysics.myPlayer.Data.Role.IsImpostor)
                    {
                        color = ColorBasedTracers ? playerPhysics.myPlayer.Data.Color : playerPhysics.myPlayer.Data.Role.TeamColor;
                    }
                }
                else
                {
                    if (TracersGhosts) color = ColorBasedTracers ? playerPhysics.myPlayer.Data.Color : Palette.White;
                }

                if (color != Color.clear) Utils.DrawTracer(playerPhysics.myPlayer.gameObject, PlayerControl.LocalPlayer.gameObject, color);
            } catch { }
        }

        public static void DrawBodyTracer(DeadBody deadBody)
        {
            var color = Color.clear;
            if (TracersBodies)
            {
                color = ColorBasedTracers ? GameData.Instance.GetPlayerById(deadBody.ParentId).Color : Color.yellow;
            }
            if (color != Color.clear) Utils.DrawTracer(deadBody.gameObject, PlayerControl.LocalPlayer.gameObject, color);
        }

        public static void ZoomOutCheat(HudManager hudManager)
        {
            if (ZoomOut)
            {
                if (hudManager == null || hudManager.Chat == null) return;
                if (hudManager.Chat.IsOpenOrOpening || MatchInfoGuide.Instance.IsActive || PlayerCustomizationMenu.Instance) return;

                _resolutionChangeNeeded = true;

                if (Input.GetAxis("Mouse ScrollWheel") < 0f)
                {
                    if (Camera.main != null) Camera.main.orthographicSize++;
                    if (hudManager.UICamera != null) hudManager.UICamera.orthographicSize++;
                    Utils.AdjustResolution();
                }
                else if (Input.GetAxis("Mouse ScrollWheel") > 0f)
                {
                    if (Camera.main != null && Camera.main.orthographicSize > 3f)
                    {
                        Camera.main.orthographicSize--;
                        if (hudManager.UICamera != null) hudManager.UICamera.orthographicSize--;
                        Utils.AdjustResolution();
                    }
                }
            }
            else
            {
                if (Camera.main != null) Camera.main.orthographicSize = 3f;
                if (hudManager != null && hudManager.UICamera != null) hudManager.UICamera.orthographicSize = 3f;
                if (_resolutionChangeNeeded)
                {
                    Utils.AdjustResolution();
                    _resolutionChangeNeeded = false;
                }
            }
        }

        public static void FreecamCheat()
        {
            if (Freecam)
            {
                if (Camera.main == null) return;
                if (!_freecamActive)
                {
                    var follower = Camera.main.gameObject.GetComponent<FollowerCamera>();
                    if (follower != null)
                    {
                        follower.enabled = false;
                        follower.Target = null;
                    }
                    _freecamActive = true;
                }
                PlayerControl.LocalPlayer.moveable = false;
                var movement = new Vector3(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"), 0.0f);
                Camera.main.transform.position = Camera.main.transform.position + movement * 10f * Time.deltaTime;
            }
            else
            {
                if (!_freecamActive) return;
                if (Camera.main == null) return;
                PlayerControl.LocalPlayer.moveable = true;
                var follower = Camera.main.gameObject.GetComponent<FollowerCamera>();
                if (follower != null)
                {
                    follower.enabled = true;
                    follower.SetTarget(PlayerControl.LocalPlayer);
                }
                _freecamActive = false;
            }
        }

        [HarmonyPatch(typeof(HudManager), "Update")]
        public static class HudManager_Update
        {
            public static void Postfix(HudManager __instance)
            {
                if (__instance == null || __instance.ShadowQuad == null) return;
                __instance.ShadowQuad.gameObject.SetActive(!IsFullbrightActive());
                ZoomOutCheat(__instance);
                FreecamCheat();
            }
        }

        [HarmonyPatch(typeof(PlayerPhysics), "LateUpdate")]
        public static class PlayerPhysics_LateUpdate
        {
            public static void Postfix(PlayerPhysics __instance)
            {
                PlayerNametags(__instance);
                SeeGhostsCheat(__instance);
                DrawPlayerTracer(__instance);

                var bodyObjects = GameObject.FindGameObjectsWithTag("DeadBody");
                foreach (var bodyObject in bodyObjects)
                {
                    var deadBody = bodyObject.GetComponent<DeadBody>();
                    if (deadBody != null) DrawBodyTracer(deadBody);
                }
            }
        }

        [HarmonyPatch(typeof(MeetingHud), "Update")]
        public static class MeetingHud_Update
        {
            public static void Postfix(MeetingHud __instance)
            {
                MeetingNametags(__instance);
            }
        }

        [HarmonyPatch(typeof(ChatBubble), "SetName")]
        public static class ChatBubble_SetName
        {
            public static void Postfix(ChatBubble __instance)
            {
                ChatNametags(__instance);
            }
        }

        [HarmonyPatch(typeof(LogicOptions), "GetAnonymousVotes")]
        public static class LogicOptions_GetAnonymousVotes
        {
            public static void Postfix(ref bool __result)
            {
                if (RevealVotes) __result = false;
            }
        }

        [HarmonyPatch(typeof(LogicOptionsNormal), "GetAnonymousVotes")]
        public static class LogicOptionsNormal_GetAnonymousVotes
        {
            public static void Postfix(ref bool __result)
            {
                if (RevealVotes) __result = false;
            }
        }

        [HarmonyPatch(typeof(Mushroom), "FixedUpdate")]
        public static class Mushroom_FixedUpdate
        {
            public static void Postfix(Mushroom __instance)
            {
                if (__instance == null || __instance.sporeMask == null) return;
                if (NoShadows) __instance.sporeMask.transform.position = new Vector3(__instance.sporeMask.transform.position.x, __instance.sporeMask.transform.position.y, -1);
                else __instance.sporeMask.transform.position = new Vector3(__instance.sporeMask.transform.position.x, __instance.sporeMask.transform.position.y, 5f);
            }
        }
    }

    public class HerePoint
    {
        public PlayerControl player;
        public SpriteRenderer sprite;

        public HerePoint(PlayerControl player, SpriteRenderer sprite)
        {
            this.player = player;
            this.sprite = sprite;
        }
    }
}