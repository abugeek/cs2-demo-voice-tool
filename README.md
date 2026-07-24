# DemoPulse - CS2 Match Analyzer & Demo Manager 🎮🎧

**DemoPulse** is a 100% native, ultra-lightweight Windows desktop application built with C# .NET 8 WPF. It parses `.dem` files instantly, displays full match scoreboards (K/D/A/Damage/HS%), calculates 32-bit signed voice bitmasks for team channels, and configures a clean demo review layout in CS2.

---

## ⚡ Key Features

- **Ultra-Lightweight (~500 KB Release Download)**: Built with native C# Windows WPF without bundling Chromium or heavy runtimes.
- **Instant Launch & Zero Overhead**: Opens in `<30ms`, consuming `<15MB RAM` and 0% CPU while playing CS2.
- **CS2 Tactical Dark GUI & Drag-and-Drop**: Drag any `.dem` file directly into the application window or double-click `.dem` files from Windows Explorer.
- **Match Scoreboard & Damage Analytics**: Instant extraction of player kills, deaths, assists, total damage dealt, and headshot percentages.
- **Automatic Team Voice Channel Filtering**: Dynamically separates Terrorist and Counter-Terrorist team voice channels by calculating 32-bit signed bitmask integers.
- **1-Click Game Launch**: Click **"Launch & Watch in CS2"** to automatically copy the demo into your CS2 folder and start playback via Steam protocol.
- **YouTube-Style Playback & Review Binds**:
  - `SPACEBAR`: Instant Pause / Resume toggle.
  - `SHIFT`: Hold to Fast-Forward (4x speed), release to return to 1x normal speed.
  - `B` / `N` / `V`: Selective team voice switching (T-Only, CT-Only, or All Players).
  - `X`: Native CS2 X-Ray wallhack outline toggle.
  - `1` – `0`: Instant player spectating shortcuts.

---

## 🚀 Quick Download & Usage (500 KB Native Windows Release)

1. Download **[DemoPulse-v1.0.0-Native-5MB.zip](https://github.com/abugeek/cs2-demo-voice-tool/releases/download/v1.0.0/DemoPulse-v1.0.0-Native-5MB.zip)** from the [Releases Page](https://github.com/abugeek/cs2-demo-voice-tool/releases).
2. Extract the `.zip` archive anywhere on your computer.
3. Run **`DemoPulseNative.exe`** (or double-click any `.dem` file).
4. View your match statistics, then click **"Launch & Watch in CS2"**!

---

## 💻 Developer Setup (Build from Source)

```bash
git clone https://github.com/abugeek/cs2-demo-voice-tool.git
cd cs2-demo-voice-tool/DemoPulseNative
dotnet build
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

- **[C# .NET 8 WPF](https://dotnet.microsoft.com/)**
- **[Microsoft WebWindow WebView2](https://developer.microsoft.com/en-us/microsoft-edge/webview2/)**

---

## 📄 License

Distributed under the MIT License. See `LICENSE` for more information.
