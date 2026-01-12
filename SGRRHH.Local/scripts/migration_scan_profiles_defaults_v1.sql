-- ============================================
-- MIGRACION: Perfil de Escaneo DOCUMENTO
-- Version: 1.1
-- Fecha: 2026-01-11
-- Descripcion: Perfil unico basado en IJ Scan (Canon)
-- ============================================

-- Asegurar que la tabla existe
CREATE TABLE IF NOT EXISTS scan_profiles (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    name TEXT NOT NULL,
    description TEXT,
    is_default INTEGER DEFAULT 0,
    dpi INTEGER DEFAULT 300,
    color_mode TEXT DEFAULT 'Color',
    source TEXT DEFAULT 'Flatbed',
    page_size TEXT DEFAULT 'Letter',
    brightness INTEGER DEFAULT 0,
    contrast INTEGER DEFAULT 0,
    gamma REAL DEFAULT 1.0,
    sharpness INTEGER DEFAULT 0,
    black_white_threshold INTEGER DEFAULT 128,
    auto_deskew INTEGER DEFAULT 0,
    auto_crop INTEGER DEFAULT 0,
    created_at TEXT DEFAULT CURRENT_TIMESTAMP,
    last_used_at TEXT
);

-- Limpiar perfiles anteriores
DELETE FROM scan_profiles;

-- PERFIL: DOCUMENTO (Unico perfil predeterminado)
-- Configuracion basada en IJ Scan de Canon:
--   - Seleccionar origen: Documento (Flatbed)
--   - Modo de color: Color
--   - Tamano de papel: Carta (Letter)
--   - Resolucion: 300 ppp (ajustable 75-600)
INSERT INTO scan_profiles (
    id, name, description, is_default, dpi, color_mode, source, page_size,
    brightness, contrast, gamma, sharpness, black_white_threshold,
    auto_deskew, auto_crop, created_at
) VALUES (
    1,
    'DOCUMENTO',
    'Perfil estandar para escaneo de documentos',
    1,
    300,
    'Color',
    'Flatbed',
    'Letter',
    0, 0, 1.0, 0, 128,
    0, 0,
    datetime('now')
);

-- Verificar
SELECT id, name, dpi, color_mode, source, is_default FROM scan_profiles;
