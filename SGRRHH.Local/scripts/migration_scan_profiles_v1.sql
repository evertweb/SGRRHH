-- =====================================================
-- MIGRACIÓN: Perfiles de Escaneo
-- Versión: 1.0
-- Fecha: 2026-01-11
-- Descripción: Tabla para guardar configuraciones de escaneo reutilizables
-- =====================================================

-- Crear tabla de perfiles de escaneo
CREATE TABLE IF NOT EXISTS ScanProfiles (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    Name TEXT NOT NULL,
    Description TEXT,
    IsDefault INTEGER NOT NULL DEFAULT 0,
    Dpi INTEGER NOT NULL DEFAULT 200,
    ColorMode TEXT NOT NULL DEFAULT 'Color',
    Source TEXT NOT NULL DEFAULT 'Flatbed',
    PageSize TEXT NOT NULL DEFAULT 'Letter',
    Brightness INTEGER DEFAULT 0,
    Contrast INTEGER DEFAULT 0,
    Gamma REAL DEFAULT 1.0,
    Sharpness INTEGER DEFAULT 0,
    BlackWhiteThreshold INTEGER DEFAULT 128,
    AutoDeskew INTEGER DEFAULT 0,
    AutoCrop INTEGER DEFAULT 0,
    CreatedAt TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,
    LastUsedAt TEXT
);

-- Índice para búsqueda por nombre
CREATE INDEX IF NOT EXISTS IX_ScanProfiles_Name ON ScanProfiles(Name);

-- Índice para encontrar perfil por defecto
CREATE INDEX IF NOT EXISTS IX_ScanProfiles_IsDefault ON ScanProfiles(IsDefault);

-- Perfiles predefinidos del sistema
INSERT OR IGNORE INTO ScanProfiles (Id, Name, Description, IsDefault, Dpi, ColorMode, Source, PageSize)
VALUES 
    (1, 'Documento Rápido', 'Escaneo rápido de documentos en escala de grises', 0, 150, 'Grayscale', 'Flatbed', 'Letter'),
    (2, 'Documento Alta Calidad', 'Escaneo de alta calidad para archivo permanente', 1, 300, 'Color', 'Flatbed', 'Letter'),
    (3, 'Cédula/ID', 'Optimizado para documentos de identidad', 0, 300, 'Color', 'Flatbed', 'A5'),
    (4, 'Foto', 'Escaneo de fotografías en alta resolución', 0, 600, 'Color', 'Flatbed', 'Letter'),
    (5, 'Blanco y Negro', 'Documentos de texto para OCR', 0, 300, 'BlackWhite', 'Flatbed', 'Letter');

-- =====================================================
-- FIN DE MIGRACIÓN
-- =====================================================
