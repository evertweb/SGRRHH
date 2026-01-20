# Prompt: Continuación de Refactorización de Nomenclatura (Inglés -> Español)

## Contexto
Estamos en medio de una refactorización masiva para eliminar inconsistencias de nomenclatura (tipos en inglés vs dominio en español). 
Ya se han corregido los DTOs principales (`ResultadoImpresionDto`, `EmployeeExportOptions`) y completado la mayor parte de `ReportListadoEmpleados.razor` y `ReportPermisos.razor`.

## Estado Actual
El proyecto tiene **~50 errores de compilación** pendaientes.

### Archivos Críticos a Corregir:

1.  **`SGRRHH.Local.Server/Components/Reports/ReportVacaciones.razor`**
    *   **Errores conocidos**:
        *   Uso de `Departments` -> cambiar a `Departamentos`.
        *   Uso de `FullName` -> cambiar a `NombreCompleto`.
        *   Uso de `DocumentoNumero` o `IdNumber` -> cambiar a `Cedula`.
        *   Revisar cualquier lógica de filtrado/ordenamiento que siga usando nombres en inglés.

2.  **`SGRRHH.Local.Server/Components/Reports/ReportAsistencia.razor`** (y otros reportes no verificados)
    *   Probablemente tiene los mismos problemas de `FullName`, `Departments`, `Employees` que ya corregimos en los otros reportes.

3.  **`SGRRHH.Local.Infrastructure/Repositories/ProyectoEmpleadoRepository.cs`**
    *   Verificar referencias a `ForestProjectRole` -> debe ser `RolProyectoForestal`.

4.  **`SGRRHH.Local.Server/Components/Proyecto/ProyectoEncabezado.razor`**
    *   Verificar referencias a `ForestProjectType` -> debe ser `TipoProyectoForestal`.

## Instrucciones para el Agente
1.  **NO INVENTAR TIPOS**. Los tipos en español YA EXISTEN en `SGRRHH.Local.Domain`.
    *   `Empleado` (no Employee)
    *   `Departamento` (no Department)
    *   `Cargo` (no Position)
    *   `EstadoVacacion` (no VacationStatus)
    *   `RolProyectoForestal` (no ForestProjectRole)
    *   `ResultadoImpresionDto` (no PrintJobResultDto)

2.  **Pasos de Ejecución**:
    *   Ejecuta `dotnet build -v:m /bl:build.binlog`.
    *   Lee el log de errores (`build.log`).
    *   Ve archivo por archivo corrigiendo los nombres de propiedades y tipos.
    *   Presta especial atención a propiedades anidadas en Razor (ej: `emp.Department.Name` -> `emp.Departamento.Nombre`).

3.  **Meta**: Lograr 0 errores de compilación.
