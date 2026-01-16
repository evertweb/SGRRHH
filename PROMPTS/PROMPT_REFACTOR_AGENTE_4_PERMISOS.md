# 🔧 AGENTE 4: REFACTORIZACIÓN - Permisos.razor

## 📋 INFORMACIÓN DEL COMPONENTE

**Componente Objetivo:** `SGRRHH.Local\SGRRHH.Local.Server\Components\Pages\Permisos.razor`  
**Tamaño Actual:** 1,513 líneas (65 KB)  
**Complejidad:** ⚠️ MUY ALTA  
**Prioridad:** 🟠 ALTA

### Descripción
Sistema completo de gestión de permisos laborales con:
- Tabla de permisos con múltiples filtros
- Formulario de creación/edición de permisos
- Sistema de aprobación de permisos
- Gestión de seguimiento de permisos
- Cálculo de días de permiso
- Integración con calendario
- Reportes de permisos
- Estados: Pendiente, Aprobado, Rechazado, Compensado

### Archivos Exclusivos de Este Agente (NO TOCAR POR OTROS)
```
✅ ARCHIVOS PERMITIDOS PARA MODIFICAR/CREAR:
- SGRRHH.Local\SGRRHH.Local.Server\Components\Pages\Permisos.razor
- SGRRHH.Local\SGRRHH.Local.Server\Components\Permisos\PermisosHeader.razor (NUEVO)
- SGRRHH.Local\SGRRHH.Local.Server\Components\Permisos\PermisosFilters.razor (NUEVO)
- SGRRHH.Local\SGRRHH.Local.Server\Components\Permisos\PermisosTable.razor (NUEVO)
- SGRRHH.Local\SGRRHH.Local.Server\Components\Permisos\PermisoFormModal.razor (NUEVO)
- SGRRHH.Local\SGRRHH.Local.Server\Components\Permisos\PermisoAprobacionModal.razor (NUEVO)
- SGRRHH.Local\SGRRHH.Local.Server\Components\Permisos\PermisoSeguimientoPanel.razor (NUEVO)
- SGRRHH.Local\SGRRHH.Local.Server\Components\Permisos\PermisoCalculadora.razor (NUEVO)

❌ ARCHIVOS PROHIBIDOS:
- EmpleadoOnboarding.razor (Agente 1)
- ScannerModal.razor (Agente 2)
- EmpleadoExpediente.razor (Agente 3)
- ControlDiario.razor (Agente 5)
```

---

## 🎯 OBJETIVOS DE REFACTORIZACIÓN

### Metas Principales
1. ✅ Reducir `Permisos.razor` de **1,513 líneas → ~250 líneas**
2. ✅ Extraer **7 componentes especializados**
3. ✅ Crear servicio `PermisoCalculationService` para lógica de cálculo
4. ✅ Consolidar validaciones de permisos
5. ✅ Mejorar sistema de filtros (más performante)
6. ✅ Mantener 100% funcionalidad de aprobación/rechazo
7. ✅ Compilación sin errores

### KPIs de Éxito
- **Reducción:** Mínimo 83%
- **Componentes:** 7 nuevos + 1 servicio
- **Redundancias eliminadas:** Mínimo 5
- **Tests:** 0 errores
- **Funcionalidad:** 100%

---

## 📊 FASE 1: INVESTIGACIÓN (2-3 horas)

### 1.1 Análisis Estructural

**Mapear:**
- Header con indicadores (líneas ~20-50)
- Sistema de filtros (líneas ~50-150)
- Tabla de permisos (líneas ~150-400)
- Modal de formulario (líneas ~400-800)
- Modal de aprobación (líneas ~800-1000)
- Panel de seguimiento (líneas ~1000-1200)
- Lógica de cálculo de días (líneas ~1200-1400)

**Deliverable 1.1:** `ANALISIS_PERMISOS.md`

### 1.2 Análisis de Lógica de Negocio

**Identificar reglas de negocio:**
1. Cálculo de días de permiso (laborables vs calendario)
2. Validación de fechas (no puede ser en el pasado)
3. Validación de solapamiento con otros permisos
4. Flujo de aprobación (Operador → Aprobador → Aprobado)
5. Cálculo de compensación
6. Descuento en nómina

**Pregunta clave:** ¿Qué lógica debe ir en un servicio vs componente?

**Deliverable 1.2:** Sección "Reglas de Negocio" en `ANALISIS_PERMISOS.md`:
```markdown
## Reglas de Negocio

### Cálculo de Días
- Debe considerar festivos
- Debe considerar fines de semana
- Debe calcular días laborables
→ MOVER A: PermisoCalculationService

### Validación de Fechas
- No permite fechas pasadas
- No permite solapamiento
→ MOVER A: PermisoValidationService

### Flujo de Aprobación
- Operador crea → estado Pendiente
- Aprobador revisa → Aprobado/Rechazado
→ MANTENER EN: Componentes (UI)
```

### 1.3 Búsqueda de Redundancias

**Investigar:**
1. **Cálculo de días:** ¿Se repite en múltiples lugares?
2. **Validación de fechas:** ¿Código duplicado?
3. **Formateo de fechas:** ¿Múltiples formas de formatear?
4. **Filtrado de permisos:** ¿Lógica repetida?

**Tareas:**
```bash
# Buscar cálculos de días
grep -n "DateTime\|AddDays\|DayOfWeek" Permisos.razor

# Buscar validaciones
grep -n "Validate\|IsValid" Permisos.razor

# Buscar formateos
grep -n "ToString\|Format" Permisos.razor
```

**Deliverable 1.3:** Lista de redundancias con líneas

### 1.4 Revisión de Skills

```bash
.cursor/skills/blazor-component/SKILL.md
.cursor/skills/hospital-ui-style/SKILL.md
.cursor/skills/build-and-verify/SKILL.md
```

---

## 🗺️ FASE 2: PLANEACIÓN (2-3 horas)

### 2.1 Arquitectura de Componentes

```
Permisos.razor (Orquestador - ~250 líneas)
│
├─ <PermisosHeader 
│     TotalPermisos="@permisos.Count"
│     PendientesAprobacion="@CountPendientes()"
│     OnNuevoPermiso="@AbrirFormulario" />
│
├─ <PermisosFilters 
│     @bind-FechaInicio="fechaInicio"
│     @bind-FechaFin="fechaFin"
│     @bind-Estado="estadoFiltro"
│     @bind-EmpleadoId="empleadoFiltro"
│     OnFilterChanged="@LoadPermisos" />
│
├─ <PermisosTable 
│     Permisos="@permisosFiltrados"
│     OnEdit="@EditarPermiso"
│     OnApprove="@AbrirAprobacion"
│     OnReject="@RechazarPermiso"
│     OnDelete="@EliminarPermiso"
│     OnViewSeguimiento="@AbrirSeguimiento"
│     UsuarioActual="@AuthService.CurrentUser" />
│
├─ <PermisoFormModal 
│     @ref="formularioModal"
│     PermisoId="@permisoSeleccionadoId"
│     OnSave="@HandleSavePermiso"
│     CalculationService="@calculationService" />
│
├─ <PermisoAprobacionModal 
│     @ref="aprobacionModal"
│     Permiso="@permisoSeleccionado"
│     OnApprove="@HandleApprovePermiso"
│     OnReject="@HandleRejectPermiso" />
│
└─ <PermisoSeguimientoPanel 
      @ref="seguimientoPanel"
      PermisoId="@permisoSeleccionadoId"
      OnClose="@CerrarSeguimiento" />
```

**Deliverable 2.1:** `PLAN_ARQUITECTURA_PERMISOS.md` con diagrama

### 2.2 Diseño de Servicios

#### PermisoCalculationService
```csharp
// SGRRHH.Local/SGRRHH.Local.Domain/Services/PermisoCalculationService.cs
namespace SGRRHH.Local.Domain.Services;

public interface IPermisoCalculationService
{
    Task<int> CalcularDiasLaborablesAsync(DateTime fechaInicio, DateTime fechaFin);
    Task<decimal> CalcularMontoDescuentoAsync(int empleadoId, int diasPermiso);
    Task<bool> TieneSolapamientoAsync(int empleadoId, DateTime inicio, DateTime fin, int? permisoIdActual = null);
    Task<List<DateTime>> ObtenerDiasFestivosEnRangoAsync(DateTime inicio, DateTime fin);
    int ContarDiasSemanaEnRango(DateTime inicio, DateTime fin, DayOfWeek diaSemana);
}

public class PermisoCalculationService : IPermisoCalculationService
{
    private readonly IFestivoRepository _festivoRepo;
    private readonly IPermisoRepository _permisoRepo;
    private readonly IEmpleadoRepository _empleadoRepo;
    
    public PermisoCalculationService(
        IFestivoRepository festivoRepo,
        IPermisoRepository permisoRepo,
        IEmpleadoRepository empleadoRepo)
    {
        _festivoRepo = festivoRepo;
        _permisoRepo = permisoRepo;
        _empleadoRepo = empleadoRepo;
    }
    
    public async Task<int> CalcularDiasLaborablesAsync(DateTime fechaInicio, DateTime fechaFin)
    {
        var festivos = await ObtenerDiasFestivosEnRangoAsync(fechaInicio, fechaFin);
        int diasLaborables = 0;
        
        for (var fecha = fechaInicio.Date; fecha <= fechaFin.Date; fecha = fecha.AddDays(1))
        {
            // Excluir sábados, domingos y festivos
            if (fecha.DayOfWeek != DayOfWeek.Saturday &&
                fecha.DayOfWeek != DayOfWeek.Sunday &&
                !festivos.Contains(fecha.Date))
            {
                diasLaborables++;
            }
        }
        
        return diasLaborables;
    }
    
    public async Task<decimal> CalcularMontoDescuentoAsync(int empleadoId, int diasPermiso)
    {
        var empleado = await _empleadoRepo.GetByIdAsync(empleadoId);
        if (empleado?.SalarioBase == null) return 0;
        
        // Calcular salario diario: salario mensual / 30
        var salarioDiario = empleado.SalarioBase.Value / 30m;
        return salarioDiario * diasPermiso;
    }
    
    public async Task<bool> TieneSolapamientoAsync(
        int empleadoId, 
        DateTime inicio, 
        DateTime fin, 
        int? permisoIdActual = null)
    {
        var permisosEmpleado = await _permisoRepo.GetByEmpleadoIdAsync(empleadoId);
        
        return permisosEmpleado
            .Where(p => p.Id != permisoIdActual) // Excluir el permiso actual si se está editando
            .Where(p => p.Estado == EstadoPermiso.Aprobado) // Solo considerar aprobados
            .Any(p => 
                (inicio >= p.FechaInicio && inicio <= p.FechaFin) || // Inicio dentro de otro permiso
                (fin >= p.FechaInicio && fin <= p.FechaFin) ||       // Fin dentro de otro permiso
                (inicio <= p.FechaInicio && fin >= p.FechaFin));     // Envuelve otro permiso
    }
    
    public async Task<List<DateTime>> ObtenerDiasFestivosEnRangoAsync(DateTime inicio, DateTime fin)
    {
        var festivos = await _festivoRepo.GetByRangoAsync(inicio, fin);
        return festivos.Select(f => f.Fecha.Date).ToList();
    }
    
    public int ContarDiasSemanaEnRango(DateTime inicio, DateTime fin, DayOfWeek diaSemana)
    {
        int count = 0;
        for (var fecha = inicio.Date; fecha <= fin.Date; fecha = fecha.AddDays(1))
        {
            if (fecha.DayOfWeek == diaSemana)
                count++;
        }
        return count;
    }
}
```

**Deliverable 2.2:** Especificación completa de servicios

### 2.3 Plan de Consolidación

**Redundancias a eliminar:**

1. **Cálculo de días laborables:**
   - ❌ ANTES: Código duplicado en 3 lugares (formulario, tabla, reporte)
   - ✅ DESPUÉS: `PermisoCalculationService.CalcularDiasLaborablesAsync()`

2. **Validación de solapamiento:**
   - ❌ ANTES: Lógica repetida en formulario y edición
   - ✅ DESPUÉS: `PermisoCalculationService.TieneSolapamientoAsync()`

3. **Formato de fechas:**
   - ❌ ANTES: `ToString("dd/MM/yyyy")` en múltiples lugares
   - ✅ DESPUÉS: Helper `DateHelpers.FormatShortDate(DateTime date)`

4. **Obtención de días festivos:**
   - ❌ ANTES: Queries duplicadas a tabla festivos
   - ✅ DESPUÉS: `PermisoCalculationService.ObtenerDiasFestivosEnRangoAsync()`

5. **Cálculo de descuento:**
   - ❌ ANTES: Fórmula repetida
   - ✅ DESPUÉS: `PermisoCalculationService.CalcularMontoDescuentoAsync()`

**Deliverable 2.3:** Tabla de consolidaciones en `PLAN_ARQUITECTURA_PERMISOS.md`

### 2.4 Plan de Pruebas

**Checklist:**
```markdown
## Funcionalidad Básica
- [ ] Compilación: 0 errores
- [ ] Cargar lista de permisos
- [ ] Filtrar por fechas
- [ ] Filtrar por estado
- [ ] Filtrar por empleado

## Creación y Edición
- [ ] Crear nuevo permiso
- [ ] Calcular días laborables correctamente
- [ ] Validar solapamiento
- [ ] Validar fechas pasadas (rechazar)
- [ ] Guardar permiso correctamente

## Aprobación y Rechazo
- [ ] Aprobar permiso (rol Aprobador)
- [ ] Rechazar permiso (rol Aprobador)
- [ ] Operador NO puede aprobar (validar permisos)
- [ ] Notificación de cambio de estado

## Seguimiento
- [ ] Ver seguimiento de permiso
- [ ] Actualizar seguimiento
- [ ] Ver historial de cambios

## Reportes y Cálculos
- [ ] Generar reporte de permisos
- [ ] Calcular descuento en nómina correcto
- [ ] Calcular días de compensación
- [ ] Exportar a PDF/Excel
```

**Deliverable 2.4:** `TEST_PLAN_PERMISOS.md`

---

## ⚙️ FASE 3: EJECUCIÓN CONTROLADA (10-12 horas)

### 3.1 Preparación

```bash
mkdir -p SGRRHH.Local/SGRRHH.Local.Server/Components/Permisos
cp Permisos.razor Permisos.razor.BACKUP
dotnet build SGRRHH.Local/SGRRHH.Local.Server/SGRRHH.Local.Server.csproj
```

### 3.2 Iteración 1: Crear Servicios

**Paso 1: IPermisoCalculationService.cs**
```csharp
// (Ver diseño en sección 2.2)
```

**Paso 2: PermisoCalculationService.cs**
```csharp
// (Ver implementación en sección 2.2)
```

**Paso 3: Registrar en Program.cs**
```csharp
builder.Services.AddScoped<IPermisoCalculationService, PermisoCalculationService>();
```

**✅ CHECKPOINT 1:** Compilar

### 3.3 Iteración 2: Componentes de UI

#### Paso 4: PermisosHeader.razor
```razor
<div class="permisos-header">
    <h1 class="permisos-title">SISTEMA DE GESTIÓN DE PERMISOS</h1>
    <div class="permisos-stats">
        <div class="stat-card">
            <span class="stat-value">@TotalPermisos</span>
            <span class="stat-label">Total Permisos</span>
        </div>
        <div class="stat-card highlight">
            <span class="stat-value">@PendientesAprobacion</span>
            <span class="stat-label">Pendientes Aprobación</span>
        </div>
    </div>
    <div class="permisos-actions">
        <button @onclick="OnNuevoPermiso" class="hospital-btn hospital-btn-primary">
            + NUEVO PERMISO
        </button>
    </div>
</div>

@code {
    [Parameter] public int TotalPermisos { get; set; }
    [Parameter] public int PendientesAprobacion { get; set; }
    [Parameter] public EventCallback OnNuevoPermiso { get; set; }
}
```

**✅ CHECKPOINT 2:** Compilar

#### Paso 5: PermisosFilters.razor
```razor
<div class="permisos-filters">
    <div class="filter-group">
        <label>Fecha Inicio:</label>
        <input type="date" @bind="fechaInicioLocal" @bind:after="NotifyChange" class="hospital-input" />
    </div>
    <div class="filter-group">
        <label>Fecha Fin:</label>
        <input type="date" @bind="fechaFinLocal" @bind:after="NotifyChange" class="hospital-input" />
    </div>
    <div class="filter-group">
        <label>Estado:</label>
        <select @bind="estadoLocal" @bind:after="NotifyChange" class="hospital-input">
            <option value="">Todos</option>
            <option value="@EstadoPermiso.Pendiente">Pendiente</option>
            <option value="@EstadoPermiso.Aprobado">Aprobado</option>
            <option value="@EstadoPermiso.Rechazado">Rechazado</option>
            <option value="@EstadoPermiso.Compensado">Compensado</option>
        </select>
    </div>
    <div class="filter-group">
        <label>Empleado:</label>
        <EmpleadoSelector @bind-EmpleadoId="empleadoIdLocal" OnChange="NotifyChange" />
    </div>
    <button @onclick="Limpiar" class="hospital-btn hospital-btn-secondary">
        LIMPIAR FILTROS
    </button>
</div>

@code {
    [Parameter] public DateTime? FechaInicio { get; set; }
    [Parameter] public EventCallback<DateTime?> FechaInicioChanged { get; set; }
    
    [Parameter] public DateTime? FechaFin { get; set; }
    [Parameter] public EventCallback<DateTime?> FechaFinChanged { get; set; }
    
    [Parameter] public EstadoPermiso? Estado { get; set; }
    [Parameter] public EventCallback<EstadoPermiso?> EstadoChanged { get; set; }
    
    [Parameter] public int? EmpleadoId { get; set; }
    [Parameter] public EventCallback<int?> EmpleadoIdChanged { get; set; }
    
    [Parameter] public EventCallback OnFilterChanged { get; set; }
    
    private DateTime? fechaInicioLocal;
    private DateTime? fechaFinLocal;
    private EstadoPermiso? estadoLocal;
    private int? empleadoIdLocal;
    
    protected override void OnParametersSet()
    {
        fechaInicioLocal = FechaInicio;
        fechaFinLocal = FechaFin;
        estadoLocal = Estado;
        empleadoIdLocal = EmpleadoId;
    }
    
    private async Task NotifyChange()
    {
        await FechaInicioChanged.InvokeAsync(fechaInicioLocal);
        await FechaFinChanged.InvokeAsync(fechaFinLocal);
        await EstadoChanged.InvokeAsync(estadoLocal);
        await EmpleadoIdChanged.InvokeAsync(empleadoIdLocal);
        await OnFilterChanged.InvokeAsync();
    }
    
    private async Task Limpiar()
    {
        fechaInicioLocal = null;
        fechaFinLocal = null;
        estadoLocal = null;
        empleadoIdLocal = null;
        await NotifyChange();
    }
}
```

**✅ CHECKPOINT 3:** Compilar

#### Paso 6-9: Crear componentes restantes
- PermisosTable.razor
- PermisoFormModal.razor (con integración de PermisoCalculationService)
- PermisoAprobacionModal.razor
- PermisoSeguimientoPanel.razor

**✅ CHECKPOINTS 4-7:** Compilar después de cada uno

### 3.4 Iteración 3: Refactorizar Permisos.razor

```razor
@page "/permisos"
@page "/permisos/{PermisoIdParam:int?}"
@using SGRRHH.Local.Domain.Entities
@using SGRRHH.Local.Domain.Enums
@using SGRRHH.Local.Domain.Services
@using SGRRHH.Local.Shared.Interfaces
@inject IAuthService AuthService
@inject IPermisoRepository PermisoRepository
@inject IPermisoCalculationService CalculationService
@inject NavigationManager Navigation
@inject ILogger<Permisos> Logger

<PageTitle>Permisos - SGRRHH</PageTitle>

<div class="permisos-container">
    <PermisosHeader 
        TotalPermisos="@permisos.Count"
        PendientesAprobacion="@CountPendientes()"
        OnNuevoPermiso="@AbrirNuevoPermiso" />
    
    <PermisosFilters 
        @bind-FechaInicio="fechaInicio"
        @bind-FechaFin="fechaFin"
        @bind-Estado="estadoFiltro"
        @bind-EmpleadoId="empleadoFiltro"
        OnFilterChanged="@LoadPermisos" />
    
    @if (isLoading)
    {
        <div class="loading">Cargando permisos...</div>
    }
    else
    {
        <PermisosTable 
            Permisos="@permisosFiltrados"
            OnEdit="@EditarPermiso"
            OnApprove="@AbrirAprobacion"
            OnReject="@RechazarPermiso"
            OnDelete="@EliminarPermiso"
            OnViewSeguimiento="@AbrirSeguimiento"
            UsuarioActual="@AuthService.CurrentUser" />
    }
</div>

<PermisoFormModal 
    @ref="formularioModal"
    OnSave="@HandleSavePermiso" />

<PermisoAprobacionModal 
    @ref="aprobacionModal"
    OnApprove="@HandleApprovePermiso"
    OnReject="@HandleRejectPermiso" />

<PermisoSeguimientoPanel 
    @ref="seguimientoPanel" />

<MessageToast @ref="messageToast" />

@code {
    [Parameter] public int? PermisoIdParam { get; set; }
    
    private List<Permiso> permisos = new();
    private List<Permiso> permisosFiltrados => AplicarFiltros();
    private bool isLoading = true;
    
    // Filtros
    private DateTime? fechaInicio;
    private DateTime? fechaFin;
    private EstadoPermiso? estadoFiltro;
    private int? empleadoFiltro;
    
    // Refs
    private PermisoFormModal? formularioModal;
    private PermisoAprobacionModal? aprobacionModal;
    private PermisoSeguimientoPanel? seguimientoPanel;
    private MessageToast? messageToast;
    
    protected override async Task OnInitializedAsync()
    {
        if (!AuthService.IsAuthenticated)
        {
            Navigation.NavigateTo("/login");
            return;
        }
        
        await LoadPermisos();
        
        // Si viene con ID de permiso, abrir directamente
        if (PermisoIdParam.HasValue)
        {
            await EditarPermiso(PermisoIdParam.Value);
        }
    }
    
    private async Task LoadPermisos()
    {
        isLoading = true;
        try
        {
            permisos = await PermisoRepository.GetAllAsync();
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error cargando permisos");
            messageToast?.ShowError("Error al cargar permisos");
        }
        finally
        {
            isLoading = false;
        }
    }
    
    private List<Permiso> AplicarFiltros()
    {
        var query = permisos.AsEnumerable();
        
        if (fechaInicio.HasValue)
            query = query.Where(p => p.FechaInicio >= fechaInicio.Value);
        
        if (fechaFin.HasValue)
            query = query.Where(p => p.FechaFin <= fechaFin.Value);
        
        if (estadoFiltro.HasValue)
            query = query.Where(p => p.Estado == estadoFiltro.Value);
        
        if (empleadoFiltro.HasValue)
            query = query.Where(p => p.EmpleadoId == empleadoFiltro.Value);
        
        return query.ToList();
    }
    
    private int CountPendientes()
    {
        return permisos.Count(p => p.Estado == EstadoPermiso.Pendiente);
    }
    
    private void AbrirNuevoPermiso()
    {
        formularioModal?.Open();
    }
    
    private async Task EditarPermiso(int permisoId)
    {
        formularioModal?.Open(permisoId);
    }
    
    private void AbrirAprobacion(Permiso permiso)
    {
        aprobacionModal?.Open(permiso);
    }
    
    private async Task RechazarPermiso(Permiso permiso)
    {
        // Confirmación y rechazo
    }
    
    private async Task EliminarPermiso(Permiso permiso)
    {
        // Confirmación y eliminación
    }
    
    private void AbrirSeguimiento(int permisoId)
    {
        seguimientoPanel?.Open(permisoId);
    }
    
    private async Task HandleSavePermiso(Permiso permiso)
    {
        await LoadPermisos();
        messageToast?.ShowSuccess("Permiso guardado correctamente");
    }
    
    private async Task HandleApprovePermiso(Permiso permiso)
    {
        await LoadPermisos();
        messageToast?.ShowSuccess($"Permiso aprobado para {permiso.EmpleadoNombre}");
    }
    
    private async Task HandleRejectPermiso(Permiso permiso, string motivo)
    {
        await LoadPermisos();
        messageToast?.ShowWarning($"Permiso rechazado: {motivo}");
    }
}
```

**✅ CHECKPOINT FINAL:**
```bash
dotnet build
wc -l Permisos.razor  # ~250 líneas
```

### 3.5 Pruebas

Ejecutar `TEST_PLAN_PERMISOS.md`

**Documentar en:** `RESULTADO_PRUEBAS_PERMISOS.md`

---

## 📝 FASE 4: DOCUMENTACIÓN (1 hora)

### Entregables
1. **ANALISIS_PERMISOS.md**
2. **PLAN_ARQUITECTURA_PERMISOS.md**
3. **TEST_PLAN_PERMISOS.md**
4. **RESULTADO_PRUEBAS_PERMISOS.md**
5. **REFACTOR_SUMMARY_PERMISOS.md**

### REFACTOR_SUMMARY_PERMISOS.md
```markdown
# Resumen: Permisos.razor

## Métricas
- **ANTES:** 1,513 líneas
- **DESPUÉS:** ~250 líneas
- **Reducción:** 83%
- **Componentes:** 7 nuevos
- **Servicios:** 1 nuevo (PermisoCalculationService)

## Componentes Creados
1. PermisosHeader.razor
2. PermisosFilters.razor
3. PermisosTable.razor
4. PermisoFormModal.razor
5. PermisoAprobacionModal.razor
6. PermisoSeguimientoPanel.razor
7. PermisoCalculadora.razor

## Servicios Creados
1. PermisoCalculationService - Centraliza cálculos de negocio

## Redundancias Eliminadas
1. Cálculo de días laborables (3 ocurrencias)
2. Validación de solapamiento (2 ocurrencias)
3. Formato de fechas (15+ ocurrencias)
4. Cálculo de descuento (2 ocurrencias)
5. Obtención de festivos (3 ocurrencias)

## Pruebas
- ✅ Compilación: 0 errores
- ✅ CRUD permisos: Funciona
- ✅ Aprobación/Rechazo: Funciona
- ✅ Cálculos: Correctos
- ✅ Filtros: Funcionan
```

---

## ⚠️ REGLAS

### ❌ NO HACER:
1. NO modificar archivos de otros agentes
2. NO cambiar lógica de aprobación sin documentar
3. NO eliminar validaciones existentes

### ✅ HACER:
1. Mover lógica de cálculo a servicio
2. Compilar después de cada paso
3. Probar flujo completo de aprobación
4. Documentar reglas de negocio

---

## ✅ CHECKLIST
```markdown
- [ ] Investigación completada
- [ ] Planeación completada
- [ ] PermisoCalculationService creado ✅
- [ ] 7 componentes creados ✅
- [ ] Permisos.razor refactorizado ✅
- [ ] Todas las pruebas pasadas ✅
- [ ] Documentación completada ✅
- [ ] Build: 0 errores ✅
```

**DURACIÓN ESTIMADA:** 2-3 días  
**AGENTE:** [ID]
