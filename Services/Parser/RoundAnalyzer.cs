using System.Collections.Generic;
using DemoPulse.Models;

namespace DemoPulse.Services.Parser
{
    public class RoundAnalyzer
    {
        public int RoundNumber { get; private set; }
        public string? LastBombSite { get; private set; }
        public bool BombWasPlanted { get; private set; }
        public bool RoundOpeningKillDone { get; set; }
        public string? RoundOpeningKillerName { get; set; }
        public string? RoundOpeningVictimName { get; set; }
        public List<(string killer, string victim)> RoundDeaths { get; } = new();

        public int FirstHalfT { get; private set; }
        public int FirstHalfCT { get; private set; }
        public List<RoundInfo> Rounds { get; } = new();

        public void OnRoundStart()
        {
            RoundNumber++;
            RoundOpeningKillDone = false;
            RoundOpeningKillerName = null;
            RoundOpeningVictimName = null;
            BombWasPlanted = false;
            LastBombSite = null;
            RoundDeaths.Clear();
        }

        public void OnBombPlanted(int site)
        {
            BombWasPlanted = true;
            LastBombSite = Cs2BombSiteExtensions.ToSiteLabel(site);
        }

        public RoundInfo OnRoundEnd(int winningTeamNum, int reason)
        {
            var winningTeam = (Cs2Team)winningTeamNum;
            string winner = winningTeam.ToTeamCode();

            if (RoundNumber <= 12)
            {
                if (winningTeam == Cs2Team.Terrorist) FirstHalfT++;
                else if (winningTeam == Cs2Team.CounterTerrorist) FirstHalfCT++;
            }

            string winType = Cs2RoundEndReasonExtensions.ToWinTypeString(reason);

            if (!BombWasPlanted && winType == "Bomb Exploded")
                winType = "Team Eliminated";

            string site = LastBombSite ?? "";
            var facts = new List<string>
            {
                $"Round {RoundNumber}: <b>{winner} Side</b> won via {winType}" +
                (string.IsNullOrEmpty(site) ? "." : $" on <b>Site {site}</b>.")
            };

            if (!string.IsNullOrEmpty(RoundOpeningKillerName))
                facts.Add($"Round {RoundNumber}: Opening frag by <b>{RoundOpeningKillerName}</b> (killed {RoundOpeningVictimName}).");

            var roundInfo = new RoundInfo
            {
                RoundNum = RoundNumber,
                Winner = winner,
                WinType = winType,
                BombSite = site,
                Facts = facts
            };

            Rounds.Add(roundInfo);
            return roundInfo;
        }
    }
}
