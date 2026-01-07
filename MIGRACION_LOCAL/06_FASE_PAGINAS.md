# 📄 FASE 5: Migración de Páginas

## 📋 Contexto

Fases anteriores completadas:
- ✅ Estructura base del proyecto
- ✅ Infraestructura SQLite con Dapper
- ✅ Sistema de archivos local
- ✅ Autenticación local con BCrypt
- ✅ UI Premium estilo ForestechOil

**Objetivo:** Migrar todas las páginas de SGRRHH.Web a SGRRHH.Local.Server usando el nuevo estilo.

---

## 🎯 Objetivo de esta Fase

Crear todas las páginas del sistema con funcionalidad completa CRUD y el estilo ForestechOil.

---

## 📝 PROMPT PARA CLAUDE

```
Necesito que migres las páginas de SGRRHH.Web a SGRRHH.Local.Server usando el estilo ForestechOil.

**REFERENCIA:**
- Páginas actuales: C:\Users\evert\Documents\rrhh\src\SGRRHH.Web\Pages\
- Estilo ForestechOil: C:\programajava\forestech-blazor\ForestechOil.Server\Components\Pages\

---

## PÁGINAS A CREAR:

### 1. Dashboard.razor (/dashboard)

Página principal con estadísticas:

```razor
@page "/dashboard"

<KeyboardHandler @ref="keyboardHandler" Shortcuts="shortcuts" ShowShortcutBar="true" />

<h1 class="page-title">PANEL DE CONTROL</h1>

<!-- Stats Cards -->
<div style="display: flex; gap: 16px; margin-bottom: 24px; flex-wrap: wrap;">
    <div class="stats-card">
        <div class="stats-label">Empleados Activos</div>
        <div class="stats-value">@stats.EmpleadosActivos</div>
        <div class="stats-sublabel">de @stats.EmpleadosTotal total</div>
    </div>
    <div class="stats-card">
        <div class="stats-label">Permisos Pendientes</div>
        <div class="stats-value">@stats.PermisosPendientes</div>
        <div class="stats-sublabel">requieren aprobación</div>
    </div>
    <div class="stats-card">
        <div class="stats-label">Vacaciones Pendientes</div>
        <div class="stats-value">@stats.VacacionesPendientes</div>
        <div class="stats-sublabel">en espera</div>
    </div>
    <div class="stats-card">
        <div class="stats-label">Contratos por Vencer</div>
        <div class="stats-value">@stats.ContratosPorVencer</div>
        <div class="stats-sublabel">próximos 30 días</div>
    </div>
</div>

<!-- Quick Actions -->
<div class="panel" style="margin-bottom: 24px;">
    <h3>ACCIONES RÁPIDAS</h3>
    <div style="display: flex; gap: 8px; margin-top: 12px;">
        <button @onclick="@(() => Navigation.NavigateTo("/empleados/nuevo"))">
            NUEVO EMPLEADO
        </button>
        <button @onclick="@(() => Navigation.NavigateTo("/permisos/nuevo"))">
            NUEVO PERMISO
        </button>
        <button @onclick="@(() => Navigation.NavigateTo("/control-diario"))">
            REGISTRO DIARIO
        </button>
    </div>
</div>

<!-- Permisos Pendientes (si es aprobador) -->
@if (AuthService.IsAprobador && permisosPendientes.Any())
{
    <div class="panel">
        <h3>PERMISOS PENDIENTES DE APROBACIÓN</h3>
        <table>
            <thead>
                <tr>
                    <th>N° ACTA</th>
                    <th>EMPLEADO</th>
                    <th>TIPO</th>
                    <th>FECHAS</th>
                    <th>ACCIONES</th>
                </tr>
            </thead>
            <tbody>
                @foreach (var permiso in permisosPendientes.Take(5))
                {
                    <tr>
                        <td>@permiso.NumeroActa</td>
                        <td>@permiso.Empleado?.NombreCompleto</td>
                        <td>@permiso.TipoPermiso?.Nombre</td>
                        <td>@permiso.FechaInicio.ToString("dd/MM") - @permiso.FechaFin.ToString("dd/MM")</td>
                        <td class="actions-cell">
                            <button @onclick="() => VerPermiso(permiso.Id)">VER</button>
                        </td>
                    </tr>
                }
            </tbody>
        </table>
        @if (permisosPendientes.Count() > 5)
        {
            <p style="margin-top: 8px;">
                <a href="/permisos?estado=pendiente">Ver todos (@permisosPendientes.Count())</a>
            </p>
        }
    </div>
}

@code {
    // Inyectar servicios y repositorios
    // Cargar estadísticas en OnInitializedAsync
    // Implementar acciones rápidas
}
```

---

### 2. Empleados.razor (/empleados)

Listado de empleados con CRUD completo:

```razor
@page "/empleados"
@page "/empleados/{EmpleadoIdParam:int?}"

<KeyboardHandler @ref="keyboardHandler" Shortcuts="shortcuts" ShowShortcutBar="true" />

<h1 class="page-title">GESTIÓN DE EMPLEADOS</h1>

@if (errorMessage != null)
{
    <div class="error-block">
        <h3>ERROR</h3>
        <p>@errorMessage</p>
        <button @onclick="() => errorMessage = null">ACEPTAR</button>
    </div>
}

@if (successMessage != null)
{
    <div class="success-block">
        <p>@successMessage</p>
    </div>
}

<!-- Header con controles -->
<div style="margin-bottom: 16px; display: flex; justify-content: space-between; align-items: center;">
    <div>
        <button @onclick="NuevoEmpleado">NUEVO (F3)</button>
        <button @onclick="CargarEmpleados">ACTUALIZAR</button>
    </div>
    <div style="display: flex; gap: 8px; align-items: center;">
        <input type="text" id="searchInput" placeholder="Buscar... (F2)" 
               @bind="searchTerm" @bind:event="oninput" @onkeyup="OnSearchKeyUp" />
        <button @onclick="Buscar">BUSCAR</button>
        @if (!string.IsNullOrEmpty(searchTerm))
        {
            <button @onclick="LimpiarBusqueda">LIMPIAR</button>
        }
        <select @bind="filtroEstado" @bind:after="Filtrar">
            <option value="">Todos los estados</option>
            <option value="1">Activos</option>
            <option value="2">Inactivos</option>
            <option value="3">Pendientes</option>
        </select>
    </div>
</div>

<!-- Tabla de empleados -->
<table>
    <thead>
        <tr>
            <th style="width: 50px;">FOTO</th>
            <th>CÓDIGO</th>
            <th>CÉDULA</th>
            <th>NOMBRE COMPLETO</th>
            <th>CARGO</th>
            <th>DEPARTAMENTO</th>
            <th>ESTADO</th>
            <th>ACCIONES</th>
        </tr>
    </thead>
    <tbody>
        @if (isLoading)
        {
            <tr><td colspan="8" class="loading">Cargando empleados...</td></tr>
        }
        else if (!empleados.Any())
        {
            <tr><td colspan="8" class="empty-state">No hay empleados para mostrar</td></tr>
        }
        else
        {
            @foreach (var emp in empleados)
            {
                <tr class="@(selectedEmpleado?.Id == emp.Id ? "selected" : "")"
                    @onclick="() => SelectEmpleado(emp)">
                    <td>
                        <img src="@GetFotoUrl(emp)" class="empleado-foto-mini" 
                             onerror="this.src='/images/default-avatar.png'" />
                    </td>
                    <td>@emp.Codigo</td>
                    <td>@emp.Cedula</td>
                    <td><strong>@emp.NombreCompleto</strong></td>
                    <td>@(emp.Cargo?.Nombre ?? "-")</td>
                    <td>@(emp.Departamento?.Nombre ?? "-")</td>
                    <td>
                        <span class="badge-@emp.Estado.ToString().ToLower()">
                            @emp.Estado
                        </span>
                    </td>
                    <td class="actions-cell">
                        <button @onclick="() => EditarEmpleado(emp)" @onclick:stopPropagation="true">
                            EDITAR
                        </button>
                        <button @onclick="() => VerExpediente(emp)" @onclick:stopPropagation="true">
                            EXPEDIENTE
                        </button>
                    </td>
                </tr>
            }
        }
    </tbody>
</table>

<!-- Modal de Empleado (Nuevo/Editar) -->
<FormModal @bind-IsVisible="showModal" 
           Title="@(isEditing ? "EDITAR EMPLEADO" : "NUEVO EMPLEADO")"
           Width="800px"
           OnSaveClicked="Guardar"
           OnCancelClicked="CerrarModal"
           IsSaving="isSaving">
    
    <div style="display: grid; grid-template-columns: 1fr 1fr; gap: 16px;">
        <!-- Columna izquierda: Datos básicos -->
        <div>
            <div class="form-group required">
                <label>Código:</label>
                <input type="text" @bind="empleadoEdit.Codigo" />
            </div>
            <div class="form-group required">
                <label>Cédula:</label>
                <input type="text" @bind="empleadoEdit.Cedula" />
            </div>
            <div class="form-group required">
                <label>Nombres:</label>
                <input type="text" @bind="empleadoEdit.Nombres" />
            </div>
            <div class="form-group required">
                <label>Apellidos:</label>
                <input type="text" @bind="empleadoEdit.Apellidos" />
            </div>
            <div class="form-group">
                <label>Fecha Nacimiento:</label>
                <input type="date" @bind="empleadoEdit.FechaNacimiento" />
            </div>
            <div class="form-group">
                <label>Género:</label>
                <select @bind="empleadoEdit.Genero">
                    <option value="">-- Seleccione --</option>
                    @foreach (var g in Enum.GetValues<Genero>())
                    {
                        <option value="@g">@g</option>
                    }
                </select>
            </div>
        </div>
        
        <!-- Columna derecha: Contacto y laboral -->
        <div>
            <div class="form-group">
                <label>Teléfono:</label>
                <input type="text" @bind="empleadoEdit.Telefono" />
            </div>
            <div class="form-group">
                <label>Email:</label>
                <input type="email" @bind="empleadoEdit.Email" />
            </div>
            <div class="form-group required">
                <label>Fecha Ingreso:</label>
                <input type="date" @bind="empleadoEdit.FechaIngreso" />
            </div>
            <div class="form-group">
                <label>Departamento:</label>
                <select @bind="empleadoEdit.DepartamentoId">
                    <option value="">-- Seleccione --</option>
                    @foreach (var d in departamentos)
                    {
                        <option value="@d.Id">@d.Nombre</option>
                    }
                </select>
            </div>
            <div class="form-group">
                <label>Cargo:</label>
                <select @bind="empleadoEdit.CargoId">
                    <option value="">-- Seleccione --</option>
                    @foreach (var c in cargos)
                    {
                        <option value="@c.Id">@c.Nombre</option>
                    }
                </select>
            </div>
            <div class="form-group">
                <label>Foto:</label>
                <InputFile OnChange="OnFotoSelected" accept="image/*" />
            </div>
        </div>
    </div>
</FormModal>

@code {
    [Parameter] public int? EmpleadoIdParam { get; set; }
    
    // Lista de atajos de teclado
    private List<KeyboardShortcut> shortcuts = new()
    {
        new() { Key = "F2", KeyDisplay = "F2", Description = "Buscar", Action = FocusBusqueda },
        new() { Key = "F3", KeyDisplay = "F3", Description = "Nuevo", Action = NuevoEmpleado },
        new() { Key = "F4", KeyDisplay = "F4", Description = "Editar", Action = EditarSeleccionado },
        new() { Key = "Escape", KeyDisplay = "ESC", Description = "Cerrar", Action = CerrarModal }
    };
    
    // Implementar lógica completa...
}
```

---

### 3. Permisos.razor (/permisos)

Gestión de permisos con flujo de aprobación:

- Listado con filtros por estado
- Formulario de solicitud
- Vista de detalle con historial
- Botones de aprobar/rechazar (solo aprobadores)
- Generación de acta PDF

---

### 4. Vacaciones.razor (/vacaciones)

- Listado por empleado
- Cálculo automático de días disponibles
- Flujo de aprobación similar a permisos
- Vista de calendario

---

### 5. Contratos.razor (/contratos)

- Listado de contratos por empleado
- Historial de contratos
- Alertas de vencimiento
- Adjuntar documento de contrato

---

### 6. ControlDiario.razor (/control-diario)

- Registro de actividades diarias
- Selector de fecha
- Lista de empleados con checkbox de asistencia
- Detalle de actividades por empleado

---

### 7. Catalogos.razor (/catalogos)

Página madre con tabs para:
- Departamentos
- Cargos
- Tipos de Permiso
- Proyectos
- Actividades

---

### 8. Usuarios.razor (/usuarios) - Solo Admin

- CRUD de usuarios
- Asignación de roles
- Reset de contraseña
- Habilitar/deshabilitar usuarios

---

### 9. Reportes.razor (/reportes)

Centro de reportes:
- Listado de empleados (PDF/Excel)
- Reporte de permisos por rango de fechas
- Reporte de vacaciones por período
- Reporte de asistencia
- Certificados laborales

---

### 10. Configuracion.razor (/configuracion) - Solo Admin

- Datos de la empresa
- Logo
- Configuraciones del sistema
- Backup/Restore de base de datos

---

## PATRÓN COMÚN PARA TODAS LAS PÁGINAS:

```razor
@page "/[entidad]"
@inject IAuthService AuthService
@inject NavigationManager Navigation
@inject I[Entidad]Repository [Entidad]Repository
@inject ILogger<[Entidad]Page> Logger

<KeyboardHandler @ref="keyboardHandler" Shortcuts="shortcuts" ShowShortcutBar="true" />

<h1 class="page-title">GESTIÓN DE [ENTIDAD]</h1>

<!-- Mensajes de error/éxito -->
@if (errorMessage != null) { ... }
@if (successMessage != null) { ... }

<!-- Barra de herramientas -->
<div style="margin-bottom: 16px; display: flex; justify-content: space-between;">
    <div>
        <button @onclick="Nuevo">NUEVO (F3)</button>
        <button @onclick="Actualizar">ACTUALIZAR</button>
    </div>
    <div>
        <!-- Búsqueda y filtros -->
    </div>
</div>

<!-- Tabla de datos -->
<table>...</table>

<!-- Modal de edición -->
<FormModal>...</FormModal>

@code {
    private KeyboardHandler? keyboardHandler;
    private List<KeyboardShortcut> shortcuts;
    
    private List<[Entidad]> items = new();
    private [Entidad]? selectedItem;
    private [Entidad] editItem = new();
    
    private bool isLoading;
    private bool showModal;
    private bool isSaving;
    private bool isEditing;
    private string? errorMessage;
    private string? successMessage;
    
    protected override async Task OnInitializedAsync()
    {
        if (!AuthService.IsAuthenticated)
        {
            Navigation.NavigateTo("/login");
            return;
        }
        
        await CargarDatos();
    }
    
    // Implementar métodos CRUD...
}
```

---

**IMPORTANTE:**
- Mantener el estilo ForestechOil en todas las páginas
- Implementar navegación por teclado
- Mostrar mensajes de error/éxito consistentes
- Validar permisos según rol del usuario
- Loggear todas las operaciones importantes
```

---

## ✅ Checklist de Entregables

### Páginas Principales:
- [ ] Dashboard.razor
- [ ] Empleados.razor
- [ ] Permisos.razor
- [ ] Vacaciones.razor
- [ ] Contratos.razor
- [ ] ControlDiario.razor

### Catálogos (pueden ser componentes dentro de Catalogos.razor):
- [ ] Catalogos.razor (página madre)
- [ ] DepartamentosTab.razor
- [ ] CargosTab.razor
- [ ] TiposPermisoTab.razor
- [ ] ProyectosTab.razor
- [ ] ActividadesTab.razor

### Administración:
- [ ] Usuarios.razor
- [ ] Reportes.razor
- [ ] Configuracion.razor

### Componentes de Apoyo:
- [ ] EmpleadoCard.razor (tarjeta de empleado)
- [ ] EmpleadoSelector.razor (selector/buscador)
- [ ] EstadoBadge.razor (badge de estado)
- [ ] CalendarioMini.razor (calendario pequeño)
- [ ] ConfirmDialog.razor (diálogo de confirmación)

---

## 📊 Funcionalidad por Página

| Página | Crear | Leer | Actualizar | Eliminar | Especial |
|--------|-------|------|------------|----------|----------|
| Dashboard | - | ✅ | - | - | Estadísticas |
| Empleados | ✅ | ✅ | ✅ | Soft | Foto, Expediente |
| Permisos | ✅ | ✅ | ✅ | Soft | Aprobar/Rechazar, PDF |
| Vacaciones | ✅ | ✅ | ✅ | Soft | Cálculo días |
| Contratos | ✅ | ✅ | ✅ | Soft | Adjuntos |
| ControlDiario | ✅ | ✅ | ✅ | ✅ | Actividades |
| Catálogos | ✅ | ✅ | ✅ | Soft | - |
| Usuarios | ✅ | ✅ | ✅ | Soft | Reset password |
| Reportes | - | ✅ | - | - | Generar PDF/Excel |
| Configuración | - | ✅ | ✅ | - | Backup |
