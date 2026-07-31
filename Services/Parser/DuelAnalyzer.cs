using System;
using System.Collections.Generic;
using DemoPulse.Models;

namespace DemoPulse.Services.Parser
{
    public class DuelAnalyzer
    {
        public Dictionary<(ulong keyA, ulong keyB), DuelStats> DuelsByPair { get; } = new();
        public Dictionary<(ulong p1, ulong p2), int> RoundHurtStartTick { get; } = new();

        public void OnRoundStart()
        {
            RoundHurtStartTick.Clear();
        }

        public void OnPlayerHurt(ulong attackerSteamId, ulong victimSteamId, int currentTick)
        {
            var hurtKey = (attackerSteamId, victimSteamId);
            if (!RoundHurtStartTick.ContainsKey(hurtKey))
            {
                RoundHurtStartTick[hurtKey] = currentTick;
            }
        }

        public void RecordDuel(
            ulong attackerSteamId,
            ulong victimSteamId,
            bool isHeadshot,
            PlayerStats attackerStat,
            PlayerStats victimStat,
            int currentTick)
        {
            int deathTick = currentTick;
            int firstHurtTick = deathTick;

            if (RoundHurtStartTick.TryGetValue((attackerSteamId, victimSteamId), out int t1))
                firstHurtTick = Math.Min(firstHurtTick, t1);
            if (RoundHurtStartTick.TryGetValue((victimSteamId, attackerSteamId), out int t2))
                firstHurtTick = Math.Min(firstHurtTick, t2);

            int fightTicks = Math.Max(1, deathTick - firstHurtTick);
            double tickMs = 15.625; // CS2 64-tick demo standard
            int ttkMs = Math.Clamp((int)(fightTicks * tickMs), 15, 8000);

            attackerStat.TtkList.Add(ttkMs);
            victimStat.TtdList.Add(ttkMs);

            var pairKey = attackerSteamId < victimSteamId
                ? (attackerSteamId, victimSteamId)
                : (victimSteamId, attackerSteamId);

            if (!DuelsByPair.TryGetValue(pairKey, out var duel))
            {
                duel = new DuelStats
                {
                    KeyA = pairKey.Item1,
                    KeyB = pairKey.Item2
                };
                DuelsByPair[pairKey] = duel;
            }

            duel.TtkMsList.Add(ttkMs);
            if (attackerSteamId == duel.KeyA)
            {
                duel.WinsA++;
                if (isHeadshot) duel.HsA++;
            }
            else
            {
                duel.WinsB++;
                if (isHeadshot) duel.HsB++;
            }
        }
    }
}
