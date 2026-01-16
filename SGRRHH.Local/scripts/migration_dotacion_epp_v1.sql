-- ====================================================================
-- MIGRACIÓN: Módulo de Dotación y EPP
-- Versión: 1.0
-- Fecha: Enero 2026
-- Descripción: Sistema de control de dotación y elementos de protección
-- ====================================================================

-- 1. TABLA: TALLAS DEL EMPLEADO (1-1 con empleados)
CREATE TABLE IF NOT EXISTS tallas_empleado (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    empleado_id INTEGER NOT NULL UNIQUE,
    
    -- TALLAS ROPA
    talla_camisa TEXT, -- S, M, L, XL, XXL, XXXL
    talla_pantalon TEXT, -- 28, 30, 32, 34, 36, 38, 40, 42, 44
    talla_overall TEXT, -- S, M, L, XL, XXL
    talla_chaqueta TEXT, -- S, M, L, XL, XXL, XXXL
    
    -- TALLAS CALZADO
    talla_calzado_numero INTEGER, -- 36-46
    ancho_calzado TEXT, -- Normal, Ancho
    tipo_calzado_preferido TEXT, -- Bota, Zapato, Tenis
    
    -- TALLAS PROTECCIÓN
    talla_guantes TEXT, -- S, M, L, XL
    talla_casco TEXT, -- Ajustable, S, M, L
    talla_gafas TEXT, -- Universal, Graduadas
    
    -- OBSERVACIONES
    observaciones TEXT, -- Ej: "Pie ancho, necesita bota especial"
    
    -- AUDITORÍA
    activo INTEGER NOT NULL DEFAULT 1,
    fecha_creacion TEXT NOT NULL,
    fecha_modificacion TEXT,
    
    FOREIGN KEY (empleado_id) REFERENCES empleados(id) ON DELETE CASCADE
);

CREATE INDEX idx_tallas_empleado ON tallas_empleado(empleado_id);


-- 2. TABLA: ENTREGAS DE DOTACIÓN (1-N con empleados)
CREATE TABLE IF NOT EXISTS entregas_dotacion (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    empleado_id INTEGER NOT NULL,
    
    -- DATOS DE LA ENTREGA
    fecha_entrega TEXT NOT NULL, -- ISO: YYYY-MM-DD
    periodo TEXT NOT NULL, -- Ej: "2024-1" (año-periodo)
    tipo_entrega INTEGER NOT NULL DEFAULT 1, -- 1=Dotación Legal, 2=EPP, 3=Ambos
    numero_entrega_anual INTEGER, -- 1, 2, 3 (para dotación legal)
    
    -- ESTADO
    estado INTEGER NOT NULL DEFAULT 1, -- 1=Programada, 2=Entregada, 3=Parcial, 4=Cancelada
    fecha_entrega_real TEXT, -- Fecha en que se entregó (si estado=Entregada)
    
    -- DOCUMENTACIÓN
    documento_acta_id INTEGER, -- FK a documentos_empleados (acta firmada)
    observaciones TEXT,
    
    -- RESPONSABLES
    entregado_por_usuario_id INTEGER, -- Usuario que hizo la entrega
    entregado_por_nombre TEXT, -- Nombre del responsable
    
    -- AUDITORÍA
    activo INTEGER NOT NULL DEFAULT 1,
    fecha_creacion TEXT NOT NULL,
    fecha_modificacion TEXT,
    
    FOREIGN KEY (empleado_id) REFERENCES empleados(id) ON DELETE CASCADE,
    FOREIGN KEY (documento_acta_id) REFERENCES documentos_empleados(id) ON DELETE SET NULL,
    FOREIGN KEY (entregado_por_usuario_id) REFERENCES usuarios(id) ON DELETE SET NULL
);

CREATE INDEX idx_entregas_empleado ON entregas_dotacion(empleado_id);
CREATE INDEX idx_entregas_fecha ON entregas_dotacion(fecha_entrega);
CREATE INDEX idx_entregas_estado ON entregas_dotacion(estado, fecha_entrega);
CREATE INDEX idx_entregas_periodo ON entregas_dotacion(periodo);


-- 3. TABLA: DETALLE DE ELEMENTOS ENTREGADOS (1-N con entregas_dotacion)
CREATE TABLE IF NOT EXISTS detalle_entrega_dotacion (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    entrega_id INTEGER NOT NULL,
    
    -- ELEMENTO
    categoria_elemento INTEGER NOT NULL, -- 1=Camisa, 2=Pantalón, 3=Calzado, 4=EPP, etc.
    nombre_elemento TEXT NOT NULL, -- "Camisa manga larga", "Bota de seguridad", etc.
    cantidad INTEGER NOT NULL DEFAULT 1,
    talla TEXT, -- Talla específica del elemento
    
    -- CLASIFICACIÓN
    es_dotacion_legal INTEGER NOT NULL DEFAULT 0, -- 0=No, 1=Sí
    es_epp INTEGER NOT NULL DEFAULT 0, -- 0=No, 1=Sí (Elemento Protección Personal)
    
    -- DATOS ADICIONALES
    marca TEXT, -- Ej: "3M", "North Face"
    referencia TEXT, -- Código/referencia del producto
    valor_unitario REAL, -- Costo del elemento
    
    observaciones TEXT,
    
    -- AUDITORÍA
    activo INTEGER NOT NULL DEFAULT 1,
    fecha_creacion TEXT NOT NULL,
    
    FOREIGN KEY (entrega_id) REFERENCES entregas_dotacion(id) ON DELETE CASCADE
);

CREATE INDEX idx_detalle_entrega ON detalle_entrega_dotacion(entrega_id);
CREATE INDEX idx_detalle_categoria ON detalle_entrega_dotacion(categoria_elemento);


-- 4. TABLA: PLANTILLAS DE DOTACIÓN (por cargo/actividad)
CREATE TABLE IF NOT EXISTS plantillas_dotacion (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    
    nombre TEXT NOT NULL, -- "Dotación Operario Forestal", "EPP Altura"
    descripcion TEXT,
    tipo INTEGER NOT NULL DEFAULT 1, -- 1=Dotación Legal, 2=EPP, 3=Ambos
    
    -- APLICABILIDAD
    aplica_cargo_id INTEGER, -- FK a cargos (NULL = aplica a todos)
    aplica_departamento_id INTEGER, -- FK a departamentos
    
    -- PERIODICIDAD
    periodicidad_meses INTEGER DEFAULT 4, -- Cada cuántos meses se entrega
    
    -- AUDITORÍA
    activo INTEGER NOT NULL DEFAULT 1,
    fecha_creacion TEXT NOT NULL,
    fecha_modificacion TEXT,
    
    FOREIGN KEY (aplica_cargo_id) REFERENCES cargos(id) ON DELETE SET NULL,
    FOREIGN KEY (aplica_departamento_id) REFERENCES departamentos(id) ON DELETE SET NULL
);

CREATE INDEX idx_plantillas_cargo ON plantillas_dotacion(aplica_cargo_id);


-- 5. TABLA: ELEMENTOS DE PLANTILLA (1-N con plantillas_dotacion)
CREATE TABLE IF NOT EXISTS elementos_plantilla_dotacion (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    plantilla_id INTEGER NOT NULL,
    
    categoria_elemento INTEGER NOT NULL,
    nombre_elemento TEXT NOT NULL,
    cantidad INTEGER NOT NULL DEFAULT 1,
    es_obligatorio INTEGER NOT NULL DEFAULT 1, -- 0=Opcional, 1=Obligatorio
    
    activo INTEGER NOT NULL DEFAULT 1,
    fecha_creacion TEXT NOT NULL,
    
    FOREIGN KEY (plantilla_id) REFERENCES plantillas_dotacion(id) ON DELETE CASCADE
);

CREATE INDEX idx_elementos_plantilla ON elementos_plantilla_dotacion(plantilla_id);


-- ====================================================================
-- MIGRACIÓN AUTOMÁTICA: Crear entregas para dotaciones pasadas (si existen)
-- ====================================================================

-- Si ya hay empleados con fecha de ingreso, crear entregas retroactivas programadas
-- (Solo para empleados activos que devenguen <= 2 SMMLV)

INSERT INTO entregas_dotacion (
    empleado_id, 
    fecha_entrega, 
    periodo, 
    tipo_entrega, 
    numero_entrega_anual, 
    estado, 
    observaciones,
    fecha_creacion
)
SELECT 
    e.id,
    DATE(e.fecha_ingreso, '+' || (n.num * 4) || ' months') as fecha_entrega,
    CAST(strftime('%Y', DATE(e.fecha_ingreso, '+' || (n.num * 4) || ' months')) AS TEXT) || '-' || 
    CAST((n.num % 3) + 1 AS TEXT) as periodo,
    1, -- Dotación Legal
    (n.num % 3) + 1, -- Número de entrega en el año (1, 2, 3)
    CASE 
        WHEN DATE(e.fecha_ingreso, '+' || (n.num * 4) || ' months') <= DATE('now') THEN 2 -- Entregada (pasado)
        ELSE 1 -- Programada (futuro)
    END as estado,
    'Generado automáticamente en migración' as observaciones,
    datetime('now') as fecha_creacion
FROM empleados e
CROSS JOIN (
    SELECT 0 as num UNION SELECT 1 UNION SELECT 2 UNION 
    SELECT 3 UNION SELECT 4 UNION SELECT 5 UNION 
    SELECT 6 UNION SELECT 7 UNION SELECT 8
) n
WHERE e.estado IN (1, 2) -- Activo o En Prueba
  AND e.fecha_ingreso IS NOT NULL
  AND DATE(e.fecha_ingreso, '+' || (n.num * 4) || ' months') <= DATE('now', '+12 months')
  AND NOT EXISTS (
      SELECT 1 FROM entregas_dotacion ed 
      WHERE ed.empleado_id = e.id 
      AND ed.periodo = CAST(strftime('%Y', DATE(e.fecha_ingreso, '+' || (n.num * 4) || ' months')) AS TEXT) || '-' || CAST((n.num % 3) + 1 AS TEXT)
  );

-- Registrar cuántas entregas se generaron
SELECT 'Entregas de dotación generadas', COUNT(*) FROM entregas_dotacion WHERE observaciones LIKE '%migración%';

SELECT '✓ Migración completada exitosamente' as resultado;
