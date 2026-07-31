using System.Collections.Generic;

namespace DemoPulse.Models.Dto
{
    public class PlayerDto
    {
        public int Id { get; set; }
        public int SlotIndex { get; set; }
        public string Team { get; set; } = "";
        public string Name { get; set; } = "";
        public double Rating { get; set; }
        public double Impact { get; set; }
        public int Kills { get; set; }
        public int Deaths { get; set; }
        public int Assists { get; set; }
        public double Adr { get; set; }
        public long Damage { get; set; }
        public int HsPct { get; set; }
        public double KdRatio { get; set; }
        public double KrRatio { get; set; }
        public double KastPct { get; set; }
        public int TtkMs { get; set; }
        public int TtdMs { get; set; }
        public int OpeningKills { get; set; }
        public int OpeningDeaths { get; set; }
        public int EntryAttempts { get; set; }
        public int EntryDiff { get; set; }
        public int EntryAttemptsPct { get; set; }
        public int EntrySuccessPct { get; set; }
        public int TradeKills { get; set; }
        public int TradeDeaths { get; set; }
        public int TradeDiff { get; set; }
        public int TradeAttemptsPct { get; set; }
        public int TradeSuccessPct { get; set; }
        public int ClutchesWon { get; set; }
        public int ClutchRoundsLost { get; set; }
        public int ClutchSuccessPct { get; set; }
        public int C1v1 { get; set; }
        public int C1v2 { get; set; }
        public int C1v3 { get; set; }
        public int C1v4 { get; set; }
        public int C1v5 { get; set; }
        public int MultiKills { get; set; }
        public int MultiK5 { get; set; }
        public int MultiK4 { get; set; }
        public int MultiK3 { get; set; }
        public int MultiK2 { get; set; }
        public int Mvps { get; set; }
        public int FlashesThrown { get; set; }
        public int EnemiesBlinded { get; set; }
        public double AvgBlindDuration { get; set; }
        public int TeamFlashes { get; set; }
        public int UtilityDamage { get; set; }
    }

    public class DuelDto
    {
        public string TName { get; set; } = "";
        public string CtName { get; set; } = "";
        public int TWins { get; set; }
        public int CtWins { get; set; }
        public int TotalDuels { get; set; }
        public int THsPct { get; set; }
        public int CtHsPct { get; set; }
        public int AvgTtkMs { get; set; }
    }

    public class UtilityDto
    {
        public string Name { get; set; } = "";
        public string Team { get; set; } = "";
        public int Flashes { get; set; }
        public int Blinded { get; set; }
        public string Efficiency { get; set; } = "";
        public string AvgDuration { get; set; } = "";
        public int TeamFlashes { get; set; }
        public int UtilDmg { get; set; }
        public string Rating { get; set; } = "";
    }

    public class RoundDto
    {
        public int RoundNum { get; set; }
        public string Winner { get; set; } = "";
        public string WinType { get; set; } = "";
        public int DurationTicks { get; set; }
        public string BombSite { get; set; } = "";
        public List<string> Facts { get; set; } = new();
    }

    public class ClutchDto
    {
        public int RoundNum { get; set; }
        public string PlayerName { get; set; } = "";
        public string Team { get; set; } = "";
        public string ClutchType { get; set; } = "";
        public int VsCount { get; set; }
        public string WinType { get; set; } = "";
        public List<string> Opponents { get; set; } = new();
        public string Details { get; set; } = "";
    }

    public class VoiceConfigDto
    {
        public ulong TSideBitmask { get; set; }
        public ulong CtSideBitmask { get; set; }
        public ulong AllBitmask { get; set; }
        public string THex { get; set; } = "";
        public string CtHex { get; set; } = "";
    }

    public class MatchDataDto
    {
        public MatchMetaData Meta { get; set; } = new();
        public VoiceConfigDto VoiceConfig { get; set; } = new();
        public List<PlayerDto> Players { get; set; } = new();
        public List<DuelDto> Duels { get; set; } = new();
        public List<UtilityDto> Utility { get; set; } = new();
        public List<RoundDto> Rounds { get; set; } = new();
        public List<ClutchDto> Clutches { get; set; } = new();
    }
}
