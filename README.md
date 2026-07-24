# DemoPulse - CS2 Match Analyzer & Demo Manager 🎮🎧

**DemoPulse** is a high-performance CS2 desktop application built with a CS2 Tactical Dark GUI. It parses `.dem` files instantly using WASM Rust bindings (`@laihoe/demoparser2`), displays full match scoreboards (K/D/A/Damage/HS%), calculates 32-bit signed voice bitmasks for team channels, and configures a clean demo review layout in CS2.

---

## ⚡ Features

- **CS2 Tactical Dark GUI & Drag-and-Drop**: Drag any `.dem` file directly into the application window or double-click `.dem` files from Windows Explorer.
- **Match Scoreboard & Damage Analytics**: Instant extraction of player kills, deaths, assists, total damage dealt, and headshot percentages.
- **Automatic Team Voice Channel Filtering**: Dynamically separates Terrorist and Counter-Terrorist team voice channels by calculating 32-bit signed bitmask integers.
- **1-Click Game Launch**: Click **"Launch & Watch in CS2"** to automatically copy the demo into your CS2 folder and start playback via Steam protocol.
- **Zero-Lag & Zero-Overhead**: Runs with minimal memory footprint (`<40MB RAM`) and 0% CPU utilization while CS2 is running.
- **YouTube-Style Playback & Review Binds**:
  - `SPACEBAR`: Instant Pause / Resume toggle.
  - `SHIFT`: Hold to Fast-Forward (4x speed), release to return to 1x normal speed.
  - `B` / `N` / `V`: Selective team voice switching (T-Only, CT-Only, or All Players).
  - `X`: Native CS2 X-Ray wallhack outline toggle.
  - `1` – `0`: Instant player spectating shortcuts.

---

## 🚀 Quick Download & Usage (Standalone Windows App)

1. Download **[DemoPulse-v1.0.0-Windows-GUI.zip](https://github.com/abugeek/cs2-demo-voice-tool/releases/download/v1.0.0/DemoPulse-v1.0.0-Windows-GUI.zip)** from the [Releases Page](https://github.com/abugeek/cs2-demo-voice-tool/releases).
2. Extract the `.zip` archive anywhere on your computer.
3. Run **`DemoPulse.exe`** (or drag any `.dem` file onto it).
4. View your match statistics, then click **"Launch & Watch in CS2"**!

---

## 💻 Developer Setup (Build from Source)

```bash
git clone https://github.com/abugeek/cs2-demo-voice-tool.git
cd cs2-demo-voice-tool
npm install
npm start
```

---

## ⌨️ Controls Cheat Sheet

| Key | Action | Description |
| :--- | :--- | :--- |
| **`SPACE`** | Pause / Resume | Toggles playback pause and play (like YouTube) |
| **HOLD `SHIFT`** | Fast Forward (4x) | Speeds up playback while held, returns to 1x on release |
| **`B`** | Terrorist Voice | Listen to T-side team voice chat only |
| **`N`** | Counter-Terrorist Voice | Listen to CT-side team voice chat only |
| **`V`** | All Voice | Listen to both teams simultaneously |
| **`1` – `0`** | Spectate Player | Jump directly to player 1 through 10 |
| **`←` / `→`** | Skip -10s / +10s | Rewind or fast-forward by 10 seconds |
| **`X`** | Toggle X-Ray | Toggle player outlines through walls |
| **`C`** | Toggle Demo UI | Open or close the graphical timeline panel |
| **`H`** | Toggle Clean HUD | Hides UI elements for cinema/clip recording |
| **`R`** | Exit Demo Mode | Resets voice bitmasks and restores your standard `autoexec.cfg` |

---

## 🛠️ Built With

- **[Electron](https://www.electronjs.org/)**
- **[Node.js](https://nodejs.org/)**
- **[@laihoe/demoparser2](https://github.com/LaihoE/demoparser)** (Source 2 WebAssembly Demo Parser)

---

## 📄 License

Distributed under the MIT License. See `LICENSE` for more information.
