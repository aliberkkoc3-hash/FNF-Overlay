@echo off
cls
title FNF Overlay Derleyici
color 0a
echo ===================================================
echo          FNF OVERLAY STANDALONE EXE YAPILIYOR
echo ===================================================
echo.
echo Kodlar derleniyor ve ikon gomuluyor, lutfen bekleyin...
echo.

pushd "%~dp0"

C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe /nologo /target:winexe /win32icon:"icons\app.ico" /resource:"icons\FNFOverlay_48x_.ico",FNFOverlay.ico /resource:"config\supported_games.json",SupportedGames.json /out:"FNFOverlay(v1.1).exe" /r:System.Windows.Forms.dll,System.Drawing.dll,System.dll,System.Core.dll,System.Runtime.Serialization.dll AnimationController.cs LoadingPanel.cs CreatorsPanel.cs InfoPanel.cs GoodByePanel.cs ThemePalette.cs SettingsPanel.cs FNFOverlay.cs

if %errorlevel% neq 0 (
    echo.
    color 0c
    echo [HATA] Derleme basarisiz oldu! Yukaridaki hatalari kontrol et.
    echo.
    pause
    exit
)

echo.
echo ===================================================
echo [OK] Tebrikler! Derleme islemi basariyla tamamlandi.
echo [OK] Klasorunuzde ikonlu 'FNFOverlay(v1.1).exe' olusturuldu!
echo ===================================================
echo.
pause