# 📊 Flujo de la Aplicación SGRRHH y Sistema de Actualizaciones

## 🎯 Resumen Ejecutivo

SGRRHH es una aplicación de escritorio WPF (.NET 8) para gestión de recursos humanos diseñada para funcionar en red local con **3 usuarios**:

| PC | Usuario | Rol | Función Principal |
|----|---------|-----|-------------------|
| **Servidor** (Tu PC) | `admin` | Administrador | Configuración, backups, supervisión total |
| **PC Ingeniera** | `ingeniera` | Aprobador | Aprobar/rechazar permisos y solicitudes |
| **PC Secretaria** | `secretaria` | Operador | Registrar empleados, control diario, solicitar permisos |

---

## 🔄 Arquitectura de Red

```
┌─────────────────────────────────────────────────────────────────────┐
│                      RED LOCAL - WiFi/Ethernet                       │
├─────────────────────────────────────────────────────────────────────┤
│                                                                      │
│  ┌──────────────────┐  ┌──────────────────┐  ┌──────────────────┐  │
│  │   PC SERVIDOR    │  │   PC INGENIERA   │  │  PC SECRETARIA   │  │
│  │   (ELITEBOOK)    │  │                  │  │                  │  │
│  │                  │  │                  │  │                  │  │
│  │  ┌────────────┐  │  │  ┌────────────┐  │  │  ┌────────────┐  │  │
│  │  │ SGRRHH.exe │  │  │  │ SGRRHH.exe │  │  │  │ SGRRHH.exe │  │  │
│  │  │ (Admin)    │  │  │  │ (Aprobador)│  │  │  │ (Operador) │  │  │
│  │  └────────────┘  │  │  └────────────┘  │  │  └────────────┘  │  │
│  │                  │  │         │        │  │         │        │  │
│  └────────┬─────────┘  └─────────┼────────┘  └─────────┼────────┘  │
│           │                      │                     │           │
│           │    ┌─────────────────┴─────────────────────┘           │
│           │    │                                                    │
│           ▼    ▼                                                    │
│  ┌─────────────────────────────────────────────────────────────┐   │
│  │              C:\SGRRHH_Data (Carpeta Compartida)            │   │
│  │              \\ELITEBOOK-EVERT\SGRRHH                        │   │
│  │                                                              │   │
│  │  ├── sgrrhh.db          ← Base de datos SQLite (WAL mode)   │   │
│  │  ├── sgrrhh.db-wal      ← Archivo WAL (auto-generado)       │   │
│  │  ├── sgrrhh.db-shm      ← Memoria compartida SQLite         │   │
│  │  ├── fotos/             ← Fotos de empleados                │   │
│  │  ├── documentos/        ← Documentos de permisos            │   │
│  │  ├── backups/           ← Copias de seguridad               │   │
│  │  ├── config/            ← Configuración (logo, etc.)        │   │
│  │  ├── logs/              ← Logs de errores                   │   │
│  │  └── updates/           ← 🆕 ACTUALIZACIONES (nuevo)        │   │
│  │      ├── latest/        ← Última versión disponible         │   │
│  │      ├── version.json   ← Info de versión actual            │   │
│  │      └── history/       ← Historial de versiones            │   │
│  └─────────────────────────────────────────────────────────────┘   │
│                                                                      │
└─────────────────────────────────────────────────────────────────────┘
```

---

## 📁 Flujo de Inicio de la Aplicación

```
┌─────────────────────────────────────────────────────────────────────┐
│                    FLUJO DE INICIO - SGRRHH.exe                     │
└─────────────────────────────────────────────────────────────────────┘

    Usuario ejecuta SGRRHH.exe
              │
              ▼
    ┌─────────────────────┐
    │ App.OnStartup()     │
    │                     │
    │ 1. Configurar       │
    │    excepciones      │
    │    globales         │
    └──────────┬──────────┘
               │
               ▼
    ┌─────────────────────┐
    │ ConfigureServices() │
    │                     │
    │ - Lee appsettings   │
    │   .json             │
    │ - Configura         │
    │   DbContext SQLite  │
    │ - Registra repos    │
    │ - Registra services │
    │ - Registra VMs      │
    └──────────┬──────────┘
               │
               ▼
    ┌─────────────────────┐
    │ InitializeDatabase()│
    │                     │
    │ - Crea BD si no     │
    │   existe            │
    │ - Ejecuta           │
    │   migraciones       │
    │ - Configura WAL     │
    │   mode para red     │
    └──────────┬──────────┘
               │
               ▼
    ┌─────────────────────┐        ┌─────────────────────┐
    │   LoginWindow       │        │   Validación        │
    │                     │──────► │                     │
    │ - Usuario           │        │ - Verifica usuario  │
    │ - Contraseña        │        │ - Verifica password │
    │                     │        │   (BCrypt hash)     │
    └──────────┬──────────┘        │ - Verifica activo   │
               │                   └─────────────────────┘
               │ Login exitoso
               ▼
    ┌─────────────────────┐
    │   MainWindow        │
    │                     │
    │ - Menú lateral      │
    │   (según rol)       │
    │ - Dashboard inicial │
    │ - Navegación        │
    │   ContentFrame      │
    └─────────────────────┘
```

---

## 👤 Flujo por Rol de Usuario

### 🔷 Administrador (Tu PC - Servidor)

```
Dashboard
    │
    ├── 👥 Empleados ────────► CRUD completo + fotos
    │
    ├── 📅 Control Diario ──► Registrar horas + actividades
    │
    ├── 📝 Permisos ────────► Crear + Aprobar solicitudes
    │       └── Bandeja de Aprobación
    │
    ├── 🏖️ Vacaciones ──────► Gestionar vacaciones
    │
    ├── 📄 Contratos ───────► Gestionar contratos
    │
    ├── 📁 Catálogos ───────► Departamentos, Cargos, 
    │       │                  Actividades, Proyectos,
    │       │                  Tipos de Permiso
    │       └──────────────► CRUD en cada catálogo
    │
    ├── 📈 Reportes ────────► Todos los reportes
    │
    ├── 📄 Documentos ──────► Certificados, constancias
    │
    ├── 👤 Usuarios ────────► Crear/editar usuarios
    │
    ├── 📋 Auditoría ───────► Ver log de cambios
    │
    └── ⚙️ Configuración
            ├── Empresa (logo, datos)
            └── Backup/Restore
```

### 🔶 Aprobador (PC Ingeniera)

```
Dashboard
    │
    ├── 📬 Bandeja Aprobación ──► Ver permisos pendientes
    │       │                     Aprobar/Rechazar
    │       └──────────────────► Agregar observaciones
    │
    ├── 👥 Empleados ──────────► Solo VER (sin editar)
    │
    ├── 📅 Control Diario ─────► Solo VER
    │
    ├── 📝 Permisos ───────────► Ver todos los permisos
    │
    ├── 🏖️ Vacaciones ─────────► Ver + Aprobar
    │
    ├── 📁 Catálogos ──────────► Solo VER
    │
    ├── 📈 Reportes ───────────► Generar reportes
    │
    └── 📄 Documentos ─────────► Generar documentos
```

### 🔸 Operador (PC Secretaria)

```
Dashboard
    │
    ├── 👥 Empleados ──────────► CRUD completo
    │       │                    Crear/Editar empleados
    │       └──────────────────► Subir fotos
    │
    ├── 📅 Control Diario ─────► CRUD completo
    │       │                    Registrar entradas/salidas
    │       └──────────────────► Registrar actividades
    │
    ├── 📝 Permisos
    │       ├── Crear solicitudes ──► Nueva solicitud
    │       │                          Adjuntar documentos
    │       └── Ver mis solicitudes ─► Ver estado (Pendiente/
    │                                   Aprobado/Rechazado)
    │       ⚠️ NO puede aprobar
    │
    ├── 🏖️ Vacaciones ─────────► Solo VER
    │
    ├── 📁 Catálogos ──────────► Solo VER
    │
    └── 📈 Reportes ───────────► Reportes básicos
```

---

## 🔄 Flujo de un Permiso (Ejemplo Completo)

```
    ┌──────────────────┐
    │   PC SECRETARIA  │
    │                  │
    │  1. Crea permiso │
    │     - Empleado   │
    │     - Tipo       │
    │     - Fechas     │
    │     - Motivo     │
    │     - Documento  │
    │                  │
    │  Estado:         │
    │  🟡 PENDIENTE    │
    └────────┬─────────┘
             │
             │ Guarda en BD compartida
             │ (\\SERVIDOR\SGRRHH\sgrrhh.db)
             ▼
    ┌──────────────────┐
    │   PC INGENIERA   │
    │                  │
    │  2. Ve notificac.│
    │     en Bandeja   │
    │                  │
    │  3. Revisa       │
    │     solicitud    │
    │                  │
    │  4. Decide:      │
    │     ┌───┴───┐    │
    │     ▼       ▼    │
    │  APROBAR  RECHAZAR
    │     │       │    │
    │  🟢       🔴     │
    └────┬───────┬─────┘
         │       │
         │ Actualiza BD
         ▼       ▼
    ┌──────────────────┐
    │  TODAS LAS PCs   │
    │                  │
    │  5. Ven el       │
    │     estado       │
    │     actualizado  │
    │                  │
    │  6. Admin puede  │
    │     generar Acta │
    │     PDF          │
    └──────────────────┘
```

---

## 🚀 Sistema de Actualizaciones Automáticas (NUEVO)

### Concepto

El servidor (tu PC) publica nuevas versiones en la carpeta compartida. Las demás PCs detectan automáticamente cuando hay una actualización disponible y la instalan.

### Arquitectura de Actualizaciones

```
┌─────────────────────────────────────────────────────────────────────┐
│                    SISTEMA DE ACTUALIZACIONES                        │
└─────────────────────────────────────────────────────────────────────┘

    PC SERVIDOR (Tú)
    ┌─────────────────────────────────────────────────────────────────┐
    │                                                                  │
    │  1. Compilas nueva versión:                                     │
    │     dotnet publish -c Release                                   │
    │                                                                  │
    │  2. Ejecutas script de publicación:                             │
    │     .\Publish-Update.ps1 -Version "1.1.0"                       │
    │                                                                  │
    │  3. El script copia los archivos a:                             │
    │     C:\SGRRHH_Data\updates\latest\                              │
    │     C:\SGRRHH_Data\updates\version.json                         │
    │                                                                  │
    └─────────────────────────────────────────────────────────────────┘
                                │
                                │ Red Local (carpeta compartida)
                                ▼
    ┌─────────────────────────────────────────────────────────────────┐
    │               \\SERVIDOR\SGRRHH\updates\                        │
    │                                                                  │
    │  ├── version.json    ← Metadatos de la versión                  │
    │  │   {                                                          │
    │  │     "version": "1.1.0",                                      │
    │  │     "releaseDate": "2025-11-27T10:00:00",                    │
    │  │     "mandatory": false,                                       │
    │  │     "releaseNotes": "Mejoras de rendimiento...",             │
    │  │     "checksum": "sha256:abc123..."                           │
    │  │   }                                                          │
    │  │                                                               │
    │  └── latest\         ← Archivos de la nueva versión             │
    │      ├── SGRRHH.exe                                             │
    │      ├── SGRRHH.dll                                             │
    │      ├── appsettings.json (plantilla)                           │
    │      └── ... demás archivos                                      │
    │                                                                  │
    └─────────────────────────────────────────────────────────────────┘
                                │
                                │
          ┌─────────────────────┼─────────────────────┐
          │                     │                     │
          ▼                     ▼                     ▼
    ┌──────────┐         ┌──────────┐         ┌──────────┐
    │ Servidor │         │Ingeniera │         │Secretaria│
    │          │         │          │         │          │
    │ Al inicio│         │ Al inicio│         │ Al inicio│
    │ verifica │         │ verifica │         │ verifica │
    │ versión  │         │ versión  │         │ versión  │
    │          │         │          │         │          │
    │ Si hay   │         │ Si hay   │         │ Si hay   │
    │ nueva:   │         │ nueva:   │         │ nueva:   │
    │ - Avisa  │         │ - Avisa  │         │ - Avisa  │
    │ - Descarga         │ - Descarga         │ - Descarga
    │ - Instala│         │ - Instala│         │ - Instala│
    │ - Reinicia         │ - Reinicia         │ - Reinicia
    └──────────┘         └──────────┘         └──────────┘
```

### Flujo de Actualización (Detallado)

```
┌─────────────────────────────────────────────────────────────────────┐
│                 FLUJO DE ACTUALIZACIÓN AUTOMÁTICA                    │
└─────────────────────────────────────────────────────────────────────┘

    Usuario inicia SGRRHH.exe
              │
              ▼
    ┌─────────────────────┐
    │  UpdateService      │
    │  .CheckForUpdates() │
    │                     │
    │  Lee version.json   │
    │  de red compartida  │
    └──────────┬──────────┘
               │
               ▼
    ┌─────────────────────┐
    │  Compara versiones  │
    │                     │
    │  Local: 1.0.0       │
    │  Servidor: 1.1.0    │
    └──────────┬──────────┘
               │
        ┌──────┴──────┐
        │             │
        ▼             ▼
    Sin cambios    Nueva versión
        │             │
        ▼             ▼
    Continúa      ┌─────────────────────┐
    normal        │  Muestra diálogo:   │
                  │                     │
                  │  "Hay una nueva     │
                  │   versión (1.1.0)   │
                  │   disponible"       │
                  │                     │
                  │  [Actualizar ahora] │
                  │  [Recordar después] │
                  │  [Omitir versión]   │
                  └──────────┬──────────┘
                             │
                             │ Usuario acepta
                             ▼
                  ┌─────────────────────┐
                  │  Proceso de         │
                  │  actualización:     │
                  │                     │
                  │  1. Copia archivos  │
                  │     a carpeta temp  │
                  │                     │
                  │  2. Verifica        │
                  │     checksum        │
                  │                     │
                  │  3. Cierra app      │
                  │                     │
                  │  4. Ejecuta updater │
                  │     (proceso ext.)  │
                  │                     │
                  │  5. Reemplaza       │
                  │     archivos        │
                  │                     │
                  │  6. Reinicia app    │
                  └─────────────────────┘
```

---

## 📋 Archivo version.json (Estructura)

```json
{
  "version": "1.1.0",
  "releaseDate": "2025-11-27T10:00:00Z",
  "mandatory": false,
  "minimumVersion": "1.0.0",
  "releaseNotes": "## Cambios en v1.1.0\n\n- Mejora de rendimiento\n- Corrección de errores\n- Nueva funcionalidad X",
  "checksum": "sha256:e3b0c44298fc1c149afbf4c8996fb92427ae41e4649b934ca495991b7852b855",
  "downloadSize": 45678900,
  "files": [
    {"name": "SGRRHH.exe", "checksum": "sha256:abc..."},
    {"name": "SGRRHH.dll", "checksum": "sha256:def..."}
  ]
}
```

---

## 📁 Estructura de Carpetas del Sistema

### En el Servidor (C:\SGRRHH_Data)

```
C:\SGRRHH_Data\                    ← Carpeta compartida como \\SERVIDOR\SGRRHH
├── sgrrhh.db                      ← Base de datos principal
├── sgrrhh.db-wal                  ← WAL (auto-generado)
├── sgrrhh.db-shm                  ← Shared memory (auto-generado)
│
├── fotos\                         ← Fotos de empleados
│   └── {empleadoId}\
│       └── foto.jpg
│
├── documentos\                    ← Documentos adjuntos
│   ├── permisos\
│   │   └── {permisoId}\
│   │       └── soporte.pdf
│   └── contratos\
│       └── {contratoId}\
│           └── contrato.pdf
│
├── backups\                       ← Copias de seguridad
│   └── sgrrhh_20251127_100000.db
│
├── config\                        ← Configuración compartida
│   └── logo.png
│
├── logs\                          ← Logs de errores
│   └── error_2025-11-27.log
│
└── updates\                       ← 🆕 SISTEMA DE ACTUALIZACIONES
    ├── version.json               ← Metadatos versión actual
    ├── latest\                    ← Archivos última versión
    │   ├── SGRRHH.exe
    │   ├── SGRRHH.dll
    │   ├── appsettings.template.json
    │   └── ... otros archivos
    └── history\                   ← Historial (opcional)
        ├── 1.0.0\
        └── 1.1.0\
```

### En cada PC cliente (C:\SGRRHH o C:\Program Files\SGRRHH)

```
C:\SGRRHH\                         ← Carpeta de instalación
├── SGRRHH.exe                     ← Ejecutable principal
├── SGRRHH.dll                     ← Bibliotecas
├── appsettings.json               ← Configuración LOCAL (no se sobrescribe)
├── SGRRHH.Updater.exe             ← Actualizador (nuevo)
├── data\                          ← Datos locales temporales
│   └── logs\
│       └── error_2025-11-27.log
└── runtimes\                      ← Runtime de .NET (si es self-contained)
```

---

## ⚙️ Archivos appsettings.json por PC

### PC Servidor (usa ruta local)

```json
{
  "Database": {
    "Path": "C:\\SGRRHH_Data\\sgrrhh.db",
    "EnableWalMode": true,
    "BusyTimeout": 30000
  },
  "Network": {
    "IsNetworkMode": true,
    "SharedFolder": "C:\\SGRRHH_Data"
  },
  "Updates": {
    "Enabled": true,
    "CheckOnStartup": true,
    "UpdatesPath": "C:\\SGRRHH_Data\\updates"
  },
  "Application": {
    "Name": "SGRRHH",
    "Version": "1.0.0",
    "Company": "Mi Empresa"
  }
}
```

### PC Ingeniera / Secretaria (usa ruta de red)

```json
{
  "Database": {
    "Path": "\\\\ELITEBOOK-EVERT\\SGRRHH\\sgrrhh.db",
    "EnableWalMode": true,
    "BusyTimeout": 30000
  },
  "Network": {
    "IsNetworkMode": true,
    "SharedFolder": "\\\\ELITEBOOK-EVERT\\SGRRHH"
  },
  "Updates": {
    "Enabled": true,
    "CheckOnStartup": true,
    "UpdatesPath": "\\\\ELITEBOOK-EVERT\\SGRRHH\\updates"
  },
  "Application": {
    "Name": "SGRRHH",
    "Version": "1.0.0",
    "Company": "Mi Empresa"
  }
}
```

---

## 📝 Próximos Pasos

1. **Implementar UpdateService** - Servicio que verifica y descarga actualizaciones
2. **Crear SGRRHH.Updater.exe** - Proceso externo que reemplaza archivos
3. **Script Publish-Update.ps1** - Para publicar nuevas versiones
4. **Integrar en App.xaml.cs** - Verificar actualizaciones al inicio
5. **UI para actualizaciones** - Ventana de notificación y progreso

---

*Última actualización: Noviembre 2025*
