# 🔧 AGENTE 5: REFACTORIZACIÓN - ControlDiario.razor

## 📋 INFORMACIÓN DEL COMPONENTE

**Componente Objetivo:** `SGRRHH.Local\SGRRHH.Local.Server\Components\Pages\ControlDiario.razor`  
**Tamaño Actual:** 1,541 líneas (64 KB)  
**Complejidad:** ⚠️ EXTREMADAMENTE ALTA  
**Prioridad:** 🔴 CRÍTICA

### Descripción
Sistema complejo de control diario de empleados, considerado el componente MÁS CRÍTICO de la aplicación:
- Registro de asistencia diaria de empleados
- Asignación de actividades por proyecto
- Cálculo de horas trabajadas
- Vista de calendario de múltiples empleados simultáneamente
- Gestión de actividades productivas
- Marcado masivo de empleados
- Navegación por fechas
- Exportación de reportes diarios
- Integración con proyectos, departamentos y actividades

### Archivos Exclusivos de Este Agente (NO TOCAR POR OTROS)
```
✅ ARCHIVOS PERMITIDOS PARA MODIFICAR/CREAR:
- SGRRHH.Local\SGRRHH.Local.Server\Components\Pages\ControlDiario.razor
- SGRRHH.Local\SGRRHH.Local.Server\Components\ControlDiario\ControlDiarioHeader.razor (NUEVO)
- SGRRHH.Local\SGRRHH.Local.Server\Components\ControlDiario\DateNavigator.razor (NUEVO)
- SGRRHH.Local\SGRRHH.Local.Server\Components\ControlDiario\FiltrosDiarios.razor (NUEVO)
- SGRRHH.Local\SGRRHH.Local.Server\Components\ControlDiario\EmpleadoRow.razor (NUEVO)
- SGRRHH.Local\SGRRHH.Local.Server\Components\ControlDiario\ActividadSelector.razor (NUEVO)
- SGRRHH.Local\SGRRHH.Local.Server\Components\ControlDiario\RegistroAsistenciaModal.razor (NUEVO)
- SGRRHH.Local\SGRRHH.Local.Server\Components\ControlDiario\AccionesMasivasPanel.razor (NUEVO)
- SGRRHH.Local\SGRRHH.Local.Server\Components\ControlDiario\ResumenDiarioCard.razor (NUEVO)

❌ ARCHIVOS PROHIBIDOS:
- EmpleadoOnboarding.razor (Agente 1)
- ScannerModal.razor (Agente 2)
- EmpleadoExpediente.razor (Agente 3)
- Permisos.razor (Agente 4)
```

---

## 🎯 OBJETIVOS DE REFACTORIZACIÓN

### Metas Principales
1. ✅ Reducir `ControlDiario.razor` de **1,541 líneas → ~300 líneas**
2. ✅ Extraer **8 componentes especializados**
3. ✅ Crear servicio `RegistroDiarioService` para lógica de negocio
4. ✅ Optimizar rendimiento (componente crítico con muchos empleados)
5. ✅ Mejorar UX de marcado masivo
6. ✅ Consolidar lógica de cálculo de horas
7. ✅ Mantener 100% funcionalidad sin regresiones
8. ✅ Compilación sin errores

### KPIs de Éxito
- **Reducción:** Mínimo 80%
- **Componentes:** 8 nuevos + 1 servicio
- **Performance:** Renderizado 50% más rápido (componentes especializados)
- **Redundancias eliminadas:** Mínimo 6
- **Tests:** 0 errores
- **Funcionalidad:** 100%

---

## 📊 FASE 1: INVESTIGACIÓN (3-4 horas)

### 1.1 Análisis Estructural

**Componente CRÍTICO - Requiere análisis exhaustivo:**

**Mapear:**
- Header con navegación de fecha (líneas ~20-80)
- Filtros de departamento/proyecto (líneas ~80-150)
- Resumen del día (líneas ~150-200)
- Grid de empleados con actividades (líneas ~200-800)
- Modal de registro de asistencia (líneas ~800-1000)
- Panel de acciones masivas (líneas ~1000-1200)
- Lógica de cálculo de horas (líneas ~1200-1400)
- Lógica de guardado (líneas ~1400-1541)

**Deliverable 1.1:** `ANALISIS_CONTROL_DIARIO.md` con:
- Mapa detallado (este es el más importante de todos)
- Diagramas de flujo de datos
- Estados críticos que no pueden perderse
- Dependencias entre secciones

### 1.2 Análisis de Performance

**CRÍTICO: Este componente maneja 50-200 empleados simultáneamente**

**Investigar problemas de rendimiento:**
1. ¿Se re-renderiza todo el componente en cada cambio?
2. ¿Hay bucles innecesarios en el código?
3. ¿Las actividades se cargan eficientemente?
4. ¿El marcado masivo causa lag?

**Tareas:**
```bash
# Buscar llamadas a StateHasChanged()
grep -n "StateHasChanged" ControlDiario.razor

# Buscar bucles foreach anidados (potencial O(n²))
grep -A 5 "foreach.*foreach" ControlDiario.razor

# Buscar queries en bucles (N+1 problem)
grep -A 3 "@foreach.*Repository" ControlDiario.razor
```

**Deliverable 1.2:** Sección "Análisis de Performance" en `ANALISIS_CONTROL_DIARIO.md`:
```markdown
## Problemas de Performance Identificados

### Problema 1: Re-renderizado completo
- **Ubicación:** Línea X
- **Causa:** StateHasChanged() en método OnActividadChanged
- **Impacto:** Re-renderiza 100+ empleados
- **Solución:** Usar componente EmpleadoRow con ShouldRender()

### Problema 2: N+1 Queries
- **Ubicación:** Línea Y
- **Causa:** GetActividades() dentro de foreach empleados
- **Impacto:** 100 queries a BD por cada carga
- **Solución:** Cargar todas las actividades de una vez

### [Más problemas...]
```

### 1.3 Análisis de Lógica de Negocio

**Reglas de negocio críticas:**
1. Cálculo de horas trabajadas
2. Validación de horas (no puede exceder 24h)
3. Distribución de horas por actividad
4. Marcado de ausencias/permisos
5. Cálculo de productividad
6. Bloqueo de edición de días pasados (opcional)

**Deliverable 1.3:** Sección "Reglas de Negocio" con decisión de dónde va cada lógica:
```markdown
## Reglas de Negocio → Ubicación

| Regla | Ubicación Actual | Ubicación Propuesta |
|-------|------------------|---------------------|
| Cálculo de horas | Componente (línea 1250) | RegistroDiarioService |
| Validación 24h | Componente (línea 1300) | RegistroDiarioService |
| Distribución actividades | Componente (línea 1350) | Componente (requiere UI) |
| Marcado ausencias | Componente (línea 900) | RegistroAsistenciaModal |
```

### 1.4 Búsqueda de Redundancias

**Investigar:**
1. **Cálculo de horas:** ¿Se repite en múltiples lugares?
2. **Carga de empleados:** ¿Se carga múltiples veces?
3. **Formateo de fechas:** ¿Código duplicado?
4. **Validaciones:** ¿Lógica repetida?
5. **Actualización de UI:** ¿StateHasChanged innecesario?

**Tareas:**
```bash
# Buscar cálculos de horas
grep -n "HorasTrabajadas\|TotalHoras" ControlDiario.razor

# Buscar cargas de empleados
grep -n "GetAllAsync\|GetByDepartamento" ControlDiario.razor

# Buscar conversiones de fecha
grep -n "ToString.*dd/MM/yyyy\|Date\.Parse" ControlDiario.razor
```

**Deliverable 1.4:** Lista de redundancias con líneas y propuesta de consolidación

### 1.5 Revisión de Skills

```bash
.cursor/skills/blazor-component/SKILL.md
.cursor/skills/hospital-ui-style/SKILL.md
.cursor/skills/build-and-verify/SKILL.md
```

---

## 🗺️ FASE 2: PLANEACIÓN (3-4 horas)

### 2.1 Arquitectura de Componentes

```
ControlDiario.razor (Orquestador - ~300 líneas)
│
├─ <ControlDiarioHeader 
│     FechaActual="@fechaSeleccionada"
│     OnExportar="@ExportarReporte" />
│
├─ <DateNavigator 
│     @bind-FechaSeleccionada="fechaSeleccionada"
│     OnFechaChanged="@LoadRegistros"
│     MostrarAtajos="true" />
│
├─ <FiltrosDiarios 
│     @bind-DepartamentoId="departamentoFiltro"
│     @bind-ProyectoId="proyectoFiltro"
│     @bind-MostrarSoloPresentes="mostrarSoloPresentes"
│     OnFilterChanged="@AplicarFiltros" />
│
├─ <ResumenDiarioCard 
│     TotalEmpleados="@empleadosActivos.Count"
│     Presentes="@CountPresentes()"
│     Ausentes="@CountAusentes()"
│     TotalHoras="@CalcularTotalHoras()" />
│
├─ <AccionesMasivasPanel 
│     EmpleadosSeleccionados="@empleadosSeleccionados"
│     OnMarcarPresentes="@MarcarPresentes"
│     OnMarcarAusentes="@MarcarAusentes"
│     OnAsignarActividad="@AsignarActividadMasiva" />
│
├─ TABLA DE EMPLEADOS:
│  └─ @foreach (var empleado in empleadosFiltrados)
│     {
│         <EmpleadoRow 
│             EmpleadoId="@empleado.Id"
│             Fecha="@fechaSeleccionada"
│             Registro="@GetRegistro(empleado.Id)"
│             OnRegistroChanged="@HandleRegistroChanged"
│             OnSelectionChanged="@HandleSelectionChanged"
│             RegistroDiarioService="@registroService" />
│     }
│
├─ <RegistroAsistenciaModal 
│     @ref="registroModal"
│     Fecha="@fechaSeleccionada"
│     EmpleadoId="@empleadoSeleccionadoId"
│     OnSave="@HandleSaveRegistro" />
│
└─ <ActividadSelector 
      @ref="actividadSelector"
      Actividades="@actividades"
      OnSelect="@HandleActividadSelected" />
```

**Deliverable 2.1:** `PLAN_ARQUITECTURA_CONTROL_DIARIO.md` con:
- Diagrama completo
- Especificación de props de cada componente
- Diagrama de flujo de datos (muy importante en este componente)
- Estrategia de optimización de renderizado

### 2.2 Diseño del Servicio RegistroDiarioService

```csharp
// SGRRHH.Local/SGRRHH.Local.Domain/Services/RegistroDiarioService.cs
namespace SGRRHH.Local.Domain.Services;

public interface IRegistroDiarioService
{
    Task<List<RegistroDiario>> GetRegistrosByFechaAsync(DateTime fecha);
    Task<RegistroDiario?> GetRegistroAsync(int empleadoId, DateTime fecha);
    Task<RegistroDiario> CreateOrUpdateRegistroAsync(RegistroDiario registro);
    Task<List<RegistroDiario>> MarcarPresenciasMasivasAsync(List<int> empleadoIds, DateTime fecha, bool presente);
    Task<bool> ValidarHorasTotales(List<DetalleActividad> detalles);
    Task<decimal> CalcularHorasTrabajadas(int empleadoId, DateTime fecha);
    Task<ResumenDiarioDTO> GetResumenDiarioAsync(DateTime fecha, int? departamentoId = null);
    Task<bool> PuedEditarFecha(DateTime fecha);
}

public class RegistroDiarioService : IRegistroDiarioService
{
    private readonly IRegistroDiarioRepository _registroRepo;
    private readonly IDetalleActividadRepository _detalleRepo;
    private readonly IEmpleadoRepository _empleadoRepo;
    private readonly ILogger<RegistroDiarioService> _logger;
    
    public RegistroDiarioService(
        IRegistroDiarioRepository registroRepo,
        IDetalleActividadRepository detalleRepo,
        IEmpleadoRepository empleadoRepo,
        ILogger<RegistroDiarioService> logger)
    {
        _registroRepo = registroRepo;
        _detalleRepo = detalleRepo;
        _empleadoRepo = empleadoRepo;
        _logger = logger;
    }
    
    public async Task<List<RegistroDiario>> GetRegistrosByFechaAsync(DateTime fecha)
    {
        // OPTIMIZACIÓN: Cargar todo de una vez en lugar de N queries
        var registros = await _registroRepo.GetByFechaAsync(fecha.Date);
        
        // Pre-cargar detalles de actividades para todos los registros
        var registroIds = registros.Select(r => r.Id).ToList();
        var detalles = await _detalleRepo.GetByRegistroIdsAsync(registroIds);
        
        // Asociar detalles a registros
        foreach (var registro in registros)
        {
            registro.DetallesActividad = detalles.Where(d => d.RegistroDiarioId == registro.Id).ToList();
        }
        
        return registros;
    }
    
    public async Task<RegistroDiario?> GetRegistroAsync(int empleadoId, DateTime fecha)
    {
        return await _registroRepo.GetByEmpleadoFechaAsync(empleadoId, fecha.Date);
    }
    
    public async Task<RegistroDiario> CreateOrUpdateRegistroAsync(RegistroDiario registro)
    {
        // Validar horas
        if (!await ValidarHorasTotales(registro.DetallesActividad))
        {
            throw new InvalidOperationException("Las horas totales no pueden exceder 24 horas");
        }
        
        var existente = await _registroRepo.GetByEmpleadoFechaAsync(registro.EmpleadoId, registro.Fecha);
        
        if (existente != null)
        {
            // Actualizar
            existente.Presente = registro.Presente;
            existente.Observaciones = registro.Observaciones;
            existente.HorasTrabajadas = registro.HorasTrabajadas;
            
            await _registroRepo.UpdateAsync(existente);
            
            // Actualizar detalles de actividades
            await _detalleRepo.DeleteByRegistroIdAsync(existente.Id);
            foreach (var detalle in registro.DetallesActividad)
            {
                detalle.RegistroDiarioId = existente.Id;
                await _detalleRepo.AddAsync(detalle);
            }
            
            return existente;
        }
        else
        {
            // Crear nuevo
            var nuevoRegistro = await _registroRepo.AddAsync(registro);
            
            // Crear detalles
            foreach (var detalle in registro.DetallesActividad)
            {
                detalle.RegistroDiarioId = nuevoRegistro.Id;
                await _detalleRepo.AddAsync(detalle);
            }
            
            return nuevoRegistro;
        }
    }
    
    public async Task<List<RegistroDiario>> MarcarPresenciasMasivasAsync(
        List<int> empleadoIds, 
        DateTime fecha, 
        bool presente)
    {
        var registros = new List<RegistroDiario>();
        
        foreach (var empleadoId in empleadoIds)
        {
            var registro = await GetRegistroAsync(empleadoId, fecha) 
                ?? new RegistroDiario 
                { 
                    EmpleadoId = empleadoId, 
                    Fecha = fecha.Date 
                };
            
            registro.Presente = presente;
            
            if (!presente)
            {
                // Si marca ausente, limpiar actividades
                registro.HorasTrabajadas = 0;
                registro.DetallesActividad.Clear();
            }
            
            registros.Add(await CreateOrUpdateRegistroAsync(registro));
        }
        
        return registros;
    }
    
    public async Task<bool> ValidarHorasTotales(List<DetalleActividad> detalles)
    {
        var totalHoras = detalles.Sum(d => d.HorasTrabajadas);
        return totalHoras <= 24;
    }
    
    public async Task<decimal> CalcularHorasTrabajadas(int empleadoId, DateTime fecha)
    {
        var registro = await GetRegistroAsync(empleadoId, fecha);
        return registro?.HorasTrabajadas ?? 0;
    }
    
    public async Task<ResumenDiarioDTO> GetResumenDiarioAsync(DateTime fecha, int? departamentoId = null)
    {
        var registros = await GetRegistrosByFechaAsync(fecha);
        
        if (departamentoId.HasValue)
        {
            var empleadosDepto = await _empleadoRepo.GetByDepartamentoIdAsync(departamentoId.Value);
            var empleadoIds = empleadosDepto.Select(e => e.Id).ToHashSet();
            registros = registros.Where(r => empleadoIds.Contains(r.EmpleadoId)).ToList();
        }
        
        return new ResumenDiarioDTO
        {
            Fecha = fecha.Date,
            TotalEmpleados = registros.Count,
            Presentes = registros.Count(r => r.Presente),
            Ausentes = registros.Count(r => !r.Presente),
            TotalHorasTrabajadas = registros.Sum(r => r.HorasTrabajadas)
        };
    }
    
    public async Task<bool> PuedeEditarFecha(DateTime fecha)
    {
        // Opcional: Bloquear edición de fechas muy antiguas
        var diasPermitidos = 30; // Configurable
        return fecha.Date >= DateTime.Today.AddDays(-diasPermitidos);
    }
}

public class ResumenDiarioDTO
{
    public DateTime Fecha { get; set; }
    public int TotalEmpleados { get; set; }
    public int Presentes { get; set; }
    public int Ausentes { get; set; }
    public decimal TotalHorasTrabajadas { get; set; }
}
```

**Deliverable 2.2:** Especificación completa del servicio

### 2.3 Estrategia de Optimización de Renderizado

**CRÍTICO para performance:**

#### EmpleadoRow.razor - Componente optimizado
```razor
@implements IDisposable

<tr class="empleado-row @(IsSelected ? "selected" : "")">
    <td>
        <input type="checkbox" @bind="isSelectedLocal" @bind:after="NotifySelectionChanged" />
    </td>
    <td>@Empleado.Codigo</td>
    <td>@Empleado.NombreCompleto</td>
    <td>
        <input type="checkbox" @bind="registroLocal.Presente" @bind:after="SaveRegistro" />
    </td>
    <td>
        @if (registroLocal.Presente)
        {
            <input type="number" 
                   @bind="registroLocal.HorasTrabajadas" 
                   @bind:after="SaveRegistro"
                   min="0" 
                   max="24" 
                   step="0.5" 
                   class="hospital-input-small" />
        }
        else
        {
            <span class="text-muted">N/A</span>
        }
    </td>
    <td>
        <button @onclick="AbrirActividades" 
                disabled="@(!registroLocal.Presente)" 
                class="btn-small">
            🎯 Actividades (@registroLocal.DetallesActividad.Count)
        </button>
    </td>
</tr>

@code {
    [Parameter] public int EmpleadoId { get; set; }
    [Parameter] public Empleado Empleado { get; set; } = new();
    [Parameter] public DateTime Fecha { get; set; }
    [Parameter] public RegistroDiario? Registro { get; set; }
    [Parameter] public EventCallback<RegistroDiario> OnRegistroChanged { get; set; }
    [Parameter] public EventCallback<(int EmpleadoId, bool Selected)> OnSelectionChanged { get; set; }
    [Parameter] public IRegistroDiarioService RegistroDiarioService { get; set; } = null!;
    
    public bool IsSelected { get; set; }
    
    private RegistroDiario registroLocal = new();
    private bool isSelectedLocal;
    private bool isSaving;
    
    protected override void OnParametersSet()
    {
        registroLocal = Registro ?? new RegistroDiario 
        { 
            EmpleadoId = EmpleadoId, 
            Fecha = Fecha 
        };
    }
    
    // OPTIMIZACIÓN: Solo re-renderizar si cambian parámetros relevantes
    protected override bool ShouldRender()
    {
        // Solo renderizar si:
        // 1. Cambió el registro
        // 2. Cambió la selección
        // 3. Está guardando
        return true; // Por defecto, luego optimizar según necesidad
    }
    
    private async Task SaveRegistro()
    {
        if (isSaving) return;
        
        isSaving = true;
        try
        {
            var registroGuardado = await RegistroDiarioService.CreateOrUpdateRegistroAsync(registroLocal);
            await OnRegistroChanged.InvokeAsync(registroGuardado);
        }
        finally
        {
            isSaving = false;
        }
    }
    
    private async Task NotifySelectionChanged()
    {
        IsSelected = isSelectedLocal;
        await OnSelectionChanged.InvokeAsync((EmpleadoId, IsSelected));
    }
    
    private void AbrirActividades()
    {
        // Abrir modal de actividades
    }
    
    public void Dispose()
    {
        // Cleanup si es necesario
    }
}
```

**Deliverable 2.3:** Documento "Estrategia de Optimización" en `PLAN_ARQUITECTURA_CONTROL_DIARIO.md`:
```markdown
## Estrategia de Optimización

### 1. Componentes con ShouldRender()
- EmpleadoRow implementa ShouldRender() para evitar re-renders innecesarios
- Solo actualiza si sus props específicas cambian

### 2. Carga de Datos en Batch
- GetRegistrosByFechaAsync() carga TODOS los registros + detalles en 2 queries
- NO hacer queries dentro de bucles

### 3. Actualización Selectiva
- Usar EventCallback en lugar de StateHasChanged() global
- Actualizar solo el EmpleadoRow afectado

### 4. Debounce en Inputs
- Inputs numéricos guardan con debounce de 500ms
- Evita múltiples guardados por cada tecla

### 5. Virtualización (Opcional)
- Si >200 empleados, considerar virtualización con Virtualize component
```

### 2.4 Plan de Consolidación

**Redundancias a eliminar:**

1. **Cálculo de horas totales:**
   - ❌ ANTES: Código duplicado en 4 lugares
   - ✅ DESPUÉS: `RegistroDiarioService.CalcularHorasTrabajadas()`

2. **Validación de 24 horas:**
   - ❌ ANTES: Lógica repetida en formulario y guardado
   - ✅ DESPUÉS: `RegistroDiarioService.ValidarHorasTotales()`

3. **Carga de registros:**
   - ❌ ANTES: Múltiples queries (N+1)
   - ✅ DESPUÉS: Batch loading en servicio

4. **Marcado masivo:**
   - ❌ ANTES: Bucle con SaveAsync() individual
   - ✅ DESPUÉS: `RegistroDiarioService.MarcarPresenciasMasivasAsync()`

5. **Resumen del día:**
   - ❌ ANTES: Cálculos manuales en componente
   - ✅ DESPUÉS: `RegistroDiarioService.GetResumenDiarioAsync()`

6. **Formato de fecha:**
   - ❌ ANTES: `ToString("dd/MM/yyyy")` en 10+ lugares
   - ✅ DESPUÉS: `DateHelpers.FormatShortDate()`

**Deliverable 2.4:** Tabla completa de consolidaciones

### 2.5 Plan de Pruebas

**Checklist (MÁS EXHAUSTIVO que otros):**
```markdown
## Funcionalidad Básica
- [ ] Compilación: 0 errores
- [ ] Cargar empleados del día
- [ ] Navegar entre fechas (anterior/siguiente/hoy)
- [ ] Filtrar por departamento
- [ ] Filtrar por proyecto

## Registro Individual
- [ ] Marcar empleado presente
- [ ] Marcar empleado ausente
- [ ] Ingresar horas trabajadas
- [ ] Asignar actividad a empleado
- [ ] Validar máximo 24 horas
- [ ] Guardar registro correctamente

## Acciones Masivas
- [ ] Seleccionar múltiples empleados
- [ ] Marcar presentes en masa (10 empleados)
- [ ] Marcar ausentes en masa (10 empleados)
- [ ] Asignar actividad en masa
- [ ] Validar guardado masivo correcto

## Actividades
- [ ] Abrir selector de actividades
- [ ] Asignar actividad con horas
- [ ] Distribuir horas entre múltiples actividades
- [ ] Validar suma de horas = horas trabajadas
- [ ] Eliminar actividad

## Resumen y Reportes
- [ ] Resumen del día muestra totales correctos
- [ ] Contador de presentes correcto
- [ ] Contador de ausentes correcto
- [ ] Total de horas correcto
- [ ] Exportar reporte del día

## Performance (CRÍTICO)
- [ ] Carga inicial < 2 segundos (100 empleados)
- [ ] Cambio de fecha < 1 segundo
- [ ] Marcado masivo (50 empleados) < 3 segundos
- [ ] NO lag al escribir en inputs
- [ ] Scroll fluido en tabla grande
```

**Deliverable 2.5:** `TEST_PLAN_CONTROL_DIARIO.md` (el más completo de todos)

---

## ⚙️ FASE 3: EJECUCIÓN CONTROLADA (12-16 horas)

**NOTA:** Este es el componente más complejo. Tomar tiempo extra si es necesario.

### 3.1 Preparación

```bash
mkdir -p SGRRHH.Local/SGRRHH.Local.Server/Components/ControlDiario
cp ControlDiario.razor ControlDiario.razor.BACKUP
dotnet build SGRRHH.Local/SGRRHH.Local.Server/SGRRHH.Local.Server.csproj
```

### 3.2 Iteración 1: Crear Servicio (PRIMERO)

**Paso 1: IRegistroDiarioService.cs**
```csharp
// (Ver diseño completo en sección 2.2)
```

**Paso 2: RegistroDiarioService.cs**
```csharp
// (Ver implementación completa en sección 2.2)
```

**Paso 3: Registrar en Program.cs**
```csharp
builder.Services.AddScoped<IRegistroDiarioService, RegistroDiarioService>();
```

**✅ CHECKPOINT 1:** Compilar y verificar que el servicio funciona

### 3.3 Iteración 2: Componentes Críticos

**ORDEN DE CREACIÓN (importante):**

#### Paso 4: EmpleadoRow.razor (COMPONENTE MÁS IMPORTANTE)
```razor
@* Ver diseño completo en sección 2.3 *@
@* Este componente ES CRÍTICO para performance *@
```

**✅ CHECKPOINT 2:** Compilar - Este componente debe funcionar perfectamente

#### Paso 5: DateNavigator.razor
```razor
<div class="date-navigator">
    <button @onclick="Anterior" class="nav-btn">◄ ANTERIOR</button>
    <button @onclick="Hoy" class="nav-btn-today">HOY</button>
    <input type="date" 
           @bind="fechaLocal" 
           @bind:after="NotifyChange"
           class="date-input" />
    <button @onclick="Siguiente" class="nav-btn">SIGUIENTE ▶</button>
    
    @if (MostrarAtajos)
    {
        <div class="date-shortcuts">
            <button @onclick="() => IrAFecha(DateTime.Today.AddDays(-7))">-7 días</button>
            <button @onclick="() => IrAFecha(DateTime.Today.AddDays(7))">+7 días</button>
        </div>
    }
</div>

@code {
    [Parameter] public DateTime FechaSeleccionada { get; set; } = DateTime.Today;
    [Parameter] public EventCallback<DateTime> FechaSeleccionadaChanged { get; set; }
    [Parameter] public EventCallback OnFechaChanged { get; set; }
    [Parameter] public bool MostrarAtajos { get; set; }
    
    private DateTime fechaLocal;
    
    protected override void OnParametersSet()
    {
        fechaLocal = FechaSeleccionada;
    }
    
    private async Task Anterior()
    {
        await IrAFecha(fechaLocal.AddDays(-1));
    }
    
    private async Task Siguiente()
    {
        await IrAFecha(fechaLocal.AddDays(1));
    }
    
    private async Task Hoy()
    {
        await IrAFecha(DateTime.Today);
    }
    
    private async Task IrAFecha(DateTime fecha)
    {
        fechaLocal = fecha;
        await NotifyChange();
    }
    
    private async Task NotifyChange()
    {
        await FechaSeleccionadaChanged.InvokeAsync(fechaLocal);
        await OnFechaChanged.InvokeAsync();
    }
}
```

**✅ CHECKPOINT 3:** Compilar

#### Paso 6-11: Crear componentes restantes
- ControlDiarioHeader.razor
- FiltrosDiarios.razor
- ResumenDiarioCard.razor
- AccionesMasivasPanel.razor
- RegistroAsistenciaModal.razor
- ActividadSelector.razor

**✅ CHECKPOINTS 4-9:** Compilar después de cada uno

### 3.4 Iteración 3: Refactorizar ControlDiario.razor

**Versión refactorizada (~300 líneas):**

```razor
@page "/control-diario"
@page "/control-diario/{FechaParam}"
@using SGRRHH.Local.Domain.Entities
@using SGRRHH.Local.Domain.Services
@using SGRRHH.Local.Shared.Interfaces
@inject IAuthService AuthService
@inject IRegistroDiarioService RegistroService
@inject IEmpleadoRepository EmpleadoRepo
@inject NavigationManager Navigation
@inject ILogger<ControlDiario> Logger

<PageTitle>Control Diario - SGRRHH</PageTitle>

<div class="hospital-page">
    <ControlDiarioHeader 
        FechaActual="@fechaSeleccionada"
        OnExportar="@ExportarReporte" />
    
    <DateNavigator 
        @bind-FechaSeleccionada="fechaSeleccionada"
        OnFechaChanged="@LoadRegistros"
        MostrarAtajos="true" />
    
    <FiltrosDiarios 
        @bind-DepartamentoId="departamentoFiltro"
        @bind-ProyectoId="proyectoFiltro"
        @bind-MostrarSoloPresentes="mostrarSoloPresentes"
        OnFilterChanged="@AplicarFiltros" />
    
    <ResumenDiarioCard 
        TotalEmpleados="@empleadosFiltrados.Count"
        Presentes="@CountPresentes()"
        Ausentes="@CountAusentes()"
        TotalHoras="@CalcularTotalHoras()" />
    
    @if (empleadosSeleccionados.Any())
    {
        <AccionesMasivasPanel 
            EmpleadosSeleccionados="@empleadosSeleccionados"
            OnMarcarPresentes="@MarcarPresentes"
            OnMarcarAusentes="@MarcarAusentes"
            OnAsignarActividad="@AsignarActividadMasiva" />
    }
    
    @if (isLoading)
    {
        <div class="loading">Cargando registros del día...</div>
    }
    else
    {
        <div class="tabla-control-diario">
            <table class="hospital-table">
                <thead>
                    <tr>
                        <th><input type="checkbox" @bind="seleccionarTodos" @bind:after="ToggleSeleccionarTodos" /></th>
                        <th>CÓDIGO</th>
                        <th>EMPLEADO</th>
                        <th>PRESENTE</th>
                        <th>HORAS</th>
                        <th>ACTIVIDADES</th>
                    </tr>
                </thead>
                <tbody>
                    @foreach (var empleado in empleadosFiltrados)
                    {
                        <EmpleadoRow 
                            EmpleadoId="@empleado.Id"
                            Empleado="@empleado"
                            Fecha="@fechaSeleccionada"
                            Registro="@GetRegistro(empleado.Id)"
                            OnRegistroChanged="@HandleRegistroChanged"
                            OnSelectionChanged="@HandleSelectionChanged"
                            RegistroDiarioService="@RegistroService" />
                    }
                </tbody>
            </table>
        </div>
    }
</div>

<RegistroAsistenciaModal 
    @ref="registroModal"
    OnSave="@HandleSaveRegistro" />

<ActividadSelector 
    @ref="actividadSelector"
    OnSelect="@HandleActividadSelected" />

<MessageToast @ref="messageToast" />

@code {
    [Parameter] public string? FechaParam { get; set; }
    
    private DateTime fechaSeleccionada = DateTime.Today;
    private List<Empleado> empleadosActivos = new();
    private List<Empleado> empleadosFiltrados => AplicarFiltros();
    private Dictionary<int, RegistroDiario> registrosPorEmpleado = new();
    
    // Filtros
    private int? departamentoFiltro;
    private int? proyectoFiltro;
    private bool mostrarSoloPresentes;
    
    // Selección masiva
    private HashSet<int> empleadosSeleccionados = new();
    private bool seleccionarTodos;
    
    // Estado
    private bool isLoading = true;
    
    // Refs
    private RegistroAsistenciaModal? registroModal;
    private ActividadSelector? actividadSelector;
    private MessageToast? messageToast;
    
    protected override async Task OnInitializedAsync()
    {
        if (!AuthService.IsAuthenticated)
        {
            Navigation.NavigateTo("/login");
            return;
        }
        
        // Parsear fecha de parámetro si existe
        if (!string.IsNullOrEmpty(FechaParam) && DateTime.TryParse(FechaParam, out var fecha))
        {
            fechaSeleccionada = fecha;
        }
        
        await LoadEmpleados();
        await LoadRegistros();
    }
    
    private async Task LoadEmpleados()
    {
        try
        {
            empleadosActivos = await EmpleadoRepo.GetActivosAsync();
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error cargando empleados");
            messageToast?.ShowError("Error al cargar empleados");
        }
    }
    
    private async Task LoadRegistros()
    {
        isLoading = true;
        try
        {
            // OPTIMIZACIÓN: Cargar todos los registros del día en batch
            var registros = await RegistroService.GetRegistrosByFechaAsync(fechaSeleccionada);
            
            // Convertir a diccionario para acceso rápido O(1)
            registrosPorEmpleado = registros.ToDictionary(r => r.EmpleadoId);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error cargando registros");
            messageToast?.ShowError("Error al cargar registros del día");
        }
        finally
        {
            isLoading = false;
        }
    }
    
    private List<Empleado> AplicarFiltros()
    {
        var query = empleadosActivos.AsEnumerable();
        
        if (departamentoFiltro.HasValue)
            query = query.Where(e => e.DepartamentoId == departamentoFiltro.Value);
        
        if (proyectoFiltro.HasValue)
        {
            // Filtrar por proyecto (requiere join con registros)
            var empleadosEnProyecto = registrosPorEmpleado.Values
                .Where(r => r.DetallesActividad.Any(d => d.ProyectoId == proyectoFiltro.Value))
                .Select(r => r.EmpleadoId)
                .ToHashSet();
            
            query = query.Where(e => empleadosEnProyecto.Contains(e.Id));
        }
        
        if (mostrarSoloPresentes)
            query = query.Where(e => GetRegistro(e.Id)?.Presente == true);
        
        return query.ToList();
    }
    
    private RegistroDiario? GetRegistro(int empleadoId)
    {
        return registrosPorEmpleado.GetValueOrDefault(empleadoId);
    }
    
    private int CountPresentes()
    {
        return registrosPorEmpleado.Values.Count(r => r.Presente);
    }
    
    private int CountAusentes()
    {
        return empleadosFiltrados.Count - CountPresentes();
    }
    
    private decimal CalcularTotalHoras()
    {
        return registrosPorEmpleado.Values
            .Where(r => r.Presente)
            .Sum(r => r.HorasTrabajadas);
    }
    
    private async Task HandleRegistroChanged(RegistroDiario registro)
    {
        registrosPorEmpleado[registro.EmpleadoId] = registro;
        // NO llamar StateHasChanged() aquí - el componente hijo se actualiza solo
    }
    
    private void HandleSelectionChanged((int EmpleadoId, bool Selected) args)
    {
        if (args.Selected)
            empleadosSeleccionados.Add(args.EmpleadoId);
        else
            empleadosSeleccionados.Remove(args.EmpleadoId);
        
        StateHasChanged();
    }
    
    private void ToggleSeleccionarTodos()
    {
        if (seleccionarTodos)
        {
            foreach (var empleado in empleadosFiltrados)
                empleadosSeleccionados.Add(empleado.Id);
        }
        else
        {
            empleadosSeleccionados.Clear();
        }
        
        StateHasChanged();
    }
    
    private async Task MarcarPresentes()
    {
        try
        {
            var registros = await RegistroService.MarcarPresenciasMasivasAsync(
                empleadosSeleccionados.ToList(), 
                fechaSeleccionada, 
                presente: true);
            
            // Actualizar diccionario
            foreach (var registro in registros)
            {
                registrosPorEmpleado[registro.EmpleadoId] = registro;
            }
            
            messageToast?.ShowSuccess($"{empleadosSeleccionados.Count} empleados marcados como presentes");
            empleadosSeleccionados.Clear();
            seleccionarTodos = false;
            StateHasChanged();
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error en marcado masivo");
            messageToast?.ShowError("Error al marcar empleados");
        }
    }
    
    private async Task MarcarAusentes()
    {
        try
        {
            var registros = await RegistroService.MarcarPresenciasMasivasAsync(
                empleadosSeleccionados.ToList(), 
                fechaSeleccionada, 
                presente: false);
            
            // Actualizar diccionario
            foreach (var registro in registros)
            {
                registrosPorEmpleado[registro.EmpleadoId] = registro;
            }
            
            messageToast?.ShowSuccess($"{empleadosSeleccionados.Count} empleados marcados como ausentes");
            empleadosSeleccionados.Clear();
            seleccionarTodos = false;
            StateHasChanged();
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error en marcado masivo");
            messageToast?.ShowError("Error al marcar empleados");
        }
    }
    
    private void AsignarActividadMasiva()
    {
        actividadSelector?.Open(empleadosSeleccionados.ToList());
    }
    
    private void HandleActividadSelected(/* ... */)
    {
        // Lógica de asignación masiva
    }
    
    private async Task ExportarReporte()
    {
        // Lógica de exportación
    }
    
    private async Task HandleSaveRegistro(RegistroDiario registro)
    {
        await LoadRegistros();
        messageToast?.ShowSuccess("Registro guardado");
    }
}
```

**✅ CHECKPOINT FINAL:**
```bash
dotnet build
wc -l ControlDiario.razor  # Debe ser ~300 líneas
```

### 3.5 Pruebas Exhaustivas

**CRÍTICO:** Este componente requiere pruebas MÁS EXHAUSTIVAS que todos los demás.

Ejecutar TODAS las pruebas de `TEST_PLAN_CONTROL_DIARIO.md`, incluyendo:
- Pruebas funcionales
- Pruebas de performance
- Pruebas con 100+ empleados
- Pruebas de marcado masivo

**Documentar en:** `RESULTADO_PRUEBAS_CONTROL_DIARIO.md` con métricas de performance

---

## 📝 FASE 4: DOCUMENTACIÓN (1-2 horas)

### Entregables
1. **ANALISIS_CONTROL_DIARIO.md** (MÁS DETALLADO)
2. **PLAN_ARQUITECTURA_CONTROL_DIARIO.md** (CON DIAGRAMAS)
3. **TEST_PLAN_CONTROL_DIARIO.md** (MÁS EXHAUSTIVO)
4. **RESULTADO_PRUEBAS_CONTROL_DIARIO.md** (CON MÉTRICAS)
5. **REFACTOR_SUMMARY_CONTROL_DIARIO.md**

### REFACTOR_SUMMARY_CONTROL_DIARIO.md
```markdown
# Resumen: ControlDiario.razor

## Métricas
- **ANTES:** 1,541 líneas
- **DESPUÉS:** ~300 líneas
- **Reducción:** 80%
- **Componentes:** 8 nuevos
- **Servicios:** 1 nuevo (RegistroDiarioService)

## Performance
- **Carga inicial ANTES:** ~5 segundos (100 empleados)
- **Carga inicial DESPUÉS:** ~1.5 segundos
- **Mejora:** 70% más rápido

## Componentes Creados
1. ControlDiarioHeader.razor
2. DateNavigator.razor
3. FiltrosDiarios.razor
4. EmpleadoRow.razor (componente crítico optimizado)
5. ResumenDiarioCard.razor
6. AccionesMasivasPanel.razor
7. RegistroAsistenciaModal.razor
8. ActividadSelector.razor

## Servicios Creados
1. RegistroDiarioService - Lógica de negocio centralizada

## Redundancias Eliminadas
1. Cálculo de horas (4 ocurrencias)
2. Validación 24h (2 ocurrencias)
3. Carga de registros N+1 → Batch loading
4. Marcado masivo bucle → Método optimizado
5. Resumen manual → Método en servicio
6. Formato fechas (10+ ocurrencias)

## Optimizaciones de Performance
1. Batch loading de registros (1 query vs N queries)
2. Dictionary lookup O(1) en lugar de búsqueda O(n)
3. EmpleadoRow con ShouldRender() optimizado
4. Evitado StateHasChanged() global
5. EventCallbacks selectivos

## Pruebas
- ✅ Funcionales: 100% pasadas
- ✅ Performance: Mejora 70%
- ✅ Regresiones: 0 detectadas
- ✅ Estabilidad: Probado con 150 empleados

## Recomendaciones Futuras
1. Implementar virtualización si >300 empleados
2. Agregar caching de empleados activos
3. Considerar websockets para actualización en tiempo real
4. Agregar undo/redo para cambios masivos
```

---

## ⚠️ REGLAS CRÍTICAS

### ❌ NO HACER NUNCA:
1. NO modificar archivos de otros agentes
2. NO hacer commit sin pruebas exhaustivas de performance
3. NO eliminar optimizaciones de carga
4. NO usar StateHasChanged() global sin justificación
5. NO hacer queries en bucles

### ✅ HACER SIEMPRE:
1. Priorizar performance sobre todo
2. Compilar después de cada paso
3. Probar con 100+ empleados
4. Medir tiempos de carga
5. Documentar optimizaciones

---

## ✅ CHECKLIST FINAL
```markdown
- [ ] Investigación completada (análisis exhaustivo)
- [ ] Planeación completada (con estrategia de performance)
- [ ] RegistroDiarioService creado ✅
- [ ] EmpleadoRow optimizado creado ✅
- [ ] 7 componentes restantes creados ✅
- [ ] ControlDiario.razor refactorizado ✅
- [ ] Pruebas funcionales pasadas ✅
- [ ] Pruebas de performance pasadas ✅
- [ ] Carga <2s con 100 empleados ✅
- [ ] Marcado masivo <3s (50 empleados) ✅
- [ ] Documentación completada ✅
- [ ] Build: 0 errores ✅
```

---

## 📊 MÉTRICAS DE ÉXITO

Este componente se considera **EXITOSO** si cumple:

1. ✅ Reducción ≥ 80% en líneas
2. ✅ Performance mejorada ≥ 50%
3. ✅ Carga inicial < 2 segundos (100 empleados)
4. ✅ NO regresiones funcionales
5. ✅ Compilación exitosa
6. ✅ Pruebas exhaustivas pasadas

---

**DURACIÓN ESTIMADA:** 3-4 días (el más complejo de todos)  
**PRIORIDAD:** 🔴 CRÍTICA  
**AGENTE:** [ID]

**NOTA IMPORTANTE:** Este es el componente más crítico y complejo de la aplicación. Tomar el tiempo necesario para hacerlo correctamente. La performance es CRÍTICA.
