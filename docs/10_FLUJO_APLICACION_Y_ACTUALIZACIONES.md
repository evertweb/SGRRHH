# 📊 Flujo de la Aplicación SGRRHH y Sistema de Actualizaciones

## 🎯 Resumen Ejecutivo

SGRRHH es una aplicación de escritorio WPF (.NET 8) para gestión de recursos humanos con backend Firebase (Firestore + Storage). Diseñada para **3 usuarios**:

| PC | Usuario | Rol | Función Principal |
|----|---------|-----|-------------------|
| **Servidor** (Tu PC) | `admin` | Administrador | Configuración, supervisión total |
| **PC Ingeniera** | `ingeniera` | Aprobador | Aprobar/rechazar permisos y solicitudes |
| **PC Secretaria** | `secretaria` | Operador | Registrar empleados, control diario, solicitar permisos |

---

## 🔄 Arquitectura

```
┌─────────────────────────────────────────────────────────────────────┐
│                    ARQUITECTURA SGRRHH v1.1.x                        │
├─────────────────────────────────────────────────────────────────────┤
│                                                                      │
│  ┌──────────────────┐  ┌──────────────────┐  ┌──────────────────┐  │
│  │   PC SERVIDOR    │  │   PC INGENIERA   │  │  PC SECRETARIA   │  │
│  │                  │  │                  │  │                  │  │
│  │  ┌────────────┐  │  │  ┌────────────┐  │  │  ┌────────────┐  │  │
│  │  │ SGRRHH.exe │  │  │  │ SGRRHH.exe │  │  │  │ SGRRHH.exe │  │  │
│  │  │ (Admin)    │  │  │  │ (Aprobador)│  │  │  │ (Operador) │  │  │
│  │  └──────┬─────┘  │  │  └──────┬─────┘  │  │  └──────┬─────┘  │  │
│  └─────────┼────────┘  └─────────┼────────┘  └─────────┼────────┘  │
│            │                     │                     │           │
│            └─────────────────────┼─────────────────────┘           │
│                                  │                                  │
│                                  ▼                                  │
│  ┌─────────────────────────────────────────────────────────────┐   │
│  │                   ☁️ FIREBASE (Internet)                      │   │
│  │                                                              │   │
│  │  ├── Firestore        ← Base de datos en tiempo real        │   │
│  │  ├── Firebase Storage ← Fotos y documentos                  │   │
│  │  └── Auth             ← Autenticación                       │   │
│  └─────────────────────────────────────────────────────────────┘   │
│                                  │                                  │
│                                  ▼                                  │
│  ┌─────────────────────────────────────────────────────────────┐   │
│  │                   🐙 GITHUB RELEASES                          │   │
│  │                                                              │   │
│  │  ├── Última versión disponible                              │   │
│  │  ├── ZIP de distribución (~12 MB)                           │   │
│  │  └── Notas de versión                                       │   │
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
    │   Firebase          │
    │ - Registra repos    │
    │ - Registra services │
    │ - Registra VMs      │
    └──────────┬──────────┘
               │
               ▼
    ┌─────────────────────┐
    │ CheckForUpdates()   │
    │                     │
    │ - Consulta GitHub   │
    │   API releases      │
    │ - Compara versiones │
    │ - Si hay nueva:     │
    │   muestra diálogo   │
    └──────────┬──────────┘
               │
               ▼
    ┌─────────────────────┐        ┌─────────────────────┐
    │   LoginWindow       │        │   Validación        │
    │                     │──────► │                     │
    │ - Usuario           │        │ - Verifica usuario  │
    │ - Contraseña        │        │ - Verifica password │
    │                     │        │   (Firebase Auth)   │
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
    ├── 💬 Chat ────────────► Chat en tiempo real
    │
    └── ⚙️ Configuración
            ├── Empresa (logo, datos)
            └── Sistema
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
    ├── 💬 Chat ───────────────► Chat en tiempo real
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
    ├── 💬 Chat ───────────────► Chat en tiempo real
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
             │ Guarda en Firestore
             │ (sincronización instantánea)
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
         │ Actualiza Firestore
         ▼       ▼
    ┌──────────────────┐
    │  TODAS LAS PCs   │
    │                  │
    │  5. Ven el       │
    │     estado       │
    │     actualizado  │
    │     (tiempo real)│
    │                  │
    │  6. Admin puede  │
    │     generar Acta │
    │     PDF          │
    └──────────────────┘
```

---

## 🚀 Sistema de Actualizaciones Automáticas (GitHub Releases)

### Arquitectura de Actualizaciones

```
┌─────────────────────────────────────────────────────────────────────┐
│              SISTEMA DE ACTUALIZACIONES AUTOMÁTICAS                  │
└─────────────────────────────────────────────────────────────────────┘

    DESARROLLADOR
    ┌─────────────────────────────────────────────────────────────────┐
    │                                                                  │
    │  1. Push cambios a GitHub                                       │
    │                                                                  │
    │  2. Crear tag de versión (ej: v1.1.4)                          │
    │     git tag v1.1.4                                              │
    │     git push origin v1.1.4                                      │
    │                                                                  │
    │  3. GitHub Actions compila automáticamente:                     │
    │     - dotnet publish --self-contained false                     │
    │     - Crea ZIP (~12 MB, requiere .NET 8 runtime)               │
    │     - Publica GitHub Release                                    │
    │                                                                  │
    └─────────────────────────────────────────────────────────────────┘
                                │
                                │ GitHub Release publicado
                                ▼
    ┌─────────────────────────────────────────────────────────────────┐
    │               🐙 GITHUB RELEASES                                 │
    │               github.com/evertweb/SGRRHH/releases               │
    │                                                                  │
    │  ├── v1.1.4 (latest)                                           │
    │  │   ├── SGRRHH.zip (~12 MB)                                   │
    │  │   └── Release notes                                         │
    │  ├── v1.1.3                                                    │
    │  └── ...                                                        │
    │                                                                  │
    └─────────────────────────────────────────────────────────────────┘
                                │
                                │ Al iniciar la app
                                ▼
    ┌─────────────────────────────────────────────────────────────────┐
    │               CLIENTE (cualquier PC)                             │
    │                                                                  │
    │  GithubUpdateService.CheckForUpdatesAsync()                     │
    │                                                                  │
    │  1. GET api.github.com/repos/evertweb/SGRRHH/releases/latest    │
    │  2. Compara versión local vs GitHub                             │
    │  3. Si hay nueva versión → muestra diálogo                      │
    │                                                                  │
    └─────────────────────────────────────────────────────────────────┘
```

### Flujo de Actualización (Usuario Final)

```
┌─────────────────────────────────────────────────────────────────────┐
│                 FLUJO DE ACTUALIZACIÓN AUTOMÁTICA                    │
└─────────────────────────────────────────────────────────────────────┘

    Usuario inicia SGRRHH.exe
              │
              ▼
    ┌─────────────────────┐
    │ GithubUpdateService │
    │ .CheckForUpdates()  │
    │                     │
    │  Consulta GitHub    │
    │  API releases       │
    └──────────┬──────────┘
               │
               ▼
    ┌─────────────────────┐
    │  Compara versiones  │
    │                     │
    │  Local: 1.1.2       │
    │  GitHub: 1.1.4      │
    └──────────┬──────────┘
               │
        ┌──────┴──────┐
        │             │
        ▼             ▼
    Sin cambios    Nueva versión
        │             │
        ▼             ▼
    Continúa      ┌─────────────────────┐
    al login      │  Muestra diálogo:   │
                  │                     │
                  │  "Hay una nueva     │
                  │   versión (1.1.4)   │
                  │   disponible"       │
                  │                     │
                  │  [Actualizar ahora] │
                  │  [Recordar después] │
                  └──────────┬──────────┘
                             │
                             │ Usuario acepta
                             ▼
                  ┌─────────────────────┐
                  │  Proceso de         │
                  │  actualización:     │
                  │                     │
                  │  1. Descarga ZIP    │
                  │     desde GitHub    │
                  │                     │
                  │  2. Extrae en       │
                  │     carpeta temp    │
                  │                     │
                  │  3. Lanza           │
                  │     SGRRHH.Updater  │
                  │                     │
                  │  4. Cierra app      │
                  │     principal       │
                  │                     │
                  │  5. Updater copia   │
                  │     archivos        │
                  │     (excepto sí     │
                  │      mismo)         │
                  │                     │
                  │  6. Reinicia app    │
                  └─────────────────────┘
```

### Componentes del Sistema de Actualización

| Componente | Archivo | Función |
|------------|---------|---------|
| **GithubUpdateService** | `Infrastructure/Services/GithubUpdateService.cs` | Verifica releases en GitHub API, descarga ZIP |
| **SGRRHH.Updater** | `src/SGRRHH.Updater/Program.cs` | Proceso externo que aplica la actualización |
| **UpdateDialog** | `WPF/Views/UpdateDialog.xaml` | UI para notificar y gestionar actualización |
| **UpdateDialogViewModel** | `WPF/ViewModels/UpdateDialogViewModel.cs` | Lógica de descarga y progreso |
| **GitHub Actions** | `.github/workflows/release.yml` | Compilación y publicación automática |

### Características del Updater

- ✅ **Mata todos los procesos SGRRHH** antes de actualizar
- ✅ **Excluye sus propios archivos** (SGRRHH.Updater.*) para evitar "archivo en uso"
- ✅ **Retry con delay incremental** si hay archivos bloqueados
- ✅ **Logging detallado** en `updater_log.txt`
- ✅ **Reinicio automático** de la aplicación

---

## 📁 Estructura de Carpetas del Sistema

### En cada PC (C:\SGRRHH)

```
C:\SGRRHH\                         ← Carpeta de instalación
├── SGRRHH.exe                     ← Ejecutable principal
├── SGRRHH.dll                     ← Bibliotecas .NET
├── appsettings.json               ← Configuración (Firebase, versión)
├── firebase-credentials.json      ← Credenciales Firebase
├── SGRRHH.Updater.exe             ← Proceso de actualización
├── SGRRHH.Updater.dll             ← 
├── SGRRHH.Updater.deps.json       ← 
├── updater_log.txt                ← Log del último proceso de actualización
└── *.dll                          ← Dependencias .NET
```

### Carpeta temporal de actualización

```
%LOCALAPPDATA%\Temp\
└── SGRRHH_update_temp\            ← Creada durante actualización
    └── extracted\                 ← Archivos descomprimidos del ZIP
        ├── SGRRHH.exe
        ├── SGRRHH.dll
        └── ...
```

---

## ⚙️ Archivo appsettings.json

```json
{
  "Firebase": {
    "ProjectId": "sgrrhh-xxxxx",
    "CredentialsPath": "firebase-credentials.json"
  },
  "Application": {
    "Name": "SGRRHH",
    "Version": "1.1.4",
    "Company": "Mi Empresa"
  },
  "Updates": {
    "Enabled": true,
    "CheckOnStartup": true,
    "Repository": "evertweb/SGRRHH"
  }
}
```

---

## 🔧 Cómo Publicar una Nueva Versión

### Método 1: Automático con GitHub Actions (Recomendado)

```bash
# 1. Actualizar versión en csproj
# src/SGRRHH.WPF/SGRRHH.WPF.csproj → <Version>1.1.5</Version>

# 2. Commit y push
git add .
git commit -m "Release v1.1.5: descripción de cambios"
git push

# 3. Crear y push tag
git tag v1.1.5
git push origin v1.1.5

# 4. GitHub Actions compila y publica automáticamente
```

### Método 2: Manual (para distribución inicial)

```powershell
# Usar las tareas de VS Code:
# Task: "1. Build + Actualizar Local" → Compila y copia a C:\SGRRHH
# Task: "2b. Publicar TODO" → Sube a GitHub + actualiza local
```

---

## 📊 Tamaño del ZIP

| Tipo | Tamaño | Descripción |
|------|--------|-------------|
| **Non-self-contained** | ~12 MB | Requiere .NET 8 Runtime instalado |
| Self-contained (antiguo) | ~82 MB | Incluye .NET Runtime |

El sistema actual usa **non-self-contained** para descargas más rápidas.

---

*Última actualización: Enero 2025*
*Versión del sistema: 1.1.x*
