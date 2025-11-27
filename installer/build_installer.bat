@echo off
REM ============================================================
REM Script de construcción del instalador SGRRHH
REM ============================================================

echo ============================================================
echo   SGRRHH - Construccion del Instalador
echo ============================================================
echo.

REM Configurar variables
set PROJECT_ROOT=%~dp0..
set SRC_DIR=%PROJECT_ROOT%\src
set PUBLISH_DIR=%SRC_DIR%\publish\SGRRHH
set INSTALLER_DIR=%PROJECT_ROOT%\installer
set OUTPUT_DIR=%INSTALLER_DIR%\output

REM Verificar que estamos en el directorio correcto
if not exist "%SRC_DIR%\SGRRHH.sln" (
    echo ERROR: No se encontro la solucion SGRRHH.sln
    echo Asegurese de ejecutar este script desde la carpeta installer
    pause
    exit /b 1
)

echo [1/4] Limpiando publicacion anterior...
if exist "%PUBLISH_DIR%" (
    rmdir /s /q "%PUBLISH_DIR%"
)
echo      Completado.
echo.

echo [2/4] Compilando proyecto en modo Release...
cd "%SRC_DIR%"
dotnet build --configuration Release
if errorlevel 1 (
    echo ERROR: Fallo la compilacion
    pause
    exit /b 1
)
echo      Completado.
echo.

echo [3/4] Publicando aplicacion...
cd "%SRC_DIR%\SGRRHH.WPF"
dotnet publish --configuration Release --runtime win-x64 --self-contained true --output "%PUBLISH_DIR%"
if errorlevel 1 (
    echo ERROR: Fallo la publicacion
    pause
    exit /b 1
)
echo      Completado.
echo.

echo [4/4] Creando instalador con Inno Setup...
REM Buscar Inno Setup en ubicaciones comunes
set ISCC_PATH=
if exist "C:\Program Files (x86)\Inno Setup 6\ISCC.exe" (
    set ISCC_PATH=C:\Program Files (x86)\Inno Setup 6\ISCC.exe
) else if exist "C:\Program Files\Inno Setup 6\ISCC.exe" (
    set ISCC_PATH=C:\Program Files\Inno Setup 6\ISCC.exe
)

if "%ISCC_PATH%"=="" (
    echo.
    echo ADVERTENCIA: No se encontro Inno Setup 6
    echo.
    echo Para crear el instalador, necesita:
    echo   1. Descargar Inno Setup 6 desde: https://jrsoftware.org/isdl.php
    echo   2. Instalarlo en su sistema
    echo   3. Ejecutar este script nuevamente
    echo.
    echo La aplicacion publicada esta disponible en:
    echo   %PUBLISH_DIR%
    echo.
    echo Puede ejecutar la aplicacion directamente desde esa carpeta.
    echo.
    pause
    exit /b 0
)

REM Crear directorio de salida si no existe
if not exist "%OUTPUT_DIR%" mkdir "%OUTPUT_DIR%"

REM Compilar el instalador
cd "%INSTALLER_DIR%"
"%ISCC_PATH%" SGRRHH_Setup.iss
if errorlevel 1 (
    echo ERROR: Fallo la creacion del instalador
    pause
    exit /b 1
)

echo.
echo ============================================================
echo   INSTALADOR CREADO EXITOSAMENTE
echo ============================================================
echo.
echo El instalador se encuentra en:
echo   %OUTPUT_DIR%\SGRRHH_Setup_1.0.0.exe
echo.
echo ============================================================

pause
