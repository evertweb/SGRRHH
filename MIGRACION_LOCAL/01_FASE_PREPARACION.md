# 🏗️ FASE 0: Preparación y Estructura Base

## 📋 Contexto

Estamos migrando SGRRHH desde Firebase a una arquitectura 100% local con SQLite y Blazor Server.

**Proyecto de referencia:** ForestechOil (C:\programajava\forestech-blazor)
**Proyecto actual:** SGRRHH (C:\Users\evert\Documents\rrhh\src)

---

## 🎯 Objetivo de esta Fase

Crear la estructura base del nuevo proyecto SGRRHH.Local con Blazor Server, copiando las entidades y enums existentes.

---

## 📝 PROMPT PARA CLAUDE

```
Necesito que crees la estructura base de un nuevo proyecto Blazor Server llamado SGRRHH.Local en la carpeta C:\Users\evert\Documents\rrhh\SGRRHH.Local.

**ARQUITECTURA REQUERIDA:**

1. **SGRRHH.Local.Domain** (Class Library .NET 8)
   - Copiar todas las entidades de SGRRHH.Core/Entities (Empleado, Permiso, Vacacion, Contrato, etc.)
   - Copiar todos los enums de SGRRHH.Core/Enums
   - Crear carpeta DTOs para futuros objetos de transferencia
   - Crear carpeta Exceptions para excepciones personalizadas

2. **SGRRHH.Local.Shared** (Class Library .NET 8)
   - Crear interfaces base (IRepository<T>, IUnitOfWork)
   - Crear clase Result.cs (patrón Result para manejo de errores)
   - Crear carpeta Configuration para clases de configuración
   - Mover interfaces de repositorio de SGRRHH.Core/Interfaces

3. **SGRRHH.Local.Infrastructure** (Class Library .NET 8)
   - Referencia a Domain y Shared
   - Carpeta Data/ (vacía por ahora, para DapperContext)
   - Carpeta Repositories/ (vacía por ahora)
   - Carpeta Services/ (vacía por ahora)
   - Agregar paquetes NuGet: Dapper, Microsoft.Data.Sqlite, BCrypt.Net-Next

4. **SGRRHH.Local.Server** (Blazor Server App .NET 8)
   - Referencia a Domain, Shared e Infrastructure
   - Configurar para Interactive Server rendering
   - Estructura de carpetas:
     - Components/Layout/
     - Components/Pages/
     - Components/Shared/
     - wwwroot/css/
     - wwwroot/js/
   - Agregar paquete NuGet: QuestPDF

**ARCHIVOS A CREAR:**

1. Solution file: SGRRHH.Local.sln
2. Cada .csproj con las referencias correctas
3. _Imports.razor con los usings comunes
4. Program.cs básico de Blazor Server
5. appsettings.json con configuración de rutas de datos

**CONFIGURACIÓN EN appsettings.json:**

```json
{
  "LocalDatabase": {
    "DatabasePath": "C:\\SGRRHH\\Data\\sgrrhh.db",
    "StoragePath": "C:\\SGRRHH\\Data",
    "BackupPath": "C:\\SGRRHH\\Data\\Backups"
  },
  "Application": {
    "Name": "SGRRHH Local",
    "Version": "1.0.0"
  }
}
```

**IMPORTANTE:**
- NO crear implementaciones todavía, solo la estructura
- Mantener los mismos nombres de propiedades en las entidades
- El Usuario debe tener PasswordHash en lugar de FirebaseUid
- Quitar cualquier referencia a Firebase en las entidades
- Usar int como tipo de ID (no string como en Firestore)

Por favor, muéstrame la estructura completa que vas a crear antes de empezar, y luego crea los archivos uno por uno.
```

---

## ✅ Checklist de Entregables

- [ ] Solución SGRRHH.Local.sln creada
- [ ] Proyecto SGRRHH.Local.Domain con:
  - [ ] Entities/ (19 archivos)
  - [ ] Enums/ (11 archivos)
  - [ ] DTOs/ (carpeta vacía)
  - [ ] Exceptions/ (carpeta vacía)
- [ ] Proyecto SGRRHH.Local.Shared con:
  - [ ] Interfaces/IRepository.cs
  - [ ] Interfaces/IUnitOfWork.cs
  - [ ] Result.cs
  - [ ] Configuration/ (carpeta vacía)
- [ ] Proyecto SGRRHH.Local.Infrastructure con:
  - [ ] Data/ (carpeta vacía)
  - [ ] Repositories/ (carpeta vacía)
  - [ ] Services/ (carpeta vacía)
  - [ ] Paquetes NuGet instalados
- [ ] Proyecto SGRRHH.Local.Server con:
  - [ ] Program.cs configurado
  - [ ] appsettings.json
  - [ ] Estructura de carpetas Components/
  - [ ] wwwroot/css/ y wwwroot/js/

---

## 📚 Archivos de Referencia

### Entidades a copiar (de SGRRHH.Core/Entities):
1. Actividad.cs
2. AuditLog.cs
3. Cargo.cs
4. ChatMessage.cs (opcional, puede omitirse)
5. ConfiguracionSistema.cs
6. Contrato.cs
7. Departamento.cs
8. DetalleActividad.cs
9. DocumentoEmpleado.cs
10. Empleado.cs
11. EntidadBase.cs
12. Permiso.cs
13. Proyecto.cs
14. ProyectoEmpleado.cs
15. RegistroDiario.cs
16. TipoPermiso.cs
17. UserPresence.cs (opcional, puede omitirse)
18. Usuario.cs
19. Vacacion.cs

### Enums a copiar (de SGRRHH.Core/Enums):
1. EstadoCivil.cs
2. EstadoContrato.cs
3. EstadoEmpleado.cs
4. EstadoPermiso.cs
5. EstadoVacacion.cs
6. Genero.cs
7. NivelEducacion.cs
8. RolUsuario.cs
9. TipoContrato.cs
10. TipoDocumentoEmpleado.cs
11. TipoDocumentoPdf.cs

---

## 🔧 Modificaciones Necesarias en Entidades

### Usuario.cs - Cambios requeridos:
```csharp
// QUITAR:
public string? FirebaseUid { get; set; }
public string? EmpleadoFirestoreId { get; set; }

// MANTENER:
public string PasswordHash { get; set; } = string.Empty;
```

### EntidadBase.cs - Cambios requeridos:
```csharp
// ANTES (Firebase):
public string Id { get; set; } = string.Empty;

// DESPUÉS (SQLite):
public int Id { get; set; }
```

### Todas las entidades - Cambios de ID:
```csharp
// ANTES: int? EmpleadoId / string EmpleadoId
// DESPUÉS: int EmpleadoId (sin nullable en relaciones requeridas)
```

---

## 📁 Estructura Final Esperada

```
SGRRHH.Local/
├── SGRRHH.Local.sln
├── SGRRHH.Local.Domain/
│   ├── SGRRHH.Local.Domain.csproj
│   ├── Entities/
│   │   ├── Actividad.cs
│   │   ├── AuditLog.cs
│   │   ├── Cargo.cs
│   │   ├── ConfiguracionSistema.cs
│   │   ├── Contrato.cs
│   │   ├── Departamento.cs
│   │   ├── DetalleActividad.cs
│   │   ├── DocumentoEmpleado.cs
│   │   ├── Empleado.cs
│   │   ├── EntidadBase.cs
│   │   ├── Permiso.cs
│   │   ├── Proyecto.cs
│   │   ├── ProyectoEmpleado.cs
│   │   ├── RegistroDiario.cs
│   │   ├── TipoPermiso.cs
│   │   ├── Usuario.cs
│   │   └── Vacacion.cs
│   ├── Enums/
│   │   └── (11 archivos)
│   ├── DTOs/
│   └── Exceptions/
├── SGRRHH.Local.Shared/
│   ├── SGRRHH.Local.Shared.csproj
│   ├── Interfaces/
│   │   ├── IRepository.cs
│   │   └── IUnitOfWork.cs
│   ├── Configuration/
│   └── Result.cs
├── SGRRHH.Local.Infrastructure/
│   ├── SGRRHH.Local.Infrastructure.csproj
│   ├── Data/
│   ├── Repositories/
│   └── Services/
└── SGRRHH.Local.Server/
    ├── SGRRHH.Local.Server.csproj
    ├── Program.cs
    ├── appsettings.json
    ├── Components/
    │   ├── Layout/
    │   ├── Pages/
    │   └── Shared/
    └── wwwroot/
        ├── css/
        └── js/
```
