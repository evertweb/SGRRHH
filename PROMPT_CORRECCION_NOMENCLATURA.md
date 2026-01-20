# PROMPT: Corrección Masiva de Inconsistencias de Nomenclatura (Inglés → Español)

## Contexto del Problema

El proyecto SGRRHH.Local tiene múltiples archivos que fueron creados con nombres de tipos en **inglés**, pero el resto del proyecto sigue la convención en **español**. Esto causa errores de compilación en cascada.

### Estado Actual del Repositorio
- **Commit base**: `f83e6d7a01c7f6c06c258db602d9414b2f99c4fd`
- **Errores de compilación**: ~56 errores en Server/Components
- **Capas afectadas**: Infrastructure/Services, Server/Components (VacacionesComponents, Proyecto)

---

## Objetivo

Corregir TODOS los archivos que usan tipos/enums/propiedades en **inglés** para que usen los equivalentes en **español** que ya existen en el proyecto.

---

## Mapeo Completo de Tipos (Inglés → Español)

### Enums

| Tipo Inglés (Incorrecto) | Tipo Español (Correcto) | Ubicación |
|----|----|---|
| `ForestProjectRole` | `RolProyectoForestal` | `Domain/Enums/RolProyectoForestal.cs` |
| `VacationStatus` | `EstadoVacacion` | `Domain/Enums/EstadoVacacion.cs` |
| `VacationStatus.Rejected` | `EstadoVacacion.Rechazada` | - |
| `VacationStatus.Cancelled` | `EstadoVacacion.Cancelada` | - |
| `VacationStatus.Approved` | `EstadoVacacion.Aprobada` | - |
| `VacationStatus.Pending` | `EstadoVacacion.Pendiente` | - |

### DTOs

| Tipo Inglés (Incorrecto) | Tipo Español (Correcto) | Ubicación |
|----|----|---|
| `VacationSummary` | `ResumenVacaciones` | `Domain/DTOs/ResumenVacaciones.cs` |

### Interfaces

| Tipo Inglés (Incorrecto) | Tipo Español (Correcto) | Ubicación |
|----|----|---|
| `IEmployeeRepository` | `IEmpleadoRepository` | `Shared/Interfaces/IEmpleadoRepository.cs` |

### Propiedades de Entidades

| Propiedad Inglés | Propiedad Español | Entidad |
|----|----|---|
| `empleado.HireDate` | `empleado.FechaIngreso` | `Empleado` |

---

## Archivos a Corregir

### 1. Infrastructure/Services

#### VacacionService.cs
- [x] `IEmployeeRepository` → `IEmpleadoRepository` (HECHO)
- [x] `VacationStatus.Rejected/Cancelled` → `EstadoVacacion.Rechazada/Cancelada` (HECHO)
- [x] `VacationSummary` → `ResumenVacaciones` (HECHO)
- [x] `HireDate` → `FechaIngreso` (HECHO)

#### ProyectoService.cs
- [x] `ForestProjectRole` → `RolProyectoForestal` (HECHO)
- [x] Using de `Shared.Interfaces` (HECHO)

### 2. Shared/Interfaces

#### IVacacionService.cs
- [x] `VacationSummary` → `ResumenVacaciones` (HECHO)

### 3. Server/Components (PENDIENTE)

Ejecutar los siguientes comandos para identificar todos los archivos afectados:

```powershell
# Buscar todos los usos de tipos en inglés
cd SGRRHH.Local
Get-ChildItem -Recurse -Include *.cs,*.razor | Select-String -Pattern "VacationStatus|VacationSummary|ForestProjectRole|IEmployeeRepository|HireDate" | Group-Object Path | Select-Object Name, Count
```

Archivos probables:
- `Server/Components/Pages/Vacaciones.razor`
- `Server/Components/Pages/VacacionesComponents/*.razor`
- `Server/Components/Proyecto/*.razor`

---

## Fases de Implementación

### Fase 1: Identificar todos los archivos

```powershell
Get-ChildItem -Path SGRRHH.Local -Recurse -Include *.cs,*.razor | 
    Select-String -Pattern "VacationStatus|VacationSummary|ForestProjectRole|HireDate" |
    Group-Object Path |
    Format-Table Name, Count
```

### Fase 2: Aplicar correcciones con búsqueda/reemplazo

Para cada archivo identificado:
1. `VacationStatus.Pending` → `EstadoVacacion.Pendiente`
2. `VacationStatus.Approved` → `EstadoVacacion.Aprobada`
3. `VacationStatus.Rejected` → `EstadoVacacion.Rechazada`
4. `VacationStatus.Cancelled` → `EstadoVacacion.Cancelada`
5. `VacationStatus` → `EstadoVacacion`
6. `VacationSummary` → `ResumenVacaciones`
7. `ForestProjectRole` → `RolProyectoForestal`
8. `.HireDate` → `.FechaIngreso`

### Fase 3: Compilar y verificar

```powershell
dotnet build -v:m /bl:build.binlog 2>&1 | Tee-Object build.log
```

### Fase 4: Confirmar correcciones originales

Después de resolver los errores de nomenclatura, verificar que las correcciones originales siguen aplicadas:

1. **Festivos.razor**: Líneas 12-201 de CSS suelto deben seguir eliminadas
2. **ControlDiarioWizard.razor**: Línea 414 debe usar `GetAllActiveWithRelationsAsync()`

---

## Verificación Final

- [ ] 0 errores de compilación
- [ ] 0 warnings críticos (pueden haber warnings menores)
- [ ] Festivos.razor: CSS no se renderiza como texto
- [ ] ControlDiarioWizard.razor: Empleados muestran su departamento
