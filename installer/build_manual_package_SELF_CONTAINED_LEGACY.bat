@echo off
REM ============================================================
REM Script LEGACY para crear paquete Self-Contained
REM SOLO USAR EN CASOS ESPECIALES
REM ============================================================

echo ============================================================
echo   ADVERTENCIA: Script Self-Contained (LEGACY)
echo ============================================================
echo.
echo Este script crea un paquete SELF-CONTAINED (~100+ MB)
echo Solo uselo si el cliente NO PUEDE instalar .NET Runtime
echo.
echo La configuracion oficial del proyecto es FRAMEWORK-DEPENDENT
echo Ver: BUILD_CONFIGURATION.md
echo.
pause

REM Configurar variables
set PROJECT_ROOT=%~dp0..
set SRC_DIR=%PROJECT_ROOT%\src
set PUBLISH_DIR=%SRC_DIR%\publish\SGRRHH-SelfContained
set UPDATER_DIR=%SRC_DIR%\publish\Updater-SC-Temp
set INSTALLER_DIR=%PROJECT_ROOT%\installer
set OUTPUT_DIR=%INSTALLER_DIR%\manual-package

echo [1/6] Limpiando directorios anteriores...
if exist "%PUBLISH_DIR%" rmdir /s /q "%PUBLISH_DIR%"
if exist "%UPDATER_DIR%" rmdir /s /q "%UPDATER_DIR%"
if not exist "%OUTPUT_DIR%" mkdir "%OUTPUT_DIR%"
echo      Completado.
echo.

echo [2/6] Compilando SGRRHH.WPF (Self-Contained)...
cd "%SRC_DIR%\SGRRHH.WPF"
dotnet publish --configuration Release --runtime win-x64 --self-contained true --output "%PUBLISH_DIR%"
if errorlevel 1 (
    echo ERROR: Fallo la compilacion de SGRRHH.WPF
    pause
    exit /b 1
)
echo      Completado.
echo.

echo [3/6] Compilando SGRRHH.Updater (Self-Contained)...
cd "%SRC_DIR%\SGRRHH.Updater"
dotnet publish --configuration Release --runtime win-x64 --self-contained true --output "%UPDATER_DIR%"
if errorlevel 1 (
    echo ERROR: Fallo la compilacion de SGRRHH.Updater
    pause
    exit /b 1
)
echo      Completado.
echo.

echo [4/6] Copiando SGRRHH.Updater.exe a la carpeta principal...
copy "%UPDATER_DIR%\SGRRHH.Updater.exe" "%PUBLISH_DIR%\"
if errorlevel 1 (
    echo ERROR: Fallo la copia de SGRRHH.Updater.exe
    pause
    exit /b 1
)
echo      Completado.
echo.

echo [5/6] Creando archivo ZIP para distribucion...
cd "%PUBLISH_DIR%"
powershell -Command "Compress-Archive -Path * -DestinationPath '%OUTPUT_DIR%\SGRRHH-SelfContained-Install.zip' -Force"
if errorlevel 1 (
    echo ERROR: Fallo la creacion del ZIP
    pause
    exit /b 1
)
echo      Completado.
echo.

echo [6/6] Limpiando archivos temporales...
if exist "%UPDATER_DIR%" rmdir /s /q "%UPDATER_DIR%"
echo      Completado.
echo.

echo ============================================================
echo   PAQUETE SELF-CONTAINED CREADO
echo ============================================================
echo.
echo Los archivos estan disponibles en:
echo   1. Carpeta completa: %PUBLISH_DIR%
echo   2. Archivo ZIP:      %OUTPUT_DIR%\SGRRHH-SelfContained-Install.zip
echo.
echo RECORDATORIO: Este es un paquete LEGACY
echo Tamano aproximado: ~100-120 MB
echo Incluye todo el runtime .NET
echo.
echo ============================================================

pause
