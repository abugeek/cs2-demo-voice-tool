using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using DemoFile;
using DemoFile.Sdk;
using DemoPulse.Models;
using DemoPulse.Models.Dto;
using DemoPulse.Services.Calculators;
using DemoPulse.Services.Parser;

namespace DemoPulse.Services
{
    /// <summary>
    /// Refactored Orchestrator Service for parsing real CS2 .dem files.
    /// Uses strongly-typed DTOs (MatchDataDto) and JSON CamelCase Naming Policy for clean type safety.
    /// </summary>
    public class DemoParserService
    {
        public static Task<MatchDataDto> ParseDemoMatchDataAsync(string filePath)
        {
            var provider = new Providers.PhysicalFileSystemService();
            return ParseDemoMatchDataAsync(filePath, provider);
        }

        public static async Task<MatchDataDto> ParseDemoMatchDataAsync(string filePath, IFileStreamProvider streamProvider)
        {
            if (streamProvider == null) throw new ArgumentNullException(nameof(streamProvider));
            string fileName = Path.GetFileName(filePath) ?? "unknown.dem";
            using var stream = streamProvider.OpenReadStream(filePath);
            return await ParseDemoMatchDataAsync(stream, fileName, filePath);
        }

        public static async Task<string> ParseDemoAsync(string filePath)
        {
            var provider = new Providers.PhysicalFileSystemService();
            return await ParseDemoAsync(filePath, provider);
        }

        public static async Task<string> ParseDemoAsync(string filePath, IFileStreamProvider streamProvider)
        {
            var matchData = await ParseDemoMatchDataAsync(filePath, streamProvider);
            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = false
            };
            return JsonSerializer.Serialize(matchData, options);
        }

        public static async Task<string> ParseDemoAsync(Stream stream, string fileName = "unknown.dem", string filePath = "")
        {
            var matchData = await ParseDemoMatchDataAsync(stream, fileName, filePath);
            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = false
            };
            return JsonSerializer.Serialize(matchData, options);
        }

        public static async Task<MatchDataDto> ParseDemoMatchDataAsync(Stream stream, string fileName = "unknown.dem", string filePath = "")
        {
            var statsBySteamId = new Dictionary<ulong, PlayerStats>();
            var roundKills = new Dictionary<ulong, int>();
            var roundKast = new Dictionary<ulong, bool>();

            var roundAnalyzer = new RoundAnalyzer();
            var clutchAnalyzer = new ClutchAnalyzer();
            var duelAnalyzer = new DuelAnalyzer();
            var utilityAnalyzer = new UtilityAnalyzer();

            var demo = new CsDemoParser();
            string mapName = "";
            string serverName = "";
            bool isWarmup = false;

            bool IsWarmupActive()
            {
                try
                {
                    if (demo.GameRules != null)
                        return demo.GameRules.WarmupPeriod;
                }
                catch { }
                return isWarmup;
            }

            // ── Warmup Events ────────────────────────────────────────────────────────
            demo.Source1GameEvents.RoundAnnounceWarmup += e =>
            {
                isWarmup = true;
            };

            demo.Source1GameEvents.RoundAnnounceMatchStart += e =>
            {
                isWarmup = false;
            };

            // ── Map & Server from SvcServerInfo ──────────────────────────────────────
            demo.PacketEvents.SvcServerInfo += msg =>
            {
                if (!string.IsNullOrEmpty(msg.MapName))
                    mapName = msg.MapName;
            };

            // ── Round Start ───────────────────────────────────────────────────────────
            demo.Source1GameEvents.RoundStart += e =>
            {
                if (IsWarmupActive()) return;
                isWarmup = false;

                roundAnalyzer.OnRoundStart();
                duelAnalyzer.OnRoundStart();
                clutchAnalyzer.ResetRoundAliveSets(statsBySteamId);

                roundKills.Clear();
                roundKast.Clear();
            };

            // ── Bomb Events ───────────────────────────────────────────────────────────
            demo.Source1GameEvents.BombPlanted += e =>
            {
                if (IsWarmupActive()) return;
                roundAnalyzer.OnBombPlanted(e.Site);
            };

            // ── Round MVP Event ────────────────────────────────────────────────────────
            demo.Source1GameEvents.RoundMvp += e =>
            {
                if (IsWarmupActive()) return;
                var player = e.Player;
                if (player != null)
                {
                    string playerName = player.PlayerName ?? "Unknown";
                    ulong steamId = GetPlayerKey(player.SteamID, playerName);
                    int team = (int)player.TeamNum;
                    var stat = GetOrCreate(statsBySteamId, steamId, playerName, team, (int)player.EntityIndex.Value - 1);
                    stat.Mvps++;
                }
            };

            // ── Round End ─────────────────────────────────────────────────────────────
            demo.Source1GameEvents.RoundEnd += e =>
            {
                if (IsWarmupActive()) return;

                int winningTeamNum = e.Winner; // 2=T, 3=CT
                var roundInfo = roundAnalyzer.OnRoundEnd(winningTeamNum, e.Reason);

                // Clutch evaluation
                clutchAnalyzer.EvaluateRoundEndClutches(
                    roundAnalyzer.RoundNumber,
                    winningTeamNum,
                    roundInfo.WinType,
                    statsBySteamId,
                    roundInfo.Facts);

                // Survivors gain KAST round point
                foreach (var aliveId in clutchAnalyzer.AliveT.Concat(clutchAnalyzer.AliveCT))
                {
                    roundKast[aliveId] = true;
                }

                foreach (var kv in roundKast)
                {
                    if (kv.Value && statsBySteamId.TryGetValue(kv.Key, out var pSt))
                    {
                        pSt.KastRounds++;
                    }
                }

                foreach (var kv in roundKills)
                {
                    if (statsBySteamId.TryGetValue(kv.Key, out var pSt))
                    {
                        if (kv.Value == 2) pSt.MultiK2++;
                        else if (kv.Value == 3) pSt.MultiK3++;
                        else if (kv.Value == 4) pSt.MultiK4++;
                        else if (kv.Value >= 5) pSt.MultiK5++;
                    }
                }
            };

            // ── Player Death ──────────────────────────────────────────────────────────
            demo.Source1GameEvents.PlayerDeath += e =>
            {
                if (IsWarmupActive()) return;

                var attacker = e.Attacker;
                var victim = e.Player;
                var assister = e.Assister;

                if (victim == null) return;
                string victimName = victim.PlayerName ?? "Unknown";
                ulong victimSteamId = GetPlayerKey(victim.SteamID, victimName);
                int victimTeam = (int)victim.TeamNum;

                var victimStat = GetOrCreate(statsBySteamId, victimSteamId, victimName, victimTeam, (int)victim.EntityIndex.Value - 1);
                victimStat.Deaths++;

                clutchAnalyzer.OnPlayerDeath(victimSteamId, victimTeam, statsBySteamId);

                if (attacker != null && attacker != victim)
                {
                    string attackerName = attacker.PlayerName ?? "Unknown";
                    ulong attackerSteamId = GetPlayerKey(attacker.SteamID, attackerName);
                    int attackerTeam = (int)attacker.TeamNum;
                    var attackerStat = GetOrCreate(statsBySteamId, attackerSteamId, attackerName, attackerTeam, (int)attacker.EntityIndex.Value - 1);

                    if (attackerTeam != victimTeam && victimTeam != 1 && attackerTeam != 1)
                    {
                        attackerStat.Kills++;
                        if (e.Headshot) attackerStat.Headshots++;

                        roundKills[attackerSteamId] = roundKills.GetValueOrDefault(attackerSteamId) + 1;
                        roundKast[attackerSteamId] = true;

                        if (!roundAnalyzer.RoundOpeningKillDone)
                        {
                            roundAnalyzer.RoundOpeningKillDone = true;
                            roundAnalyzer.RoundOpeningKillerName = attackerName;
                            roundAnalyzer.RoundOpeningVictimName = victimName;
                            attackerStat.OpeningKills++;
                            victimStat.OpeningDeaths++;
                        }

                        // Trade detection: if last killer was just killed
                        int recentCount = roundAnalyzer.RoundDeaths.Count;
                        if (recentCount >= 1 && roundAnalyzer.RoundDeaths[recentCount - 1].killer == victimName)
                        {
                            attackerStat.TradeKills++;
                            victimStat.TradeDeaths++;
                            roundKast[victimSteamId] = true;
                        }

                        roundAnalyzer.RoundDeaths.Add((attackerName, victimName));

                        // Record pairwise duel and TTK/TTD
                        int currentTick = demo.CurrentDemoTick.Value;
                        duelAnalyzer.RecordDuel(attackerSteamId, victimSteamId, e.Headshot, attackerStat, victimStat, currentTick);
                    }
                }

                if (assister != null && assister != victim && assister != attacker)
                {
                    string assisterName = assister.PlayerName ?? "Unknown";
                    ulong assisterSteamId = GetPlayerKey(assister.SteamID, assisterName);
                    int assisterTeam = (int)assister.TeamNum;
                    if (assisterTeam != victimTeam && assisterTeam != 1 && victimTeam != 1)
                    {
                        var assisterStat = GetOrCreate(statsBySteamId, assisterSteamId, assisterName, assisterTeam, (int)assister.EntityIndex.Value - 1);
                        assisterStat.Assists++;
                        roundKast[assisterSteamId] = true;
                    }
                }
            };

            // ── Player Hurt (damage) ──────────────────────────────────────────────────
            demo.Source1GameEvents.PlayerHurt += e =>
            {
                if (IsWarmupActive()) return;

                var attacker = e.Attacker;
                var victim = e.Player;
                if (attacker == null || victim == null || attacker == victim) return;

                int attackerTeam = (int)attacker.TeamNum;
                int victimTeam = (int)victim.TeamNum;
                if (attackerTeam == victimTeam || attackerTeam == 1 || victimTeam == 1) return;

                string attackerName = attacker.PlayerName ?? "Unknown";
                string victimName = victim.PlayerName ?? "Unknown";
                ulong attackerSteamId = GetPlayerKey(attacker.SteamID, attackerName);
                ulong victimSteamId = GetPlayerKey(victim.SteamID, victimName);

                var stat = GetOrCreate(statsBySteamId, attackerSteamId, attackerName, attackerTeam, (int)attacker.EntityIndex.Value - 1);
                utilityAnalyzer.OnPlayerHurt(stat, e.DmgHealth, e.Weapon ?? "");

                int currentTick = demo.CurrentDemoTick.Value;
                duelAnalyzer.OnPlayerHurt(attackerSteamId, victimSteamId, currentTick);
            };

            // ── Player Blind (flashes) ────────────────────────────────────────────────
            demo.Source1GameEvents.PlayerBlind += e =>
            {
                if (IsWarmupActive()) return;

                var attacker = e.Attacker;
                var victim = e.Player;
                if (attacker == null || victim == null) return;

                string attackerName = attacker.PlayerName ?? "Unknown";
                ulong attackerSteamId = GetPlayerKey(attacker.SteamID, attackerName);
                int attackerTeam = (int)attacker.TeamNum;
                var stat = GetOrCreate(statsBySteamId, attackerSteamId, attackerName, attackerTeam, (int)attacker.EntityIndex.Value - 1);

                utilityAnalyzer.OnPlayerBlind(stat, attackerTeam, (int)victim.TeamNum, e.BlindDuration);
            };

            // ── Flashbang Thrown ──────────────────────────────────────────────────────
            demo.Source1GameEvents.FlashbangDetonate += e =>
            {
                if (IsWarmupActive()) return;

                var player = e.Player;
                if (player == null) return;
                string playerName = player.PlayerName ?? "Unknown";
                ulong steamId = GetPlayerKey(player.SteamID, playerName);
                int team = (int)player.TeamNum;
                var stat = GetOrCreate(statsBySteamId, steamId, playerName, team, (int)player.EntityIndex.Value - 1);

                utilityAnalyzer.OnFlashbangDetonate(stat);
            };

            // ── Parse the demo stream ─────────────────────────────────────────────────
            try
            {
                var reader = DemoFileReader.Create(demo, stream);
                await reader.ReadAllAsync();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to parse demo stream: {ex.Message}", ex);
            }

            // ── Header info (available after parse) ──────────────────────────────────
            if (string.IsNullOrEmpty(mapName))
                mapName = demo.FileHeader?.MapName ?? "unknown";
            if (string.IsNullOrEmpty(serverName))
                serverName = demo.FileHeader?.ServerName ?? "CS2 Server";

            // ── Organise players ─────────────────────────────────────────────────────
            var validPlayers = statsBySteamId.Values
                .Where(p => p.TeamNum == (int)Cs2Team.Terrorist || p.TeamNum == (int)Cs2Team.CounterTerrorist)
                .Where(p => !string.IsNullOrWhiteSpace(p.Name) && p.Name != "Unknown")
                .GroupBy(p => p.Name)
                .Select(g => g.OrderByDescending(p => p.Kills + p.Deaths + p.Assists).First())
                .ToList();

            var tPlayers = validPlayers.Where(p => p.TeamNum == (int)Cs2Team.Terrorist).OrderByDescending(p => p.Kills).ToList();
            var ctPlayers = validPlayers.Where(p => p.TeamNum == (int)Cs2Team.CounterTerrorist).OrderByDescending(p => p.Kills).ToList();

            foreach (var p in tPlayers) { p.Team = Cs2Team.Terrorist.ToTeamCode(); }
            foreach (var p in ctPlayers) { p.Team = Cs2Team.CounterTerrorist.ToTeamCode(); }

            // Ensure every player has a valid slot index (fallback if slotIndex was not captured)
            int fallbackSlot = 0;
            var usedSlots = validPlayers.Where(p => p.SlotIndex >= 0).Select(p => p.SlotIndex).ToHashSet();
            foreach (var p in validPlayers)
            {
                if (p.SlotIndex < 0)
                {
                    while (usedSlots.Contains(fallbackSlot)) fallbackSlot++;
                    p.SlotIndex = fallbackSlot;
                    usedSlots.Add(fallbackSlot);
                }
            }

            var allPlayers = tPlayers.Concat(ctPlayers).ToList();

            // ── Round scores ─────────────────────────────────────────────────────────
            int scoreCT = roundAnalyzer.Rounds.Count(r => r.Winner == Cs2Team.CounterTerrorist.ToTeamCode());
            int scoreT = roundAnalyzer.Rounds.Count(r => r.Winner == Cs2Team.Terrorist.ToTeamCode());
            int totalRounds = Math.Max(roundAnalyzer.Rounds.Count, roundAnalyzer.RoundNumber);

            // ── Build player DTO objects ─────────────────────────────────────────────
            var playersDto = new List<PlayerDto>();
            int pId = 1;
            foreach (var p in allPlayers)
            {
                playersDto.Add(RatingCalculator.BuildPlayerJson(p, totalRounds, ref pId));
            }

            // ── Duels matrix (Real Pairwise Kills, Headshots & TTK extracted from Demo) ─
            var duelsDto = DuelsMatrixCalculator.BuildDuelsJson(
                tPlayers, ctPlayers, statsBySteamId, duelAnalyzer.DuelsByPair, GetPlayerKey);

            // ── Utility DTO ──────────────────────────────────────────────────────────
            var utilityDto = allPlayers.Select(p =>
            {
                double eff = p.FlashesThrown > 0
                    ? Math.Round((double)p.EnemiesBlinded / p.FlashesThrown, 2)
                    : 0.0;
                string rating = eff >= 1.7 ? "S Tier" : eff >= 1.3 ? "A Tier" : eff >= 1.0 ? "B Tier" : "C Tier";
                string avgBlind = p.EnemiesBlinded > 0
                    ? $"{p.TotalBlindDuration / p.EnemiesBlinded:F1}s"
                    : "N/A";

                return new UtilityDto
                {
                    Name = p.Name,
                    Team = p.Team,
                    Flashes = p.FlashesThrown,
                    Blinded = p.EnemiesBlinded,
                    Efficiency = $"{eff:F2} enemies/flash",
                    AvgDuration = avgBlind,
                    TeamFlashes = p.TeamFlashes,
                    UtilDmg = p.UtilityDamage,
                    Rating = rating
                };
            }).ToList();

            // ── Rounds DTO ───────────────────────────────────────────────────────────
            var roundsDto = roundAnalyzer.Rounds.Select(r => new RoundDto
            {
                RoundNum = r.RoundNum,
                Winner = r.Winner,
                WinType = r.WinType,
                DurationTicks = 0,
                BombSite = r.BombSite,
                Facts = r.Facts
            }).ToList();

            // ── Clutches DTO ─────────────────────────────────────────────────────────
            var clutchesDto = clutchAnalyzer.ClutchesList.Select(c => new ClutchDto
            {
                RoundNum = c.RoundNum,
                PlayerName = c.PlayerName,
                Team = c.Team,
                ClutchType = c.ClutchType,
                VsCount = c.VsCount,
                WinType = c.WinType,
                Opponents = c.Opponents,
                Details = c.Details
            }).ToList();

            // ── Voice bitmask ─────────────────────────────────────────────────────────
            var tSlots = tPlayers.Select(p => p.SlotIndex).ToList();
            var ctSlots = ctPlayers.Select(p => p.SlotIndex).ToList();
            ulong tMask = VoiceBitmaskService.CalculateBitmask(tSlots);
            ulong ctMask = VoiceBitmaskService.CalculateBitmask(ctSlots);

            // ── Final Strongly Typed Match Data DTO ──────────────────────────────────
            return new MatchDataDto
            {
                Meta = new MatchMetaData
                {
                    FileName = fileName,
                    FilePath = filePath,
                    Map = mapName,
                    Server = serverName,
                    RoundsCount = totalRounds,
                    ScoreT = scoreT,
                    ScoreCT = scoreCT,
                    FirstHalfT = roundAnalyzer.FirstHalfT,
                    FirstHalfCT = roundAnalyzer.FirstHalfCT,
                    SecondHalfT = scoreT - roundAnalyzer.FirstHalfT,
                    SecondHalfCT = scoreCT - roundAnalyzer.FirstHalfCT,
                    Winner = scoreCT >= scoreT ? "CT" : "T"
                },
                VoiceConfig = new VoiceConfigDto
                {
                    TSideBitmask = tMask,
                    CtSideBitmask = ctMask,
                    AllBitmask = VoiceBitmaskService.GetAllVoicesBitmask(),
                    THex = FormatHexBitmask(tMask),
                    CtHex = FormatHexBitmask(ctMask)
                },
                Players = playersDto,
                Duels = duelsDto,
                Utility = utilityDto,
                Rounds = roundsDto,
                Clutches = clutchesDto
            };
        }

        private static string FormatHexBitmask(ulong mask)
        {
            var sb = StringBuilderCache.Acquire(32);
            sb.Append("0x").AppendFormat("{0:X16}", mask);
            return StringBuilderCache.GetStringAndRelease(sb);
        }

        private static ulong GetPlayerKey(ulong steamId, string name)
        {
            return steamId != 0 ? steamId : unchecked((ulong)(long)name.GetHashCode());
        }

        private static PlayerStats GetOrCreate(
            Dictionary<ulong, PlayerStats> dict, ulong key, string name, int teamNum, int slotIndex = -1)
        {
            if (!dict.TryGetValue(key, out var stat))
            {
                stat = new PlayerStats { Name = name, TeamNum = teamNum, SlotIndex = slotIndex };
                dict[key] = stat;
            }
            else
            {
                if (!string.IsNullOrEmpty(name) && stat.Name != name)
                    stat.Name = name;
                // Only set team if not already assigned to a valid competitive team.
                // This preserves the first-half team assignment through halftime
                // side-swaps, which is critical for correct voice bitmask generation.
                if (stat.TeamNum != (int)Cs2Team.Terrorist && stat.TeamNum != (int)Cs2Team.CounterTerrorist)
                {
                    if (teamNum == (int)Cs2Team.Terrorist || teamNum == (int)Cs2Team.CounterTerrorist)
                        stat.TeamNum = teamNum;
                }
                if (slotIndex >= 0 && stat.SlotIndex < 0)
                    stat.SlotIndex = slotIndex;
            }
            return stat;
        }
    }

    // Backward compatibility wrapper class so existing calls to DemoParserEngine or VoiceBitmaskGenerator remain valid
    public static class DemoParserEngine
    {
        public static Task<string> ParseDemoAsync(string filePath) => DemoParserService.ParseDemoAsync(filePath);
    }

    public static class VoiceBitmaskGenerator
    {
        public static ulong CalculateBitmask(List<int> playerIndices) => VoiceBitmaskService.CalculateBitmask(playerIndices);
        public static ulong GetAllVoicesBitmask() => VoiceBitmaskService.GetAllVoicesBitmask();
        public static string GenerateCS2Config(string mode, ulong? customMask = null, ulong? tMask = null, ulong? ctMask = null, AppSettings? settings = null) =>
            VoiceBitmaskService.GenerateCS2Config(mode, customMask, tMask, ctMask, settings);
    }
}
