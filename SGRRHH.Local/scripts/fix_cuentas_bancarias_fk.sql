-- =============================================
-- CORRECCIÓN: Foreign Key incorrecta en cuentas_bancarias
-- Fecha: 2026-01-XX
-- Problema: La foreign key referencia 'documentos_empleados' (plural) 
--           pero la tabla correcta es 'documentos_empleado' (singular)
-- Solución: Recrear la tabla con la foreign key correcta
-- =============================================

-- Verificar si la tabla existe
SELECT 'Verificando estructura actual...' AS info;

-- Verificar foreign keys actuales
SELECT sql FROM sqlite_master 
WHERE type = 'table' AND name = 'cuentas_bancarias';

-- Paso 1: Crear tabla temporal con la estructura correcta
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

-- Paso 2: Copiar todos los datos de la tabla original a la temporal
-- Solo si la tabla existe y tiene datos
INSERT INTO cuentas_bancarias_temp 
SELECT * FROM cuentas_bancarias;

-- Paso 3: Verificar que los datos se copiaron correctamente
SELECT 
    'Registros en tabla original' AS descripcion,
    COUNT(*) AS cantidad
FROM cuentas_bancarias;

SELECT 
    'Registros en tabla temporal' AS descripcion,
    COUNT(*) AS cantidad
FROM cuentas_bancarias_temp;

-- Paso 4: Eliminar la tabla original
DROP TABLE cuentas_bancarias;

-- Paso 5: Renombrar la tabla temporal a la original
ALTER TABLE cuentas_bancarias_temp RENAME TO cuentas_bancarias;

-- Paso 6: Recrear los índices
CREATE INDEX IF NOT EXISTS idx_cuentas_bancarias_empleado 
    ON cuentas_bancarias(empleado_id);

CREATE INDEX IF NOT EXISTS idx_cuentas_bancarias_nomina 
    ON cuentas_bancarias(empleado_id, es_cuenta_nomina) 
    WHERE es_cuenta_nomina = 1 AND esta_activa = 1;

CREATE INDEX IF NOT EXISTS idx_cuentas_bancarias_activas 
    ON cuentas_bancarias(empleado_id, esta_activa) 
    WHERE esta_activa = 1;

-- Paso 7: Verificar la estructura final
SELECT 'Estructura corregida:' AS info;
SELECT sql FROM sqlite_master 
WHERE type = 'table' AND name = 'cuentas_bancarias';

-- Verificación final
SELECT 
    'Corrección completada. Registros en tabla corregida:' AS descripcion,
    COUNT(*) AS cantidad
FROM cuentas_bancarias;

-- =============================================
-- NOTA IMPORTANTE
-- =============================================
-- Este script corrige la foreign key incorrecta.
-- La tabla ahora referencia correctamente 'documentos_empleado' (singular)
-- en lugar de 'documentos_empleados' (plural).
-- =============================================
