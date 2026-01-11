# PROMPT: Crear Páginas UI Blazor para SGRRHH Colombia - Fase 2

## CONTEXTO DEL PROYECTO
Sistema de Gestión de Recursos Humanos (SGRRHH) para Colombia.
- **Stack**: .NET 8, Blazor Server, SQLite + Dapper
- **Arquitectura**: Clean Architecture (Domain, Infrastructure, Server)
- **Estilo UI**: Retro-terminal verde sobre negro, atajos de teclado (F2, F5, ESC)

---

## ⚠️ REGLA CRÍTICA: VERIFICAR MODELOS ANTES DE CREAR UI

**ANTES de escribir cualquier página Razor, DEBES:**
1. Leer el archivo de la entidad en `SGRRHH.Local.Domain/Entities/`
2. Leer el archivo del enum relacionado en `SGRRHH.Local.Domain/Enums/`
3. Leer la interfaz del repositorio/servicio en `SGRRHH.Local.Shared/Interfaces/`
4. Usar **EXACTAMENTE** los nombres de propiedades y métodos que existen

**NUNCA inventes propiedades como:**
- `NetoAPagar` si la entidad tiene `NetoPagar`
- `Valor` si la entidad tiene `ValorCalculado`
- `EmpleadoNombre` si no existe (usar join o propiedad de navegación)
- `Pendiente` si el enum tiene `Borrador` o `Calculada`

---

## MODELOS EXISTENTES (Referencia Exacta)

### Entidad: Nomina
```csharp
// Propiedades principales:
public int EmpleadoId { get; set; }
public Empleado Empleado { get; set; }
public DateTime Periodo { get; set; }  // NO FechaInicio/FechaFin
public DateTime? FechaPago { get; set; }

// Devengos:
public decimal SalarioBase { get; set; }
public decimal AuxilioTransporte { get; set; }
public decimal HorasExtrasDiurnas { get; set; }
public decimal HorasExtrasNocturnas { get; set; }
public decimal HorasNocturnas { get; set; }
public decimal HorasDominicalesFestivos { get; set; }
public decimal Comisiones { get; set; }
public decimal Bonificaciones { get; set; }
public decimal OtrosDevengos { get; set; }  // NO OtrosDevengados
public decimal TotalDevengado { get; } // Propiedad calculada

// Deducciones:
public decimal DeduccionSalud { get; set; }
public decimal DeduccionPension { get; set; }
public decimal RetencionFuente { get; set; }  // NO DeduccionRetencionFuente
public decimal Prestamos { get; set; }
public decimal Embargos { get; set; }
public decimal FondoEmpleados { get; set; }  // NO DeduccionFondoSolidaridad
public decimal OtrasDeducciones { get; set; }
public decimal TotalDeducciones { get; } // Propiedad calculada

// Neto:
public decimal NetoPagar { get; } // NO NetoAPagar

// Estado:
public EstadoNomina Estado { get; set; }
public string? Observaciones { get; set; }
```

### Enum: EstadoNomina
```csharp
Borrador = 1,      // NO "Pendiente"
Calculada = 2,     // NO "Procesado"
Aprobada = 3,
Pagada = 4,        // NO "Pagado"
Contabilizada = 5,
Anulada = 6
```

### Entidad: Prestacion
```csharp
public int EmpleadoId { get; set; }
public Empleado Empleado { get; set; }
public int Periodo { get; set; } // Año como int
public TipoPrestacion Tipo { get; set; }
public DateTime FechaInicio { get; set; }
public DateTime FechaFin { get; set; }
public decimal SalarioBase { get; set; }
public decimal ValorCalculado { get; set; }  // NO "Valor"
public decimal ValorPagado { get; set; }
public DateTime? FechaPago { get; set; }
public EstadoPrestacion Estado { get; set; }
public int? DiasProporcionales { get; set; }  // NO "DiasLiquidados"
public string? Observaciones { get; set; }
```

### Enum: EstadoPrestacion
```csharp
Calculada = 1,    // NO "Calculado"
Aprobada = 2,
Pagada = 3,       // NO "Pagado"
Consignada = 4,   // NO "Consignado"
Anulada = 5
```

### Enum: TipoPrestacion
```csharp
Cesantias = 1,
InteresesCesantias = 2,
PrimaServicios = 3,
Dotacion = 4,
AuxilioTransporte = 5,
Bonificacion = 6
// NO existe "Vacaciones" como TipoPrestacion
```

### Entidad: FestivoColombia
```csharp
public DateTime Fecha { get; set; }
public string Nombre { get; set; }
public string? Descripcion { get; set; }
public bool EsLeyEmiliani { get; set; }
// NO existe "EsRecurrente"
```

### Entidad: ConfiguracionLegal
```csharp
public int Año { get; set; }
public decimal SalarioMinimoMensual { get; set; }
public decimal AuxilioTransporte { get; set; }
public decimal PorcentajeSaludEmpleado { get; set; }
public decimal PorcentajeSaludEmpleador { get; set; }
public decimal PorcentajePensionEmpleado { get; set; }
public decimal PorcentajePensionEmpleador { get; set; }
// ... (verificar archivo para lista completa)
public bool EsVigente { get; set; }
```

---

## INTERFACES DE REPOSITORIOS (Métodos Exactos)

### IPrestacionRepository
```csharp
Task<IEnumerable<Prestacion>> GetByEmpleadoAsync(int empleadoId);
Task<IEnumerable<Prestacion>> GetByPeriodoAsync(int año);
Task<IEnumerable<Prestacion>> GetByTipoAsync(TipoPrestacion tipo, int año);
Task<Prestacion?> GetByEmpleadoTipoPeriodoAsync(int empleadoId, TipoPrestacion tipo, int año);
// NO existe: GetByTipoYAñoAsync, MarcarPagadoAsync, MarcarConsignadoAsync
```

### INominaRepository
```csharp
Task<IEnumerable<Nomina>> GetByEmpleadoAsync(int empleadoId);
Task<IEnumerable<Nomina>> GetByPeriodoAsync(int año, int mes);
Task<Nomina?> GetByEmpleadoPeriodoAsync(int empleadoId, DateTime periodo);
// NO existe: GetByPeriodoAsync(DateTime, DateTime), CambiarEstadoAsync
```

### IFestivoColombiaRepository
```csharp
Task<IEnumerable<FestivoColombia>> GetByAñoAsync(int año);
Task<bool> EsFestivoAsync(DateTime fecha);
Task<int> ContarDiasHabilesAsync(DateTime inicio, DateTime fin);
// NO existe: ExisteFechaAsync
```

---

## INTERFACES DE SERVICIOS (Métodos Exactos)

### ILiquidacionService
```csharp
Task<Result<Prestacion>> CalcularCesantiasAsync(int empleadoId, DateTime fechaInicio, DateTime fechaFin);
Task<Result<Prestacion>> CalcularInteresesCesantiasAsync(int empleadoId, int año);
Task<Result<Prestacion>> CalcularPrimaServiciosAsync(int empleadoId, int año, int semestre);
Task<Result<LiquidacionCompleta>> CalcularLiquidacionDefinitivaAsync(int contratoId, DateTime fechaRetiro, string motivoRetiro);
// NO existe: CalcularPrimaAsync, CalcularVacacionesAsync
```

### INominaService
```csharp
Task<Result<Nomina>> CalcularNominaEmpleadoAsync(int empleadoId, DateTime periodo);
Task<Result<IEnumerable<Nomina>>> GenerarNominaMasivaAsync(int año, int mes);
Task<Result<Nomina>> AprobarNominaAsync(int nominaId, int aprobadoPorId);
// NO existe: GenerarNominasMasivasAsync, ProcesarNominaAsync
```

---

## PATRÓN DE PÁGINA UI (Copiar Estructura)

```razor
@page "/ruta-pagina"
@using SGRRHH.Local.Domain.Entities
@using SGRRHH.Local.Domain.Enums
@using SGRRHH.Local.Shared.Interfaces
@inject IAuthService AuthService
@inject I[Nombre]Repository Repo  // Usar interfaz exacta
@inject NavigationManager Navigation
@inject ILogger<NombrePagina> Logger

<PageTitle>TITULO - SGRRHH</PageTitle>

<div class="page-container">
    <!-- Verificar autenticación -->
    @if (!AuthService.IsAuthenticated)
    {
        <div class="error-message">ACCESO DENEGADO</div>
        return;
    }
    
    <!-- Contenido usando SOLO propiedades que existen -->
    @foreach (var item in items)
    {
        <span>@item.PropiedadQueExiste</span>  // Verificar en entidad
    }
</div>

@code {
    private List<EntidadReal> items = new();
    
    // Usar métodos exactos de la interfaz
    private async Task CargarDatos()
    {
        items = (await Repo.MetodoQueExiste()).ToList();
    }
}
```

---

## PROCESO PASO A PASO

### Para cada página:

1. **LEER** la entidad correspondiente:
   ```
   read_file SGRRHH.Local.Domain/Entities/[Entidad].cs
   ```

2. **LEER** el enum de estado (si aplica):
   ```
   read_file SGRRHH.Local.Domain/Enums/Estado[Entidad].cs
   ```

3. **LEER** la interfaz del repositorio:
   ```
   read_file SGRRHH.Local.Shared/Interfaces/I[Entidad]Repository.cs
   ```

4. **LEER** una página existente como referencia de estilo:
   ```
   read_file SGRRHH.Local.Server/Components/Pages/Empleados.razor
   ```

5. **CREAR** la página usando SOLO propiedades y métodos verificados

6. **COMPILAR** después de cada página:
   ```
   dotnet build SGRRHH.Local.sln
   ```

---

## PÁGINAS A CREAR

1. **ConfiguracionLegal.razor** - CRUD de configuración legal por año
2. **Festivos.razor** - Gestión de festivos colombianos
3. **Nomina.razor** - Generación y gestión de nómina (usar EstadoNomina.Borrador, etc.)
4. **Prestaciones.razor** - Cálculo de cesantías, primas, etc. (usar EstadoPrestacion.Calculada, etc.)

---

## ERRORES COMUNES A EVITAR

| ❌ Incorrecto | ✅ Correcto |
|--------------|-------------|
| `nomina.NetoAPagar` | `nomina.NetoPagar` |
| `EstadoNomina.Pendiente` | `EstadoNomina.Borrador` |
| `EstadoNomina.Procesado` | `EstadoNomina.Calculada` |
| `EstadoPrestacion.Calculado` | `EstadoPrestacion.Calculada` |
| `prestacion.Valor` | `prestacion.ValorCalculado` |
| `prestacion.DiasLiquidados` | `prestacion.DiasProporcionales` |
| `festivo.EsRecurrente` | (No existe, eliminar) |
| `TipoPrestacion.Vacaciones` | (No existe en el enum) |
| `Repo.ExisteFechaAsync()` | (No existe, usar otro método) |
| `@onclick="() => CambiarTab("x")"` | `@onclick='() => CambiarTab("x")'` o usar método |

---

## VALIDACIÓN FINAL

Después de crear cada página, ejecutar:
```bash
dotnet build SGRRHH.Local.sln --verbosity minimal 2>&1 | Select-String "error"
```

Si hay errores, verificar que todas las propiedades y métodos usados existan en los modelos reales.
