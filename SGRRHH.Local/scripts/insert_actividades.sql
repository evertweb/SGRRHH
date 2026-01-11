-- Script para insertar actividades silviculturales
-- Las categorías ya están insertadas

-- PREPARACIÓN DE TERRENO (CategoriaId = 1)
INSERT INTO Actividades (codigo, nombre, descripcion, CategoriaId, UnidadMedida, UnidadAbreviatura, RendimientoEsperado, RequiereCantidad, requiere_proyecto, EsDestacada, activo, orden)
SELECT 'PREP-001', 'Chapeo/Limpieza de terreno', 'Limpieza de maleza y vegetación', id, 1, 'ha', 0.25, 1, 1, 1, 1, 1 FROM CategoriasActividades WHERE Codigo = 'PREP';

INSERT INTO Actividades (codigo, nombre, descripcion, CategoriaId, UnidadMedida, UnidadAbreviatura, RendimientoEsperado, RequiereCantidad, requiere_proyecto, EsDestacada, activo, orden)
SELECT 'PREP-002', 'Preparación de suelo manual', 'Preparación del suelo con herramientas manuales', id, 1, 'ha', 0.15, 1, 1, 0, 1, 2 FROM CategoriasActividades WHERE Codigo = 'PREP';

INSERT INTO Actividades (codigo, nombre, descripcion, CategoriaId, UnidadMedida, UnidadAbreviatura, RendimientoEsperado, RequiereCantidad, requiere_proyecto, EsDestacada, activo, orden)
SELECT 'PREP-003', 'Preparación mecanizada', 'Preparación del suelo con maquinaria', id, 1, 'ha', 2.0, 1, 1, 0, 1, 3 FROM CategoriasActividades WHERE Codigo = 'PREP';

INSERT INTO Actividades (codigo, nombre, descripcion, CategoriaId, UnidadMedida, UnidadAbreviatura, RendimientoEsperado, RequiereCantidad, requiere_proyecto, EsDestacada, activo, orden)
SELECT 'PREP-004', 'Trazado y estacado', 'Marcación de puntos de siembra', id, 2, 'pts', 200, 1, 1, 0, 1, 4 FROM CategoriasActividades WHERE Codigo = 'PREP';

INSERT INTO Actividades (codigo, nombre, descripcion, CategoriaId, UnidadMedida, UnidadAbreviatura, RendimientoEsperado, RequiereCantidad, requiere_proyecto, EsDestacada, activo, orden)
SELECT 'PREP-005', 'Hoyado manual', 'Apertura de hoyos para siembra', id, 2, 'hoyos', 80, 1, 1, 1, 1, 5 FROM CategoriasActividades WHERE Codigo = 'PREP';

-- SIEMBRA Y PLANTACIÓN
INSERT INTO Actividades (codigo, nombre, descripcion, CategoriaId, UnidadMedida, UnidadAbreviatura, RendimientoEsperado, RequiereCantidad, requiere_proyecto, EsDestacada, activo, orden)
SELECT 'SIEM-001', 'Siembra de plántulas', 'Plantación de árboles en campo', id, 7, 'plts', 150, 1, 1, 1, 1, 10 FROM CategoriasActividades WHERE Codigo = 'SIEM';

INSERT INTO Actividades (codigo, nombre, descripcion, CategoriaId, UnidadMedida, UnidadAbreviatura, RendimientoEsperado, RequiereCantidad, requiere_proyecto, EsDestacada, activo, orden)
SELECT 'SIEM-002', 'Resiembra/Replante', 'Reposición de plantas muertas', id, 7, 'plts', 100, 1, 1, 0, 1, 11 FROM CategoriasActividades WHERE Codigo = 'SIEM';

INSERT INTO Actividades (codigo, nombre, descripcion, CategoriaId, UnidadMedida, UnidadAbreviatura, RendimientoEsperado, RequiereCantidad, requiere_proyecto, EsDestacada, activo, orden)
SELECT 'SIEM-003', 'Aplicación de fertilizante', 'Fertilización en siembra', id, 7, 'plts', 200, 1, 1, 0, 1, 12 FROM CategoriasActividades WHERE Codigo = 'SIEM';

INSERT INTO Actividades (codigo, nombre, descripcion, CategoriaId, UnidadMedida, UnidadAbreviatura, RendimientoEsperado, RequiereCantidad, requiere_proyecto, EsDestacada, activo, orden)
SELECT 'SIEM-004', 'Riego inicial', 'Riego post-siembra', id, 7, 'plts', 300, 1, 1, 0, 1, 13 FROM CategoriasActividades WHERE Codigo = 'SIEM';

-- MANTENIMIENTO
INSERT INTO Actividades (codigo, nombre, descripcion, CategoriaId, UnidadMedida, UnidadAbreviatura, RendimientoEsperado, RequiereCantidad, requiere_proyecto, EsDestacada, activo, orden)
SELECT 'MANT-001', 'Control de malezas manual', 'Limpieza de malezas con machete', id, 1, 'ha', 0.2, 1, 1, 1, 1, 20 FROM CategoriasActividades WHERE Codigo = 'MANT';

INSERT INTO Actividades (codigo, nombre, descripcion, CategoriaId, UnidadMedida, UnidadAbreviatura, RendimientoEsperado, RequiereCantidad, requiere_proyecto, EsDestacada, activo, orden)
SELECT 'MANT-002', 'Control de malezas químico', 'Aplicación de herbicida', id, 1, 'ha', 1.5, 1, 1, 0, 1, 21 FROM CategoriasActividades WHERE Codigo = 'MANT';

INSERT INTO Actividades (codigo, nombre, descripcion, CategoriaId, UnidadMedida, UnidadAbreviatura, RendimientoEsperado, RequiereCantidad, requiere_proyecto, EsDestacada, activo, orden)
SELECT 'MANT-003', 'Fertilización de mantenimiento', 'Aplicación de fertilizante', id, 2, 'arb', 200, 1, 1, 0, 1, 22 FROM CategoriasActividades WHERE Codigo = 'MANT';

INSERT INTO Actividades (codigo, nombre, descripcion, CategoriaId, UnidadMedida, UnidadAbreviatura, RendimientoEsperado, RequiereCantidad, requiere_proyecto, EsDestacada, activo, orden)
SELECT 'MANT-004', 'Riego de mantenimiento', 'Riego de árboles establecidos', id, 2, 'arb', 250, 1, 1, 0, 1, 23 FROM CategoriasActividades WHERE Codigo = 'MANT';

INSERT INTO Actividades (codigo, nombre, descripcion, CategoriaId, UnidadMedida, UnidadAbreviatura, RendimientoEsperado, RequiereCantidad, requiere_proyecto, EsDestacada, activo, orden)
SELECT 'MANT-005', 'Plateo/Coroneo', 'Limpieza alrededor del árbol', id, 2, 'arb', 100, 1, 1, 1, 1, 24 FROM CategoriasActividades WHERE Codigo = 'MANT';

-- PODAS
INSERT INTO Actividades (codigo, nombre, descripcion, CategoriaId, UnidadMedida, UnidadAbreviatura, RendimientoEsperado, RequiereCantidad, requiere_proyecto, EsDestacada, activo, orden)
SELECT 'PODA-001', 'Poda de formación', 'Poda para formar estructura del árbol', id, 2, 'arb', 40, 1, 1, 1, 1, 30 FROM CategoriasActividades WHERE Codigo = 'PODA';

INSERT INTO Actividades (codigo, nombre, descripcion, CategoriaId, UnidadMedida, UnidadAbreviatura, RendimientoEsperado, RequiereCantidad, requiere_proyecto, EsDestacada, activo, orden)
SELECT 'PODA-002', 'Poda de elevación', 'Eliminación de ramas bajas', id, 2, 'arb', 50, 1, 1, 0, 1, 31 FROM CategoriasActividades WHERE Codigo = 'PODA';

INSERT INTO Actividades (codigo, nombre, descripcion, CategoriaId, UnidadMedida, UnidadAbreviatura, RendimientoEsperado, RequiereCantidad, requiere_proyecto, EsDestacada, activo, orden)
SELECT 'PODA-003', 'Poda sanitaria', 'Eliminación de ramas enfermas o secas', id, 2, 'arb', 35, 1, 1, 0, 1, 32 FROM CategoriasActividades WHERE Codigo = 'PODA';

INSERT INTO Actividades (codigo, nombre, descripcion, CategoriaId, UnidadMedida, UnidadAbreviatura, RendimientoEsperado, RequiereCantidad, requiere_proyecto, EsDestacada, activo, orden)
SELECT 'PODA-004', 'Desrame de árboles en pie', 'Poda de ramas antes del aprovechamiento', id, 2, 'arb', 25, 1, 1, 0, 1, 33 FROM CategoriasActividades WHERE Codigo = 'PODA';

-- RALEOS
INSERT INTO Actividades (codigo, nombre, descripcion, CategoriaId, UnidadMedida, UnidadAbreviatura, RendimientoEsperado, RequiereCantidad, requiere_proyecto, EsDestacada, activo, orden)
SELECT 'RALE-001', 'Raleo selectivo', 'Eliminación de árboles seleccionados', id, 2, 'arb', 15, 1, 1, 1, 1, 40 FROM CategoriasActividades WHERE Codigo = 'RALE';

INSERT INTO Actividades (codigo, nombre, descripcion, CategoriaId, UnidadMedida, UnidadAbreviatura, RendimientoEsperado, RequiereCantidad, requiere_proyecto, EsDestacada, activo, orden)
SELECT 'RALE-002', 'Aclareo sistemático', 'Eliminación por filas o patrón', id, 2, 'arb', 20, 1, 1, 0, 1, 41 FROM CategoriasActividades WHERE Codigo = 'RALE';

INSERT INTO Actividades (codigo, nombre, descripcion, CategoriaId, UnidadMedida, UnidadAbreviatura, RendimientoEsperado, RequiereCantidad, requiere_proyecto, EsDestacada, activo, orden)
SELECT 'RALE-003', 'Marcación para raleo', 'Identificación de árboles a eliminar', id, 2, 'arb', 150, 1, 1, 0, 1, 42 FROM CategoriasActividades WHERE Codigo = 'RALE';

-- PROTECCIÓN FITOSANITARIA
INSERT INTO Actividades (codigo, nombre, descripcion, CategoriaId, UnidadMedida, UnidadAbreviatura, RendimientoEsperado, RequiereCantidad, requiere_proyecto, EsDestacada, activo, orden)
SELECT 'FITO-001', 'Monitoreo de plagas', 'Revisión y detección de plagas', id, 1, 'ha', 3.0, 1, 1, 1, 1, 50 FROM CategoriasActividades WHERE Codigo = 'FITO';

INSERT INTO Actividades (codigo, nombre, descripcion, CategoriaId, UnidadMedida, UnidadAbreviatura, RendimientoEsperado, RequiereCantidad, requiere_proyecto, EsDestacada, activo, orden)
SELECT 'FITO-002', 'Aplicación de insecticida', 'Control químico de insectos', id, 1, 'ha', 2.0, 1, 1, 0, 1, 51 FROM CategoriasActividades WHERE Codigo = 'FITO';

INSERT INTO Actividades (codigo, nombre, descripcion, CategoriaId, UnidadMedida, UnidadAbreviatura, RendimientoEsperado, RequiereCantidad, requiere_proyecto, EsDestacada, activo, orden)
SELECT 'FITO-003', 'Aplicación de fungicida', 'Control de enfermedades fúngicas', id, 1, 'ha', 2.0, 1, 1, 0, 1, 52 FROM CategoriasActividades WHERE Codigo = 'FITO';

INSERT INTO Actividades (codigo, nombre, descripcion, CategoriaId, UnidadMedida, UnidadAbreviatura, RendimientoEsperado, RequiereCantidad, requiere_proyecto, EsDestacada, activo, orden)
SELECT 'FITO-004', 'Control biológico', 'Liberación de controladores', id, 1, 'ha', 4.0, 1, 1, 0, 1, 53 FROM CategoriasActividades WHERE Codigo = 'FITO';

-- COSECHA
INSERT INTO Actividades (codigo, nombre, descripcion, CategoriaId, UnidadMedida, UnidadAbreviatura, RendimientoEsperado, RequiereCantidad, requiere_proyecto, EsDestacada, activo, orden)
SELECT 'COSE-001', 'Tala/Apeo de árboles', 'Corte de árboles con motosierra', id, 2, 'arb', 12, 1, 1, 1, 1, 60 FROM CategoriasActividades WHERE Codigo = 'COSE';

INSERT INTO Actividades (codigo, nombre, descripcion, CategoriaId, UnidadMedida, UnidadAbreviatura, RendimientoEsperado, RequiereCantidad, requiere_proyecto, EsDestacada, activo, orden)
SELECT 'COSE-002', 'Desrame y troceo', 'Corte de ramas y trozas', id, 4, 'm3', 5.0, 1, 1, 0, 1, 61 FROM CategoriasActividades WHERE Codigo = 'COSE';

INSERT INTO Actividades (codigo, nombre, descripcion, CategoriaId, UnidadMedida, UnidadAbreviatura, RendimientoEsperado, RequiereCantidad, requiere_proyecto, EsDestacada, activo, orden)
SELECT 'COSE-003', 'Arrastre/Extracción', 'Movimiento de trozas al patio', id, 4, 'm3', 8.0, 1, 1, 0, 1, 62 FROM CategoriasActividades WHERE Codigo = 'COSE';

INSERT INTO Actividades (codigo, nombre, descripcion, CategoriaId, UnidadMedida, UnidadAbreviatura, RendimientoEsperado, RequiereCantidad, requiere_proyecto, EsDestacada, activo, orden)
SELECT 'COSE-004', 'Cubicación de madera', 'Medición de volumen', id, 4, 'm3', 20.0, 1, 1, 0, 1, 63 FROM CategoriasActividades WHERE Codigo = 'COSE';

INSERT INTO Actividades (codigo, nombre, descripcion, CategoriaId, UnidadMedida, UnidadAbreviatura, RendimientoEsperado, RequiereCantidad, requiere_proyecto, EsDestacada, activo, orden)
SELECT 'COSE-005', 'Carga de madera', 'Carga a vehículos de transporte', id, 4, 'm3', 15.0, 1, 1, 0, 1, 64 FROM CategoriasActividades WHERE Codigo = 'COSE';

-- VIVERO
INSERT INTO Actividades (codigo, nombre, descripcion, CategoriaId, UnidadMedida, UnidadAbreviatura, RendimientoEsperado, RequiereCantidad, requiere_proyecto, EsDestacada, activo, orden)
SELECT 'VIVE-001', 'Llenado de bolsas', 'Preparación de sustrato en bolsas', id, 8, 'bls', 400, 1, 0, 1, 1, 70 FROM CategoriasActividades WHERE Codigo = 'VIVE';

INSERT INTO Actividades (codigo, nombre, descripcion, CategoriaId, UnidadMedida, UnidadAbreviatura, RendimientoEsperado, RequiereCantidad, requiere_proyecto, EsDestacada, activo, orden)
SELECT 'VIVE-002', 'Siembra en bolsas', 'Siembra de semillas o estacas', id, 8, 'bls', 300, 1, 0, 0, 1, 71 FROM CategoriasActividades WHERE Codigo = 'VIVE';

INSERT INTO Actividades (codigo, nombre, descripcion, CategoriaId, UnidadMedida, UnidadAbreviatura, RendimientoEsperado, RequiereCantidad, requiere_proyecto, EsDestacada, activo, orden)
SELECT 'VIVE-003', 'Riego en vivero', 'Riego de plántulas', id, 7, 'plts', 2000, 1, 0, 0, 1, 72 FROM CategoriasActividades WHERE Codigo = 'VIVE';

INSERT INTO Actividades (codigo, nombre, descripcion, CategoriaId, UnidadMedida, UnidadAbreviatura, RendimientoEsperado, RequiereCantidad, requiere_proyecto, EsDestacada, activo, orden)
SELECT 'VIVE-004', 'Deshierbe en vivero', 'Control de malezas en bolsas', id, 7, 'plts', 500, 1, 0, 0, 1, 73 FROM CategoriasActividades WHERE Codigo = 'VIVE';

INSERT INTO Actividades (codigo, nombre, descripcion, CategoriaId, UnidadMedida, UnidadAbreviatura, RendimientoEsperado, RequiereCantidad, requiere_proyecto, EsDestacada, activo, orden)
SELECT 'VIVE-005', 'Selección/Clasificación', 'Clasificación de plántulas por calidad', id, 7, 'plts', 800, 1, 0, 0, 1, 74 FROM CategoriasActividades WHERE Codigo = 'VIVE';

INSERT INTO Actividades (codigo, nombre, descripcion, CategoriaId, UnidadMedida, UnidadAbreviatura, RendimientoEsperado, RequiereCantidad, requiere_proyecto, EsDestacada, activo, orden)
SELECT 'VIVE-006', 'Preparación de estacas', 'Corte y tratamiento de estacas', id, 9, 'est', 200, 1, 0, 0, 1, 75 FROM CategoriasActividades WHERE Codigo = 'VIVE';

-- INVENTARIO FORESTAL
INSERT INTO Actividades (codigo, nombre, descripcion, CategoriaId, UnidadMedida, UnidadAbreviatura, RendimientoEsperado, RequiereCantidad, requiere_proyecto, EsDestacada, activo, orden)
SELECT 'INVE-001', 'Inventario forestal', 'Medición de parcelas de inventario', id, 1, 'ha', 2.0, 1, 1, 1, 1, 80 FROM CategoriasActividades WHERE Codigo = 'INVE';

INSERT INTO Actividades (codigo, nombre, descripcion, CategoriaId, UnidadMedida, UnidadAbreviatura, RendimientoEsperado, RequiereCantidad, requiere_proyecto, EsDestacada, activo, orden)
SELECT 'INVE-002', 'Medición de árboles', 'Medición DAP y altura', id, 2, 'arb', 100, 1, 1, 0, 1, 81 FROM CategoriasActividades WHERE Codigo = 'INVE';

INSERT INTO Actividades (codigo, nombre, descripcion, CategoriaId, UnidadMedida, UnidadAbreviatura, RendimientoEsperado, RequiereCantidad, requiere_proyecto, EsDestacada, activo, orden)
SELECT 'INVE-003', 'Recorrido de linderos', 'Verificación de límites', id, 3, 'm', 2000, 1, 1, 0, 1, 82 FROM CategoriasActividades WHERE Codigo = 'INVE';

INSERT INTO Actividades (codigo, nombre, descripcion, CategoriaId, UnidadMedida, UnidadAbreviatura, RendimientoEsperado, RequiereCantidad, requiere_proyecto, EsDestacada, activo, orden)
SELECT 'INVE-004', 'Conteo de sobrevivencia', 'Evaluación de mortalidad', id, 2, 'arb', 500, 1, 1, 0, 1, 83 FROM CategoriasActividades WHERE Codigo = 'INVE';

-- INFRAESTRUCTURA
INSERT INTO Actividades (codigo, nombre, descripcion, CategoriaId, UnidadMedida, UnidadAbreviatura, RendimientoEsperado, RequiereCantidad, requiere_proyecto, EsDestacada, activo, orden)
SELECT 'INFR-001', 'Construcción de cercas', 'Instalación de cercado', id, 3, 'm', 50, 1, 1, 0, 1, 90 FROM CategoriasActividades WHERE Codigo = 'INFR';

INSERT INTO Actividades (codigo, nombre, descripcion, CategoriaId, UnidadMedida, UnidadAbreviatura, RendimientoEsperado, RequiereCantidad, requiere_proyecto, EsDestacada, activo, orden)
SELECT 'INFR-002', 'Mantenimiento de caminos', 'Reparación de caminos internos', id, 3, 'm', 100, 1, 1, 0, 1, 91 FROM CategoriasActividades WHERE Codigo = 'INFR';

INSERT INTO Actividades (codigo, nombre, descripcion, CategoriaId, UnidadMedida, UnidadAbreviatura, RendimientoEsperado, RequiereCantidad, requiere_proyecto, EsDestacada, activo, orden)
SELECT 'INFR-003', 'Construcción de drenajes', 'Instalación de cunetas', id, 3, 'm', 30, 1, 1, 0, 1, 92 FROM CategoriasActividades WHERE Codigo = 'INFR';

INSERT INTO Actividades (codigo, nombre, descripcion, CategoriaId, UnidadMedida, UnidadAbreviatura, RendimientoEsperado, RequiereCantidad, requiere_proyecto, EsDestacada, activo, orden)
SELECT 'INFR-004', 'Instalación de rótulos', 'Señalización de áreas', id, 99, 'und', 10, 1, 1, 0, 1, 93 FROM CategoriasActividades WHERE Codigo = 'INFR';

-- PREVENCIÓN DE INCENDIOS
INSERT INTO Actividades (codigo, nombre, descripcion, CategoriaId, UnidadMedida, UnidadAbreviatura, RendimientoEsperado, RequiereCantidad, requiere_proyecto, EsDestacada, activo, orden)
SELECT 'INCE-001', 'Construcción de rondas', 'Apertura de brechas cortafuego', id, 3, 'm', 80, 1, 1, 1, 1, 100 FROM CategoriasActividades WHERE Codigo = 'INCE';

INSERT INTO Actividades (codigo, nombre, descripcion, CategoriaId, UnidadMedida, UnidadAbreviatura, RendimientoEsperado, RequiereCantidad, requiere_proyecto, EsDestacada, activo, orden)
SELECT 'INCE-002', 'Mantenimiento de rondas', 'Limpieza de brechas existentes', id, 3, 'm', 200, 1, 1, 0, 1, 101 FROM CategoriasActividades WHERE Codigo = 'INCE';

INSERT INTO Actividades (codigo, nombre, descripcion, CategoriaId, UnidadMedida, UnidadAbreviatura, RendimientoEsperado, RequiereCantidad, requiere_proyecto, EsDestacada, activo, orden)
SELECT 'INCE-003', 'Quemas controladas', 'Quema prescrita de residuos', id, 1, 'ha', 1.0, 1, 1, 0, 1, 102 FROM CategoriasActividades WHERE Codigo = 'INCE';

INSERT INTO Actividades (codigo, nombre, descripcion, CategoriaId, UnidadMedida, UnidadAbreviatura, RendimientoEsperado, RequiereCantidad, requiere_proyecto, EsDestacada, activo, orden)
SELECT 'INCE-004', 'Patrullaje preventivo', 'Vigilancia contra incendios', id, 1, 'ha', 10, 1, 1, 0, 1, 103 FROM CategoriasActividades WHERE Codigo = 'INCE';

-- ACTIVIDADES ADMINISTRATIVAS (sin unidad, solo horas)
INSERT INTO Actividades (codigo, nombre, descripcion, CategoriaId, UnidadMedida, UnidadAbreviatura, RendimientoEsperado, RequiereCantidad, requiere_proyecto, EsDestacada, activo, orden)
SELECT 'ADMI-001', 'Capacitación', 'Formación y entrenamiento', id, 0, '', 0, 0, 0, 0, 1, 110 FROM CategoriasActividades WHERE Codigo = 'ADMI';

INSERT INTO Actividades (codigo, nombre, descripcion, CategoriaId, UnidadMedida, UnidadAbreviatura, RendimientoEsperado, RequiereCantidad, requiere_proyecto, EsDestacada, activo, orden)
SELECT 'ADMI-002', 'Reunión de planificación', 'Coordinación de trabajos', id, 0, '', 0, 0, 0, 0, 1, 111 FROM CategoriasActividades WHERE Codigo = 'ADMI';

INSERT INTO Actividades (codigo, nombre, descripcion, CategoriaId, UnidadMedida, UnidadAbreviatura, RendimientoEsperado, RequiereCantidad, requiere_proyecto, EsDestacada, activo, orden)
SELECT 'ADMI-003', 'Traslado/Transporte', 'Movilización de personal', id, 0, '', 0, 0, 0, 0, 1, 112 FROM CategoriasActividades WHERE Codigo = 'ADMI';

INSERT INTO Actividades (codigo, nombre, descripcion, CategoriaId, UnidadMedida, UnidadAbreviatura, RendimientoEsperado, RequiereCantidad, requiere_proyecto, EsDestacada, activo, orden)
SELECT 'ADMI-004', 'Mantenimiento de equipo', 'Reparación de herramientas', id, 0, '', 0, 0, 0, 0, 1, 113 FROM CategoriasActividades WHERE Codigo = 'ADMI';

-- OTRAS ACTIVIDADES
INSERT INTO Actividades (codigo, nombre, descripcion, CategoriaId, UnidadMedida, UnidadAbreviatura, RendimientoEsperado, RequiereCantidad, requiere_proyecto, EsDestacada, activo, orden)
SELECT 'OTRA-001', 'Otra actividad', 'Actividad no especificada', id, 0, '', 0, 0, 0, 0, 1, 200 FROM CategoriasActividades WHERE Codigo = 'OTRA';

-- Verificar
SELECT 'Actividades insertadas: ' || COUNT(*) FROM Actividades;
