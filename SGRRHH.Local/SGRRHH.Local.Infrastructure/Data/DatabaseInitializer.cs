namespace SGRRHH.Local.Infrastructure.Data;

public static class DatabaseInitializer
{
    /// <summary>
    /// Pragmas optimizados para concurrencia multiusuario en red local.
    /// WAL permite lecturas concurrentes mientras hay una escritura.
    /// busy_timeout evita errores "database is locked" en concurrencia.
    /// </summary>
    public const string Pragmas = @"
PRAGMA foreign_keys = ON;
PRAGMA journal_mode = WAL;
PRAGMA synchronous = NORMAL;
PRAGMA busy_timeout = 5000;
PRAGMA cache_size = -20000;
PRAGMA temp_store = MEMORY;
PRAGMA mmap_size = 268435456;
";

    public const string Schema = @"-- catálogos
CREATE TABLE IF NOT EXISTS departamentos (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    codigo TEXT NOT NULL UNIQUE,
    nombre TEXT NOT NULL,
    descripcion TEXT,
    jefe_id INTEGER,
    activo INTEGER DEFAULT 1,
    fecha_creacion TEXT DEFAULT CURRENT_TIMESTAMP,
    fecha_modificacion TEXT
);

CREATE TABLE IF NOT EXISTS cargos (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    codigo TEXT NOT NULL UNIQUE,
    nombre TEXT NOT NULL,
    descripcion TEXT,
    nivel INTEGER DEFAULT 1,
    departamento_id INTEGER REFERENCES departamentos(id),
    salario_base REAL,
    requisitos TEXT,
    competencias TEXT,
    cargo_superior_id INTEGER REFERENCES cargos(id),
    numero_plazas INTEGER DEFAULT 1,
    activo INTEGER DEFAULT 1,
    fecha_creacion TEXT DEFAULT CURRENT_TIMESTAMP,
    fecha_modificacion TEXT
);

CREATE TABLE IF NOT EXISTS tipos_permiso (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    nombre TEXT NOT NULL,
    descripcion TEXT,
    color TEXT DEFAULT '#1E88E5',
    requiere_aprobacion INTEGER DEFAULT 1,
    requiere_documento INTEGER DEFAULT 0,
    dias_por_defecto INTEGER DEFAULT 1,
    dias_maximos INTEGER DEFAULT 0,
    es_compensable INTEGER DEFAULT 0,
    activo INTEGER DEFAULT 1,
    fecha_creacion TEXT DEFAULT CURRENT_TIMESTAMP,
    fecha_modificacion TEXT
);

-- empleados
CREATE TABLE IF NOT EXISTS empleados (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    codigo TEXT NOT NULL UNIQUE,
    cedula TEXT NOT NULL UNIQUE,
    nombres TEXT NOT NULL,
    apellidos TEXT NOT NULL,
    fecha_nacimiento TEXT,
    genero INTEGER,
    estado_civil INTEGER,
    direccion TEXT,
    -- CONTACTO EXPANDIDO (Enero 2026)
    telefono TEXT,
    telefono_celular TEXT,
    telefono_fijo TEXT,
    whatsapp TEXT,
    email TEXT,
    municipio TEXT,
    barrio TEXT,
    -- INFORMACIÓN MÉDICA (EMERGENCIAS)
    tipo_sangre TEXT,
    alergias TEXT,
    condiciones_medicas TEXT,
    medicamentos_actuales TEXT,
    -- CONTACTOS DE EMERGENCIA
    contacto_emergencia TEXT,
    telefono_emergencia TEXT,
    parentesco_contacto_emergencia TEXT,
    telefono_emergencia_2 TEXT,
    contacto_emergencia_2 TEXT,
    telefono_emergencia_2_contacto_2 TEXT,
    parentesco_contacto_emergencia_2 TEXT,
    telefono_emergencia_2_alternativo TEXT,
    -- FIN CONTACTO EXPANDIDO
    foto_path TEXT,
    fecha_ingreso TEXT NOT NULL,
    fecha_retiro TEXT,
    estado INTEGER DEFAULT 1,
    tipo_contrato INTEGER,
    cargo_id INTEGER REFERENCES cargos(id),
    departamento_id INTEGER REFERENCES departamentos(id),
    supervisor_id INTEGER REFERENCES empleados(id),
    observaciones TEXT,
    numero_cuenta TEXT,
    banco TEXT,
    nivel_educacion INTEGER,
    eps TEXT,
    codigo_eps TEXT,
    arl TEXT,
    codigo_arl TEXT,
    clase_riesgo_arl INTEGER DEFAULT 1,
    afp TEXT,
    codigo_afp TEXT,
    caja_compensacion TEXT,
    codigo_caja_compensacion TEXT,
    salario_base REAL,
    creado_por_id INTEGER,
    fecha_solicitud TEXT,
    aprobado_por_id INTEGER,
    fecha_aprobacion TEXT,
    motivo_rechazo TEXT,
    activo INTEGER DEFAULT 1,
    fecha_creacion TEXT DEFAULT CURRENT_TIMESTAMP,
    fecha_modificacion TEXT
);

-- usuarios y autenticación
CREATE TABLE IF NOT EXISTS usuarios (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    username TEXT NOT NULL UNIQUE,
    password_hash TEXT NOT NULL,
    nombre_completo TEXT NOT NULL,
    email TEXT,
    phone_number TEXT,
    rol INTEGER NOT NULL,
    ultimo_acceso TEXT,
    empleado_id INTEGER REFERENCES empleados(id),
    activo INTEGER DEFAULT 1,
    fecha_creacion TEXT DEFAULT CURRENT_TIMESTAMP,
    fecha_modificacion TEXT
);

-- contratos
CREATE TABLE IF NOT EXISTS contratos (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    empleado_id INTEGER NOT NULL REFERENCES empleados(id),
    tipo_contrato INTEGER NOT NULL,
    fecha_inicio TEXT NOT NULL,
    fecha_fin TEXT,
    salario REAL NOT NULL,
    cargo_id INTEGER NOT NULL REFERENCES cargos(id),
    estado INTEGER NOT NULL,
    archivo_adjunto_path TEXT,
    observaciones TEXT,
    activo INTEGER DEFAULT 1,
    fecha_creacion TEXT DEFAULT CURRENT_TIMESTAMP,
    fecha_modificacion TEXT
);

-- permisos
CREATE TABLE IF NOT EXISTS permisos (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    numero_acta TEXT NOT NULL UNIQUE,
    empleado_id INTEGER NOT NULL REFERENCES empleados(id),
    tipo_permiso_id INTEGER NOT NULL REFERENCES tipos_permiso(id),
    motivo TEXT NOT NULL,
    fecha_solicitud TEXT NOT NULL,
    fecha_inicio TEXT NOT NULL,
    fecha_fin TEXT NOT NULL,
    total_dias INTEGER NOT NULL,
    estado INTEGER DEFAULT 1,
    observaciones TEXT,
    documento_soporte_path TEXT,
    dias_pendientes_compensacion INTEGER,
    fecha_compensacion TEXT,
    solicitado_por_id INTEGER NOT NULL REFERENCES usuarios(id),
    aprobado_por_id INTEGER REFERENCES usuarios(id),
    fecha_aprobacion TEXT,
    motivo_rechazo TEXT,
    activo INTEGER DEFAULT 1,
    fecha_creacion TEXT DEFAULT CURRENT_TIMESTAMP,
    fecha_modificacion TEXT
);

-- vacaciones
CREATE TABLE IF NOT EXISTS vacaciones (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    empleado_id INTEGER NOT NULL REFERENCES empleados(id),
    fecha_inicio TEXT NOT NULL,
    fecha_fin TEXT NOT NULL,
    dias_tomados INTEGER NOT NULL,
    periodo_correspondiente INTEGER NOT NULL,
    estado INTEGER DEFAULT 0,
    observaciones TEXT,
    fecha_solicitud TEXT NOT NULL,
    solicitado_por_id INTEGER REFERENCES usuarios(id),
    aprobado_por_id INTEGER REFERENCES usuarios(id),
    fecha_aprobacion TEXT,
    motivo_rechazo TEXT,
    activo INTEGER DEFAULT 1,
    fecha_creacion TEXT DEFAULT CURRENT_TIMESTAMP,
    fecha_modificacion TEXT
);

-- documentos de empleado
CREATE TABLE IF NOT EXISTS documentos_empleado (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    empleado_id INTEGER NOT NULL REFERENCES empleados(id),
    tipo_documento INTEGER NOT NULL,
    nombre TEXT NOT NULL,
    descripcion TEXT,
    archivo_path TEXT NOT NULL,
    nombre_archivo_original TEXT NOT NULL,
    tamano_archivo INTEGER,
    tipo_mime TEXT,
    fecha_vencimiento TEXT,
    fecha_emision TEXT,
    subido_por_usuario_id INTEGER REFERENCES usuarios(id),
    subido_por_nombre TEXT,
    activo INTEGER DEFAULT 1,
    fecha_creacion TEXT DEFAULT CURRENT_TIMESTAMP,
    fecha_modificacion TEXT
);

-- proyectos y actividades
CREATE TABLE IF NOT EXISTS proyectos (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    codigo TEXT NOT NULL UNIQUE,
    nombre TEXT NOT NULL,
    descripcion TEXT,
    cliente TEXT,
    ubicacion TEXT,
    presupuesto REAL,
    progreso INTEGER DEFAULT 0,
    fecha_inicio TEXT,
    fecha_fin TEXT,
    estado INTEGER DEFAULT 0,
    responsable_id INTEGER REFERENCES empleados(id),
    activo INTEGER DEFAULT 1,
    fecha_creacion TEXT DEFAULT CURRENT_TIMESTAMP,
    fecha_modificacion TEXT
);

CREATE TABLE IF NOT EXISTS proyectos_empleados (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    proyecto_id INTEGER NOT NULL REFERENCES proyectos(id),
    empleado_id INTEGER NOT NULL REFERENCES empleados(id),
    fecha_asignacion TEXT,
    fecha_desasignacion TEXT,
    rol TEXT,
    observaciones TEXT,
    activo INTEGER DEFAULT 1,
    fecha_creacion TEXT DEFAULT CURRENT_TIMESTAMP,
    fecha_modificacion TEXT,
    UNIQUE (proyecto_id, empleado_id)
);

CREATE TABLE IF NOT EXISTS actividades (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    codigo TEXT NOT NULL UNIQUE,
    nombre TEXT NOT NULL,
    descripcion TEXT,
    categoria TEXT,
    category_id INTEGER REFERENCES activity_categories(id),
    category_text TEXT,
    unit_of_measure INTEGER DEFAULT 0,
    unit_abbreviation TEXT,
    expected_yield REAL,
    minimum_yield REAL,
    unit_cost REAL,
    requiere_proyecto INTEGER DEFAULT 0,
    requires_quantity INTEGER DEFAULT 0,
    applicable_project_types TEXT,
    applicable_species TEXT,
    orden INTEGER DEFAULT 0,
    is_featured INTEGER DEFAULT 0,
    activo INTEGER DEFAULT 1,
    fecha_creacion TEXT DEFAULT CURRENT_TIMESTAMP,
    fecha_modificacion TEXT
);

-- control diario
CREATE TABLE IF NOT EXISTS registros_diarios (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    fecha TEXT NOT NULL,
    empleado_id INTEGER NOT NULL REFERENCES empleados(id),
    hora_entrada TEXT,
    hora_salida TEXT,
    observaciones TEXT,
    estado INTEGER DEFAULT 0,
    usuario_registro_id INTEGER,
    activo INTEGER DEFAULT 1,
    fecha_creacion TEXT DEFAULT CURRENT_TIMESTAMP,
    fecha_modificacion TEXT,
    UNIQUE(empleado_id, fecha)
);

CREATE TABLE IF NOT EXISTS detalles_actividad (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    registro_diario_id INTEGER NOT NULL REFERENCES registros_diarios(id),
    actividad_id INTEGER REFERENCES actividades(id),
    proyecto_id INTEGER REFERENCES proyectos(id),
    descripcion TEXT,
    horas REAL,
    hora_inicio TEXT,
    hora_fin TEXT,
    orden INTEGER DEFAULT 0,
    activo INTEGER DEFAULT 1,
    fecha_creacion TEXT DEFAULT CURRENT_TIMESTAMP,
    fecha_modificacion TEXT
);

-- auditoría y configuración
CREATE TABLE IF NOT EXISTS audit_logs (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    fecha_hora TEXT NOT NULL,
    usuario_id INTEGER,
    usuario_nombre TEXT,
    accion TEXT NOT NULL,
    entidad TEXT NOT NULL,
    entidad_id INTEGER,
    descripcion TEXT,
    direccion_ip TEXT,
    datos_adicionales TEXT,
    activo INTEGER DEFAULT 1,
    fecha_creacion TEXT DEFAULT CURRENT_TIMESTAMP,
    fecha_modificacion TEXT
);

CREATE TABLE IF NOT EXISTS configuracion_sistema (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    clave TEXT NOT NULL UNIQUE,
    valor TEXT,
    descripcion TEXT,
    categoria TEXT,
    activo INTEGER DEFAULT 1,
    fecha_creacion TEXT DEFAULT CURRENT_TIMESTAMP,
    fecha_modificacion TEXT
);

-- TABLA DE PRESTACIONES SOCIALES (Fase 2)
CREATE TABLE IF NOT EXISTS prestaciones (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    empleado_id INTEGER NOT NULL,
    periodo INTEGER NOT NULL,
    tipo INTEGER NOT NULL,
    fecha_inicio TEXT NOT NULL,
    fecha_fin TEXT NOT NULL,
    salario_base REAL NOT NULL,
    valor_calculado REAL NOT NULL,
    valor_pagado REAL NOT NULL DEFAULT 0,
    fecha_pago TEXT,
    estado INTEGER NOT NULL DEFAULT 1,
    metodo_pago TEXT,
    comprobante_referencia TEXT,
    observaciones TEXT,
    valor_base REAL,
    porcentaje_aplicado REAL,
    dias_proporcionales INTEGER,
    activo INTEGER NOT NULL DEFAULT 1,
    fecha_creacion TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,
    fecha_modificacion TEXT,
    FOREIGN KEY (empleado_id) REFERENCES empleados(id)
);

-- TABLA DE FESTIVOS COLOMBIA (Fase 2)
CREATE TABLE IF NOT EXISTS festivos_colombia (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    fecha TEXT NOT NULL,
    nombre TEXT NOT NULL,
    descripcion TEXT,
    es_ley_emiliani INTEGER NOT NULL DEFAULT 0,
    fecha_original TEXT,
    tipo INTEGER NOT NULL,
    es_fecha_fija INTEGER NOT NULL DEFAULT 1,
    año INTEGER NOT NULL,
    activo INTEGER NOT NULL DEFAULT 1,
    fecha_creacion TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,
    fecha_modificacion TEXT
);

-- TABLA DE CONFIGURACION LEGAL COLOMBIA (Fase 2)
CREATE TABLE IF NOT EXISTS configuracion_legal (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    año INTEGER NOT NULL UNIQUE,
    salario_minimo_mensual REAL NOT NULL,
    auxilio_transporte REAL NOT NULL,
    porcentaje_salud_empleado REAL NOT NULL DEFAULT 4.0,
    porcentaje_salud_empleador REAL NOT NULL DEFAULT 8.5,
    porcentaje_pension_empleado REAL NOT NULL DEFAULT 4.0,
    porcentaje_pension_empleador REAL NOT NULL DEFAULT 12.0,
    porcentaje_caja_compensacion REAL NOT NULL DEFAULT 4.0,
    porcentaje_icbf REAL NOT NULL DEFAULT 3.0,
    porcentaje_sena REAL NOT NULL DEFAULT 2.0,
    arl_clase1_min REAL NOT NULL DEFAULT 0.522,
    arl_clase5_max REAL NOT NULL DEFAULT 6.96,
    porcentaje_intereses_cesantias REAL NOT NULL DEFAULT 12.0,
    dias_vacaciones_año INTEGER NOT NULL DEFAULT 15,
    horas_maximas_semanales INTEGER NOT NULL DEFAULT 48,
    horas_ordinarias_diarias INTEGER NOT NULL DEFAULT 8,
    recargo_hora_extra_diurna REAL NOT NULL DEFAULT 25.0,
    recargo_hora_extra_nocturna REAL NOT NULL DEFAULT 75.0,
    recargo_hora_nocturna REAL NOT NULL DEFAULT 35.0,
    recargo_hora_dominical_festivo REAL NOT NULL DEFAULT 75.0,
    edad_minima_laboral INTEGER NOT NULL DEFAULT 18,
    observaciones TEXT,
    es_vigente INTEGER NOT NULL DEFAULT 0,
    activo INTEGER NOT NULL DEFAULT 1,
    fecha_creacion TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,
    fecha_modificacion TEXT
);

-- TABLA DE NOMINAS (Fase 2)
CREATE TABLE IF NOT EXISTS nominas (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    empleado_id INTEGER NOT NULL,
    periodo TEXT NOT NULL,
    fecha_pago TEXT,
    salario_base REAL NOT NULL,
    auxilio_transporte REAL NOT NULL DEFAULT 0,
    horas_extras_diurnas REAL NOT NULL DEFAULT 0,
    horas_extras_nocturnas REAL NOT NULL DEFAULT 0,
    horas_nocturnas REAL NOT NULL DEFAULT 0,
    horas_dominicales_festivos REAL NOT NULL DEFAULT 0,
    comisiones REAL NOT NULL DEFAULT 0,
    bonificaciones REAL NOT NULL DEFAULT 0,
    otros_devengos REAL NOT NULL DEFAULT 0,
    deduccion_salud REAL NOT NULL DEFAULT 0,
    deduccion_pension REAL NOT NULL DEFAULT 0,
    retencion_fuente REAL NOT NULL DEFAULT 0,
    prestamos REAL NOT NULL DEFAULT 0,
    embargos REAL NOT NULL DEFAULT 0,
    fondo_empleados REAL NOT NULL DEFAULT 0,
    otras_deducciones REAL NOT NULL DEFAULT 0,
    aporte_salud_empleador REAL NOT NULL DEFAULT 0,
    aporte_pension_empleador REAL NOT NULL DEFAULT 0,
    aporte_arl REAL NOT NULL DEFAULT 0,
    aporte_caja_compensacion REAL NOT NULL DEFAULT 0,
    aporte_icbf REAL NOT NULL DEFAULT 0,
    aporte_sena REAL NOT NULL DEFAULT 0,
    estado INTEGER NOT NULL DEFAULT 1,
    aprobado_por_id INTEGER,
    fecha_aprobacion TEXT,
    comprobante_numero TEXT,
    observaciones TEXT,
    activo INTEGER NOT NULL DEFAULT 1,
    fecha_creacion TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,
    fecha_modificacion TEXT,
    FOREIGN KEY (empleado_id) REFERENCES empleados(id),
    FOREIGN KEY (aprobado_por_id) REFERENCES usuarios(id)
);

-- TABLA DE INCAPACIDADES (Módulo Incapacidades)
CREATE TABLE IF NOT EXISTS incapacidades (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    numero_incapacidad TEXT NOT NULL UNIQUE,
    empleado_id INTEGER NOT NULL REFERENCES empleados(id),
    permiso_origen_id INTEGER REFERENCES permisos(id),
    incapacidad_anterior_id INTEGER REFERENCES incapacidades(id),
    es_prorroga INTEGER DEFAULT 0,
    fecha_inicio TEXT NOT NULL,
    fecha_fin TEXT NOT NULL,
    total_dias INTEGER NOT NULL,
    fecha_expedicion TEXT NOT NULL,
    diagnostico_cie10 TEXT,
    diagnostico_descripcion TEXT NOT NULL,
    tipo_incapacidad INTEGER NOT NULL DEFAULT 1,
    entidad_emisora TEXT NOT NULL,
    entidad_pagadora TEXT,
    dias_empresa INTEGER NOT NULL DEFAULT 0,
    dias_eps_arl INTEGER NOT NULL DEFAULT 0,
    porcentaje_pago REAL DEFAULT 66.67,
    valor_dia_base REAL,
    valor_total_cobrar REAL,
    estado INTEGER NOT NULL DEFAULT 1,
    transcrita INTEGER DEFAULT 0,
    fecha_transcripcion TEXT,
    numero_radicado_eps TEXT,
    cobrada INTEGER DEFAULT 0,
    fecha_cobro TEXT,
    valor_cobrado REAL,
    documento_incapacidad_path TEXT,
    documento_transcripcion_path TEXT,
    observaciones TEXT,
    registrado_por_id INTEGER NOT NULL REFERENCES usuarios(id),
    activo INTEGER DEFAULT 1,
    fecha_creacion TEXT DEFAULT CURRENT_TIMESTAMP,
    fecha_modificacion TEXT
);

-- TABLA DE SEGUIMIENTO DE INCAPACIDADES
CREATE TABLE IF NOT EXISTS seguimiento_incapacidades (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    incapacidad_id INTEGER NOT NULL REFERENCES incapacidades(id),
    fecha_accion TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,
    tipo_accion INTEGER NOT NULL,
    descripcion TEXT,
    realizado_por_id INTEGER NOT NULL REFERENCES usuarios(id),
    datos_adicionales TEXT,
    activo INTEGER DEFAULT 1,
    fecha_creacion TEXT DEFAULT CURRENT_TIMESTAMP
);

-- TABLA DE NOTIFICACIONES (Sistema de notificaciones profesional)
CREATE TABLE IF NOT EXISTS notificaciones (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    usuario_destino_id INTEGER NULL REFERENCES usuarios(id) ON DELETE CASCADE,
    titulo TEXT NOT NULL,
    mensaje TEXT NOT NULL,
    icono TEXT DEFAULT '📌',
    tipo TEXT NOT NULL DEFAULT 'Info',
    categoria TEXT NOT NULL DEFAULT 'Sistema',
    prioridad INTEGER DEFAULT 0,
    link TEXT NULL,
    entidad_tipo TEXT NULL,
    entidad_id INTEGER NULL,
    leida INTEGER DEFAULT 0,
    fecha_lectura TEXT NULL,
    fecha_creacion TEXT NOT NULL DEFAULT (datetime('now', 'localtime')),
    creado_por TEXT NULL,
    fecha_expiracion TEXT NULL
);

-- TABLA DE PREFERENCIAS DE NOTIFICACIÓN
CREATE TABLE IF NOT EXISTS preferencias_notificacion (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    usuario_id INTEGER NOT NULL UNIQUE REFERENCES usuarios(id) ON DELETE CASCADE,
    recibir_permisos INTEGER DEFAULT 1,
    recibir_vacaciones INTEGER DEFAULT 1,
    recibir_incapacidades INTEGER DEFAULT 1,
    recibir_sistema INTEGER DEFAULT 1,
    sonido_habilitado INTEGER DEFAULT 1,
    mostrar_en_escritorio INTEGER DEFAULT 0,
    resumen_email TEXT DEFAULT 'Nunca'
);

-- índices
CREATE INDEX IF NOT EXISTS idx_empleados_cedula ON empleados(cedula);
CREATE INDEX IF NOT EXISTS idx_empleados_codigo ON empleados(codigo);
CREATE INDEX IF NOT EXISTS idx_empleados_estado ON empleados(estado);
CREATE INDEX IF NOT EXISTS idx_empleados_departamento ON empleados(departamento_id);
CREATE INDEX IF NOT EXISTS idx_permisos_empleado ON permisos(empleado_id);
CREATE INDEX IF NOT EXISTS idx_permisos_estado ON permisos(estado);
CREATE INDEX IF NOT EXISTS idx_permisos_fechas ON permisos(fecha_inicio, fecha_fin);
CREATE INDEX IF NOT EXISTS idx_vacaciones_empleado ON vacaciones(empleado_id);
CREATE INDEX IF NOT EXISTS idx_vacaciones_periodo ON vacaciones(periodo_correspondiente);
CREATE INDEX IF NOT EXISTS idx_contratos_empleado ON contratos(empleado_id);
CREATE INDEX IF NOT EXISTS idx_registros_fecha ON registros_diarios(fecha);
CREATE INDEX IF NOT EXISTS idx_registros_empleado_fecha ON registros_diarios(empleado_id, fecha);
CREATE INDEX IF NOT EXISTS idx_audit_fecha ON audit_logs(fecha_hora);
CREATE INDEX IF NOT EXISTS idx_audit_entidad ON audit_logs(entidad, entidad_id);

-- Índices para nuevas tablas (Fase 2)
CREATE INDEX IF NOT EXISTS idx_prestaciones_empleado ON prestaciones(empleado_id);
CREATE INDEX IF NOT EXISTS idx_prestaciones_periodo ON prestaciones(periodo);
CREATE INDEX IF NOT EXISTS idx_prestaciones_tipo ON prestaciones(tipo);
CREATE INDEX IF NOT EXISTS idx_festivos_fecha ON festivos_colombia(fecha);
CREATE INDEX IF NOT EXISTS idx_festivos_año ON festivos_colombia(año);
CREATE INDEX IF NOT EXISTS idx_configuracion_legal_año ON configuracion_legal(año);
CREATE INDEX IF NOT EXISTS idx_nominas_empleado ON nominas(empleado_id);
CREATE INDEX IF NOT EXISTS idx_nominas_periodo ON nominas(periodo);
CREATE INDEX IF NOT EXISTS idx_nominas_estado ON nominas(estado);

-- Índices para incapacidades
CREATE INDEX IF NOT EXISTS idx_incapacidad_empleado ON incapacidades(empleado_id);
CREATE INDEX IF NOT EXISTS idx_incapacidad_fechas ON incapacidades(fecha_inicio, fecha_fin);
CREATE INDEX IF NOT EXISTS idx_incapacidad_estado ON incapacidades(estado);
CREATE INDEX IF NOT EXISTS idx_incapacidad_tipo ON incapacidades(tipo_incapacidad);
CREATE INDEX IF NOT EXISTS idx_incapacidad_permiso ON incapacidades(permiso_origen_id);
CREATE INDEX IF NOT EXISTS idx_incapacidad_anterior ON incapacidades(incapacidad_anterior_id);
CREATE INDEX IF NOT EXISTS idx_incapacidad_transcripcion ON incapacidades(transcrita, fecha_inicio);
CREATE INDEX IF NOT EXISTS idx_seg_incapacidad ON seguimiento_incapacidades(incapacidad_id);

-- Índices para notificaciones
CREATE INDEX IF NOT EXISTS idx_notificaciones_usuario ON notificaciones(usuario_destino_id);
CREATE INDEX IF NOT EXISTS idx_notificaciones_leida ON notificaciones(leida);
CREATE INDEX IF NOT EXISTS idx_notificaciones_tipo ON notificaciones(tipo);
CREATE INDEX IF NOT EXISTS idx_notificaciones_fecha ON notificaciones(fecha_creacion DESC);
CREATE INDEX IF NOT EXISTS idx_notificaciones_entidad ON notificaciones(entidad_tipo, entidad_id);

-- Índice UNIQUE para prevenir duplicados en catálogos
CREATE UNIQUE INDEX IF NOT EXISTS idx_tipos_permiso_nombre_unique ON tipos_permiso(nombre);
CREATE UNIQUE INDEX IF NOT EXISTS idx_departamentos_nombre_unique ON departamentos(nombre);
CREATE UNIQUE INDEX IF NOT EXISTS idx_cargos_nombre_unique ON cargos(nombre);

-- =====================================================
-- CATÁLOGOS DE SEGURIDAD SOCIAL COLOMBIA
-- Tablas de referencia para selección en formularios
-- =====================================================

-- TABLA DE EPS COLOMBIA
CREATE TABLE IF NOT EXISTS eps_colombia (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    codigo TEXT NOT NULL UNIQUE,
    nombre TEXT NOT NULL,
    estado TEXT DEFAULT 'Activa',
    cobertura TEXT DEFAULT 'Nacional',
    regimen TEXT DEFAULT 'Contributivo',
    observaciones TEXT,
    activo INTEGER DEFAULT 1,
    fecha_creacion TEXT DEFAULT CURRENT_TIMESTAMP
);

-- TABLA DE AFP COLOMBIA
CREATE TABLE IF NOT EXISTS afp_colombia (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    codigo TEXT NOT NULL UNIQUE,
    nombre TEXT NOT NULL,
    tipo TEXT DEFAULT 'Privado',
    observaciones TEXT,
    activo INTEGER DEFAULT 1,
    fecha_creacion TEXT DEFAULT CURRENT_TIMESTAMP
);

-- TABLA DE ARL COLOMBIA
CREATE TABLE IF NOT EXISTS arl_colombia (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    codigo TEXT NOT NULL UNIQUE,
    nombre TEXT NOT NULL,
    observaciones TEXT,
    activo INTEGER DEFAULT 1,
    fecha_creacion TEXT DEFAULT CURRENT_TIMESTAMP
);

-- TABLA DE CAJAS DE COMPENSACIÓN FAMILIAR
CREATE TABLE IF NOT EXISTS cajas_compensacion (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    codigo TEXT NOT NULL UNIQUE,
    nombre TEXT NOT NULL,
    region TEXT,
    observaciones TEXT,
    activo INTEGER DEFAULT 1,
    fecha_creacion TEXT DEFAULT CURRENT_TIMESTAMP
);

-- Índices para catálogos de seguridad social
CREATE INDEX IF NOT EXISTS idx_eps_codigo ON eps_colombia(codigo);
CREATE INDEX IF NOT EXISTS idx_eps_activo ON eps_colombia(activo);
CREATE INDEX IF NOT EXISTS idx_afp_codigo ON afp_colombia(codigo);
CREATE INDEX IF NOT EXISTS idx_arl_codigo ON arl_colombia(codigo);
CREATE INDEX IF NOT EXISTS idx_cajas_codigo ON cajas_compensacion(codigo);

-- =====================================================
-- TABLAS SILVICULTURALES AVANZADAS
-- =====================================================

-- TABLA DE CATEGORÍAS DE ACTIVIDADES (snake_case estandarizado)
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

-- Vista de compatibilidad para tablas antiguas (transición)
CREATE VIEW IF NOT EXISTS CategoriasActividades AS 
SELECT id AS Id, code AS Codigo, name AS Nombre, description AS Descripcion, 
       icon AS Icono, color_hex AS ColorHex, display_order AS Orden, 
       is_active AS Activo, created_at AS FechaCreacion, updated_at AS FechaModificacion
FROM activity_categories;

-- TABLA DE ESPECIES FORESTALES
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

-- TABLA DE PERFILES DE ESCANEO (snake_case estandarizado)
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

-- Vista de compatibilidad para tablas antiguas (transición)
CREATE VIEW IF NOT EXISTS ScanProfiles AS 
SELECT id AS Id, name AS Name, description AS Description, is_default AS IsDefault,
       dpi AS Dpi, color_mode AS ColorMode, source AS Source, page_size AS PageSize,
       brightness AS Brightness, contrast AS Contrast, gamma AS Gamma, sharpness AS Sharpness,
       black_white_threshold AS BlackWhiteThreshold, auto_deskew AS AutoDeskew, 
       auto_crop AS AutoCrop, created_at AS CreatedAt, last_used_at AS LastUsedAt
FROM scan_profiles;

-- TABLA DE SEGUIMIENTO DE PERMISOS
CREATE TABLE IF NOT EXISTS seguimiento_permisos (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    permiso_id INTEGER NOT NULL REFERENCES permisos(id),
    fecha_accion TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,
    tipo_accion INTEGER NOT NULL,
    descripcion TEXT,
    realizado_por_id INTEGER NOT NULL REFERENCES usuarios(id),
    datos_adicionales TEXT,
    activo INTEGER DEFAULT 1,
    fecha_creacion TEXT DEFAULT CURRENT_TIMESTAMP,
    fecha_modificacion TEXT
);

-- TABLA DE COMPENSACIONES DE HORAS
CREATE TABLE IF NOT EXISTS compensaciones_horas (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    permiso_id INTEGER NOT NULL REFERENCES permisos(id),
    fecha_compensacion TEXT NOT NULL,
    horas_compensadas INTEGER NOT NULL,
    descripcion TEXT,
    aprobado_por_id INTEGER REFERENCES usuarios(id),
    fecha_aprobacion TEXT,
    activo INTEGER DEFAULT 1,
    fecha_creacion TEXT DEFAULT CURRENT_TIMESTAMP,
    fecha_modificacion TEXT
);

-- Índices para tablas silviculturales (snake_case estandarizado)
CREATE INDEX IF NOT EXISTS idx_activity_categories_code ON activity_categories(code);
CREATE INDEX IF NOT EXISTS idx_activity_categories_is_active ON activity_categories(is_active);
CREATE INDEX IF NOT EXISTS idx_especies_forestales_codigo ON especies_forestales(codigo);
CREATE INDEX IF NOT EXISTS idx_especies_forestales_nombre ON especies_forestales(nombre_comun);
CREATE INDEX IF NOT EXISTS idx_especies_forestales_activo ON especies_forestales(activo);
CREATE INDEX IF NOT EXISTS idx_scan_profiles_name ON scan_profiles(name);
CREATE INDEX IF NOT EXISTS idx_scan_profiles_default ON scan_profiles(is_default);
CREATE INDEX IF NOT EXISTS idx_seguimiento_permisos_permiso ON seguimiento_permisos(permiso_id);
CREATE INDEX IF NOT EXISTS idx_seguimiento_permisos_fecha ON seguimiento_permisos(fecha_accion);
CREATE INDEX IF NOT EXISTS idx_seguimiento_permisos_tipo ON seguimiento_permisos(tipo_accion);
CREATE INDEX IF NOT EXISTS idx_compensaciones_horas_permiso ON compensaciones_horas(permiso_id);
CREATE INDEX IF NOT EXISTS idx_compensaciones_horas_fecha ON compensaciones_horas(fecha_compensacion);

-- =====================================================
-- DISPOSITIVOS AUTORIZADOS (Login tipo SSH)
-- Autenticación sin contraseña desde equipos de confianza
-- =====================================================
CREATE TABLE IF NOT EXISTS dispositivos_autorizados (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    usuario_id INTEGER NOT NULL REFERENCES usuarios(id) ON DELETE CASCADE,
    device_token TEXT NOT NULL UNIQUE,
    nombre_dispositivo TEXT NOT NULL,
    huella_navegador TEXT,
    ip_autorizacion TEXT,
    fecha_autorizacion TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,
    fecha_ultimo_uso TEXT,
    fecha_expiracion TEXT,
    activo INTEGER DEFAULT 1,
    fecha_creacion TEXT DEFAULT CURRENT_TIMESTAMP,
    fecha_modificacion TEXT
);

CREATE INDEX IF NOT EXISTS idx_dispositivos_autorizados_token ON dispositivos_autorizados(device_token);
CREATE INDEX IF NOT EXISTS idx_dispositivos_autorizados_usuario ON dispositivos_autorizados(usuario_id);
CREATE INDEX IF NOT EXISTS idx_dispositivos_autorizados_activo ON dispositivos_autorizados(activo);
";

    public const string SeedData = @"-- =====================================================
-- SEED DATA PARA SGRRHH - EMPRESA SILVICULTURAL COLOMBIA
-- Datos precargados para aplicar 'Convention Over Configuration'
-- Generado: Enero 2026
-- =====================================================

-- Usuario administrador por defecto
-- Username: admin | Password: Admin123!
INSERT OR IGNORE INTO usuarios (id, username, password_hash, nombre_completo, rol, activo)
VALUES (1, 'admin', '$2a$11$OWtWjbnlNgdhoar3QcVu2OjSnFAzqytgUxFjEYZF4CYAp3vuB/4CW', 'Administrador del Sistema', 1, 1);

-- =====================================================
-- DEPARTAMENTOS (Estructura típica empresa forestal)
-- =====================================================
INSERT OR IGNORE INTO departamentos (codigo, nombre, descripcion, activo) VALUES
('100', 'Dirección General', 'Estrategia y representación legal', 1),
('200', 'Operaciones Forestales', 'Planeación y ejecución de actividades silviculturales', 1),
('300', 'Administración y Finanzas', 'Contabilidad, compras, nómina y gestión humana', 1),
('400', 'SST y Ambiente', 'Seguridad y Salud en el Trabajo, gestión ambiental', 1),
('500', 'Logística y Maquinaria', 'Mantenimiento de equipos y transporte', 1),
('600', 'Vivero', 'Producción de material vegetal', 1);

-- =====================================================
-- CARGOS (Estructura típica ~20 empleados)
-- Cada cargo está asociado a su departamento correspondiente
-- =====================================================
INSERT OR IGNORE INTO cargos (codigo, nombre, descripcion, nivel, departamento_id, requisitos, activo) VALUES
('GEN01', 'Gerente General', 'Dirección estratégica de la empresa', 5, 1, 'Profesional + Experiencia directiva', 1),
('OPE01', 'Ingeniero Forestal', 'Director técnico de operaciones forestales', 4, 2, 'Ingeniero Forestal titulado', 1),
('OPE02', 'Supervisor de Campo', 'Coordinación de cuadrillas y actividades diarias', 3, 2, 'Técnico Forestal o experiencia equivalente', 1),
('OPE03', 'Operario Forestal - Motosierrista', 'Operación de motosierra para aprovechamiento', 2, 2, 'Certificado competencia motosierrista', 1),
('OPE04', 'Auxiliar de Campo', 'Actividades silviculturales generales', 1, 2, 'Primaria o experiencia en campo', 1),
('OPE05', 'Viverista', 'Producción y cuidado de plántulas', 2, 6, 'Experiencia en vivero forestal', 1),
('OPE06', 'Operario Forestal', 'Actividades silviculturales generales sin especialización', 1, 2, 'Primaria o experiencia en campo', 1),
('LOG01', 'Conductor', 'Transporte de personal y materiales', 2, 5, 'Licencia de conducción C2/C3', 1),
('LOG02', 'Almacenista', 'Control de inventarios y despachos', 2, 5, 'Técnico o experiencia equivalente', 1),
('LOG03', 'Tractorista', 'Operación de tractores para actividades forestales y agrícolas', 2, 5, 'Licencia de conducción, experiencia en maquinaria agrícola', 1),
('MAN01', 'Mecánico', 'Mantenimiento de maquinaria y equipos', 2, 5, 'Técnico en mecánica', 1),
('ADM01', 'Auxiliar Administrativo', 'Apoyo administrativo y gestión documental', 2, 3, 'Técnico Administrativo', 1),
('ADM02', 'Coordinador SST', 'Seguridad y Salud en el Trabajo', 3, 4, 'Profesional SST o Tecnólogo', 1),
('ADM03', 'Contador', 'Gestión contable y tributaria', 4, 3, 'Contador Público titulado', 1);

-- =====================================================
-- TIPOS DE PERMISO (Código Sustantivo del Trabajo 2026)
-- =====================================================
INSERT OR IGNORE INTO tipos_permiso (nombre, descripcion, color, requiere_aprobacion, requiere_documento, dias_por_defecto, dias_maximos, es_compensable, activo) VALUES
('Licencia de Maternidad', 'Ley 1822 de 2017 - 18 semanas remuneradas por EPS', '#9C27B0', 1, 1, 126, 126, 0, 1),
('Licencia de Paternidad', 'Ley 2114 de 2021 - 2 semanas remuneradas por EPS', '#9C27B0', 1, 1, 14, 14, 0, 1),
('Licencia de Luto', 'Ley 1280 de 2009 - 5 días hábiles por fallecimiento familiar', '#212121', 1, 1, 5, 5, 0, 1),
('Grave Calamidad Doméstica', 'Sentencia C-930/09 - Tiempo razonable según situación', '#F44336', 1, 1, 1, 5, 0, 1),
('Cita Médica', 'Ley 2466 de 2025 - Tiempo para asistir a cita médica', '#2196F3', 1, 1, 1, 1, 0, 1),
('Eventos Escolares Hijos', 'Ley 2466 de 2025 - Asistencia a eventos escolares', '#4CAF50', 1, 1, 1, 1, 0, 1),
('Diligencias Legales', 'Citaciones judiciales o administrativas obligatorias', '#FF9800', 1, 1, 1, 3, 0, 1),
('Votación Electoral', 'Ley 403 de 1997 - Media jornada compensatoria', '#795548', 0, 1, 1, 1, 1, 1),
('Permiso Personal', 'Diligencias personales - A criterio del empleador', '#607D8B', 1, 0, 1, 2, 1, 1),
('Licencia No Remunerada', 'CST Art. 57 - Permiso sin goce de salario', '#9E9E9E', 1, 0, 1, 30, 0, 1),
('Capacitación', 'Tiempo para formación autorizada por la empresa', '#00BCD4', 1, 1, 1, 5, 0, 1),
('Incapacidad Médica', 'Incapacidad temporal por enfermedad o accidente', '#E91E63', 0, 1, 1, 180, 0, 1);

-- =====================================================
-- EPS COLOMBIA VIGENTES 2026
-- Códigos según PILA/Supersalud
-- =====================================================
INSERT OR IGNORE INTO eps_colombia (codigo, nombre, estado, cobertura, regimen, observaciones) VALUES
('EPS037', 'Nueva EPS S.A.', 'Activa', 'Nacional', 'Contributivo', 'Mayor cobertura nacional'),
('EPS005', 'Sanitas S.A.S.', 'Activa', 'Nacional', 'Contributivo', 'Intervenida - Operando'),
('EPS010', 'Sura EPS', 'Activa', 'Nacional', 'Contributivo', 'Proceso de retiro voluntario'),
('EPS017', 'Famisanar', 'Activa', 'Nacional', 'Contributivo', 'Énfasis Cundinamarca/Boyacá'),
('EPS044', 'Salud Total', 'Activa', 'Nacional', 'Contributivo', NULL),
('EPS048', 'Salud Mía', 'Activa', 'Regional', 'Contributivo', 'Santander/Norte de Santander'),
('EPS040', 'Aliansalud', 'Activa', 'Nacional', 'Contributivo', NULL),
('EPS016', 'Coosalud', 'Activa', 'Nacional', 'Subsidiado', 'También régimen contributivo'),
('CCF055', 'Cajacopi Atlántico', 'Activa', 'Regional', 'Contributivo', 'Región Caribe'),
('ESS118', 'Emssanar', 'Activa', 'Regional', 'Subsidiado', 'Intervenida - Suroccidente'),
('EPS025', 'Capresoca', 'Activa', 'Regional', 'Contributivo', 'Casanare/Orinoquia');

-- =====================================================
-- AFP COLOMBIA VIGENTES 2026
-- Códigos según PILA
-- =====================================================
INSERT OR IGNORE INTO afp_colombia (codigo, nombre, tipo, observaciones) VALUES
('25-14', 'Colpensiones', 'Público', 'Régimen de Prima Media - Administradora pública'),
('230301', 'Porvenir S.A.', 'Privado', 'Mayor número de afiliados'),
('230201', 'Protección S.A.', 'Privado', 'Grupo Sura'),
('231001', 'Colfondos', 'Privado', 'Grupo Scotiabank'),
('230901', 'Skandia', 'Privado', 'Antes Old Mutual');

-- =====================================================
-- ARL COLOMBIA VIGENTES 2026
-- Códigos según PILA
-- =====================================================
INSERT OR IGNORE INTO arl_colombia (codigo, nombre, observaciones) VALUES
('14-23', 'Positiva Compañía de Seguros', 'Recomendada para sector público y alto riesgo'),
('14-28', 'ARL Sura', 'Líder en prevención - Sector privado'),
('14-4', 'AXA Colpatria Seguros', 'Cobertura nacional'),
('14-7', 'Seguros Bolívar S.A.', 'Cobertura nacional'),
('14-29', 'La Equidad Seguros', 'Enfoque cooperativo'),
('14-25', 'Colmena Seguros', 'Fundación Grupo Social - Enfoque social'),
('14-17', 'Seguros de Vida Alfa', 'Cobertura nacional'),
('14-30', 'Mapfre Colombia Vida', 'Multinacional');

-- =====================================================
-- CAJAS DE COMPENSACIÓN FAMILIAR
-- Énfasis en regiones forestales
-- =====================================================
INSERT OR IGNORE INTO cajas_compensacion (codigo, nombre, region, observaciones) VALUES
('CCF03', 'Comfama', 'Antioquia', 'Caja de Compensación Familiar de Antioquia'),
('CCF02', 'Comfenalco Antioquia', 'Antioquia', NULL),
('CCF36', 'Cofrem', 'Meta/Orinoquia', 'Caja de Compensación del Meta - Zonas forestales'),
('CCF37', 'Comcaja', 'Nacional', 'Caja de Compensación Campesina - Zonas rurales'),
('CCF13', 'Confa', 'Caldas', 'Caja de Compensación de Caldas - Eje Cafetero'),
('CCF09', 'Comfamiliar Huila', 'Huila', NULL),
('CCF27', 'Comfenalco Valle', 'Valle del Cauca', NULL),
('CCF29', 'Colsubsidio', 'Cundinamarca/Nacional', 'Mayor cobertura Bogotá');

-- =====================================================
-- ACTIVIDADES SILVICULTURALES
-- Catálogo base para control diario
-- =====================================================
INSERT OR IGNORE INTO actividades (codigo, nombre, descripcion, categoria, requiere_proyecto, orden, activo) VALUES
-- Preparación de Terreno
('SIL01', 'Rocería/Limpieza manual', 'Limpieza de vegetación con machete o guadaña', 'Preparación Terreno', 1, 10, 1),
('SIL02', 'Ahoyado manual', 'Apertura de huecos 30x30x30 cm para siembra', 'Establecimiento', 1, 20, 1),
('SIL03', 'Trazado y estacado', 'Marcación de puntos de siembra', 'Establecimiento', 1, 15, 1),
-- Establecimiento
('SIL04', 'Siembra de plántulas', 'Plantación de árboles en campo', 'Establecimiento', 1, 25, 1),
('SIL05', 'Resiembra/Replante', 'Reposición de plantas muertas', 'Mantenimiento', 1, 30, 1),
('SIL06', 'Fertilización en siembra', 'Aplicación de fertilizante al momento de plantar', 'Establecimiento', 1, 26, 1),
-- Mantenimiento
('SIL07', 'Plateo manual', 'Limpieza circular alrededor del árbol (1m radio)', 'Mantenimiento', 1, 40, 1),
('SIL08', 'Control de malezas manual', 'Limpieza de malezas con machete', 'Mantenimiento', 1, 41, 1),
('SIL09', 'Control químico de malezas', 'Aplicación de herbicida con bomba', 'Mantenimiento', 1, 42, 1),
('SIL10', 'Fertilización de mantenimiento', 'Aplicación de fertilizante a árboles establecidos', 'Mantenimiento', 1, 43, 1),
-- Protección
('SIL11', 'Control hormiga arriera', 'Control de hormigueros con cebo o aspersión', 'Protección', 1, 50, 1),
('SIL12', 'Monitoreo fitosanitario', 'Revisión y detección de plagas y enfermedades', 'Protección', 1, 51, 1),
('SIL13', 'Aplicación de insecticida', 'Control químico de insectos plaga', 'Protección', 1, 52, 1),
-- Podas y Raleos
('SIL14', 'Poda de formación', 'Primera poda para formar estructura del árbol', 'Podas y Raleos', 1, 60, 1),
('SIL15', 'Poda de elevación', 'Eliminación de ramas bajas', 'Podas y Raleos', 1, 61, 1),
('SIL16', 'Raleo selectivo', 'Eliminación de árboles para reducir competencia', 'Podas y Raleos', 1, 65, 1),
-- Prevención Incendios
('SIL17', 'Construcción de rondas', 'Apertura de brechas cortafuego', 'Prevención Incendios', 1, 70, 1),
('SIL18', 'Mantenimiento de rondas', 'Limpieza de brechas cortafuego existentes', 'Prevención Incendios', 1, 71, 1),
('SIL19', 'Vigilancia/Torrero', 'Patrullaje y vigilancia contra incendios', 'Prevención Incendios', 1, 72, 1),
-- Inventario
('SIL20', 'Inventario forestal', 'Medición de parcelas permanentes', 'Inventario', 1, 80, 1),
('SIL21', 'Conteo de sobrevivencia', 'Evaluación de mortalidad post-siembra', 'Inventario', 1, 81, 1),
('SIL22', 'Recorrido de linderos', 'Verificación y mantenimiento de límites', 'Inventario', 1, 82, 1),
-- Cosecha
('SIL23', 'Tala/Apeo', 'Corte de árboles con motosierra', 'Cosecha', 1, 90, 1),
('SIL24', 'Desrame y troceo', 'Corte de ramas y división en trozas', 'Cosecha', 1, 91, 1),
('SIL25', 'Arrastre/Extracción', 'Movimiento de trozas al patio de acopio', 'Cosecha', 1, 92, 1),
('SIL26', 'Cubicación de madera', 'Medición de volumen de trozas', 'Cosecha', 1, 93, 1),
('SIL27', 'Carga de madera', 'Carga a vehículos de transporte', 'Cosecha', 1, 94, 1),
-- Vivero
('VIV01', 'Llenado de bolsas', 'Preparación de sustrato y llenado', 'Vivero', 0, 100, 1),
('VIV02', 'Siembra en bolsas', 'Siembra de semillas o estacas', 'Vivero', 0, 101, 1),
('VIV03', 'Riego en vivero', 'Riego de plántulas', 'Vivero', 0, 102, 1),
('VIV04', 'Deshierbe en vivero', 'Control de malezas en bolsas', 'Vivero', 0, 103, 1),
('VIV05', 'Selección de plántulas', 'Clasificación por calidad', 'Vivero', 0, 104, 1),
-- Administrativas
('ADM01', 'Reunión de coordinación', 'Planeación y seguimiento de actividades', 'Administrativa', 0, 200, 1),
('ADM02', 'Capacitación', 'Formación y entrenamiento del personal', 'Administrativa', 0, 201, 1),
('ADM03', 'Traslado/Transporte', 'Movilización de personal o materiales', 'Administrativa', 0, 202, 1),
('ADM04', 'Mantenimiento de equipos', 'Reparación y mantenimiento de herramientas', 'Administrativa', 0, 203, 1),
('ADM05', 'Otra actividad', 'Actividad no especificada en catálogo', 'Administrativa', 0, 999, 1);

-- =====================================================
-- CONFIGURACIÓN LEGAL COLOMBIA 2026
-- =====================================================
INSERT OR IGNORE INTO configuracion_legal (
    año, salario_minimo_mensual, auxilio_transporte,
    porcentaje_salud_empleado, porcentaje_salud_empleador,
    porcentaje_pension_empleado, porcentaje_pension_empleador,
    porcentaje_caja_compensacion, porcentaje_icbf, porcentaje_sena,
    arl_clase1_min, arl_clase5_max, porcentaje_intereses_cesantias,
    dias_vacaciones_año, horas_maximas_semanales, horas_ordinarias_diarias,
    recargo_hora_extra_diurna, recargo_hora_extra_nocturna,
    recargo_hora_nocturna, recargo_hora_dominical_festivo,
    edad_minima_laboral, observaciones, es_vigente
) VALUES (
    2026, 1750905, 200000,
    4.0, 8.5,
    4.0, 12.0,
    4.0, 3.0, 2.0,
    0.522, 6.96, 12.0,
    15, 48, 8,
    25.0, 75.0,
    35.0, 75.0,
    18, 'Configuración legal para Colombia 2026 - Salario mínimo oficial $1.750.905', 1
);

-- CONFIGURACIÓN LEGAL COLOMBIA 2025 (referencia histórica)
INSERT OR IGNORE INTO configuracion_legal (
    año, salario_minimo_mensual, auxilio_transporte,
    porcentaje_salud_empleado, porcentaje_salud_empleador,
    porcentaje_pension_empleado, porcentaje_pension_empleador,
    porcentaje_caja_compensacion, porcentaje_icbf, porcentaje_sena,
    arl_clase1_min, arl_clase5_max, porcentaje_intereses_cesantias,
    dias_vacaciones_año, horas_maximas_semanales, horas_ordinarias_diarias,
    recargo_hora_extra_diurna, recargo_hora_extra_nocturna,
    recargo_hora_nocturna, recargo_hora_dominical_festivo,
    edad_minima_laboral, observaciones, es_vigente
) VALUES (
    2025, 1300000, 162000,
    4.0, 8.5,
    4.0, 12.0,
    4.0, 3.0, 2.0,
    0.522, 6.96, 12.0,
    15, 48, 8,
    25.0, 75.0,
    35.0, 75.0,
    18, 'Configuración legal para Colombia 2025', 0
);

-- FESTIVOS COLOMBIA 2026 (Ley 51 de 1983 - Ley Emiliani)
-- Festivos de fecha fija (no se trasladan)
INSERT OR IGNORE INTO festivos_colombia (fecha, nombre, descripcion, tipo, es_fecha_fija, año, es_ley_emiliani)
VALUES 
('2026-01-01', 'Año Nuevo', 'Celebración del año nuevo', 2, 1, 2026, 0),
('2026-05-01', 'Día del Trabajo', 'Día internacional de los trabajadores', 2, 1, 2026, 0),
('2026-07-20', 'Independencia de Colombia', 'Grito de independencia', 3, 1, 2026, 0),
('2026-08-07', 'Batalla de Boyacá', 'Victoria decisiva para la independencia', 3, 1, 2026, 0),
('2026-12-08', 'Inmaculada Concepción', 'Festividad religiosa mariana', 1, 1, 2026, 0),
('2026-12-25', 'Navidad', 'Nacimiento de Jesucristo', 1, 1, 2026, 0);

-- Festivos trasladados al lunes (Ley Emiliani - Ley 51/1983)
-- Estos festivos se celebran el lunes siguiente si caen entre martes y domingo
INSERT OR IGNORE INTO festivos_colombia (fecha, nombre, descripcion, tipo, es_fecha_fija, año, es_ley_emiliani, fecha_original)
VALUES 
('2026-01-12', 'Reyes Magos', 'Epifanía del Señor (trasladado)', 1, 0, 2026, 1, '2026-01-06'),
('2026-03-23', 'San José', 'Día del padre (trasladado)', 1, 0, 2026, 1, '2026-03-19'),
('2026-06-29', 'San Pedro y San Pablo', 'Apóstoles Pedro y Pablo (trasladado)', 1, 0, 2026, 1, '2026-06-29'),
('2026-08-17', 'Asunción de la Virgen', 'Asunción de María (trasladado)', 1, 0, 2026, 1, '2026-08-15'),
('2026-10-12', 'Día de la Raza', 'Descubrimiento de América (trasladado)', 3, 0, 2026, 1, '2026-10-12'),
('2026-11-02', 'Todos los Santos', 'Día de todos los santos (trasladado)', 1, 0, 2026, 1, '2026-11-01'),
('2026-11-16', 'Independencia de Cartagena', 'Independencia de Cartagena (trasladado)', 3, 0, 2026, 1, '2026-11-11');

-- Semana Santa 2026 (fechas variables basadas en Pascua)
INSERT OR IGNORE INTO festivos_colombia (fecha, nombre, descripcion, tipo, es_fecha_fija, año, es_ley_emiliani)
VALUES 
('2026-04-02', 'Jueves Santo', 'Semana Santa - Jueves Santo', 1, 0, 2026, 0),
('2026-04-03', 'Viernes Santo', 'Semana Santa - Viernes Santo', 1, 0, 2026, 0);

-- Festivos variables religiosos 2026 (basados en Pascua)
INSERT OR IGNORE INTO festivos_colombia (fecha, nombre, descripcion, tipo, es_fecha_fija, año, es_ley_emiliani)
VALUES 
('2026-05-18', 'Ascensión del Señor', 'Ascensión de Jesús (trasladado al lunes)', 1, 0, 2026, 1),
('2026-06-08', 'Corpus Christi', 'Cuerpo y Sangre de Cristo (trasladado al lunes)', 1, 0, 2026, 1),
('2026-06-15', 'Sagrado Corazón de Jesús', 'Sagrado Corazón (trasladado al lunes)', 1, 0, 2026, 1);

-- =====================================================
-- CATEGORÍAS DE ACTIVIDADES SILVICULTURALES (snake_case)
-- =====================================================
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

-- =====================================================
-- ESPECIES FORESTALES COLOMBIANAS
-- =====================================================
INSERT OR IGNORE INTO especies_forestales (codigo, nombre_comun, nombre_cientifico, familia, turno_promedio, densidad_recomendada, activo) VALUES
('PIN-PAT', 'Pino Pátula', 'Pinus patula', 'Pinaceae', 18, 1111, 1),
('PIN-CAR', 'Pino Caribe', 'Pinus caribaea', 'Pinaceae', 15, 1111, 1),
('PIN-OOC', 'Pino Oocarpa', 'Pinus oocarpa', 'Pinaceae', 20, 1111, 1),
('PIN-RAD', 'Pino Radiata', 'Pinus radiata', 'Pinaceae', 20, 1111, 1),
('PIN-TEC', 'Pino Tecunumanii', 'Pinus tecunumanii', 'Pinaceae', 16, 1111, 1),
('EUC-GRA', 'Eucalipto Grandis', 'Eucalyptus grandis', 'Myrtaceae', 7, 1111, 1),
('EUC-GLO', 'Eucalipto Glóbulus', 'Eucalyptus globulus', 'Myrtaceae', 10, 1111, 1),
('EUC-URO', 'Eucalipto Urograndis', 'Eucalyptus urograndis', 'Myrtaceae', 6, 1111, 1),
('EUC-PEL', 'Eucalipto Pellita', 'Eucalyptus pellita', 'Myrtaceae', 8, 1111, 1),
('EUC-TER', 'Eucalipto Tereticornis', 'Eucalyptus tereticornis', 'Myrtaceae', 8, 1111, 1),
('TEC-GRA', 'Teca', 'Tectona grandis', 'Lamiaceae', 20, 816, 1),
('ACA-MAN', 'Acacia Mangium', 'Acacia mangium', 'Fabaceae', 8, 1111, 1),
('ACA-AUR', 'Acacia Auriculiformis', 'Acacia auriculiformis', 'Fabaceae', 8, 1111, 1),
('MEL-AZE', 'Melina', 'Gmelina arborea', 'Lamiaceae', 12, 1111, 1),
('CED-ODO', 'Cedro Rosado', 'Cedrela odorata', 'Meliaceae', 25, 625, 1),
('CED-MON', 'Cedro de Montaña', 'Cedrela montana', 'Meliaceae', 25, 625, 1),
('CIP-LUS', 'Ciprés', 'Cupressus lusitanica', 'Cupressaceae', 20, 1111, 1),
('NOG-CAF', 'Nogal Cafetero', 'Cordia alliodora', 'Boraginaceae', 15, 400, 1),
('CAU-CAU', 'Caucho', 'Hevea brasiliensis', 'Euphorbiaceae', 30, 500, 1),
('BAL-SAM', 'Balso', 'Ochroma pyramidale', 'Malvaceae', 5, 1111, 1),
('ROB-NEG', 'Roble Negro', 'Colombobalanus excelsa', 'Fagaceae', 40, 400, 1),
('ALI-MAC', 'Aliso', 'Alnus acuminata', 'Betulaceae', 12, 816, 1),
('MAC-INT', 'Macadamia', 'Macadamia integrifolia', 'Proteaceae', 15, 278, 1),
('GUA-ANG', 'Guadua', 'Guadua angustifolia', 'Poaceae', 5, 625, 1);

-- =====================================================
-- PERFIL DE ESCANEO PREDEFINIDO (snake_case)
-- =====================================================
INSERT OR IGNORE INTO scan_profiles (id, name, description, is_default, dpi, color_mode, source, page_size) VALUES 
(1, 'DOCUMENTO', 'Perfil estándar para escaneo de documentos', 1, 300, 'Color', 'Flatbed', 'Letter');
";

    public const string PerformanceIndexes = @"-- Índices adicionales para consultas frecuentes
CREATE INDEX IF NOT EXISTS idx_empleados_nombres ON empleados(nombres, apellidos);
CREATE INDEX IF NOT EXISTS idx_empleados_cargo ON empleados(cargo_id);
CREATE INDEX IF NOT EXISTS idx_permisos_empleado_fecha ON permisos(empleado_id, fecha_solicitud);
CREATE INDEX IF NOT EXISTS idx_vacaciones_empleado_periodo ON vacaciones(empleado_id, periodo_correspondiente);

-- Índices para optimizar JOINs en catálogos
CREATE INDEX IF NOT EXISTS idx_cargos_departamento ON cargos(departamento_id);
CREATE INDEX IF NOT EXISTS idx_cargos_activo ON cargos(activo);
CREATE INDEX IF NOT EXISTS idx_departamentos_activo ON departamentos(activo);
";

    /// <summary>
    /// Script de migración para agregar columnas a tablas existentes (Fase 2).
    /// Usa ALTER TABLE con manejo seguro de errores (SQLite no soporta IF NOT EXISTS en ALTER).
    /// </summary>
    public const string MigrationsPhase2 = @"
-- Migración: Agregar campos de seguridad social a empleados
-- Estos campos pueden ya existir si la entidad fue creada con ellos
-- SQLite ignora errores de 'duplicate column name'

-- Empleados: Campos adicionales de seguridad social
-- Se usan intentos separados porque SQLite no tiene ALTER COLUMN IF NOT EXISTS

-- Contratos: Campos de liquidación
-- motivo_terminacion, fecha_terminacion, pago_indemnizacion, valor_indemnizacion, liquidacion_id, observaciones_terminacion

-- Registros Diarios: Campos de horas extras
-- horas_extras_diurnas, horas_extras_nocturnas, horas_nocturnas, horas_dominicales_festivos, 
-- horas_extras_dominicales_nocturnas, es_dominical_o_festivo
";

    /// <summary>
    /// Script de migración para agregar columnas de incapacidad a permisos.
    /// Estas columnas permiten vincular permisos con incapacidades.
    /// </summary>
    public const string MigrationsIncapacidades = @"
-- Migración: Agregar campos de incapacidad a permisos
-- Estos ALTER pueden fallar si las columnas ya existen (SQLite no soporta IF NOT EXISTS en ALTER)
-- El código debe manejar estos errores silenciosamente

-- Permisos: Campos para vincular con incapacidades
-- incapacidad_id: FK a incapacidades
-- convertido_a_incapacidad: Flag para saber si el permiso fue convertido
";
}
