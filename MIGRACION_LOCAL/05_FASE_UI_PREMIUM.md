# 🎨 FASE 4: UI Premium (Estilo ForestechOil)

## 📋 Contexto

Fases anteriores completadas:
- ✅ Estructura base del proyecto
- ✅ Infraestructura SQLite con Dapper
- ✅ Sistema de archivos local
- ✅ Autenticación local con BCrypt

**Objetivo:** Implementar la UI estilo "hospital/POS" de ForestechOil con navegación por teclado.

---

## 🎯 Objetivo de esta Fase

Copiar y adaptar hospital.css, keyboard-handler.js, MainLayout y KeyboardHandler de ForestechOil para darle a SGRRHH el mismo look premium.

---

## 📝 PROMPT PARA CLAUDE

```
Necesito que implementes la UI premium estilo ForestechOil para SGRRHH.Local.

**REFERENCIA:** ForestechOil (C:\programajava\forestech-blazor\ForestechOil.Server)
- wwwroot/css/hospital.css
- wwwroot/js/keyboard-handler.js
- Components/Layout/MainLayout.razor
- Components/Shared/KeyboardHandler.razor

---

## ARCHIVOS A CREAR:

### 1. wwwroot/css/hospital.css

Copiar el CSS completo de ForestechOil y adaptarlo para SGRRHH:

```css
/* ========================================
   SGRRHH LOCAL - ESTILO HOSPITAL-LIKE
   Basado en ForestechOil
   ======================================== */

/* RESET Y BASE */
* {
    margin: 0;
    padding: 0;
    box-sizing: border-box;
    animation: none !important;
    transition: none !important;
}

/* TIPOGRAFÍA MONOESPACIADA */
body {
    font-family: 'Courier New', Courier, monospace;
    font-size: 14px;
    line-height: 1.4;
    background-color: #FFFFFF;
    color: #000000;
}

/* ... resto del CSS de ForestechOil ... */

/* AGREGAR: Estilos específicos para RRHH */

/* Badge de estado de empleado */
.badge-activo {
    background-color: #4CAF50;
    color: white;
    padding: 2px 8px;
    font-size: 11px;
}

.badge-inactivo {
    background-color: #9E9E9E;
    color: white;
    padding: 2px 8px;
    font-size: 11px;
}

.badge-pendiente {
    background-color: #FF9800;
    color: white;
    padding: 2px 8px;
    font-size: 11px;
}

/* Estados de permiso/vacación */
.estado-pendiente { color: #FF9800; font-weight: bold; }
.estado-aprobado { color: #4CAF50; font-weight: bold; }
.estado-rechazado { color: #F44336; font-weight: bold; }
.estado-cancelado { color: #9E9E9E; font-weight: bold; }

/* Foto de empleado */
.empleado-foto {
    width: 80px;
    height: 80px;
    border: 1px solid #808080;
    object-fit: cover;
}

.empleado-foto-mini {
    width: 32px;
    height: 32px;
    border: 1px solid #808080;
    object-fit: cover;
    vertical-align: middle;
    margin-right: 8px;
}

/* Tarjeta de empleado */
.empleado-card {
    background-color: #FAFAFA;
    border: 1px solid #808080;
    padding: 16px;
    display: flex;
    gap: 16px;
}

.empleado-card .info {
    flex: 1;
}

.empleado-card .info h3 {
    margin-bottom: 8px;
}

.empleado-card .info p {
    margin: 4px 0;
    font-size: 13px;
}

/* Calendario simple */
.calendar-mini {
    border: 1px solid #808080;
}

.calendar-mini th,
.calendar-mini td {
    padding: 4px;
    text-align: center;
    border: 1px solid #E0E0E0;
    width: 28px;
    height: 28px;
}

.calendar-mini .today {
    background-color: #FFEB3B;
    font-weight: bold;
}

.calendar-mini .has-event {
    background-color: #E3F2FD;
}

/* Indicador de días */
.dias-indicator {
    display: flex;
    gap: 4px;
    align-items: center;
}

.dias-indicator .dias-usados {
    color: #F44336;
}

.dias-indicator .dias-disponibles {
    color: #4CAF50;
}

/* Timeline de permisos */
.timeline {
    position: relative;
    padding-left: 24px;
    border-left: 2px solid #808080;
}

.timeline-item {
    position: relative;
    padding: 8px 0;
    margin-bottom: 8px;
}

.timeline-item::before {
    content: '';
    position: absolute;
    left: -29px;
    top: 12px;
    width: 12px;
    height: 12px;
    background-color: #808080;
    border: 2px solid #FFFFFF;
}

.timeline-item.aprobado::before {
    background-color: #4CAF50;
}

.timeline-item.rechazado::before {
    background-color: #F44336;
}
```

---

### 2. wwwroot/js/keyboard-handler.js

Copiar el archivo completo de ForestechOil sin modificaciones.
(Ver C:\programajava\forestech-blazor\ForestechOil.Server\wwwroot\js\keyboard-handler.js)

---

### 3. Components/Layout/MainLayout.razor

```razor
@inherits LayoutComponentBase
@inject IAuthService AuthService
@inject NavigationManager Navigation

@if (!AuthService.IsAuthenticated)
{
    <RedirectToLogin />
}
else
{
    <!-- MARCA DE AGUA -->
    <div style="position: fixed; top: 50%; left: 50%; transform: translate(-50%, -50%); 
                width: 400px; height: 400px; 
                background-image: url('/images/logo-watermark.png'); 
                background-size: contain; background-repeat: no-repeat; 
                background-position: center; opacity: 0.08; 
                z-index: 9999; pointer-events: none;"></div>

    <div class="main-container">
        <!-- Header del Sistema -->
        <header class="system-header">
            <span class="title">SGRRHH LOCAL v1.0</span>
            <span class="info">
                Usuario: @AuthService.CurrentUser?.NombreCompleto 
                (@AuthService.CurrentUser?.Rol) | 
                @DateTime.Now.ToString("dd/MM/yyyy HH:mm")
            </span>
        </header>

        <!-- Menú de Navegación Textual -->
        <nav class="nav-menu">
            <NavLink href="/dashboard" Match="NavLinkMatch.All">INICIO</NavLink>
            <NavLink href="/empleados">EMPLEADOS</NavLink>
            <NavLink href="/permisos">PERMISOS</NavLink>
            <NavLink href="/vacaciones">VACACIONES</NavLink>
            <NavLink href="/contratos">CONTRATOS</NavLink>
            <NavLink href="/control-diario">CONTROL DIARIO</NavLink>
            
            @if (AuthService.IsAdmin)
            {
                <span style="margin-left: 16px; border-left: 1px solid #808080; padding-left: 16px;">
                    <NavLink href="/catalogos">CATÁLOGOS</NavLink>
                    <NavLink href="/usuarios">USUARIOS</NavLink>
                    <NavLink href="/reportes">REPORTES</NavLink>
                    <NavLink href="/configuracion">CONFIG</NavLink>
                </span>
            }
            
            <span style="margin-left: auto;">
                <button class="btn" @onclick="Logout" style="padding: 4px 12px;">
                    SALIR
                </button>
            </span>
        </nav>

        <!-- Breadcrumb -->
        <div class="breadcrumb">
            Ruta: @GetBreadcrumb()
        </div>

        <!-- Área de Trabajo -->
        <main class="work-area">
            @Body
        </main>
    </div>

    <!-- Espacio para la barra de atajos (fija abajo) -->
    <div style="height: 50px;"></div>
}

@code {
    protected override void OnInitialized()
    {
        AuthService.OnAuthStateChanged += OnAuthChanged;
    }

    private void OnAuthChanged(object? sender, Usuario? user)
    {
        if (user == null)
        {
            Navigation.NavigateTo("/login");
        }
        StateHasChanged();
    }

    private string GetBreadcrumb()
    {
        var uri = new Uri(Navigation.Uri);
        var path = uri.AbsolutePath.Trim('/');

        if (string.IsNullOrEmpty(path) || path == "dashboard") 
            return "INICIO";

        var segments = path.Split('/');
        var breadcrumb = "INICIO > " + string.Join(" > ", segments.Select(s => s.ToUpper()));
        
        return breadcrumb;
    }

    private async Task Logout()
    {
        await AuthService.LogoutAsync();
        Navigation.NavigateTo("/login");
    }

    public void Dispose()
    {
        AuthService.OnAuthStateChanged -= OnAuthChanged;
    }
}
```

---

### 4. Components/Shared/KeyboardHandler.razor

Copiar el componente de ForestechOil y adaptarlo:

```razor
@using Microsoft.JSInterop
@inject IJSRuntime JSRuntime
@implements IAsyncDisposable

@* Barra de atajos de teclado visible *@
@if (ShowShortcutBar)
{
    <div class="keyboard-shortcut-bar">
        @foreach (var shortcut in Shortcuts.Where(s => s.ShowInBar))
        {
            <span class="shortcut-item">
                <kbd>@shortcut.KeyDisplay</kbd> @shortcut.Description
            </span>
        }
        <span class="keyboard-mode-indicator">
            MODO TECLADO: @(IsEnabled ? "ACTIVO" : "INACTIVO")
        </span>
    </div>
}

@code {
    [Parameter] public bool ShowShortcutBar { get; set; } = true;
    [Parameter] public bool IsEnabled { get; set; } = true;
    [Parameter] public List<KeyboardShortcut> Shortcuts { get; set; } = new();
    [Parameter] public EventCallback<KeyboardEventArgs> OnKeyPressedCallback { get; set; }

    private DotNetObjectReference<KeyboardHandler>? dotNetRef;
    private bool isInitialized = false;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender && IsEnabled)
        {
            await InitializeAsync();
        }
    }

    public async Task InitializeAsync()
    {
        if (isInitialized) return;
        dotNetRef = DotNetObjectReference.Create(this);
        await JSRuntime.InvokeVoidAsync("KeyboardHandler.initialize", dotNetRef);
        isInitialized = true;
    }

    public async Task SetEnabledAsync(bool enabled)
    {
        IsEnabled = enabled;
        if (isInitialized)
        {
            await JSRuntime.InvokeVoidAsync("KeyboardHandler.setEnabled", enabled);
        }
        StateHasChanged();
    }

    [JSInvokable]
    public async Task OnKeyPressed(KeyboardEventArgs e)
    {
        var shortcut = Shortcuts.FirstOrDefault(s =>
            s.Key == e.Key &&
            s.CtrlKey == e.CtrlKey &&
            s.AltKey == e.AltKey &&
            s.ShiftKey == e.ShiftKey);

        if (shortcut?.Action != null)
        {
            await shortcut.Action.Invoke();
            StateHasChanged();
        }

        await OnKeyPressedCallback.InvokeAsync(e);
    }

    public async Task FocusElementAsync(string elementId)
    {
        await JSRuntime.InvokeVoidAsync("KeyboardHandler.focusElement", elementId);
    }

    public async ValueTask DisposeAsync()
    {
        if (isInitialized)
        {
            try { await JSRuntime.InvokeVoidAsync("KeyboardHandler.dispose"); }
            catch { }
        }
        dotNetRef?.Dispose();
    }

    public class KeyboardShortcut
    {
        public string Key { get; set; } = string.Empty;
        public string KeyDisplay { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public bool CtrlKey { get; set; } = false;
        public bool AltKey { get; set; } = false;
        public bool ShiftKey { get; set; } = false;
        public bool ShowInBar { get; set; } = true;
        public Func<Task>? Action { get; set; }
    }

    public class KeyboardEventArgs
    {
        public string Key { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public bool CtrlKey { get; set; }
        public bool AltKey { get; set; }
        public bool ShiftKey { get; set; }
    }
}
```

---

### 5. Atajos de teclado estándar para SGRRHH:

| Tecla | Función | Contexto |
|-------|---------|----------|
| F1 | Ayuda | Global |
| F2 | Buscar | Listados |
| F3 | Nuevo registro | Listados |
| F4 | Editar seleccionado | Listados |
| F5 | Actualizar/Agregar línea | Listados/Formularios |
| F8 | Eliminar seleccionado | Listados |
| F9 | Guardar | Formularios |
| ESC | Cancelar/Cerrar | Formularios/Modales |
| Enter | Siguiente campo | Formularios |
| ↑/↓ | Navegar filas | Tablas |

---

### 6. Components/Shared/FormModal.razor

Modal genérico para formularios:

```razor
@if (IsVisible)
{
    <div class="modal-overlay" @onclick="OnBackdropClick">
        <div class="modal-box" @onclick:stopPropagation="true" style="min-width: @Width;">
            @if (!string.IsNullOrEmpty(Title))
            {
                <h3>@Title</h3>
            }
            
            <!-- Barra de atajos del formulario -->
            <div class="keyboard-hint-bar">
                <span><kbd>F9</kbd> Guardar</span>
                <span><kbd>ESC</kbd> Cancelar</span>
                <span><kbd>Enter</kbd> Siguiente campo</span>
                @if (AdditionalHints != null)
                {
                    @AdditionalHints
                }
            </div>
            
            <div class="modal-content">
                @ChildContent
            </div>
            
            <div class="modal-actions">
                @if (ShowCancelButton)
                {
                    <button @onclick="OnCancel">CANCELAR (ESC)</button>
                }
                @if (ShowSaveButton)
                {
                    <button class="btn-primary" @onclick="OnSave" disabled="@IsSaving">
                        @if (IsSaving)
                        {
                            <span>GUARDANDO...</span>
                        }
                        else
                        {
                            <span>GUARDAR (F9)</span>
                        }
                    </button>
                }
            </div>
        </div>
    </div>
}

@code {
    [Parameter] public bool IsVisible { get; set; }
    [Parameter] public EventCallback<bool> IsVisibleChanged { get; set; }
    [Parameter] public string? Title { get; set; }
    [Parameter] public string Width { get; set; } = "500px";
    [Parameter] public RenderFragment? ChildContent { get; set; }
    [Parameter] public RenderFragment? AdditionalHints { get; set; }
    [Parameter] public EventCallback OnSaveClicked { get; set; }
    [Parameter] public EventCallback OnCancelClicked { get; set; }
    [Parameter] public bool ShowSaveButton { get; set; } = true;
    [Parameter] public bool ShowCancelButton { get; set; } = true;
    [Parameter] public bool IsSaving { get; set; }
    [Parameter] public bool CloseOnBackdropClick { get; set; } = false;

    private async Task OnSave()
    {
        await OnSaveClicked.InvokeAsync();
    }

    private async Task OnCancel()
    {
        await OnCancelClicked.InvokeAsync();
        await Close();
    }

    private async Task OnBackdropClick()
    {
        if (CloseOnBackdropClick)
        {
            await Close();
        }
    }

    private async Task Close()
    {
        IsVisible = false;
        await IsVisibleChanged.InvokeAsync(false);
    }
}
```

---

### 7. Components/Shared/DataTable.razor

Tabla genérica con selección y navegación:

```razor
@typeparam TItem

<table>
    <thead>
        <tr>
            @HeaderTemplate
        </tr>
    </thead>
    <tbody>
        @if (Items == null || !Items.Any())
        {
            <tr>
                <td colspan="100" class="empty-state">
                    @(EmptyMessage ?? "No hay registros para mostrar")
                </td>
            </tr>
        }
        else
        {
            @foreach (var item in Items)
            {
                <tr class="@(IsSelected(item) ? "selected" : "")"
                    @onclick="() => SelectItem(item)"
                    @ondblclick="() => OnDoubleClick.InvokeAsync(item)">
                    @RowTemplate(item)
                </tr>
            }
        }
    </tbody>
</table>

@if (ShowPagination && TotalPages > 1)
{
    <div style="margin-top: 8px; display: flex; justify-content: space-between; align-items: center;">
        <span>Mostrando @Items?.Count() de @TotalItems registros</span>
        <div>
            <button @onclick="PreviousPage" disabled="@(CurrentPage <= 1)">← ANTERIOR</button>
            <span style="margin: 0 8px;">Página @CurrentPage de @TotalPages</span>
            <button @onclick="NextPage" disabled="@(CurrentPage >= TotalPages)">SIGUIENTE →</button>
        </div>
    </div>
}

@code {
    [Parameter] public IEnumerable<TItem>? Items { get; set; }
    [Parameter] public RenderFragment? HeaderTemplate { get; set; }
    [Parameter] public RenderFragment<TItem>? RowTemplate { get; set; }
    [Parameter] public string? EmptyMessage { get; set; }
    
    [Parameter] public TItem? SelectedItem { get; set; }
    [Parameter] public EventCallback<TItem?> SelectedItemChanged { get; set; }
    [Parameter] public EventCallback<TItem> OnDoubleClick { get; set; }
    
    [Parameter] public bool ShowPagination { get; set; }
    [Parameter] public int CurrentPage { get; set; } = 1;
    [Parameter] public int PageSize { get; set; } = 20;
    [Parameter] public int TotalItems { get; set; }
    [Parameter] public EventCallback<int> OnPageChanged { get; set; }

    private int TotalPages => (int)Math.Ceiling((double)TotalItems / PageSize);

    private bool IsSelected(TItem item)
    {
        if (SelectedItem == null) return false;
        return EqualityComparer<TItem>.Default.Equals(item, SelectedItem);
    }

    private async Task SelectItem(TItem item)
    {
        SelectedItem = item;
        await SelectedItemChanged.InvokeAsync(item);
    }

    private async Task PreviousPage()
    {
        if (CurrentPage > 1)
        {
            CurrentPage--;
            await OnPageChanged.InvokeAsync(CurrentPage);
        }
    }

    private async Task NextPage()
    {
        if (CurrentPage < TotalPages)
        {
            CurrentPage++;
            await OnPageChanged.InvokeAsync(CurrentPage);
        }
    }
}
```

---

### 8. wwwroot/images/

Agregar archivos:
- logo-watermark.png (logo semi-transparente de la empresa)
- default-avatar.png (avatar por defecto para empleados sin foto)

---

## REGISTRO EN App.razor O _Host.cshtml:

```html
<head>
    <!-- ... -->
    <link href="css/hospital.css" rel="stylesheet" />
</head>

<body>
    <!-- ... -->
    <script src="js/keyboard-handler.js"></script>
</body>
```

---

**IMPORTANTE:**
- Mantener el look "retro/terminal" con Courier New
- Cero animaciones para máxima performance
- Colores neutros (grises) excepto para estados
- Bordes rectos, sin bordes redondeados
- Barra de atajos siempre visible abajo
- Marca de agua sutil del logo
```

---

## ✅ Checklist de Entregables

- [ ] wwwroot/css/hospital.css (adaptado de ForestechOil)
- [ ] wwwroot/js/keyboard-handler.js (copiado de ForestechOil)
- [ ] wwwroot/images/logo-watermark.png
- [ ] wwwroot/images/default-avatar.png
- [ ] Components/Layout/MainLayout.razor
- [ ] Components/Layout/EmptyLayout.razor (para login)
- [ ] Components/Shared/KeyboardHandler.razor
- [ ] Components/Shared/FormModal.razor
- [ ] Components/Shared/DataTable.razor
- [ ] Components/Shared/RedirectToLogin.razor
- [ ] Actualizar App.razor para incluir CSS y JS

---

## 🎨 Paleta de Colores

| Uso | Color | Hex |
|-----|-------|-----|
| Fondo principal | Blanco | #FFFFFF |
| Header/Footer | Gris claro | #E0E0E0 |
| Bordes | Gris medio | #808080 |
| Texto principal | Negro | #000000 |
| Error fondo | Rosa claro | #FFCCCC |
| Error borde | Rojo | #FF0000 |
| Éxito fondo | Verde claro | #CCFFCC |
| Éxito borde | Verde | #00AA00 |
| Selección | Azul claro | #D0D0FF |
| Pendiente | Naranja | #FF9800 |
| Aprobado | Verde | #4CAF50 |
| Rechazado | Rojo | #F44336 |
