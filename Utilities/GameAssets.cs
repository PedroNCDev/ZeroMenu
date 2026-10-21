using AmongUs.GameOptions;
using System.Collections.Generic;

namespace ZeroMenu.Utilities
{
    public static class GameAssets
    {
        public static readonly List<RoleTypes> RolesList = new List<RoleTypes>()
        {
            RoleTypes.Crewmate,
            RoleTypes.Scientist,
            RoleTypes.Engineer,
            RoleTypes.Noisemaker,
            RoleTypes.Tracker,
            RoleTypes.Detective,
            RoleTypes.Judge,
            RoleTypes.Impostor,
            RoleTypes.Shapeshifter,
            RoleTypes.Phantom,
            RoleTypes.Viper,
            RoleTypes.CrewmateGhost,
            RoleTypes.GuardianAngel,
            RoleTypes.SpiritGuide,
            RoleTypes.ImpostorGhost
        };

        public static readonly SortedList<int, string> BroadcastVersions = new SortedList<int, string>()
        {
            { 50632950, "16.1.0" },
            { 50643450, "17.1" },
            { 50647000, "17.1.2" },
            { 50645050, "17.2" },
            { 50652900, "17.2.1" },
            { 50653700, "17.2.2" },
            { 50652400, "17.3" },
            { 50656300, "17.4" },
            { 50663350, "18.0" },
            { 50663400, "18.0 (build 7348)" },
            { 50663650, "19.0 (build 7488)" },
            { 50663600, "19.0 (build 7489)" }
        };

        public static readonly SortedList<int, string> PlayerPlatforms = new SortedList<int, string>()
        {
            { 0, "Unknown" },
            { 1, "Epic Games" },
            { 2, "Steam" },
            { 3, "MacOS" },
            { 4, "Microsoft Store" },
            { 5, "Itch.io" },
            { 6, "iOS" },
            { 7, "Android" },
            { 8, "Nintendo Switch" },
            { 9, "Xbox" },
            { 10, "PlayStation" },
            { 112, "Starlight" }
        };

        public static readonly SortedList<string, string> ModRegistrationGuids = new SortedList<string, string>()
        {
            { "", "None" },
            { "446420cc-ce35-4c2f-8f5d-b14f84354421", "Endless Host Roles"},
            { "789bf6f3-41e4-439f-82d3-ed6d2d2fb710", "Town of Next"},
            { "9f21dfdc-1224-4322-af7b-63db54cea706", "Super New Roles" }
        };

        public static string GetPlatformName(Platforms platformId)
        {
            return PlayerPlatforms.GetValueOrDefault((int)platformId, $"Unknown {platformId}");
        }

        public static string GetPlatformName(int platformId)
        {
            return PlayerPlatforms.GetValueOrDefault(platformId, $"Unknown {platformId}");
        }
    }
}