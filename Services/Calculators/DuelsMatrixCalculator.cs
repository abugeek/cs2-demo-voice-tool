using System;
using System.Collections.Generic;
using System.Linq;
using DemoPulse.Models;
using DemoPulse.Models.Dto;

namespace DemoPulse.Services.Calculators
{
    public static class DuelsMatrixCalculator
    {
        public static List<DuelDto> BuildDuelsJson(
            List<PlayerStats> tPlayers,
            List<PlayerStats> ctPlayers,
            Dictionary<ulong, PlayerStats> statsBySteamId,
            Dictionary<(ulong keyA, ulong keyB), DuelStats> duelsByPair,
            Func<ulong, string, ulong> getPlayerKeyFunc)
        {
            var duelsJson = new List<DuelDto>(tPlayers.Count * ctPlayers.Count);

            // Pre-index SteamID keys by player name for O(1) lookup
            var playerKeyByName = new Dictionary<string, ulong>(StringComparer.Ordinal);
            foreach (var kv in statsBySteamId)
            {
                if (!string.IsNullOrEmpty(kv.Value.Name) && !playerKeyByName.ContainsKey(kv.Value.Name))
                {
                    playerKeyByName[kv.Value.Name] = kv.Key;
                }
            }

            foreach (var t in tPlayers)
            {
                ulong tKey = playerKeyByName.TryGetValue(t.Name, out ulong foundTKey) && foundTKey != 0
                    ? foundTKey
                    : getPlayerKeyFunc(0, t.Name);

                foreach (var ct in ctPlayers)
                {
                    ulong ctKey = playerKeyByName.TryGetValue(ct.Name, out ulong foundCtKey) && foundCtKey != 0
                        ? foundCtKey
                        : getPlayerKeyFunc(0, ct.Name);

                    var pairKey = tKey < ctKey ? (tKey, ctKey) : (ctKey, tKey);
                    int tWins = 0;
                    int ctWins = 0;
                    int tHs = 0;
                    int ctHs = 0;
                    int avgPairTtk = 0;

                    if (duelsByPair.TryGetValue(pairKey, out var duel))
                    {
                        if (tKey == duel.KeyA)
                        {
                            tWins = duel.WinsA;
                            ctWins = duel.WinsB;
                            tHs = duel.HsA;
                            ctHs = duel.HsB;
                        }
                        else
                        {
                            tWins = duel.WinsB;
                            ctWins = duel.WinsA;
                            tHs = duel.HsB;
                            ctHs = duel.HsA;
                        }

                        if (duel.TtkMsList.Count > 0)
                        {
                            avgPairTtk = (int)Math.Round(duel.TtkMsList.Average());
                        }
                    }

                    int totalDuels = tWins + ctWins;
                    int tHsPct = tWins > 0 ? (int)Math.Round((double)tHs / tWins * 100.0) : 0;
                    int ctHsPct = ctWins > 0 ? (int)Math.Round((double)ctHs / ctWins * 100.0) : 0;

                    duelsJson.Add(new DuelDto
                    {
                        TName = t.Name,
                        CtName = ct.Name,
                        TWins = tWins,
                        CtWins = ctWins,
                        TotalDuels = totalDuels,
                        THsPct = tHsPct,
                        CtHsPct = ctHsPct,
                        AvgTtkMs = avgPairTtk
                    });
                }
            }
            return duelsJson;
        }
    }
}
