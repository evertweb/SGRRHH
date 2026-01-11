-- =====================================================
-- MIGRACIÓN: Catálogo de Actividades Silviculturales
-- Versión: 1.0
-- Fecha: 2026-01-09
-- Descripción: Agrega categorías, unidades de medida y 
--              actividades silviculturales pre-cargadas
-- =====================================================

-- 1. CREAR TABLA DE CATEGORÍAS
CREATE TABLE IF NOT EXISTS CategoriasActividades (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    Codigo TEXT NOT NULL UNIQUE,
    Nombre TEXT NOT NULL,
    Descripcion TEXT,
    Icono TEXT,
    ColorHex TEXT,
    Orden INTEGER DEFAULT 0,
    Activo INTEGER DEFAULT 1,
    FechaCreacion TEXT DEFAULT (datetime('now', 'localtime')),
    FechaModificacion TEXT
);

-- 2. AGREGAR COLUMNAS A ACTIVIDADES (cada columna por separado para compatibilidad)
-- Nota: SQLite no soporta ADD COLUMN IF NOT EXISTS, usamos PRAGMA para verificar

-- Agregar CategoriaId
ALTER TABLE Actividades ADD COLUMN CategoriaId INTEGER REFERENCES CategoriasActividades(Id);

-- Agregar UnidadMedida
ALTER TABLE Actividades ADD COLUMN UnidadMedida INTEGER DEFAULT 0;

-- Agregar UnidadAbreviatura
ALTER TABLE Actividades ADD COLUMN UnidadAbreviatura TEXT;

-- Agregar RendimientoEsperado
ALTER TABLE Actividades ADD COLUMN RendimientoEsperado REAL;

-- Agregar RendimientoMinimo
ALTER TABLE Actividades ADD COLUMN RendimientoMinimo REAL;

-- Agregar CostoUnitario
ALTER TABLE Actividades ADD COLUMN CostoUnitario REAL;

-- Agregar RequiereCantidad
ALTER TABLE Actividades ADD COLUMN RequiereCantidad INTEGER DEFAULT 0;

-- Agregar TiposProyectoAplicables
ALTER TABLE Actividades ADD COLUMN TiposProyectoAplicables TEXT;

-- Agregar EspeciesAplicables
ALTER TABLE Actividades ADD COLUMN EspeciesAplicables TEXT;

-- Agregar EsDestacada
ALTER TABLE Actividades ADD COLUMN EsDestacada INTEGER DEFAULT 0;

-- Agregar CategoriaTexto (para compatibilidad)
ALTER TABLE Actividades ADD COLUMN CategoriaTexto TEXT;

-- Copiar datos de categoria a CategoriaTexto si existe
UPDATE Actividades SET CategoriaTexto = categoria WHERE CategoriaTexto IS NULL AND categoria IS NOT NULL;

-- 3. AGREGAR COLUMNAS A DETALLES ACTIVIDADES
ALTER TABLE DetallesActividades ADD COLUMN Cantidad REAL;

ALTER TABLE DetallesActividades ADD COLUMN UnidadMedida TEXT;

ALTER TABLE DetallesActividades ADD COLUMN LoteEspecifico TEXT;

-- 4. CREAR ÍNDICES
CREATE INDEX IF NOT EXISTS IX_Actividades_CategoriaId ON Actividades(CategoriaId);
CREATE INDEX IF NOT EXISTS IX_Actividades_UnidadMedida ON Actividades(UnidadMedida);
CREATE INDEX IF NOT EXISTS IX_Actividades_EsDestacada ON Actividades(EsDestacada);
CREATE INDEX IF NOT EXISTS IX_DetallesActividades_Cantidad ON DetallesActividades(Cantidad);

-- =====================================================
-- DATOS SEMILLA: CATEGORÍAS DE ACTIVIDADES
-- =====================================================

INSERT OR IGNORE INTO CategoriasActividades (Codigo, Nombre, Descripcion, ColorHex, Orden, Activo, FechaCreacion) VALUES
('PREP', 'Preparación de Terreno', 'Actividades previas a la siembra: limpieza, trazado, ahoyado', '#8B4513', 1, 1, datetime('now', 'localtime')),
('SIEM', 'Siembra y Establecimiento', 'Plantación de árboles y establecimiento inicial', '#228B22', 2, 1, datetime('now', 'localtime')),
('MANT', 'Mantenimiento', 'Limpias, plateos, control de malezas, fertilización', '#32CD32', 3, 1, datetime('now', 'localtime')),
('PODA', 'Podas', 'Podas de formación, sanitarias, levante', '#006400', 4, 1, datetime('now', 'localtime')),
('RALE', 'Raleos', 'Entresaca y raleo comercial', '#556B2F', 5, 1, datetime('now', 'localtime')),
('FITO', 'Control Fitosanitario', 'Control de plagas, enfermedades, aplicación de agroquímicos', '#FF6347', 6, 1, datetime('now', 'localtime')),
('COSE', 'Cosecha', 'Apeo, desrame, troceo, extracción, cargue', '#A0522D', 7, 1, datetime('now', 'localtime')),
('VIVE', 'Vivero', 'Producción de plántulas: llenado, siembra, riego, mantenimiento', '#90EE90', 8, 1, datetime('now', 'localtime')),
('INVE', 'Inventarios', 'Inventarios forestales, mediciones, parcelas', '#4682B4', 9, 1, datetime('now', 'localtime')),
('INFR', 'Infraestructura', 'Vías, cercas, campamentos, bodegas', '#708090', 10, 1, datetime('now', 'localtime')),
('INCE', 'Control de Incendios', 'Cortafuegos, quemas controladas, vigilancia', '#FF4500', 11, 1, datetime('now', 'localtime')),
('ADMI', 'Administrativa', 'Supervisión, reuniones, capacitación, transporte', '#4169E1', 12, 1, datetime('now', 'localtime')),
('OTRA', 'Otras Actividades', 'Actividades no clasificadas', '#808080', 99, 1, datetime('now', 'localtime'));

-- =====================================================
-- DATOS SEMILLA: ACTIVIDADES SILVICULTURALES
-- =====================================================

-- Obtener IDs de categorías dinámicamente usando subconsultas

-- PREPARACIÓN DE TERRENO
INSERT OR IGNORE INTO Actividades (Codigo, Nombre, Descripcion, CategoriaId, CategoriaTexto, UnidadMedida, UnidadAbreviatura, RendimientoEsperado, requiere_proyecto, RequiereCantidad, EsDestacada, Orden, Activo, FechaCreacion) 
SELECT 'PREP-001', 'Limpieza de Terreno', 'Rocería y limpieza general del lote', Id, 'Preparación de Terreno', 1, 'ha', 0.15, 1, 1, 1, 101, 1, datetime('now', 'localtime') FROM CategoriasActividades WHERE Codigo = 'PREP';

INSERT OR IGNORE INTO Actividades (Codigo, Nombre, Descripcion, CategoriaId, CategoriaTexto, UnidadMedida, UnidadAbreviatura, RendimientoEsperado, requiere_proyecto, RequiereCantidad, EsDestacada, Orden, Activo, FechaCreacion) 
SELECT 'PREP-002', 'Trazado y Estacado', 'Marcación de puntos de siembra según diseño', Id, 'Preparación de Terreno', 2, 'árb', 200, 1, 1, 1, 102, 1, datetime('now', 'localtime') FROM CategoriasActividades WHERE Codigo = 'PREP';

INSERT OR IGNORE INTO Actividades (Codigo, Nombre, Descripcion, CategoriaId, CategoriaTexto, UnidadMedida, UnidadAbreviatura, RendimientoEsperado, requiere_proyecto, RequiereCantidad, EsDestacada, Orden, Activo, FechaCreacion) 
SELECT 'PREP-003', 'Ahoyado Manual', 'Excavación manual de hoyos para siembra', Id, 'Preparación de Terreno', 2, 'hoyos', 80, 1, 1, 1, 103, 1, datetime('now', 'localtime') FROM CategoriasActividades WHERE Codigo = 'PREP';

INSERT OR IGNORE INTO Actividades (Codigo, Nombre, Descripcion, CategoriaId, CategoriaTexto, UnidadMedida, UnidadAbreviatura, RendimientoEsperado, requiere_proyecto, RequiereCantidad, EsDestacada, Orden, Activo, FechaCreacion) 
SELECT 'PREP-004', 'Ahoyado Mecánico', 'Ahoyado con barreno o retroexcavadora', Id, 'Preparación de Terreno', 2, 'hoyos', 300, 1, 1, 0, 104, 1, datetime('now', 'localtime') FROM CategoriasActividades WHERE Codigo = 'PREP';

INSERT OR IGNORE INTO Actividades (Codigo, Nombre, Descripcion, CategoriaId, CategoriaTexto, UnidadMedida, UnidadAbreviatura, RendimientoEsperado, requiere_proyecto, RequiereCantidad, EsDestacada, Orden, Activo, FechaCreacion) 
SELECT 'PREP-005', 'Aplicación de Enmiendas', 'Cal, abonos orgánicos pre-siembra', Id, 'Preparación de Terreno', 1, 'ha', 0.5, 1, 1, 0, 105, 1, datetime('now', 'localtime') FROM CategoriasActividades WHERE Codigo = 'PREP';

INSERT OR IGNORE INTO Actividades (Codigo, Nombre, Descripcion, CategoriaId, CategoriaTexto, UnidadMedida, UnidadAbreviatura, RendimientoEsperado, requiere_proyecto, RequiereCantidad, EsDestacada, Orden, Activo, FechaCreacion) 
SELECT 'PREP-006', 'Construcción de Drenajes', 'Zanjas y canales de drenaje', Id, 'Preparación de Terreno', 3, 'ml', 50, 1, 1, 0, 106, 1, datetime('now', 'localtime') FROM CategoriasActividades WHERE Codigo = 'PREP';

-- SIEMBRA Y ESTABLECIMIENTO
INSERT OR IGNORE INTO Actividades (Codigo, Nombre, Descripcion, CategoriaId, CategoriaTexto, UnidadMedida, UnidadAbreviatura, RendimientoEsperado, requiere_proyecto, RequiereCantidad, EsDestacada, Orden, Activo, FechaCreacion) 
SELECT 'SIEM-001', 'Siembra', 'Plantación de árboles en campo definitivo', Id, 'Siembra y Establecimiento', 2, 'árb', 100, 1, 1, 1, 201, 1, datetime('now', 'localtime') FROM CategoriasActividades WHERE Codigo = 'SIEM';

INSERT OR IGNORE INTO Actividades (Codigo, Nombre, Descripcion, CategoriaId, CategoriaTexto, UnidadMedida, UnidadAbreviatura, RendimientoEsperado, requiere_proyecto, RequiereCantidad, EsDestacada, Orden, Activo, FechaCreacion) 
SELECT 'SIEM-002', 'Resiembra', 'Reposición de árboles muertos o enfermos', Id, 'Siembra y Establecimiento', 2, 'árb', 80, 1, 1, 1, 202, 1, datetime('now', 'localtime') FROM CategoriasActividades WHERE Codigo = 'SIEM';

INSERT OR IGNORE INTO Actividades (Codigo, Nombre, Descripcion, CategoriaId, CategoriaTexto, UnidadMedida, UnidadAbreviatura, RendimientoEsperado, requiere_proyecto, RequiereCantidad, EsDestacada, Orden, Activo, FechaCreacion) 
SELECT 'SIEM-003', 'Transporte de Plántulas', 'Traslado de vivero a campo', Id, 'Siembra y Establecimiento', 7, 'plán', 500, 1, 1, 0, 203, 1, datetime('now', 'localtime') FROM CategoriasActividades WHERE Codigo = 'SIEM';

INSERT OR IGNORE INTO Actividades (Codigo, Nombre, Descripcion, CategoriaId, CategoriaTexto, UnidadMedida, UnidadAbreviatura, RendimientoEsperado, requiere_proyecto, RequiereCantidad, EsDestacada, Orden, Activo, FechaCreacion) 
SELECT 'SIEM-004', 'Fertilización de Establecimiento', 'Primera fertilización post-siembra', Id, 'Siembra y Establecimiento', 2, 'árb', 150, 1, 1, 0, 204, 1, datetime('now', 'localtime') FROM CategoriasActividades WHERE Codigo = 'SIEM';

INSERT OR IGNORE INTO Actividades (Codigo, Nombre, Descripcion, CategoriaId, CategoriaTexto, UnidadMedida, UnidadAbreviatura, RendimientoEsperado, requiere_proyecto, RequiereCantidad, EsDestacada, Orden, Activo, FechaCreacion) 
SELECT 'SIEM-005', 'Tutorado', 'Instalación de tutores o estacas', Id, 'Siembra y Establecimiento', 2, 'árb', 120, 1, 1, 0, 205, 1, datetime('now', 'localtime') FROM CategoriasActividades WHERE Codigo = 'SIEM';

-- MANTENIMIENTO
INSERT OR IGNORE INTO Actividades (Codigo, Nombre, Descripcion, CategoriaId, CategoriaTexto, UnidadMedida, UnidadAbreviatura, RendimientoEsperado, requiere_proyecto, RequiereCantidad, EsDestacada, Orden, Activo, FechaCreacion) 
SELECT 'MANT-001', 'Plateo Manual', 'Limpieza circular alrededor del árbol', Id, 'Mantenimiento', 2, 'árb', 100, 1, 1, 1, 301, 1, datetime('now', 'localtime') FROM CategoriasActividades WHERE Codigo = 'MANT';

INSERT OR IGNORE INTO Actividades (Codigo, Nombre, Descripcion, CategoriaId, CategoriaTexto, UnidadMedida, UnidadAbreviatura, RendimientoEsperado, requiere_proyecto, RequiereCantidad, EsDestacada, Orden, Activo, FechaCreacion) 
SELECT 'MANT-002', 'Plateo Químico', 'Control de malezas con herbicida en plato', Id, 'Mantenimiento', 2, 'árb', 200, 1, 1, 1, 302, 1, datetime('now', 'localtime') FROM CategoriasActividades WHERE Codigo = 'MANT';

INSERT OR IGNORE INTO Actividades (Codigo, Nombre, Descripcion, CategoriaId, CategoriaTexto, UnidadMedida, UnidadAbreviatura, RendimientoEsperado, requiere_proyecto, RequiereCantidad, EsDestacada, Orden, Activo, FechaCreacion) 
SELECT 'MANT-003', 'Rocería Manual', 'Limpieza de calles con machete/guadaña', Id, 'Mantenimiento', 1, 'ha', 0.15, 1, 1, 1, 303, 1, datetime('now', 'localtime') FROM CategoriasActividades WHERE Codigo = 'MANT';

INSERT OR IGNORE INTO Actividades (Codigo, Nombre, Descripcion, CategoriaId, CategoriaTexto, UnidadMedida, UnidadAbreviatura, RendimientoEsperado, requiere_proyecto, RequiereCantidad, EsDestacada, Orden, Activo, FechaCreacion) 
SELECT 'MANT-004', 'Rocería Mecánica', 'Limpieza con guadañadora o tractor', Id, 'Mantenimiento', 1, 'ha', 0.8, 1, 1, 0, 304, 1, datetime('now', 'localtime') FROM CategoriasActividades WHERE Codigo = 'MANT';

INSERT OR IGNORE INTO Actividades (Codigo, Nombre, Descripcion, CategoriaId, CategoriaTexto, UnidadMedida, UnidadAbreviatura, RendimientoEsperado, requiere_proyecto, RequiereCantidad, EsDestacada, Orden, Activo, FechaCreacion) 
SELECT 'MANT-005', 'Fertilización de Mantenimiento', 'Aplicación de fertilizantes', Id, 'Mantenimiento', 2, 'árb', 150, 1, 1, 1, 305, 1, datetime('now', 'localtime') FROM CategoriasActividades WHERE Codigo = 'MANT';

INSERT OR IGNORE INTO Actividades (Codigo, Nombre, Descripcion, CategoriaId, CategoriaTexto, UnidadMedida, UnidadAbreviatura, RendimientoEsperado, requiere_proyecto, RequiereCantidad, EsDestacada, Orden, Activo, FechaCreacion) 
SELECT 'MANT-006', 'Control de Hormiga Arriera', 'Aplicación de cebos y control', Id, 'Mantenimiento', 1, 'ha', 2, 1, 1, 1, 306, 1, datetime('now', 'localtime') FROM CategoriasActividades WHERE Codigo = 'MANT';

INSERT OR IGNORE INTO Actividades (Codigo, Nombre, Descripcion, CategoriaId, CategoriaTexto, UnidadMedida, UnidadAbreviatura, RendimientoEsperado, requiere_proyecto, RequiereCantidad, EsDestacada, Orden, Activo, FechaCreacion) 
SELECT 'MANT-007', 'Limpieza de Borde', 'Mantenimiento de linderos', Id, 'Mantenimiento', 3, 'ml', 100, 1, 1, 0, 307, 1, datetime('now', 'localtime') FROM CategoriasActividades WHERE Codigo = 'MANT';

-- PODAS
INSERT OR IGNORE INTO Actividades (Codigo, Nombre, Descripcion, CategoriaId, CategoriaTexto, UnidadMedida, UnidadAbreviatura, RendimientoEsperado, requiere_proyecto, RequiereCantidad, EsDestacada, Orden, Activo, FechaCreacion) 
SELECT 'PODA-001', 'Poda de Formación Baja', 'Poda hasta 2m de altura (primera poda)', Id, 'Podas', 2, 'árb', 60, 1, 1, 1, 401, 1, datetime('now', 'localtime') FROM CategoriasActividades WHERE Codigo = 'PODA';

INSERT OR IGNORE INTO Actividades (Codigo, Nombre, Descripcion, CategoriaId, CategoriaTexto, UnidadMedida, UnidadAbreviatura, RendimientoEsperado, requiere_proyecto, RequiereCantidad, EsDestacada, Orden, Activo, FechaCreacion) 
SELECT 'PODA-002', 'Poda de Formación Alta', 'Poda de 2m a 4m de altura (segunda poda)', Id, 'Podas', 2, 'árb', 40, 1, 1, 1, 402, 1, datetime('now', 'localtime') FROM CategoriasActividades WHERE Codigo = 'PODA';

INSERT OR IGNORE INTO Actividades (Codigo, Nombre, Descripcion, CategoriaId, CategoriaTexto, UnidadMedida, UnidadAbreviatura, RendimientoEsperado, requiere_proyecto, RequiereCantidad, EsDestacada, Orden, Activo, FechaCreacion) 
SELECT 'PODA-003', 'Poda de Levante', 'Poda superior a 4m', Id, 'Podas', 2, 'árb', 25, 1, 1, 0, 403, 1, datetime('now', 'localtime') FROM CategoriasActividades WHERE Codigo = 'PODA';

INSERT OR IGNORE INTO Actividades (Codigo, Nombre, Descripcion, CategoriaId, CategoriaTexto, UnidadMedida, UnidadAbreviatura, RendimientoEsperado, requiere_proyecto, RequiereCantidad, EsDestacada, Orden, Activo, FechaCreacion) 
SELECT 'PODA-004', 'Poda Sanitaria', 'Eliminación de ramas enfermas o muertas', Id, 'Podas', 2, 'árb', 30, 1, 1, 0, 404, 1, datetime('now', 'localtime') FROM CategoriasActividades WHERE Codigo = 'PODA';

INSERT OR IGNORE INTO Actividades (Codigo, Nombre, Descripcion, CategoriaId, CategoriaTexto, UnidadMedida, UnidadAbreviatura, RendimientoEsperado, requiere_proyecto, RequiereCantidad, EsDestacada, Orden, Activo, FechaCreacion) 
SELECT 'PODA-005', 'Desbrote', 'Eliminación de brotes basales en eucalipto', Id, 'Podas', 2, 'árb', 100, 1, 1, 0, 405, 1, datetime('now', 'localtime') FROM CategoriasActividades WHERE Codigo = 'PODA';

-- RALEOS
INSERT OR IGNORE INTO Actividades (Codigo, Nombre, Descripcion, CategoriaId, CategoriaTexto, UnidadMedida, UnidadAbreviatura, RendimientoEsperado, requiere_proyecto, RequiereCantidad, EsDestacada, Orden, Activo, FechaCreacion) 
SELECT 'RALE-001', 'Marcación de Raleo', 'Selección y marcado de árboles a extraer', Id, 'Raleos', 2, 'árb', 200, 1, 1, 1, 501, 1, datetime('now', 'localtime') FROM CategoriasActividades WHERE Codigo = 'RALE';

INSERT OR IGNORE INTO Actividades (Codigo, Nombre, Descripcion, CategoriaId, CategoriaTexto, UnidadMedida, UnidadAbreviatura, RendimientoEsperado, requiere_proyecto, RequiereCantidad, EsDestacada, Orden, Activo, FechaCreacion) 
SELECT 'RALE-002', 'Apeo de Raleo', 'Corte de árboles seleccionados', Id, 'Raleos', 2, 'árb', 30, 1, 1, 1, 502, 1, datetime('now', 'localtime') FROM CategoriasActividades WHERE Codigo = 'RALE';

INSERT OR IGNORE INTO Actividades (Codigo, Nombre, Descripcion, CategoriaId, CategoriaTexto, UnidadMedida, UnidadAbreviatura, RendimientoEsperado, requiere_proyecto, RequiereCantidad, EsDestacada, Orden, Activo, FechaCreacion) 
SELECT 'RALE-003', 'Desrame', 'Eliminación de ramas del fuste', Id, 'Raleos', 2, 'árb', 40, 1, 1, 0, 503, 1, datetime('now', 'localtime') FROM CategoriasActividades WHERE Codigo = 'RALE';

INSERT OR IGNORE INTO Actividades (Codigo, Nombre, Descripcion, CategoriaId, CategoriaTexto, UnidadMedida, UnidadAbreviatura, RendimientoEsperado, requiere_proyecto, RequiereCantidad, EsDestacada, Orden, Activo, FechaCreacion) 
SELECT 'RALE-004', 'Troceo', 'Corte del fuste en trozas', Id, 'Raleos', 4, 'm³', 3, 1, 1, 0, 504, 1, datetime('now', 'localtime') FROM CategoriasActividades WHERE Codigo = 'RALE';

INSERT OR IGNORE INTO Actividades (Codigo, Nombre, Descripcion, CategoriaId, CategoriaTexto, UnidadMedida, UnidadAbreviatura, RendimientoEsperado, requiere_proyecto, RequiereCantidad, EsDestacada, Orden, Activo, FechaCreacion) 
SELECT 'RALE-005', 'Extracción Manual', 'Arrastre manual de trozas', Id, 'Raleos', 4, 'm³', 2, 1, 1, 0, 505, 1, datetime('now', 'localtime') FROM CategoriasActividades WHERE Codigo = 'RALE';

-- CONTROL FITOSANITARIO
INSERT OR IGNORE INTO Actividades (Codigo, Nombre, Descripcion, CategoriaId, CategoriaTexto, UnidadMedida, UnidadAbreviatura, RendimientoEsperado, requiere_proyecto, RequiereCantidad, EsDestacada, Orden, Activo, FechaCreacion) 
SELECT 'FITO-001', 'Aplicación de Herbicida', 'Control químico de malezas', Id, 'Control Fitosanitario', 1, 'ha', 1.5, 1, 1, 1, 601, 1, datetime('now', 'localtime') FROM CategoriasActividades WHERE Codigo = 'FITO';

INSERT OR IGNORE INTO Actividades (Codigo, Nombre, Descripcion, CategoriaId, CategoriaTexto, UnidadMedida, UnidadAbreviatura, RendimientoEsperado, requiere_proyecto, RequiereCantidad, EsDestacada, Orden, Activo, FechaCreacion) 
SELECT 'FITO-002', 'Aplicación de Insecticida', 'Control de plagas insectiles', Id, 'Control Fitosanitario', 1, 'ha', 1, 1, 1, 0, 602, 1, datetime('now', 'localtime') FROM CategoriasActividades WHERE Codigo = 'FITO';

INSERT OR IGNORE INTO Actividades (Codigo, Nombre, Descripcion, CategoriaId, CategoriaTexto, UnidadMedida, UnidadAbreviatura, RendimientoEsperado, requiere_proyecto, RequiereCantidad, EsDestacada, Orden, Activo, FechaCreacion) 
SELECT 'FITO-003', 'Aplicación de Fungicida', 'Control de enfermedades fúngicas', Id, 'Control Fitosanitario', 1, 'ha', 1, 1, 1, 0, 603, 1, datetime('now', 'localtime') FROM CategoriasActividades WHERE Codigo = 'FITO';

INSERT OR IGNORE INTO Actividades (Codigo, Nombre, Descripcion, CategoriaId, CategoriaTexto, UnidadMedida, UnidadAbreviatura, RendimientoEsperado, requiere_proyecto, RequiereCantidad, EsDestacada, Orden, Activo, FechaCreacion) 
SELECT 'FITO-004', 'Monitoreo Fitosanitario', 'Inspección y registro de plagas', Id, 'Control Fitosanitario', 1, 'ha', 3, 1, 1, 0, 604, 1, datetime('now', 'localtime') FROM CategoriasActividades WHERE Codigo = 'FITO';

-- COSECHA
INSERT OR IGNORE INTO Actividades (Codigo, Nombre, Descripcion, CategoriaId, CategoriaTexto, UnidadMedida, UnidadAbreviatura, RendimientoEsperado, requiere_proyecto, RequiereCantidad, EsDestacada, Orden, Activo, FechaCreacion) 
SELECT 'COSE-001', 'Apeo de Cosecha', 'Corte final de árboles', Id, 'Cosecha', 2, 'árb', 25, 1, 1, 1, 701, 1, datetime('now', 'localtime') FROM CategoriasActividades WHERE Codigo = 'COSE';

INSERT OR IGNORE INTO Actividades (Codigo, Nombre, Descripcion, CategoriaId, CategoriaTexto, UnidadMedida, UnidadAbreviatura, RendimientoEsperado, requiere_proyecto, RequiereCantidad, EsDestacada, Orden, Activo, FechaCreacion) 
SELECT 'COSE-002', 'Desrame de Cosecha', 'Eliminación de ramas', Id, 'Cosecha', 2, 'árb', 35, 1, 1, 0, 702, 1, datetime('now', 'localtime') FROM CategoriasActividades WHERE Codigo = 'COSE';

INSERT OR IGNORE INTO Actividades (Codigo, Nombre, Descripcion, CategoriaId, CategoriaTexto, UnidadMedida, UnidadAbreviatura, RendimientoEsperado, requiere_proyecto, RequiereCantidad, EsDestacada, Orden, Activo, FechaCreacion) 
SELECT 'COSE-003', 'Troceo de Cosecha', 'Corte en trozas según especificaciones', Id, 'Cosecha', 4, 'm³', 4, 1, 1, 1, 703, 1, datetime('now', 'localtime') FROM CategoriasActividades WHERE Codigo = 'COSE';

INSERT OR IGNORE INTO Actividades (Codigo, Nombre, Descripcion, CategoriaId, CategoriaTexto, UnidadMedida, UnidadAbreviatura, RendimientoEsperado, requiere_proyecto, RequiereCantidad, EsDestacada, Orden, Activo, FechaCreacion) 
SELECT 'COSE-004', 'Extracción con Mula', 'Arrastre con animal', Id, 'Cosecha', 4, 'm³', 8, 1, 1, 0, 704, 1, datetime('now', 'localtime') FROM CategoriasActividades WHERE Codigo = 'COSE';

INSERT OR IGNORE INTO Actividades (Codigo, Nombre, Descripcion, CategoriaId, CategoriaTexto, UnidadMedida, UnidadAbreviatura, RendimientoEsperado, requiere_proyecto, RequiereCantidad, EsDestacada, Orden, Activo, FechaCreacion) 
SELECT 'COSE-005', 'Extracción Mecánica', 'Arrastre con skidder o cable', Id, 'Cosecha', 4, 'm³', 25, 1, 1, 0, 705, 1, datetime('now', 'localtime') FROM CategoriasActividades WHERE Codigo = 'COSE';

INSERT OR IGNORE INTO Actividades (Codigo, Nombre, Descripcion, CategoriaId, CategoriaTexto, UnidadMedida, UnidadAbreviatura, RendimientoEsperado, requiere_proyecto, RequiereCantidad, EsDestacada, Orden, Activo, FechaCreacion) 
SELECT 'COSE-006', 'Cargue de Madera', 'Cargue a camión', Id, 'Cosecha', 4, 'm³', 15, 1, 1, 1, 706, 1, datetime('now', 'localtime') FROM CategoriasActividades WHERE Codigo = 'COSE';

INSERT OR IGNORE INTO Actividades (Codigo, Nombre, Descripcion, CategoriaId, CategoriaTexto, UnidadMedida, UnidadAbreviatura, RendimientoEsperado, requiere_proyecto, RequiereCantidad, EsDestacada, Orden, Activo, FechaCreacion) 
SELECT 'COSE-007', 'Apilado', 'Organización de madera en pilas', Id, 'Cosecha', 4, 'm³', 10, 1, 1, 0, 707, 1, datetime('now', 'localtime') FROM CategoriasActividades WHERE Codigo = 'COSE';

-- VIVERO
INSERT OR IGNORE INTO Actividades (Codigo, Nombre, Descripcion, CategoriaId, CategoriaTexto, UnidadMedida, UnidadAbreviatura, RendimientoEsperado, requiere_proyecto, RequiereCantidad, EsDestacada, Orden, Activo, FechaCreacion) 
SELECT 'VIVE-001', 'Llenado de Bolsas', 'Preparación de sustrato y llenado', Id, 'Vivero', 8, 'bolsas', 400, 1, 1, 1, 801, 1, datetime('now', 'localtime') FROM CategoriasActividades WHERE Codigo = 'VIVE';

INSERT OR IGNORE INTO Actividades (Codigo, Nombre, Descripcion, CategoriaId, CategoriaTexto, UnidadMedida, UnidadAbreviatura, RendimientoEsperado, requiere_proyecto, RequiereCantidad, EsDestacada, Orden, Activo, FechaCreacion) 
SELECT 'VIVE-002', 'Siembra en Vivero', 'Siembra de semilla o estaca', Id, 'Vivero', 8, 'bolsas', 500, 1, 1, 1, 802, 1, datetime('now', 'localtime') FROM CategoriasActividades WHERE Codigo = 'VIVE';

INSERT OR IGNORE INTO Actividades (Codigo, Nombre, Descripcion, CategoriaId, CategoriaTexto, UnidadMedida, UnidadAbreviatura, RendimientoEsperado, requiere_proyecto, RequiereCantidad, EsDestacada, Orden, Activo, FechaCreacion) 
SELECT 'VIVE-003', 'Riego de Vivero', 'Riego de plántulas', Id, 'Vivero', 7, 'plán', 2000, 1, 1, 0, 803, 1, datetime('now', 'localtime') FROM CategoriasActividades WHERE Codigo = 'VIVE';

INSERT OR IGNORE INTO Actividades (Codigo, Nombre, Descripcion, CategoriaId, CategoriaTexto, UnidadMedida, UnidadAbreviatura, RendimientoEsperado, requiere_proyecto, RequiereCantidad, EsDestacada, Orden, Activo, FechaCreacion) 
SELECT 'VIVE-004', 'Deshierbe de Bolsas', 'Limpieza de malezas en bolsas', Id, 'Vivero', 8, 'bolsas', 300, 1, 1, 0, 804, 1, datetime('now', 'localtime') FROM CategoriasActividades WHERE Codigo = 'VIVE';

INSERT OR IGNORE INTO Actividades (Codigo, Nombre, Descripcion, CategoriaId, CategoriaTexto, UnidadMedida, UnidadAbreviatura, RendimientoEsperado, requiere_proyecto, RequiereCantidad, EsDestacada, Orden, Activo, FechaCreacion) 
SELECT 'VIVE-005', 'Fertilización de Vivero', 'Aplicación de fertilizantes', Id, 'Vivero', 7, 'plán', 1000, 1, 1, 0, 805, 1, datetime('now', 'localtime') FROM CategoriasActividades WHERE Codigo = 'VIVE';

INSERT OR IGNORE INTO Actividades (Codigo, Nombre, Descripcion, CategoriaId, CategoriaTexto, UnidadMedida, UnidadAbreviatura, RendimientoEsperado, requiere_proyecto, RequiereCantidad, EsDestacada, Orden, Activo, FechaCreacion) 
SELECT 'VIVE-006', 'Clasificación de Plántulas', 'Selección por tamaño y calidad', Id, 'Vivero', 7, 'plán', 500, 1, 1, 0, 806, 1, datetime('now', 'localtime') FROM CategoriasActividades WHERE Codigo = 'VIVE';

INSERT OR IGNORE INTO Actividades (Codigo, Nombre, Descripcion, CategoriaId, CategoriaTexto, UnidadMedida, UnidadAbreviatura, RendimientoEsperado, requiere_proyecto, RequiereCantidad, EsDestacada, Orden, Activo, FechaCreacion) 
SELECT 'VIVE-007', 'Rustificación', 'Preparación para campo', Id, 'Vivero', 7, 'plán', 1000, 1, 1, 0, 807, 1, datetime('now', 'localtime') FROM CategoriasActividades WHERE Codigo = 'VIVE';

-- INVENTARIOS
INSERT OR IGNORE INTO Actividades (Codigo, Nombre, Descripcion, CategoriaId, CategoriaTexto, UnidadMedida, UnidadAbreviatura, RendimientoEsperado, requiere_proyecto, RequiereCantidad, EsDestacada, Orden, Activo, FechaCreacion) 
SELECT 'INVE-001', 'Inventario Forestal', 'Medición de DAP y altura', Id, 'Inventarios', 2, 'árb', 50, 1, 1, 1, 901, 1, datetime('now', 'localtime') FROM CategoriasActividades WHERE Codigo = 'INVE';

INSERT OR IGNORE INTO Actividades (Codigo, Nombre, Descripcion, CategoriaId, CategoriaTexto, UnidadMedida, UnidadAbreviatura, RendimientoEsperado, requiere_proyecto, RequiereCantidad, EsDestacada, Orden, Activo, FechaCreacion) 
SELECT 'INVE-002', 'Conteo de Supervivencia', 'Verificación de plantas vivas', Id, 'Inventarios', 1, 'ha', 2, 1, 1, 1, 902, 1, datetime('now', 'localtime') FROM CategoriasActividades WHERE Codigo = 'INVE';

INSERT OR IGNORE INTO Actividades (Codigo, Nombre, Descripcion, CategoriaId, CategoriaTexto, UnidadMedida, UnidadAbreviatura, RendimientoEsperado, requiere_proyecto, RequiereCantidad, EsDestacada, Orden, Activo, FechaCreacion) 
SELECT 'INVE-003', 'Parcela de Muestreo', 'Levantamiento de parcela permanente', Id, 'Inventarios', 99, 'parc', 2, 1, 1, 0, 903, 1, datetime('now', 'localtime') FROM CategoriasActividades WHERE Codigo = 'INVE';

INSERT OR IGNORE INTO Actividades (Codigo, Nombre, Descripcion, CategoriaId, CategoriaTexto, UnidadMedida, UnidadAbreviatura, RendimientoEsperado, requiere_proyecto, RequiereCantidad, EsDestacada, Orden, Activo, FechaCreacion) 
SELECT 'INVE-004', 'Cubicación en Pie', 'Estimación de volumen en pie', Id, 'Inventarios', 1, 'ha', 1, 1, 1, 0, 904, 1, datetime('now', 'localtime') FROM CategoriasActividades WHERE Codigo = 'INVE';

-- INFRAESTRUCTURA
INSERT OR IGNORE INTO Actividades (Codigo, Nombre, Descripcion, CategoriaId, CategoriaTexto, UnidadMedida, UnidadAbreviatura, RendimientoEsperado, requiere_proyecto, RequiereCantidad, EsDestacada, Orden, Activo, FechaCreacion) 
SELECT 'INFR-001', 'Construcción de Cerca', 'Instalación de postes y alambre', Id, 'Infraestructura', 3, 'ml', 30, 1, 1, 1, 1001, 1, datetime('now', 'localtime') FROM CategoriasActividades WHERE Codigo = 'INFR';

INSERT OR IGNORE INTO Actividades (Codigo, Nombre, Descripcion, CategoriaId, CategoriaTexto, UnidadMedida, UnidadAbreviatura, RendimientoEsperado, requiere_proyecto, RequiereCantidad, EsDestacada, Orden, Activo, FechaCreacion) 
SELECT 'INFR-002', 'Mantenimiento de Cerca', 'Reparación y tensado', Id, 'Infraestructura', 3, 'ml', 100, 1, 1, 0, 1002, 1, datetime('now', 'localtime') FROM CategoriasActividades WHERE Codigo = 'INFR';

INSERT OR IGNORE INTO Actividades (Codigo, Nombre, Descripcion, CategoriaId, CategoriaTexto, UnidadMedida, UnidadAbreviatura, RendimientoEsperado, requiere_proyecto, RequiereCantidad, EsDestacada, Orden, Activo, FechaCreacion) 
SELECT 'INFR-003', 'Apertura de Vía', 'Construcción de camino interno', Id, 'Infraestructura', 3, 'ml', 10, 1, 1, 0, 1003, 1, datetime('now', 'localtime') FROM CategoriasActividades WHERE Codigo = 'INFR';

INSERT OR IGNORE INTO Actividades (Codigo, Nombre, Descripcion, CategoriaId, CategoriaTexto, UnidadMedida, UnidadAbreviatura, RendimientoEsperado, requiere_proyecto, RequiereCantidad, EsDestacada, Orden, Activo, FechaCreacion) 
SELECT 'INFR-004', 'Mantenimiento de Vía', 'Reparación y perfilado', Id, 'Infraestructura', 3, 'ml', 50, 1, 1, 0, 1004, 1, datetime('now', 'localtime') FROM CategoriasActividades WHERE Codigo = 'INFR';

-- CONTROL DE INCENDIOS
INSERT OR IGNORE INTO Actividades (Codigo, Nombre, Descripcion, CategoriaId, CategoriaTexto, UnidadMedida, UnidadAbreviatura, RendimientoEsperado, requiere_proyecto, RequiereCantidad, EsDestacada, Orden, Activo, FechaCreacion) 
SELECT 'INCE-001', 'Construcción Cortafuegos', 'Apertura de fajas cortafuegos', Id, 'Control de Incendios', 3, 'ml', 20, 1, 1, 1, 1101, 1, datetime('now', 'localtime') FROM CategoriasActividades WHERE Codigo = 'INCE';

INSERT OR IGNORE INTO Actividades (Codigo, Nombre, Descripcion, CategoriaId, CategoriaTexto, UnidadMedida, UnidadAbreviatura, RendimientoEsperado, requiere_proyecto, RequiereCantidad, EsDestacada, Orden, Activo, FechaCreacion) 
SELECT 'INCE-002', 'Mantenimiento Cortafuegos', 'Limpieza de fajas existentes', Id, 'Control de Incendios', 3, 'ml', 80, 1, 1, 1, 1102, 1, datetime('now', 'localtime') FROM CategoriasActividades WHERE Codigo = 'INCE';

INSERT OR IGNORE INTO Actividades (Codigo, Nombre, Descripcion, CategoriaId, CategoriaTexto, UnidadMedida, UnidadAbreviatura, RendimientoEsperado, requiere_proyecto, RequiereCantidad, EsDestacada, Orden, Activo, FechaCreacion) 
SELECT 'INCE-003', 'Vigilancia contra Incendios', 'Patrullaje y monitoreo', Id, 'Control de Incendios', 0, '', NULL, 1, 0, 0, 1103, 1, datetime('now', 'localtime') FROM CategoriasActividades WHERE Codigo = 'INCE';

INSERT OR IGNORE INTO Actividades (Codigo, Nombre, Descripcion, CategoriaId, CategoriaTexto, UnidadMedida, UnidadAbreviatura, RendimientoEsperado, requiere_proyecto, RequiereCantidad, EsDestacada, Orden, Activo, FechaCreacion) 
SELECT 'INCE-004', 'Quema Controlada', 'Quema prescrita de residuos', Id, 'Control de Incendios', 1, 'ha', 1, 1, 1, 0, 1104, 1, datetime('now', 'localtime') FROM CategoriasActividades WHERE Codigo = 'INCE';

-- ADMINISTRATIVAS
INSERT OR IGNORE INTO Actividades (Codigo, Nombre, Descripcion, CategoriaId, CategoriaTexto, UnidadMedida, UnidadAbreviatura, RendimientoEsperado, requiere_proyecto, RequiereCantidad, EsDestacada, Orden, Activo, FechaCreacion) 
SELECT 'ADMI-001', 'Supervisión de Campo', 'Supervisión y control de actividades', Id, 'Administrativa', 0, '', NULL, 0, 0, 1, 1201, 1, datetime('now', 'localtime') FROM CategoriasActividades WHERE Codigo = 'ADMI';

INSERT OR IGNORE INTO Actividades (Codigo, Nombre, Descripcion, CategoriaId, CategoriaTexto, UnidadMedida, UnidadAbreviatura, RendimientoEsperado, requiere_proyecto, RequiereCantidad, EsDestacada, Orden, Activo, FechaCreacion) 
SELECT 'ADMI-002', 'Capacitación', 'Entrenamiento de personal', Id, 'Administrativa', 0, '', NULL, 0, 0, 0, 1202, 1, datetime('now', 'localtime') FROM CategoriasActividades WHERE Codigo = 'ADMI';

INSERT OR IGNORE INTO Actividades (Codigo, Nombre, Descripcion, CategoriaId, CategoriaTexto, UnidadMedida, UnidadAbreviatura, RendimientoEsperado, requiere_proyecto, RequiereCantidad, EsDestacada, Orden, Activo, FechaCreacion) 
SELECT 'ADMI-003', 'Transporte de Personal', 'Traslado de trabajadores', Id, 'Administrativa', 0, '', NULL, 0, 0, 0, 1203, 1, datetime('now', 'localtime') FROM CategoriasActividades WHERE Codigo = 'ADMI';

INSERT OR IGNORE INTO Actividades (Codigo, Nombre, Descripcion, CategoriaId, CategoriaTexto, UnidadMedida, UnidadAbreviatura, RendimientoEsperado, requiere_proyecto, RequiereCantidad, EsDestacada, Orden, Activo, FechaCreacion) 
SELECT 'ADMI-004', 'Reunión de Planificación', 'Programación de actividades', Id, 'Administrativa', 0, '', NULL, 0, 0, 0, 1204, 1, datetime('now', 'localtime') FROM CategoriasActividades WHERE Codigo = 'ADMI';

INSERT OR IGNORE INTO Actividades (Codigo, Nombre, Descripcion, CategoriaId, CategoriaTexto, UnidadMedida, UnidadAbreviatura, RendimientoEsperado, requiere_proyecto, RequiereCantidad, EsDestacada, Orden, Activo, FechaCreacion) 
SELECT 'ADMI-005', 'Trámites Legales', 'Permisos, licencias, documentación', Id, 'Administrativa', 0, '', NULL, 0, 0, 0, 1205, 1, datetime('now', 'localtime') FROM CategoriasActividades WHERE Codigo = 'ADMI';

-- Verificación
SELECT 'Categorías insertadas: ' || COUNT(*) FROM CategoriasActividades;
SELECT 'Actividades insertadas: ' || COUNT(*) FROM Actividades WHERE CategoriaId IS NOT NULL;
