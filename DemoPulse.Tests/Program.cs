using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using DemoPulse;
using DemoPulse.Models;
using DemoPulse.Services;
using DemoPulse.Services.Calculators;
using DemoPulse.Services.Parser;

namespace DemoPulse.Tests
{
    public class Program
    {
        public static int Main(string[] args)
        {
            Console.WriteLine("=================================================");
            Console.WriteLine("  DEMOPULSE AUTOMATED UNIT TEST SUITE            ");
            Console.WriteLine("=================================================");
            Console.WriteLine();

            int passed = 0;
            int failed = 0;

            // ─── VoiceBitmaskGenerator Tests (pure math, no demo file needed) ────────

            // ─── VoiceBitmaskGenerator Tests (pure math, no demo file needed) ────────

            RunTest("VoiceBitmask_ZeroSlots_ReturnsMaskOfZero", () =>
            {
                ulong mask = VoiceBitmaskGenerator.CalculateBitmask(new List<int>());
                Assert(mask == 0, "Empty slot list should produce mask 0");
            }, ref passed, ref failed);

            RunTest("VoiceBitmask_Slot0Only_Returns1", () =>
            {
                ulong mask = VoiceBitmaskGenerator.CalculateBitmask(new List<int> { 0 });
                Assert(mask == 1, "Slot 0 only => mask should be 1 (0b00000001)");
            }, ref passed, ref failed);

            RunTest("VoiceBitmask_Slots012_Returns7", () =>
            {
                ulong mask = VoiceBitmaskGenerator.CalculateBitmask(new List<int> { 0, 1, 2 });
                Assert(mask == 7, "Slots 0,1,2 should produce mask 7 (0b00000111)");
            }, ref passed, ref failed);

            RunTest("VoiceBitmask_Slot32AndAbove_HandledCorrectly", () =>
            {
                ulong mask = VoiceBitmaskGenerator.CalculateBitmask(new List<int> { 32 });
                Assert(mask == (1UL << 32), "Slot 32 should set bit 32 (4294967296)");
            }, ref passed, ref failed);

            RunTest("VoiceBitmask_SlotRanges_DoNotOverlap", () =>
            {
                var tSlots = new List<int> { 0, 1, 2 };
                var ctSlots = new List<int> { 3, 4, 5, 6 };
                ulong tMask = VoiceBitmaskGenerator.CalculateBitmask(tSlots);
                ulong ctMask = VoiceBitmaskGenerator.CalculateBitmask(ctSlots);
                Assert((tMask & ctMask) == 0, "T-side and CT-side slot masks must not overlap");
            }, ref passed, ref failed);

            RunTest("VoiceBitmask_AllVoicesMask_Is0xFFFFFFFFFFFFFFFF", () =>
            {
                ulong allMask = VoiceBitmaskGenerator.GetAllVoicesBitmask();
                Assert(allMask == Cs2VoiceConstants.AllVoicesMask, "All-voices bitmask should match Cs2VoiceConstants.AllVoicesMask");
            }, ref passed, ref failed);

            RunTest("Cs2Enums_TeamAndReasonCodeTranslations_EvaluatedCorrectly", () =>
            {
                Assert(Cs2Team.Terrorist.ToTeamCode() == "T", "Terrorist code is T");
                Assert(Cs2Team.CounterTerrorist.ToTeamCode() == "CT", "CounterTerrorist code is CT");
                Assert(Cs2BombSiteExtensions.ToSiteLabel(0) == "A", "Site 0 is A");
                Assert(Cs2BombSiteExtensions.ToSiteLabel(1) == "B", "Site 1 is B");
                Assert(Cs2RoundEndReasonExtensions.ToWinTypeString(9) == "Bomb Exploded", "Reason 9 is Bomb Exploded");
                Assert(Cs2VoiceConstants.DefaultTSideMask == 31UL, "TSideMask is 31");
                Assert(Cs2VoiceConstants.DefaultCtSideMask == 992UL, "CtSideMask is 992");
            }, ref passed, ref failed);

            RunTest("VoiceBitmask_OutOfRangeSlots_AreIgnored", () =>
            {
                ulong mask = VoiceBitmaskGenerator.CalculateBitmask(new List<int> { 0, 64, 100 });
                Assert(mask == 1, "Only slot 0 should contribute; slots 64+ must be ignored");
            }, ref passed, ref failed);

            RunTest("VoiceBitmask_NegativeSlots_AreIgnored", () =>
            {
                ulong mask = VoiceBitmaskGenerator.CalculateBitmask(new List<int> { -1, 0, -5 });
                Assert(mask == 1, "Negative slot indices must be ignored; only slot 0 contributes");
            }, ref passed, ref failed);

            RunTest("Security_SanitizeConfigFileName_StripsPathTraversalSequences", () =>
            {
                Assert(AppSettings.SanitizeConfigFileName("../../evil.cfg") == "evil", "Relative path traversal '../../evil.cfg' must sanitize to 'evil'");
                Assert(AppSettings.SanitizeConfigFileName("..\\..\\malicious") == "malicious", "Windows path traversal '..\\..\\malicious' must sanitize to 'malicious'");
                Assert(AppSettings.SanitizeConfigFileName("../../../etc/passwd") == "passwd", "Unix path traversal must sanitize to filename 'passwd'");
                Assert(AppSettings.SanitizeConfigFileName("") == "demopulse", "Empty filename falls back to 'demopulse'");
                Assert(AppSettings.SanitizeConfigFileName(null) == "demopulse", "Null filename falls back to 'demopulse'");
            }, ref passed, ref failed);

            RunTest("VoiceConfig_AutoSaveFailure_ReportsError", () =>
            {
                var settings = new AppSettings
                {
                    AutoSaveToCs2 = true,
                    Cs2CfgFolder = "C:\\NonExistentDirectoryPath12345"
                };
                bool success = DemoPulse.Interop.Handlers.GenerateVoiceCfgHandler.AutoSaveConfigToCs2Folder("test cfg", settings, out string? error);
                Assert(!success, "AutoSave should return false for non-existent folder");
                Assert(!string.IsNullOrWhiteSpace(error), "Error message should be populated");
            }, ref passed, ref failed);

            RunTest("MockFileSystemService_AutoSaveAndStreamParsing_WorksInMemory", () =>
            {
                var mockFs = new DemoPulse.Services.Providers.MockFileSystemService();
                mockFs.CreateDirectory("C:\\csgo\\cfg");
                var settings = new AppSettings
                {
                    AutoSaveToCs2 = true,
                    Cs2CfgFolder = "C:\\csgo\\cfg"
                };

                bool success = DemoPulse.Interop.Handlers.GenerateVoiceCfgHandler.AutoSaveConfigToCs2Folder("test cfg content", settings, mockFs, out string? error);
                Assert(success, "AutoSave with MockFileSystemService should succeed in memory");
                Assert(mockFs.FileExists("C:\\csgo\\cfg\\demopulse.cfg"), "File should exist in MockFileSystemService memory dictionary");
                Assert(mockFs.ReadAllText("C:\\csgo\\cfg\\demopulse.cfg") == "test cfg content", "Content should match");
            }, ref passed, ref failed);

            // ─── VoiceConfig CS2 Config Generation Tests ─────────────────────────────

            RunTest("VoiceConfig_GenerateCS2Config_ContainsRequiredCommands", () =>
            {
                string cfg = VoiceBitmaskGenerator.GenerateCS2Config("T", 0x1F);
                Assert(cfg.Contains("tv_listen_voice_indices"), "Config must contain tv_listen_voice_indices");
                Assert(cfg.Contains("sv_cheats 1"), "Config must enable sv_cheats 1");
                Assert(cfg.Contains("DemoPulse"), "Config must be tagged with DemoPulse");
            }, ref passed, ref failed);

            RunTest("VoiceConfig_CustomMask_IsUsedInConfig", () =>
            {
                ulong customMask = 0b0000_1111; // 15
                string cfg = VoiceBitmaskGenerator.GenerateCS2Config("CUSTOM", customMask);
                Assert(cfg.Contains("15"), "Config should contain the decimal value of mask 15");
            }, ref passed, ref failed);

            RunTest("VoiceConfig_AllMask_UsesMinusOne", () =>
            {
                ulong allMask = VoiceBitmaskGenerator.GetAllVoicesBitmask();
                string cfg = VoiceBitmaskGenerator.GenerateCS2Config("ALL", allMask);
                Assert(cfg.Contains("tv_listen_voice_indices -1"), "All-voices config must contain tv_listen_voice_indices -1");
                Assert(cfg.Contains("tv_listen_voice_indices_h -1"), "All-voices config must contain tv_listen_voice_indices_h -1");
            }, ref passed, ref failed);

            RunTest("VoiceConfig_CustomKeyBindings_AreEmittedInCS2Config", () =>
            {
                var settings = new AppSettings
                {
                    ConfigFileName = "my_custom_voice",
                    KeyBindT = "f5",
                    KeyBindCT = "f6",
                    KeyBindAll = "f7",
                    KeyBindMute = "f8",
                    KeyBindSpeedUp = "shift",
                    KeyBindSlowMo = "ctrl",
                    KeyBindPause = "space",
                    KeyBindResetSpeed = "r"
                };

                string cfg = VoiceBitmaskGenerator.GenerateCS2Config("T", 31, 31, 992, settings);
                Assert(cfg.Contains("bind \"f5\""), "Config must contain bind for f5 (T-side)");
                Assert(cfg.Contains("bind \"f6\""), "Config must contain bind for f6 (CT-side)");
                Assert(cfg.Contains("bind \"f7\""), "Config must contain bind for f7 (All comms)");
                Assert(cfg.Contains("bind \"f8\""), "Config must contain bind for f8 (Mute)");
                Assert(cfg.Contains("bind \"shift\" \"demo_timescale 2.0"), "Config must contain bind for 2x speedup");
                Assert(cfg.Contains("bind \"ctrl\" \"demo_timescale 0.5"), "Config must contain bind for slow mo");
                Assert(cfg.Contains("bind \"space\" \"demo_togglepause"), "Config must contain bind for pause");
                Assert(cfg.Contains("my_custom_voice.cfg"), "Config header must mention custom config name");
            }, ref passed, ref failed);

            RunTest("AppSettings_LoadAndSave_PersistsDefaults", () =>
            {
                var settings = AppSettings.Load();
                Assert(!string.IsNullOrWhiteSpace(settings.ConfigFileName), "ConfigFileName should not be empty");
                Assert(!string.IsNullOrWhiteSpace(settings.KeyBindT), "KeyBindT should not be empty");
                Assert(!string.IsNullOrWhiteSpace(settings.KeyBindSpeedUp), "KeyBindSpeedUp should not be empty");
            }, ref passed, ref failed);

            RunTest("AppSettings_SanitizeKeyBind_CleansInvalidInputs", () =>
            {
                Assert(AppSettings.SanitizeKeyBind("ctrl", "shift") == "ctrl", "Valid 'ctrl' should be preserved");
                Assert(AppSettings.SanitizeKeyBind("  SHIFT ", "b") == "shift", "Whitespace should be trimmed and lowercased");
                Assert(AppSettings.SanitizeKeyBind("b; echo hack", "b") == "bechohack", "Special characters like semicolons should be sanitized");
                Assert(AppSettings.SanitizeKeyBind("", "space") == "space", "Empty string should fall back to default");
                Assert(AppSettings.SanitizeKeyBind(null, "r") == "r", "Null string should fall back to default");
            }, ref passed, ref failed);

            RunTest("AppSettings_EnsureUniqueKeyBindings_ResolvesDuplicates", () =>
            {
                var settings = new AppSettings
                {
                    KeyBindSpeedUp = "ctrl",
                    KeyBindSlowMo = "ctrl" // Duplicate!
                };
                settings.SanitizeAll();
                Assert(settings.KeyBindSpeedUp != settings.KeyBindSlowMo, "Duplicate 'ctrl' key bindings must be automatically resolved to unique keys");
            }, ref passed, ref failed);

            // ─── Calculator Tests ────────────────────────────────────────────────────

            RunTest("RatingCalculator_BuildPlayerJson_ComputesExpectedStats", () =>
            {
                var p = new PlayerStats
                {
                    SlotIndex = 0,
                    Team = "T",
                    Name = "TestPlayer",
                    Kills = 20,
                    Deaths = 10,
                    Assists = 5,
                    TotalDamage = 2500,
                    Headshots = 10,
                    KastRounds = 15
                };
                int pId = 1;
                var dto = RatingCalculator.BuildPlayerJson(p, 20, ref pId);
                Assert(dto.Name == "TestPlayer", "Player name matches");
                Assert(dto.Kills == 20, "Kills match");
                Assert(dto.Adr == 125.0, "ADR is 2500 / 20 = 125.0");
                Assert(dto.HsPct == 50, "HS% is 10 / 20 = 50%");
                Assert(dto.Rating > 1.0, "Rating formula evaluates to high rating for 20K/10D/125ADR");
            }, ref passed, ref failed);

            RunTest("JsonSerialization_CamelCaseContractValidation", () =>
            {
                var p = new PlayerStats { SlotIndex = 0, Team = "T", Name = "ContractCheck", Kills = 10, Headshots = 5, KastRounds = 8, OpeningKills = 2, OpeningDeaths = 1, TradeKills = 3, TradeDeaths = 2, ClutchesWon = 1, MultiK2 = 2 };
                int pId = 1;
                var dto = RatingCalculator.BuildPlayerJson(p, 10, ref pId);
                var matchData = new DemoPulse.Models.Dto.MatchDataDto { Players = new List<DemoPulse.Models.Dto.PlayerDto> { dto } };
                var options = new System.Text.Json.JsonSerializerOptions { PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase };
                string json = System.Text.Json.JsonSerializer.Serialize(matchData, options);

                Assert(json.Contains("\"hsPct\":"), "JSON contract must contain 'hsPct'");
                Assert(json.Contains("\"kastPct\":"), "JSON contract must contain 'kastPct'");
                Assert(json.Contains("\"openingKills\":"), "JSON contract must contain 'openingKills'");
                Assert(json.Contains("\"tradeKills\":"), "JSON contract must contain 'tradeKills'");
                Assert(json.Contains("\"clutchesWon\":"), "JSON contract must contain 'clutchesWon'");
                Assert(json.Contains("\"multiKills\":"), "JSON contract must contain 'multiKills'");
                Assert(json.Contains("\"multiK2\":"), "JSON contract must contain 'multiK2'");
                Assert(json.Contains("\"ttkMs\":"), "JSON contract must contain 'ttkMs'");
            }, ref passed, ref failed);

            RunTest("DuelsMatrixCalculator_BuildDuelsJson_AggregatesPairsCorrectly", () =>
            {
                var tPlayer = new PlayerStats { SlotIndex = 0, Team = "T", Name = "TerroristOne" };
                var ctPlayer = new PlayerStats { SlotIndex = 1, Team = "CT", Name = "CounterOne" };
                var tPlayers = new List<PlayerStats> { tPlayer };
                var ctPlayers = new List<PlayerStats> { ctPlayer };
                var statsBySteamId = new Dictionary<ulong, PlayerStats>
                {
                    { 100, tPlayer },
                    { 200, ctPlayer }
                };
                var duelsByPair = new Dictionary<(ulong keyA, ulong keyB), DuelStats>
                {
                    { (100, 200), new DuelStats { KeyA = 100, KeyB = 200, WinsA = 3, WinsB = 1, HsA = 2, HsB = 1 } }
                };

                var duels = DuelsMatrixCalculator.BuildDuelsJson(
                    tPlayers, ctPlayers, statsBySteamId, duelsByPair, (id, name) => id);

                Assert(duels.Count == 1, "Should output exactly 1 pair duel");
                var d = duels[0];
                Assert(d.TWins == 3, "T player wins should be 3");
                Assert(d.CtWins == 1, "CT player wins should be 1");
                Assert(d.TotalDuels == 4, "Total duels should be 4");
            }, ref passed, ref failed);

            // ─── Domain Analyzer Unit Tests ──────────────────────────────────────────

            RunTest("RoundAnalyzer_OnRoundStartAndEnd_TracksScoresAndFacts", () =>
            {
                var roundAnalyzer = new RoundAnalyzer();
                roundAnalyzer.OnRoundStart();
                roundAnalyzer.OnBombPlanted(0); // Site A

                var roundInfo = roundAnalyzer.OnRoundEnd(3, 8); // CT win via Bomb Defused (reason 8)

                Assert(roundAnalyzer.RoundNumber == 1, "Round number should be 1");
                Assert(roundInfo.Winner == "CT", "Winner should be CT");
                Assert(roundInfo.WinType == "Bomb Defused", "Win type should be Bomb Defused");
                Assert(roundInfo.BombSite == "A", "Bomb site should be A");
                Assert(roundAnalyzer.FirstHalfCT == 1, "First half CT wins should be 1");
            }, ref passed, ref failed);

            RunTest("ClutchAnalyzer_Evaluates1vXClutches_Correctly", () =>
            {
                var clutchAnalyzer = new ClutchAnalyzer();
                var stats = new Dictionary<ulong, PlayerStats>
                {
                    { 1, new PlayerStats { Name = "HeroT", TeamNum = 2, Team = "T" } },
                    { 2, new PlayerStats { Name = "OtherT", TeamNum = 2, Team = "T" } },
                    { 3, new PlayerStats { Name = "CTOne", TeamNum = 3, Team = "CT" } },
                    { 4, new PlayerStats { Name = "CTTwo", TeamNum = 3, Team = "CT" } }
                };

                clutchAnalyzer.ResetRoundAliveSets(stats);

                // OtherT dies -> HeroT remains lone survivor (1v2 clutch triggered against CTOne and CTTwo)
                clutchAnalyzer.OnPlayerDeath(2, 2, stats);

                Assert(clutchAnalyzer.RoundTClutchSteamId == 1, "T player #1 triggered clutch situation");

                // HeroT kills CTOne, then CTTwo (wins the 1v2 round)
                clutchAnalyzer.OnPlayerDeath(3, 3, stats);
                clutchAnalyzer.OnPlayerDeath(4, 3, stats);

                var facts = new List<string>();
                clutchAnalyzer.EvaluateRoundEndClutches(1, 2, "Team Eliminated", stats, facts);

                Assert(stats[1].ClutchesWon == 1, "HeroT won 1 clutch");
                Assert(stats[1].C1v2 == 1, "HeroT won a 1v2 clutch");
                Assert(clutchAnalyzer.ClutchesList.Count == 1, "Recorded 1 clutch in list");
            }, ref passed, ref failed);

            RunTest("DuelAnalyzer_RecordDuel_CalculatesTTKAndHeadshots", () =>
            {
                var duelAnalyzer = new DuelAnalyzer();
                duelAnalyzer.OnRoundStart();

                var attacker = new PlayerStats { Name = "Attacker", TeamNum = 2 };
                var victim = new PlayerStats { Name = "Victim", TeamNum = 3 };

                // Hurt at tick 100
                duelAnalyzer.OnPlayerHurt(100, 200, 100);

                // Death at tick 116 (16 ticks * 15.625ms = 250ms fight time)
                duelAnalyzer.RecordDuel(100, 200, true, attacker, victim, 116);

                Assert(attacker.TtkList.Count == 1, "Attacker logged 1 TTK entry");
                Assert(attacker.TtkList[0] == 250, "TTK evaluated to 250ms");

                var pairKey = (100UL, 200UL);
                Assert(duelAnalyzer.DuelsByPair.ContainsKey(pairKey), "Duel pair logged in dictionary");
                var duel = duelAnalyzer.DuelsByPair[pairKey];
                Assert(duel.WinsA == 1, "Attacker (KeyA) has 1 win");
                Assert(duel.HsA == 1, "Attacker (KeyA) has 1 headshot win");
            }, ref passed, ref failed);

            RunTest("UtilityAnalyzer_TracksFlashEfficiencyAndDamage", () =>
            {
                var utilAnalyzer = new UtilityAnalyzer();
                var attacker = new PlayerStats { Name = "UtilityMaster", TeamNum = 2 };

                // Utility Damage
                utilAnalyzer.OnPlayerHurt(attacker, 45, "hegrenade");
                Assert(attacker.TotalDamage == 45, "Total damage updated to 45");
                Assert(attacker.UtilityDamage == 45, "Utility damage updated to 45");

                // Flashbang detonate & enemy blind
                utilAnalyzer.OnFlashbangDetonate(attacker);
                utilAnalyzer.OnPlayerBlind(attacker, 2, 3, 2.5f); // Blind enemy
                utilAnalyzer.OnPlayerBlind(attacker, 2, 2, 1.0f); // Team flash

                Assert(attacker.FlashesThrown == 1, "Flashes thrown count is 1");
                Assert(attacker.EnemiesBlinded == 1, "Enemies blinded count is 1");
                Assert(attacker.TeamFlashes == 1, "Team flashes count is 1");
                Assert(attacker.TotalBlindDuration == 2.5f, "Blind duration updated to 2.5s");
            }, ref passed, ref failed);

            // ─── IPC CommandDispatcher Unit Tests (No WPF Thread Required) ──────────

            RunTest("IPC_GetSettings_DispatchesSettingsDataMessage", () =>
            {
                var messenger = new TestUiMessenger();
                var dialogService = new TestDialogService();
                var demoService = new TestDemoService();
                var settings = new AppSettings { ConfigFileName = "test_config" };
                var dispatcher = DemoPulse.Interop.CommandDispatcher.CreateDefault(messenger, dialogService, demoService, settings);

                bool handled = dispatcher.Dispatch("GET_SETTINGS");

                Assert(handled, "GET_SETTINGS should be handled");
                Assert(messenger.LastResponse != null && messenger.LastResponse.Type == "SETTINGS_DATA", "Should send SETTINGS_DATA response");
                Assert(messenger.LastMessage!.Contains("test_config"), "Payload should serialize AppSettings");
            }, ref passed, ref failed);

            RunTest("IPC_ParseDemo_InvokesDemoService", () =>
            {
                var messenger = new TestUiMessenger();
                var dialogService = new TestDialogService();
                var demoService = new TestDemoService();
                var settings = new AppSettings();
                var dispatcher = DemoPulse.Interop.CommandDispatcher.CreateDefault(messenger, dialogService, demoService, settings);

                bool handled = dispatcher.Dispatch("PARSE_DEMO:match_de_inferno.dem");

                Assert(handled, "PARSE_DEMO should be handled");
                Assert(demoService.LastLoadedPath == "match_de_inferno.dem", "DemoService received correct path");
            }, ref passed, ref failed);

            RunTest("DemoService_DirectoryDrop_ReturnsParseError", () =>
            {
                var messenger = new TestUiMessenger();
                var demoService = new DemoService(messenger);
                string tempDir = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "test_folder_match.dem");
                if (!System.IO.Directory.Exists(tempDir)) System.IO.Directory.CreateDirectory(tempDir);

                try
                {
                    demoService.LoadDemoPathAsync(tempDir).GetAwaiter().GetResult();
                    Assert(messenger.LastResponse != null, "Response should be sent");
                    Assert(messenger.LastResponse!.Type == "DEMO_PARSE_ERROR", "Type should be DEMO_PARSE_ERROR");
                    Assert(messenger.LastResponse.Success == false, "Success should be false");
                    Assert(messenger.LastResponse.Error!.Contains("folder"), "Error should report target path is a folder");
                }
                finally
                {
                    if (System.IO.Directory.Exists(tempDir)) System.IO.Directory.Delete(tempDir);
                }
            }, ref passed, ref failed);

            RunTest("IPC_UnknownCommand_ReturnsFalse", () =>
            {
                var messenger = new TestUiMessenger();
                var dialogService = new TestDialogService();
                var demoService = new TestDemoService();
                var settings = new AppSettings();
                var dispatcher = DemoPulse.Interop.CommandDispatcher.CreateDefault(messenger, dialogService, demoService, settings);

                bool handled = dispatcher.Dispatch("UNKNOWN_COMMAND_XYZ");

                Assert(!handled, "Unknown command should return false");
            }, ref passed, ref failed);

            RunTest("IPC_JsonEnvelope_PreservesCorrelationId", () =>
            {
                var messenger = new TestUiMessenger();
                var dialogService = new TestDialogService();
                var demoService = new TestDemoService();
                var settings = new AppSettings { ConfigFileName = "test_config" };
                var dispatcher = DemoPulse.Interop.CommandDispatcher.CreateDefault(messenger, dialogService, demoService, settings);

                string jsonReq = "{\"id\":\"req_100\",\"command\":\"GET_SETTINGS\",\"payload\":null}";
                bool handled = dispatcher.Dispatch(jsonReq);

                Assert(handled, "JSON request GET_SETTINGS should be handled");
                Assert(messenger.LastResponse != null, "LastResponse should not be null");
                Assert(messenger.LastResponse!.Id == "req_100", "Correlation ID 'req_100' preserved");
                Assert(messenger.LastResponse.Type == "SETTINGS_DATA", "Type should be SETTINGS_DATA");
                Assert(messenger.LastResponse.Success == true, "Success should be true");
            }, ref passed, ref failed);

            RunTest("IPC_FailingHandler_IsCaughtSafelyWithoutCrashing", () =>
            {
                var messenger = new TestUiMessenger();
                var dialogService = new TestDialogService();
                var demoService = new TestDemoService();
                var settings = new AppSettings();
                var dispatcher = DemoPulse.Interop.CommandDispatcher.CreateDefault(messenger, dialogService, demoService, settings);

                dispatcher.Register(new ThrowingTestHandler());

                string jsonReq = "{\"id\":\"req_err_1\",\"command\":\"FAILING_CMD\",\"payload\":null}";
                bool handled = dispatcher.Dispatch(jsonReq);

                Assert(!handled, "Failing handler should return false");
                Assert(messenger.LastResponse != null, "LastResponse should be sent with error payload");
                Assert(messenger.LastResponse!.Id == "req_err_1", "Correlation ID preserved on error");
                Assert(messenger.LastResponse.Type == "COMMAND_ERROR", "Type should be COMMAND_ERROR");
                Assert(messenger.LastResponse.Success == false, "Success should be false");
                Assert(messenger.LastResponse.Error!.Contains("Simulated failure"), "Error message captured");
            }, ref passed, ref failed);

            RunTest("IoC_Container_RegistersAndResolvesAllServicesAndCommandHandlers", () =>
            {
                var services = new Microsoft.Extensions.DependencyInjection.ServiceCollection();
                services.AddSingleton<DemoPulse.AppSettings>(sp => new DemoPulse.AppSettings { ConfigFileName = "ioc_test" });
                services.AddSingleton<TestUiMessenger>();
                services.AddSingleton<DemoPulse.Services.IUiMessenger>(sp => sp.GetRequiredService<TestUiMessenger>());
                services.AddSingleton<DemoPulse.Services.IDialogService, TestDialogService>();
                services.AddSingleton<DemoPulse.Services.IFileSystemService, DemoPulse.Services.Providers.MockFileSystemService>();
                services.AddSingleton<DemoPulse.Services.IDemoService, TestDemoService>();

                var handlerTypes = typeof(DemoPulse.Interop.ICommandHandler).Assembly.GetTypes()
                    .Where(t => typeof(DemoPulse.Interop.ICommandHandler).IsAssignableFrom(t) && !t.IsAbstract && !t.IsInterface);

                foreach (var handlerType in handlerTypes)
                {
                    services.AddSingleton(typeof(DemoPulse.Interop.ICommandHandler), handlerType);
                }

                services.AddSingleton<DemoPulse.Interop.CommandDispatcher>();
                services.AddSingleton<DemoPulse.Interop.WebViewMessageRouter>();

                var provider = services.BuildServiceProvider();
                var dispatcher = provider.GetRequiredService<DemoPulse.Interop.CommandDispatcher>();
                var router = provider.GetRequiredService<DemoPulse.Interop.WebViewMessageRouter>();

                bool handled = dispatcher.Dispatch("GET_SETTINGS");
                Assert(handled, "IoC resolved dispatcher should handle GET_SETTINGS");
                Assert(router.Settings.ConfigFileName == "ioc_test", "Router received correct AppSettings via DI");
            }, ref passed, ref failed);

            RunTest("VoiceBitmask_EdgeSlots0_31_32_63_ProducesExactBitmasks", () =>
            {
                ulong mask0 = VoiceBitmaskService.CalculateBitmask(new List<int> { 0 });
                Assert(mask0 == 1UL, "Slot 0 mask must be 1");

                ulong mask31 = VoiceBitmaskService.CalculateBitmask(new List<int> { 31 });
                Assert(mask31 == (1UL << 31), "Slot 31 mask must be 1UL << 31");

                ulong mask32 = VoiceBitmaskService.CalculateBitmask(new List<int> { 32 });
                Assert(mask32 == (1UL << 32), "Slot 32 mask must be 1UL << 32");

                ulong mask63 = VoiceBitmaskService.CalculateBitmask(new List<int> { 63 });
                Assert(mask63 == (1UL << 63), "Slot 63 mask must be 1UL << 63");

                ulong combo = VoiceBitmaskService.CalculateBitmask(new List<int> { 0, 31, 32, 63 });
                ulong expected = (1UL << 0) | (1UL << 31) | (1UL << 32) | (1UL << 63);
                Assert(combo == expected, "Combination of edge slots 0, 31, 32, 63 must match bitwise OR");

                ulong duplicate = VoiceBitmaskService.CalculateBitmask(new List<int> { 0, 0, 31, 31 });
                Assert(duplicate == ((1UL << 0) | (1UL << 31)), "Duplicate slots must produce identical bitmask");
            }, ref passed, ref failed);

            RunTest("VoiceCfgHandlerSecurityTests_PathTraversalAttacksBlocked", () =>
            {
                var mockFs = new DemoPulse.Services.Providers.MockFileSystemService();
                mockFs.CreateDirectory("C:\\cs2\\cfg");

                var settingsEscaping = new AppSettings
                {
                    AutoSaveToCs2 = true,
                    Cs2CfgFolder = "C:\\cs2\\cfg",
                    ConfigFileName = "../../etc/hosts"
                };

                bool success = DemoPulse.Interop.Handlers.GenerateVoiceCfgHandler.AutoSaveConfigToCs2Folder("malicious cfg", settingsEscaping, mockFs, out string? error);
                Assert(success, "AutoSave should succeed because ConfigFileName is safely sanitized to 'hosts'");
                Assert(mockFs.FileExists("C:\\cs2\\cfg\\hosts.cfg"), "Config must be written safely inside target folder 'C:\\cs2\\cfg\\hosts.cfg'");
                Assert(!mockFs.FileExists("C:\\etc\\hosts.cfg"), "Config must not escape target folder");
            }, ref passed, ref failed);

            RunTest("DuelsMatrixCalculator_HeadToHeadMatrixCalculations", () =>
            {
                var tPlayer1 = new PlayerStats { SlotIndex = 0, Team = "T", Name = "s1mple" };
                var tPlayer2 = new PlayerStats { SlotIndex = 1, Team = "T", Name = "electronic" };
                var ctPlayer1 = new PlayerStats { SlotIndex = 2, Team = "CT", Name = "zywoo" };
                var ctPlayers = new List<PlayerStats> { ctPlayer1 };
                var tPlayers = new List<PlayerStats> { tPlayer1, tPlayer2 };

                var statsBySteamId = new Dictionary<ulong, PlayerStats>
                {
                    { 76561198000000001UL, tPlayer1 },
                    { 76561198000000002UL, tPlayer2 },
                    { 76561198000000003UL, ctPlayer1 }
                };

                var duelsByPair = new Dictionary<(ulong keyA, ulong keyB), DuelStats>
                {
                    { (76561198000000001UL, 76561198000000003UL), new DuelStats { KeyA = 76561198000000001UL, KeyB = 76561198000000003UL, WinsA = 5, WinsB = 2, HsA = 4, HsB = 1 } }
                };

                var duels = DuelsMatrixCalculator.BuildDuelsJson(tPlayers, ctPlayers, statsBySteamId, duelsByPair, (id, name) => id);
                Assert(duels.Count == 2, "Should output 2 pair duels (s1mple vs zywoo and electronic vs zywoo)");

                var d1 = duels.First(d => d.TName == "s1mple");
                Assert(d1.TWins == 5, "s1mple wins should be 5");
                Assert(d1.CtWins == 2, "zywoo wins should be 2");
                Assert(d1.TotalDuels == 7, "Total duels should be 7");
                Assert(d1.THsPct == 80, "s1mple headshot % should be 4/5 = 80%");

                var d2 = duels.First(d => d.TName == "electronic");
                Assert(d2.TotalDuels == 0, "electronic vs zywoo had 0 duels");
            }, ref passed, ref failed);

            RunTest("AppSettings_GetCs2GameFolder_ResolvesParentCsgoPath", () =>
            {
                var settings = new AppSettings { Cs2CfgFolder = @"C:\Program Files (x86)\Steam\steamapps\common\Counter-Strike Global Offensive\game\csgo\cfg" };
                string csgo = settings.GetCs2GameFolder();
                Assert(csgo.EndsWith("csgo", StringComparison.OrdinalIgnoreCase) || string.IsNullOrEmpty(csgo), "GetCs2GameFolder must resolve parent csgo directory if path exists");
            }, ref passed, ref failed);

            RunTest("RenameDemoHandler_RenamesFileOnDisk_AndDispatchesEvent", () =>
            {
                string tempDir = Path.Combine(Path.GetTempPath(), "DemoPulseTests_" + Guid.NewGuid().ToString("N"));
                Directory.CreateDirectory(tempDir);
                string originalFile = Path.Combine(tempDir, "original.dem");
                File.WriteAllText(originalFile, "mock demo binary data");

                var messenger = new TestUiMessenger();
                var dialog = new TestDialogService();
                var handler = new DemoPulse.Interop.Handlers.RenameDemoHandler(messenger, dialog);

                using var doc = JsonDocument.Parse(JsonSerializer.Serialize(new { currentPath = originalFile, newName = "renamed_match.dem" }));
                var req = new DemoPulse.Interop.Contracts.IpcRequest { Id = "test_1", Command = "RENAME_DEMO", Payload = doc.RootElement.Clone() };

                handler.Execute(req);

                string newFile = Path.Combine(tempDir, "renamed_match.dem");
                Assert(!File.Exists(originalFile), "Original file should no longer exist after rename");
                Assert(File.Exists(newFile), "Renamed file must exist on disk");

                try { Directory.Delete(tempDir, true); } catch { }
            }, ref passed, ref failed);

            RunTest("RealDemo_Faceit1_ParsingValidation", () =>
            {
                string faceitPath = @"C:\Program Files (x86)\Steam\steamapps\common\Counter-Strike Global Offensive\game\csgo\faceit1.dem";
                if (!System.IO.File.Exists(faceitPath))
                {
                    Console.WriteLine($"      [SKIP] {faceitPath} not found");
                    return;
                }

                Console.WriteLine($"      [PARSE] Reading and parsing {faceitPath}...");
                var matchData = DemoParserService.ParseDemoMatchDataAsync(faceitPath).GetAwaiter().GetResult();
                string json = DemoParserService.ParseDemoAsync(faceitPath).GetAwaiter().GetResult();

                Console.WriteLine($"      Map: {matchData.Meta.Map}, Server: {matchData.Meta.Server}, Score: T {matchData.Meta.ScoreT} - {matchData.Meta.ScoreCT} CT, Rounds: {matchData.Meta.RoundsCount}");
                Console.WriteLine($"      Parsed {matchData.Players.Count} players, {matchData.Duels.Count} duels, {matchData.Utility.Count} utility entries, {matchData.Rounds.Count} rounds, {matchData.Clutches.Count} clutches.");

                foreach (var p in matchData.Players)
                {
                    Console.WriteLine($"      Player '{p.Name}' ({p.Team}): K={p.Kills}, D={p.Deaths}, A={p.Assists}, ADR={p.Adr:F1}, Rating={p.Rating:F2}, Impact={p.Impact:F2}, Kast={p.KastPct:F1}%, HsPct={p.HsPct}%, K/D={p.KdRatio:F2}, K/R={p.KrRatio:F2}, MK={p.MultiKills}, 5K={p.MultiK5}, 4K={p.MultiK4}, 3K={p.MultiK3}, 2K={p.MultiK2}, OpeningK/D={p.OpeningKills}/{p.OpeningDeaths}, EntryAttempts={p.EntryAttempts}, EntrySucc={p.EntrySuccessPct}%, TradeK/D={p.TradeKills}/{p.TradeDeaths}, TradeSucc={p.TradeSuccessPct}%, ClutchesWon={p.ClutchesWon}, 1v1={p.C1v1}, 1v2={p.C1v2}, TTK={p.TtkMs}ms, TTD={p.TtdMs}ms");
                }

                Assert(matchData.Players.Count > 0, "Demo should contain players");
                Assert(matchData.Meta.RoundsCount > 0, "Demo should contain rounds");
            }, ref passed, ref failed);

            RunTest("RealDemo_UserSpecificDemo_ParsingValidation", () =>
            {
                string demoPath = @"C:\Users\abdul\Downloads\1-9130568d-f06d-4d61-8829-1a4ca7613061-1-1.dem";
                if (!File.Exists(demoPath))
                {
                    Console.WriteLine($"      [SKIP] File not found: {demoPath}");
                    return;
                }

                Console.WriteLine($"      [PARSE] Reading and parsing {demoPath}...");
                var matchData = DemoParserService.ParseDemoMatchDataAsync(demoPath).GetAwaiter().GetResult();
                Console.WriteLine($"      Map: {matchData.Meta.Map}, Server: {matchData.Meta.Server}, Score: T {matchData.Meta.ScoreT} - {matchData.Meta.ScoreCT} CT, Rounds: {matchData.Meta.RoundsCount}");
                Console.WriteLine($"      T Mask: 0x{matchData.VoiceConfig.TSideBitmask:X} ({matchData.VoiceConfig.TSideBitmask}), CT Mask: 0x{matchData.VoiceConfig.CtSideBitmask:X} ({matchData.VoiceConfig.CtSideBitmask})");
                Console.WriteLine($"      Parsed {matchData.Players.Count} players:");
                foreach (var p in matchData.Players)
                {
                    Console.WriteLine($"        Player '{p.Name}' ({p.Team}): Slot={p.SlotIndex}, K={p.Kills}, D={p.Deaths}");
                }
            }, ref passed, ref failed);

            // ─── Summary ──────────────────────────────────────────────────────────────

            Console.WriteLine();
            Console.WriteLine("=================================================");
            Console.WriteLine($"  TEST RESULTS: {passed} PASSED | {failed} FAILED");
            Console.WriteLine("=================================================");

            if (failed > 0)
            {
                Console.WriteLine();
                Console.WriteLine("NOTE: Integration tests against real .dem files");
                Console.WriteLine("      require a demo file and must be run manually.");
            }

            return failed == 0 ? 0 : 1;
        }

        private static void RunTest(string testName, Action testAction, ref int passed, ref int failed)
        {
            try
            {
                testAction();
                Console.WriteLine($" [PASS] {testName}");
                passed++;
            }
            catch (Exception ex)
            {
                Console.WriteLine($" [FAIL] {testName}: {ex.Message}");
                failed++;
            }
        }

        private static void Assert(bool condition, string message)
        {
            if (!condition)
                throw new Exception($"Assertion Failed: {message}");
        }
    }

    public class TestUiMessenger : IUiMessenger
    {
        public string? LastMessage { get; private set; }
        public DemoPulse.Interop.Contracts.IpcResponse? LastResponse { get; private set; }

        public void PostMessage(string message) => LastMessage = message;
        public void SendResponse(DemoPulse.Interop.Contracts.IpcResponse response)
        {
            LastResponse = response;
            LastMessage = System.Text.Json.JsonSerializer.Serialize(response);
        }
    }

    public class TestDialogService : IDialogService
    {
        public string? OpenFileResult { get; set; }
        public string? SaveFileResult { get; set; }
        public string? LastMessageBoxMessage { get; private set; }

        public string? ShowOpenFileDialog(string title, string filter, string? defaultFileName = null, bool checkFileExists = true) => OpenFileResult;
        public string? ShowSaveFileDialog(string title, string filter, string defaultFileName) => SaveFileResult;
        public void ShowMessageBox(string message, string caption, bool isWarning = false) => LastMessageBoxMessage = message;
    }

    public class TestDemoService : IDemoService
    {
        public string? LastLoadedPath { get; private set; }
        public string? LastRequestId { get; private set; }
        public DemoPulse.Models.Dto.MatchDataDto? CurrentMatchData { get; set; }

        public System.Threading.Tasks.Task LoadDemoPathAsync(string filePath) => LoadDemoPathAsync(filePath, null);

        public System.Threading.Tasks.Task LoadDemoPathAsync(string filePath, string? requestId)
        {
            LastLoadedPath = filePath;
            LastRequestId = requestId;
            return System.Threading.Tasks.Task.CompletedTask;
        }
    }

    public class ThrowingTestHandler : DemoPulse.Interop.ICommandHandler
    {
        public string CommandName => "FAILING_CMD";

        public void Execute(DemoPulse.Interop.Contracts.IpcRequest request)
        {
            throw new InvalidOperationException("Simulated failure in command execution handler");
        }
    }
}

