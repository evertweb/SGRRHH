# 🔧 AGENTE 1: REFACTORIZACIÓN - EmpleadoOnboarding.razor

## 📋 INFORMACIÓN DEL COMPONENTE

**Componente Objetivo:** `SGRRHH.Local\SGRRHH.Local.Server\Components\Pages\EmpleadoOnboarding.razor`  
**Tamaño Actual:** 1,843 líneas (89 KB)  
**Complejidad:** ⚠️ MUY ALTA  
**Prioridad:** 🔴 CRÍTICA

### Descripción
Wizard de incorporación de nuevos empleados con formularios extensos de:
- Datos personales y laborales
- Seguridad social (EPS, AFP, ARL, Caja Compensación)
- Información bancaria
- Datos de contacto
- Carga de documentos obligatorios y opcionales
- Validaciones complejas y guardado en base de datos

### Archivos Exclusivos de Este Agente (NO TOCAR POR OTROS)
```
✅ ARCHIVOS PERMITIDOS PARA MODIFICAR/CREAR:
- SGRRHH.Local\SGRRHH.Local.Server\Components\Pages\EmpleadoOnboarding.razor
- SGRRHH.Local\SGRRHH.Local.Server\Components\Forms\DatosPersonalesForm.razor (NUEVO)
- SGRRHH.Local\SGRRHH.Local.Server\Components\Forms\DatosLaboralesForm.razor (NUEVO)
- SGRRHH.Local\SGRRHH.Local.Server\Components\Forms\SeguridadSocialForm.razor (NUEVO)
- SGRRHH.Local\SGRRHH.Local.Server\Components\Forms\DatosBancariosForm.razor (NUEVO)
- SGRRHH.Local\SGRRHH.Local.Server\Components\Forms\ContactoEmpleadoForm.razor (NUEVO)
- SGRRHH.Local\SGRRHH.Local.Server\Components\Shared\WizardNavigation.razor (NUEVO)
- SGRRHH.Local\SGRRHH.Local.Server\Components\Shared\WizardProgress.razor (NUEVO)

❌ ARCHIVOS PROHIBIDOS (USADOS POR OTROS AGENTES):
- ScannerModal.razor (Agente 2)
- EmpleadoExpediente.razor (Agente 3)
- Permisos.razor (Agente 4)
- ControlDiario.razor (Agente 5)
- Cualquier archivo en Components/Tabs/ (Agente 3)
```

---

## 🎯 OBJETIVOS DE REFACTORIZACIÓN

### Metas Principales
1. ✅ Reducir `EmpleadoOnboarding.razor` de **1,843 líneas → ~300 líneas** (componente orquestador)
2. ✅ Extraer **7 componentes reutilizables** independientes
3. ✅ Eliminar código duplicado y redundancias
4. ✅ Mejorar rendimiento (renderizado selectivo por componente)
5. ✅ Facilitar testing unitario de cada sección
6. ✅ Mantener 100% de funcionalidad existente
7. ✅ Asegurar que el proyecto compile sin errores

### KPIs de Éxito
- **Reducción de líneas:** Mínimo 75% en archivo principal
- **Componentes creados:** 7 nuevos componentes
- **Redundancias eliminadas:** Mínimo 3 bloques de código duplicado
- **Tests de compilación:** 0 errores de build
- **Funcionalidad:** 100% operativa sin regresiones

---

## 📊 FASE 1: INVESTIGACIÓN (2-3 horas)

### 1.1 Análisis Estructural

**Tareas:**
```bash
# 1. Leer y documentar el componente completo
- Leer EmpleadoOnboarding.razor línea por línea
- Identificar todas las secciones funcionales
- Mapear dependencias entre secciones
- Identificar props/parámetros necesarios
- Listar todas las inyecciones de dependencia usadas
```

**Deliverable 1.1:** Crear archivo `ANALISIS_EMPLEADO_ONBOARDING.md` con:
- Mapa de secciones (líneas inicio-fin)
- Dependencias de cada sección
- Variables de estado compartidas
- Métodos por sección

### 1.2 Búsqueda de Redundancias

**Investigar:**
1. **Código duplicado interno:** ¿Hay validaciones repetidas?
2. **Lógica similar en otros componentes:** 
   - Comparar con `EmpleadoEditar.razor`
   - Comparar con tabs existentes: `InformacionBancariaTab.razor`, `SeguridadSocialTab.razor`
3. **Patrones comunes:** Validaciones de cédula, email, campos requeridos

**Tareas Específicas:**
```bash
# Buscar duplicación de validación de cédula
grep -r "Cedula" SGRRHH.Local/SGRRHH.Local.Server/Components/Pages/

# Buscar duplicación de lógica de seguridad social
grep -r "EPS\|AFP\|ARL" SGRRHH.Local/SGRRHH.Local.Server/Components/

# Buscar componentes de formulario existentes que se puedan reutilizar
ls SGRRHH.Local/SGRRHH.Local.Server/Components/Shared/Input*.razor
```

**Deliverable 1.2:** Sección en `ANALISIS_EMPLEADO_ONBOARDING.md`:
- Lista de código duplicado encontrado (con líneas)
- Componentes existentes que se pueden reutilizar
- Oportunidades de consolidación

### 1.3 Revisión de Skills y Patrones del Proyecto

**Leer obligatoriamente:**
```bash
.cursor/skills/blazor-component/SKILL.md       # Patrones de componentes
.cursor/skills/hospital-ui-style/SKILL.md      # Estilos UI hospitalarios
.cursor/skills/build-and-verify/SKILL.md       # Comando de compilación
```

**Identificar:**
- Estructura estándar de componentes en el proyecto
- Convenciones de nombres
- Patrones de validación usados
- Estilos CSS aplicables

**Deliverable 1.3:** Checklist en `ANALISIS_EMPLEADO_ONBOARDING.md`:
- ✅ Leído skill blazor-component
- ✅ Leído skill hospital-ui-style
- ✅ Identificadas convenciones de nombres
- ✅ Identificados patrones de validación

### 1.4 Análisis de Dependencias

**Mapear:**
1. Servicios inyectados y su uso por sección
2. Catálogos necesarios (cargos, departamentos, EPS, etc.)
3. Eventos y callbacks entre secciones
4. Estado compartido crítico

**Deliverable 1.4:** Diagrama de dependencias en `ANALISIS_EMPLEADO_ONBOARDING.md`

---

## 🗺️ FASE 2: PLANEACIÓN (2-3 horas)

### 2.1 Diseño de Arquitectura de Componentes

**Crear el siguiente árbol de componentes:**

```
EmpleadoOnboarding.razor (Orquestador - ~300 líneas)
│
├─ <WizardProgress currentStep="@currentStep" totalSteps="2" />
│
├─ PASO 1: Formularios
│  │
│  ├─ <DatosPersonalesForm 
│  │     @bind-Empleado="empleado"
│  │     OnValidationChanged="@HandleValidation" />
│  │
│  ├─ <DatosLaboralesForm 
│  │     @bind-Empleado="empleado"
│  │     Cargos="@cargos"
│  │     Departamentos="@departamentos"
│  │     OnCargoChanged="@OnCargoChanged" />
│  │
│  ├─ <SeguridadSocialForm 
│  │     @bind-Empleado="empleado"
│  │     EpsList="@epsLista"
│  │     AfpList="@afpLista"
│  │     ArlList="@arlLista"
│  │     CajasList="@cajasLista" />
│  │
│  ├─ <DatosBancariosForm 
│  │     @bind-Empleado="empleado"
│  │     Bancos="@bancos" />
│  │
│  └─ <ContactoEmpleadoForm 
│        @bind-Empleado="empleado" />
│
├─ PASO 2: Revisión
│  └─ <ResumenEmpleadoOnboarding Empleado="@empleado" />
│
└─ <WizardNavigation 
      CurrentStep="@currentStep"
      TotalSteps="2"
      CanGoNext="@CanGoNext()"
      IsSaving="@isSaving"
      OnPrevious="@Anterior"
      OnNext="@Siguiente"
      OnCancel="@Cancelar"
      OnFinish="@Finalizar" />
```

**Deliverable 2.1:** Archivo `PLAN_ARQUITECTURA_ONBOARDING.md` con:
- Diagrama de componentes
- Props/parámetros de cada componente
- Eventos/callbacks de cada componente
- Estado local vs estado compartido

### 2.2 Plan de Migración de Código

**Para CADA componente nuevo, especificar:**

| Componente | Líneas Origen | Líneas Destino | Dependencias | Validaciones |
|------------|---------------|----------------|--------------|--------------|
| DatosPersonalesForm | 630-698 | 1-150 | ICatalogCache | Cédula, nombres, apellidos |
| DatosLaboralesForm | 699-778 | 1-200 | ICatalogCache | Fecha ingreso, cargo |
| SeguridadSocialForm | 780-878 | 1-250 | ICatalogCache | EPS, AFP, ARL, Caja |
| DatosBancariosForm | 900-1000 | 1-120 | ICatalogCache | Número cuenta, banco |
| ContactoEmpleadoForm | 880-950 | 1-100 | Ninguna | Teléfono, email |
| WizardProgress | 32-36 | 1-50 | Ninguna | N/A |
| WizardNavigation | 51-87 | 1-80 | Ninguna | N/A |

**Deliverable 2.2:** Tabla completa en `PLAN_ARQUITECTURA_ONBOARDING.md`

### 2.3 Identificación de Código a Consolidar

**Redundancias a eliminar:**

1. **Validación de Cédula:**
   - ❌ ANTES: Código duplicado en líneas ~450, ~1200
   - ✅ DESPUÉS: Usar componente existente `InputCedula.razor`

2. **Validación de Email:**
   - ❌ ANTES: Regex duplicada en múltiples lugares
   - ✅ DESPUÉS: Crear método estático en `ValidationHelpers.cs`

3. **Formato de Moneda:**
   - ❌ ANTES: Conversión manual en múltiples lugares
   - ✅ DESPUÉS: Usar componente existente `InputMoneda.razor`

4. **Selección de Catálogos:**
   - ❌ ANTES: HTML repetido para selects de EPS, AFP, ARL, Caja
   - ✅ DESPUÉS: Crear componente genérico `CatalogoSelect.razor`

**Deliverable 2.3:** Sección "Consolidaciones" en `PLAN_ARQUITECTURA_ONBOARDING.md`

### 2.4 Plan de Pruebas

**Estrategia de validación:**
1. Compilar después de CADA componente creado
2. Probar funcionalidad completa al final
3. Validar todos los flujos de usuario

**Checklist de pruebas:**
```markdown
- [ ] Compilación exitosa sin warnings críticos
- [ ] Flujo normal: Crear empleado Operador (estado PendienteAprobacion)
- [ ] Flujo normal: Crear empleado Aprobador (estado Activo)
- [ ] Validación: Campos requeridos marcan error
- [ ] Validación: Cédula duplicada rechazada
- [ ] Validación: Email duplicado rechazado
- [ ] Navegación: Pasos 1→2 funciona
- [ ] Navegación: Botón "Anterior" funciona
- [ ] Guardado: Empleado se crea en BD correctamente
- [ ] Guardado: Redirección a /documentos funciona
- [ ] Cancelación: Modal de confirmación aparece
- [ ] Estilos: Mantiene estilo hospitalario
```

**Deliverable 2.4:** Archivo `TEST_PLAN_ONBOARDING.md`

---

## ⚙️ FASE 3: EJECUCIÓN CONTROLADA (8-12 horas)

### 3.1 Preparación del Entorno

```bash
# 1. Crear carpetas necesarias
mkdir -p SGRRHH.Local/SGRRHH.Local.Server/Components/Forms
mkdir -p SGRRHH.Local/SGRRHH.Local.Server/Components/Shared

# 2. Backup del componente original
cp EmpleadoOnboarding.razor EmpleadoOnboarding.razor.BACKUP

# 3. Verificar que compila ANTES de refactorizar
dotnet build SGRRHH.Local/SGRRHH.Local.Server/SGRRHH.Local.Server.csproj
```

### 3.2 Iteración 1: Crear Componentes de Formulario

**ORDEN DE EJECUCIÓN (uno por uno):**

#### Paso 1: DatosPersonalesForm.razor
```razor
@* Extraer líneas 630-698 de EmpleadoOnboarding *@
@using SGRRHH.Local.Domain.Entities

<div class="hospital-section">
    <div class="hospital-section-header">DATOS PERSONALES</div>
    <div class="hospital-section-body">
        @* Código extraído aquí *@
    </div>
</div>

@code {
    [Parameter]
    public Empleado Empleado { get; set; } = new();
    
    [Parameter]
    public EventCallback<Empleado> EmpleadoChanged { get; set; }
    
    [Parameter]
    public EventCallback<bool> OnValidationChanged { get; set; }
    
    // Métodos de validación
    private async Task NotifyChange()
    {
        await EmpleadoChanged.InvokeAsync(Empleado);
        await OnValidationChanged.InvokeAsync(ValidateForm());
    }
    
    private bool ValidateForm()
    {
        return !string.IsNullOrWhiteSpace(Empleado.Cedula)
            && !string.IsNullOrWhiteSpace(Empleado.Nombres)
            && !string.IsNullOrWhiteSpace(Empleado.Apellidos);
    }
}
```

**✅ CHECKPOINT 1:**
```bash
# Compilar
dotnet build SGRRHH.Local/SGRRHH.Local.Server/SGRRHH.Local.Server.csproj

# Verificar 0 errores
echo $? # Debe ser 0
```

#### Paso 2: DatosLaboralesForm.razor
```razor
@* Similar estructura *@
@code {
    [Parameter] public Empleado Empleado { get; set; } = new();
    [Parameter] public EventCallback<Empleado> EmpleadoChanged { get; set; }
    [Parameter] public List<Cargo> Cargos { get; set; } = new();
    [Parameter] public List<Departamento> Departamentos { get; set; } = new();
    [Parameter] public EventCallback<int?> OnCargoChanged { get; set; }
    
    private List<Cargo> CargosFiltrados => 
        Empleado.DepartamentoId.HasValue 
            ? Cargos.Where(c => c.DepartamentoId == Empleado.DepartamentoId).ToList()
            : new();
}
```

**✅ CHECKPOINT 2:** Compilar nuevamente

#### Paso 3: SeguridadSocialForm.razor (MÁS COMPLEJO)
```razor
@* Extraer líneas 780-878 *@
@code {
    [Parameter] public Empleado Empleado { get; set; } = new();
    [Parameter] public EventCallback<Empleado> EmpleadoChanged { get; set; }
    [Parameter] public List<CatalogoEPS> EpsList { get; set; } = new();
    [Parameter] public List<CatalogoAFP> AfpList { get; set; } = new();
    [Parameter] public List<CatalogoARL> ArlList { get; set; } = new();
    [Parameter] public List<CatalogoCajaCompensacion> CajasList { get; set; } = new();
    
    // CONSOLIDACIÓN: Evitar duplicar lógica de OnEpsChanged
    private async Task OnEpsChanged(ChangeEventArgs e)
    {
        if (int.TryParse(e.Value?.ToString(), out var id))
        {
            var eps = EpsList.FirstOrDefault(x => x.Id == id);
            if (eps != null)
            {
                Empleado.EPS = eps.Nombre;
                Empleado.CodigoEPS = eps.Codigo;
                await EmpleadoChanged.InvokeAsync(Empleado);
            }
        }
    }
    
    // Similar para AFP, ARL, Caja...
}
```

**✅ CHECKPOINT 3:** Compilar

#### Paso 4: DatosBancariosForm.razor
```razor
@* Reutilizar lógica de InformacionBancariaTab si existe similitud *@
```

**✅ CHECKPOINT 4:** Compilar

#### Paso 5: ContactoEmpleadoForm.razor
```razor
@* Formulario simple de contacto *@
```

**✅ CHECKPOINT 5:** Compilar

### 3.3 Iteración 2: Componentes de Navegación

#### Paso 6: WizardProgress.razor
```razor
<div class="hospital-progress-bar">
    <div class="hospital-progress-text">
        PASO @CurrentStep DE @TotalSteps: @GetStepName().ToUpper()
    </div>
</div>

@code {
    [Parameter] public int CurrentStep { get; set; }
    [Parameter] public int TotalSteps { get; set; }
    [Parameter] public Func<int, string> StepNameProvider { get; set; } = _ => "";
    
    private string GetStepName() => StepNameProvider?.Invoke(CurrentStep) ?? "";
}
```

**✅ CHECKPOINT 6:** Compilar

#### Paso 7: WizardNavigation.razor
```razor
<div class="hospital-footer">
    <button type="button" 
            @onclick="OnCancel" 
            disabled="@IsSaving" 
            class="hospital-btn hospital-btn-secondary">
        CANCELAR (F8)
    </button>
    <div class="hospital-btn-group">
        @if (CurrentStep > 1)
        {
            <button type="button" 
                    @onclick="OnPrevious" 
                    disabled="@IsSaving" 
                    class="hospital-btn hospital-btn-secondary">
                ANTERIOR
            </button>
        }
        @if (CurrentStep < TotalSteps)
        {
            <button type="button" 
                    @onclick="OnNext" 
                    disabled="@(!CanGoNext || IsSaving)" 
                    class="hospital-btn hospital-btn-primary">
                SIGUIENTE
            </button>
        }
        else
        {
            <button type="button" 
                    @onclick="OnFinish" 
                    disabled="@IsSaving" 
                    class="hospital-btn hospital-btn-primary">
                @(IsSaving ? "GUARDANDO..." : "GUARDAR (F5)")
            </button>
        }
    </div>
</div>

@code {
    [Parameter] public int CurrentStep { get; set; }
    [Parameter] public int TotalSteps { get; set; }
    [Parameter] public bool CanGoNext { get; set; }
    [Parameter] public bool IsSaving { get; set; }
    [Parameter] public EventCallback OnPrevious { get; set; }
    [Parameter] public EventCallback OnNext { get; set; }
    [Parameter] public EventCallback OnCancel { get; set; }
    [Parameter] public EventCallback OnFinish { get; set; }
}
```

**✅ CHECKPOINT 7:** Compilar

### 3.4 Iteración 3: Refactorizar EmpleadoOnboarding.razor

**Reducir a componente orquestador (~300 líneas):**

```razor
@page "/empleados/onboarding"
@using SGRRHH.Local.Domain.Entities
@using SGRRHH.Local.Domain.Enums
@using SGRRHH.Local.Domain.Services
@using SGRRHH.Local.Shared.Interfaces
@inject IAuthService AuthService
@inject IEmpleadoRepository EmpleadoRepo
@inject ICatalogCacheService CatalogCache
@inject NavigationManager Navigation
@inject ILogger<EmpleadoOnboarding> Logger

<PageTitle>Nuevo Empleado - SGRRHH</PageTitle>

<MessageToast @ref="messageToast" />

<div class="hospital-page-container">
    <div class="hospital-page-header">
        <h1 class="hospital-page-title">NUEVO EMPLEADO</h1>
        <div class="hospital-shortcuts-bar">
            F5=GUARDAR | F8=CANCELAR | ESC=SALIR | TAB=SIGUIENTE CAMPO
        </div>
    </div>

    <WizardProgress 
        CurrentStep="@currentStep" 
        TotalSteps="2"
        StepNameProvider="@GetStepName" />

    <div class="hospital-content">
        @if (currentStep == 1)
        {
            <DatosPersonalesForm 
                @bind-Empleado="empleado"
                OnValidationChanged="@HandleValidation" />
            
            <DatosLaboralesForm 
                @bind-Empleado="empleado"
                Cargos="@cargos"
                Departamentos="@departamentos"
                OnCargoChanged="@OnCargoChanged" />
            
            <SeguridadSocialForm 
                @bind-Empleado="empleado"
                EpsList="@epsLista"
                AfpList="@afpLista"
                ArlList="@arlLista"
                CajasList="@cajasLista" />
            
            <DatosBancariosForm 
                @bind-Empleado="empleado" />
            
            <ContactoEmpleadoForm 
                @bind-Empleado="empleado" />
        }
        else if (currentStep == 2)
        {
            @RenderStep4Revisar()
        }
    </div>

    <WizardNavigation 
        CurrentStep="@currentStep"
        TotalSteps="2"
        CanGoNext="@CanGoNext()"
        IsSaving="@isSaving"
        OnPrevious="@Anterior"
        OnNext="@Siguiente"
        OnCancel="@Cancelar"
        OnFinish="@Finalizar" />
</div>

@* Modal de confirmación (mantener como está) *@
@if (showConfirmModal)
{
    @* ... código existente ... *@
}

@code {
    // SOLO lógica de orquestación aquí (~150 líneas)
    private int currentStep = 1;
    private bool isSaving = false;
    private Empleado empleado = new();
    
    // Catálogos
    private List<Cargo> cargos = new();
    private List<Departamento> departamentos = new();
    private List<CatalogoEPS> epsLista = new();
    private List<CatalogoAFP> afpLista = new();
    private List<CatalogoARL> arlLista = new();
    private List<CatalogoCajaCompensacion> cajasLista = new();
    
    protected override async Task OnInitializedAsync()
    {
        if (!AuthService.IsAuthenticated)
        {
            Navigation.NavigateTo("/login");
            return;
        }
        
        var rolUsuario = AuthService.CurrentUser?.Rol ?? RolUsuario.Operador;
        empleado.Estado = EstadoEmpleadoService.ObtenerEstadoInicialSegunRol(rolUsuario);
        
        await CargarCatalogos();
        empleado.Codigo = await EmpleadoRepo.GetNextCodigoAsync();
    }
    
    private async Task CargarCatalogos()
    {
        cargos = await CatalogCache.GetCargosAsync();
        departamentos = await CatalogCache.GetDepartamentosAsync();
        epsLista = await CatalogCache.GetEpsAsync();
        afpLista = await CatalogCache.GetAfpAsync();
        arlLista = await CatalogCache.GetArlAsync();
        cajasLista = await CatalogCache.GetCajasCompensacionAsync();
    }
    
    private string GetStepName(int step) => step switch
    {
        1 => "Datos Básicos",
        2 => "Revisar y Confirmar",
        _ => ""
    };
    
    private bool CanGoNext()
    {
        // Delegar validación a componentes hijos
        return currentStep == 1 && ValidarStep1();
    }
    
    private bool ValidarStep1()
    {
        return !string.IsNullOrWhiteSpace(empleado.Cedula)
            && !string.IsNullOrWhiteSpace(empleado.Nombres)
            && !string.IsNullOrWhiteSpace(empleado.Apellidos)
            && empleado.FechaIngreso.HasValue
            && !string.IsNullOrWhiteSpace(empleado.EPS)
            && !string.IsNullOrWhiteSpace(empleado.AFP)
            && !string.IsNullOrWhiteSpace(empleado.ARL)
            && !string.IsNullOrWhiteSpace(empleado.CajaCompensacion)
            && empleado.SalarioBase.HasValue
            && !string.IsNullOrWhiteSpace(empleado.TelefonoCelular);
    }
    
    private void Siguiente() => currentStep++;
    private void Anterior() => currentStep--;
    
    private async Task Cancelar()
    {
        confirmMessage = "¿Cancelar operacion? Los datos ingresados se perderan.";
        confirmAction = () => Navigation.NavigateTo("/empleados");
        showConfirmModal = true;
    }
    
    private async Task Finalizar()
    {
        confirmMessage = $"¿Confirmar creacion del empleado {empleado.Codigo}?";
        confirmAction = async () => await ExecuteFinalizar();
        showConfirmModal = true;
    }
    
    private async Task ExecuteFinalizar()
    {
        // Mantener lógica de guardado existente (líneas 434-571)
        // ... (código existente)
    }
    
    // Otros métodos necesarios...
}
```

**✅ CHECKPOINT FINAL:**
```bash
# Compilación completa
dotnet build SGRRHH.Local/SGRRHH.Local.Server/SGRRHH.Local.Server.csproj

# Verificar líneas finales
wc -l EmpleadoOnboarding.razor  # Debe ser ~300 líneas
```

### 3.5 Consolidación de Redundancias

**Acción 1: Crear ValidationHelpers.cs**
```csharp
// SGRRHH.Local/SGRRHH.Local.Shared/Helpers/ValidationHelpers.cs
namespace SGRRHH.Local.Shared.Helpers;

public static class ValidationHelpers
{
    private static readonly Regex EmailRegex = new(
        @"^[^@\s]+@[^@\s]+\.[^@\s]+$", 
        RegexOptions.Compiled | RegexOptions.IgnoreCase
    );
    
    public static bool IsValidEmail(string? email)
    {
        return !string.IsNullOrWhiteSpace(email) && EmailRegex.IsMatch(email);
    }
    
    public static bool IsValidCedula(string? cedula)
    {
        return !string.IsNullOrWhiteSpace(cedula) 
            && cedula.Length >= 6 
            && cedula.All(char.IsDigit);
    }
    
    public static bool IsValidPhone(string? phone)
    {
        return !string.IsNullOrWhiteSpace(phone) 
            && phone.Length >= 7 
            && phone.All(c => char.IsDigit(c) || c == '+' || c == '-');
    }
}
```

**Acción 2: Usar ValidationHelpers en componentes**
```csharp
// Reemplazar en todos los nuevos componentes
if (ValidationHelpers.IsValidEmail(Empleado.Email))
{
    // ...
}
```

**✅ CHECKPOINT:** Compilar

### 3.6 Pruebas de Funcionalidad

**Ejecutar TODAS las pruebas del TEST_PLAN_ONBOARDING.md:**

```bash
# 1. Iniciar aplicación
dotnet run --project SGRRHH.Local/SGRRHH.Local.Server

# 2. Navegar a http://localhost:5000/empleados/onboarding

# 3. Ejecutar cada caso de prueba manualmente:
# - Crear empleado como Operador
# - Crear empleado como Aprobador
# - Validar campos requeridos
# - Validar cédula duplicada
# - Probar navegación entre pasos
# - Probar guardado
# - Probar cancelación
```

**Documentar resultados en:** `RESULTADO_PRUEBAS_ONBOARDING.md`

---

## 📝 FASE 4: DOCUMENTACIÓN Y ENTREGA (1 hora)

### 4.1 Archivos Entregables

Crear los siguientes documentos:

1. **ANALISIS_EMPLEADO_ONBOARDING.md** (Fase 1)
2. **PLAN_ARQUITECTURA_ONBOARDING.md** (Fase 2)
3. **TEST_PLAN_ONBOARDING.md** (Fase 2)
4. **RESULTADO_PRUEBAS_ONBOARDING.md** (Fase 3)
5. **REFACTOR_SUMMARY_ONBOARDING.md** (Resumen ejecutivo)

### 4.2 Contenido de REFACTOR_SUMMARY_ONBOARDING.md

```markdown
# Resumen de Refactorización: EmpleadoOnboarding.razor

## Métricas Finales
- **Líneas ANTES:** 1,843
- **Líneas DESPUÉS:** ~300
- **Reducción:** 84%
- **Componentes creados:** 7
- **Redundancias eliminadas:** 4

## Componentes Creados
1. DatosPersonalesForm.razor (150 líneas)
2. DatosLaboralesForm.razor (200 líneas)
3. SeguridadSocialForm.razor (250 líneas)
4. DatosBancariosForm.razor (120 líneas)
5. ContactoEmpleadoForm.razor (100 líneas)
6. WizardProgress.razor (50 líneas)
7. WizardNavigation.razor (80 líneas)

## Redundancias Eliminadas
1. Validación de email (consolidada en ValidationHelpers)
2. Validación de cédula (consolidada en ValidationHelpers)
3. Lógica de selección de catálogos (encapsulada en componentes)
4. Código de navegación de wizard (extraído a WizardNavigation)

## Pruebas Realizadas
- ✅ Compilación: 0 errores
- ✅ Funcionalidad: 100% operativa
- ✅ Regresiones: 0 detectadas
- ✅ Estilos: Mantenidos correctamente

## Beneficios Obtenidos
1. Código más mantenible
2. Componentes reutilizables en EmpleadoEditar.razor
3. Testing unitario ahora posible
4. Mejor rendimiento (renderizado selectivo)
5. Facilita trabajo en equipo

## Archivos Modificados/Creados
- ✅ EmpleadoOnboarding.razor (refactorizado)
- ✅ DatosPersonalesForm.razor (nuevo)
- ✅ DatosLaboralesForm.razor (nuevo)
- ✅ SeguridadSocialForm.razor (nuevo)
- ✅ DatosBancariosForm.razor (nuevo)
- ✅ ContactoEmpleadoForm.razor (nuevo)
- ✅ WizardProgress.razor (nuevo)
- ✅ WizardNavigation.razor (nuevo)
- ✅ ValidationHelpers.cs (nuevo)

## Recomendaciones Futuras
1. Aplicar mismo patrón a EmpleadoEditar.razor
2. Crear tests unitarios para cada componente nuevo
3. Considerar extraer ResumenEmpleadoOnboarding.razor
```

### 4.3 Actualizar Architecture.md

Agregar sección sobre nueva arquitectura de formularios.

---

## ⚠️ REGLAS CRÍTICAS

### ❌ NO HACER NUNCA:
1. NO modificar archivos de otros agentes (ScannerModal, EmpleadoExpediente, Permisos, ControlDiario)
2. NO eliminar funcionalidad existente
3. NO cambiar nombres de archivos principales sin documentar
4. NO hacer commit hasta validar compilación
5. NO saltarse los CHECKPOINTS de compilación

### ✅ HACER SIEMPRE:
1. Compilar después de cada componente creado
2. Mantener estilos hospitalarios existentes
3. Seguir convenciones de nombres del proyecto
4. Documentar cada cambio en REFACTOR_SUMMARY
5. Probar funcionalidad completa antes de marcar como finalizado

---

## 🚀 COMANDO DE COMPILACIÓN

```bash
# Según skill build-and-verify
dotnet build SGRRHH.Local/SGRRHH.Local.Server/SGRRHH.Local.Server.csproj --no-incremental
```

---

## 📞 COORDINACIÓN CON OTROS AGENTES

### Dependencias de Agentes:
- **Agente 2 (ScannerModal):** NO hay dependencia directa
- **Agente 3 (EmpleadoExpediente):** Podría reutilizar tus componentes de formulario después
- **Agente 4 (Permisos):** NO hay dependencia directa
- **Agente 5 (ControlDiario):** NO hay dependencia directa

### Comunicación:
Si necesitas usar código de otro agente:
1. Leer el archivo pero NO modificarlo
2. Si hay oportunidad de consolidación, documentar en REFACTOR_SUMMARY
3. Proponer creación de componente compartido en reunión posterior

---

## ✅ CHECKLIST FINAL

```markdown
- [ ] Fase 1: Investigación completada (ANALISIS_EMPLEADO_ONBOARDING.md creado)
- [ ] Fase 2: Planeación completada (PLAN_ARQUITECTURA_ONBOARDING.md creado)
- [ ] Fase 3.1: DatosPersonalesForm.razor creado y compilado ✅
- [ ] Fase 3.2: DatosLaboralesForm.razor creado y compilado ✅
- [ ] Fase 3.3: SeguridadSocialForm.razor creado y compilado ✅
- [ ] Fase 3.4: DatosBancariosForm.razor creado y compilado ✅
- [ ] Fase 3.5: ContactoEmpleadoForm.razor creado y compilado ✅
- [ ] Fase 3.6: WizardProgress.razor creado y compilado ✅
- [ ] Fase 3.7: WizardNavigation.razor creado y compilado ✅
- [ ] Fase 3.8: EmpleadoOnboarding.razor refactorizado y compilado ✅
- [ ] Fase 3.9: ValidationHelpers.cs creado ✅
- [ ] Fase 3.10: Todas las redundancias consolidadas ✅
- [ ] Fase 3.11: Pruebas de funcionalidad pasadas ✅
- [ ] Fase 4: Documentación completada (REFACTOR_SUMMARY_ONBOARDING.md creado) ✅
- [ ] Build final: 0 errores ✅
- [ ] Funcionalidad: 100% operativa ✅
```

---

## 🎯 CRITERIOS DE ACEPTACIÓN

La refactorización se considera **EXITOSA** si:

1. ✅ EmpleadoOnboarding.razor tiene ≤ 350 líneas (reducción ≥ 75%)
2. ✅ Se crearon 7 componentes nuevos funcionales
3. ✅ Build exitoso sin errores: `dotnet build` retorna 0
4. ✅ Funcionalidad 100% operativa (todas las pruebas pasan)
5. ✅ Al menos 3 redundancias eliminadas
6. ✅ Documentación completa entregada
7. ✅ NO se modificaron archivos de otros agentes

---

**INICIO DE EJECUCIÓN:** [FECHA]  
**FIN ESPERADO:** [FECHA + 2-3 días]  
**AGENTE ASIGNADO:** [NOMBRE/ID]
