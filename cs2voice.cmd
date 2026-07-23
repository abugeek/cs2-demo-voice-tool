@echo off
setlocal enabledelayedexpansion
set "NODE_BIN=C:\Program Files\nodejs\node.exe"
set "TOOL_DIR=C:\Users\abdul\Downloads\cs2-demo-voice-tool"

if "%~1"=="" (
    echo ===================================================================
    echo              CS2 VOICE BITMASK AUTO-CONFIGURATOR                   
    echo ===================================================================
    echo  To use: Drag a .dem file from File Explorer and DROP IT ONTO this file!
    echo ===================================================================
    echo.
    echo Press any key to exit...
    pause >nul
    exit /b 1
)

echo ===================================================================
echo  PROCESSING DEMO FILE...
echo ===================================================================
echo.

"%NODE_BIN%" "%TOOL_DIR%\index.js" "%~1"

echo.
echo ===================================================================
echo  Done! Press any key to exit...
echo ===================================================================
pause >nul
