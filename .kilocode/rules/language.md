---
trigger: always_on
description: Apply when writing any code, comments, or UI text. Everything must be in Spanish: variables, methods, properties, comments, messages.
---

# Idioma: Español - SGRRHH

## Regla Principal

> [!IMPORTANT]
> **TODO** el código, comentarios, UI y mensajes deben estar en **español**.

Esta regla se aplica a:
- ✅ Nombres de variables
- ✅ Nombres de métodos
- ✅ Nombres de propiedades
- ✅ Nombres de clases y archivos
- ✅ Comentarios en código
- ✅ Mensajes de error
- ✅ Texto de UI (botones, labels, títulos)
- ✅ Logs y mensajes de consola
- ✅ Documentación

## Nomenclatura

### Variables y Campos Privados

**Formato:** `camelCase`

```csharp
// ✅ CORRECTO
private bool mostrarFormulario = false;
private string mensajeError = "";
private List<EmpleadoDto> empleados = new();
private int cantidadSeleccionada = 0;

// ❌ INCORRECTO
private bool showForm = false;
private string errorMessage = "";
private List<EmpleadoDto> employees = new();
private int selectedCount = 0;
```

### Métodos

**Formato:** `PascalCase`

```csharp
// ✅ CORRECTO
private async Task CargarEmpleadosAsync()
private void MostrarModal()
private bool ValidarFormulario()
private decimal CalcularTotal()

// ❌ INCORRECTO
private async Task LoadEmployeesAsync()
private void ShowModal()
private bool ValidateForm()
private decimal CalculateTotal()
```

### Propiedades y Parámetros

**Formato:** `PascalCase`

```csharp
// ✅ CORRECTO
public int EmpleadoId { get; set; }
public string NombreCompleto { get; set; }
public DateTime FechaIngreso { get; set; }

[Parameter] public EventCallback OnGuardado { get; set; }
[Parameter] public bool MostrarInactivos { get; set; }

// ❌ INCORRECTO
public int EmployeeId { get; set; }
public string FullName { get; set; }
public DateTime HireDate { get; set; }

[Parameter] public EventCallback OnSave { get; set; }
[Parameter] public bool ShowInactive { get; set; }
```

### Clases y Archivos

**Formato:** `PascalCase`

```csharp
// ✅ CORRECTO
public class Empleado { }
public class ContratoLaboral { }
public class SolicitudVacacion { }
public interface IEmpleadoRepository { }

// Archivos
Empleado.cs
ContratoLaboral.cs
SolicitudVacacion.cs

// ❌ INCORRECTO
public class Employee { }
public class LaborContract { }
public class VacationRequest { }
public interface IEmployeeRepository { }

// Archivos
Employee.cs
LaborContract.cs
VacationRequest.cs
```

## Comentarios

**SIEMPRE en español:**

```csharp
// ✅ CORRECTO
// Validar que el empleado esté activo antes de guardar
if (empleado.Estado == EstadoEmpleado.Activo)
{
    // Aplicar cálculo de prestaciones sociales
    var prestaciones = CalcularPrestaciones(empleado);
}

/* 
 * Este método calcula las vacaciones disponibles
 * basado en los días acumulados y disfrutados
 */
private int CalcularVacacionesDisponibles(int acumulados, int disfrutados)
{
    return acumulados - disfrutados;
}
```

```csharp
// ❌ INCORRECTO
// Validate that employee is active before saving
if (employee.Status == EmployeeStatus.Active)
{
    // Apply social benefits calculation
    var benefits = CalculateBenefits(employee);
}
```

## Mensajes de Error

```csharp
// ✅ CORRECTO
throw new InvalidOperationException("El empleado ya tiene una solicitud de vacaciones activa");
throw new ArgumentNullException(nameof(empleadoId), "El ID del empleado es requerido");

mensajeError = "No se pudo guardar el registro. Verifique los datos ingresados.";

// ❌ INCORRECTO
throw new InvalidOperationException("Employee already has an active vacation request");
throw new ArgumentNullException(nameof(employeeId), "Employee ID is required");

errorMessage = "Could not save record. Please check the entered data.";
```

## UI: Blazor Components

```razor
@* ✅ CORRECTO *@
<h1 class="page-title">EMPLEADOS</h1>

<button class="btn-primary" @onclick="Guardar">GUARDAR [F9]</button>
<button class="btn-action" @onclick="Cancelar">CANCELAR [ESC]</button>

<label class="campo-label campo-requerido">Nombre Completo</label>
<input type="text" class="campo-input" @bind="modelo.NombreCompleto" />

@if (!string.IsNullOrEmpty(mensajeError))
{
    <div class="error-block">@mensajeError</div>
}
```

```razor
@* ❌ INCORRECTO *@
<h1 class="page-title">EMPLOYEES</h1>

<button class="btn-primary" @onclick="Save">SAVE [F9]</button>
<button class="btn-action" @onclick="Cancel">CANCEL [ESC]</button>

<label class="field-label required">Full Name</label>
<input type="text" class="field-input" @bind="model.FullName" />

@if (!string.IsNullOrEmpty(errorMessage))
{
    <div class="error-block">@errorMessage</div>
}
```

## Logs

```csharp
// ✅ CORRECTO
_logger.LogInformation("Empleado {EmpleadoId} creado exitosamente", id);
_logger.LogWarning("No se encontraron resultados para la búsqueda '{Termino}'", termino);
_logger.LogError(ex, "Error al guardar el contrato del empleado {EmpleadoId}", empleadoId);

// ❌ INCORRECTO
_logger.LogInformation("Employee {EmployeeId} created successfully", id);
_logger.LogWarning("No results found for search '{Term}'", term);
_logger.LogError(ex, "Error saving contract for employee {EmployeeId}", employeeId);
```

## Enums

```csharp
// ✅ CORRECTO
public enum EstadoEmpleado
{
    PendienteAprobacion,
    Activo,
    EnVacaciones,
    EnLicencia,
    Suspendido,
    Retirado
}

// ❌ INCORRECTO
public enum EmployeeStatus
{
    PendingApproval,
    Active,
    OnVacation,
    OnLeave,
    Suspended,
    Terminated
}
```

## DTOs y Propiedades

```csharp
// ✅ CORRECTO
public class EmpleadoDto
{
    public int Id { get; set; }
    public string Nombres { get; set; } = string.Empty;
    public string Apellidos { get; set; } = string.Empty;
    public string NumeroDocumento { get; set; } = string.Empty;
    public DateTime FechaNacimiento { get; set; }
    public string Cargo { get; set; } = string.Empty;
}

// ❌ INCORRECTO
public class EmployeeDto
{
    public int Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string DocumentNumber { get; set; } = string.Empty;
    public DateTime BirthDate { get; set; }
    public string Position { get; set; } = string.Empty;
}
```

## Excepciones

**NO hay excepciones** para esta regla. Incluso términos técnicos deben adaptarse:

| Inglés | Español |
|--------|---------|
| Repository | Repositorio |
| Service | Servicio |
| Controller | Controlador |
| Handler | Manejador |
| Manager | Gestor |
| Provider | Proveedor |
| Factory | Fábrica |
| Builder | Constructor |

```csharp
// ✅ CORRECTO
public class EmpleadoRepositorio : IEmpleadoRepositorio { }
public class LiquidacionServicio { }
public class AutenticacionManejador { }

// ❌ INCORRECTO
public class EmpleadoRepository : IEmpleadoRepository { }
public class LiquidacionService { }
public class AuthenticationHandler { }
```

## Abreviaturas Permitidas

Algunas abreviaturas técnicas son aceptables:

- `Id` (en lugar de "Identificador")
- `Dto` (Data Transfer Object)
- `SQL` (no traducir)
- `HTTP`, `HTTPS`
- `JSON`, `XML`
- `API`

---

**Última actualización:** 2026-01-28
