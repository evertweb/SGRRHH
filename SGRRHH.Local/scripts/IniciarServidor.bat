@echo off
title SGRRHH Local - Servidor
color 0A
cd /d C:\SGRRHH

echo ============================================================
echo                SGRRHH LOCAL - SERVIDOR
echo ============================================================
echo.
echo Iniciando aplicacion...
echo.
echo El navegador se abrira automaticamente...
echo.
echo Logs en tiempo real. Presione Ctrl+C para detener.
echo ============================================================

C:\SGRRHH\SGRRHH.Local.Server.exe

echo.
echo ============================================================
echo La aplicacion se ha detenido.
echo Presione cualquier tecla para cerrar esta ventana...
pause >nul
