namespace DemoPulse.Models
{
    public enum Cs2Team
    {
        Unassigned = 0,
        Spectator = 1,
        Terrorist = 2,
        CounterTerrorist = 3
    }

    public static class Cs2TeamExtensions
    {
        public static string ToTeamCode(this Cs2Team team) => team switch
        {
            Cs2Team.Terrorist => "T",
            Cs2Team.CounterTerrorist => "CT",
            Cs2Team.Spectator => "SPEC",
            _ => "UNASSIGNED"
        };
    }

    public enum Cs2RoundEndReasonCode
    {
        TargetEliminated = 1,
        VIPEscaped = 2,
        VIPKilled = 3,
        TerroristsEscaped = 4,
        CTStoppedEscape = 5,
        TerroristsStopped = 6,
        BombDefused = 7,
        BombDefusedAlt = 8,
        BombExploded = 9,
        TerroristsSurrendered = 10
    }

    public static class Cs2RoundEndReasonExtensions
    {
        public static string ToWinTypeString(int reasonCode) => reasonCode switch
        {
            (int)Cs2RoundEndReasonCode.BombDefusedAlt => "Bomb Defused",
            (int)Cs2RoundEndReasonCode.BombDefused => "Team Eliminated",
            (int)Cs2RoundEndReasonCode.BombExploded => "Bomb Exploded",
            (int)Cs2RoundEndReasonCode.TargetEliminated => "Team Eliminated",
            _ => "Round Win"
        };
    }

    public enum Cs2BombSite
    {
        SiteA = 0,
        SiteB = 1
    }

    public static class Cs2BombSiteExtensions
    {
        public static string ToSiteLabel(int siteIndex) => siteIndex switch
        {
            (int)Cs2BombSite.SiteA => "A",
            (int)Cs2BombSite.SiteB => "B",
            _ => "A"
        };
    }

    public static class Cs2VoiceConstants
    {
        /// <summary>Default 5-player T-side bitmask (slots 0-4 = 31 / 0x1F)</summary>
        public const ulong DefaultTSideMask = 31UL;

        /// <summary>Default 5-player CT-side bitmask (slots 5-9 = 992 / 0x3E0)</summary>
        public const ulong DefaultCtSideMask = 992UL;

        /// <summary>All voices enabled bitmask (0xFFFFFFFFFFFFFFFF = 18446744073709551615)</summary>
        public const ulong AllVoicesMask = 0xFFFFFFFFFFFFFFFFUL;

        /// <summary>Muted voice bitmask (0x0)</summary>
        public const ulong MutedVoiceMask = 0UL;
    }
}
