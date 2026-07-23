const fs = require('fs');
const path = require('path');
const { parseTicks, parseHeader } = require('@laihoe/demoparser2');

// Terminal ANSI Color Codes
const colors = {
    reset: "\x1b[0m",
    bright: "\x1b[1m",
    dim: "\x1b[2m",
    red: "\x1b[31m",
    green: "\x1b[32m",
    yellow: "\x1b[33m",
    blue: "\x1b[34m",
    cyan: "\x1b[36m",
    white: "\x1b[37m",
    bgGreen: "\x1b[42m"
};

function printBanner() {
    console.log(`\n${colors.cyan}${colors.bright}`);
    console.log(`╔═══════════════════════════════════════════════════════════════════╗`);
    console.log(`║                  CS2 VOICE & DEMO MANAGER v1.0                    ║`);
    console.log(`╚═══════════════════════════════════════════════════════════════════╝${colors.reset}\n`);
}

let cachedCS2Path = null;
function findCS2CfgPath() {
    if (cachedCS2Path) return cachedCS2Path;

    const candidateDirs = [
        "C:\\Program Files (x86)\\Steam\\steamapps\\common\\Counter-Strike Global Offensive\\game\\csgo\\cfg",
        "C:\\Program Files\\Steam\\steamapps\\common\\Counter-Strike Global Offensive\\game\\csgo\\cfg",
        "D:\\SteamLibrary\\steamapps\\common\\Counter-Strike Global Offensive\\game\\csgo\\cfg",
        "E:\\SteamLibrary\\steamapps\\common\\Counter-Strike Global Offensive\\game\\csgo\\cfg",
        "F:\\SteamLibrary\\steamapps\\common\\Counter-Strike Global Offensive\\game\\csgo\\cfg",
        "G:\\SteamLibrary\\steamapps\\common\\Counter-Strike Global Offensive\\game\\csgo\\cfg"
    ];

    for (const dir of candidateDirs) {
        if (fs.existsSync(dir)) {
            cachedCS2Path = path.join(dir, "demo.cfg");
            return cachedCS2Path;
        }
    }
    return null;
}

async function main() {
    const startTime = Date.now();
    printBanner();

    const args = process.argv.slice(2);
    if (args.length === 0) {
        console.log(`${colors.yellow}===================================================================`);
        console.log(`  HOW TO USE THIS TOOL (SIMPLE 3-STEP GUIDE):`);
        console.log(`  -----------------------------------------------------------------`);
        console.log(`  1. Drag any CS2 demo (.dem) file and DROP IT onto 'cs2voice.cmd'`);
        console.log(`  2. Launch Counter-Strike 2, open console (~) and type: exec demo`);
        console.log(`  3. Start your demo (playdemo matchname) and use your controls!`);
        console.log(`===================================================================${colors.reset}`);
        process.exit(1);
    }

    let demoPath = args[0];

    if (demoPath.endsWith('.gz') || demoPath.endsWith('.zip')) {
        console.log(`${colors.red}${colors.bright}[X] ERROR: Compressed archive file detected! (${path.basename(demoPath)})`);
        console.log(`${colors.yellow}[!] CS2 cannot play .gz or .zip files directly.`);
        console.log(`[i] Please right-click the file -> Extract (7-Zip/WinRAR) -> Drop the extracted .dem file here!${colors.reset}`);
        process.exit(1);
    }

    if (!demoPath.endsWith('.dem')) {
        console.log(`${colors.red}${colors.bright}[X] ERROR: Invalid file dropped! (${path.basename(demoPath)})`);
        console.log(`${colors.yellow}[i] Please make sure you are dropping a valid Counter-Strike 2 match file ending in '.dem'${colors.reset}`);
        process.exit(1);
    }

    if (!fs.existsSync(demoPath)) {
        console.log(`${colors.red}${colors.bright}[X] ERROR: Could not find file: ${demoPath}${colors.reset}`);
        process.exit(1);
    }

    console.log(`${colors.blue}[+] Analyzing Match Demo: ${colors.white}${colors.bright}${path.basename(demoPath)}${colors.reset}`);

    try {
        const [header, playersData] = await Promise.all([
            Promise.resolve().then(() => parseHeader(demoPath)),
            Promise.resolve().then(() => parseTicks(demoPath, ['team_num', 'user_id', 'steamid'], [500]))
        ]);

        if (header) {
            console.log(`${colors.dim}[+] Map: ${header.map_name || 'Unknown'} | Match Server: ${header.server_name || 'FACEIT / GOTV'}${colors.reset}`);
        }

        if (!playersData || playersData.length === 0) {
            console.log(`${colors.red}${colors.bright}[X] ERROR: No player voice data found inside this demo file!${colors.reset}`);
            process.exit(1);
        }

        let ctSlots = [];
        let tSlots = [];
        let allSlots = [];

        console.log(`\n${colors.cyan}-------------------------------------------------------------------`);
        console.log(` MATCH TEAMS & PLAYER VOICE CHANNELS DETECTED:`);
        console.log(`-------------------------------------------------------------------${colors.reset}`);

        for (let i = 0; i < playersData.length; i++) {
            const p = playersData[i];
            const slot = p.user_id;
            const team = p.team_num; // 2 = T, 3 = CT
            const name = p.name || `Player_${slot}`;
            
            allSlots.push(slot);

            if (team === 2) {
                tSlots.push(slot);
                console.log(` ${colors.yellow}[TERRORISTS]         ${name.padEnd(25)}${colors.reset}`);
            } else if (team === 3) {
                ctSlots.push(slot);
                console.log(` ${colors.blue}[COUNTER-TERRORISTS] ${name.padEnd(25)}${colors.reset}`);
            }
        }

        let tLow = 0, tHigh = 0;
        for (let i = 0; i < tSlots.length; i++) {
            const s = tSlots[i];
            if (s < 32) tLow |= (1 << s); else tHigh |= (1 << (s - 32));
        }

        let ctLow = 0, ctHigh = 0;
        for (let i = 0; i < ctSlots.length; i++) {
            const s = ctSlots[i];
            if (s < 32) ctLow |= (1 << s); else ctHigh |= (1 << (s - 32));
        }

        let allLow = 0, allHigh = 0;
        for (let i = 0; i < allSlots.length; i++) {
            const s = allSlots[i];
            if (s < 32) allLow |= (1 << s); else allHigh |= (1 << (s - 32));
        }

        let cs2CfgPath = findCS2CfgPath();

        if (!cs2CfgPath) {
            console.log(`${colors.red}[X] Could not locate your Counter-Strike 2 installation path automatically.${colors.reset}`);
            process.exit(0);
        }

        const CLEAN_DEMO_CFG = `// =============================================================================
//               CS2 SWISS-CLOCK PERFECT DEMO REVIEW CONFIG (demo.cfg)
// =============================================================================
// Auto-generated for current match

echo "=========================================================="
echo "===   CS2 ULTIMATE DEMO REVIEW CONFIG LOADED (MATCH)   ==="
echo "=========================================================="

// 1. VOICE CHAT CONFIGURATION
voice_enable 1
snd_voipvolume 1.0
tv_listen_voice_indices ${allLow}
tv_listen_voice_indices_h ${allHigh}

// 2. PLAYBACK CONTROLS (SPACEBAR = PAUSE/PLAY TOGGLE, SHIFT = HOLD TO FAST FORWARD)
bind "SPACE" "demo_togglepause"

alias "+fw" "demo_timescale 4"
alias "-fw" "demo_timescale 1"
bind "SHIFT" "+fw"

// 3. ALTERNATE PLAYBACK SPEED KEYS
bind "F5"    "demo_togglepause"     // F5: Toggle Pause / Resume
bind "F6"    "demo_timescale 0.25"  // F6: 0.25x Slow Motion
bind "F7"    "demo_timescale 1"     // F7: 1.0x Normal Speed
bind "F8"    "demo_timescale 4"     // F8: 4.0x Fast Forward
bind "F9"    "demo_timescale 10"    // F9: 10.0x Ultra Speed

// 4. SECONDS SKIP (ARROW KEYS)
bind "LEFTARROW"  "demo_goto -640 relative"
bind "RIGHTARROW" "demo_goto 640 relative"
bind "UPARROW"    "demo_goto 1280 relative"
bind "DOWNARROW"  "demo_goto -1280 relative"

// 5. INSTANT CS2 PLAYER SPECTATING
bind "1" "slot1"
bind "2" "slot2"
bind "3" "slot3"
bind "4" "slot4"
bind "5" "slot5"
bind "6" "slot6"
bind "7" "slot7"
bind "8" "slot8"
bind "9" "slot9"
bind "0" "slot10"

// 6. MATCH VOICE DIRECT TOGGLE BINDS
bind "v" "tv_listen_voice_indices ${allLow}; tv_listen_voice_indices_h ${allHigh}"
bind "b" "tv_listen_voice_indices ${tLow}; tv_listen_voice_indices_h ${tHigh}"
bind "n" "tv_listen_voice_indices ${ctLow}; tv_listen_voice_indices_h ${ctHigh}"

// 7. VISUAL & UI CONTROLS (X-Ray & Cinema Mode)
bind "x" "toggle spec_show_xray"
bind "c" "demoui"
bind "h" "cl_draw_only_deathnotices !cl_draw_only_deathnotices"

// 8. CLEAN EXIT (RESETS VOICE & RESTORES PLAYING AUTOEXEC)
alias "enddemo" "tv_listen_voice_indices 0; tv_listen_voice_indices_h 0; exec autoexec; echo '>>> DEMO MODE EXITED. PLAYING CONFIG RESTORED!'"
bind "r" "enddemo"
`;

        fs.writeFileSync(cs2CfgPath, CLEAN_DEMO_CFG, 'utf8');

        const elapsedTime = Date.now() - startTime;
        console.log(`\n${colors.bgGreen}${colors.white}${colors.bright}  [SUCCESS] CS2 Demo Config written in ${elapsedTime}ms!  ${colors.reset}\n`);

        console.log(`${colors.bright}${colors.white}╔═══════════════════════════════════════════════════════════════════╗`);
        console.log(`║                  🎮 HOW TO WATCH IN CS2 (CHEAT SHEET)            ║`);
        console.log(`╠═══════════════════════════════════════════════════════════════════╣`);
        console.log(`║ 1. Launch CS2 & open console (~) -> Type: ${colors.yellow}exec demo${colors.white}              ║`);
        console.log(`║ 2. Load your match demo         -> Type: ${colors.yellow}playdemo <filename>${colors.white}     ║`);
        console.log(`║                                                                   ║`);
        console.log(`║ 🎙️ VOICE KEYS:                                                    ║`);
        console.log(`║    - Press ${colors.yellow}[B]${colors.white} : Listen to ${colors.yellow}TERRORIST TEAM ONLY (${tLow})${colors.white}            ║`);
        console.log(`║    - Press ${colors.blue}[N]${colors.white} : Listen to ${colors.blue}COUNTER-TERRORIST TEAM ONLY (${ctLow})${colors.white}    ║`);
        console.log(`║    - Press ${colors.green}[V]${colors.white} : Listen to ${colors.green}ALL PLAYERS (${allLow})${colors.reset}${colors.white}                      ║`);
        console.log(`║                                                                   ║`);
        console.log(`║ ⌨️ DEMO CONTROLS:                                                 ║`);
        console.log(`║    - Press ${colors.yellow}[SPACE]${colors.white} : Pause / Play Toggle (Like YouTube)           ║`);
        console.log(`║    - HOLD ${colors.yellow}[SHIFT]${colors.white}  : Fast Forward (4x) while held!                 ║`);
        console.log(`║    - RELEASE ${colors.yellow}[SHIFT]${colors.white}: Returns to Normal Speed (1x)                ║`);
        console.log(`║    - Press [1 - 0] : Spectate Player 1 through 10                 ║`);
        console.log(`║    - Press [← / →] : Skip 10 seconds back / forward               ║`);
        console.log(`║    - Press [F6-F9] : Slow Motion (0.25x) to Fast Forward (10x)    ║`);
        console.log(`║    - Press [X]     : Toggle X-Ray (Wallhack View)                 ║`);
        console.log(`║    - Press [R]     : Exit Demo Mode & restore normal play config  ║`);
        console.log(`╚═══════════════════════════════════════════════════════════════════╝${colors.reset}\n`);

    } catch (err) {
        console.error(`\n${colors.red}${colors.bright}[X] ERROR Parsing Demo:${colors.reset}`, err.message || err);
    }
}

main();
