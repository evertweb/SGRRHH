---
name: blazor-component
description: Patrones y convenciones para crear componentes Blazor en SGRRHH. Usar al crear páginas, tabs, formularios o componentes reutilizables.
---
# Componentes Blazor - SGRRHH

## Arquitectura del Proyecto

| Capa | Proyecto | Responsabilidad |
|------|----------|-----------------|
| Domain | `SGRRHH.Local.Domain` | Entidades, Enums, Interfaces |
| Shared | `SGRRHH.Local.Shared` | DTOs, Validaciones |
| Infrastructure | `SGRRHH.Local.Infrastructure` | Repositorios, Servicios |
| Presentation | `SGRRHH.Local.Server` | UI Blazor, Pages, Components |

**ORM:** Dapper (NO Entity Framework)
**BD:** SQLite

## Estructura de Componentes

```
SGRRHH.Local.Server/Components/
├── App.razor              # Entrada de la app
├── Routes.razor           # Configuración de rutas
├── _Imports.razor         # Imports globales
├── Layout/
│   └── MainLayout.razor   # Layout principal
├── Pages/                 # Páginas con @page
│   ├── Empleados.razor
│   ├── EmpleadoExpediente.razor
│   └── ...
├── Tabs/                  # Tabs del expediente
│   ├── InformacionBancariaTab.razor
│   └── ...
└── Shared/                # Componentes reutilizables
    ├── ModalConfirmacion.razor
    └── ...
```

## Convenciones de Nomenclatura

| Elemento | Formato | Ejemplo |
|----------|---------|---------|
| Páginas | PascalCase | `Empleados.razor` |
| Tabs | PascalCase + Tab | `InformacionBancariaTab.razor` |
| Componentes | PascalCase | `SelectorEstado.razor` |
| Variables | camelCase | `empleadoActual` |
| Métodos | PascalCase | `CargarEmpleados()` |
| Propiedades | PascalCase | `EmpleadoId` |
| Parámetros | PascalCase | `[Parameter] public int Id { get; set; }` |

## Idioma

> [!IMPORTANT]
> Todo en **español**: código, comentarios, UI, mensajes de error.

```csharp
// ✅ Correcto
private bool mostrarFormulario = false;
private string mensajeError = "";

// ❌ Incorrecto
private bool showForm = false;
private string errorMessage = "";
```

## Patrón Code-Behind

Separar la lógica en archivo `.razor.cs`:

### MiComponente.razor
```razor
@page "/mi-pagina"
@inherits MiComponenteBase

<div class="page-container">
    <h1 class="page-title">MI PÁGINA</h1>
    
    @if (cargando)
    {
        <div class="loading-block">CARGANDO...</div>
    }
    else
    {
        @foreach (var item in items)
        {
            <div>@item.Nombre</div>
        }
    }
</div>
```

### MiComponente.razor.cs
```csharp
public partial class MiComponente : ComponentBase
{
    [Inject] private IServicio Servicio { get; set; } = default!;
    [Inject] private NavigationManager Navigation { get; set; } = default!;
    
    private List<EntidadDto> items = new();
    private bool cargando = true;
    private string? mensajeError;
    
    protected override async Task OnInitializedAsync()
    {
        try
        {
            items = await Servicio.ObtenerTodosAsync();
        }
        catch (Exception ex)
        {
            mensajeError = $"Error al cargar: {ex.Message}";
        }
        finally
        {
            cargando = false;
        }
    }
}
```

## Inyección de Dependencias

Los servicios se registran en `Program.cs` (líneas 19-155):

```csharp
// En el componente
[Inject] private IEmpleadoRepository EmpleadoRepo { get; set; } = default!;
[Inject] private ILiquidacionService LiquidacionService { get; set; } = default!;
[Inject] private NavigationManager Navigation { get; set; } = default!;
[Inject] private IJSRuntime JS { get; set; } = default!;
```

## Parámetros de Componentes

### Parámetro simple
```csharp
[Parameter] public int EmpleadoId { get; set; }
```

### Parámetro con callback
```csharp
[Parameter] public EventCallback<bool> OnGuardado { get; set; }

// Invocar
await OnGuardado.InvokeAsync(true);
```

### Cascading Parameter
```csharp
[CascadingParameter] public EmpleadoDto? EmpleadoActual { get; set; }
```

## Formularios

### Estructura de Formulario
```razor
<div class="modal-overlay" @onclick="Cerrar">
    <div class="modal-box" @onclick:stopPropagation="true">
        <div class="modal-header">NUEVO REGISTRO</div>
        
        <div class="formulario-seccion">
            <div class="campo-form">
                <label class="campo-label campo-requerido">Nombre</label>
                <input type="text" class="campo-input" @bind="modelo.Nombre" />
                @if (!string.IsNullOrEmpty(errorNombre))
                {
                    <div class="error-block">@errorNombre</div>
                }
            </div>
        </div>
        
        <div class="modal-footer">
            <button class="btn-action" @onclick="Cerrar">CANCELAR</button>
            <button class="btn-primary" @onclick="Guardar">GUARDAR [F9]</button>
        </div>
    </div>
</div>
```

### Validación
```csharp
private bool Validar()
{
    errores.Clear();
    
    if (string.IsNullOrWhiteSpace(modelo.Nombre))
        errores["Nombre"] = "El nombre es obligatorio";
    
    if (modelo.FechaNacimiento > DateTime.Now)
        errores["FechaNacimiento"] = "La fecha no puede ser futura";
    
    return errores.Count == 0;
}
```

## Atajos de Teclado

Implementar en `OnAfterRenderAsync`:

```csharp
private DotNetObjectReference<MiComponente>? objRef;

protected override async Task OnAfterRenderAsync(bool firstRender)
{
    if (firstRender)
    {
        objRef = DotNetObjectReference.Create(this);
        await JS.InvokeVoidAsync("addKeyboardListener", objRef);
    }
}

[JSInvokable]
public async Task HandleKeyPress(string key)
{
    switch (key)
    {
        case "F9":
            await Guardar();
            break;
        case "Escape":
            Cerrar();
            break;
        case "F2":
            NuevoRegistro();
            break;
    }
    StateHasChanged();
}

public void Dispose()
{
    objRef?.Dispose();
}
```

## Mensajes de Estado

### Auto-hide (0.5 segundos)
```csharp
private async Task MostrarExito(string mensaje)
{
    mensajeExito = mensaje;
    StateHasChanged();
    await Task.Delay(500);
    mensajeExito = null;
    StateHasChanged();
}
```

### En la UI
```razor
@if (!string.IsNullOrEmpty(mensajeExito))
{
    <div class="success-box">@mensajeExito</div>
}

@if (!string.IsNullOrEmpty(mensajeError))
{
    <div class="error-box">
        <div class="error-title">ERROR</div>
        <div class="error-text">@mensajeError</div>
        <button class="btn-action" @onclick="() => mensajeError = null">ACEPTAR</button>
    </div>
}
```

## Navegación

```csharp
// Navegar a otra página
Navigation.NavigateTo("/empleados");

// Navegar con parámetro
Navigation.NavigateTo($"/empleados/{id}/expediente");

// Navegar con force reload
Navigation.NavigateTo("/empleados", forceLoad: true);
```

## Ciclo de Vida

| Método | Cuándo usar |
|--------|-------------|
| `OnInitialized` | Inicialización síncrona |
| `OnInitializedAsync` | Cargar datos iniciales |
| `OnParametersSet` | Cuando cambian parámetros |
| `OnAfterRender(firstRender)` | Después del render (JS interop) |
| `Dispose` | Limpiar recursos |

## Referencias Rápidas

### Archivo de estilos
`wwwroot/css/hospital.css` (ver skill `hospital-ui-style`)

### Registro de servicios
`Program.cs` líneas 19-155
