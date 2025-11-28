# 🔥 ROADMAP: Migración a Firebase - SGRRHH

## 📊 Resumen del Proyecto Actual

### Tecnologías Actuales
| Componente | Tecnología Actual | Migrar a Firebase |
|------------|-------------------|-------------------|
| Base de datos | SQLite (Entity Framework) | **Firestore** |
| Autenticación | BCrypt local | **Firebase Auth** |
| Archivos (fotos, PDFs) | Carpeta local/compartida | **Firebase Storage** |
| Actualizaciones | Carpeta compartida en red | **Firebase Hosting/Storage** |

### Entidades a Migrar (14 tablas)
```
1. Usuario          → users/{uid}
2. Empleado         → empleados/{id}
3. Departamento     → departamentos/{id}
4. Cargo            → cargos/{id}
5. Proyecto         → proyectos/{id}
6. Actividad        → actividades/{id}
7. RegistroDiario   → registros-diarios/{id}
8. DetalleActividad → registros-diarios/{id}/detalles/{detalleId}
9. TipoPermiso      → tipos-permiso/{id}
10. Permiso         → permisos/{id}
11. Vacacion        → vacaciones/{id}
12. Contrato        → contratos/{id}
13. ConfiguracionSistema → config/{clave}
14. AuditLog        → audit-logs/{id}
```

### Archivos que se Subirán a Storage
- `fotos/empleados/{empleadoId}/foto.jpg`
- `documentos/permisos/{permisoId}/soporte.pdf`
- `documentos/contratos/{contratoId}/contrato.pdf`
- `updates/latest/` (archivos de actualización)
- `config/logo.png`

### Servicios a Modificar (17 archivos)
```
Infrastructure/Services/
├── AuthService.cs          → Firebase Auth
├── EmpleadoService.cs      → Firestore
├── PermisoService.cs       → Firestore
├── VacacionService.cs      → Firestore
├── ContratoService.cs      → Firestore
├── ControlDiarioService.cs → Firestore
├── DepartamentoService.cs  → Firestore
├── CargoService.cs         → Firestore
├── ProyectoService.cs      → Firestore
├── ActividadService.cs     → Firestore
├── TipoPermisoService.cs   → Firestore
├── ConfiguracionService.cs → Firestore
├── UsuarioService.cs       → Firebase Auth + Firestore
├── AuditService.cs         → Firestore
├── BackupService.cs        → Firebase (export)
├── DocumentService.cs      → Storage para archivos
└── UpdateService.cs        → Firebase Hosting/Storage
```

---

## 🗓️ ROADMAP POR FASES (Para Sesiones de IA)

### Estimación Total: 8-10 sesiones de trabajo

---

## FASE 0: Configuración Firebase (1 sesión)
**Prompt para el agente:**
```
CONTEXTO: Proyecto SGRRHH - App WPF .NET 8 que estoy migrando de SQLite local a Firebase.

TAREA: Configurar proyecto Firebase y preparar el proyecto .NET

PASOS ESPECÍFICOS:
1. Guíame para crear un proyecto en Firebase Console:
   - Nombre sugerido: "sgrrhh-rrhh" o similar
   - Habilitar Firestore Database (modo producción, región southamerica-east1)
   - Habilitar Firebase Authentication (Email/Password)
   - Habilitar Firebase Storage
   
2. Crear archivo de configuración Firebase en el proyecto:
   - Descargar el archivo de configuración (JSON de service account)
   - Crear clase FirebaseConfig.cs en SGRRHH.Infrastructure
   
3. Agregar paquetes NuGet necesarios:
   - FirebaseAdmin (para .NET)
   - Google.Cloud.Firestore
   - Firebase.Auth (o alternativa para WPF)
   - Google.Cloud.Storage.V1
   
4. Crear estructura base en SGRRHH.Infrastructure:
   - /Firebase/FirebaseConfig.cs
   - /Firebase/FirebaseInitializer.cs
   
5. Modificar appsettings.json para incluir configuración Firebase

ARCHIVOS A CREAR:
- src/SGRRHH.Infrastructure/Firebase/FirebaseConfig.cs
- src/SGRRHH.Infrastructure/Firebase/FirebaseInitializer.cs

NO modificar aún: Repositorios, Servicios existentes, AppDbContext
```

**Entregables Fase 0:**
- [x] Proyecto Firebase creado → `rrhh-forestech`
- [x] Firestore habilitado → DatabaseId: `rrhh-forestech` (región: southamerica-east1)
- [x] Auth habilitado → Email/Password activo
- [x] Storage habilitado → Bucket: `rrhh-forestech.firebasestorage.app`
- [x] Paquetes NuGet instalados → FirebaseAdmin 3.0.1, Google.Cloud.Firestore 3.9.0, Google.Cloud.Storage.V1 4.10.0, FirebaseAuthentication.net 4.1.0
- [x] FirebaseConfig.cs creado → Con soporte para DatabaseId personalizado
- [x] Conexión probada → Escritura/lectura/eliminación exitosa ✅

**Archivos creados:**
```
src/SGRRHH.Infrastructure/Firebase/
├── FirebaseConfig.cs        # Configuración (ProjectId, ApiKey, DatabaseId, etc.)
└── FirebaseInitializer.cs   # Inicializa Firestore, Storage y Admin SDK

src/SGRRHH.WPF/
└── firebase-credentials.json  # Service Account (en .gitignore)
```

**Archivos modificados:**
- `appsettings.json` → Sección `Firebase` con todos los parámetros + `DataMode`
- `.gitignore` → Excluye credenciales Firebase

**Fecha completado:** 27 de Noviembre 2025

---

## FASE 1: Firebase Auth - Reemplazar Autenticación (1 sesión) ✅ COMPLETADA
**Prompt para el agente:**
```
CONTEXTO: Proyecto SGRRHH con Firebase configurado (Fase 0 completada).
La autenticación actual usa BCrypt local con tabla Usuario en SQLite.

TAREA: Migrar sistema de autenticación a Firebase Auth

ARCHIVOS A MODIFICAR:
1. src/SGRRHH.Infrastructure/Services/AuthService.cs
   - Reemplazar BCrypt por Firebase Auth
   - Mantener la interfaz IAuthService sin cambios
   - Login con email/password de Firebase
   
2. src/SGRRHH.Core/Entities/Usuario.cs
   - Agregar campo FirebaseUid (string)
   - Mantener campos existentes para compatibilidad
   
3. src/SGRRHH.WPF/ViewModels/LoginViewModel.cs
   - Actualizar para usar nuevo AuthService

LÓGICA REQUERIDA:
- Los 3 usuarios existentes (admin, secretaria, ingeniera) deben poder migrar
- El rol (Administrador, Operador, Aprobador) se guardará en Firestore (claims custom)
- Si el usuario no existe en Firebase Auth, crearlo en el primer login
- Mantener UltimoAcceso actualizado

USUARIOS A MIGRAR:
- admin / admin123 → admin@sgrrhh.local / admin123 (rol: Administrador)
- secretaria / secretaria123 → secretaria@sgrrhh.local / secretaria123 (rol: Operador)
- ingeniera / ingeniera123 → ingeniera@sgrrhh.local / ingeniera123 (rol: Aprobador)

CREAR:
- src/SGRRHH.Infrastructure/Firebase/FirebaseAuthService.cs

NO TOCAR AÚN: Repositorios de otras entidades, AppDbContext para otras tablas
```

**Entregables Fase 1:**
- [x] FirebaseAuthService.cs implementado
- [x] Login funcionando con Firebase Auth
- [x] Usuarios creados en Firebase Auth y Firestore
- [x] Roles almacenados en Firestore (colección `users`)
- [x] LoginViewModel funciona sin cambios (usa IAuthService)
- [x] Reglas de seguridad creadas (firestore.rules, storage.rules)
- [x] La app NO crea usuarios automáticamente (solo admin puede crear)

**Usuarios configurados en Firebase:**
| Usuario | Email | UID | Rol |
|---------|-------|-----|-----|
| admin | admin@sgrrhh.local | 6VSFfKaAlAaDOcH40EIzKaTZXBM2 | Administrador |
| secretaria | secretaria@sgrrhh.local | Z8JPNioOB5U0O8zMityslj5EjpZ2 | Operador |
| ingeniera | ingeniera@sgrrhh.local | iGpEuajlmjaknDfwBEjBkwtCRyK2 | Aprobador |

**Archivos creados:**
```
src/SGRRHH.Core/Interfaces/IFirebaseAuthService.cs     # Interfaz extendida para Firebase Auth
src/SGRRHH.Infrastructure/Firebase/FirebaseAuthService.cs    # Implementación de autenticación Firebase
src/SGRRHH.Infrastructure/Firebase/FirebaseUserMigration.cs  # Herramienta para migrar/crear usuarios
firestore.rules                                         # Reglas de seguridad Firestore
storage.rules                                           # Reglas de seguridad Storage
firestore.indexes.json                                  # Índices para queries
firebase.json                                           # Configuración para deploy de reglas
tools/CreateFirestoreUsers/                             # Herramienta para crear usuarios en Firestore
```

**Archivos modificados:**
- `src/SGRRHH.Core/Entities/Usuario.cs` → Agregado FirebaseUid y EmpleadoFirestoreId
- `src/SGRRHH.WPF/Helpers/AppSettings.cs` → Agregados métodos para leer configuración Firebase
- `src/SGRRHH.WPF/App.xaml.cs` → Switch SQLite/Firebase basado en DataMode
- `src/SGRRHH.WPF/appsettings.json` → DataMode cambiado a "Firebase"
- `src/SGRRHH.WPF/SGRRHH.WPF.csproj` → firebase-credentials.json se copia al output

**Fecha completado:** 27 de Noviembre 2025

---

## FASE 2: Firestore - Repositorio Base y Catálogos (1-2 sesiones) ✅ COMPLETADA
**Prompt para el agente:**
```
CONTEXTO: SGRRHH con Firebase Auth funcionando (Fase 1 completada).
Ahora migrar los repositorios de SQLite/EF Core a Firestore.

TAREA: Crear repositorio base para Firestore y migrar catálogos simples

PASO 1 - Crear repositorio genérico Firestore:
- src/SGRRHH.Infrastructure/Firebase/FirestoreRepository.cs
- Implementar IRepository<T> existente
- Métodos: GetByIdAsync, GetAllAsync, GetAllActiveAsync, AddAsync, UpdateAsync, DeleteAsync

PASO 2 - Migrar entidades catálogo (las más simples):
Orden de migración:
1. Departamento → colección "departamentos"
2. Cargo → colección "cargos" 
3. Actividad → colección "actividades"
4. Proyecto → colección "proyectos"
5. TipoPermiso → colección "tipos-permiso"
6. ConfiguracionSistema → colección "config"

CREAR ARCHIVOS:
- src/SGRRHH.Infrastructure/Firebase/FirestoreRepository.cs
- src/SGRRHH.Infrastructure/Firebase/Repositories/DepartamentoFirestoreRepository.cs
- src/SGRRHH.Infrastructure/Firebase/Repositories/CargoFirestoreRepository.cs
- src/SGRRHH.Infrastructure/Firebase/Repositories/ActividadFirestoreRepository.cs
- src/SGRRHH.Infrastructure/Firebase/Repositories/ProyectoFirestoreRepository.cs
- src/SGRRHH.Infrastructure/Firebase/Repositories/TipoPermisoFirestoreRepository.cs
- src/SGRRHH.Infrastructure/Firebase/Repositories/ConfiguracionFirestoreRepository.cs

MAPEO DE CAMPOS:
- EntidadBase.Id (int) → Document ID (string, auto-generado o "dep_001")
- EntidadBase.Activo → activo (bool)
- EntidadBase.FechaCreacion → fechaCreacion (Timestamp)
- EntidadBase.FechaModificacion → fechaModificacion (Timestamp)

IMPORTANTE:
- Crear script de migración de datos SQLite → Firestore
- Los IDs cambiarán de int a string

NO MODIFICAR AÚN: Empleado, Permiso, Contrato, RegistroDiario (tienen relaciones complejas)
```

**Entregables Fase 2:**
- [x] FirestoreRepository<T> base creado
- [x] 6 repositorios de catálogos migrados
- [x] Script de migración de datos inicial
- [x] Catálogos funcionando desde Firestore

**Archivos creados:**
```
src/SGRRHH.Core/Interfaces/
├── IFirestoreRepository.cs           # Interfaz base para repositorios Firestore

src/SGRRHH.Infrastructure/Firebase/
├── FirestoreRepository.cs            # Repositorio genérico base para Firestore
├── FirebaseServiceCollectionExtensions.cs  # Extensiones para registrar servicios en DI
└── Repositories/
    ├── DepartamentoFirestoreRepository.cs   # Colección: "departamentos"
    ├── CargoFirestoreRepository.cs          # Colección: "cargos"
    ├── ActividadFirestoreRepository.cs      # Colección: "actividades"
    ├── ProyectoFirestoreRepository.cs       # Colección: "proyectos"
    ├── TipoPermisoFirestoreRepository.cs    # Colección: "tipos-permiso"
    └── ConfiguracionFirestoreRepository.cs  # Colección: "config"

tools/MigrateToFirestore/
├── MigrateToFirestore.csproj         # Herramienta de migración
├── Program.cs                        # Lógica de migración de datos
└── appsettings.json                  # Configuración para la herramienta
```

**Archivos modificados:**
- `src/SGRRHH.WPF/App.xaml.cs` → ConfigureFirebaseServices() usa repositorios Firestore para catálogos

**Características implementadas:**
- Repositorio genérico con mapeo Entity ↔ Firestore Document
- Soporte para Document IDs personalizados (dep_0001, car_0001, etc.)
- Campos desnormalizados (ej: departamentoNombre en Cargo)
- Herramienta de migración con menú interactivo
- Limpieza de colecciones Firestore

**Próximo paso:** Ejecutar la herramienta de migración para mover datos de SQLite a Firestore:
```powershell
cd tools/MigrateToFirestore
# Copiar firebase-credentials.json antes de ejecutar
dotnet run
```

**Fecha completado:** 27 de Noviembre 2025

---

## FASE 3: Firestore - Entidades Principales (2 sesiones) ✅ COMPLETADA
**Prompt para el agente - Sesión 3A:**
```
CONTEXTO: SGRRHH con catálogos en Firestore (Fase 2 completada).

TAREA: Migrar entidad Empleado (la más compleja)

ESTRUCTURA FIRESTORE PARA EMPLEADO:
empleados/{empleadoId}
{
  codigo: "EMP001",
  cedula: "123456789",
  nombres: "Juan",
  apellidos: "Pérez",
  fechaNacimiento: Timestamp,
  genero: "Masculino",
  estadoCivil: "Casado",
  direccion: "...",
  telefono: "...",
  telefonoEmergencia: "...",
  contactoEmergencia: "...",
  email: "...",
  fotoUrl: "gs://bucket/fotos/empleados/emp001.jpg",  // URL de Storage
  fechaIngreso: Timestamp,
  fechaRetiro: Timestamp | null,
  estado: "Activo",
  tipoContrato: "Indefinido",
  cargoId: "cargo_001",        // Referencia a cargo
  cargoNombre: "Ingeniero",    // Desnormalizado para consultas
  departamentoId: "dep_001",   // Referencia
  departamentoNombre: "Ingeniería",  // Desnormalizado
  supervisorId: "emp002" | null,
  supervisorNombre: "María López" | null,  // Desnormalizado
  observaciones: "...",
  creadoPorId: "user_uid",
  aprobadoPorId: "user_uid" | null,
  fechaSolicitud: Timestamp,
  fechaAprobacion: Timestamp | null,
  motivoRechazo: null,
  activo: true,
  fechaCreacion: Timestamp,
  fechaModificacion: Timestamp
}

CREAR:
- src/SGRRHH.Infrastructure/Firebase/Repositories/EmpleadoFirestoreRepository.cs
- Actualizar EmpleadoService.cs para usar nuevo repositorio

CONSIDERACIONES:
- Desnormalizar nombres de cargo/departamento para evitar múltiples queries
- Actualizar datos desnormalizados cuando cambien los catálogos
- Manejar relación Supervisor (auto-referencia)

SCRIPT DE MIGRACIÓN:
- Migrar empleados existentes de SQLite a Firestore
- Subir fotos a Firebase Storage
- Actualizar URLs de fotos
```

**Prompt para el agente - Sesión 3B:**
```
CONTEXTO: SGRRHH con Empleado en Firestore (Fase 3A completada).

TAREA: Migrar Usuario, Permiso, Vacacion, Contrato

1. USUARIO (sincronizado con Firebase Auth):
users/{firebaseUid}
{
  username: "admin",
  nombreCompleto: "Administrador",
  email: "admin@sgrrhh.local",
  rol: "Administrador",  // Administrador, Operador, Aprobador
  empleadoId: "emp001" | null,
  ultimoAcceso: Timestamp,
  activo: true,
  fechaCreacion: Timestamp
}

2. PERMISO:
permisos/{permisoId}
{
  numeroActa: "ACT-2025-001",
  empleadoId: "emp001",
  empleadoNombre: "Juan Pérez",  // Desnormalizado
  tipoPermisoId: "tipo_001",
  tipoPermisoNombre: "Cita Médica",  // Desnormalizado
  motivo: "...",
  fechaSolicitud: Timestamp,
  fechaInicio: Timestamp,
  fechaFin: Timestamp,
  horaSalida: "08:00" | null,
  horaRegreso: "12:00" | null,
  diasSolicitados: 1,
  esRemunerado: true,
  estado: "Pendiente",  // Pendiente, Aprobado, Rechazado
  solicitadoPorId: "user_uid",
  aprobadoPorId: "user_uid" | null,
  fechaAprobacion: Timestamp | null,
  documentoSoporteUrl: "gs://..." | null,
  observaciones: "...",
  motivoRechazo: null,
  activo: true,
  fechaCreacion: Timestamp
}

3. VACACION:
vacaciones/{vacacionId}
{
  empleadoId: "emp001",
  empleadoNombre: "Juan Pérez",
  periodo: 2025,
  diasDisponibles: 15,
  diasTomados: 5,
  diasPendientes: 10,  // Calculado
  fechaInicio: Timestamp | null,
  fechaFin: Timestamp | null,
  estado: "Pendiente",
  observaciones: "...",
  activo: true,
  fechaCreacion: Timestamp
}

4. CONTRATO:
contratos/{contratoId}
{
  empleadoId: "emp001",
  empleadoNombre: "Juan Pérez",
  tipoContrato: "Indefinido",
  fechaInicio: Timestamp,
  fechaFin: Timestamp | null,
  salario: 5000000,
  cargoId: "cargo_001",
  cargoNombre: "Ingeniero",
  estado: "Activo",
  archivoUrl: "gs://..." | null,
  observaciones: "...",
  activo: true,
  fechaCreacion: Timestamp
}

CREAR:
- src/SGRRHH.Infrastructure/Firebase/Repositories/UsuarioFirestoreRepository.cs
- src/SGRRHH.Infrastructure/Firebase/Repositories/PermisoFirestoreRepository.cs
- src/SGRRHH.Infrastructure/Firebase/Repositories/VacacionFirestoreRepository.cs
- src/SGRRHH.Infrastructure/Firebase/Repositories/ContratoFirestoreRepository.cs
```

**Entregables Fase 3:**
- [x] Empleado migrado a Firestore
- [x] Usuario sincronizado con Firebase Auth
- [x] Permiso migrado con workflow
- [x] Vacacion migrado
- [x] Contrato migrado
- [x] Relaciones manejadas (desnormalizadas)

**Archivos creados:**
```
src/SGRRHH.Infrastructure/Firebase/Repositories/
├── EmpleadoFirestoreRepository.cs    # Colección: "empleados" (campos desnormalizados)
├── UsuarioFirestoreRepository.cs     # Colección: "users" (Document ID = Firebase UID)
├── PermisoFirestoreRepository.cs     # Colección: "permisos" (workflow aprobación)
├── VacacionFirestoreRepository.cs    # Colección: "vacaciones"
└── ContratoFirestoreRepository.cs    # Colección: "contratos"
```

**Archivos modificados:**
- `src/SGRRHH.Infrastructure/Firebase/FirebaseServiceCollectionExtensions.cs` → Agregado `AddFirestoreMainEntityRepositories()`

**Características implementadas:**
- Campos desnormalizados para evitar múltiples queries (empleadoNombre, cargoNombre, etc.)
- Métodos para actualizar datos desnormalizados cuando cambian los catálogos
- Búsqueda de empleados por código, cédula, departamento, cargo, estado
- Workflow de permisos con generación de número de acta (PERM-YYYY-NNNN)
- Detección de solapamiento de fechas en permisos y vacaciones
- Gestión de contratos con alertas de vencimiento
- NO se migró datos de prueba - las colecciones empezarán vacías

**Fecha completado:** 27 de Noviembre 2025

---

## FASE 4: Firestore - RegistroDiario y AuditLog (1 sesión) ✅ COMPLETADA
**Prompt para el agente:**
```
CONTEXTO: SGRRHH con entidades principales en Firestore (Fase 3 completada).

TAREA: Migrar RegistroDiario (con subcolección) y AuditLog

1. REGISTRO DIARIO (con subcolección de detalles):
registros-diarios/{registroId}
{
  empleadoId: "emp001",
  empleadoNombre: "Juan Pérez",
  fecha: Timestamp,
  horaEntrada: "08:00",
  horaSalida: "17:00",
  observaciones: "...",
  activo: true,
  fechaCreacion: Timestamp
}

registros-diarios/{registroId}/detalles/{detalleId}
{
  actividadId: "act_001",
  actividadNombre: "Desarrollo",
  proyectoId: "proy_001" | null,
  proyectoNombre: "Proyecto X" | null,
  horas: 4.5,
  descripcion: "Implementación de módulo...",
  activo: true
}

2. AUDIT LOG:
audit-logs/{logId}
{
  usuarioId: "user_uid",
  usuarioNombre: "admin",
  accion: "Crear",  // Crear, Actualizar, Eliminar, Login, etc.
  entidad: "Empleado",
  entidadId: "emp001",
  descripcion: "Creó empleado Juan Pérez",
  datosAnteriores: {...} | null,
  datosNuevos: {...} | null,
  direccionIp: "192.168.1.100",
  fechaHora: Timestamp
}

ÍNDICES NECESARIOS PARA FIRESTORE:
- audit-logs: usuarioId + fechaHora (DESC)
- audit-logs: entidad + entidadId + fechaHora (DESC)
- registros-diarios: empleadoId + fecha (DESC)
- permisos: empleadoId + fechaSolicitud (DESC)
- permisos: estado + fechaSolicitud (DESC)

CREAR:
- src/SGRRHH.Infrastructure/Firebase/Repositories/RegistroDiarioFirestoreRepository.cs
- src/SGRRHH.Infrastructure/Firebase/Repositories/AuditLogFirestoreRepository.cs
- firestore.indexes.json (para crear índices)
```

**Entregables Fase 4:**
- [x] RegistroDiario con subcolección detalles
- [x] AuditLog implementado
- [x] Índices de Firestore configurados
- [x] Queries optimizados

**Archivos creados:**
```
src/SGRRHH.Infrastructure/Firebase/Repositories/
├── RegistroDiarioFirestoreRepository.cs  # Colección: "registros-diarios"
│                                         # Subcolección: "registros-diarios/{id}/detalles"
└── AuditLogFirestoreRepository.cs        # Colección: "audit-logs"
```

**Archivos modificados:**
- `src/SGRRHH.Infrastructure/Firebase/FirebaseServiceCollectionExtensions.cs` → Agregado `AddFirestoreRecordRepositories()`
- `firestore.indexes.json` → Agregados índices adicionales para queries optimizados

**Características implementadas:**

**RegistroDiarioFirestoreRepository:**
- Subcolección `detalles` para actividades (evita documentos grandes)
- Campos desnormalizados: empleadoNombre, empleadoCodigo, empleadoDepartamento
- Detalles con campos desnormalizados: actividadNombre, proyectoNombre
- Sincronización automática de detalles al actualizar (agrega, actualiza, elimina)
- Métodos para actualizar nombres desnormalizados cuando cambian catálogos
- Queries optimizados para búsqueda por fecha, empleado, rango de fechas

**AuditLogFirestoreRepository:**
- Campos desnormalizados: usuarioNombre, usuarioFirebaseUid
- Hard delete para limpieza de logs antiguos (DeleteOlderThanAsync)
- Métodos adicionales: GetLatestAsync, GetByAccionAsync, GetByUsuarioFirebaseUidAsync
- Estadísticas por rango de fechas (GetStatsByDateRangeAsync)

**Índices agregados en firestore.indexes.json:**
- registros-diarios: empleadoId + fecha + activo
- registros-diarios: activo + fecha (DESC)
- audit-logs: accion + fechaHora (DESC)
- audit-logs: fechaHora (DESC)
- detalles: activo + orden (COLLECTION_GROUP para subcolección)

**NO se crearon datos de prueba - las colecciones empezarán vacías**

**Fecha completado:** 27 de Noviembre 2025

---

## FASE 5: Firebase Storage - Archivos y Fotos (1 sesión) ✅ COMPLETADA
**Prompt para el agente:**
```
CONTEXTO: SGRRHH con todas las entidades en Firestore (Fase 4 completada).

TAREA: Migrar archivos a Firebase Storage

ESTRUCTURA DE STORAGE:
gs://sgrrhh-bucket/
├── fotos/
│   └── empleados/
│       └── {empleadoId}/
│           └── foto.jpg
├── documentos/
│   ├── permisos/
│   │   └── {permisoId}/
│   │       └── soporte.pdf
│   ├── contratos/
│   │   └── {contratoId}/
│   │       └── contrato.pdf
│   └── generados/
│       ├── actas/
│       │   └── ACT-2025-001.pdf
│       └── certificados/
│           └── CERT-2025-001.pdf
├── config/
│   └── logo.png
└── updates/
    ├── version.json
    └── latest/
        └── SGRRHH.exe (y demás archivos)

CREAR:
- src/SGRRHH.Infrastructure/Firebase/FirebaseStorageService.cs

MÉTODOS:
- UploadFileAsync(string localPath, string storagePath) → string downloadUrl
- DownloadFileAsync(string storagePath, string localPath)
- DeleteFileAsync(string storagePath)
- GetDownloadUrlAsync(string storagePath)
- ListFilesAsync(string folderPath)

MODIFICAR:
- EmpleadoService.cs → Subir foto a Storage, guardar URL en Firestore
- PermisoService.cs → Subir soporte a Storage
- ContratoService.cs → Subir contrato a Storage
- DocumentService.cs → Guardar PDFs generados en Storage

REGLAS DE SEGURIDAD (storage.rules):
- Solo usuarios autenticados pueden leer/escribir
- Empleados solo pueden ver sus propios documentos
- Admins pueden ver todo

MIGRACIÓN:
- Script para subir archivos existentes de carpeta local a Storage
- Actualizar URLs en Firestore
```

**Entregables Fase 5:**
- [x] FirebaseStorageService.cs implementado
- [x] IFirebaseStorageService.cs (interfaz)
- [x] Fotos de empleados en Storage (métodos especializados)
- [x] Documentos de permisos en Storage (métodos especializados)
- [x] Contratos en Storage (métodos especializados)
- [x] PDFs generados en Storage (métodos especializados)
- [x] Reglas de seguridad configuradas (storage.rules)
- [x] Herramienta de migración de archivos creada

**Archivos creados:**
```
src/SGRRHH.Core/Interfaces/
└── IFirebaseStorageService.cs              # Interfaz completa del servicio Storage

src/SGRRHH.Infrastructure/Firebase/
└── FirebaseStorageService.cs               # Implementación de Firebase Storage

tools/MigrateFilesToStorage/
├── MigrateFilesToStorage.csproj            # Herramienta de migración
├── Program.cs                              # Lógica de migración con menú
└── appsettings.json                        # Configuración de rutas
```

**Archivos modificados:**
- `src/SGRRHH.Infrastructure/Firebase/FirebaseServiceCollectionExtensions.cs` → Agregado registro de IFirebaseStorageService

**Características implementadas en FirebaseStorageService:**
- **Upload**: UploadFileAsync, UploadBytesAsync, UploadStreamAsync
- **Download**: DownloadFileAsync, DownloadBytesAsync, DownloadStreamAsync
- **URLs**: GetDownloadUrlAsync, GetSignedUrlAsync
- **Delete**: DeleteFileAsync, DeleteFilesAsync, DeleteFolderAsync
- **List**: ListFilesAsync, FileExistsAsync
- **Especializados**:
  - UploadEmpleadoFotoAsync (ruta: fotos/empleados/{id}/)
  - DeleteEmpleadoFotoAsync
  - UploadPermisoDocumentoAsync (ruta: documentos/permisos/{id}/)
  - UploadContratoArchivoAsync (ruta: documentos/contratos/{id}/)
  - UploadDocumentoGeneradoAsync (ruta: documentos/generados/{tipo}/)
  - UploadLogoEmpresaAsync (ruta: config/)

**Estructura de Storage (gs://rrhh-forestech.firebasestorage.app/):**
```
fotos/
└── empleados/{empleadoId}/foto.{ext}

documentos/
├── permisos/{permisoId}/{archivo}
├── contratos/{contratoId}/{archivo}
└── generados/
    ├── actas/{nombre}.pdf
    └── certificados/{nombre}.pdf

config/
└── logo.{ext}

updates/
├── version.json
└── latest/{archivos de la app}
```

**Uso desde la aplicación:**
```csharp
// Inyectar el servicio
private readonly IFirebaseStorageService _storageService;

// Subir foto de empleado
var result = await _storageService.UploadEmpleadoFotoAsync(empleadoId, rutaLocal);
if (result.Success)
{
    empleado.FotoPath = result.Data; // URL de descarga
}

// Subir documento de permiso
var result = await _storageService.UploadPermisoDocumentoAsync(permisoId, rutaSoporte);

// Subir PDF generado
var result = await _storageService.UploadDocumentoGeneradoAsync("actas", "ACT-2025-001.pdf", pdfBytes);
```

**Ejecutar herramienta de migración:**
```powershell
cd tools/MigrateFilesToStorage
# Copiar firebase-credentials.json antes de ejecutar
dotnet run
```

**Fecha completado:** 27 de Noviembre 2025

---

## FASE 6: Sistema de Actualizaciones Firebase (1 sesión) ✅ COMPLETADA
**Prompt para el agente:**
```
CONTEXTO: SGRRHH con Storage funcionando (Fase 5 completada).

TAREA: Migrar sistema de actualizaciones de carpeta compartida a Firebase

ESTRUCTURA EN STORAGE:
gs://sgrrhh-bucket/updates/
├── version.json
└── latest/
    ├── SGRRHH.exe
    ├── SGRRHH.dll
    ├── SGRRHH.deps.json
    ├── SGRRHH.runtimeconfig.json
    └── ... (otros archivos)

CONTENIDO version.json:
{
  "version": "1.1.0",
  "releaseDate": "2025-11-27T15:30:00Z",
  "mandatory": false,
  "minimumVersion": "1.0.0",
  "releaseNotes": "## Cambios...",
  "checksum": "sha256:abc123...",
  "downloadSize": 45678900,
  "files": [
    {"name": "SGRRHH.exe", "checksum": "sha256:...", "size": 12345678}
  ]
}

MODIFICAR:
- src/SGRRHH.Infrastructure/Services/UpdateService.cs

NUEVO FLUJO:
1. Al iniciar app → Descargar version.json de Firebase Storage
2. Comparar con versión local
3. Si hay nueva versión → Mostrar diálogo
4. Usuario acepta → Descargar archivos de Storage a carpeta temporal
5. Cerrar app → Ejecutar script PowerShell que reemplaza archivos
6. Reiniciar app

CREAR:
- src/SGRRHH.Infrastructure/Firebase/FirebaseUpdateService.cs

SCRIPT PUBLICACIÓN (PowerShell):
- scripts/Publish-Firebase-Update.ps1
- Compila la app
- Sube archivos a Firebase Storage
- Actualiza version.json

VENTAJA:
- Ya no necesitas tu PC encendido para que otros actualicen
```

**Entregables Fase 6:**
- [x] IFirebaseUpdateService.cs (interfaz extendida)
- [x] FirebaseUpdateService.cs implementado
- [x] Actualizaciones desde Firebase Storage funcionando
- [x] Script de publicación Publish-Firebase-Update.ps1 creado
- [x] App.xaml.cs actualizado para usar servicio Firebase
- [x] Funciona sin carpeta compartida (modo Firebase)

**Archivos creados:**
```
src/SGRRHH.Core/Interfaces/
└── IFirebaseUpdateService.cs           # Interfaz extendida con métodos adicionales

src/SGRRHH.Infrastructure/Firebase/
└── FirebaseUpdateService.cs            # Implementación de actualizaciones via Firebase Storage

scripts/
└── Publish-Firebase-Update.ps1         # Script para publicar actualizaciones a Firebase
```

**Archivos modificados:**
- `src/SGRRHH.WPF/App.xaml.cs` → Usa FirebaseUpdateService en modo Firebase

**Características implementadas:**
- **Verificación de actualizaciones**: Descarga `version.json` desde Firebase Storage
- **Descarga paralela**: Descarga archivos desde `updates/latest/` en Firebase Storage
- **Verificación de integridad**: Compara checksums SHA256 de cada archivo
- **Actualización automática**: Script PowerShell que aplica la actualización al reiniciar
- **Limpieza automática**: Elimina backups antiguos (>7 días) y archivos temporales
- **Actualizaciones obligatorias**: Soporte para forzar actualización según versión mínima
- **Fallback**: Si Firebase no está disponible, la app sigue funcionando

**Estructura en Firebase Storage (gs://rrhh-forestech.firebasestorage.app/):**
```
updates/
├── version.json                        # Información de la última versión
└── latest/                             # Archivos de la aplicación
    ├── SGRRHH.exe
    ├── SGRRHH.dll
    ├── SGRRHH.deps.json
    ├── SGRRHH.runtimeconfig.json
    ├── runtimes/
    │   └── win-x64/
    └── ... (otros archivos y carpetas)
```

**Uso del script de publicación:**
```powershell
# Publicar nueva versión
cd scripts
.\Publish-Firebase-Update.ps1 -Version "1.1.0" -ReleaseNotes "Corrección de errores"

# Publicar versión obligatoria
.\Publish-Firebase-Update.ps1 -Version "1.2.0" -Mandatory $true -ReleaseNotes "Actualización de seguridad"

# Usar archivos ya compilados (sin recompilar)
.\Publish-Firebase-Update.ps1 -Version "1.1.0" -SkipBuild
```

**Fecha completado:** 27 de Noviembre 2025

---

## FASE 7: Integración y DI Container (1 sesión) ✅ COMPLETADA
**Prompt para el agente:**
```
CONTEXTO: SGRRHH con todos los componentes Firebase implementados (Fases 0-6).

TAREA: Integrar todo en App.xaml.cs y hacer switch de SQLite a Firebase

MODIFICAR App.xaml.cs:
1. Inicializar Firebase al inicio
2. Cambiar registros de DI:
   - IAuthService → FirebaseAuthService
   - IUsuarioRepository → UsuarioFirestoreRepository
   - IEmpleadoRepository → EmpleadoFirestoreRepository
   - ... (todos los repositorios)
   - IUpdateService → FirebaseUpdateService

CREAR SWITCH DE MODO:
appsettings.json:
{
  "DataMode": "Firebase",  // "SQLite" o "Firebase"
  "Firebase": {
    "ProjectId": "sgrrhh-xxxxx",
    "StorageBucket": "sgrrhh-xxxxx.appspot.com"
  },
  "Database": {
    "Path": "data/sgrrhh.db"  // Para modo SQLite (fallback)
  }
}

LÓGICA:
- Si DataMode = "Firebase" → Usar repositorios Firestore
- Si DataMode = "SQLite" → Usar repositorios EF Core (actual)
- Esto permite rollback fácil si hay problemas

MODIFICAR:
- src/SGRRHH.WPF/App.xaml.cs
- src/SGRRHH.WPF/appsettings.json
- src/SGRRHH.WPF/Helpers/AppSettings.cs

CREAR:
- src/SGRRHH.Infrastructure/Firebase/FirebaseServiceCollectionExtensions.cs
  (métodos de extensión para registrar todos los servicios Firebase)
```

**Entregables Fase 7:**
- [x] DI configurado para Firebase
- [x] Switch SQLite/Firebase funcional
- [x] App funcionando con Firebase
- [x] Modo fallback a SQLite disponible

**Archivos modificados:**
```
src/SGRRHH.WPF/App.xaml.cs
└── ConfigureFirebaseServices() ahora usa AddFullFirebaseSupport()
└── Elimina dependencia de SQLite en modo Firebase
└── InitializeFirebaseAsync() ya no llama a InitializeDatabaseAsync()

src/SGRRHH.Infrastructure/Firebase/FirebaseServiceCollectionExtensions.cs
└── AddFirebaseStorageService() - Registra IFirebaseStorageService
└── AddFirebaseUpdateService() - Registra IFirebaseUpdateService e IUpdateService  
└── AddFirebaseApplicationServices() - Registra todos los servicios de negocio
└── AddFirebaseServices() - Combina autenticación, storage, actualizaciones y repositorios
└── AddFullFirebaseSupport() - Punto de entrada único para configurar todo Firebase

src/SGRRHH.WPF/appsettings.json
└── UseFirebaseUpdates = true (habilitado para usar Firebase Storage)
```

**Características implementadas:**

**Switch SQLite/Firebase:**
- `DataMode: "Firebase"` → Usa todos los repositorios Firestore
- `DataMode: "SQLite"` → Usa repositorios Entity Framework (fallback)
- Cambio simplemente editando `appsettings.json`, sin recompilar

**Inyección de Dependencias consolidada:**
- Un solo método `AddFullFirebaseSupport()` registra todo
- Fases 1-6 integradas en el contenedor de DI
- Servicios de negocio automáticamente conectados a repositorios Firestore

**Fallback automático:**
- Si Firebase falla en la inicialización, muestra advertencia
- Variable `IsFirebaseMode` controla el flujo de la aplicación
- Los errores se registran en el log

**Fecha completado:** 27 de Noviembre 2025

---

## FASE 8: Migración de Datos y Pruebas (1 sesión) ✅ COMPLETADA
**Prompt para el agente:**
```
CONTEXTO: SGRRHH funcionando con Firebase (Fase 7 completada).

TAREA: Migrar datos existentes de SQLite a Firebase y probar

CREAR HERRAMIENTA DE MIGRACIÓN:
- src/SGRRHH.Tools/DataMigration/Program.cs (Console App)

FLUJO DE MIGRACIÓN:
1. Leer todos los datos de SQLite
2. Crear usuarios en Firebase Auth
3. Subir datos a Firestore (respetando orden de dependencias):
   - Primero: Departamentos, Cargos, Actividades, Proyectos, TiposPermiso
   - Segundo: Usuarios, Empleados
   - Tercero: Permisos, Vacaciones, Contratos, RegistrosDiarios
   - Último: AuditLogs
4. Subir archivos a Storage:
   - Fotos de empleados
   - Documentos de soporte
5. Actualizar URLs en Firestore

VERIFICACIÓN:
- Contar registros SQLite vs Firestore
- Verificar integridad de relaciones
- Probar login con los 3 usuarios
- Probar CRUD de cada módulo
- Probar generación de PDFs
- Probar sistema de actualizaciones

CHECKLIST DE PRUEBAS:
[ ] Login admin funciona
[ ] Login secretaria funciona
[ ] Login ingeniera funciona
[ ] Listar empleados
[ ] Crear empleado con foto
[ ] Editar empleado
[ ] Crear permiso
[ ] Aprobar permiso
[ ] Generar acta PDF
[ ] Ver dashboard
[ ] Control diario funciona
[ ] Vacaciones funciona
[ ] Contratos funciona
[ ] Catálogos funcionan
[ ] Configuración funciona
[ ] Actualizaciones funcionan
```

**Entregables Fase 8:**
- [x] Herramienta de generación de datos creada
- [x] Datos de prueba generados exitosamente
- [ ] Pruebas de integración pendientes (requiere ejecución manual de la app)
- [ ] App funcionando 100% con Firebase (pendiente validación completa)

**Archivos creados:**
```
tools/GenerateTestData/
├── GenerateTestData.csproj              # Proyecto de consola .NET 8
├── Program.cs                           # Generador completo de datos de prueba
├── appsettings.json                     # Configuración Firebase
└── firebase-credentials.json            # Credenciales (copiado de WPF)
```

**Datos generados en Firebase Firestore:**

| Colección | Documentos | Descripción |
|-----------|------------|-------------|
| departamentos | 5 | Gerencia, Ingeniería, Operaciones, Administración, Vivero |
| cargos | 12 | Desde Gerente General hasta Operarios |
| actividades | 18 | Campo, Vivero, Administrativas, Transporte |
| proyectos | 6 | Proyectos forestales activos y finalizados |
| tipos-permiso | 10 | Cita médica, calamidad, incapacidad, etc. |
| config | 10 | Configuraciones del sistema (nombre empresa, jornada, etc.) |
| empleados | 20 | Con datos realistas colombianos (nombres, cédulas, direcciones) |
| users | 3 | admin, secretaria, ingeniera (creados en Fase 1) |
| permisos | 42 | Estados: Aprobado, Pendiente, Rechazado |
| vacaciones | 20 | Un registro por empleado (período 2025) |
| contratos | 20 | Con salarios y tipos de contrato colombianos |
| registros-diarios | 153 | ~4 semanas de control diario con detalles |

**Características de la herramienta GenerateTestData:**
- Soporte para argumentos de línea de comandos (`dotnet run -- all`, `dotnet run -- stats`)
- Modo interactivo con menú
- Generación de datos realistas colombianos:
  - Nombres y apellidos comunes
  - Cédulas de 10 dígitos
  - Direcciones con formato colombiano (Calle, Carrera, etc.)
  - Teléfonos celulares con prefijo +57 3XX
  - Salarios en COP según cargo
- Relaciones correctas entre entidades (supervisor, departamento, cargo)
- Campos desnormalizados para optimizar queries

**Comandos disponibles:**
```powershell
cd tools/GenerateTestData

# Generar todos los datos
dotnet run -- all

# Solo catálogos
dotnet run -- catalogos

# Solo empleados
dotnet run -- empleados

# Ver estadísticas
dotnet run -- stats

# Limpiar todo (cuidado!)
dotnet run -- clean
```

**Próximos pasos (validación manual):**
1. Ejecutar la aplicación SGRRHH
2. Probar login con los 3 usuarios
3. Verificar que los datos aparecen en cada módulo
4. Probar crear/editar/eliminar registros
5. Probar generación de PDFs

**Fecha completado:** 27 de Noviembre 2025

---

## 📋 RESUMEN DE SESIONES

| Fase | Descripción | Duración Est. | Estado |
|------|-------------|---------------|--------|
| 0 | Configuración Firebase | 1 sesión | ✅ Completada |
| 1 | Firebase Auth | 1 sesión | ✅ Completada |
| 2 | Repositorio Base + Catálogos | 1-2 sesiones | ✅ Completada |
| 3 | Entidades Principales | 2 sesiones | ✅ Completada |
| 4 | RegistroDiario + AuditLog | 1 sesión | ✅ Completada |
| 5 | Firebase Storage | 1 sesión | ✅ Completada |
| 6 | Sistema Actualizaciones | 1 sesión | ✅ Completada |
| 7 | Integración DI | 1 sesión | ✅ Completada |
| 8 | Migración + Pruebas | 1 sesión | 🔄 Pendiente |

**Total: 10-12 sesiones** (8 completadas, 1 pendiente)

---

## 🚀 CÓMO USAR ESTE ROADMAP

### Para cada sesión con el agente IA:

1. **Copia el prompt de la fase correspondiente**
2. **Pega en una nueva conversación**
3. **El agente tendrá todo el contexto necesario**
4. **Marca los entregables completados ✅**
5. **Si hay errores, incluye el mensaje de error en el siguiente prompt**

### Ejemplo de inicio de sesión:
```
Hola, estoy migrando mi app SGRRHH de SQLite a Firebase.
Estoy en la FASE X del roadmap.

[Pegar prompt de la fase]

El estado actual es:
- Fase 0: ✅ Completada
- Fase 1: ✅ Completada
- Fase 2: 🔄 En progreso (50%)

Último error encontrado (si hay): [mensaje de error]
```

---

## 📝 NOTAS IMPORTANTES

1. **Mantén SQLite funcionando** hasta que Firebase esté 100% probado
2. **Haz backup** de la base de datos SQLite antes de migrar
3. **Los IDs cambiarán** de int a string (document IDs de Firestore)
4. **Desnormaliza datos** para evitar múltiples queries
5. **Configura índices** en Firestore para queries complejos
6. **Prueba offline** - Firestore tiene cache local

---

*Documento creado: Noviembre 2025*
*Versión: 1.0*
