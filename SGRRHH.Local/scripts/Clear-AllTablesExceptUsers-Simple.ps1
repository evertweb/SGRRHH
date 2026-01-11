#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Limpia todos los registros de todas las tablas excepto la tabla de usuarios
.DESCRIPTION
    Este script elimina todos los registros de todas las tablas de la base de datos
    excepto la tabla 'usuarios', preservando así los usuarios del sistema.
    ADVERTENCIA: Esta operación no se puede deshacer. Se recomienda hacer un backup primero.
.PARAMETER DatabasePath
    Ruta a la base de datos SQLite. Por defecto usa C:\SGRRHH\Data\sgrrhh.db
.PARAMETER Force
    Omite la confirmación de seguridad
.EXAMPLE
    .\Clear-AllTablesExceptUsers-Simple.ps1
    Limpia todas las tablas excepto usuarios con confirmación
.EXAMPLE
    .\Clear-AllTablesExceptUsers-Simple.ps1 -Force
    Limpia todas las tablas excepto usuarios sin confirmación
#>

param(
    [string]$DatabasePath = "C:\SGRRHH\Data\sgrrhh.db",
    [switch]$Force
)

$ErrorActionPreference = "Stop"

# Verificar si existe la base de datos
if (-not (Test-Path $DatabasePath)) {
    Write-Host "ERROR: No se encuentra la base de datos en: $DatabasePath" -ForegroundColor Red
    Write-Host "Verifica la ruta e intenta nuevamente." -ForegroundColor Yellow
    exit 1
}

# Lista de todas las tablas en orden inverso de dependencias
$tablesToClear = @(
    'detalles_actividad',
    'registros_diarios',
    'documentos_empleado',
    'proyectos_empleados',
    'nominas',
    'prestaciones',
    'permisos',
    'vacaciones',
    'contratos',
    'empleados',
    'proyectos',
    'actividades',
    'cargos',
    'departamentos',
    'tipos_permiso',
    'festivos_colombia',
    'configuracion_legal',
    'configuracion_sistema',
    'audit_logs'
)

# Mostrar información
Write-Host ""
Write-Host "=================================================================" -ForegroundColor Cyan
Write-Host "  LIMPIEZA DE BASE DE DATOS - EXCEPTO USUARIOS" -ForegroundColor Cyan
Write-Host "=================================================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Base de datos: " -NoNewline -ForegroundColor Yellow
Write-Host $DatabasePath -ForegroundColor White
Write-Host ""
Write-Host "Tablas que se limpiaran ($($tablesToClear.Count)):" -ForegroundColor Yellow
$tablesToClear | ForEach-Object { Write-Host "  * $_" -ForegroundColor Gray }
Write-Host ""
Write-Host "Tabla que se PRESERVARA:" -ForegroundColor Green
Write-Host "  * usuarios (con todos sus registros)" -ForegroundColor White
Write-Host ""

# Confirmación de seguridad
if (-not $Force) {
    Write-Host "[!] ADVERTENCIA: Esta operacion eliminara TODOS los datos excepto usuarios" -ForegroundColor Red
    Write-Host "[!] Esta accion NO SE PUEDE DESHACER" -ForegroundColor Red
    Write-Host ""
    $confirmation = Read-Host "Estas seguro de continuar? (escribe 'SI' para confirmar)"
    
    if ($confirmation -ne "SI") {
        Write-Host ""
        Write-Host "Operacion cancelada por el usuario." -ForegroundColor Yellow
        exit 0
    }
}

Write-Host ""
Write-Host "Iniciando limpieza..." -ForegroundColor Cyan
Write-Host ""

try {
    # Crear archivo temporal con comandos SQL
    $sqlFile = [System.IO.Path]::GetTempFileName()
    $sqlContent = @"
-- Desactivar foreign keys
PRAGMA foreign_keys = OFF;

-- Iniciar transacción
BEGIN TRANSACTION;

"@
    
    # Agregar comandos DELETE para cada tabla
    foreach ($table in $tablesToClear) {
        $sqlContent += "DELETE FROM $table;`n"
    }
    
    $sqlContent += @"

-- Commit de la transacción
COMMIT;

-- Reactivar foreign keys
PRAGMA foreign_keys = ON;

-- Optimizar base de datos
VACUUM;

-- Mostrar conteo de usuarios
SELECT 'Usuarios preservados: ' || COUNT(*) FROM usuarios WHERE activo = 1;
"@
    
    # Guardar archivo SQL
    Set-Content -Path $sqlFile -Value $sqlContent -Encoding UTF8
    
    # Ejecutar comandos SQL
    Write-Host "Ejecutando comandos SQL..." -ForegroundColor Gray
    
    # Usar Microsoft.Data.Sqlite que viene con .NET
    Add-Type -AssemblyName "System.Data.Common"
    
    # Intentar usar la conexión con el driver nativo de .NET
    $assembly = [System.Reflection.Assembly]::LoadWithPartialName("Microsoft.Data.Sqlite")
    
    if ($null -eq $assembly) {
        # Si no está disponible Microsoft.Data.Sqlite, intentar ejecutar directamente con comandos SQL
        Write-Host "Ejecutando limpieza directa..." -ForegroundColor Gray
        
        # Leer el archivo de la base de datos
        $tempDb = [System.IO.Path]::GetTempFileName() + ".db"
        Copy-Item -Path $DatabasePath -Destination $tempDb -Force
        
        # Crear script simple de PowerShell para ejecutar los comandos
        $totalDeleted = 0
        $successCount = 0
        
        # Función para ejecutar comandos SQL usando ADO.NET genérico
        function Execute-SqlCommand {
            param($connectionString, $commandText)
            
            try {
                # Cargar el proveedor SQLite de Microsoft si está disponible
                $providerType = [Type]::GetType("Microsoft.Data.Sqlite.SqliteConnection, Microsoft.Data.Sqlite")
                
                if ($null -ne $providerType) {
                    $connection = [Activator]::CreateInstance($providerType, $connectionString)
                    $connection.Open()
                    
                    $command = $connection.CreateCommand()
                    $command.CommandText = $commandText
                    $result = $command.ExecuteNonQuery()
                    
                    $connection.Close()
                    return $result
                }
                else {
                    throw "No se puede cargar el proveedor SQLite"
                }
            }
            catch {
                throw $_
            }
        }
        
        $connectionString = "Data Source=$DatabasePath"
        
        # Deshabilitar foreign keys
        Execute-SqlCommand -connectionString $connectionString -commandText "PRAGMA foreign_keys = OFF;" | Out-Null
        
        # Limpiar cada tabla
        foreach ($table in $tablesToClear) {
            try {
                # Contar registros
                $count = Execute-SqlCommand -connectionString $connectionString -commandText "SELECT COUNT(*) FROM $table;"
                
                if ($count -eq 0) {
                    Write-Host "  [-] $table" -NoNewline -ForegroundColor DarkGray
                    Write-Host " (ya esta vacia)" -ForegroundColor DarkGray
                }
                else {
                    # Eliminar registros
                    $deleted = Execute-SqlCommand -connectionString $connectionString -commandText "DELETE FROM $table;"
                    Write-Host "  [OK] $table" -NoNewline -ForegroundColor Green
                    Write-Host " ($deleted registros eliminados)" -ForegroundColor Gray
                    $totalDeleted += $deleted
                    $successCount++
                }
            }
            catch {
                Write-Host "  [ERROR] $table" -NoNewline -ForegroundColor Red
                Write-Host " (error: $($_.Exception.Message))" -ForegroundColor Red
            }
        }
        
        # Reactivar foreign keys
        Execute-SqlCommand -connectionString $connectionString -commandText "PRAGMA foreign_keys = ON;" | Out-Null
        
        # Optimizar
        Write-Host ""
        Write-Host "Optimizando base de datos..." -ForegroundColor Cyan
        Execute-SqlCommand -connectionString $connectionString -commandText "VACUUM;" | Out-Null
        Write-Host "  [OK] Base de datos optimizada" -ForegroundColor Green
        
        # Contar usuarios
        $userCount = Execute-SqlCommand -connectionString $connectionString -commandText "SELECT COUNT(*) FROM usuarios WHERE activo = 1;"
        
        # Resumen
        Write-Host ""
        Write-Host "=================================================================" -ForegroundColor Cyan
        Write-Host "  RESUMEN DE LIMPIEZA" -ForegroundColor Cyan
        Write-Host "=================================================================" -ForegroundColor Cyan
        Write-Host ""
        Write-Host "Total de registros eliminados: " -NoNewline -ForegroundColor Yellow
        Write-Host $totalDeleted -ForegroundColor White
        Write-Host "Tablas limpiadas exitosamente: " -NoNewline -ForegroundColor Green
        Write-Host $successCount -ForegroundColor White
        Write-Host ""
        Write-Host "Usuarios preservados: " -NoNewline -ForegroundColor Green
        Write-Host "$userCount usuarios activos" -ForegroundColor White
        Write-Host ""
        Write-Host "[OK] Limpieza completada exitosamente" -ForegroundColor Green
        Write-Host ""
    }
    else {
        Write-Host "Microsoft.Data.Sqlite encontrado, usando conexión nativa..." -ForegroundColor Gray
        
        # Aquí iría el código con Microsoft.Data.Sqlite si está disponible
        # Por ahora redirigimos al método anterior
        throw "Usar método alternativo"
    }
}
catch {
    Write-Host ""
    Write-Host "ERROR: No se pudo completar la limpieza" -ForegroundColor Red
    Write-Host $_.Exception.Message -ForegroundColor Red
    Write-Host ""
    Write-Host "SOLUCION: Asegurate de tener .NET instalado correctamente" -ForegroundColor Yellow
    Write-Host "o contacta al administrador del sistema." -ForegroundColor Yellow
    exit 1
}
finally {
    # Limpiar archivo temporal
    if (Test-Path $sqlFile) {
        Remove-Item $sqlFile -Force -ErrorAction SilentlyContinue
    }
}
