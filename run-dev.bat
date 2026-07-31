@echo off
echo ===================================================================
echo   DEMOPULSE - Running in Development / Test Mode
echo ===================================================================
echo.
dotnet run --project "%~dp0DemoPulse.csproj"
echo.
if %ERRORLEVEL% NEQ 0 (
    echo [!] App exited with code %ERRORLEVEL%
    pause
)
