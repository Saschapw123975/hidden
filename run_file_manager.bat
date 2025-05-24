@echo off
echo Closing any running instances of File Manager...
taskkill /F /IM FileManager.exe 2>nul

echo Building File Manager...
dotnet build --configuration Release

if %ERRORLEVEL% NEQ 0 (
    echo Build failed!
    pause
    exit /b %ERRORLEVEL%
)

echo Starting File Manager...
start "" "bin\Release\net6.0-windows\FileManager.exe" 