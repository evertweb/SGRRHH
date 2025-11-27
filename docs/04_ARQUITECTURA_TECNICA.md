# 🏗️ Arquitectura Técnica - Sistema RRHH

## 🎯 Decisión de Tecnología

### Opción Recomendada: **C# + WPF + SQLite**

| Aspecto | Tecnología | Razón |
|---------|------------|-------|
| **Lenguaje** | C# (.NET 8) | Nativo Windows, maduro, excelente rendimiento |
| **Interfaz** | WPF (Windows Presentation Foundation) | UI nativa Windows, moderna, profesional |
| **Base de datos** | SQLite | Local, sin servidor, archivo único, portable |
| **Reportes** | FastReport o similar | Generación de PDF/impresión |
| **Arquitectura** | MVVM + Clean Architecture | Mantenible, escalable, testeable |

### ¿Por qué esta combinación?

✅ **100% Nativo Windows** - No depende de navegadores ni frameworks web
✅ **Sin instalaciones complejas** - SQLite es un archivo, no un servidor
✅ **Rendimiento excelente** - C# compilado es muy rápido
✅ **Interfaz profesional** - WPF permite UIs modernas y atractivas
✅ **Multi-usuario en red** - SQLite soporta acceso concurrente moderado (3 usuarios es perfecto)
✅ **Fácil distribución** - Se puede crear un instalador .exe
✅ **Mantenimiento futuro** - C# tiene excelente documentación y comunidad

### Alternativas Consideradas:

| Opción | Pros | Contras | Decisión |
|--------|------|---------|----------|
| C# + WPF | Nativo, profesional, robusto | Curva de aprendizaje | ✅ **ELEGIDA** |
| C# + WinForms | Más simple | UI anticuada | ❌ |
| Python + Tkinter | Rápido desarrollo | UI básica, no tan nativa | ❌ |
| Electron | UI moderna | Pesado, no nativo | ❌ |
| C++ + Qt | Muy nativo | Complejidad alta | ❌ |

---

## 📁 ESTRUCTURA DEL PROYECTO

```
SGRRHH/
├── 📁 src/
│   ├── 📁 SGRRHH.Core/                 # Lógica de negocio (sin dependencias UI)
│   │   ├── 📁 Entities/                # Entidades del dominio
│   │   │   ├── Empleado.cs
│   │   │   ├── RegistroDiario.cs
│   │   │   ├── Permiso.cs
│   │   │   ├── Contrato.cs
│   │   │   ├── Vacacion.cs
│   │   │   ├── Actividad.cs
│   │   │   ├── Proyecto.cs
│   │   │   ├── Departamento.cs
│   │   │   ├── Cargo.cs
│   │   │   └── Usuario.cs
│   │   │
│   │   ├── 📁 Interfaces/              # Contratos/Interfaces
│   │   │   ├── IEmpleadoRepository.cs
│   │   │   ├── IPermisoRepository.cs
│   │   │   └── ...
│   │   │
│   │   ├── 📁 Services/                # Servicios de negocio
│   │   │   ├── EmpleadoService.cs
│   │   │   ├── PermisoService.cs
│   │   │   ├── VacacionesService.cs
│   │   │   ├── ReporteService.cs
│   │   │   └── AlertaService.cs
│   │   │
│   │   └── 📁 Enums/                   # Enumeraciones
│   │       ├── TipoContrato.cs
│   │       ├── TipoPermiso.cs
│   │       ├── EstadoPermiso.cs
│   │       └── RolUsuario.cs
│   │
│   ├── 📁 SGRRHH.Infrastructure/       # Acceso a datos
│   │   ├── 📁 Data/
│   │   │   ├── AppDbContext.cs         # Contexto Entity Framework
│   │   │   └── DatabaseInitializer.cs  # Datos iniciales
│   │   │
│   │   ├── 📁 Repositories/            # Implementación de repositorios
│   │   │   ├── EmpleadoRepository.cs
│   │   │   ├── PermisoRepository.cs
│   │   │   └── ...
│   │   │
│   │   └── 📁 Migrations/              # Migraciones de BD
│   │
│   ├── 📁 SGRRHH.WPF/                  # Aplicación WPF (UI)
│   │   ├── 📁 Views/                   # Ventanas y páginas
│   │   │   ├── MainWindow.xaml
│   │   │   ├── LoginWindow.xaml
│   │   │   ├── 📁 Empleados/
│   │   │   │   ├── EmpleadosListView.xaml
│   │   │   │   ├── EmpleadoDetailView.xaml
│   │   │   │   └── EmpleadoFormView.xaml
│   │   │   ├── 📁 ControlDiario/
│   │   │   ├── 📁 Permisos/
│   │   │   ├── 📁 Vacaciones/
│   │   │   ├── 📁 Reportes/
│   │   │   ├── 📁 Catalogos/
│   │   │   └── 📁 Configuracion/
│   │   │
│   │   ├── 📁 ViewModels/              # ViewModels (MVVM)
│   │   │   ├── MainViewModel.cs
│   │   │   ├── EmpleadosViewModel.cs
│   │   │   ├── PermisosViewModel.cs
│   │   │   └── ...
│   │   │
│   │   ├── 📁 Controls/                # Controles personalizados
│   │   │   ├── AlertPanel.xaml
│   │   │   ├── DashboardCard.xaml
│   │   │   └── ...
│   │   │
│   │   ├── 📁 Resources/               # Recursos
│   │   │   ├── 📁 Styles/              # Estilos XAML
│   │   │   ├── 📁 Images/              # Imágenes/iconos
│   │   │   └── 📁 Templates/           # Plantillas de documentos
│   │   │
│   │   ├── 📁 Converters/              # Convertidores XAML
│   │   ├── 📁 Helpers/                 # Utilidades
│   │   └── App.xaml                    # Configuración de la app
│   │
│   └── 📁 SGRRHH.Reports/              # Generación de reportes/documentos
│       ├── ActaPermisoReport.cs
│       ├── CertificadoLaboralReport.cs
│       └── ...
│
├── 📁 data/                            # Carpeta de datos (en producción)
│   ├── sgrrhh.db                       # Base de datos SQLite
│   ├── 📁 documentos/                  # Documentos adjuntos
│   │   ├── 📁 empleados/               # Por empleado
│   │   └── 📁 permisos/                # Soportes de permisos
│   └── 📁 backups/                     # Copias de seguridad
│
├── 📁 docs/                            # Documentación
│
└── 📁 installer/                       # Archivos para crear instalador
```

---

## 🗄️ MODELO DE BASE DE DATOS

### Diagrama Entidad-Relación:

```
┌─────────────────┐       ┌─────────────────┐
│    USUARIOS     │       │  DEPARTAMENTOS  │
├─────────────────┤       ├─────────────────┤
│ Id              │       │ Id              │
│ Username        │       │ Codigo          │
│ PasswordHash    │       │ Nombre          │
│ NombreCompleto  │       │ JefeId (FK)     │
│ Rol             │       │ Activo          │
│ Activo          │       └────────┬────────┘
└─────────────────┘                │
                                   │
┌─────────────────┐       ┌────────┴────────┐       ┌─────────────────┐
│     CARGOS      │       │   EMPLEADOS     │       │   CONTRATOS     │
├─────────────────┤       ├─────────────────┤       ├─────────────────┤
│ Id              │◄──────┤ Id              │───────►│ Id              │
│ Codigo          │       │ Codigo          │       │ EmpleadoId (FK) │
│ Nombre          │       │ Cedula          │       │ NumeroContrato  │
│ DepartamentoId  │       │ Nombres         │       │ TipoContrato    │
│ Nivel           │       │ Apellidos       │       │ FechaInicio     │
│ Activo          │       │ FechaNacimiento │       │ FechaFin        │
└─────────────────┘       │ Genero          │       │ Cargo           │
                          │ EstadoCivil     │       │ Salario         │
                          │ Direccion       │       │ Estado          │
                          │ Telefono        │       │ Documento       │
                          │ TelefonoEmerg.  │       └─────────────────┘
                          │ Email           │
                          │ FotoPath        │       ┌─────────────────┐
                          │ FechaIngreso    │       │   VACACIONES    │
                          │ CargoId (FK)    │       ├─────────────────┤
                          │ DepartamentoId  │◄──────┤ Id              │
                          │ SupervisorId    │       │ EmpleadoId (FK) │
                          │ TipoContrato    │       │ Periodo         │
                          │ Estado          │       │ DiasDisponibles │
                          │ Activo          │       │ DiasTomados     │
                          └────────┬────────┘       │ FechaInicio     │
                                   │                │ FechaFin        │
                 ┌─────────────────┼─────────────────┐ Estado         │
                 │                 │                 │ └─────────────────┘
                 ▼                 ▼                 ▼
┌─────────────────┐  ┌─────────────────┐  ┌─────────────────┐
│REGISTROS_DIARIOS│  │    PERMISOS     │  │   DOCUMENTOS    │
├─────────────────┤  ├─────────────────┤  ├─────────────────┤
│ Id              │  │ Id              │  │ Id              │
│ EmpleadoId (FK) │  │ NumeroActa      │  │ EmpleadoId (FK) │
│ Fecha           │  │ EmpleadoId (FK) │  │ Tipo            │
│ HoraEntrada     │  │ TipoPermiso     │  │ Nombre          │
│ HoraSalida      │  │ Motivo          │  │ RutaArchivo     │
│ TotalHoras      │  │ FechaSolicitud  │  │ FechaSubida     │
│ Observaciones   │  │ FechaInicio     │  └─────────────────┘
└────────┬────────┘  │ FechaFin        │
         │           │ HoraSalida      │
         ▼           │ HoraRegreso     │
┌─────────────────┐  │ TotalDias       │
│DETALLE_ACTIVIDAD│  │ Estado          │
├─────────────────┤  │ AprobadoPor     │
│ Id              │  │ FechaAprobacion │
│ RegistroId (FK) │  │ TipoCompensacion│
│ ActividadId(FK) │  │ FechaCompensar  │
│ ProyectoId (FK) │  │ DocumentoPath   │
│ HorasDedicadas  │  │ Observaciones   │
│ Estado          │  │ CreadoPor       │
│ Avance          │  │ FechaCreacion   │
│ Observaciones   │  └─────────────────┘
└─────────────────┘

┌─────────────────┐  ┌─────────────────┐
│   ACTIVIDADES   │  │    PROYECTOS    │
├─────────────────┤  ├─────────────────┤
│ Id              │  │ Id              │
│ Codigo          │  │ Codigo          │
│ Nombre          │  │ Nombre          │
│ Categoria       │  │ Descripcion     │
│ Descripcion     │  │ FechaInicio     │
│ Activo          │  │ FechaFinEstimada│
└─────────────────┘  │ Estado          │
                     │ ResponsableId   │
┌─────────────────┐  │ Activo          │
│ TIPOS_PERMISO   │  └─────────────────┘
├─────────────────┤
│ Id              │  ┌─────────────────┐
│ Codigo          │  │  CONFIGURACION  │
│ Nombre          │  ├─────────────────┤
│ RemuneradoDef.  │  │ Id              │
│ RequiereSoporte │  │ Clave           │
│ DiasMaximos     │  │ Valor           │
│ Activo          │  │ Descripcion     │
└─────────────────┘  └─────────────────┘

┌─────────────────┐
│    AUDITORÍA    │
├─────────────────┤
│ Id              │
│ Tabla           │
│ RegistroId      │
│ Accion          │
│ ValorAnterior   │
│ ValorNuevo      │
│ UsuarioId       │
│ Fecha           │
└─────────────────┘
```

---

## 🔐 SISTEMA DE AUTENTICACIÓN

### Roles:

```csharp
public enum RolUsuario
{
    Administrador = 1,  // Todo
    Aprobador = 2,      // Ingeniera: aprobar, consultar
    Operador = 3        // Secretaria: registrar, solicitar
}
```

### Permisos por Módulo:

| Módulo | Admin | Aprobador (Ing.) | Operador (Secre.) |
|--------|-------|------------------|-------------------|
| Dashboard | ✅ Ver todo | ✅ Ver todo | ✅ Ver básico |
| Empleados | ✅ CRUD | ✅ Ver | ✅ CRUD |
| Control Diario | ✅ CRUD | ✅ Ver | ✅ CRUD |
| Permisos - Crear | ✅ | ❌ | ✅ |
| Permisos - Aprobar | ✅ | ✅ | ❌ |
| Permisos - Ver | ✅ | ✅ | ✅ (solo creados) |
| Vacaciones | ✅ CRUD | ✅ Ver/Aprobar | ✅ Ver |
| Catálogos | ✅ CRUD | ✅ Ver | ✅ Ver |
| Reportes | ✅ Todos | ✅ Todos | ✅ Básicos |
| Configuración | ✅ | ❌ | ❌ |
| Usuarios | ✅ | ❌ | ❌ |
| Backup | ✅ | ❌ | ❌ |

---

## 🌐 ARQUITECTURA MULTI-PC (Red Local)

### Opción Elegida: Carpeta Compartida + SQLite

```
┌─────────────────────────────────────────────────────────────┐
│                    RED LOCAL EMPRESA                        │
├─────────────────────────────────────────────────────────────┤
│                                                             │
│  ┌─────────────┐  ┌─────────────┐  ┌─────────────┐        │
│  │  PC Admin   │  │PC Secretaria│  │PC Ingeniera │        │
│  │  (Tu PC)    │  │             │  │             │        │
│  │             │  │             │  │             │        │
│  │ [SGRRHH.exe]│  │ [SGRRHH.exe]│  │ [SGRRHH.exe]│        │
│  └──────┬──────┘  └──────┬──────┘  └──────┬──────┘        │
│         │                │                │                │
│         └────────────────┼────────────────┘                │
│                          │                                  │
│                          ▼                                  │
│              ┌───────────────────────┐                     │
│              │   CARPETA COMPARTIDA  │                     │
│              │   (En tu PC o NAS)    │                     │
│              │                       │                     │
│              │  \\SERVIDOR\SGRRHH\   │                     │
│              │  ├── sgrrhh.db        │                     │
│              │  ├── documentos\      │                     │
│              │  └── backups\         │                     │
│              └───────────────────────┘                     │
│                                                             │
└─────────────────────────────────────────────────────────────┘
```

### Configuración de Red:
1. Tu PC será el "servidor" con la carpeta compartida
2. Las otras PCs acceden a esa carpeta con permisos de lectura/escritura
3. SQLite maneja bien 3 usuarios concurrentes con WAL mode

### Manejo de Concurrencia:
- SQLite en modo WAL (Write-Ahead Logging)
- Bloqueos optimistas para ediciones
- Notificación si otro usuario está editando

---

## 🎨 DISEÑO DE INTERFAZ (MOCKUPS)

### Paleta de Colores Sugerida:

| Uso | Color | Hex |
|-----|-------|-----|
| Primario | Azul corporativo | #1E88E5 |
| Secundario | Gris oscuro | #37474F |
| Acento | Verde éxito | #43A047 |
| Alerta | Amarillo | #FFA000 |
| Error | Rojo | #E53935 |
| Fondo | Blanco/Gris claro | #FAFAFA |
| Texto | Gris oscuro | #212121 |

### Estructura de Pantallas:

```
┌────────────────────────────────────────────────────────────────┐
│  [Logo] SGRRHH - Sistema de Gestión RRHH    👤 Admin  [⚙️][✖]│
├────────┬───────────────────────────────────────────────────────┤
│        │                                                       │
│  📊    │                    CONTENIDO                         │
│ Panel  │                    PRINCIPAL                         │
│        │                                                       │
│  👥    │  ┌─────────┐ ┌─────────┐ ┌─────────┐ ┌─────────┐   │
│Emplead.│  │ 👥 20   │ │ ⚠️ 3    │ │ 📋 2    │ │ 🎂 1    │   │
│        │  │Empleados│ │Alertas  │ │Pendiente│ │Cumpleaños│   │
│  📅    │  └─────────┘ └─────────┘ └─────────┘ └─────────┘   │
│Control │                                                       │
│ Diario │  ┌───────────────────────────────────────────────┐   │
│        │  │           ALERTAS DEL DÍA                     │   │
│  📝    │  │ ⚠️ Contrato de Juan Pérez vence en 7 días    │   │
│Permisos│  │ 📋 2 permisos pendientes de aprobar          │   │
│        │  │ 🎂 Mañana cumple años María López             │   │
│  🏖️    │  └───────────────────────────────────────────────┘   │
│Vacacio.│                                                       │
│        │  ┌───────────────────────────────────────────────┐   │
│  📁    │  │           ACCIONES RÁPIDAS                    │   │
│Catálog.│  │  [+ Nuevo Empleado] [+ Registro Diario]       │   │
│        │  │  [+ Solicitar Permiso] [Ver Reportes]         │   │
│  📈    │  └───────────────────────────────────────────────┘   │
│Reportes│                                                       │
│        │                                                       │
│  ⚙️    │                                                       │
│Config. │                                                       │
│        │                                                       │
└────────┴───────────────────────────────────────────────────────┘
```

---

## 📦 TECNOLOGÍAS Y PAQUETES

### NuGet Packages a usar:

| Paquete | Propósito |
|---------|-----------|
| Microsoft.EntityFrameworkCore.Sqlite | ORM + SQLite |
| CommunityToolkit.Mvvm | MVVM helpers |
| MaterialDesignThemes | UI moderna (opcional) |
| MahApps.Metro | Controles modernos (opcional) |
| QuestPDF | Generación de PDFs |
| BCrypt.Net-Next | Encriptación de contraseñas |
| Serilog | Logging |
| AutoMapper | Mapeo de objetos |

---

## 📅 PLAN DE DESARROLLO (MVP)

### Fase 1 - Fundación (Semana 1-2)
- [ ] Estructura del proyecto
- [ ] Base de datos y migraciones
- [ ] Sistema de login
- [ ] Ventana principal con navegación
- [ ] CRUD de Empleados básico

### Fase 2 - Módulos Core (Semana 3-4)
- [ ] Control Diario completo
- [ ] Catálogos (Actividades, Departamentos, Cargos)
- [ ] Proyectos

### Fase 3 - Permisos (Semana 5-6)
- [ ] CRUD de Permisos
- [ ] Flujo de aprobación
- [ ] Generación de Acta PDF

### Fase 4 - Vacaciones y Contratos (Semana 7-8)
- [ ] Gestión de Vacaciones
- [ ] Gestión de Contratos
- [ ] Alertas

### Fase 5 - Reportes y Dashboard (Semana 9-10)
- [ ] Dashboard con estadísticas
- [ ] Reportes principales
- [ ] Documentos (Certificado laboral)

### Fase 6 - Pulido (Semana 11-12)
- [ ] Backup/Restore
- [ ] Configuración de empresa
- [ ] Pruebas y ajustes
- [ ] Instalador

---

## ✅ SIGUIENTE PASO

¿Apruebas esta arquitectura para comenzar el desarrollo?

- [ ] Sí, comenzar con Fase 1
- [ ] Tengo dudas/cambios (especificar)
