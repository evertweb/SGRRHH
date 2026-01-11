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
    .\Clear-AllTablesExceptUsers.ps1
    Limpia todas las tablas excepto usuarios con confirmación
.EXAMPLE
    .\Clear-AllTablesExceptUsers.ps1 -Force
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
# (las tablas con claves foráneas deben limpiarse primero)
$tablesToClear = @(
    # Detalles y registros dependientes
    'detalles_actividad',
    'registros_diarios',
    
    # Documentos y asociaciones
    'documentos_empleado',
    'proyectos_empleados',
    
    # Nóminas y prestaciones (Fase 2)
    'nominas',
    'prestaciones',
    
    # Permisos y vacaciones
    'permisos',
    'vacaciones',
    
    # Contratos
    'contratos',
    
    # Empleados (tiene muchas dependencias)
    'empleados',
    
    # Proyectos y actividades
    'proyectos',
    'actividades',
    
    # Catálogos
    'cargos',
    'departamentos',
    'tipos_permiso',
    
    # Configuración y festivos
    'festivos_colombia',
    'configuracion_legal',
    'configuracion_sistema',
    
    # Auditoría
    'audit_logs'
)

# Mostrar información
Write-Host ""
Write-Host "═══════════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host "  LIMPIEZA DE BASE DE DATOS - EXCEPTO USUARIOS" -ForegroundColor Cyan
Write-Host "═══════════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host ""
Write-Host "Base de datos: " -NoNewline -ForegroundColor Yellow
Write-Host $DatabasePath -ForegroundColor White
Write-Host ""
Write-Host "Tablas que se limpiarán ($($tablesToClear.Count)):" -ForegroundColor Yellow
$tablesToClear | ForEach-Object { Write-Host "  • $_" -ForegroundColor Gray }
Write-Host ""
Write-Host "Tabla que se PRESERVARÁ:" -ForegroundColor Green
Write-Host "  • usuarios (con todos sus registros)" -ForegroundColor White
Write-Host ""

# Confirmación de seguridad
if (-not $Force) {
    Write-Host "[!] ADVERTENCIA: Esta operacion eliminara TODOS los datos excepto usuarios" -ForegroundColor Red
    Write-Host "[!] Esta accion NO SE PUEDE DESHACER" -ForegroundColor Red
    Write-Host ""
    $confirmation = Read-Host "¿Estás seguro de continuar? (escribe 'SI' para confirmar)"
    
    if ($confirmation -ne "SI") {
        Write-Host ""
        Write-Host "Operación cancelada por el usuario." -ForegroundColor Yellow
        exit 0
    }
}

Write-Host ""
Write-Host "Iniciando limpieza..." -ForegroundColor Cyan
Write-Host ""

try {
    # Buscar la DLL de SQLite en el proyecto
    $projectRoot = Split-Path -Parent (Split-Path -Parent $PSScriptRoot)
    $sqliteDll = Get-ChildItem -Path $projectRoot -Filter "System.Data.SQLite.dll" -Recurse -ErrorAction SilentlyContinue | 
                 Where-Object { $_.FullName -notmatch '\\obj\\' } | 
                 Select-Object -First 1
    
    if (-not $sqliteDll) {
        # Intentar cargar desde GAC o ubicaciones del sistema
        try {
            Add-Type -AssemblyName "System.Data.SQLite" -ErrorAction Stop
        }
        catch {
            Write-Host "ERROR: No se puede cargar System.Data.SQLite" -ForegroundColor Red
            Write-Host "Instala el paquete NuGet System.Data.SQLite o asegúrate de que esté disponible." -ForegroundColor Yellow
            exit 1
        }
    }
    else {
        Write-Host "Usando SQLite desde: $($sqliteDll.FullName)" -ForegroundColor Gray
        Add-Type -Path $sqliteDll.FullName
    }
    
    # Crear conexión
    $connectionString = "Data Source=$DatabasePath;Version=3;"
    $connection = New-Object System.Data.SQLite.SQLiteConnection($connectionString)
    $connection.Open()
    
    $totalRecordsDeleted = 0
    $successCount = 0
    $errorCount = 0
    
    # Deshabilitar foreign keys temporalmente para evitar problemas
    $disableFKCommand = $connection.CreateCommand()
    $disableFKCommand.CommandText = "PRAGMA foreign_keys = OFF;"
    $disableFKCommand.ExecuteNonQuery() | Out-Null
    
    # Iniciar transacción
    $transaction = $connection.BeginTransaction()
    
    foreach ($table in $tablesToClear) {
        try {
            # Contar registros antes de eliminar
            $countCommand = $connection.CreateCommand()
            $countCommand.CommandText = "SELECT COUNT(*) FROM $table;"
            $countCommand.Transaction = $transaction
            $count = [int]$countCommand.ExecuteScalar()
            
            if ($count -eq 0) {
                Write-Host "  [-] $table" -NoNewline -ForegroundColor DarkGray
                Write-Host " (ya esta vacia)" -ForegroundColor DarkGray
            }
            else {
                # Eliminar todos los registros
                $deleteCommand = $connection.CreateCommand()
                $deleteCommand.CommandText = "DELETE FROM $table;"
                $deleteCommand.Transaction = $transaction
                $rowsAffected = $deleteCommand.ExecuteNonQuery()
                
                Write-Host "  [OK] $table" -NoNewline -ForegroundColor Green
                Write-Host " ($rowsAffected registros eliminados)" -ForegroundColor Gray
                
                $totalRecordsDeleted += $rowsAffected
                $successCount++
            }
        }
        catch {
            Write-Host "  [ERROR] $table" -NoNewline -ForegroundColor Red
            Write-Host " (error: $($_.Exception.Message))" -ForegroundColor Red
            $errorCount++
        }
    }
    
    # Commit de la transacción
    $transaction.Commit()
    
    # Reactivar foreign keys
    $enableFKCommand = $connection.CreateCommand()
    $enableFKCommand.CommandText = "PRAGMA foreign_keys = ON;"
    $enableFKCommand.ExecuteNonQuery() | Out-Null
    
    # Optimizar base de datos
    Write-Host ""
    Write-Host "Optimizando base de datos..." -ForegroundColor Cyan
    $vacuumCommand = $connection.CreateCommand()
    $vacuumCommand.CommandText = "VACUUM;"
    $vacuumCommand.ExecuteNonQuery() | Out-Null
    Write-Host "  [OK] Base de datos optimizada" -ForegroundColor Green
    
    # Cerrar conexión
    $connection.Close()
    
    # Resumen
    Write-Host ""
    Write-Host "═══════════════════════════════════════════════════════════" -ForegroundColor Cyan
    Write-Host "  RESUMEN DE LIMPIEZA" -ForegroundColor Cyan
    Write-Host "═══════════════════════════════════════════════════════════" -ForegroundColor Cyan
    Write-Host ""
    Write-Host "Total de registros eliminados: " -NoNewline -ForegroundColor Yellow
    Write-Host $totalRecordsDeleted -ForegroundColor White
    Write-Host "Tablas limpiadas exitosamente: " -NoNewline -ForegroundColor Green
    Write-Host $successCount -ForegroundColor White
    
    if ($errorCount -gt 0) {
        Write-Host "Tablas con errores: " -NoNewline -ForegroundColor Red
        Write-Host $errorCount -ForegroundColor White
    }
    
    # Verificar usuarios preservados
    $connection2 = New-Object System.Data.SQLite.SQLiteConnection($connectionString)
    $connection2.Open()
    $userCountCommand = $connection2.CreateCommand()
    $userCountCommand.CommandText = "SELECT COUNT(*) FROM usuarios WHERE activo = 1;"
    $userCount = [int]$userCountCommand.ExecuteScalar()
    $connection2.Close()
    
    Write-Host ""
    Write-Host "Usuarios preservados: " -NoNewline -ForegroundColor Green
    Write-Host "$userCount usuarios activos" -ForegroundColor White
    Write-Host ""
    Write-Host "[OK] Limpieza completada exitosamente" -ForegroundColor Green
    Write-Host ""
}
catch {
    Write-Host ""
    Write-Host "ERROR: No se pudo completar la limpieza" -ForegroundColor Red
    Write-Host $_.Exception.Message -ForegroundColor Red
    
    if ($null -ne $transaction) {
        Write-Host "Revertiendo cambios..." -ForegroundColor Yellow
        $transaction.Rollback()
    }
    
    if ($null -ne $connection -and $connection.State -eq 'Open') {
        $connection.Close()
    }
    
    exit 1
}
