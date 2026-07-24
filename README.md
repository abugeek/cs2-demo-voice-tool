# CS2 Voice & Demo Manager 🎮🎧

A zero-GUI, high-performance utility for Counter-Strike 2. It parses `.dem` files instantly using WASM Rust bindings (`@laihoe/demoparser2`), calculates 32-bit signed voice bitmasks for team channels, and automatically configures a clean, ergonomic demo review layout in CS2.

---

## ⚡ Key Features

- **Zero-GUI Drag & Drop Workflow**: Simply drop any `.dem` file onto `cs2voice.cmd` to parse and configure your match in under **300ms**.
- **Automatic Team Voice Channel Filtering**: Dynamically separates Terrorist and Counter-Terrorist team voice channels by calculating 32-bit signed bitmask integers.
- **Universal Setup & Auto-Detection**: Searches Steam libraries across all drives (`C:`, `D:`, `E:`, etc.) and automatically creates `demo.cfg` if it does not exist.
- **YouTube-Style Playback & Review Binds**:
  - `SPACEBAR`: Instant Pause / Resume toggle.
  - `SHIFT`: Hold to Fast-Forward (4x speed), release to return to 1x normal speed.
  - `B` / `N` / `V`: Selective team voice switching (T-Only, CT-Only, or All Players).
  - `X`: Native CS2 X-Ray wallhack outline toggle.
  - `1` – `0`: Instant player spectating shortcuts.
- **Robust Error Handling**: Handles missing files, invalid extensions, compressed archives (`.gz`/`.zip`), and non-SourceTV/MM demo limitations cleanly.

---

## 🚀 Quick Download & Usage (No Installation Required!)

1. Download **[CS2-Voice-Manager-v1.0.0-Windows.zip](https://github.com/abugeek/cs2-demo-voice-tool/releases/download/v1.0.0/CS2-Voice-Manager-v1.0.0-Windows.zip)** from the [Releases Page](https://github.com/abugeek/cs2-demo-voice-tool/releases).
2. Extract the `.zip` archive anywhere on your computer.
3. Extract your FACEIT or SourceTV demo archive to get the `.dem` file.
4. **Drag & Drop** the `.dem` file onto **`cs2voice.cmd`**.
5. Launch **Counter-Strike 2**, open the developer console (`~`), and type:
   ```text
   exec demo
   playdemo match_filename
   ```

---

## 💻 Developer Setup (Build from Source)

If you want to modify or build from source:

```bash
git clone https://github.com/abugeek/cs2-demo-voice-tool.git
cd cs2-demo-voice-tool
npm install
node index.js "path/to/match.dem"
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

- **[Node.js](https://nodejs.org/)**
- **[@laihoe/demoparser2](https://github.com/LaihoE/demoparser)** (Source 2 WebAssembly Demo Parser)

---

## 📄 License

Distributed under the MIT License. See `LICENSE` for more information.
