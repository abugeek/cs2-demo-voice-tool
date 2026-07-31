using System;
using DemoPulse.Models;
using DemoPulse.Models.Dto;

namespace DemoPulse.Services.Calculators
{
    public static class RatingCalculator
    {
        public static PlayerDto BuildPlayerJson(PlayerStats p, int totalRounds, ref int pId)
        {
            int kills = p.Kills;
            int roundsPlayed = Math.Max(totalRounds, 1);
            double adr = Math.Round((double)p.TotalDamage / roundsPlayed, 1);
            double hsPct = kills > 0 ? Math.Round((double)p.Headshots / kills * 100.0, 0) : 0;
            int avgTtk = p.TtkList.Count > 0 ? (int)Math.Round(p.TtkList.Average()) : 0;
            int avgTtd = p.TtdList.Count > 0 ? (int)Math.Round(p.TtdList.Average()) : 0;

            double kpr = (double)kills / roundsPlayed;
            double dpr = (double)p.Deaths / roundsPlayed;
            double apr = (double)p.Assists / roundsPlayed;
            double impact = Math.Round(2.13 * kpr + 0.42 * apr - 0.41, 2);
            double rating = Math.Round(0.0073 * adr + 0.359 * kpr - 0.532 * dpr + 0.237 * impact + 0.158, 2);
            if (rating < 0.2) rating = 0.5;

            double kastPct = Math.Round((double)p.KastRounds / roundsPlayed * 100.0, 1);
            double kdRatio = p.Deaths > 0 ? Math.Round((double)kills / p.Deaths, 2) : kills;
            double krRatio = Math.Round((double)kills / roundsPlayed, 2);

            int totalClutchAttempts = p.ClutchesWon + p.ClutchRoundsLost;
            double clutchSuccessPct = totalClutchAttempts > 0 ? Math.Round((double)p.ClutchesWon / totalClutchAttempts * 100.0, 0) : 0;

            int entryAttempts = p.OpeningKills + p.OpeningDeaths;
            int entryDiff = p.OpeningKills - p.OpeningDeaths;
            double entryAttemptsPct = Math.Round((double)entryAttempts / roundsPlayed * 100.0, 0);
            double entrySuccessPct = entryAttempts > 0 ? Math.Round((double)p.OpeningKills / entryAttempts * 100.0, 0) : 0;

            int tradeTotal = p.TradeKills + p.TradeDeaths;
            int tradeDiff = p.TradeKills - p.TradeDeaths;
            double tradeAttemptsPct = Math.Round((double)tradeTotal / roundsPlayed * 100.0, 0);
            double tradeSuccessPct = tradeTotal > 0 ? Math.Round((double)p.TradeKills / tradeTotal * 100.0, 0) : 0;

            int multiKills = p.MultiK2 + p.MultiK3 + p.MultiK4 + p.MultiK5;

            return new PlayerDto
            {
                Id = pId++,
                SlotIndex = p.SlotIndex,
                Team = p.Team,
                Name = p.Name,
                Rating = rating,
                Impact = impact,
                Kills = kills,
                Deaths = p.Deaths,
                Assists = p.Assists,
                Adr = adr,
                Damage = p.TotalDamage,
                HsPct = (int)hsPct,
                KdRatio = kdRatio,
                KrRatio = krRatio,
                KastPct = kastPct,
                TtkMs = avgTtk,
                TtdMs = avgTtd,
                OpeningKills = p.OpeningKills,
                OpeningDeaths = p.OpeningDeaths,
                EntryAttempts = entryAttempts,
                EntryDiff = entryDiff,
                EntryAttemptsPct = (int)entryAttemptsPct,
                EntrySuccessPct = (int)entrySuccessPct,
                TradeKills = p.TradeKills,
                TradeDeaths = p.TradeDeaths,
                TradeDiff = tradeDiff,
                TradeAttemptsPct = (int)tradeAttemptsPct,
                TradeSuccessPct = (int)tradeSuccessPct,
                ClutchesWon = p.ClutchesWon,
                ClutchRoundsLost = p.ClutchRoundsLost,
                ClutchSuccessPct = (int)clutchSuccessPct,
                C1v1 = p.C1v1,
                C1v2 = p.C1v2,
                C1v3 = p.C1v3,
                C1v4 = p.C1v4,
                C1v5 = p.C1v5,
                MultiKills = multiKills,
                MultiK5 = p.MultiK5,
                MultiK4 = p.MultiK4,
                MultiK3 = p.MultiK3,
                MultiK2 = p.MultiK2,
                Mvps = p.Mvps,
                FlashesThrown = p.FlashesThrown,
                EnemiesBlinded = p.EnemiesBlinded,
                AvgBlindDuration = p.EnemiesBlinded > 0 ? Math.Round((double)p.TotalBlindDuration / p.EnemiesBlinded, 1) : 0.0,
                TeamFlashes = p.TeamFlashes,
                UtilityDamage = p.UtilityDamage
            };
        }
    }
}
