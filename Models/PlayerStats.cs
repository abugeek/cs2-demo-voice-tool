using System.Collections.Generic;

namespace DemoPulse.Models
{
    public class PlayerStats
    {
        public string Name { get; set; } = "";
        public string Team { get; set; } = "";  // "T" or "CT"
        public int TeamNum { get; set; }         // 2=T, 3=CT
        public int Kills { get; set; }
        public int Deaths { get; set; }
        public int Assists { get; set; }
        public int Headshots { get; set; }
        public long TotalDamage { get; set; }
        public int UtilityDamage { get; set; }
        public int FlashesThrown { get; set; }
        public int EnemiesBlinded { get; set; }
        public float TotalBlindDuration { get; set; }
        public int TeamFlashes { get; set; }
        public int OpeningKills { get; set; }
        public int OpeningDeaths { get; set; }
        public int TradeKills { get; set; }
        public int TradeDeaths { get; set; }
        public int ClutchesWon { get; set; }
        public int ClutchRoundsLost { get; set; }
        public int C1v1 { get; set; }
        public int C1v2 { get; set; }
        public int C1v3 { get; set; }
        public int C1v4 { get; set; }
        public int C1v5 { get; set; }
        public int MultiK2 { get; set; }
        public int MultiK3 { get; set; }
        public int MultiK4 { get; set; }
        public int MultiK5 { get; set; }
        public int Mvps { get; set; }
        public int KastRounds { get; set; }
        public int SlotIndex { get; set; } = -1;
        public List<int> TtkList { get; } = new();
        public List<int> TtdList { get; } = new();
    }
}
