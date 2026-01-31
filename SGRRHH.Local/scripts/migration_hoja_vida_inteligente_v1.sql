-- =====================================================
-- MIGRACIÓN: Módulo Hoja de Vida Inteligente v1.0
-- Fecha: 2026-01-29
-- Descripción: Crea tablas para gestión de vacantes, aspirantes y hojas de vida PDF
-- =====================================================

-- =====================================================
-- 1. TABLA: vacantes
-- =====================================================
CREATE TABLE IF NOT EXISTS vacantes (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    cargo_id INTEGER NOT NULL,
    departamento_id INTEGER NOT NULL,
    titulo TEXT NOT NULL,
    descripcion TEXT,
    requisitos TEXT,
    salario_minimo REAL,
    salario_maximo REAL,
    fecha_publicacion TEXT NOT NULL,
    fecha_cierre TEXT,
    estado TEXT NOT NULL DEFAULT 'Borrador',
    cantidad_posiciones INTEGER DEFAULT 1,
    es_activo INTEGER NOT NULL DEFAULT 1,
    fecha_creacion TEXT NOT NULL DEFAULT (datetime('now')),
    fecha_modificacion TEXT,
    FOREIGN KEY (cargo_id) REFERENCES cargos(id),
    FOREIGN KEY (departamento_id) REFERENCES departamentos(id)
);

CREATE INDEX IF NOT EXISTS idx_vacantes_estado ON vacantes(estado);
CREATE INDEX IF NOT EXISTS idx_vacantes_cargo ON vacantes(cargo_id);
CREATE INDEX IF NOT EXISTS idx_vacantes_departamento ON vacantes(departamento_id);
CREATE INDEX IF NOT EXISTS idx_vacantes_fecha_publicacion ON vacantes(fecha_publicacion);

-- =====================================================
-- 2. TABLA: aspirantes
-- =====================================================
CREATE TABLE IF NOT EXISTS aspirantes (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    vacante_id INTEGER,
    cedula TEXT NOT NULL UNIQUE,
    nombres TEXT NOT NULL,
    apellidos TEXT NOT NULL,
    fecha_nacimiento TEXT NOT NULL,
    genero TEXT NOT NULL,
    estado_civil TEXT NOT NULL,
    direccion TEXT NOT NULL,
    ciudad TEXT NOT NULL,
    departamento TEXT NOT NULL,
    telefono TEXT NOT NULL,
    email TEXT,
    nivel_educacion TEXT NOT NULL,
    titulo_obtenido TEXT,
    institucion_educativa TEXT,
    tallas_casco TEXT,
    tallas_botas TEXT,
    estado TEXT NOT NULL DEFAULT 'Registrado',
    fecha_registro TEXT NOT NULL DEFAULT (datetime('now')),
    fecha_modificacion TEXT,
    notas TEXT,
    puntaje_evaluacion INTEGER,
    es_activo INTEGER NOT NULL DEFAULT 1,
    FOREIGN KEY (vacante_id) REFERENCES vacantes(id)
);

CREATE INDEX IF NOT EXISTS idx_aspirantes_cedula ON aspirantes(cedula);
CREATE INDEX IF NOT EXISTS idx_aspirantes_estado ON aspirantes(estado);
CREATE INDEX IF NOT EXISTS idx_aspirantes_vacante ON aspirantes(vacante_id);
CREATE INDEX IF NOT EXISTS idx_aspirantes_nombres ON aspirantes(nombres, apellidos);

-- =====================================================
-- 3. TABLA: formacion_aspirante
-- =====================================================
CREATE TABLE IF NOT EXISTS formacion_aspirante (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    aspirante_id INTEGER NOT NULL,
    nivel TEXT NOT NULL,
    titulo TEXT NOT NULL,
    institucion TEXT NOT NULL,
    fecha_inicio TEXT NOT NULL,
    fecha_fin TEXT,
    en_curso INTEGER DEFAULT 0,
    es_activo INTEGER NOT NULL DEFAULT 1,
    fecha_creacion TEXT NOT NULL DEFAULT (datetime('now')),
    fecha_modificacion TEXT,
    FOREIGN KEY (aspirante_id) REFERENCES aspirantes(id) ON DELETE CASCADE
);

CREATE INDEX IF NOT EXISTS idx_formacion_aspirante ON formacion_aspirante(aspirante_id);

-- =====================================================
-- 4. TABLA: experiencia_aspirante
-- =====================================================
CREATE TABLE IF NOT EXISTS experiencia_aspirante (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    aspirante_id INTEGER NOT NULL,
    empresa TEXT NOT NULL,
    cargo TEXT NOT NULL,
    fecha_inicio TEXT NOT NULL,
    fecha_fin TEXT,
    trabajo_actual INTEGER DEFAULT 0,
    funciones TEXT,
    motivo_retiro TEXT,
    es_activo INTEGER NOT NULL DEFAULT 1,
    fecha_creacion TEXT NOT NULL DEFAULT (datetime('now')),
    fecha_modificacion TEXT,
    FOREIGN KEY (aspirante_id) REFERENCES aspirantes(id) ON DELETE CASCADE
);

CREATE INDEX IF NOT EXISTS idx_experiencia_aspirante ON experiencia_aspirante(aspirante_id);

-- =====================================================
-- 5. TABLA: referencias_aspirante
-- =====================================================
CREATE TABLE IF NOT EXISTS referencias_aspirante (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    aspirante_id INTEGER NOT NULL,
    tipo TEXT NOT NULL,
    nombre_completo TEXT NOT NULL,
    telefono TEXT NOT NULL,
    relacion TEXT NOT NULL,
    empresa TEXT,
    cargo TEXT,
    es_activo INTEGER NOT NULL DEFAULT 1,
    fecha_creacion TEXT NOT NULL DEFAULT (datetime('now')),
    fecha_modificacion TEXT,
    FOREIGN KEY (aspirante_id) REFERENCES aspirantes(id) ON DELETE CASCADE
);

CREATE INDEX IF NOT EXISTS idx_referencias_aspirante ON referencias_aspirante(aspirante_id);
CREATE INDEX IF NOT EXISTS idx_referencias_tipo ON referencias_aspirante(tipo);

-- =====================================================
-- 6. TABLA: hoja_vida_pdf (Metadatos)
-- =====================================================
CREATE TABLE IF NOT EXISTS hoja_vida_pdf (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    aspirante_id INTEGER,
    empleado_id INTEGER,
    documento_empleado_id INTEGER,
    version INTEGER NOT NULL DEFAULT 1,
    hash_contenido TEXT NOT NULL,
    origen TEXT NOT NULL,
    fecha_generacion TEXT,
    fecha_subida TEXT NOT NULL DEFAULT (datetime('now')),
    datos_extraidos TEXT,
    tiene_firma INTEGER DEFAULT 0,
    es_valido INTEGER DEFAULT 1,
    errores_validacion TEXT,
    es_activo INTEGER NOT NULL DEFAULT 1,
    fecha_creacion TEXT NOT NULL DEFAULT (datetime('now')),
    fecha_modificacion TEXT,
    FOREIGN KEY (aspirante_id) REFERENCES aspirantes(id),
    FOREIGN KEY (empleado_id) REFERENCES empleados(id),
    FOREIGN KEY (documento_empleado_id) REFERENCES documentos_empleado(id)
);

CREATE INDEX IF NOT EXISTS idx_hoja_vida_aspirante ON hoja_vida_pdf(aspirante_id);
CREATE INDEX IF NOT EXISTS idx_hoja_vida_empleado ON hoja_vida_pdf(empleado_id);
CREATE INDEX IF NOT EXISTS idx_hoja_vida_origen ON hoja_vida_pdf(origen);
CREATE INDEX IF NOT EXISTS idx_hoja_vida_version ON hoja_vida_pdf(aspirante_id, version);

-- =====================================================
-- 7. TABLA VIRTUAL FTS5: hojas_vida_fts (Búsqueda Full-Text)
-- =====================================================
CREATE VIRTUAL TABLE IF NOT EXISTS hojas_vida_fts USING fts5(
    aspirante_id,
    empleado_id,
    nombres,
    apellidos,
    cedula,
    formacion,
    experiencia,
    habilidades,
    content='hoja_vida_pdf',
    content_rowid='id'
);

-- =====================================================
-- 8. VERIFICACIÓN DE CREACIÓN
-- =====================================================
SELECT 'Tabla vacantes creada: ' || COUNT(*) as verificacion FROM sqlite_master WHERE type='table' AND name='vacantes';
SELECT 'Tabla aspirantes creada: ' || COUNT(*) as verificacion FROM sqlite_master WHERE type='table' AND name='aspirantes';
SELECT 'Tabla formacion_aspirante creada: ' || COUNT(*) as verificacion FROM sqlite_master WHERE type='table' AND name='formacion_aspirante';
SELECT 'Tabla experiencia_aspirante creada: ' || COUNT(*) as verificacion FROM sqlite_master WHERE type='table' AND name='experiencia_aspirante';
SELECT 'Tabla referencias_aspirante creada: ' || COUNT(*) as verificacion FROM sqlite_master WHERE type='table' AND name='referencias_aspirante';
SELECT 'Tabla hoja_vida_pdf creada: ' || COUNT(*) as verificacion FROM sqlite_master WHERE type='table' AND name='hoja_vida_pdf';
SELECT 'Tabla hojas_vida_fts creada: ' || COUNT(*) as verificacion FROM sqlite_master WHERE type='table' AND name='hojas_vida_fts';
