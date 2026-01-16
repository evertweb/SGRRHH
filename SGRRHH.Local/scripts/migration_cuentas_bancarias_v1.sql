-- =============================================
-- MIGRACIÓN: Información Bancaria de Empleados
-- Fecha: 2026-01-12
-- Descripción: Crea tabla para gestionar múltiples cuentas bancarias por empleado
-- =============================================

-- Crear tabla de cuentas bancarias
CREATE TABLE IF NOT EXISTS cuentas_bancarias (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    empleado_id INTEGER NOT NULL,
    banco TEXT NOT NULL,
    tipo_cuenta INTEGER NOT NULL DEFAULT 1, -- 1=Ahorros, 2=Corriente
    numero_cuenta TEXT NOT NULL,
    nombre_titular TEXT,
    documento_titular TEXT,
    es_cuenta_nomina INTEGER NOT NULL DEFAULT 0, -- 0=No, 1=Sí
    esta_activa INTEGER NOT NULL DEFAULT 1, -- 0=Inactiva, 1=Activa
    fecha_apertura TEXT, -- Formato ISO: YYYY-MM-DD
    documento_certificacion_id INTEGER, -- FK a documentos_empleado
    observaciones TEXT,
    activo INTEGER NOT NULL DEFAULT 1,
    fecha_creacion TEXT NOT NULL,
    fecha_modificacion TEXT,
    FOREIGN KEY (empleado_id) REFERENCES empleados(id) ON DELETE CASCADE,
    FOREIGN KEY (documento_certificacion_id) REFERENCES documentos_empleado(id) ON DELETE SET NULL
);

-- Índices para optimizar consultas
CREATE INDEX IF NOT EXISTS idx_cuentas_bancarias_empleado 
    ON cuentas_bancarias(empleado_id);

CREATE INDEX IF NOT EXISTS idx_cuentas_bancarias_nomina 
    ON cuentas_bancarias(empleado_id, es_cuenta_nomina) 
    WHERE es_cuenta_nomina = 1 AND esta_activa = 1;

CREATE INDEX IF NOT EXISTS idx_cuentas_bancarias_activas 
    ON cuentas_bancarias(empleado_id, esta_activa) 
    WHERE esta_activa = 1;

-- =============================================
-- MIGRACIÓN DE DATOS EXISTENTES
-- =============================================
-- Migrar datos bancarios básicos de la tabla empleados a cuentas_bancarias
-- Solo para empleados que tengan banco y número de cuenta registrados

INSERT INTO cuentas_bancarias (
    empleado_id,
    banco,
    tipo_cuenta,
    numero_cuenta,
    nombre_titular,
    documento_titular,
    es_cuenta_nomina,
    esta_activa,
    fecha_apertura,
    observaciones,
    activo,
    fecha_creacion
)
SELECT 
    e.id,
    e.banco,
    1, -- Por defecto: Cuenta de Ahorros
    e.numero_cuenta,
    e.nombres || ' ' || e.apellidos, -- Titular = nombre del empleado
    e.cedula, -- Documento del titular = cédula del empleado
    1, -- Por defecto: Es cuenta de nómina
    1, -- Activa
    e.fecha_ingreso, -- Fecha apertura = fecha de ingreso
    'Migrada desde datos básicos del empleado', -- Observación
    1,
    COALESCE(e.fecha_creacion, datetime('now'))
FROM empleados e
WHERE e.banco IS NOT NULL 
  AND e.banco != '' 
  AND e.numero_cuenta IS NOT NULL 
  AND e.numero_cuenta != ''
  AND NOT EXISTS (
      SELECT 1 FROM cuentas_bancarias cb 
      WHERE cb.empleado_id = e.id 
        AND cb.numero_cuenta = e.numero_cuenta
  );

-- =============================================
-- ESTADÍSTICAS POST-MIGRACIÓN
-- =============================================
SELECT 
    'Cuentas bancarias migradas' AS descripcion,
    COUNT(*) AS cantidad
FROM cuentas_bancarias
WHERE observaciones LIKE '%Migrada desde datos básicos%';

SELECT 
    'Empleados con cuentas bancarias' AS descripcion,
    COUNT(DISTINCT empleado_id) AS cantidad
FROM cuentas_bancarias
WHERE activo = 1;

-- =============================================
-- NOTA IMPORTANTE
-- =============================================
-- Los campos 'banco' y 'numero_cuenta' en la tabla empleados
-- se mantienen por compatibilidad pero ya NO deben usarse.
-- Toda la gestión bancaria debe hacerse a través de la tabla cuentas_bancarias.
-- En el futuro se pueden marcar como @deprecated o eliminar.
