# PROMPT: Estandarización de Nomenclatura - SGRRHH

## 📋 Contexto del Problema

El sistema tiene **inconsistencias críticas de nomenclatura** que causan errores al desplegar a servidores nuevos.

### Decisión: Migración Completa a Inglés

**Se migrará TODO el sistema a inglés + snake_case** antes de que la aplicación escale.

---

## 🎯 Objetivo

Estandarizar **TODO** el sistema a una única convención:

### Convención Elegida: `snake_case` + Inglés

| Elemento | Convención | Ejemplo |
|----------|------------|---------|
| Nombres de tablas | snake_case, plural, inglés | `employees`, `leave_types`, `activity_categories` |
| Nombres de columnas | snake_case, inglés | `created_at`, `category_id`, `is_active` |
| Propiedades C# | PascalCase, inglés | `CreatedAt`, `CategoryId`, `IsActive` |
| Claves foráneas | `<entity>_id` | `employee_id`, `category_id` |

---

## 📊 Mapeo Completo de Tablas

### Catálogos Base
| Tabla Actual | Tabla Nueva | Descripción |
|--------------|-------------|-------------|
| `departamentos` | `departments` | Departamentos de la empresa |
| `cargos` | `positions` | Cargos/puestos de trabajo |
| `tipos_permiso` | `leave_types` | Tipos de permisos laborales |

### Empleados y Usuarios
| Tabla Actual | Tabla Nueva | Descripción |
|--------------|-------------|-------------|
| `empleados` | `employees` | Empleados de la empresa |
| `usuarios` | `users` | Usuarios del sistema |
| `contratos` | `contracts` | Contratos laborales |
| `documentos_empleado` | `employee_documents` | Documentos adjuntos |

### Permisos y Vacaciones
| Tabla Actual | Tabla Nueva | Descripción |
|--------------|-------------|-------------|
| `permisos` | `leaves` | Solicitudes de permisos |
| `vacaciones` | `vacations` | Vacaciones |
| `incapacidades` | `disabilities` | Incapacidades médicas |
| `seguimiento_incapacidades` | `disability_tracking` | Seguimiento de incapacidades |
| `seguimiento_permisos` | `leave_tracking` | Seguimiento de permisos |
| `compensaciones_horas` | `hour_compensations` | Compensación de horas |

### Proyectos y Actividades
| Tabla Actual | Tabla Nueva | Descripción |
|--------------|-------------|-------------|
| `proyectos` | `projects` | Proyectos forestales |
| `proyectos_empleados` | `project_employees` | Asignación empleados-proyectos |
| `actividades` | `activities` | Catálogo de actividades |
| `CategoriasActividades` | `activity_categories` | Categorías de actividades |
| `registros_diarios` | `daily_records` | Registro diario de trabajo |
| `detalles_actividad` | `activity_details` | Detalles de actividades |

### Nómina y Legal
| Tabla Actual | Tabla Nueva | Descripción |
|--------------|-------------|-------------|
| `nominas` | `payroll` | Nóminas |
| `prestaciones` | `benefits` | Prestaciones sociales |
| `configuracion_legal` | `legal_config` | Configuración legal Colombia |
| `festivos_colombia` | `colombian_holidays` | Festivos nacionales |

### Catálogos Seguridad Social
| Tabla Actual | Tabla Nueva | Descripción |
|--------------|-------------|-------------|
| `eps_colombia` | `health_providers` | EPS |
| `afp_colombia` | `pension_funds` | Fondos de pensiones |
| `arl_colombia` | `work_risk_insurers` | ARL |
| `cajas_compensacion` | `compensation_funds` | Cajas de compensación |

### Silvicultura
| Tabla Actual | Tabla Nueva | Descripción |
|--------------|-------------|-------------|
| `especies_forestales` | `forest_species` | Especies forestales |

### Sistema
| Tabla Actual | Tabla Nueva | Descripción |
|--------------|-------------|-------------|
| `configuracion_sistema` | `system_config` | Configuración del sistema |
| `audit_logs` | `audit_logs` | Logs de auditoría (ya en inglés) |
| `Notificaciones` | `notifications` | Notificaciones |
| `PreferenciasNotificacion` | `notification_preferences` | Preferencias |
| `ScanProfiles` | `scan_profiles` | Perfiles de escaneo |

---

## 📊 Inventario de Cambios Necesarios

### FASE 1: Tabla CategoriasActividades → activity_categories

**Tabla actual (PascalCase):**
```sql
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
```

**Tabla nueva (snake_case):**
```sql
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
```

**Entidad C# a actualizar:** `CategoriaActividad.cs`
```csharp
public class ActivityCategory : BaseEntity  // Renombrar o mantener español
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Icon { get; set; }
    public string? ColorHex { get; set; }
    public int DisplayOrder { get; set; }
}
```

---

### FASE 2: Tabla Actividades - Columnas Nuevas

**Columnas actuales con inconsistencia:**
```sql
-- DapperContext.cs usa:
categoria_id, unidad_medida, unidad_abreviatura, rendimiento_esperado...

-- migration_actividades_silviculturales_v1.sql usa:
CategoriaId, UnidadMedida, UnidadAbreviatura, RendimientoEsperado...
```

**Script de migración para renombrar columnas:**
```sql
-- SQLite no permite renombrar columnas directamente
-- Hay que recrear la tabla

-- 1. Crear tabla temporal con nombres correctos
CREATE TABLE actividades_new (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    code TEXT NOT NULL UNIQUE,  -- antes: codigo
    name TEXT NOT NULL,  -- antes: nombre
    description TEXT,  -- antes: descripcion
    category_legacy TEXT,  -- antes: categoria (para compatibilidad)
    category_id INTEGER REFERENCES activity_categories(id),  -- ESTANDARIZADO
    category_text TEXT,  -- antes: CategoriaTexto
    unit_of_measure INTEGER DEFAULT 0,  -- antes: UnidadMedida
    unit_abbreviation TEXT,  -- antes: UnidadAbreviatura
    expected_yield REAL,  -- antes: RendimientoEsperado
    minimum_yield REAL,  -- antes: RendimientoMinimo
    unit_cost REAL,  -- antes: CostoUnitario
    requires_project INTEGER DEFAULT 0,  -- antes: requiere_proyecto
    requires_quantity INTEGER DEFAULT 0,  -- antes: RequiereCantidad
    applicable_project_types TEXT,  -- antes: TiposProyectoAplicables
    applicable_species TEXT,  -- antes: EspeciesAplicables
    display_order INTEGER DEFAULT 0,  -- antes: orden
    is_featured INTEGER DEFAULT 0,  -- antes: EsDestacada
    is_active INTEGER DEFAULT 1,  -- antes: activo
    created_at TEXT DEFAULT CURRENT_TIMESTAMP,  -- antes: fecha_creacion
    updated_at TEXT  -- antes: fecha_modificacion
);

-- 2. Copiar datos
INSERT INTO actividades_new SELECT 
    id, codigo, nombre, descripcion, categoria,
    COALESCE(CategoriaId, categoria_id), 
    COALESCE(CategoriaTexto, categoria_texto),
    COALESCE(UnidadMedida, unidad_medida, 0),
    COALESCE(UnidadAbreviatura, unidad_abreviatura),
    COALESCE(RendimientoEsperado, rendimiento_esperado),
    COALESCE(RendimientoMinimo, rendimiento_minimo),
    COALESCE(CostoUnitario, costo_unitario),
    requiere_proyecto,
    COALESCE(RequiereCantidad, requiere_cantidad, 0),
    COALESCE(TiposProyectoAplicables, tipos_proyecto_aplicables),
    COALESCE(EspeciesAplicables, especies_aplicables),
    orden,
    COALESCE(EsDestacada, es_destacada, 0),
    activo,
    fecha_creacion,
    fecha_modificacion
FROM actividades;

-- 3. Eliminar tabla vieja y renombrar
DROP TABLE actividades;
ALTER TABLE actividades_new RENAME TO actividades;

-- 4. Recrear índices
CREATE INDEX IF NOT EXISTS idx_actividades_category_id ON actividades(category_id);
CREATE INDEX IF NOT EXISTS idx_actividades_code ON actividades(code);
CREATE INDEX IF NOT EXISTS idx_actividades_is_active ON actividades(is_active);
```

---

### FASE 3: Tabla Notificaciones → notifications

**Tabla actual:**
```sql
CREATE TABLE IF NOT EXISTS Notificaciones (
    Id, UsuarioDestinoId, Titulo, Mensaje, Icono, Tipo, Categoria...
);
```

**Tabla nueva:**
```sql
CREATE TABLE IF NOT EXISTS notifications (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    target_user_id INTEGER NULL,
    title TEXT NOT NULL,
    message TEXT NOT NULL,
    icon TEXT DEFAULT '📌',
    type TEXT NOT NULL DEFAULT 'Info',
    category TEXT NOT NULL DEFAULT 'Sistema',
    priority INTEGER DEFAULT 0,
    link TEXT NULL,
    entity_type TEXT NULL,
    entity_id INTEGER NULL,
    is_read INTEGER DEFAULT 0,
    read_at TEXT NULL,
    created_at TEXT NOT NULL DEFAULT (datetime('now', 'localtime')),
    created_by TEXT NULL,
    expires_at TEXT NULL,
    FOREIGN KEY (target_user_id) REFERENCES usuarios(id) ON DELETE CASCADE
);
```

---

### FASE 4: Tabla ScanProfiles → scan_profiles

**Tabla actual:**
```sql
CREATE TABLE IF NOT EXISTS ScanProfiles (
    Id, Name, Description, IsDefault, Dpi, ColorMode...
);
```

**Tabla nueva:**
```sql
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
```

---

### FASE 5: Tabla PreferenciasNotificacion → notification_preferences

```sql
CREATE TABLE IF NOT EXISTS notification_preferences (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    user_id INTEGER NOT NULL UNIQUE,
    receive_permissions INTEGER DEFAULT 1,
    receive_vacations INTEGER DEFAULT 1,
    receive_disabilities INTEGER DEFAULT 1,
    receive_system INTEGER DEFAULT 1,
    sound_enabled INTEGER DEFAULT 1,
    show_desktop INTEGER DEFAULT 0,
    email_summary TEXT DEFAULT 'Never',
    FOREIGN KEY (user_id) REFERENCES usuarios(id) ON DELETE CASCADE
);
```

---

## 📁 Archivos a Modificar

### 1. Infrastructure/Data/DatabaseInitializer.cs
- Actualizar `Schema` con todas las tablas en snake_case
- Actualizar `SeedData` con nombres de columnas correctos
- Actualizar índices

### 2. Infrastructure/Data/DapperContext.cs
- Actualizar migraciones automáticas `TryAddColumnAsync` para usar snake_case
- Eliminar duplicados (no agregar `categoria_id` si ya existe `category_id`)

### 3. Scripts SQL de Migración
Archivos a reescribir completamente:
- `scripts/migration_actividades_silviculturales_v1.sql`
- `scripts/migration_notificaciones_v1.sql`
- `scripts/migration_scan_profiles_v1.sql`

### 4. Repositorios (actualizar queries SQL)
- `ActividadRepository.cs` - Actualizar todas las queries
- `CategoriaActividadRepository.cs` - Usar `activity_categories`
- `NotificacionRepository.cs` - Usar `notifications`
- `ScanProfileRepository.cs` - Usar `scan_profiles`

### 5. Entidades Domain (opcional - solo documentación)
Las entidades C# pueden mantener PascalCase ya que Dapper mapea automáticamente.
Solo actualizar comentarios XML.

---

## 🔧 Script de Migración Completo

Crear archivo: `scripts/migration_standardize_nomenclature_v1.sql`

```sql
-- =====================================================
-- MIGRACIÓN: Estandarización de Nomenclatura
-- Versión: 1.0
-- Fecha: 2026-01-11
-- Descripción: Renombra tablas y columnas a snake_case
-- IMPORTANTE: Ejecutar después de backup de BD
-- =====================================================

-- Desactivar FK temporalmente
PRAGMA foreign_keys = OFF;

-- =====================================================
-- 1. MIGRAR CategoriasActividades → activity_categories
-- =====================================================

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

-- Migrar datos si existe tabla vieja
INSERT OR IGNORE INTO activity_categories (id, code, name, description, icon, color_hex, display_order, is_active, created_at, updated_at)
SELECT Id, Codigo, Nombre, Descripcion, Icono, ColorHex, Orden, Activo, FechaCreacion, FechaModificacion
FROM CategoriasActividades;

-- No eliminar tabla vieja hasta verificar migración completa
-- DROP TABLE IF EXISTS CategoriasActividades;

-- =====================================================
-- 2. MIGRAR Actividades (columnas mixtas)
-- =====================================================

-- Crear tabla temporal con nombres correctos
CREATE TABLE IF NOT EXISTS activities_temp (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    code TEXT NOT NULL UNIQUE,
    name TEXT NOT NULL,
    description TEXT,
    category_legacy TEXT,
    category_id INTEGER REFERENCES activity_categories(id),
    category_text TEXT,
    unit_of_measure INTEGER DEFAULT 0,
    unit_abbreviation TEXT,
    expected_yield REAL,
    minimum_yield REAL,
    unit_cost REAL,
    requires_project INTEGER DEFAULT 0,
    requires_quantity INTEGER DEFAULT 0,
    applicable_project_types TEXT,
    applicable_species TEXT,
    display_order INTEGER DEFAULT 0,
    is_featured INTEGER DEFAULT 0,
    is_active INTEGER DEFAULT 1,
    created_at TEXT DEFAULT CURRENT_TIMESTAMP,
    updated_at TEXT
);

-- Copiar datos de actividades (manejando ambas nomenclaturas)
INSERT INTO activities_temp 
SELECT 
    id,
    codigo,
    nombre,
    descripcion,
    categoria,
    COALESCE(CategoriaId, NULL),
    COALESCE(CategoriaTexto, categoria_texto, NULL),
    COALESCE(UnidadMedida, unidad_medida, 0),
    COALESCE(UnidadAbreviatura, unidad_abreviatura, NULL),
    COALESCE(RendimientoEsperado, rendimiento_esperado, NULL),
    COALESCE(RendimientoMinimo, rendimiento_minimo, NULL),
    COALESCE(CostoUnitario, costo_unitario, NULL),
    requiere_proyecto,
    COALESCE(RequiereCantidad, requiere_cantidad, 0),
    COALESCE(TiposProyectoAplicables, tipos_proyecto_aplicables, NULL),
    COALESCE(EspeciesAplicables, especies_aplicables, NULL),
    orden,
    COALESCE(EsDestacada, es_destacada, 0),
    activo,
    fecha_creacion,
    fecha_modificacion
FROM actividades;

-- Renombrar tablas
DROP TABLE IF EXISTS actividades;
ALTER TABLE activities_temp RENAME TO actividades;

-- Crear índices
CREATE INDEX IF NOT EXISTS idx_actividades_category_id ON actividades(category_id);
CREATE INDEX IF NOT EXISTS idx_actividades_code ON actividades(code);
CREATE INDEX IF NOT EXISTS idx_actividades_is_active ON actividades(is_active);
CREATE INDEX IF NOT EXISTS idx_actividades_is_featured ON actividades(is_featured);

-- =====================================================
-- 3. MIGRAR Notificaciones → notifications
-- =====================================================

CREATE TABLE IF NOT EXISTS notifications (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    target_user_id INTEGER NULL,
    title TEXT NOT NULL,
    message TEXT NOT NULL,
    icon TEXT DEFAULT '📌',
    type TEXT NOT NULL DEFAULT 'Info',
    category TEXT NOT NULL DEFAULT 'Sistema',
    priority INTEGER DEFAULT 0,
    link TEXT NULL,
    entity_type TEXT NULL,
    entity_id INTEGER NULL,
    is_read INTEGER DEFAULT 0,
    read_at TEXT NULL,
    created_at TEXT NOT NULL DEFAULT (datetime('now', 'localtime')),
    created_by TEXT NULL,
    expires_at TEXT NULL,
    FOREIGN KEY (target_user_id) REFERENCES usuarios(id) ON DELETE CASCADE
);

-- Migrar datos si existe tabla vieja
INSERT OR IGNORE INTO notifications 
SELECT Id, UsuarioDestinoId, Titulo, Mensaje, Icono, Tipo, Categoria, Prioridad, Link, EntidadTipo, EntidadId, Leida, FechaLectura, FechaCreacion, CreadoPor, FechaExpiracion
FROM Notificaciones;

-- Crear índices
CREATE INDEX IF NOT EXISTS idx_notifications_target_user ON notifications(target_user_id);
CREATE INDEX IF NOT EXISTS idx_notifications_is_read ON notifications(is_read);
CREATE INDEX IF NOT EXISTS idx_notifications_type ON notifications(type);
CREATE INDEX IF NOT EXISTS idx_notifications_created_at ON notifications(created_at DESC);

-- =====================================================
-- 4. MIGRAR ScanProfiles → scan_profiles
-- =====================================================

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

-- Migrar datos
INSERT OR IGNORE INTO scan_profiles
SELECT Id, Name, Description, IsDefault, Dpi, ColorMode, Source, PageSize, Brightness, Contrast, Gamma, Sharpness, BlackWhiteThreshold, AutoDeskew, AutoCrop, CreatedAt, LastUsedAt
FROM ScanProfiles;

CREATE INDEX IF NOT EXISTS idx_scan_profiles_name ON scan_profiles(name);
CREATE INDEX IF NOT EXISTS idx_scan_profiles_default ON scan_profiles(is_default);

-- =====================================================
-- 5. MIGRAR PreferenciasNotificacion → notification_preferences
-- =====================================================

CREATE TABLE IF NOT EXISTS notification_preferences (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    user_id INTEGER NOT NULL UNIQUE,
    receive_permissions INTEGER DEFAULT 1,
    receive_vacations INTEGER DEFAULT 1,
    receive_disabilities INTEGER DEFAULT 1,
    receive_system INTEGER DEFAULT 1,
    sound_enabled INTEGER DEFAULT 1,
    show_desktop INTEGER DEFAULT 0,
    email_summary TEXT DEFAULT 'Never',
    FOREIGN KEY (user_id) REFERENCES usuarios(id) ON DELETE CASCADE
);

INSERT OR IGNORE INTO notification_preferences
SELECT Id, UsuarioId, RecibirPermisos, RecibirVacaciones, RecibirIncapacidades, RecibirSistema, SonidoHabilitado, MostrarEnEscritorio, ResumenEmail
FROM PreferenciasNotificacion;

-- Reactivar FK
PRAGMA foreign_keys = ON;

-- =====================================================
-- VERIFICACIÓN
-- =====================================================
SELECT 'activity_categories: ' || COUNT(*) FROM activity_categories;
SELECT 'actividades: ' || COUNT(*) FROM actividades;
SELECT 'notifications: ' || COUNT(*) FROM notifications;
SELECT 'scan_profiles: ' || COUNT(*) FROM scan_profiles;
```

---

## 📝 Datos Seed Actualizados

### CategoriasActividades → activity_categories

```sql
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
```

### Actividades con categorías

```sql
-- PREPARACIÓN DE TERRENO
INSERT OR IGNORE INTO actividades (code, name, description, category_id, category_text, unit_of_measure, unit_abbreviation, expected_yield, requires_project, requires_quantity, is_featured, display_order, is_active, created_at) 
SELECT 'PREP-001', 'Limpieza de Terreno', 'Rocería y limpieza general del lote', id, 'Preparación de Terreno', 1, 'ha', 0.15, 1, 1, 1, 101, 1, datetime('now', 'localtime') FROM activity_categories WHERE code = 'PREP';

INSERT OR IGNORE INTO actividades (code, name, description, category_id, category_text, unit_of_measure, unit_abbreviation, expected_yield, requires_project, requires_quantity, is_featured, display_order, is_active, created_at) 
SELECT 'PREP-002', 'Trazado y Estacado', 'Marcación de puntos de siembra según diseño', id, 'Preparación de Terreno', 2, 'árb', 200, 1, 1, 1, 102, 1, datetime('now', 'localtime') FROM activity_categories WHERE code = 'PREP';

-- ... (continuar con todas las actividades)
```

### ScanProfiles → scan_profiles

```sql
INSERT OR IGNORE INTO scan_profiles (id, name, description, is_default, dpi, color_mode, source, page_size) VALUES 
(1, 'Documento Rápido', 'Escaneo rápido de documentos en escala de grises', 0, 150, 'Grayscale', 'Flatbed', 'Letter'),
(2, 'Documento Alta Calidad', 'Escaneo de alta calidad para archivo permanente', 1, 300, 'Color', 'Flatbed', 'Letter'),
(3, 'Cédula/ID', 'Optimizado para documentos de identidad', 0, 300, 'Color', 'Flatbed', 'A5'),
(4, 'Foto', 'Escaneo de fotografías en alta resolución', 0, 600, 'Color', 'Flatbed', 'Letter'),
(5, 'Blanco y Negro', 'Documentos de texto para OCR', 0, 300, 'BlackWhite', 'Flatbed', 'Letter');
```

---

## ✅ Orden de Implementación

### Paso 1: Backup
```powershell
# Crear backup antes de cualquier cambio
Copy-Item "C:\SGRRHH\Data\sgrrhh.db" "C:\SGRRHH\Data\Backups\sgrrhh_pre_nomenclature_$(Get-Date -Format 'yyyyMMdd_HHmmss').db"
```

### Paso 2: Actualizar DatabaseInitializer.cs
- Reescribir `Schema` completo con snake_case
- Reescribir `SeedData` con nuevos nombres

### Paso 3: Actualizar DapperContext.cs
- Eliminar migraciones duplicadas
- Usar solo snake_case

### Paso 4: Crear script de migración
- `migration_standardize_nomenclature_v1.sql`
- Ejecutar en BD existentes

### Paso 5: Actualizar Repositorios
En orden de dependencias:
1. `CategoriaActividadRepository.cs` → usar `activity_categories`
2. `ActividadRepository.cs` → usar nuevas columnas
3. `NotificacionRepository.cs` → usar `notifications`
4. `ScanProfileRepository.cs` → usar `scan_profiles`

### Paso 6: Actualizar Entidades (opcional)
- Agregar atributos `[Column]` si es necesario
- O confiar en `MatchNamesWithUnderscores = true`

### Paso 7: Probar
```powershell
dotnet build
dotnet run --project SGRRHH.Local.Server
```

### Paso 8: Eliminar tablas viejas (después de verificar)
```sql
DROP TABLE IF EXISTS CategoriasActividades;
DROP TABLE IF EXISTS Notificaciones;
DROP TABLE IF EXISTS ScanProfiles;
DROP TABLE IF EXISTS PreferenciasNotificacion;
```

---

## 🚨 Notas Importantes

1. **Dapper MatchNamesWithUnderscores**: Ya está configurado en `DapperContext.cs`:
   ```csharp
   static DapperContext()
   {
       DefaultTypeMap.MatchNamesWithUnderscores = true;
   }
   ```
   Esto permite que `category_id` en BD mapee a `CategoryId` en C#.

2. **No renombrar tablas existentes en español**: Las tablas como `empleados`, `permisos`, `vacaciones` se mantienen para no romper datos existentes.

3. **Nuevas tablas en inglés**: A partir de ahora, nuevas tablas y columnas en inglés con snake_case.

4. **Compatibilidad hacia atrás**: El script de migración usa `COALESCE` para manejar ambos nombres de columna durante la transición.

---

## 📋 Checklist de Implementación

- [ ] Crear backup de BD
- [ ] Actualizar `DatabaseInitializer.cs` - Schema
- [ ] Actualizar `DatabaseInitializer.cs` - SeedData
- [ ] Actualizar `DapperContext.cs` - Migraciones
- [ ] Crear `migration_standardize_nomenclature_v1.sql`
- [ ] Actualizar `CategoriaActividadRepository.cs`
- [ ] Actualizar `ActividadRepository.cs`
- [ ] Actualizar `NotificacionRepository.cs`
- [ ] Actualizar `ScanProfileRepository.cs`
- [ ] Eliminar scripts de migración obsoletos
- [ ] Compilar y probar localmente
- [ ] Ejecutar migración en servidor de prueba
- [ ] Desplegar a producción

---

*Generado: 11 de enero de 2026*
*Contexto: Estandarización para resolver errores de despliegue SSH*
