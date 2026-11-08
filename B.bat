
@echo off
setlocal
cls
title FNF Overlay Baslatici
color 0B

echo ===================================================
echo           FNF OVERLAY DERLENIYOR...
echo ===================================================
echo.

pushd "%~dp0"

set "CSC=C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe"
set "RUNNER=%TEMP%\FNFRunner.exe"

if not exist "%CSC%" (
    echo [HATA] C# Compiler bulunamadi!
    goto :error
)

if not exist "src\FNFOverlay.cs" (
    echo [HATA] src klasorunde kaynak kodlar bulunamadi!
    goto :error
)

if not exist "icons\FNFOverlay_48x.ico" (
    echo [HATA] FNFOverlay_48x.ico bulunamadi!
    goto :error
)

if not exist "config\supported_games.json" (
    echo [HATA] supported_games.json bulunamadi!
    goto :error
)

"%CSC%" /nologo /target:winexe ^
/resource:"icons\FNFOverlay_48x.ico",FNFOverlay.ico ^
/resource:"config\supported_games.json",SupportedGames.json ^
/out:"%RUNNER%" ^
/r:System.Windows.Forms.dll,System.Drawing.dll,System.dll,System.Core.dll,System.Runtime.Serialization.dll ^
src\AnimationController.cs ^
src\LoadingPanel.cs ^
src\CreatorsPanel.cs ^
src\InfoPanel.cs ^
src\GoodByePanel.cs ^
src\ThemePalette.cs ^
src\SettingsPanel.cs ^
src\FNFOverlay.cs

if errorlevel 1 goto :error

echo.
echo [OK] Derleme basarili!
echo [OK] Uygulama baslatiliyor...
echo.
echo ===================================================
echo * Programi kapatmak icin BACKSPACE tusuna bas.
echo * Bu CMD penceresini kapatmak icin herhangi bir tusa bas.
echo ===================================================
echo.

start "" /D "%~dp0" "%RUNNER%"

popd
pause
exit /b 0

:error
echo.
color 0C
echo [HATA] Derleme sirasinda bir sorun olustu!
echo Yukaridaki hatayi kontrol et.
echo.
popd
pause
exit /b 1
