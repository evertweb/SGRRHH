-- =====================================================
-- MIGRACIÓN: Módulo de Incapacidades v1.0
-- Fecha: 2026-01-09
-- Descripción: Crea tablas para gestión de incapacidades
-- =====================================================

-- 1. Crear tabla incapacidades
CREATE TABLE IF NOT EXISTS incapacidades (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    
    -- Identificación
    numero_incapacidad TEXT NOT NULL UNIQUE,
    empleado_id INTEGER NOT NULL REFERENCES empleados(id),
    
    -- Origen (puede venir de un permiso o ser directa)
    permiso_origen_id INTEGER REFERENCES permisos(id),
    incapacidad_anterior_id INTEGER REFERENCES incapacidades(id), -- Para prórrogas
    es_prorroga INTEGER DEFAULT 0,
    
    -- Fechas
    fecha_inicio TEXT NOT NULL,
    fecha_fin TEXT NOT NULL,
    total_dias INTEGER NOT NULL,
    fecha_expedicion TEXT NOT NULL, -- Fecha que aparece en el documento
    
    -- Diagnóstico
    diagnostico_cie10 TEXT, -- Código CIE-10
    diagnostico_descripcion TEXT NOT NULL,
    
    -- Tipo y Entidad
    tipo_incapacidad INTEGER NOT NULL DEFAULT 1,
    -- 1=Enfermedad General, 2=Accidente Trabajo, 3=Enfermedad Laboral, 
    -- 4=Licencia Maternidad, 5=Licencia Paternidad
    
    entidad_emisora TEXT NOT NULL, -- Nombre del médico/institución
    entidad_pagadora TEXT, -- EPS, ARL, o nombre específico
    
    -- Cálculo de días y pagos
    dias_empresa INTEGER NOT NULL DEFAULT 0, -- Días que paga empresa (1-2 normalmente)
    dias_eps_arl INTEGER NOT NULL DEFAULT 0, -- Días que paga EPS/ARL
    porcentaje_pago REAL DEFAULT 66.67, -- Porcentaje que paga entidad
    valor_dia_base REAL, -- Salario diario para cálculo
    valor_total_cobrar REAL, -- Total a cobrar a EPS/ARL
    
    -- Estado de gestión
    estado INTEGER NOT NULL DEFAULT 1,
    -- 1=Activa, 2=Finalizada, 3=Transcrita, 4=Cobrada, 5=Cancelada
    
    transcrita INTEGER DEFAULT 0, -- ¿Ya se transcribió ante EPS?
    fecha_transcripcion TEXT,
    numero_radicado_eps TEXT, -- Número de radicado
    
    cobrada INTEGER DEFAULT 0, -- ¿Ya se cobró a EPS/ARL?
    fecha_cobro TEXT,
    valor_cobrado REAL,
    
    -- Documentos
    documento_incapacidad_path TEXT, -- Scan del documento
    documento_transcripcion_path TEXT,
    
    -- Observaciones
    observaciones TEXT,
    
    -- Auditoría
    registrado_por_id INTEGER NOT NULL REFERENCES usuarios(id),
    activo INTEGER DEFAULT 1,
    fecha_creacion TEXT DEFAULT CURRENT_TIMESTAMP,
    fecha_modificacion TEXT
);

-- 2. Crear índices para incapacidades
CREATE INDEX IF NOT EXISTS idx_incapacidad_empleado ON incapacidades(empleado_id);
CREATE INDEX IF NOT EXISTS idx_incapacidad_fechas ON incapacidades(fecha_inicio, fecha_fin);
CREATE INDEX IF NOT EXISTS idx_incapacidad_estado ON incapacidades(estado);
CREATE INDEX IF NOT EXISTS idx_incapacidad_tipo ON incapacidades(tipo_incapacidad);
CREATE INDEX IF NOT EXISTS idx_incapacidad_permiso ON incapacidades(permiso_origen_id);
CREATE INDEX IF NOT EXISTS idx_incapacidad_anterior ON incapacidades(incapacidad_anterior_id);
CREATE INDEX IF NOT EXISTS idx_incapacidad_transcripcion ON incapacidades(transcrita, fecha_inicio);

-- 3. Crear tabla seguimiento_incapacidades
CREATE TABLE IF NOT EXISTS seguimiento_incapacidades (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    incapacidad_id INTEGER NOT NULL REFERENCES incapacidades(id),
    fecha_accion TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,
    tipo_accion INTEGER NOT NULL,
    -- 1=Registro, 2=Transcripcion, 3=RadicadoEPS, 4=Cobro, 5=Prorroga, 
    -- 6=Finalizacion, 7=Observacion, 8=DocumentoAgregado, 9=ConversionDesdePermiso
    descripcion TEXT,
    realizado_por_id INTEGER NOT NULL REFERENCES usuarios(id),
    datos_adicionales TEXT, -- JSON
    activo INTEGER DEFAULT 1,
    fecha_creacion TEXT DEFAULT CURRENT_TIMESTAMP
);

CREATE INDEX IF NOT EXISTS idx_seg_incapacidad ON seguimiento_incapacidades(incapacidad_id);

-- 4. Modificar tabla permisos (agregar campos para vincular con incapacidad)
-- Nota: SQLite no permite verificar si columna existe, usamos CREATE TABLE AS workaround
-- Simplemente ejecutamos ALTER TABLE y si la columna ya existe, fallará silenciosamente

-- Intentar agregar columna incapacidad_id
ALTER TABLE permisos ADD COLUMN incapacidad_id INTEGER REFERENCES incapacidades(id);

-- Intentar agregar columna convertido_a_incapacidad
ALTER TABLE permisos ADD COLUMN convertido_a_incapacidad INTEGER DEFAULT 0;

-- 5. Verificar creación
SELECT 'Tabla incapacidades creada: ' || COUNT(*) as verificacion FROM sqlite_master WHERE type='table' AND name='incapacidades';
SELECT 'Tabla seguimiento_incapacidades creada: ' || COUNT(*) as verificacion FROM sqlite_master WHERE type='table' AND name='seguimiento_incapacidades';
