@echo off
echo ===================================================================
echo   DEMOPULSE - Packaging Optimized Release
echo ===================================================================
echo.
dotnet publish "%~dp0DemoPulse.csproj" -c Release -r win-x64 --self-contained false -p:PublishSingleFile=true -o "%~dp0dist"
echo.
echo ===================================================================
echo   Done! Release binary created in .\dist\DemoPulse.exe
echo ===================================================================
pause
