# 🤖 AGENTS.md - DemoPulse AI Agent Instructions

Welcome, AI Assistant! This document provides essential instructions, architectural guidelines, and codebase conventions for building and extending **DemoPulse**.

---

## 🎯 Project Overview
**DemoPulse** is a lightweight, native Windows application (.NET 8 WPF + WebView2) for Counter-Strike 2 (CS2) match demo analysis, player statistics, 2D radar trajectory replays, and automated voice bitmask config generation.

---

## ⚡ Tech Stack & Architecture
- **Desktop Host**: .NET 8.0 WPF (`DemoPulse.csproj`, `MainWindow.xaml`, `MainWindow.xaml.cs`).
- **UI Engine**: Microsoft WebView2 (`Microsoft.Web.WebView2`) rendering Vanilla HTML5 / CSS3 / JavaScript.
- **Styling**: Vanilla CSS custom properties (`--bg-color`, `--accent-gold`, etc.), dark esports gaming theme.
- **Communication Bridge**: `window.chrome.webview.postMessage` (JS -> C#) and `webView.CoreWebView2.PostWebMessageAsString` (C# -> JS).

---

## 🛑 Critical Agent Rules
1. **No Heavy Frameworks**: Never introduce Electron, Node.js binaries, or heavy dependencies. Keep the binary under 10 MB.
2. **Build Verification**: Always verify edits by running `dotnet build` using `run_command`. Zero compilation warnings or errors allowed.
3. **Preserve WebView2 Bridge**: Any new C# command must handle IPC via `CoreWebView2_WebMessageReceived` in `MainWindow.xaml.cs`.
4. **CSS Design System**: Maintain the dark gold/slate esports theme defined in CSS custom properties inside `ui/index.html`.

---

## 🛠️ Quick Commands for Agents
- **Build Project**: `dotnet build DemoPulse.csproj`
- **Run Test Mode**: `dotnet run --project DemoPulse.csproj`
- **Publish Release**: `dotnet publish DemoPulse.csproj -c Release -r win-x64 --self-contained false -p:PublishSingleFile=true -o .\dist`

---

## 🎨 Design System & UI Consistency Rules
1. **Color Token Hierarchy**:
   - Primary Background: `--bg-color: #0B0D10`
   - Panel & Container Background: `--panel-bg: #14171D` with border `--panel-border: #222733`
   - Primary Action Accent: `--accent-gold: #E4AE39` (buttons, active states, key metrics)
   - **T-Side (Terrorists)**: `--accent-orange: #FF7327` (Orange cards, badges, borders, player text)
   - **CT-Side (Counter-Terrorists)**: `--accent-blue: #3A96FF` (Blue cards, badges, borders, player text)
   - Success / Rating: `--accent-green: #27C93F`
   - Danger / Alert: `--accent-red: #FF4757`
2. **Team Visual Distinction Rule**:
   - Never display player names or slots without explicit team badges (`🟧 T` vs `🟦 CT`) and team-colored borders/backgrounds.
   - All player lists, voice bitmask slot cards, scoreboards, and matrix duels MUST immediately communicate team affiliation through high-contrast orange vs blue visuals.
3. **App Startup UX**:
   - App startup displays only the clean **Drag & Drop Zone**. Match statistics, dashboards, and voice bitmask configuration panels reveal only after a demo file is loaded.

