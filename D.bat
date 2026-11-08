
@echo off
setlocal
cls
title FNF Overlay Derleyici
color 0A

echo ===================================================
echo          FNF OVERLAY STANDALONE EXE YAPILIYOR
echo ===================================================
echo.
echo Kodlar derleniyor ve ikon gomuluyor, lutfen bekleyin...
echo.

pushd "%~dp0"

set "CSC=C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe"

if not exist "%CSC%" (
    color 0C
    echo [HATA] C# Compiler bulunamadi!
    goto :error
)

if not exist "src\FNFOverlay.cs" (
    color 0C
    echo [HATA] src klasorunde kaynak kodlar bulunamadi!
    goto :error
)

if not exist "icons\app.ico" (
    color 0C
    echo [HATA] icons\app.ico bulunamadi!
    goto :error
)

if not exist "config\supported_games.json" (
    color 0C
    echo [HATA] supported_games.json bulunamadi!
    goto :error
)

"%CSC%" /nologo /target:winexe ^
/win32icon:"icons\app.ico" ^
/resource:"icons\app.ico",FNFOverlay.ico ^
/resource:"config\supported_games.json",SupportedGames.json ^
/out:"FNFOverlay(v1.1).exe" ^
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
echo ===================================================
echo [OK] Derleme basariyla tamamlandi!
echo [OK] FNFOverlay(v1.1).exe olusturuldu!
echo ===================================================
echo.

popd
pause
exit /b 0

:error
echo.
echo [HATA] Derleme basarisiz oldu!
echo Kaynak dosyalarini ve klasor yapisini kontrol et.
echo.
popd
pause
exit /b 1
