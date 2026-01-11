-- ============================================================================
-- MIGRACIÓN: Sistema de Seguimiento de Permisos v1
-- Fecha: 2026-01-09
-- Descripción: Agrega campos de seguimiento a permisos, crea tablas de 
--              seguimiento y compensaciones de horas
-- ============================================================================

-- 1. AGREGAR CAMPOS A TABLA PERMISOS
-- ==============================================================================

-- Tipo de resolución: 0=Pendiente, 1=Remunerado, 2=Descontado, 3=Compensado
ALTER TABLE permisos ADD COLUMN tipo_resolucion INTEGER DEFAULT 0;

-- Indica si se aprobó con documento pendiente de entrega
ALTER TABLE permisos ADD COLUMN requiere_documento_posterior INTEGER DEFAULT 0;

-- Fecha límite para entregar documento
ALTER TABLE permisos ADD COLUMN fecha_limite_documento TEXT;

-- Fecha en que se entregó el documento
ALTER TABLE permisos ADD COLUMN fecha_entrega_documento TEXT;

-- Horas totales a compensar (si aplica)
ALTER TABLE permisos ADD COLUMN horas_compensar INTEGER;

-- Horas ya compensadas
ALTER TABLE permisos ADD COLUMN horas_compensadas INTEGER DEFAULT 0;

-- Fecha límite para completar compensación
ALTER TABLE permisos ADD COLUMN fecha_limite_compensacion TEXT;

-- Monto a descontar en nómina (si aplica)
ALTER TABLE permisos ADD COLUMN monto_descuento REAL;

-- Período de nómina en que se descontará (ej: "2026-01")
ALTER TABLE permisos ADD COLUMN periodo_descuento TEXT;

-- Indica si el permiso está completamente cerrado
ALTER TABLE permisos ADD COLUMN completado INTEGER DEFAULT 0;


-- 2. CREAR TABLA DE SEGUIMIENTO DE PERMISOS
-- ==============================================================================
-- tipo_accion: 1=Solicitud, 2=Aprobacion, 3=Rechazo, 4=DocumentoEntregado,
--              5=CompensacionIniciada, 6=HorasCompensadas, 7=Completado, 
--              8=Observacion, 9=DocumentoRechazado, 10=CompensacionVencida

CREATE TABLE IF NOT EXISTS seguimiento_permisos (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    permiso_id INTEGER NOT NULL REFERENCES permisos(id),
    fecha_accion TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,
    tipo_accion INTEGER NOT NULL,
    descripcion TEXT,
    realizado_por_id INTEGER NOT NULL REFERENCES usuarios(id),
    datos_adicionales TEXT,
    activo INTEGER DEFAULT 1,
    fecha_creacion TEXT DEFAULT CURRENT_TIMESTAMP,
    fecha_modificacion TEXT
);

-- Índices para optimización de consultas
CREATE INDEX IF NOT EXISTS idx_seguimiento_permiso ON seguimiento_permisos(permiso_id);
CREATE INDEX IF NOT EXISTS idx_seguimiento_fecha ON seguimiento_permisos(fecha_accion);
CREATE INDEX IF NOT EXISTS idx_seguimiento_tipo ON seguimiento_permisos(tipo_accion);


-- 3. CREAR TABLA DE COMPENSACIONES DE HORAS
-- ==============================================================================

CREATE TABLE IF NOT EXISTS compensaciones_horas (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    permiso_id INTEGER NOT NULL REFERENCES permisos(id),
    fecha_compensacion TEXT NOT NULL,
    horas_compensadas INTEGER NOT NULL,
    descripcion TEXT,
    aprobado_por_id INTEGER REFERENCES usuarios(id),
    fecha_aprobacion TEXT,
    activo INTEGER DEFAULT 1,
    fecha_creacion TEXT DEFAULT CURRENT_TIMESTAMP,
    fecha_modificacion TEXT
);

-- Índice para consultas por permiso
CREATE INDEX IF NOT EXISTS idx_compensacion_permiso ON compensaciones_horas(permiso_id);
CREATE INDEX IF NOT EXISTS idx_compensacion_fecha ON compensaciones_horas(fecha_compensacion);


-- 4. ÍNDICES ADICIONALES PARA CONSULTAS DE SEGUIMIENTO
-- ==============================================================================

-- Índice para permisos pendientes de documento
CREATE INDEX IF NOT EXISTS idx_permisos_pendiente_doc 
ON permisos(requiere_documento_posterior, fecha_limite_documento) 
WHERE requiere_documento_posterior = 1;

-- Índice para permisos en compensación
CREATE INDEX IF NOT EXISTS idx_permisos_compensacion 
ON permisos(tipo_resolucion, fecha_limite_compensacion) 
WHERE tipo_resolucion = 3;

-- Índice para permisos para descuento
CREATE INDEX IF NOT EXISTS idx_permisos_descuento 
ON permisos(tipo_resolucion, periodo_descuento) 
WHERE tipo_resolucion = 2;


-- 5. ACTUALIZAR PERMISOS EXISTENTES (MIGRACIÓN DE DATOS)
-- ==============================================================================

-- Los permisos existentes que están aprobados se marcan como completados
UPDATE permisos 
SET completado = 1, 
    tipo_resolucion = 1  -- Remunerado por defecto
WHERE estado = 2  -- Aprobado
  AND tipo_resolucion IS NULL;

-- Los permisos pendientes mantienen tipo_resolucion = 0 (PendienteDefinir)
UPDATE permisos 
SET tipo_resolucion = 0
WHERE estado = 1  -- Pendiente
  AND tipo_resolucion IS NULL;

-- Los permisos rechazados o cancelados se marcan como completados
UPDATE permisos 
SET completado = 1, 
    tipo_resolucion = 0
WHERE estado IN (3, 4)  -- Rechazado o Cancelado
  AND tipo_resolucion IS NULL;


-- ============================================================================
-- FIN DE MIGRACIÓN
-- ============================================================================
