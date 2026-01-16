# Script para corregir foreign key en cuentas_bancarias remotamente
$dbPath = "C:\SGRRHH\Data\sgrrhh.db"

if (-not (Test-Path $dbPath)) {
    Write-Host "ERROR: Base de datos no encontrada en $dbPath" -ForegroundColor Red
    exit 1
}

Write-Host "Corrigiendo foreign key en cuentas_bancarias..." -ForegroundColor Cyan

# SQL de corrección
$sql = @"
PRAGMA foreign_keys = OFF;

-- Crear tabla temporal con estructura correcta
CREATE TABLE IF NOT EXISTS cuentas_bancarias_temp (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    empleado_id INTEGER NOT NULL,
    banco TEXT NOT NULL,
    tipo_cuenta INTEGER NOT NULL DEFAULT 1,
    numero_cuenta TEXT NOT NULL,
    nombre_titular TEXT,
    documento_titular TEXT,
    es_cuenta_nomina INTEGER NOT NULL DEFAULT 0,
    esta_activa INTEGER NOT NULL DEFAULT 1,
    fecha_apertura TEXT,
    documento_certificacion_id INTEGER,
    observaciones TEXT,
    activo INTEGER NOT NULL DEFAULT 1,
    fecha_creacion TEXT NOT NULL,
    fecha_modificacion TEXT,
    FOREIGN KEY (empleado_id) REFERENCES empleados(id) ON DELETE CASCADE,
    FOREIGN KEY (documento_certificacion_id) REFERENCES documentos_empleado(id) ON DELETE SET NULL
);

-- Copiar datos
INSERT INTO cuentas_bancarias_temp SELECT * FROM cuentas_bancarias;

-- Eliminar tabla original
DROP TABLE cuentas_bancarias;

-- Renombrar temporal
ALTER TABLE cuentas_bancarias_temp RENAME TO cuentas_bancarias;

-- Recrear índices
CREATE INDEX IF NOT EXISTS idx_cuentas_bancarias_empleado ON cuentas_bancarias(empleado_id);
CREATE INDEX IF NOT EXISTS idx_cuentas_bancarias_nomina ON cuentas_bancarias(empleado_id, es_cuenta_nomina) WHERE es_cuenta_nomina = 1 AND esta_activa = 1;
CREATE INDEX IF NOT EXISTS idx_cuentas_bancarias_activas ON cuentas_bancarias(empleado_id, esta_activa) WHERE esta_activa = 1;

PRAGMA foreign_keys = ON;

SELECT 'Correccion completada' AS resultado;
"@

# Ejecutar SQL usando sqlite3
$sql | sqlite3 $dbPath

if ($LASTEXITCODE -eq 0) {
    Write-Host "`n[OK] Foreign key corregida exitosamente" -ForegroundColor Green
} else {
    Write-Host "`n[ERROR] Error al ejecutar correccion" -ForegroundColor Red
    exit 1
}
