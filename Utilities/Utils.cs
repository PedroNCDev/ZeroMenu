using System;
using System.Linq;
using System.Collections.Generic;
using AmongUs.GameOptions;
using Hazel;
using InnerNet;
using UnityEngine;
using HarmonyLib;
using ZeroMenu.UI;
using AmongUs.InnerNet.GameDataMessages;
using System.Runtime.CompilerServices;
using System.IO;
using BepInEx;
using ZeroMenu.Utilities;

namespace ZeroMenu.Functions
{
    public static class Utils
    {
        public static bool IsShip => ShipStatus.Instance;
        public static bool IsPlayer => PlayerControl.LocalPlayer;
        public static bool IsMeeting => MeetingHud.Instance;
        public static bool IsLobby => AmongUsClient.Instance && AmongUsClient.Instance.GameState == InnerNetClient.GameStates.Joined && !IsFreePlay;
        public static bool IsInGame => AmongUsClient.Instance && AmongUsClient.Instance.GameState == InnerNetClient.GameStates.Started && IsPlayer;
        public static bool IsFreePlay => AmongUsClient.Instance && AmongUsClient.Instance.NetworkMode == NetworkModes.FreePlay;

        public static readonly SortedDictionary<int, string> SkeldVents = new SortedDictionary<int, string>()
        {
            { 0, "Admin" }, { 1, "Hallway" }, { 2, "Cafeteria" }, { 3, "Electrical" },
            { 4, "Upper Engine" }, { 5, "Security" }, { 6, "Medbay" }, { 7, "Weapons" },
            { 8, "Lower Reactor" }, { 9, "Lower Engine" }, { 10, "Shields" },
            { 11, "Upper Reactor" }, { 12, "Upper Navigation" }, { 13, "Lower Navigation" }
        };

        public static readonly SortedDictionary<int, string> MiraVents = new SortedDictionary<int, string>()
        {
            { 1, "Balcony" }, { 2, "Above Cafeteria" }, { 3, "Reactor" }, { 4, "Laboratory" },
            { 5, "Office" }, { 6, "Admin" }, { 7, "Greenhouse" }, { 8, "Medbay" },
            { 9, "Decontamination" }, { 10, "Locker Room" }, { 11, "Launchpad" }
        };

        public static readonly SortedDictionary<int, string> PolusVents = new SortedDictionary<int, string>()
        {
            { 0, "Electrical" }, { 1, "Outside Electrical" }, { 2, "Oxygen" }, { 3, "Outside Comms" },
            { 4, "Office" }, { 5, "Comms" }, { 6, "Laboratory" }, { 7, "Outside Laboratory" },
            { 8, "Storage" }, { 9, "Outside Rocket" }, { 10, "Above Electrical" }, { 11, "Outside Office" }
        };

        public static readonly SortedDictionary<int, string> AirshipVents = new SortedDictionary<int, string>()
        {
            { 0, "Vault" }, { 1, "Cockpit" }, { 2, "Viewing Deck" }, { 3, "Engine Room" },
            { 4, "Kitchen" }, { 5, "Bedroom 1" }, { 6, "Janitor Room" }, { 7, "Gap Room Right" },
            { 8, "Gap Room Left" }, { 9, "Showers" }, { 10, "Records" }, { 11, "Cargo Bay" }
        };

        public static readonly SortedDictionary<int, string> FungleVents = new SortedDictionary<int, string>()
        {
            { 0, "Comms" }, { 1, "Kitchen" }, { 2, "Lookout" }, { 3, "Above Meeting Room" },
            { 4, "Laboratory" }, { 5, "Reactor" }, { 6, "Jungle" }, { 7, "Jungle Bottom" },
            { 8, "Splash Zone" }, { 9, "Outside Dropship" }
        };

        public static string GetVentName(int ventId)
        {
            int mapId = GetCurrentMapID();
            SortedDictionary<int, string> vents = mapId switch
            {
                0 => SkeldVents,
                1 => MiraVents,
                2 => PolusVents,
                3 => SkeldVents,
                4 => AirshipVents,
                5 => FungleVents,
                _ => SkeldVents,
            };

            return vents.TryGetValue(ventId, out string name) ? name : "Unknown";
        }

        public static string GetNameTag(NetworkedPlayerInfo playerInfo, string playerName, bool isChat = false)
        {
            var nameTag = playerName;
            if (playerInfo.Role == null || playerInfo == null || playerInfo.Disconnected || playerInfo.Object.CurrentOutfit == null) return nameTag;

            var player = AmongUsClient.Instance.GetClientFromPlayerInfo(playerInfo);
            var host = AmongUsClient.Instance.GetHost();
            var level = playerInfo.PlayerLevel + 1;

            var roleColor = ColorUtility.ToHtmlStringRGB(playerInfo.Role.TeamColor);
            var hostString = player == host ? "Host - " : "";
            var roleName = DestroyableSingleton<TranslationController>.Instance.GetString(playerInfo.Role.StringName, Il2CppSystem.Array.Empty<Il2CppSystem.Object>());

            if (ESP.SeeRoles)
            {
                if (ESP.SeePlayerInfo)
                {
                    if (isChat)
                    {
                        nameTag = $"<color=#{roleColor}>{nameTag} <size=70%>{roleName}</size></color> <size=70%><color=#fb0>{hostString}Lv:{level}</color></size>";
                        return nameTag;
                    }
                    nameTag = $"<size=70%><color=#fb0>{hostString}Lv:{level}</color></size>\r\n<color=#{roleColor}><size=70%>{roleName}</size>\r\n{nameTag}</color>";
                }
                else
                {
                    if (isChat)
                    {
                        nameTag = $"<color=#{roleColor}>{nameTag} <size=70%>{roleName}</size></color>";
                        return nameTag;
                    }
                    nameTag = $"<color=#{roleColor}><size=70%>{roleName}</size>\r\n{nameTag}</color>";
                }
            }
            else if (ESP.SeePlayerInfo)
            {
                if (PlayerControl.LocalPlayer.Data.Role.NameColor == playerInfo.Role.NameColor)
                {
                    if (isChat)
                    {
                        nameTag = $"<color=#{ColorUtility.ToHtmlStringRGB(playerInfo.Role.NameColor)}>{nameTag}</color> <size=70%><color=#fb0>{hostString}Lv:{level}</color></size>";
                        return nameTag;
                    }
                    nameTag = $"<size=70%><color=#fb0>{hostString}Lv:{level}</color></size>\r\n<color=#{ColorUtility.ToHtmlStringRGB(playerInfo.Role.NameColor)}>{nameTag}";
                }
                else
                {
                    if (isChat)
                    {
                        nameTag = $"{nameTag} <size=70%><color=#fb0>{hostString}Lv:{level}</color></size>";
                        return nameTag;
                    }
                    nameTag = $"<size=70%><color=#fb0>{hostString}Lv:{level}</color></size>\r\n{nameTag}";
                }
            }

            return nameTag;
        }

        public static void DrawTracer(GameObject sourceObject, GameObject targetObject, Color color)
        {
            var lineRenderer = sourceObject.GetComponent<LineRenderer>();
            if (!lineRenderer) lineRenderer = sourceObject.AddComponent<LineRenderer>();

            lineRenderer.SetVertexCount(2);
            lineRenderer.SetWidth(0.02F, 0.02F);
            lineRenderer.material = DestroyableSingleton<HatManager>.Instance.PlayerMaterial;
            lineRenderer.SetColors(color, color);
            lineRenderer.SetPosition(0, sourceObject.transform.position);
            lineRenderer.SetPosition(1, targetObject.transform.position);
        }

        public static void AdjustResolution()
        {
            ResolutionManager.ResolutionChanged.Invoke((float)Screen.width / Screen.height, Screen.width, Screen.height, Screen.fullScreen);
        }

        public static int GetCurrentMapID()
        {
            if (AmongUsClient.Instance.NetworkMode == NetworkModes.FreePlay)
                return AmongUsClient.Instance.TutorialMapId;
            
            if (GameOptionsManager.Instance?.currentGameOptions != null)
                return GameOptionsManager.Instance.currentGameOptions.MapId;

            return byte.MaxValue;
        }

        public static int GetClientIdByPlayer(PlayerControl player)
        {
            try
            {
                var client = AmongUsClient.Instance.GetClientFromCharacter(player);
                return client != null ? client.Id : -1;
            }
            catch { return -1; }
        }

        public static void KickPlayer(PlayerControl player, bool skipFirstStage = false, bool silent = false)
        {
            if (player == null || player.Data == null) return;

            bool amHost = AmongUsClient.Instance.AmHost;
            if (amHost)
            {
                AmongUsClient.Instance.KickPlayer(player.OwnerId, true);
                if (!silent) ZeroMenu.Plugin.notifications.Send("Ban Player", player.Data.PlayerName + " has been banned from the game.", 10f);
                return;
            }

            if (ShipStatus.Instance == null)
            {
                if (!silent) ZeroMenu.Plugin.notifications.Send("Ban Player", "The game must have started for this option to work.", 10f);
                return;
            }

            try
            {
                BatchedMessage batch = new BatchedMessage(player.OwnerId);

                MessageWriter writer = MessageWriter.Get(SendOption.Reliable);
                writer.Write((ushort)0);
                writer.Write((byte)VentilationSystem.Operation.Enter);
                writer.Write((byte)0);
                batch.QueueUpdateSystem(PlayerControl.LocalPlayer, SystemTypes.Ventilation, writer);
                writer.Recycle();

                MessageWriter writer2 = MessageWriter.Get(SendOption.Reliable);
                writer2.Write((ushort)1);
                writer2.Write((byte)VentilationSystem.Operation.BootImpostors);
                writer2.Write((byte)0);
                batch.QueueUpdateSystem(PlayerControl.LocalPlayer, SystemTypes.Ventilation, writer2);
                writer2.Recycle();

                batch.FinishBatch();
                if (!silent) ZeroMenu.Plugin.notifications.Send("Vent Kick", player.Data.PlayerName + " has been kicked.", 10f);
            }
            catch { }
        }

        public static void SendGameOptions(PlayerControl target, float speed, float light)
        {
            if (target == null || !AmongUsClient.Instance.AmHost) return;
            try
            {
                IGameOptions options = GameManager.Instance.LogicOptions.currentGameOptions;
                options.SetFloat(FloatOptionNames.PlayerSpeedMod, speed);
                options.SetFloat(FloatOptionNames.CrewLightMod, light);
                options.SetFloat(FloatOptionNames.ImpostorLightMod, light);
                
                MessageWriter writer = MessageWriter.Get((SendOption)1);
                writer.StartMessage((byte)FindLogicOptionsIndex());
                writer.WriteBytesAndSize(GameManager.Instance.LogicOptions.gameOptionsFactory.ToBytes(options, false));
                writer.EndMessage();
                
                var batch = new BatchedMessage(target.OwnerId);
                batch.QueueDataFlag(GameManager.Instance.NetId, writer);
                batch.FinishBatch();
                writer.Recycle();
            } catch { }
        }

        private static int FindLogicOptionsIndex()
        {
            for (int i = 0; i < GameManager.Instance.LogicComponents.Count; i++)
                if (GameManager.Instance.LogicComponents[i].GetType() == typeof(LogicOptions)) return i;
            return -1;
        }

        public static void CallMeeting()
        {
            if (PlayerControl.LocalPlayer == null) return;

            if (AmongUsClient.Instance.AmHost)
            {
                MeetingRoomManager.Instance.AssignSelf(PlayerControl.LocalPlayer, null);
                DestroyableSingleton<HudManager>.Instance.OpenMeetingRoom(PlayerControl.LocalPlayer);
                PlayerControl.LocalPlayer.RpcStartMeeting(null);
            }
            else
            {
                PlayerControl.LocalPlayer.CmdReportDeadBody(null);
            }
        }

        public static void CloseMeeting()
        {
            if (MeetingHud.Instance != null)
            {
                MeetingHud.Instance.DespawnOnDestroy = false;
                UnityEngine.Object.Destroy(MeetingHud.Instance.gameObject);

                DestroyableSingleton<HudManager>.Instance.StartCoroutine(DestroyableSingleton<HudManager>.Instance.CoFadeFullScreen(UnityEngine.Color.black, UnityEngine.Color.clear, 0.2f, false));
                PlayerControl.LocalPlayer.SetKillTimer(GameManager.Instance.LogicOptions.GetKillCooldown());
                ShipStatus.Instance.EmergencyCooldown = GameManager.Instance.LogicOptions.GetEmergencyCooldown();
                Camera.main.GetComponent<FollowerCamera>().Locked = false;
                DestroyableSingleton<HudManager>.Instance.SetMapAndInfoButtonsEnabled(true);
                DestroyableSingleton<HudManager>.Instance.SetHudActive(true);
                ControllerManager.Instance.CloseAndResetAll();
            }
            else if (ExileController.Instance != null)
            {
                ExileController.Instance.ReEnableGameplay();
                ExileController.Instance.WrapUp();
            }
        }

        public static void ForceSetScanner(PlayerControl player, bool toggle)
        {
            var count = ++player.scannerCount;
            player.SetScanner(toggle, count);
            RpcSetScannerMessage rpcMessage = new(player.NetId, toggle, count);
            AmongUsClient.Instance.LateBroadcastReliableMessage(Unsafe.As<IGameDataMessage>(rpcMessage));
        }

        public static void ForcePlayAnimation(byte animationType)
        {
            PlayerControl.LocalPlayer.PlayAnimation(animationType);
            RpcPlayAnimationMessage rpcMessage = new(PlayerControl.LocalPlayer.NetId, animationType);
            AmongUsClient.Instance.LateBroadcastUnreliableMessage(Unsafe.As<IGameDataMessage>(rpcMessage));
        }

        public static void OpenConfigFile()
        {
            string path = Path.Combine(Paths.ConfigPath, "ZeroMenu.json");
            if (!File.Exists(path))
            {
                ZeroMenu.Plugin.notifications.Send("Config", "Config file doesn't exist yet. Save it first!", 3f);
                return;
            }
            try
            {
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = path,
                    UseShellExecute = true
                });
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogError($"[ZeroMenu.Config] Failed to open config: {e.Message}");
            }
        }
    }

    public static class Vipershifter
    {
        public static bool Enabled = false;
        public static bool IsShifted = false;
        
        private static Sprite _mutarSprite;
        private static Sprite _anularSprite;
        public static ActionButton VipershifterButton;

        public static void LoadSprites()
        {
            if (_mutarSprite != null) return;

            try
            {
                var assembly = typeof(Vipershifter).Assembly;
                
                using var stream1 = assembly.GetManifestResourceStream("ZeroMenu.shapefist1.png");
                if (stream1 != null)
                {
                    using var ms1 = new MemoryStream();
                    stream1.CopyTo(ms1);
                    var tex1 = new Texture2D(2, 2, TextureFormat.RGBA32, false);
                    tex1.LoadImage(ms1.ToArray());
                    _mutarSprite = Sprite.Create(tex1, new Rect(0, 0, tex1.width, tex1.height), new Vector2(0.5f, 0.5f), 100f);
                    _mutarSprite.hideFlags = HideFlags.HideAndDontSave;
                }

                using var stream2 = assembly.GetManifestResourceStream("ZeroMenu.shapefist2.png");
                if (stream2 != null)
                {
                    using var ms2 = new MemoryStream();
                    stream2.CopyTo(ms2);
                    var tex2 = new Texture2D(2, 2, TextureFormat.RGBA32, false);
                    tex2.LoadImage(ms2.ToArray());
                    _anularSprite = Sprite.Create(tex2, new Rect(0, 0, tex2.width, tex2.height), new Vector2(0.5f, 0.5f), 100f);
                    _anularSprite.hideFlags = HideFlags.HideAndDontSave;
                }
            }
            catch (System.Exception e)
            {
                UnityEngine.Debug.LogError("Error loading Vipershifter sprites: " + e.Message);
            }
        }

        public static void OpenShiftMenu()
        {
            Il2CppSystem.Collections.Generic.List<NetworkedPlayerInfo> playerList = new Il2CppSystem.Collections.Generic.List<NetworkedPlayerInfo>();
            foreach (var pc in PlayerControl.AllPlayerControls)
            {
                if (pc != null && pc.Data != null && !pc.Data.Disconnected && !pc.Data.IsDead && pc != PlayerControl.LocalPlayer)
                {
                    playerList.Add(pc.Data);
                }
            }

            PlayerPickMenu.OpenPlayerPickMenu(playerList, (Il2CppSystem.Action)(() =>
            {
                var target = PlayerPickMenu.targetPlayerData?.Object;
                if (target != null)
                {
                    PlayerControl.LocalPlayer.Shapeshift(target, true);
                    IsShifted = true;
                }
            }));
        }

        public static void RevertShift()
        {
            PlayerControl.LocalPlayer.Shapeshift(PlayerControl.LocalPlayer, true);
            IsShifted = false;
        }

        [HarmonyPatch(typeof(HudManager), nameof(HudManager.Update))]
        public static class HudManager_Update_Patch
        {
            public static void Postfix(HudManager __instance)
            {
                if (!Enabled || PlayerControl.LocalPlayer == null || PlayerControl.LocalPlayer.Data == null) 
                {
                    if (VipershifterButton != null) VipershifterButton.gameObject.SetActive(false);
                    return;
                }

                if (PlayerControl.LocalPlayer.Data.RoleType != RoleTypes.Viper || MeetingHud.Instance != null || ExileController.Instance != null || PlayerControl.LocalPlayer.Data.IsDead)
                {
                    if (VipershifterButton != null) VipershifterButton.gameObject.SetActive(false);
                    return;
                }

                if (VipershifterButton == null)
                {
                    VipershifterButton = UnityEngine.Object.Instantiate(__instance.AbilityButton, __instance.AbilityButton.transform.parent);
                    VipershifterButton.transform.localPosition = __instance.ImpostorVentButton.transform.localPosition + new Vector3(-1.2f, 0f, 0f); 
                    
                    var ventRect = __instance.ImpostorVentButton.GetComponent<RectTransform>();
                    var vipRect = VipershifterButton.GetComponent<RectTransform>();
                    
                    if (ventRect != null && vipRect != null)
                    {
                        vipRect.sizeDelta = ventRect.sizeDelta;
                        vipRect.localScale = ventRect.localScale;
                    }
                    
                    var passiveButton = VipershifterButton.GetComponent<PassiveButton>();
                    if (passiveButton != null) 
                    {
                        passiveButton.OnClick = new UnityEngine.UI.Button.ButtonClickedEvent();
                        passiveButton.OnClick.AddListener((UnityEngine.Events.UnityAction)(() =>
                        {
                            if (IsShifted) RevertShift();
                            else OpenShiftMenu();
                        }));
                    }
                }

                VipershifterButton.gameObject.SetActive(true);
                VipershifterButton.OverrideText("");
                VipershifterButton.SetCoolDown(0f, 100f);
                
                VipershifterButton.graphic.sprite = IsShifted ? _anularSprite : _mutarSprite;
                
                if (VipershifterButton.cooldownTimerText != null) 
                    VipershifterButton.cooldownTimerText.gameObject.SetActive(false);
            }
        }

        [HarmonyPatch(typeof(ActionButton), nameof(ActionButton.DoClick))]
        public static class ActionButton_DoClick_Patch
        {
            public static bool Prefix(ActionButton __instance)
            {
                if (__instance == VipershifterButton)
                {
                    return false; 
                }
                return true;
            }
        }

        [HarmonyPatch(typeof(HudManager), nameof(HudManager.Start))]
        public static class HudManager_Start_Patch
        {
            public static void Postfix()
            {
                VipershifterButton = null;
            }
        }
    }
}
