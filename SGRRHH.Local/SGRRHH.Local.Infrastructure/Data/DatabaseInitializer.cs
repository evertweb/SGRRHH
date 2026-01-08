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
    telefono TEXT,
    telefono_emergencia TEXT,
    contacto_emergencia TEXT,
    email TEXT,
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
    arl TEXT,
    afp TEXT,
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
    requiere_proyecto INTEGER DEFAULT 0,
    orden INTEGER DEFAULT 0,
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
";

    public const string SeedData = @"-- Usuario administrador por defecto
-- Username: admin
-- Password: admin123
-- Hash generado con BCrypt.Net-Next (work factor 11)
INSERT OR IGNORE INTO usuarios (id, username, password_hash, nombre_completo, rol, activo)
VALUES (1, 'admin', '$2a$11$XBN0YhJz5xJ8vE0F8XQKL.rNJE5qxQJvJ8VF3xLZqyK5Z5tHqLXm2', 'Administrador del Sistema', 1, 1);

INSERT OR IGNORE INTO tipos_permiso (nombre, descripcion, color, dias_maximos)
VALUES
('Cita Médica', 'Permiso para asistir a cita médica', '#4CAF50', 1),
('Calamidad Doméstica', 'Permiso por calamidad doméstica', '#F44336', 5),
('Licencia de Luto', 'Permiso por fallecimiento de familiar', '#9C27B0', 5),
('Licencia de Maternidad', 'Licencia de maternidad (18 semanas)', '#E91E63', 126),
('Licencia de Paternidad', 'Licencia de paternidad (2 semanas)', '#2196F3', 14),
('Permiso Personal', 'Permiso personal compensable', '#FF9800', 1),
('Diligencia Personal', 'Diligencia personal (banco, notaría)', '#607D8B', 1);

INSERT OR IGNORE INTO departamentos (codigo, nombre, activo)
VALUES
('ADM', 'Administración', 1),
('OPE', 'Operaciones', 1),
('RRH', 'Recursos Humanos', 1);
";

    public const string PerformanceIndexes = @"-- Índices adicionales para consultas frecuentes
CREATE INDEX IF NOT EXISTS idx_empleados_nombres ON empleados(nombres, apellidos);
CREATE INDEX IF NOT EXISTS idx_empleados_cargo ON empleados(cargo_id);
CREATE INDEX IF NOT EXISTS idx_permisos_empleado_fecha ON permisos(empleado_id, fecha_solicitud);
CREATE INDEX IF NOT EXISTS idx_vacaciones_empleado_periodo ON vacaciones(empleado_id, periodo_correspondiente);
";
}
