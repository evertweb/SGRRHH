-- =====================================================
-- MIGRACIÓN: Estandarización de Nomenclatura a snake_case
-- Versión: 1.0
-- Fecha: 2026-01-11
-- Descripción: Migra tablas PascalCase a snake_case
-- IMPORTANTE: Ejecutar después de hacer backup de BD
-- =====================================================

-- Desactivar FK temporalmente
PRAGMA foreign_keys = OFF;

-- =====================================================
-- 1. MIGRAR CategoriasActividades → activity_categories
-- =====================================================

-- Crear nueva tabla con snake_case
CREATE TABLE IF NOT EXISTS activity_categories (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    code TEXT NOT NULL UNIQUE,
    name TEXT NOT NULL,
    description TEXT,
    icon TEXT,
    color_hex TEXT,
    display_order INTEGER DEFAULT 0,
    is_active INTEGER DEFAULT 1,
    created_at TEXT DEFAULT (datetime('now', 'localtime')),
    updated_at TEXT
);

-- Migrar datos si existe tabla vieja
INSERT OR IGNORE INTO activity_categories (id, code, name, description, icon, color_hex, display_order, is_active, created_at, updated_at)
SELECT Id, Codigo, Nombre, Descripcion, Icono, ColorHex, Orden, Activo, FechaCreacion, FechaModificacion
FROM CategoriasActividades WHERE EXISTS (SELECT 1 FROM sqlite_master WHERE type='table' AND name='CategoriasActividades');

-- Crear índices
CREATE INDEX IF NOT EXISTS idx_activity_categories_code ON activity_categories(code);
CREATE INDEX IF NOT EXISTS idx_activity_categories_is_active ON activity_categories(is_active);

-- =====================================================
-- 2. MIGRAR Actividades - Agregar columnas snake_case
-- =====================================================

-- Agregar nuevas columnas snake_case (SQLite no permite renombrar columnas)
-- Las queries del repositorio usarán COALESCE para leer de ambas

-- category_id como nueva columna
ALTER TABLE actividades ADD COLUMN category_id INTEGER REFERENCES activity_categories(id);

-- Copiar datos de CategoriaId a category_id si existe
UPDATE actividades SET category_id = CategoriaId WHERE CategoriaId IS NOT NULL;

-- Agregar otras columnas snake_case
ALTER TABLE actividades ADD COLUMN category_text TEXT;
ALTER TABLE actividades ADD COLUMN unit_of_measure INTEGER DEFAULT 0;
ALTER TABLE actividades ADD COLUMN unit_abbreviation TEXT;
ALTER TABLE actividades ADD COLUMN expected_yield REAL;
ALTER TABLE actividades ADD COLUMN minimum_yield REAL;
ALTER TABLE actividades ADD COLUMN unit_cost REAL;
ALTER TABLE actividades ADD COLUMN requires_quantity INTEGER DEFAULT 0;
ALTER TABLE actividades ADD COLUMN applicable_project_types TEXT;
ALTER TABLE actividades ADD COLUMN applicable_species TEXT;
ALTER TABLE actividades ADD COLUMN is_featured INTEGER DEFAULT 0;

-- Copiar datos de columnas PascalCase a snake_case
UPDATE actividades SET 
    category_text = COALESCE(CategoriaTexto, categoria_texto),
    unit_of_measure = COALESCE(UnidadMedida, unidad_medida, 0),
    unit_abbreviation = COALESCE(UnidadAbreviatura, unidad_abreviatura),
    expected_yield = COALESCE(RendimientoEsperado, rendimiento_esperado),
    minimum_yield = COALESCE(RendimientoMinimo, rendimiento_minimo),
    unit_cost = COALESCE(CostoUnitario, costo_unitario),
    requires_quantity = COALESCE(RequiereCantidad, requiere_cantidad, 0),
    applicable_project_types = COALESCE(TiposProyectoAplicables, tipos_proyecto_aplicables),
    applicable_species = COALESCE(EspeciesAplicables, especies_aplicables),
    is_featured = COALESCE(EsDestacada, es_destacada, 0)
WHERE 1=1;

-- Crear índices para nuevas columnas
CREATE INDEX IF NOT EXISTS idx_actividades_category_id ON actividades(category_id);

-- =====================================================
-- 3. MIGRAR ScanProfiles → scan_profiles
-- =====================================================

CREATE TABLE IF NOT EXISTS scan_profiles (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    name TEXT NOT NULL,
    description TEXT,
    is_default INTEGER NOT NULL DEFAULT 0,
    dpi INTEGER NOT NULL DEFAULT 200,
    color_mode TEXT NOT NULL DEFAULT 'Color',
    source TEXT NOT NULL DEFAULT 'Flatbed',
    page_size TEXT NOT NULL DEFAULT 'Letter',
    brightness INTEGER DEFAULT 0,
    contrast INTEGER DEFAULT 0,
    gamma REAL DEFAULT 1.0,
    sharpness INTEGER DEFAULT 0,
    black_white_threshold INTEGER DEFAULT 128,
    auto_deskew INTEGER DEFAULT 0,
    auto_crop INTEGER DEFAULT 0,
    created_at TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,
    last_used_at TEXT
);

-- Migrar datos de tabla vieja
INSERT OR IGNORE INTO scan_profiles (id, name, description, is_default, dpi, color_mode, source, page_size, brightness, contrast, gamma, sharpness, black_white_threshold, auto_deskew, auto_crop, created_at, last_used_at)
SELECT Id, Name, Description, IsDefault, Dpi, ColorMode, Source, PageSize, Brightness, Contrast, Gamma, Sharpness, BlackWhiteThreshold, AutoDeskew, AutoCrop, CreatedAt, LastUsedAt
FROM ScanProfiles WHERE EXISTS (SELECT 1 FROM sqlite_master WHERE type='table' AND name='ScanProfiles');

-- Crear índices
CREATE INDEX IF NOT EXISTS idx_scan_profiles_name ON scan_profiles(name);
CREATE INDEX IF NOT EXISTS idx_scan_profiles_default ON scan_profiles(is_default);

-- =====================================================
-- 4. INSERTAR DATOS SEED SI NO EXISTEN
-- =====================================================

-- Categorías de actividades
INSERT OR IGNORE INTO activity_categories (code, name, description, color_hex, display_order, is_active) VALUES
('PREP', 'Preparación de Terreno', 'Actividades previas a la siembra: limpieza, trazado, ahoyado', '#8B4513', 1, 1),
('SIEM', 'Siembra y Establecimiento', 'Plantación de árboles y establecimiento inicial', '#228B22', 2, 1),
('MANT', 'Mantenimiento', 'Limpias, plateos, control de malezas, fertilización', '#32CD32', 3, 1),
('PODA', 'Podas', 'Podas de formación, sanitarias, levante', '#006400', 4, 1),
('RALE', 'Raleos', 'Entresaca y raleo comercial', '#556B2F', 5, 1),
('FITO', 'Control Fitosanitario', 'Control de plagas, enfermedades, aplicación de agroquímicos', '#FF6347', 6, 1),
('COSE', 'Cosecha', 'Apeo, desrame, troceo, extracción, cargue', '#A0522D', 7, 1),
('VIVE', 'Vivero', 'Producción de plántulas: llenado, siembra, riego, mantenimiento', '#90EE90', 8, 1),
('INVE', 'Inventarios', 'Inventarios forestales, mediciones, parcelas', '#4682B4', 9, 1),
('INFR', 'Infraestructura', 'Vías, cercas, campamentos, bodegas', '#708090', 10, 1),
('INCE', 'Control de Incendios', 'Cortafuegos, quemas controladas, vigilancia', '#FF4500', 11, 1),
('ADMI', 'Administrativa', 'Supervisión, reuniones, capacitación, transporte', '#4169E1', 12, 1),
('OTRA', 'Otras Actividades', 'Actividades no clasificadas', '#808080', 99, 1);

-- Perfiles de escaneo
INSERT OR IGNORE INTO scan_profiles (id, name, description, is_default, dpi, color_mode, source, page_size) VALUES 
(1, 'Documento Rápido', 'Escaneo rápido de documentos en escala de grises', 0, 150, 'Grayscale', 'Flatbed', 'Letter'),
(2, 'Documento Alta Calidad', 'Escaneo de alta calidad para archivo permanente', 1, 300, 'Color', 'Flatbed', 'Letter'),
(3, 'Cédula/ID', 'Optimizado para documentos de identidad', 0, 300, 'Color', 'Flatbed', 'A5'),
(4, 'Foto', 'Escaneo de fotografías en alta resolución', 0, 600, 'Color', 'Flatbed', 'Letter'),
(5, 'Blanco y Negro', 'Documentos de texto para OCR', 0, 300, 'BlackWhite', 'Flatbed', 'Letter');

-- Reactivar FK
PRAGMA foreign_keys = ON;

-- =====================================================
-- VERIFICACIÓN
-- =====================================================
SELECT 'activity_categories: ' || COUNT(*) FROM activity_categories;
SELECT 'actividades: ' || COUNT(*) FROM actividades;
SELECT 'scan_profiles: ' || COUNT(*) FROM scan_profiles;
