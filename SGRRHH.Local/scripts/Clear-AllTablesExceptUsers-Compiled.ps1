#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Limpia todos los registros de todas las tablas excepto la tabla de usuarios
.DESCRIPTION
    Este script usa el proyecto SGRRHH para limpiar la base de datos.
    Ejecuta un comando C# compilado en tiempo real que usa las dependencias del proyecto.
.PARAMETER DatabasePath
    Ruta a la base de datos SQLite. Por defecto usa C:\SGRRHH\Data\sgrrhh.db
.PARAMETER Force
    Omite la confirmación de seguridad
#>

param(
    [string]$DatabasePath = "C:\SGRRHH\Data\sgrrhh.db",
    [switch]$Force
)

$ErrorActionPreference = "Stop"

# Verificar si existe la base de datos
if (-not (Test-Path $DatabasePath)) {
    Write-Host "ERROR: No se encuentra la base de datos en: $DatabasePath" -ForegroundColor Red
    exit 1
}

# Lista de tablas
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
Write-Host "Base de datos: $DatabasePath" -ForegroundColor White
Write-Host "Tablas a limpiar: $($tablesToClear.Count)" -ForegroundColor Yellow
Write-Host "Tabla preservada: usuarios" -ForegroundColor Green
Write-Host ""

# Confirmación
if (-not $Force) {
    Write-Host "[!] ADVERTENCIA: Esta operacion eliminara TODOS los datos excepto usuarios" -ForegroundColor Red
    Write-Host "[!] Esta accion NO SE PUEDE DESHACER" -ForegroundColor Red
    Write-Host ""
    $confirmation = Read-Host "Estas seguro de continuar? (escribe 'SI' para confirmar)"
    
    if ($confirmation -ne "SI") {
        Write-Host "Operacion cancelada." -ForegroundColor Yellow
        exit 0
    }
}

Write-Host ""
Write-Host "Iniciando limpieza..." -ForegroundColor Cyan
Write-Host ""

# Crear programa C# temporal
$projectRoot = Split-Path -Parent (Split-Path -Parent $PSScriptRoot)
$toolsPath = Join-Path $projectRoot "SGRRHH.Local.Infrastructure\bin\Debug\net9.0"

# Buscar la DLL compilada
$infrastructureDll = Get-ChildItem -Path $projectRoot -Filter "SGRRHH.Local.Infrastructure.dll" -Recurse -ErrorAction SilentlyContinue |
                     Where-Object { $_.FullName -notmatch '\\obj\\' -and $_.FullName -match '\\bin\\' } |
                     Select-Object -First 1

if (-not $infrastructureDll) {
    Write-Host "ERROR: No se encontro SGRRHH.Local.Infrastructure.dll compilada" -ForegroundColor Red
    Write-Host "Compila el proyecto primero con: dotnet build" -ForegroundColor Yellow
    exit 1
}

$dllPath = Split-Path -Parent $infrastructureDll.FullName

# Crear script C# para limpiar
$csharpCode = @"
using System;
using System.Data;
using Microsoft.Data.Sqlite;

class Program
{
    static void Main(string[] args)
    {
        var dbPath = args.Length > 0 ? args[0] : "C:\\SGRRHH\\Data\\sgrrhh.db";
        var connectionString = `$"Data Source={dbPath}";
        
        var tables = new[] {
            "detalles_actividad", "registros_diarios", "documentos_empleado",
            "proyectos_empleados", "nominas", "prestaciones", "permisos",
            "vacaciones", "contratos", "empleados", "proyectos", "actividades",
            "cargos", "departamentos", "tipos_permiso", "festivos_colombia",
            "configuracion_legal", "configuracion_sistema", "audit_logs"
        };
        
        try
        {
            using var connection = new SqliteConnection(connectionString);
            connection.Open();
            
            using var transaction = connection.BeginTransaction();
            
            // Deshabilitar foreign keys
            using (var cmd = connection.CreateCommand())
            {
                cmd.CommandText = "PRAGMA foreign_keys = OFF;";
                cmd.ExecuteNonQuery();
            }
            
            int totalDeleted = 0;
            
            foreach (var table in tables)
            {
                try
                {
                    // Contar registros
                    int count;
                    using (var cmd = connection.CreateCommand())
                    {
                        cmd.CommandText = `$"SELECT COUNT(*) FROM {table};";
                        count = Convert.ToInt32(cmd.ExecuteScalar());
                    }
                    
                    if (count == 0)
                    {
                        Console.WriteLine(`$"  [-] {table} (ya esta vacia)");
                    }
                    else
                    {
                        // Eliminar registros
                        using (var cmd = connection.CreateCommand())
                        {
                            cmd.CommandText = `$"DELETE FROM {table};";
                            int deleted = cmd.ExecuteNonQuery();
                            Console.WriteLine(`$"  [OK] {table} ({deleted} registros eliminados)");
                            totalDeleted += deleted;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(`$"  [ERROR] {table} ({ex.Message})");
                }
            }
            
            transaction.Commit();
            
            // Reactivar foreign keys
            using (var cmd = connection.CreateCommand())
            {
                cmd.CommandText = "PRAGMA foreign_keys = ON;";
                cmd.ExecuteNonQuery();
            }
            
            // Optimizar
            Console.WriteLine();
            Console.WriteLine("Optimizando base de datos...");
            using (var cmd = connection.CreateCommand())
            {
                cmd.CommandText = "VACUUM;";
                cmd.ExecuteNonQuery();
            }
            
            // Contar usuarios
            int userCount;
            using (var cmd = connection.CreateCommand())
            {
                cmd.CommandText = "SELECT COUNT(*) FROM usuarios WHERE activo = 1;";
                userCount = Convert.ToInt32(cmd.ExecuteScalar());
            }
            
            Console.WriteLine("[OK] Base de datos optimizada");
            Console.WriteLine();
            Console.WriteLine("=================================================================");
            Console.WriteLine("  RESUMEN DE LIMPIEZA");
            Console.WriteLine("=================================================================");
            Console.WriteLine();
            Console.WriteLine(`$"Total de registros eliminados: {totalDeleted}");
            Console.WriteLine(`$"Usuarios preservados: {userCount} usuarios activos");
            Console.WriteLine();
            Console.WriteLine("[OK] Limpieza completada exitosamente");
        }
        catch (Exception ex)
        {
            Console.WriteLine();
            Console.WriteLine(`$"ERROR: {ex.Message}");
            Environment.Exit(1);
        }
    }
}
"@

# Guardar código C#
$tempCsFile = [System.IO.Path]::GetTempFileName() + ".cs"
Set-Content -Path $tempCsFile -Value $csharpCode -Encoding UTF8

try {
    # Compilar y ejecutar
    $outputExe = [System.IO.Path]::GetTempFileName() + ".exe"
    
    Write-Host "Compilando herramienta de limpieza..." -ForegroundColor Gray
    
    # Buscar Microsoft.Data.Sqlite.dll
    $sqliteDll = Get-ChildItem -Path $dllPath -Filter "Microsoft.Data.Sqlite.dll" -Recurse -ErrorAction SilentlyContinue | Select-Object -First 1
    
    if ($sqliteDll) {
        $compileCmd = "dotnet build -c Release -o `"$([System.IO.Path]::GetDirectoryName($outputExe))`" -p:OutputType=Exe -p:TargetFramework=net9.0"
        
        # Compilar directamente con csc si está disponible
        $cscPath = Get-Command csc -ErrorAction SilentlyContinue
        
        if ($cscPath) {
            & csc /out:$outputExe /r:$($sqliteDll.FullName) $tempCsFile 2>&1 | Out-Null
            
            if ($LASTEXITCODE -eq 0) {
                Write-Host "Ejecutando limpieza..." -ForegroundColor Gray
                Write-Host ""
                & $outputExe $DatabasePath
                
                if ($LASTEXITCODE -ne 0) {
                    throw "Error al ejecutar la limpieza"
                }
            }
            else {
                throw "Error al compilar"
            }
        }
        else {
            throw "No se encontro el compilador de C#"
        }
    }
    else {
        throw "No se encontro Microsoft.Data.Sqlite.dll"
    }
}
catch {
    Write-Host ""
    Write-Host "ERROR: No se pudo completar la limpieza con el metodo automatico" -ForegroundColor Red
    Write-Host $_.Exception.Message -ForegroundColor Red
    Write-Host ""
    Write-Host "SOLUCION ALTERNATIVA: Ejecuta manualmente los comandos SQL:" -ForegroundColor Yellow
    Write-Host ""
    Write-Host "1. Abre la base de datos con un cliente SQLite" -ForegroundColor Gray
    Write-Host "2. Ejecuta: PRAGMA foreign_keys = OFF;" -ForegroundColor Gray
    Write-Host "3. Ejecuta DELETE FROM <tabla>; para cada tabla excepto usuarios" -ForegroundColor Gray
    Write-Host "4. Ejecuta: PRAGMA foreign_keys = ON;" -ForegroundColor Gray
    Write-Host "5. Ejecuta: VACUUM;" -ForegroundColor Gray
    exit 1
}
finally {
    # Limpiar archivos temporales
    if (Test-Path $tempCsFile) {
        Remove-Item $tempCsFile -Force -ErrorAction SilentlyContinue
    }
    if (Test-Path $outputExe) {
        Remove-Item $outputExe -Force -ErrorAction SilentlyContinue
    }
}
