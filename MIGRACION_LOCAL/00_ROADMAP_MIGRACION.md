# 🚀 ROADMAP: Migración de SGRRHH a Entorno Local Premium

## 📋 Resumen Ejecutivo

Este documento define el plan de migración de SGRRHH desde Firebase (Firestore + Storage) hacia una arquitectura **100% local** usando **SQLite + Sistema de Archivos Local**, adoptando el estilo visual y arquitectónico de ForestechOil.

---

## 🏗️ Arquitectura Objetivo

### Stack Tecnológico Final
| Capa | Actual (Firebase) | Nuevo (Local) |
|------|-------------------|---------------|
| **Frontend** | Blazor WebAssembly | **Blazor Server** (Interactive Server) |
| **Base de Datos** | Firestore (NoSQL Cloud) | **SQLite** (archivo local) |
| **ORM/Acceso a Datos** | Firebase SDK + JS Interop | **Dapper** (micro-ORM) |
| **Almacenamiento** | Firebase Storage | **Sistema de Archivos Local** |
| **Autenticación** | Firebase Auth | **SQLite + BCrypt** (local) |
| **Hosting** | Firebase Hosting | **Aplicación Desktop/Servidor Local** |

### Estructura de Carpetas del Nuevo Proyecto
```
SGRRHH.Local/
├── SGRRHH.Local.Domain/          # Entidades y DTOs (basado en SGRRHH.Core)
│   ├── Entities/
│   ├── Enums/
│   ├── DTOs/
│   └── Exceptions/
│
├── SGRRHH.Local.Shared/          # Interfaces y contratos
│   ├── Interfaces/
│   ├── Configuration/
│   └── Result.cs
│
├── SGRRHH.Local.Infrastructure/  # Implementaciones SQLite + Archivos
│   ├── Data/
│   │   ├── DapperContext.cs
│   │   └── DatabasePathResolver.cs
│   ├── Repositories/
│   │   ├── EmpleadoRepository.cs
│   │   ├── PermisoRepository.cs
│   │   └── ... (1 por entidad)
│   └── Services/
│       ├── LocalAuthService.cs
│       ├── LocalStorageService.cs
│       ├── ReportService.cs (QuestPDF)
│       └── BackupService.cs
│
├── SGRRHH.Local.Server/          # Blazor Server App
│   ├── Components/
│   │   ├── Layout/
│   │   │   └── MainLayout.razor
│   │   ├── Pages/
│   │   │   ├── Dashboard.razor
│   │   │   ├── Empleados.razor
│   │   │   ├── Permisos.razor
│   │   │   └── ...
│   │   └── Shared/
│   │       ├── KeyboardHandler.razor
│   │       ├── FormModal.razor
│   │       └── DataTable.razor
│   ├── wwwroot/
│   │   ├── css/
│   │   │   └── hospital.css
│   │   ├── js/
│   │   │   └── keyboard-handler.js
│   │   └── logo-watermark.png
│   ├── Program.cs
│   └── appsettings.json
│
└── Data/                         # Carpeta de datos (fuera del código)
    ├── sgrrhh.db                # Base de datos SQLite
    ├── Fotos/
    │   └── Empleados/
    │       └── {empleadoId}/
    │           └── foto.jpg
    ├── Documentos/
    │   ├── Permisos/{permisoId}/
    │   ├── Contratos/{contratoId}/
    │   └── Generados/
    │       ├── Actas/
    │       └── Certificados/
    ├── Config/
    │   └── logo.png
    └── Backups/
        └── {yyyy-MM-dd}/
```

---

## 📊 Esquema de Base de Datos SQLite

### Tablas Principales (19 tablas)

```sql
-- ===== CATÁLOGOS =====
CREATE TABLE departamentos (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    codigo TEXT NOT NULL UNIQUE,
    nombre TEXT NOT NULL,
    descripcion TEXT,
    jefe_id INTEGER,
    activo INTEGER DEFAULT 1,
    created_at TEXT DEFAULT CURRENT_TIMESTAMP,
    updated_at TEXT DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE cargos (
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
    created_at TEXT,
    updated_at TEXT
);

CREATE TABLE tipos_permiso (
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
    created_at TEXT,
    updated_at TEXT
);

-- ===== USUARIOS Y AUTENTICACIÓN =====
CREATE TABLE usuarios (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    username TEXT NOT NULL UNIQUE,
    password_hash TEXT NOT NULL,
    nombre_completo TEXT NOT NULL,
    email TEXT,
    phone_number TEXT,
    rol INTEGER NOT NULL, -- 1=Admin, 2=Aprobador, 3=Operador
    ultimo_acceso TEXT,
    empleado_id INTEGER REFERENCES empleados(id),
    activo INTEGER DEFAULT 1,
    created_at TEXT,
    updated_at TEXT
);

-- ===== EMPLEADOS =====
CREATE TABLE empleados (
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
    estado INTEGER DEFAULT 1, -- 1=Activo, 2=Inactivo, 3=Pendiente
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
    creado_por_id INTEGER REFERENCES usuarios(id),
    fecha_solicitud TEXT,
    aprobado_por_id INTEGER REFERENCES usuarios(id),
    fecha_aprobacion TEXT,
    motivo_rechazo TEXT,
    activo INTEGER DEFAULT 1,
    created_at TEXT,
    updated_at TEXT
);

-- ===== CONTRATOS =====
CREATE TABLE contratos (
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
    created_at TEXT,
    updated_at TEXT
);

-- ===== PERMISOS =====
CREATE TABLE permisos (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    numero_acta TEXT NOT NULL UNIQUE,
    empleado_id INTEGER NOT NULL REFERENCES empleados(id),
    tipo_permiso_id INTEGER NOT NULL REFERENCES tipos_permiso(id),
    motivo TEXT NOT NULL,
    fecha_solicitud TEXT NOT NULL,
    fecha_inicio TEXT NOT NULL,
    fecha_fin TEXT NOT NULL,
    total_dias INTEGER NOT NULL,
    estado INTEGER DEFAULT 0, -- 0=Pendiente, 1=Aprobado, 2=Rechazado, 3=Cancelado
    observaciones TEXT,
    documento_soporte_path TEXT,
    dias_pendientes_compensacion INTEGER,
    fecha_compensacion TEXT,
    solicitado_por_id INTEGER NOT NULL REFERENCES usuarios(id),
    aprobado_por_id INTEGER REFERENCES usuarios(id),
    fecha_aprobacion TEXT,
    motivo_rechazo TEXT,
    activo INTEGER DEFAULT 1,
    created_at TEXT,
    updated_at TEXT
);

-- ===== VACACIONES =====
CREATE TABLE vacaciones (
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
    created_at TEXT,
    updated_at TEXT
);

-- ===== DOCUMENTOS DE EMPLEADOS =====
CREATE TABLE documentos_empleado (
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
    created_at TEXT,
    updated_at TEXT
);

-- ===== PROYECTOS Y ACTIVIDADES =====
CREATE TABLE proyectos (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    codigo TEXT NOT NULL UNIQUE,
    nombre TEXT NOT NULL,
    descripcion TEXT,
    fecha_inicio TEXT,
    fecha_fin TEXT,
    estado INTEGER DEFAULT 1,
    activo INTEGER DEFAULT 1,
    created_at TEXT,
    updated_at TEXT
);

CREATE TABLE proyectos_empleados (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    proyecto_id INTEGER NOT NULL REFERENCES proyectos(id),
    empleado_id INTEGER NOT NULL REFERENCES empleados(id),
    fecha_asignacion TEXT,
    rol TEXT,
    activo INTEGER DEFAULT 1,
    created_at TEXT,
    updated_at TEXT
);

CREATE TABLE actividades (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    nombre TEXT NOT NULL,
    descripcion TEXT,
    proyecto_id INTEGER REFERENCES proyectos(id),
    activo INTEGER DEFAULT 1,
    created_at TEXT,
    updated_at TEXT
);

-- ===== CONTROL DIARIO =====
CREATE TABLE registros_diarios (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    empleado_id INTEGER NOT NULL REFERENCES empleados(id),
    fecha TEXT NOT NULL,
    hora_inicio TEXT,
    hora_fin TEXT,
    observaciones TEXT,
    estado INTEGER DEFAULT 1,
    usuario_registro_id INTEGER REFERENCES usuarios(id),
    activo INTEGER DEFAULT 1,
    created_at TEXT,
    updated_at TEXT,
    UNIQUE(empleado_id, fecha)
);

CREATE TABLE detalles_actividad (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    registro_diario_id INTEGER NOT NULL REFERENCES registros_diarios(id),
    actividad_id INTEGER REFERENCES actividades(id),
    descripcion TEXT,
    horas REAL,
    activo INTEGER DEFAULT 1,
    created_at TEXT,
    updated_at TEXT
);

-- ===== AUDITORÍA =====
CREATE TABLE audit_logs (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    fecha_hora TEXT NOT NULL,
    usuario_id INTEGER REFERENCES usuarios(id),
    usuario_nombre TEXT,
    accion TEXT NOT NULL,
    entidad TEXT NOT NULL,
    entidad_id INTEGER,
    descripcion TEXT,
    direccion_ip TEXT,
    datos_adicionales TEXT,
    created_at TEXT DEFAULT CURRENT_TIMESTAMP
);

-- ===== CONFIGURACIÓN =====
CREATE TABLE configuracion_sistema (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    clave TEXT NOT NULL UNIQUE,
    valor TEXT,
    descripcion TEXT,
    updated_at TEXT
);

-- ===== ÍNDICES PARA PERFORMANCE =====
CREATE INDEX idx_empleados_cedula ON empleados(cedula);
CREATE INDEX idx_empleados_codigo ON empleados(codigo);
CREATE INDEX idx_empleados_estado ON empleados(estado);
CREATE INDEX idx_empleados_departamento ON empleados(departamento_id);
CREATE INDEX idx_permisos_empleado ON permisos(empleado_id);
CREATE INDEX idx_permisos_estado ON permisos(estado);
CREATE INDEX idx_permisos_fechas ON permisos(fecha_inicio, fecha_fin);
CREATE INDEX idx_vacaciones_empleado ON vacaciones(empleado_id);
CREATE INDEX idx_vacaciones_periodo ON vacaciones(periodo_correspondiente);
CREATE INDEX idx_contratos_empleado ON contratos(empleado_id);
CREATE INDEX idx_registros_fecha ON registros_diarios(fecha);
CREATE INDEX idx_registros_empleado_fecha ON registros_diarios(empleado_id, fecha);
CREATE INDEX idx_audit_fecha ON audit_logs(fecha_hora);
CREATE INDEX idx_audit_entidad ON audit_logs(entidad, entidad_id);
```

---

## 🎯 Fases de Migración

### Fase 0: Preparación y Estructura Base
- Crear nuevo proyecto SGRRHH.Local (Blazor Server)
- Copiar entidades y enums de SGRRHH.Core
- Configurar estructura de carpetas
- **Duración estimada:** 2-3 horas

### Fase 1: Capa de Infraestructura SQLite
- Implementar DapperContext y DatabasePathResolver
- Crear script de inicialización de base de datos
- Implementar todos los repositorios (16+)
- **Duración estimada:** 6-8 horas

### Fase 2: Sistema de Archivos Local
- Implementar LocalStorageService (reemplazo de FirebaseStorageService)
- Gestión de fotos de empleados
- Gestión de documentos de permisos/contratos
- Sistema de backup automático
- **Duración estimada:** 3-4 horas

### Fase 3: Autenticación Local
- Implementar LocalAuthService con BCrypt
- Migrar sistema de roles y permisos
- Sesiones locales (sin tokens JWT)
- **Duración estimada:** 2-3 horas

### Fase 4: UI Premium (Estilo ForestechOil)
- Copiar hospital.css y keyboard-handler.js
- Implementar MainLayout estilo "hospital"
- Crear componente KeyboardHandler
- Implementar barra de atajos
- **Duración estimada:** 4-5 horas

### Fase 5: Migración de Páginas
- Dashboard con estadísticas
- Empleados (CRUD completo)
- Permisos con flujo de aprobación
- Vacaciones con cálculo de días
- Contratos
- Control Diario
- Catálogos (Departamentos, Cargos, TiposPermiso)
- Usuarios
- **Duración estimada:** 10-12 horas

### Fase 6: Reportes con QuestPDF
- Integrar QuestPDF
- Plantillas de documentos:
  - Acta de permiso
  - Certificado laboral
  - Reporte de vacaciones
  - Listado de empleados
- **Duración estimada:** 4-5 horas

### Fase 7: Funcionalidades Avanzadas
- Sistema de backup/restore
- Exportación a Excel
- Configuración del sistema
- Auditoría de acciones
- **Duración estimada:** 3-4 horas

### Fase 8: Testing y Pulido
- Pruebas de integración
- Optimización de performance
- Documentación de usuario
- **Duración estimada:** 2-3 horas

---

## 📁 Archivos de Prompts por Fase

Cada fase tiene su archivo markdown con el prompt detallado:

| Archivo | Fase | Descripción |
|---------|------|-------------|
| `01_FASE_PREPARACION.md` | 0 | Crear estructura base del proyecto |
| `02_FASE_INFRAESTRUCTURA.md` | 1 | Implementar SQLite con Dapper |
| `03_FASE_ARCHIVOS_LOCALES.md` | 2 | Sistema de almacenamiento local |
| `04_FASE_AUTENTICACION.md` | 3 | Login local con BCrypt |
| `05_FASE_UI_PREMIUM.md` | 4 | Estilo ForestechOil |
| `06_FASE_PAGINAS.md` | 5 | Migrar todas las páginas |
| `07_FASE_REPORTES.md` | 6 | QuestPDF para PDFs |
| `08_FASE_AVANZADO.md` | 7 | Backups, exportación, auditoría |
| `09_FASE_TESTING.md` | 8 | Testing y documentación |

---

## ⏱️ Estimación Total

| Concepto | Tiempo |
|----------|--------|
| Desarrollo total | 36-47 horas |
| Buffer (20%) | 7-9 horas |
| **Total estimado** | **43-56 horas** |

---

## 🎨 Características UI Premium (ForestechOil Style)

1. **Tipografía Monoespaciada** - Courier New para look "sistema"
2. **Paleta de Colores**
   - Fondo: #FFFFFF (blanco puro)
   - Headers: #E0E0E0 (gris claro)
   - Bordes: #808080 (gris medio)
   - Texto: #000000 (negro)
   - Error: #FF0000 / #FFCCCC
   - Éxito: #00AA00 / #CCFFCC
3. **Sin Animaciones** - Performance máxima
4. **Botones Rectangulares** - Sin bordes redondeados
5. **Barra de Atajos de Teclado** - Fija en la parte inferior
6. **Navegación por Teclado** - F1-F12, Escape, Enter
7. **Marca de Agua** - Logo semi-transparente centrado

---

## ✅ Criterios de Éxito

- [ ] Aplicación 100% funcional sin conexión a internet
- [ ] Todas las operaciones CRUD funcionando
- [ ] Sistema de archivos local para fotos/documentos
- [ ] Login local con roles de usuario
- [ ] Generación de PDFs con QuestPDF
- [ ] Navegación por teclado completa
- [ ] Estética idéntica a ForestechOil
- [ ] Performance instantánea (SQLite local)
- [ ] Sistema de backup automático

---

## 📝 Notas Importantes

1. **No migrar datos existentes** - Esta es una app nueva desde cero
2. **Priorizar funcionalidad sobre perfección** - Iterar después
3. **Mantener compatibilidad de entidades** - Mismo modelo conceptual
4. **Documentar cambios de API** - Para futuras referencias
