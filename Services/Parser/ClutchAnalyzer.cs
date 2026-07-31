using System.Collections.Generic;
using System.Linq;
using DemoPulse.Models;

namespace DemoPulse.Services.Parser
{
    public class ClutchAnalyzer
    {
        public HashSet<ulong> AliveT { get; } = new();
        public HashSet<ulong> AliveCT { get; } = new();

        public ulong? RoundTClutchSteamId => _roundTClutchSteamId;
        private ulong? _roundTClutchSteamId;
        private int _roundTClutchVs;
        private List<string> _roundTClutchOpponents = new();

        public ulong? RoundCTClutchSteamId => _roundCTClutchSteamId;
        private ulong? _roundCTClutchSteamId;
        private int _roundCTClutchVs;
        private List<string> _roundCTClutchOpponents = new();

        public List<ClutchRecord> ClutchesList { get; } = new();

        public void ResetRoundAliveSets(Dictionary<ulong, PlayerStats> statsBySteamId)
        {
            AliveT.Clear();
            AliveCT.Clear();
            foreach (var kv in statsBySteamId)
            {
                if (kv.Value.TeamNum == (int)Cs2Team.Terrorist) AliveT.Add(kv.Key);
                else if (kv.Value.TeamNum == (int)Cs2Team.CounterTerrorist) AliveCT.Add(kv.Key);
            }

            _roundTClutchSteamId = null;
            _roundTClutchVs = 0;
            _roundTClutchOpponents.Clear();

            _roundCTClutchSteamId = null;
            _roundCTClutchVs = 0;
            _roundCTClutchOpponents.Clear();
        }

        public void OnPlayerDeath(ulong victimSteamId, int victimTeam, Dictionary<ulong, PlayerStats> statsBySteamId)
        {
            if (victimTeam == (int)Cs2Team.Terrorist) AliveT.Remove(victimSteamId);
            else if (victimTeam == (int)Cs2Team.CounterTerrorist) AliveCT.Remove(victimSteamId);

            if (_roundTClutchSteamId == null && AliveT.Count == 1 && AliveCT.Count >= 1)
            {
                _roundTClutchSteamId = AliveT.First();
                _roundTClutchVs = AliveCT.Count;
                _roundTClutchOpponents = AliveCT.Select(id => statsBySteamId.TryGetValue(id, out var s) ? s.Name : "CT").ToList();
            }

            if (_roundCTClutchSteamId == null && AliveCT.Count == 1 && AliveT.Count >= 1)
            {
                _roundCTClutchSteamId = AliveCT.First();
                _roundCTClutchVs = AliveT.Count;
                _roundCTClutchOpponents = AliveT.Select(id => statsBySteamId.TryGetValue(id, out var s) ? s.Name : "T").ToList();
            }
        }

        public void EvaluateRoundEndClutches(
            int roundNumber,
            int winningTeamNum,
            string winType,
            Dictionary<ulong, PlayerStats> statsBySteamId,
            List<string> factsOut)
        {
            // T-Side Clutch Evaluation
            if (_roundTClutchSteamId.HasValue && statsBySteamId.TryGetValue(_roundTClutchSteamId.Value, out var tClutchStat))
            {
                if (winningTeamNum == (int)Cs2Team.Terrorist)
                {
                    tClutchStat.ClutchesWon++;
                    switch (_roundTClutchVs)
                    {
                        case 1: tClutchStat.C1v1++; break;
                        case 2: tClutchStat.C1v2++; break;
                        case 3: tClutchStat.C1v3++; break;
                        case 4: tClutchStat.C1v4++; break;
                        case 5: tClutchStat.C1v5++; break;
                    }

                    string clutchLabel = $"1v{_roundTClutchVs}";
                    factsOut.Add($"Round {roundNumber}: 🏆 <b>{tClutchStat.Name}</b> won a high-pressure <b>{clutchLabel} clutch</b>!");

                    ClutchesList.Add(new ClutchRecord
                    {
                        RoundNum = roundNumber,
                        PlayerName = tClutchStat.Name,
                        Team = Cs2Team.Terrorist.ToTeamCode(),
                        ClutchType = clutchLabel,
                        VsCount = _roundTClutchVs,
                        WinType = winType,
                        Opponents = new List<string>(_roundTClutchOpponents),
                        Details = $"Round {roundNumber}: {tClutchStat.Name} (T) won a {clutchLabel} clutch via {winType}."
                    });
                }
                else
                {
                    tClutchStat.ClutchRoundsLost++;
                }
            }

            // CT-Side Clutch Evaluation
            if (_roundCTClutchSteamId.HasValue && statsBySteamId.TryGetValue(_roundCTClutchSteamId.Value, out var ctClutchStat))
            {
                if (winningTeamNum == (int)Cs2Team.CounterTerrorist)
                {
                    ctClutchStat.ClutchesWon++;
                    switch (_roundCTClutchVs)
                    {
                        case 1: ctClutchStat.C1v1++; break;
                        case 2: ctClutchStat.C1v2++; break;
                        case 3: ctClutchStat.C1v3++; break;
                        case 4: ctClutchStat.C1v4++; break;
                        case 5: ctClutchStat.C1v5++; break;
                    }

                    string clutchLabel = $"1v{_roundCTClutchVs}";
                    factsOut.Add($"Round {roundNumber}: 🏆 <b>{ctClutchStat.Name}</b> won a high-pressure <b>{clutchLabel} clutch</b>!");

                    ClutchesList.Add(new ClutchRecord
                    {
                        RoundNum = roundNumber,
                        PlayerName = ctClutchStat.Name,
                        Team = Cs2Team.CounterTerrorist.ToTeamCode(),
                        ClutchType = clutchLabel,
                        VsCount = _roundCTClutchVs,
                        WinType = winType,
                        Opponents = new List<string>(_roundCTClutchOpponents),
                        Details = $"Round {roundNumber}: {ctClutchStat.Name} (CT) won a {clutchLabel} clutch via {winType}."
                    });
                }
                else
                {
                    ctClutchStat.ClutchRoundsLost++;
                }
            }
        }
    }
}
