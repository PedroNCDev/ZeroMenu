using UnityEngine;
using ZeroMenu.Functions;
using Il2CppInterop.Runtime.InteropTypes.Arrays;

namespace ZeroMenu.Tabs
{
    public class ProtectionsTab : ITab
    {
        public string Name => "Protections";
        private Vector2 _scrollPosition = Vector2.zero;

        public void Draw()
        {
            _scrollPosition = SmoothScrollView.Begin(_scrollPosition, System.Array.Empty<GUILayoutOption>());
            GUILayout.BeginVertical(new Il2CppReferenceArray<GUILayoutOption>(new GUILayoutOption[] { GUILayout.Width((float)MainUI.windowWidth * 0.425f) }));

            Protections.ForceDTLS = CustomToggle.Draw(Protections.ForceDTLS, "Force enable DTLS to encrypt network data", "ForceDTLS");
            Protections.BlockServerTeleports = CustomToggle.Draw(Protections.BlockServerTeleports, "Block position updates from server", "BlockServerTeleports");
            Protections.HardenedReadPackedUInt = CustomToggle.Draw(Protections.HardenedReadPackedUInt, "Use hardened packed int deserializer", "HardenedReadPackedUInt");
            Protections.BlockInvalidLadderOverload = CustomToggle.Draw(Protections.BlockInvalidLadderOverload, "Protect against invalid ladder overload", "BlockInvalidLadderOverload");
            Protections.BlockLargeGameMessages = CustomToggle.Draw(Protections.BlockLargeGameMessages, "Block large game messages", "BlockLargeGameMessages");
            Protections.BlockInvalidGameDataMessages = CustomToggle.Draw(Protections.BlockInvalidGameDataMessages, "Block invalid game data message types", "BlockInvalidGameDataMessages");
            Protections.BlockUnauthorizedSystemUpdates = CustomToggle.Draw(Protections.BlockUnauthorizedSystemUpdates, "Block unauthorized system updates", "BlockUnauthorizedSystemUpdates");
            Protections.ProtectAgainstNonHostKickExploit = CustomToggle.Draw(Protections.ProtectAgainstNonHostKickExploit, "Protect against non-host kick exploit", "ProtectAgainstNonHostKickExploit");
            Protections.BlockZiplineForce = CustomToggle.Draw(Protections.BlockZiplineForce, "Block forced ziplines", "BlockZiplineForce");
            Protections.BlockVentTpForce = CustomToggle.Draw(Protections.BlockVentTpForce, "Block forced vent teleports", "BlockVentTpForce");

            Protections.Votekicks = CustomToggle.Draw(Protections.Votekicks, "Prevent being votekicked as host", "Votekicks");
            Protections.MemoryAllocationOverload = CustomToggle.Draw(Protections.MemoryAllocationOverload, "Protect against VotingComplete overloads", "MemoryAllocationOverload");
            Protections.BypassShapeshiftRatelimits = CustomToggle.Draw(Protections.BypassShapeshiftRatelimits, "Bypass ratelimits for Shapeshift RPC", "BypassShapeshiftRatelimits");
            Protections.AntiCrash = CustomToggle.Draw(Protections.AntiCrash, "Protect against report-body crash exploit", "AntiCrash");
            
            Protections.AntiExploits = CustomToggle.Draw(Protections.AntiExploits, "Anti-Exploits", "AntiExploits");
            Protections.ShowProtections = CustomToggle.Draw(Protections.ShowProtections, "Show Protections (Keeper)", "ShowProtections");

            GUILayout.EndVertical();
            GUILayout.EndScrollView();
        }
    }
}