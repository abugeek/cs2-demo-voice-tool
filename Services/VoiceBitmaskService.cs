using System.Collections.Generic;
using DemoPulse.Models;

namespace DemoPulse.Services
{
    public static class VoiceBitmaskService
    {
        public static ulong CalculateBitmask(List<int> playerIndices)
        {
            ulong bitmask = 0;
            foreach (int idx in playerIndices)
            {
                if (idx >= 0 && idx < 64)
                    bitmask |= (1UL << idx);
            }
            return bitmask;
        }

        public static ulong GetAllVoicesBitmask() => Cs2VoiceConstants.AllVoicesMask;

        private static string FormatVoiceCommands(ulong mask)
        {
            if (mask == Cs2VoiceConstants.AllVoicesMask)
            {
                return "tv_listen_voice_indices -1; tv_listen_voice_indices_h -1";
            }
            uint low = (uint)(mask & 0xFFFFFFFF);
            uint high = (uint)(mask >> 32);
            return $"tv_listen_voice_indices {low}; tv_listen_voice_indices_h {high}";
        }

        public static string GenerateCS2Config(string mode, ulong? customMask = null, ulong? tMask = null, ulong? ctMask = null, AppSettings? settings = null)
        {
            settings ??= AppSettings.Load();
            ulong mask = customMask ?? GetAllVoicesBitmask();
            ulong tBitmask = tMask ?? Cs2VoiceConstants.DefaultTSideMask;
            ulong ctBitmask = ctMask ?? Cs2VoiceConstants.DefaultCtSideMask;

            string bindT = !string.IsNullOrWhiteSpace(settings.KeyBindT) ? settings.KeyBindT : "b";
            string bindCT = !string.IsNullOrWhiteSpace(settings.KeyBindCT) ? settings.KeyBindCT : "n";
            string bindAll = !string.IsNullOrWhiteSpace(settings.KeyBindAll) ? settings.KeyBindAll : "v";
            string bindMute = !string.IsNullOrWhiteSpace(settings.KeyBindMute) ? settings.KeyBindMute : "m";

            string bindSpeedUp = !string.IsNullOrWhiteSpace(settings.KeyBindSpeedUp) ? settings.KeyBindSpeedUp : "shift";
            string bindSlowMo = !string.IsNullOrWhiteSpace(settings.KeyBindSlowMo) ? settings.KeyBindSlowMo : "ctrl";
            string bindPause = !string.IsNullOrWhiteSpace(settings.KeyBindPause) ? settings.KeyBindPause : "space";
            string bindResetSpeed = !string.IsNullOrWhiteSpace(settings.KeyBindResetSpeed) ? settings.KeyBindResetSpeed : "r";

            var sb = StringBuilderCache.Acquire(1024);
            sb.Append("// =========================================================\n");
            sb.Append("// DemoPulse CS2 Voice Channel & Playback Control Config\n");
            sb.Append("// Config Name: ").Append(settings.ConfigFileName).Append(".cfg\n");
            sb.Append("// Active Mode: ").Append(mode.ToUpperInvariant()).Append(" (Bitmask: ").Append(mask).Append(" / 0x").AppendFormat("{0:X16}", mask).Append(")\n");
            sb.Append("// =========================================================\n");
            sb.Append("sv_cheats 1\n\n");
            sb.Append("// Set initial active voice channel index bitmask\n");
            sb.Append(FormatVoiceCommands(mask)).Append("\n\n");
            sb.Append("// CS2 Voice Channel Hotkeys\n");
            sb.Append("bind \"").Append(bindT).Append("\" \"").Append(FormatVoiceCommands(tBitmask)).Append("; echo [DemoPulse] Voice Channel: T-Side Only (0x").AppendFormat("{0:X16}", tBitmask).Append(")\"\n");
            sb.Append("bind \"").Append(bindCT).Append("\" \"").Append(FormatVoiceCommands(ctBitmask)).Append("; echo [DemoPulse] Voice Channel: CT-Side Only (0x").AppendFormat("{0:X16}", ctBitmask).Append(")\"\n");
            sb.Append("bind \"").Append(bindAll).Append("\" \"").Append(FormatVoiceCommands(Cs2VoiceConstants.AllVoicesMask)).Append("; echo [DemoPulse] Voice Channel: All Comms (0xFFFFFFFFFFFFFFFF)\"\n");
            sb.Append("bind \"").Append(bindMute).Append("\" \"").Append(FormatVoiceCommands(Cs2VoiceConstants.MutedVoiceMask)).Append("; echo [DemoPulse] Voice Channel: Muted (0x0)\"\n\n");
            sb.Append("// CS2 Demo Replay Playback Controls\n");
            sb.Append("bind \"").Append(bindSpeedUp).Append("\" \"demo_timescale 2.0; echo [DemoPulse] Replay Speed: 2x Fast Forward\"\n");
            sb.Append("bind \"").Append(bindSlowMo).Append("\" \"demo_timescale 0.5; echo [DemoPulse] Replay Speed: 0.5x Slow Motion\"\n");
            sb.Append("bind \"").Append(bindPause).Append("\" \"demo_togglepause; echo [DemoPulse] Replay Paused/Resumed\"\n");
            sb.Append("bind \"").Append(bindResetSpeed).Append("\" \"demo_timescale 1.0; echo [DemoPulse] Replay Speed: 1.0x Normal\"\n\n");
            sb.Append("echo \"[DemoPulse] Config '").Append(settings.ConfigFileName).Append(".cfg' loaded successfully! Voice (")
              .Append(bindT.ToUpper()).Append('/').Append(bindCT.ToUpper()).Append('/').Append(bindAll.ToUpper()).Append('/').Append(bindMute.ToUpper())
              .Append(") Playback (").Append(bindSpeedUp.ToUpper()).Append('/').Append(bindSlowMo.ToUpper()).Append('/').Append(bindPause.ToUpper()).Append('/').Append(bindResetSpeed.ToUpper()).Append(").\"\n");

            return StringBuilderCache.GetStringAndRelease(sb);
        }
    }
}
