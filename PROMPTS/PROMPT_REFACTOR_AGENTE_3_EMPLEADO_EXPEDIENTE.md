# 🔧 AGENTE 3: REFACTORIZACIÓN - EmpleadoExpediente.razor

## 📋 INFORMACIÓN DEL COMPONENTE

**Componente Objetivo:** `SGRRHH.Local\SGRRHH.Local.Server\Components\Pages\EmpleadoExpediente.razor`  
**Tamaño Actual:** 1,445 líneas (67 KB)  
**Complejidad:** ⚠️ MUY ALTA  
**Prioridad:** 🟠 ALTA

### Descripción
Página del expediente completo del empleado con sistema de tabs:
- Información general del empleado con foto
- Tab: Datos personales y laborales
- Tab: Información bancaria (InformacionBancariaTab)
- Tab: Seguridad social (SeguridadSocialTab)
- Tab: Contratos (ContratosTab)
- Tab: Dotación EPP (DotacionEppTab)
- Tab: Documentos del empleado
- Gestión de cambio de foto
- Estados y auditoría del empleado

### Archivos Exclusivos de Este Agente (NO TOCAR POR OTROS)
```
✅ ARCHIVOS PERMITIDOS PARA MODIFICAR/CREAR:
- SGRRHH.Local\SGRRHH.Local.Server\Components\Pages\EmpleadoExpediente.razor
- SGRRHH.Local\SGRRHH.Local.Server\Components\Tabs\* (TODOS los tabs)
- SGRRHH.Local\SGRRHH.Local.Server\Components\Expediente\EmpleadoHeader.razor (NUEVO)
- SGRRHH.Local\SGRRHH.Local.Server\Components\Expediente\EmpleadoInfoCard.razor (NUEVO)
- SGRRHH.Local\SGRRHH.Local.Server\Components\Expediente\TabsNavigation.razor (NUEVO)
- SGRRHH.Local\SGRRHH.Local.Server\Components\Expediente\DatosGeneralesTab.razor (NUEVO)
- SGRRHH.Local\SGRRHH.Local.Server\Components\Expediente\DocumentosTab.razor (NUEVO)
- SGRRHH.Local\SGRRHH.Local.Server\Components\Expediente\FotoChangeModal.razor (NUEVO)

❌ ARCHIVOS PROHIBIDOS (USADOS POR OTROS AGENTES):
- EmpleadoOnboarding.razor (Agente 1)
- ScannerModal.razor (Agente 2)
- Components/Forms/* (Agente 1 - pero PUEDE reutilizarlos)
- Permisos.razor (Agente 4)
- ControlDiario.razor (Agente 5)
```

---

## 🎯 OBJETIVOS DE REFACTORIZACIÓN

### Metas Principales
1. ✅ Reducir `EmpleadoExpediente.razor` de **1,445 líneas → ~200 líneas** (componente orquestador)
2. ✅ Extraer **8 componentes** especializados
3. ✅ **REUTILIZAR** componentes del Agente 1 (SeguridadSocialForm, etc.)
4. ✅ Consolidar lógica duplicada con EmpleadoOnboarding
5. ✅ Mejorar sistema de tabs (más mantenible)
6. ✅ Mantener 100% de funcionalidad
7. ✅ Compilación sin errores

### KPIs de Éxito
- **Reducción de líneas:** Mínimo 85% en archivo principal
- **Componentes creados:** 8 nuevos
- **Componentes reutilizados:** Mínimo 3 del Agente 1
- **Redundancias eliminadas:** Mínimo 4
- **Tests:** 0 errores de build
- **Funcionalidad:** 100% operativa

---

## 📊 FASE 1: INVESTIGACIÓN (2-3 horas)

### 1.1 Análisis Estructural

**Tareas:**
```bash
# 1. Mapear estructura del expediente
- Identificar header con foto (líneas 40-120)
- Identificar card de información básica (líneas 120-250)
- Identificar sistema de tabs (líneas 250-350)
- Identificar contenido de cada tab
- Mapear lógica de gestión de estado
```

**Deliverable 1.1:** Archivo `ANALISIS_EMPLEADO_EXPEDIENTE.md` con:
- Mapa de secciones (líneas inicio-fin)
- Lista de tabs y su contenido
- Dependencias de servicios
- Variables de estado

### 1.2 Análisis de Tabs Existentes

**Investigar tabs ya existentes como componentes separados:**
```bash
# Listar tabs existentes
ls -la SGRRHH.Local/SGRRHH.Local.Server/Components/Tabs/

# Identificados:
# - InformacionBancariaTab.razor
# - SeguridadSocialTab.razor
# - ContratosTab.razor
# - DotacionEppTab.razor
```

**Pregunta clave:** ¿Estos tabs están funcionando correctamente o necesitan refactorización también?

**Deliverable 1.2:** Sección en `ANALISIS_EMPLEADO_EXPEDIENTE.md`:
- Estado de cada tab existente
- Tabs que funcionan bien (mantener)
- Tabs que necesitan refactorización
- Tabs que faltan crear

### 1.3 Búsqueda de Código Duplicado con Agente 1

**IMPORTANTE:** Coordinación con EmpleadoOnboarding.razor

**Comparar:**
1. **Formulario de datos personales:** ¿Es similar entre Onboarding y Expediente?
2. **Formulario de seguridad social:** ¿Es el mismo?
3. **Lógica de validación:** ¿Se repite?
4. **Cambio de foto:** ¿Código duplicado?

**Tareas:**
```bash
# Comparar secciones de seguridad social
diff EmpleadoOnboarding.razor EmpleadoExpediente.razor | grep -A 5 "EPS\|AFP"

# Buscar validaciones duplicadas
grep -n "IsNullOrWhiteSpace\|Cedula\|Email" EmpleadoOnboarding.razor EmpleadoExpediente.razor
```

**Deliverable 1.3:** Tabla de comparación:

| Funcionalidad | Onboarding | Expediente | Duplicado? | Acción |
|---------------|------------|------------|------------|---------|
| Datos personales | Sí | Sí | ❌ Similar | Reutilizar DatosPersonalesForm |
| Seguridad social | Sí | Sí | ✅ Igual | Reutilizar SeguridadSocialForm |
| Datos bancarios | Sí | Sí (tab) | ⚠️ Parcial | Evaluar reutilización |
| Validación cédula | Sí | Sí | ✅ Igual | Reutilizar ValidationHelpers |
| Cambio de foto | No | Sí | - | Mantener único |

### 1.4 Revisión de Skills

**Leer:**
```bash
.cursor/skills/blazor-component/SKILL.md
.cursor/skills/hospital-ui-style/SKILL.md
.cursor/skills/build-and-verify/SKILL.md
```

**Deliverable 1.4:** Checklist completado

---

## 🗺️ FASE 2: PLANEACIÓN (2-3 horas)

### 2.1 Diseño de Arquitectura

**Árbol de componentes propuesto:**

```
EmpleadoExpediente.razor (Orquestador - ~200 líneas)
│
├─ <EmpleadoHeader 
│     Empleado="@empleado"
│     OnChangeFoto="@AbrirCambiarFoto"
│     OnVolver="@Volver" />
│
├─ <EmpleadoInfoCard 
│     Empleado="@empleado"
│     OnEstadoChanged="@RefreshEmpleado" />
│
├─ <TabsNavigation 
│     ActiveTab="@activeTab"
│     OnTabChanged="@ChangeTab"
│     Tabs="@availableTabs" />
│
├─ CONTENIDO DE TAB ACTIVO:
│  │
│  ├─ @if (activeTab == "general")
│  │  {
│  │      <DatosGeneralesTab 
│  │          @bind-Empleado="empleado"
│  │          OnSave="@SaveDatosGenerales" />
│  │  }
│  │
│  ├─ @if (activeTab == "bancaria")
│  │  {
│  │      <InformacionBancariaTab 
│  │          EmpleadoId="@empleado.Id" />  @* YA EXISTE *@
│  │  }
│  │
│  ├─ @if (activeTab == "seguridad-social")
│  │  {
│  │      <SeguridadSocialTab 
│  │          EmpleadoId="@empleado.Id" />  @* YA EXISTE *@
│  │  }
│  │
│  ├─ @if (activeTab == "contratos")
│  │  {
│  │      <ContratosTab 
│  │          EmpleadoId="@empleado.Id" />  @* YA EXISTE *@
│  │  }
│  │
│  ├─ @if (activeTab == "dotacion")
│  │  {
│  │      <DotacionEppTab 
│  │          EmpleadoId="@empleado.Id" />  @* YA EXISTE *@
│  │  }
│  │
│  └─ @if (activeTab == "documentos")
│     {
│         <DocumentosTab 
│             EmpleadoId="@empleado.Id"
│             OnUploadDocument="@HandleUploadDocument" />
│     }
│
└─ <FotoChangeModal 
      @ref="fotoModal"
      EmpleadoId="@empleado.Id"
      OnFotoChanged="@RefreshFoto" />
```

**Deliverable 2.1:** Archivo `PLAN_ARQUITECTURA_EXPEDIENTE.md` con diagrama completo

### 2.2 Plan de Reutilización de Componentes del Agente 1

**ESTRATEGIA CLAVE:** No duplicar, reutilizar

**Componentes a reutilizar:**
1. **ValidationHelpers.cs** (Agente 1)
   - Usar para validación de cédula, email, teléfono

2. **SeguridadSocialForm** (Agente 1) - SI FUE CREADO
   - Considerar usar dentro de DatosGeneralesTab o como tab independiente
   - ⚠️ Verificar si Agente 1 lo creó compatible para edición (no solo creación)

3. **DatosPersonalesForm** (Agente 1) - SI FUE CREADO
   - Reutilizar para sección de datos generales

4. **InputCedula, InputMoneda, InputUpperCase** (ya existen en Shared)
   - Usar en todos los formularios

**Deliverable 2.2:** Sección "Reutilización" en `PLAN_ARQUITECTURA_EXPEDIENTE.md`:
- Componentes que se pueden reutilizar directamente
- Componentes que necesitan adaptación
- Componentes que hay que crear desde cero

### 2.3 Diseño de Nuevos Componentes

#### EmpleadoHeader.razor
```razor
@* Header del expediente con foto y botones *@
<div class="expediente-header">
    <div class="expediente-info">
        <div class="expediente-foto" @onclick="OnChangeFoto" title="Click para cambiar foto">
            @* Foto del empleado *@
        </div>
        <div class="expediente-datos-principales">
            <h1>@Empleado.NombreCompleto</h1>
            <div class="expediente-metadata">
                <span>Código: @Empleado.Codigo</span>
                <span>Cédula: @Empleado.Cedula</span>
                <EstadoBadge Estado="@Empleado.Estado" />
            </div>
        </div>
    </div>
    <div class="expediente-actions">
        <button @onclick="OnVolver" class="hospital-btn hospital-btn-secondary">
            ← VOLVER
        </button>
    </div>
</div>

@code {
    [Parameter] public Empleado Empleado { get; set; } = new();
    [Parameter] public EventCallback OnChangeFoto { get; set; }
    [Parameter] public EventCallback OnVolver { get; set; }
}
```

#### TabsNavigation.razor
```razor
@* Sistema de tabs reutilizable *@
<div class="expediente-tabs">
    @foreach (var tab in Tabs)
    {
        <button class="expediente-tab @(tab.Id == ActiveTab ? "active" : "")" 
                @onclick="() => OnTabChanged.InvokeAsync(tab.Id)">
            @tab.Icon @tab.Label
        </button>
    }
</div>

@code {
    [Parameter] public string ActiveTab { get; set; } = "";
    [Parameter] public List<TabDefinition> Tabs { get; set; } = new();
    [Parameter] public EventCallback<string> OnTabChanged { get; set; }
    
    public class TabDefinition
    {
        public string Id { get; set; } = "";
        public string Label { get; set; } = "";
        public string Icon { get; set; } = "";
        public bool Visible { get; set; } = true;
    }
}
```

**Deliverable 2.3:** Especificación completa de cada componente nuevo

### 2.4 Plan de Consolidación

**Redundancias a eliminar:**

1. **Validación de empleado:**
   - ❌ ANTES: Código duplicado en Onboarding y Expediente
   - ✅ DESPUÉS: Usar `ValidationHelpers` centralizado

2. **Formato de nombre completo:**
   - ❌ ANTES: `$"{empleado.Nombres} {empleado.Apellidos}"` en varios lugares
   - ✅ DESPUÉS: Propiedad `NombreCompleto` en entidad Empleado

3. **Obtención de iniciales:**
   - ❌ ANTES: Método `GetInitials()` duplicado
   - ✅ DESPUÉS: Helper estático `StringHelpers.GetInitials(string nombre)`

4. **Manejo de foto:**
   - ❌ ANTES: Lógica repetida de upload/preview
   - ✅ DESPUÉS: Componente `FotoChangeModal` reutilizable

**Deliverable 2.4:** Sección "Consolidaciones" en `PLAN_ARQUITECTURA_EXPEDIENTE.md`

### 2.5 Plan de Pruebas

**Checklist:**
```markdown
- [ ] Compilación: 0 errores
- [ ] Cargar expediente: Datos se muestran correctamente
- [ ] Navegación tabs: Todos los tabs funcionan
- [ ] Tab General: Edición de datos funciona
- [ ] Tab Bancaria: Carga y funciona (ya existe)
- [ ] Tab Seguridad Social: Carga y funciona (ya existe)
- [ ] Tab Contratos: Carga y funciona (ya existe)
- [ ] Tab Dotación: Carga y funciona (ya existe)
- [ ] Tab Documentos: Upload funciona
- [ ] Cambio de foto: Modal abre y funciona
- [ ] Cambio de foto: Preview funciona
- [ ] Cambio de foto: Guardar funciona
- [ ] Estado empleado: Badge muestra correctamente
- [ ] Botón volver: Navega a /empleados
- [ ] Estilos: Mantiene diseño hospitalario
```

**Deliverable 2.5:** Archivo `TEST_PLAN_EXPEDIENTE.md`

---

## ⚙️ FASE 3: EJECUCIÓN CONTROLADA (8-10 horas)

### 3.1 Preparación

```bash
# 1. Crear carpetas
mkdir -p SGRRHH.Local/SGRRHH.Local.Server/Components/Expediente

# 2. Backup
cp EmpleadoExpediente.razor EmpleadoExpediente.razor.BACKUP

# 3. Compilar ANTES
dotnet build SGRRHH.Local/SGRRHH.Local.Server/SGRRHH.Local.Server.csproj
```

### 3.2 Iteración 1: Componentes de UI

#### Paso 1: EmpleadoHeader.razor
```razor
@using SGRRHH.Local.Domain.Entities

<div class="expediente-header">
    <div class="expediente-info">
        <div class="expediente-foto" @onclick="OnChangeFoto" title="Click para cambiar foto">
            @if (!string.IsNullOrEmpty(Empleado.FotoPath))
            {
                <img src="@GetFotoUrl()" alt="@Empleado.NombreCompleto" 
                     onerror="this.style.display='none'; this.parentElement.innerHTML='@GetInitials()';" />
            }
            else
            {
                <div class="expediente-foto-placeholder">
                    @GetInitials()
                </div>
            }
        </div>
        <div class="expediente-datos-principales">
            <h1>@Empleado.NombreCompleto</h1>
            <div class="expediente-metadata">
                <span><strong>CÓDIGO:</strong> @Empleado.Codigo</span>
                <span><strong>CÉDULA:</strong> @Empleado.Cedula</span>
                <EstadoBadge Estado="@Empleado.Estado" />
            </div>
        </div>
    </div>
    <div class="expediente-actions">
        <button @onclick="OnVolver" class="hospital-btn hospital-btn-secondary">
            ← VOLVER
        </button>
    </div>
</div>

@code {
    [Parameter] public Empleado Empleado { get; set; } = new();
    [Parameter] public EventCallback OnChangeFoto { get; set; }
    [Parameter] public EventCallback OnVolver { get; set; }
    
    private string GetFotoUrl()
    {
        return $"/api/storage/empleados/{Empleado.Id}/foto?t={DateTime.Now.Ticks}";
    }
    
    private string GetInitials()
    {
        if (string.IsNullOrWhiteSpace(Empleado.Nombres) || string.IsNullOrWhiteSpace(Empleado.Apellidos))
            return "?";
        
        return $"{Empleado.Nombres[0]}{Empleado.Apellidos[0]}".ToUpper();
    }
}
```

**✅ CHECKPOINT 1:** Compilar

#### Paso 2: TabsNavigation.razor
```razor
<div class="expediente-tabs">
    @foreach (var tab in Tabs.Where(t => t.Visible))
    {
        <button class="expediente-tab @(tab.Id == ActiveTab ? "active" : "")" 
                @onclick="() => OnTabChanged.InvokeAsync(tab.Id)"
                title="@tab.Label">
            <span class="tab-icon">@tab.Icon</span>
            <span class="tab-label">@tab.Label</span>
        </button>
    }
</div>

@code {
    [Parameter] public string ActiveTab { get; set; } = "";
    [Parameter] public List<TabDefinition> Tabs { get; set; } = new();
    [Parameter] public EventCallback<string> OnTabChanged { get; set; }
}

public class TabDefinition
{
    public string Id { get; set; } = "";
    public string Label { get; set; } = "";
    public string Icon { get; set; } = "📄";
    public bool Visible { get; set; } = true;
}
```

**✅ CHECKPOINT 2:** Compilar

#### Paso 3: EmpleadoInfoCard.razor
```razor
@* Card con información resumida y estado *@
@using SGRRHH.Local.Domain.Entities
@using SGRRHH.Local.Domain.Enums

<div class="expediente-info-card">
    <div class="info-section">
        <h3>Información Laboral</h3>
        <div class="info-grid">
            <div class="info-item">
                <span class="info-label">Cargo:</span>
                <span class="info-value">@Empleado.Cargo</span>
            </div>
            <div class="info-item">
                <span class="info-label">Departamento:</span>
                <span class="info-value">@Empleado.Departamento</span>
            </div>
            <div class="info-item">
                <span class="info-label">Fecha Ingreso:</span>
                <span class="info-value">@Empleado.FechaIngreso?.ToString("dd/MM/yyyy")</span>
            </div>
            <div class="info-item">
                <span class="info-label">Salario:</span>
                <span class="info-value">@FormatCurrency(Empleado.SalarioBase)</span>
            </div>
        </div>
    </div>
    
    <div class="info-section">
        <h3>Contacto</h3>
        <div class="info-grid">
            <div class="info-item">
                <span class="info-label">Celular:</span>
                <span class="info-value">@Empleado.TelefonoCelular</span>
            </div>
            <div class="info-item">
                <span class="info-label">Email:</span>
                <span class="info-value">@Empleado.Email</span>
            </div>
        </div>
    </div>
    
    @if (ShowEstadoControl)
    {
        <div class="info-section">
            <h3>Estado y Auditoría</h3>
            <EstadoBadge Estado="@Empleado.Estado" />
            @if (OnEstadoChanged.HasDelegate)
            {
                <button @onclick="OnEstadoChanged" class="btn-change-estado">
                    Cambiar Estado
                </button>
            }
        </div>
    }
</div>

@code {
    [Parameter] public Empleado Empleado { get; set; } = new();
    [Parameter] public bool ShowEstadoControl { get; set; } = true;
    [Parameter] public EventCallback OnEstadoChanged { get; set; }
    
    private string FormatCurrency(decimal? value)
    {
        return value.HasValue ? $"${value.Value:N0}" : "N/A";
    }
}
```

**✅ CHECKPOINT 3:** Compilar

#### Paso 4: DatosGeneralesTab.razor
```razor
@* Tab de datos generales - REUTILIZAR componentes del Agente 1 *@
@using SGRRHH.Local.Domain.Entities

<div class="tab-content">
    <div class="tab-header">
        <h2>DATOS GENERALES</h2>
        <button @onclick="Save" disabled="@isSaving" class="hospital-btn hospital-btn-primary">
            @(isSaving ? "GUARDANDO..." : "GUARDAR CAMBIOS")
        </button>
    </div>
    
    @* REUTILIZACIÓN: Usar componentes del Agente 1 *@
    <DatosPersonalesForm 
        @bind-Empleado="empleadoLocal"
        OnValidationChanged="@HandleValidation" />
    
    <DatosLaboralesForm 
        @bind-Empleado="empleadoLocal"
        Cargos="@cargos"
        Departamentos="@departamentos" />
    
    <ContactoEmpleadoForm 
        @bind-Empleado="empleadoLocal" />
</div>

@code {
    [Parameter] public Empleado Empleado { get; set; } = new();
    [Parameter] public EventCallback<Empleado> EmpleadoChanged { get; set; }
    [Parameter] public EventCallback<Empleado> OnSave { get; set; }
    
    private Empleado empleadoLocal = new();
    private bool isSaving = false;
    private bool isValid = false;
    
    // Catálogos
    private List<Cargo> cargos = new();
    private List<Departamento> departamentos = new();
    
    protected override void OnParametersSet()
    {
        empleadoLocal = Empleado; // Clonar para edición local
    }
    
    private void HandleValidation(bool valid)
    {
        isValid = valid;
    }
    
    private async Task Save()
    {
        if (!isValid) return;
        
        isSaving = true;
        await OnSave.InvokeAsync(empleadoLocal);
        isSaving = false;
    }
}
```

**✅ CHECKPOINT 4:** Compilar

#### Paso 5: DocumentosTab.razor
```razor
@* Tab de documentos del empleado *@
@using SGRRHH.Local.Domain.Entities
@using SGRRHH.Local.Domain.Enums
@inject IDocumentoEmpleadoRepository DocumentoRepo
@inject IDocumentoStorageService StorageService

<div class="tab-content">
    <div class="tab-header">
        <h2>DOCUMENTOS DEL EMPLEADO</h2>
        <button @onclick="AbrirUpload" class="hospital-btn hospital-btn-primary">
            + SUBIR DOCUMENTO
        </button>
    </div>
    
    @if (isLoading)
    {
        <div class="loading">Cargando documentos...</div>
    }
    else if (!documentos.Any())
    {
        <div class="empty-state">
            No hay documentos cargados para este empleado.
        </div>
    }
    else
    {
        <DataTable 
            Items="@documentos"
            Columns="@tableColumns"
            OnRowAction="@HandleRowAction" />
    }
</div>

@* Modal de upload (usar ScannerModal del Agente 2 si necesario) *@
@if (showUploadModal)
{
    <div class="modal">
        @* Formulario de upload *@
    </div>
}

@code {
    [Parameter] public int EmpleadoId { get; set; }
    [Parameter] public EventCallback<DocumentoEmpleado> OnUploadDocument { get; set; }
    
    private List<DocumentoEmpleado> documentos = new();
    private bool isLoading = true;
    private bool showUploadModal = false;
    
    protected override async Task OnInitializedAsync()
    {
        await LoadDocumentos();
    }
    
    private async Task LoadDocumentos()
    {
        isLoading = true;
        documentos = await DocumentoRepo.GetByEmpleadoIdAsync(EmpleadoId);
        isLoading = false;
    }
    
    private void AbrirUpload()
    {
        showUploadModal = true;
    }
    
    private void HandleRowAction(string action, DocumentoEmpleado doc)
    {
        // Descargar, ver, eliminar...
    }
}
```

**✅ CHECKPOINT 5:** Compilar

#### Paso 6: FotoChangeModal.razor
```razor
@* Modal para cambiar foto del empleado *@
@using Microsoft.AspNetCore.Components.Forms
@inject ILocalStorageService StorageService
@inject IEmpleadoRepository EmpleadoRepo

@if (IsVisible)
{
    <div class="modal-backdrop" @onclick="Close">
        <div class="modal-content" @onclick:stopPropagation="true">
            <div class="modal-header">
                <h2>CAMBIAR FOTO</h2>
                <button @onclick="Close" class="modal-close">✕</button>
            </div>
            
            <div class="modal-body">
                <InputFile OnChange="@HandleFileSelected" accept="image/*" />
                
                @if (!string.IsNullOrEmpty(previewUrl))
                {
                    <div class="foto-preview">
                        <img src="@previewUrl" alt="Preview" />
                    </div>
                }
            </div>
            
            <div class="modal-footer">
                <button @onclick="Close" class="hospital-btn hospital-btn-secondary">
                    CANCELAR
                </button>
                <button @onclick="Save" 
                        disabled="@(fotoFile == null || isSaving)" 
                        class="hospital-btn hospital-btn-primary">
                    @(isSaving ? "GUARDANDO..." : "GUARDAR")
                </button>
            </div>
        </div>
    </div>
}

@code {
    [Parameter] public int EmpleadoId { get; set; }
    [Parameter] public EventCallback OnFotoChanged { get; set; }
    
    public bool IsVisible { get; private set; }
    
    private IBrowserFile? fotoFile;
    private string? previewUrl;
    private bool isSaving;
    
    public void Open()
    {
        IsVisible = true;
        StateHasChanged();
    }
    
    public void Close()
    {
        IsVisible = false;
        fotoFile = null;
        previewUrl = null;
        StateHasChanged();
    }
    
    private async Task HandleFileSelected(InputFileChangeEventArgs e)
    {
        fotoFile = e.File;
        
        // Preview
        var buffer = new byte[fotoFile.Size];
        await fotoFile.OpenReadStream(2 * 1024 * 1024).ReadAsync(buffer);
        previewUrl = $"data:{fotoFile.ContentType};base64,{Convert.ToBase64String(buffer)}";
        StateHasChanged();
    }
    
    private async Task Save()
    {
        if (fotoFile == null) return;
        
        isSaving = true;
        
        try
        {
            using var stream = fotoFile.OpenReadStream(2 * 1024 * 1024);
            using var ms = new MemoryStream();
            await stream.CopyToAsync(ms);
            
            var extension = Path.GetExtension(fotoFile.Name).ToLowerInvariant();
            var result = await StorageService.SaveEmpleadoFotoAsync(EmpleadoId, ms.ToArray(), extension);
            
            if (result.IsSuccess)
            {
                await OnFotoChanged.InvokeAsync();
                Close();
            }
        }
        catch (Exception ex)
        {
            // Log error
        }
        finally
        {
            isSaving = false;
        }
    }
}
```

**✅ CHECKPOINT 6:** Compilar

### 3.3 Iteración 2: Refactorizar EmpleadoExpediente.razor

**Reducir a orquestador (~200 líneas):**

```razor
@page "/empleados/{EmpleadoId:int}/expediente"
@using SGRRHH.Local.Domain.Entities
@using SGRRHH.Local.Domain.Enums
@using SGRRHH.Local.Shared.Interfaces
@inject IAuthService AuthService
@inject IEmpleadoRepository EmpleadoRepo
@inject NavigationManager Navigation
@inject ILogger<EmpleadoExpediente> Logger

<MessageToast @ref="messageToast" />

@if (isLoading)
{
    <div class="loading-message">CARGANDO EXPEDIENTE...</div>
}
else if (empleado == null)
{
    <div class="error-message">EMPLEADO NO ENCONTRADO</div>
    <button class="btn" @onclick="Volver">VOLVER A EMPLEADOS</button>
}
else
{
    <div class="expediente-container">
        <EmpleadoHeader 
            Empleado="@empleado"
            OnChangeFoto="@AbrirCambiarFoto"
            OnVolver="@Volver" />
        
        <EmpleadoInfoCard 
            Empleado="@empleado"
            ShowEstadoControl="true"
            OnEstadoChanged="@HandleEstadoChange" />
        
        <TabsNavigation 
            ActiveTab="@activeTab"
            Tabs="@availableTabs"
            OnTabChanged="@ChangeTab" />
        
        <div class="expediente-tab-content">
            @switch (activeTab)
            {
                case "general":
                    <DatosGeneralesTab 
                        @bind-Empleado="empleado"
                        OnSave="@SaveDatosGenerales" />
                    break;
                
                case "bancaria":
                    <InformacionBancariaTab EmpleadoId="@empleado.Id" />
                    break;
                
                case "seguridad-social":
                    <SeguridadSocialTab EmpleadoId="@empleado.Id" />
                    break;
                
                case "contratos":
                    <ContratosTab EmpleadoId="@empleado.Id" />
                    break;
                
                case "dotacion":
                    <DotacionEppTab EmpleadoId="@empleado.Id" />
                    break;
                
                case "documentos":
                    <DocumentosTab 
                        EmpleadoId="@empleado.Id"
                        OnUploadDocument="@HandleUploadDocument" />
                    break;
            }
        </div>
    </div>
}

<FotoChangeModal @ref="fotoModal" 
                 EmpleadoId="@(empleado?.Id ?? 0)"
                 OnFotoChanged="@RefreshFoto" />

@code {
    [Parameter] public int EmpleadoId { get; set; }
    
    private Empleado? empleado;
    private bool isLoading = true;
    private string activeTab = "general";
    private FotoChangeModal? fotoModal;
    private MessageToast? messageToast;
    
    private List<TabDefinition> availableTabs = new()
    {
        new() { Id = "general", Label = "Datos Generales", Icon = "👤" },
        new() { Id = "bancaria", Label = "Información Bancaria", Icon = "🏦" },
        new() { Id = "seguridad-social", Label = "Seguridad Social", Icon = "🏥" },
        new() { Id = "contratos", Label = "Contratos", Icon = "📄" },
        new() { Id = "dotacion", Label = "Dotación EPP", Icon = "🦺" },
        new() { Id = "documentos", Label = "Documentos", Icon = "📁" }
    };
    
    protected override async Task OnInitializedAsync()
    {
        if (!AuthService.IsAuthenticated)
        {
            Navigation.NavigateTo("/login");
            return;
        }
        
        await LoadEmpleado();
    }
    
    private async Task LoadEmpleado()
    {
        isLoading = true;
        try
        {
            empleado = await EmpleadoRepo.GetByIdAsync(EmpleadoId);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error cargando empleado {Id}", EmpleadoId);
            messageToast?.ShowError("Error al cargar empleado");
        }
        finally
        {
            isLoading = false;
        }
    }
    
    private void ChangeTab(string tabId)
    {
        activeTab = tabId;
    }
    
    private void AbrirCambiarFoto()
    {
        fotoModal?.Open();
    }
    
    private async Task RefreshFoto()
    {
        await LoadEmpleado();
        StateHasChanged();
    }
    
    private async Task SaveDatosGenerales(Empleado empleadoActualizado)
    {
        try
        {
            await EmpleadoRepo.UpdateAsync(empleadoActualizado);
            empleado = empleadoActualizado;
            messageToast?.ShowSuccess("Datos guardados correctamente");
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error guardando empleado");
            messageToast?.ShowError("Error al guardar datos");
        }
    }
    
    private void HandleEstadoChange()
    {
        // Lógica de cambio de estado
    }
    
    private void HandleUploadDocument(DocumentoEmpleado doc)
    {
        messageToast?.ShowSuccess($"Documento {doc.Nombre} subido correctamente");
    }
    
    private void Volver()
    {
        Navigation.NavigateTo("/empleados");
    }
}
```

**✅ CHECKPOINT FINAL:**
```bash
dotnet build SGRRHH.Local/SGRRHH.Local.Server/SGRRHH.Local.Server.csproj
wc -l EmpleadoExpediente.razor  # Debe ser ~200 líneas
```

### 3.4 Consolidación de Redundancias

**Acción 1: Crear StringHelpers.cs**
```csharp
// SGRRHH.Local/SGRRHH.Local.Shared/Helpers/StringHelpers.cs
namespace SGRRHH.Local.Shared.Helpers;

public static class StringHelpers
{
    public static string GetInitials(string firstName, string lastName)
    {
        if (string.IsNullOrWhiteSpace(firstName) || string.IsNullOrWhiteSpace(lastName))
            return "?";
        
        return $"{firstName[0]}{lastName[0]}".ToUpper();
    }
    
    public static string GetFullName(string firstName, string lastName)
    {
        return $"{firstName} {lastName}".Trim();
    }
}
```

**Acción 2: Actualizar entidad Empleado**
```csharp
// Agregar propiedad computada en Empleado.cs
public string NombreCompleto => StringHelpers.GetFullName(Nombres, Apellidos);
```

**✅ CHECKPOINT:** Compilar

### 3.5 Pruebas de Funcionalidad

Ejecutar TODAS las pruebas del `TEST_PLAN_EXPEDIENTE.md`

**Documentar en:** `RESULTADO_PRUEBAS_EXPEDIENTE.md`

---

## 📝 FASE 4: DOCUMENTACIÓN Y ENTREGA (1 hora)

### 4.1 Archivos Entregables
1. **ANALISIS_EMPLEADO_EXPEDIENTE.md**
2. **PLAN_ARQUITECTURA_EXPEDIENTE.md**
3. **TEST_PLAN_EXPEDIENTE.md**
4. **RESULTADO_PRUEBAS_EXPEDIENTE.md**
5. **REFACTOR_SUMMARY_EXPEDIENTE.md**

### 4.2 Contenido de REFACTOR_SUMMARY_EXPEDIENTE.md
```markdown
# Resumen de Refactorización: EmpleadoExpediente.razor

## Métricas Finales
- **Líneas ANTES:** 1,445
- **Líneas DESPUÉS:** ~200
- **Reducción:** 86%
- **Componentes creados:** 8
- **Componentes reutilizados:** 3 (del Agente 1)

## Componentes Creados
1. EmpleadoHeader.razor
2. EmpleadoInfoCard.razor
3. TabsNavigation.razor
4. DatosGeneralesTab.razor
5. DocumentosTab.razor
6. FotoChangeModal.razor

## Componentes Reutilizados (Agente 1)
1. DatosPersonalesForm.razor
2. DatosLaboralesForm.razor
3. ContactoEmpleadoForm.razor
4. ValidationHelpers.cs

## Tabs Existentes Mantenidos
1. InformacionBancariaTab.razor ✅
2. SeguridadSocialTab.razor ✅
3. ContratosTab.razor ✅
4. DotacionEppTab.razor ✅

## Redundancias Eliminadas
1. Método GetInitials → StringHelpers
2. Propiedad NombreCompleto → Empleado.cs
3. Lógica de upload de foto → FotoChangeModal
4. Validaciones → ValidationHelpers (reutilizado)

## Pruebas Realizadas
- ✅ Compilación: 0 errores
- ✅ Funcionalidad: 100% operativa
- ✅ Navegación tabs: Funciona
- ✅ Edición: Funciona
- ✅ Cambio de foto: Funciona
```

---

## ⚠️ REGLAS CRÍTICAS

### ❌ NO HACER:
1. NO modificar archivos de Agente 1, 2, 4, 5
2. NO cambiar tabs existentes que funcionan
3. NO eliminar funcionalidad de expediente
4. NO hacer commit sin compilación

### ✅ HACER SIEMPRE:
1. REUTILIZAR componentes del Agente 1
2. Compilar después de cada componente
3. Mantener estilos hospitalarios
4. Documentar cambios

---

## ✅ CHECKLIST FINAL
```markdown
- [ ] Fase 1: Investigación completada
- [ ] Fase 2: Planeación completada
- [ ] EmpleadoHeader.razor creado ✅
- [ ] EmpleadoInfoCard.razor creado ✅
- [ ] TabsNavigation.razor creado ✅
- [ ] DatosGeneralesTab.razor creado ✅
- [ ] DocumentosTab.razor creado ✅
- [ ] FotoChangeModal.razor creado ✅
- [ ] StringHelpers.cs creado ✅
- [ ] EmpleadoExpediente.razor refactorizado ✅
- [ ] Todas las pruebas pasadas ✅
- [ ] Documentación completada ✅
- [ ] Build: 0 errores ✅
```

---

**INICIO:** [FECHA]  
**FIN ESPERADO:** [FECHA + 2-3 días]  
**AGENTE ASIGNADO:** [NOMBRE/ID]
