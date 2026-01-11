-- =====================================================
-- MIGRACIÓN: Campos Silviculturales para Proyectos
-- Fecha: 2026-01-09
-- Descripción: Agrega campos específicos del sector forestal
--              a la tabla proyectos y crea tabla de especies
-- =====================================================

-- ===== PASO 1: Crear tabla de Especies Forestales =====

CREATE TABLE IF NOT EXISTS especies_forestales (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    codigo TEXT NOT NULL UNIQUE,
    nombre_comun TEXT NOT NULL,
    nombre_cientifico TEXT,
    familia TEXT,
    turno_promedio INTEGER,
    densidad_recomendada INTEGER,
    observaciones TEXT,
    activo INTEGER NOT NULL DEFAULT 1,
    fecha_creacion TEXT NOT NULL DEFAULT (datetime('now', 'localtime')),
    fecha_modificacion TEXT
);

-- Índices para especies forestales
CREATE INDEX IF NOT EXISTS idx_especies_forestales_codigo ON especies_forestales(codigo);
CREATE INDEX IF NOT EXISTS idx_especies_forestales_nombre_comun ON especies_forestales(nombre_comun);
CREATE INDEX IF NOT EXISTS idx_especies_forestales_activo ON especies_forestales(activo);


-- ===== PASO 2: Agregar columnas a tabla Proyectos =====

-- Columna de tipo de proyecto forestal
ALTER TABLE proyectos ADD COLUMN tipo_proyecto INTEGER DEFAULT 1;

-- Columnas geográficas y catastrales
ALTER TABLE proyectos ADD COLUMN predio TEXT;
ALTER TABLE proyectos ADD COLUMN lote TEXT;
ALTER TABLE proyectos ADD COLUMN departamento TEXT;
ALTER TABLE proyectos ADD COLUMN municipio TEXT;
ALTER TABLE proyectos ADD COLUMN vereda TEXT;
ALTER TABLE proyectos ADD COLUMN latitud REAL;
ALTER TABLE proyectos ADD COLUMN longitud REAL;
ALTER TABLE proyectos ADD COLUMN altitud_msnm INTEGER;

-- Columnas silviculturales
ALTER TABLE proyectos ADD COLUMN especie_id INTEGER REFERENCES especies_forestales(id);
ALTER TABLE proyectos ADD COLUMN area_hectareas REAL;
ALTER TABLE proyectos ADD COLUMN fecha_siembra TEXT;
ALTER TABLE proyectos ADD COLUMN densidad_inicial INTEGER;
ALTER TABLE proyectos ADD COLUMN densidad_actual INTEGER;
ALTER TABLE proyectos ADD COLUMN turno_cosecha_anios INTEGER;
ALTER TABLE proyectos ADD COLUMN tipo_tenencia INTEGER DEFAULT 1;
ALTER TABLE proyectos ADD COLUMN certificacion TEXT;

-- Columnas de métricas/seguimiento
ALTER TABLE proyectos ADD COLUMN total_horas_trabajadas REAL DEFAULT 0;
ALTER TABLE proyectos ADD COLUMN costo_mano_obra_acumulado REAL DEFAULT 0;
ALTER TABLE proyectos ADD COLUMN total_jornales INTEGER DEFAULT 0;
ALTER TABLE proyectos ADD COLUMN fecha_ultima_actualizacion_metricas TEXT;


-- ===== PASO 3: Crear índices para optimizar consultas =====

CREATE INDEX IF NOT EXISTS idx_proyectos_tipo_proyecto ON proyectos(tipo_proyecto);
CREATE INDEX IF NOT EXISTS idx_proyectos_departamento ON proyectos(departamento);
CREATE INDEX IF NOT EXISTS idx_proyectos_municipio ON proyectos(municipio);
CREATE INDEX IF NOT EXISTS idx_proyectos_especie_id ON proyectos(especie_id);
CREATE INDEX IF NOT EXISTS idx_proyectos_predio ON proyectos(predio);
CREATE INDEX IF NOT EXISTS idx_proyectos_fecha_siembra ON proyectos(fecha_siembra);


-- ===== PASO 4: Insertar datos semilla de especies forestales colombianas =====

INSERT OR IGNORE INTO especies_forestales (codigo, nombre_comun, nombre_cientifico, familia, turno_promedio, densidad_recomendada, activo, fecha_creacion) VALUES
('PIN-PAT', 'Pino Pátula', 'Pinus patula', 'Pinaceae', 18, 1111, 1, datetime('now', 'localtime')),
('PIN-CAR', 'Pino Caribe', 'Pinus caribaea', 'Pinaceae', 15, 1111, 1, datetime('now', 'localtime')),
('PIN-OOC', 'Pino Oocarpa', 'Pinus oocarpa', 'Pinaceae', 20, 1111, 1, datetime('now', 'localtime')),
('PIN-RAD', 'Pino Radiata', 'Pinus radiata', 'Pinaceae', 20, 1111, 1, datetime('now', 'localtime')),
('PIN-TEC', 'Pino Tecunumanii', 'Pinus tecunumanii', 'Pinaceae', 16, 1111, 1, datetime('now', 'localtime')),
('EUC-GRA', 'Eucalipto Grandis', 'Eucalyptus grandis', 'Myrtaceae', 7, 1111, 1, datetime('now', 'localtime')),
('EUC-GLO', 'Eucalipto Glóbulus', 'Eucalyptus globulus', 'Myrtaceae', 10, 1111, 1, datetime('now', 'localtime')),
('EUC-URO', 'Eucalipto Urograndis', 'Eucalyptus urograndis', 'Myrtaceae', 6, 1111, 1, datetime('now', 'localtime')),
('EUC-PEL', 'Eucalipto Pellita', 'Eucalyptus pellita', 'Myrtaceae', 8, 1111, 1, datetime('now', 'localtime')),
('EUC-TER', 'Eucalipto Tereticornis', 'Eucalyptus tereticornis', 'Myrtaceae', 8, 1111, 1, datetime('now', 'localtime')),
('TEC-GRA', 'Teca', 'Tectona grandis', 'Lamiaceae', 20, 816, 1, datetime('now', 'localtime')),
('ACA-MAN', 'Acacia Mangium', 'Acacia mangium', 'Fabaceae', 8, 1111, 1, datetime('now', 'localtime')),
('ACA-AUR', 'Acacia Auriculiformis', 'Acacia auriculiformis', 'Fabaceae', 8, 1111, 1, datetime('now', 'localtime')),
('MEL-AZE', 'Melina', 'Gmelina arborea', 'Lamiaceae', 12, 1111, 1, datetime('now', 'localtime')),
('CED-ODO', 'Cedro Rosado', 'Cedrela odorata', 'Meliaceae', 25, 625, 1, datetime('now', 'localtime')),
('CED-MON', 'Cedro de Montaña', 'Cedrela montana', 'Meliaceae', 25, 625, 1, datetime('now', 'localtime')),
('CIP-LUS', 'Ciprés', 'Cupressus lusitanica', 'Cupressaceae', 20, 1111, 1, datetime('now', 'localtime')),
('NOG-CAF', 'Nogal Cafetero', 'Cordia alliodora', 'Boraginaceae', 15, 400, 1, datetime('now', 'localtime')),
('CAU-CAU', 'Caucho', 'Hevea brasiliensis', 'Euphorbiaceae', 30, 500, 1, datetime('now', 'localtime')),
('BAL-SAM', 'Balso', 'Ochroma pyramidale', 'Malvaceae', 5, 1111, 1, datetime('now', 'localtime')),
('ROB-NEG', 'Roble Negro', 'Colombobalanus excelsa', 'Fagaceae', 40, 400, 1, datetime('now', 'localtime')),
('ALI-MAC', 'Aliso', 'Alnus acuminata', 'Betulaceae', 12, 816, 1, datetime('now', 'localtime')),
('MAC-INT', 'Macadamia', 'Macadamia integrifolia', 'Proteaceae', 15, 278, 1, datetime('now', 'localtime')),
('GUA-ANG', 'Guadua', 'Guadua angustifolia', 'Poaceae', 5, 625, 1, datetime('now', 'localtime'));


-- ===== PASO 5: Actualizar proyectos existentes (opcional) =====
-- Si los proyectos existentes tienen ubicación en el campo antiguo,
-- se puede hacer una migración de datos según sea necesario.

-- Ejemplo: Si se quiere establecer un tipo por defecto
-- UPDATE proyectos SET tipo_proyecto = 1 WHERE tipo_proyecto IS NULL;


-- ===== VERIFICACIÓN =====
-- Ejecutar estas consultas para verificar la migración:

-- SELECT COUNT(*) as total_especies FROM especies_forestales;
-- SELECT sql FROM sqlite_master WHERE type='table' AND name='proyectos';
-- SELECT sql FROM sqlite_master WHERE type='table' AND name='especies_forestales';
