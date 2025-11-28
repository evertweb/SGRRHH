@echo off
REM ============================================================
REM Script de construcción del paquete SGRRHH
REM MODO: Framework-Dependent (requiere .NET 8 Runtime)
REM ============================================================

echo ============================================================
echo   SGRRHH - Paquete de Distribucion (Framework-Dependent)
echo ============================================================
echo.
echo NOTA: Este paquete requiere .NET 8 Desktop Runtime instalado
echo       Descarga: https://dotnet.microsoft.com/download/dotnet/8.0
echo.

REM Configurar variables
set PROJECT_ROOT=%~dp0..
set SRC_DIR=%PROJECT_ROOT%\src
set PUBLISH_DIR=%SRC_DIR%\publish\SGRRHH
set UPDATER_DIR=%SRC_DIR%\publish\Updater-Temp
set INSTALLER_DIR=%PROJECT_ROOT%\installer
set OUTPUT_DIR=%INSTALLER_DIR%\manual-package

echo [1/6] Limpiando directorios anteriores...
if exist "%PUBLISH_DIR%" rmdir /s /q "%PUBLISH_DIR%"
if exist "%UPDATER_DIR%" rmdir /s /q "%UPDATER_DIR%"
if not exist "%OUTPUT_DIR%" mkdir "%OUTPUT_DIR%"
echo      Completado.
echo.

echo [2/6] Compilando SGRRHH.WPF (Framework-Dependent)...
cd "%SRC_DIR%\SGRRHH.WPF"
dotnet publish --configuration Release --runtime win-x64 --self-contained false --output "%PUBLISH_DIR%"
if errorlevel 1 (
    echo ERROR: Fallo la compilacion de SGRRHH.WPF
    pause
    exit /b 1
)
echo      Completado.
echo.

echo [3/6] Compilando SGRRHH.Updater (Framework-Dependent)...
cd "%SRC_DIR%\SGRRHH.Updater"
dotnet publish --configuration Release --runtime win-x64 --self-contained false --output "%UPDATER_DIR%"
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
powershell -Command "Compress-Archive -Path * -DestinationPath '%OUTPUT_DIR%\SGRRHH-Install.zip' -Force"
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
echo   PAQUETE CREADO EXITOSAMENTE
echo ============================================================
echo.
echo Los archivos estan disponibles en:
echo   1. Carpeta completa: %PUBLISH_DIR%
echo   2. Archivo ZIP:      %OUTPUT_DIR%\SGRRHH-Install.zip
echo.
echo ============================================================
echo   RECORDATORIO IMPORTANTE
echo ============================================================
echo.
echo Los clientes DEBEN tener instalado:
echo   - .NET 8.0 Desktop Runtime (x64)
echo   - Descargar: https://dotnet.microsoft.com/download/dotnet/8.0
echo.
echo Ventajas de Framework-Dependent:
echo   - Paquete mas ligero (~20-30 MB vs ~100+ MB)
echo   - Actualizaciones mas rapidas
echo   - Mejor rendimiento y seguridad (runtime actualizado)
echo.
echo ============================================================

pause
