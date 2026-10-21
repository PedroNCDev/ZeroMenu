using System;
using UnityEngine;
using ZeroMenu.Functions;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using AmongUs.GameOptions;

namespace ZeroMenu.Tabs
{
    public class PlayersTab : ITab
    {
        public string Name => "Players";
        private Vector2 _listScroll = Vector2.zero;
        private Vector2 _actionScroll = Vector2.zero;
        private static bool _showHostOnly = false;

        public void Draw()
        {
            if (PlayerControl.AllPlayerControls.Count == 0)
            {
                GUILayout.Label("There are currently no online players.", MenuStyles.Label, new Il2CppReferenceArray<GUILayoutOption>(0));
                return;
            }

            GUILayout.BeginHorizontal(new Il2CppReferenceArray<GUILayoutOption>(0));

            GUILayout.BeginVertical(new Il2CppReferenceArray<GUILayoutOption>(new GUILayoutOption[] { GUILayout.Width((float)MainUI.windowWidth * 0.35f) }));
            _listScroll = SmoothScrollView.Begin(_listScroll, new GUILayoutOption[0]);
            DrawPlayerList();
            GUILayout.EndScrollView();
            GUILayout.EndVertical();

            if (Players.SelectedPlayer != null)
            {
                GUILayout.BeginVertical(new Il2CppReferenceArray<GUILayoutOption>(0));
                _actionScroll = SmoothScrollView.Begin(_actionScroll, new GUILayoutOption[0]);
                DrawPlayerControls(Players.SelectedPlayer);
                GUILayout.EndScrollView();
                GUILayout.EndVertical();
            }

            GUILayout.EndHorizontal();
        }

        private void DrawPlayerList()
        {
            foreach (var player in PlayerControl.AllPlayerControls)
            {
                if (player == null || player.Data == null) continue;

                string nameColor = ColorUtility.ToHtmlStringRGB(player.Data.Color);
                string roleColor = player.Data.Role.IsImpostor ? "red" : "#8afcfc";
                string hostTag = player.OwnerId == AmongUsClient.Instance.HostId ? " [HOST]" : "";
                string stateTag = player.Data.IsDead ? " [DEAD]" : "";

                string label = $"<color=#{nameColor}>{player.Data.PlayerName}</color>{hostTag}{stateTag}\n<color={roleColor}>{player.Data.RoleType}</color>";

                if (CustomButton.Draw(label))
                {
                    Players.SelectedPlayer = player;
                }
            }
        }

        private static void DrawPlayerControls(PlayerControl target)
        {
            if (target == null || target.Data == null)
            {
                GUILayout.Label("Specified target is not valid.", MenuStyles.Label, new Il2CppReferenceArray<GUILayoutOption>(0));
                return;
            }

            try
            {
                var client = AmongUsClient.Instance.GetClientFromCharacter(target);
                if (client != null)
                {
                    GUILayout.Label($"Name: {target.Data.PlayerName}\nRole: {target.Data.RoleType}\nState: {(target.Data.IsDead ? "Dead" : "Alive")}\nFriendcode: {target.Data.FriendCode}\nPUID: {target.Data.Puid}\nLevel: {target.Data.PlayerLevel + 1}\nPlatform: {client.PlatformData.Platform}\nDevice ID: {client.Id}", MenuStyles.Label, new Il2CppReferenceArray<GUILayoutOption>(0));
                }
            }
            catch { }

            GUILayout.Space(5f);

            GUILayout.BeginHorizontal(new Il2CppReferenceArray<GUILayoutOption>(0));
            if (CustomButton.Draw("GENERAL")) _showHostOnly = false;
            if (CustomButton.Draw("HOST-ONLY")) _showHostOnly = true;
            GUILayout.EndHorizontal();

            GUILayout.Space(5f);

            if (!_showHostOnly)
            {
                if (CustomButton.Draw("Teleport")) { try { Players.TeleportTo(target); } catch { } }
                if (CustomButton.Draw("Murder")) { try { Players.Murder(target); } catch { } }
                if (CustomButton.Draw("Telemurder")) { try { Players.StartTeleMurder(target); } catch { } }
                if (CustomButton.Draw("Shapeshift Into")) { try { Players.ShapeshiftInto(target); } catch { } }
                if (CustomButton.Draw("Protect")) { try { Players.Protect(target); } catch { } }
                if (CustomButton.Draw("Copy Avatar")) { try { Players.CopyAvatar(target); } catch { } }
                if (CustomButton.Draw("Report Body")) { try { Players.ReportBody(target); } catch { } }

                GUILayout.Space(5f);

                int maxVents = ShipStatus.Instance != null ? ShipStatus.Instance.AllVents.Count - 1 : 0;
                if (maxVents > 0)
                {
                    GUILayout.Label($"Teleport to Vent: {Utils.GetVentName(Players.SelectedVent)}", MenuStyles.Label, new Il2CppReferenceArray<GUILayoutOption>(0));
                    Players.SelectedVent = (int)CustomSlider.DrawAuto(Players.SelectedVent, 0f, maxVents, new Il2CppReferenceArray<GUILayoutOption>(new GUILayoutOption[] { GUILayout.Width(200f) }));
                    if (CustomButton.Draw("Teleport To Vent")) { try { Players.TeleportToVent(target, Players.SelectedVent); } catch { } }
                }

                GUILayout.Space(5f);

                bool isFollowing = Players.FollowTarget == target;
                if (CustomToggle.Draw(isFollowing, "Follow Player", "Follow_" + target.PlayerId) != isFollowing) Players.Follow(target);

                bool isPetting = Players.PetTarget == target;
                if (CustomToggle.Draw(isPetting, "Pet Player", "Pet_" + target.PlayerId) != isPetting) Players.PetPlayer(target);

                bool isJailing = Players.JailTarget == target;
                if (CustomToggle.Draw(isJailing, "Jail Player", "Jail_" + target.PlayerId) != isJailing) Players.JailPlayer(target);

                bool isWhispering = Players.WhisperTarget == target;
                if (CustomToggle.Draw(isWhispering, "Whisper", "Whisper_" + target.PlayerId) != isWhispering) Players.Whisper(target);

                bool isSpectating = Players.IsSpectating;
                if (CustomToggle.Draw(isSpectating, "Spectate", "Spectate_" + target.PlayerId) != isSpectating) Players.Spectate(target);

                bool isImmortal = Players.IsImmortal(target.PlayerId);
                if (CustomToggle.Draw(isImmortal, "Immortal", "Immortal_" + target.PlayerId) != isImmortal) Players.ToggleImmortality(target);
                
                GUILayout.Space(5f);

                if (CustomButton.Draw("Send Fake Lights")) { try { Players.SendFakeLights(target); } catch { } }
                if (CustomButton.Draw("Send Fake Oxygen")) { try { Players.SendFakeOxygen(target); } catch { } }
                if (CustomButton.Draw("Send Fake Reactor")) { try { Players.SendFakeReactor(target); } catch { } }
                if (CustomButton.Draw("Door Hallucination")) { try { Players.DoorHallucination(target); } catch { } }
                
                GUILayout.Space(5f);

                if (CustomButton.Draw("Kick")) { try { Utils.KickPlayer(target); } catch { } }
                if (CustomButton.Draw("Votekick")) { try { Players.Votekick(target); } catch { } }
            }
            else
            {
                if (!AmongUsClient.Instance.AmHost) GUILayout.Label("(You are not host, these may fail)", MenuStyles.Label, new Il2CppReferenceArray<GUILayoutOption>(0));
                
                if (CustomButton.Draw("Revive")) { try { Players.Revive(target); } catch { } }
                if (CustomButton.Draw("Force Meeting As")) { try { Players.ForceMeeting(target); } catch { } }
                if (CustomButton.Draw("Force All Votes To")) { try { Players.ForceVotes(target); } catch { } }
                if (CustomButton.Draw("Eject")) { try { Players.Eject(target); } catch { } }
                if (CustomButton.Draw("Ban Exploit")) { try { Players.BanExploit(target); } catch { } }
                if (CustomButton.Draw("Frame Shapeshift")) { try { Players.FrameShapeshift(target); } catch { } }
                if (CustomButton.Draw("Frame for Killing All")) { try { Players.FrameForKillingAll(target); } catch { } }
                if (CustomButton.Draw("Flood Player with Tasks")) { try { Players.FloodTasks(target); } catch { } }
                if (CustomButton.Draw("Clear Tasks")) { try { Players.ClearTasks(target); } catch { } }

                GUILayout.Space(5f);
                
                if (CustomButton.Draw("Blind")) { try { Players.SendGameOptions(target, 1f, -1f); } catch { } }
                if (CustomButton.Draw("Fullbright")) { try { Players.SendGameOptions(target, 1f, 1000f); } catch { } }
                if (CustomButton.Draw("Slow Speed")) { try { Players.SendGameOptions(target, 0.1f, 1f); } catch { } }
                if (CustomButton.Draw("Super Speed")) { try { Players.SendGameOptions(target, 3f, 1f); } catch { } }
                if (CustomButton.Draw("Nerf Player")) { try { Players.NerfPlayer(target); } catch { } }
                if (CustomButton.Draw("Buff Player")) { try { Players.BuffPlayer(target); } catch { } }
                if (CustomButton.Draw("Reset Defaults")) { try { Players.ResetOptions(target); } catch { } }

                GUILayout.Space(5f);
                
                GUILayout.Label($"Color: {((Players.PlayerColors)Players.SelectedColorIndex).ToString()}", MenuStyles.Label, new Il2CppReferenceArray<GUILayoutOption>(0));
                float colorIndexFloat = CustomSlider.DrawAuto((float)Players.SelectedColorIndex, 0f, 18f, new Il2CppReferenceArray<GUILayoutOption>(new GUILayoutOption[] { GUILayout.Width(200f) }));
                Players.SelectedColorIndex = Mathf.RoundToInt(colorIndexFloat);
                if (CustomButton.Draw("Apply Color")) { try { Players.SetColor(target); } catch { } }

                GUILayout.Space(5f);
                
                GUILayout.Label($"Role: {Players.SelectedRole}", MenuStyles.Label, new Il2CppReferenceArray<GUILayoutOption>(0));
                float roleIndex = CustomSlider.DrawAuto((float)Players.SelectedRoleIndex, 0f, Players.ValidRoles.Length - 1, new Il2CppReferenceArray<GUILayoutOption>(new GUILayoutOption[] { GUILayout.Width(200f) }));
                Players.SelectedRoleIndex = Mathf.RoundToInt(roleIndex);
                if (CustomButton.Draw("Apply Role")) { try { Players.SetRole(target); } catch { } }
            }
        }
    }
}