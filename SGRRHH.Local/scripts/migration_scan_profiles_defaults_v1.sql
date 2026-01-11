-- ============================================
-- MIGRACIÓN: Perfiles de Escaneo Predeterminados
-- Versión: 1.0
-- Fecha: 2026-01-11
-- Descripción: Crea perfiles básicos similares a IJ Scan (Canon)
-- ============================================

-- Asegurar que la tabla existe
CREATE TABLE IF NOT EXISTS scan_profiles (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    name TEXT NOT NULL,
    description TEXT,
    is_default INTEGER DEFAULT 0,
    dpi INTEGER DEFAULT 200,
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

-- Eliminar perfiles existentes (para recrear con los correctos)
DELETE FROM scan_profiles WHERE name IN ('AUTO', 'DOCUMENTO', 'FOTO', 'DOCUMENTO RAPIDO', 'ALTA CALIDAD');

-- ============================================
-- PERFIL: AUTO (Predeterminado)
-- Similar a "Auto" de IJ Scan - Balance entre calidad y velocidad
-- ============================================
INSERT INTO scan_profiles (
    name, description, is_default, dpi, color_mode, source, page_size,
    brightness, contrast, gamma, sharpness, black_white_threshold,
    auto_deskew, auto_crop, created_at
) VALUES (
    'AUTO',
    'Configuración automática balanceada. Ideal para uso general.',
    1, -- Es el predeterminado
    200,
    'Color',
    'Auto',
    'Letter',
    0, 0, 1.0, 0, 128,
    0, 0,
    datetime('now')
);

-- ============================================
-- PERFIL: DOCUMENTO
-- Similar a "Documento" de IJ Scan - Optimizado para texto
-- ============================================
INSERT INTO scan_profiles (
    name, description, is_default, dpi, color_mode, source, page_size,
    brightness, contrast, gamma, sharpness, black_white_threshold,
    auto_deskew, auto_crop, created_at
) VALUES (
    'DOCUMENTO',
    'Optimizado para documentos de texto. Escala de grises, 200 DPI.',
    0,
    200,
    'Grayscale',
    'Flatbed',
    'Letter',
    5, 10, 1.0, 20, 128,
    1, 0,
    datetime('now')
);

-- ============================================
-- PERFIL: FOTO
-- Similar a "Foto" de IJ Scan - Máxima calidad para fotos
-- ============================================
INSERT INTO scan_profiles (
    name, description, is_default, dpi, color_mode, source, page_size,
    brightness, contrast, gamma, sharpness, black_white_threshold,
    auto_deskew, auto_crop, created_at
) VALUES (
    'FOTO',
    'Alta calidad para fotografías. Color, 300 DPI.',
    0,
    300,
    'Color',
    'Flatbed',
    'Letter',
    0, 5, 1.1, 15, 128,
    0, 0,
    datetime('now')
);

-- ============================================
-- PERFIL: DOCUMENTO RÁPIDO
-- Velocidad máxima para documentos simples
-- ============================================
INSERT INTO scan_profiles (
    name, description, is_default, dpi, color_mode, source, page_size,
    brightness, contrast, gamma, sharpness, black_white_threshold,
    auto_deskew, auto_crop, created_at
) VALUES (
    'DOCUMENTO RAPIDO',
    'Escaneo rápido para documentos simples. 150 DPI, escala de grises.',
    0,
    150,
    'Grayscale',
    'Auto',
    'Letter',
    0, 5, 1.0, 0, 128,
    0, 0,
    datetime('now')
);

-- ============================================
-- PERFIL: ALTA CALIDAD
-- Máxima calidad para documentos importantes
-- ============================================
INSERT INTO scan_profiles (
    name, description, is_default, dpi, color_mode, source, page_size,
    brightness, contrast, gamma, sharpness, black_white_threshold,
    auto_deskew, auto_crop, created_at
) VALUES (
    'ALTA CALIDAD',
    'Máxima calidad para documentos importantes. Color, 600 DPI.',
    0,
    600,
    'Color',
    'Flatbed',
    'Letter',
    0, 0, 1.0, 10, 128,
    1, 0,
    datetime('now')
);

-- Verificar inserción
SELECT id, name, dpi, color_mode, source, is_default FROM scan_profiles ORDER BY is_default DESC, name;
