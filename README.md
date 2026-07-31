# 🎮 DemoPulse - CS2 Match & Tactics Analyzer

DemoPulse is a high-performance, lightweight native Windows application for analyzing Counter-Strike 2 match `.dem` files, viewing player stats, 2D radar trajectory replays, multi-kills breakdown, and configuring voice channels.

---

## 🚀 Development & Test Environment

You no longer need to publish or build a release package every time you want to test a feature! You can run and debug DemoPulse in **Test Mode** immediately.

### 1. Running in Test Mode (Instant Dev Loop)
- **Option A (VS Code / F5)**: Open this folder in VS Code and press **`F5`** (or go to **Run and Debug** -> **DemoPulse (Test Mode)**).
- **Option B (Batch Script)**: Double-click **`run-dev.bat`** in File Explorer.
- **Option C (Terminal)**: Run `dotnet run` in your terminal.

---

## 📁 Project Structure

```
DemoPulse_Project/
├── DemoPulse.csproj        # Main .NET 8 WPF Project file
├── DemoPulse.sln           # Visual Studio / IDE Solution File
├── App.xaml / .cs          # Application Entry Point
├── MainWindow.xaml / .cs   # Native Window Container & WebView2 Host
├── ui/
│   └── index.html          # Web UI (Stats, 2D Radar, Multi-kills, CS2 Launcher)
├── .vscode/
│   ├── launch.json         # F5 Debugger Configuration
│   └── tasks.json          # Build & Run tasks
├── run-dev.bat             # 1-Click Development / Test Launcher
├── build-release.bat       # 1-Click Single-File Release Builder
└── .gitignore              # Ignores build binaries (bin/, obj/, dist/)
```

---

## ⚙️ How to Edit & Add Features

1. **Modifying UI & Features (HTML/CSS/JS)**:
   - Edit [ui/index.html](file:///C:/Users/abdul/Downloads/DemoPulse_Project/ui/index.html).
   - Press **F5** or run `run-dev.bat` to test your changes immediately.

2. **Modifying Native C# Logic**:
   - Edit [MainWindow.xaml.cs](file:///C:/Users/abdul/Downloads/DemoPulse_Project/MainWindow.xaml.cs).
   - Add new bridge events between WebView2 (`webView.CoreWebView2.PostWebMessageAsString(...)`) and C# (`CoreWebView2_WebMessageReceived`).

---

## 📦 Building a Release Version

When you are ready to distribute a release version:
- Double-click **`build-release.bat`** (or run `dotnet publish -c Release -r win-x64 --self-contained false -p:PublishSingleFile=true -o .\dist`).
- The standalone release executable will be saved in the `dist\` folder.
