@echo off
cls
title FNF Overlay Baslatici
color 0b
echo ===================================================
echo           FNF OVERLAY DERLENIYOR...
echo ===================================================
echo.

pushd "%~dp0"

C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe /nologo /target:winexe /resource:"icons\FNFOverlay_48x_.ico",FNFOverlay.ico /resource:"config\supported_games.json",SupportedGames.json /out:"%temp%\FNFRunner.exe" /r:System.Windows.Forms.dll,System.Drawing.dll,System.dll,System.Core.dll,System.Runtime.Serialization.dll AnimationController.cs LoadingPanel.cs CreatorsPanel.cs InfoPanel.cs GoodByePanel.cs ThemePalette.cs SettingsPanel.cs FNFOverlay.cs

if %errorlevel% neq 0 (
    echo.
    color 0c
    echo [HATA] Derleme sirasinda bir sorun olustu! Yukaridaki hatayi kontrol et.
    echo.
    pause
    goto :eof
)

echo [OK] Derleme basarili!
echo [OK] Uygulama baslatildi.
echo.
echo ===================================================
echo * Programi kapatmak icin klavyeden 'BACKSPACE' tusuna basabilirsin.
echo * Bu CMD penceresini kapatmak icin herhangi bir tusa basabilirsin.
echo ===================================================
echo.

start "" /D "%~dp0" "%temp%\FNFRunner.exe"
pause
popd