# PROMPT: Sistema de Roles y Permisos - Módulo Empleados

## 📋 Resumen Ejecutivo

Implementar infraestructura de control de acceso basada en roles con un **toggle "Modo Corporativo"** que permita activar/desactivar las restricciones a voluntad del administrador.

**Alcance inicial:** Módulo Empleados únicamente.
**Meta:** Crear infraestructura reutilizable para todos los módulos futuros.

---

## 🎯 Objetivos

1. Crear sistema de permisos centralizado y estandarizado
2. Implementar toggle "Modo Corporativo" (ON = restricciones activas, OFF = acceso libre)
3. Aplicar al módulo Empleados como piloto
4. No romper funcionalidad existente (backwards compatible)

---

## 🔧 FASE 1: Infraestructura Base

### 1.1 Nueva Tabla en BD: `configuracion_sistema`

```sql
-- Agregar columna si no existe, o crear tabla de configuración
-- Script: migration_modo_corporativo_v1.sql

-- Opción A: Agregar a tabla existente configuracion_sistema
ALTER TABLE configuracion_sistema ADD COLUMN modo_corporativo INTEGER DEFAULT 0;

-- Nota: 0 = Desactivado (acceso libre), 1 = Activado (restricciones de roles)
```

### 1.2 Actualizar Enum `RolUsuario`

**Archivo:** `SGRRHH.Local.Domain/Enums/RolUsuario.cs`

```csharp
namespace SGRRHH.Local.Domain.Enums;

/// <summary>
/// Roles de usuario del sistema.
/// Los permisos de cada rol se aplican SOLO cuando el Modo Corporativo está activado.
/// </summary>
public enum RolUsuario
{
    /// <summary>
    /// Acceso completo al sistema. Puede activar/desactivar Modo Corporativo.
    /// </summary>
    Administrador = 1,
    
    /// <summary>
    /// Puede aprobar/rechazar solicitudes. Acceso a reportes y configuraciones básicas.
    /// Típicamente: Ingeniera, Jefe de área, Supervisor.
    /// </summary>
    Aprobador = 2,
    
    /// <summary>
    /// Puede crear y editar registros, pero requiere aprobación para cambios críticos.
    /// Típicamente: Secretaria, Auxiliar administrativo.
    /// </summary>
    Operador = 3
}
```

### 1.3 Nueva Clase: `PermisosModulo` (Define permisos por módulo)

**Archivo:** `SGRRHH.Local.Domain/Enums/PermisosModulo.cs`

```csharp
namespace SGRRHH.Local.Domain.Enums;

/// <summary>
/// Define los tipos de acciones que se pueden realizar en cada módulo.
/// Usado para verificar permisos cuando el Modo Corporativo está activo.
/// </summary>
[Flags]
public enum PermisosModulo
{
    Ninguno = 0,
    
    // Permisos básicos
    Ver = 1,
    Crear = 2,
    Editar = 4,
    Eliminar = 8,
    
    // Permisos de flujo de aprobación
    Aprobar = 16,
    Rechazar = 32,
    
    // Permisos especiales
    EditarDatosCriticos = 64,   // Salario, cargo, etc.
    Retirar = 128,              // Dar de baja empleados
    Exportar = 256,
    Importar = 512,
    
    // Combinaciones comunes
    Lectura = Ver,
    Basico = Ver | Crear | Editar,
    Completo = Ver | Crear | Editar | Eliminar | Aprobar | Rechazar | EditarDatosCriticos | Retirar | Exportar | Importar
}
```

### 1.4 Nueva Clase: `ConfiguracionRoles` (Matriz de permisos centralizada)

**Archivo:** `SGRRHH.Local.Domain/Configuration/ConfiguracionRoles.cs`

```csharp
namespace SGRRHH.Local.Domain.Configuration;

using SGRRHH.Local.Domain.Enums;

/// <summary>
/// Configuración centralizada de permisos por rol y módulo.
/// Esta es la ÚNICA fuente de verdad para los permisos del sistema.
/// </summary>
public static class ConfiguracionRoles
{
    /// <summary>
    /// Matriz de permisos: Define qué puede hacer cada rol en cada módulo.
    /// Solo se aplica cuando Modo Corporativo está ACTIVADO.
    /// </summary>
    private static readonly Dictionary<(RolUsuario Rol, string Modulo), PermisosModulo> _permisos = new()
    {
        // ========== MÓDULO: EMPLEADOS ==========
        
        // Administrador - Acceso completo
        { (RolUsuario.Administrador, "Empleados"), PermisosModulo.Completo },
        
        // Aprobador (Ingeniera) - Puede aprobar, editar todo, no eliminar
        { (RolUsuario.Aprobador, "Empleados"), 
            PermisosModulo.Ver | 
            PermisosModulo.Crear | 
            PermisosModulo.Editar | 
            PermisosModulo.Aprobar | 
            PermisosModulo.Rechazar | 
            PermisosModulo.EditarDatosCriticos | 
            PermisosModulo.Retirar |
            PermisosModulo.Exportar },
        
        // Operador (Secretaria) - Solo crear y editar básico, sin aprobar
        { (RolUsuario.Operador, "Empleados"), 
            PermisosModulo.Ver | 
            PermisosModulo.Crear | 
            PermisosModulo.Editar |
            PermisosModulo.Exportar },
        
        // ========== MÓDULOS FUTUROS (plantilla) ==========
        // Descomentar y ajustar cuando se implementen
        
        // { (RolUsuario.Administrador, "Permisos"), PermisosModulo.Completo },
        // { (RolUsuario.Aprobador, "Permisos"), PermisosModulo.Ver | PermisosModulo.Aprobar | PermisosModulo.Rechazar },
        // { (RolUsuario.Operador, "Permisos"), PermisosModulo.Ver | PermisosModulo.Crear },
        
        // { (RolUsuario.Administrador, "Vacaciones"), PermisosModulo.Completo },
        // { (RolUsuario.Aprobador, "Vacaciones"), PermisosModulo.Ver | PermisosModulo.Aprobar | PermisosModulo.Rechazar },
        // { (RolUsuario.Operador, "Vacaciones"), PermisosModulo.Ver | PermisosModulo.Crear },
    };
    
    /// <summary>
    /// Obtiene los permisos de un rol para un módulo específico.
    /// </summary>
    public static PermisosModulo ObtenerPermisos(RolUsuario rol, string modulo)
    {
        if (_permisos.TryGetValue((rol, modulo), out var permisos))
            return permisos;
        
        // Por defecto: solo lectura si no está definido
        return PermisosModulo.Ver;
    }
    
    /// <summary>
    /// Verifica si un rol tiene un permiso específico en un módulo.
    /// </summary>
    public static bool TienePermiso(RolUsuario rol, string modulo, PermisosModulo permiso)
    {
        var permisos = ObtenerPermisos(rol, modulo);
        return permisos.HasFlag(permiso);
    }
    
    /// <summary>
    /// Lista de todos los módulos configurados.
    /// </summary>
    public static IEnumerable<string> ModulosConfigurados => 
        _permisos.Keys.Select(k => k.Modulo).Distinct();
}
```

### 1.5 Actualizar Interface `IAuthService`

**Archivo:** `SGRRHH.Local.Shared/Interfaces/IAuthService.cs`

Agregar los siguientes miembros:

```csharp
// ========== MODO CORPORATIVO ==========

/// <summary>
/// Indica si el Modo Corporativo está activado.
/// Cuando está activo, se aplican las restricciones de roles.
/// Cuando está inactivo, todos tienen acceso completo.
/// </summary>
bool ModoCorporativoActivo { get; }

/// <summary>
/// Activa o desactiva el Modo Corporativo. Solo disponible para Administradores.
/// </summary>
Task<Result> SetModoCorporativoAsync(bool activar);

/// <summary>
/// Verifica si el usuario actual tiene un permiso específico en un módulo.
/// Considera el estado del Modo Corporativo.
/// </summary>
bool TienePermiso(string modulo, PermisosModulo permiso);

/// <summary>
/// Obtiene todos los permisos del usuario actual para un módulo.
/// </summary>
PermisosModulo ObtenerPermisosModulo(string modulo);
```

### 1.6 Actualizar `LocalAuthService`

**Archivo:** `SGRRHH.Local.Infrastructure/Services/LocalAuthService.cs`

```csharp
// Agregar campo privado
private bool _modoCorporativoActivo = false;

// Implementar propiedad
public bool ModoCorporativoActivo => _modoCorporativoActivo;

// Actualizar propiedades de rol para respetar Modo Corporativo
public bool IsAdmin => IsAuthenticated && 
    (!_modoCorporativoActivo || _currentUser?.Rol == RolUsuario.Administrador);

public bool IsAprobador => IsAuthenticated && 
    (!_modoCorporativoActivo || _currentUser?.Rol == RolUsuario.Administrador || 
     _currentUser?.Rol == RolUsuario.Aprobador);

public bool IsOperador => IsAuthenticated; // Todos los autenticados pueden ver

// Implementar método para activar/desactivar
public async Task<Result> SetModoCorporativoAsync(bool activar)
{
    if (!IsAuthenticated)
        return Result.Fail("Debe iniciar sesión");
    
    // Solo admin puede cambiar el modo (verificación real, no afectada por el toggle)
    if (_currentUser?.Rol != RolUsuario.Administrador)
        return Result.Fail("Solo administradores pueden cambiar el Modo Corporativo");
    
    _modoCorporativoActivo = activar;
    
    // Persistir en configuración
    await _configuracionService.SetAsync("modo_corporativo", activar ? "1" : "0");
    
    _logger.LogWarning("Modo Corporativo {Estado} por {Usuario}", 
        activar ? "ACTIVADO" : "DESACTIVADO", _currentUser.Username);
    
    await RegistrarAuditoria(
        activar ? "MODO_CORPORATIVO_ACTIVADO" : "MODO_CORPORATIVO_DESACTIVADO",
        "Sistema", null,
        $"Modo Corporativo {(activar ? "activado" : "desactivado")} por {_currentUser.NombreCompleto}");
    
    OnAuthStateChanged?.Invoke(this, _currentUser);
    
    return Result.Ok($"Modo Corporativo {(activar ? "activado" : "desactivado")}");
}

// Implementar verificación de permisos
public bool TienePermiso(string modulo, PermisosModulo permiso)
{
    if (!IsAuthenticated)
        return false;
    
    // Si Modo Corporativo está DESACTIVADO, todos tienen acceso completo
    if (!_modoCorporativoActivo)
        return true;
    
    // Si está ACTIVADO, verificar según la matriz de permisos
    return ConfiguracionRoles.TienePermiso(_currentUser!.Rol, modulo, permiso);
}

public PermisosModulo ObtenerPermisosModulo(string modulo)
{
    if (!IsAuthenticated)
        return PermisosModulo.Ninguno;
    
    // Si Modo Corporativo está DESACTIVADO, todos tienen acceso completo
    if (!_modoCorporativoActivo)
        return PermisosModulo.Completo;
    
    return ConfiguracionRoles.ObtenerPermisos(_currentUser!.Rol, modulo);
}

// Cargar estado del Modo Corporativo al iniciar sesión
// Agregar en LoginAsync después de login exitoso:
var modoCorp = await _configuracionService.GetAsync("modo_corporativo");
_modoCorporativoActivo = modoCorp == "1";
```

---

## 🔧 FASE 2: Aplicar al Módulo Empleados

### 2.1 Modificar Flujo de Creación de Empleado

**Lógica en el componente/servicio:**

```csharp
// Al crear un empleado nuevo
public async Task<Result<Empleado>> CrearEmpleadoAsync(Empleado empleado)
{
    // Determinar estado inicial según Modo Corporativo y rol
    if (_authService.ModoCorporativoActivo)
    {
        // Con Modo Corporativo: Operadores crean en estado Pendiente
        if (_authService.CurrentUser?.Rol == RolUsuario.Operador)
        {
            empleado.Estado = EstadoEmpleado.PendienteAprobacion;
            empleado.FechaSolicitud = DateTime.Now;
        }
        else
        {
            // Aprobadores y Admins crean directamente como Activo
            empleado.Estado = EstadoEmpleado.Activo;
            empleado.FechaAprobacion = DateTime.Now;
            empleado.AprobadoPorId = _authService.CurrentUserId;
        }
    }
    else
    {
        // Sin Modo Corporativo: todos crean como Activo
        empleado.Estado = EstadoEmpleado.Activo;
    }
    
    empleado.CreadoPorId = _authService.CurrentUserId;
    
    // ... resto de la lógica de guardado
}
```

### 2.2 Nuevos Métodos en `EmpleadoRepository` / `EmpleadoService`

```csharp
/// <summary>
/// Obtiene empleados pendientes de aprobación.
/// </summary>
Task<IEnumerable<Empleado>> GetPendientesAprobacionAsync();

/// <summary>
/// Aprueba un empleado pendiente. Cambia estado a Activo.
/// </summary>
Task<Result<Empleado>> AprobarEmpleadoAsync(int empleadoId, int aprobadorId);

/// <summary>
/// Rechaza un empleado pendiente con motivo.
/// </summary>
Task<Result<Empleado>> RechazarEmpleadoAsync(int empleadoId, int aprobadorId, string motivo);
```

### 2.3 Modificar UI de Empleados

**Archivo:** `Components/Pages/Empleados.razor`

Agregar lógica condicional para mostrar/ocultar elementos según permisos:

```razor
@inject IAuthService AuthService

@code {
    private PermisosModulo _permisos;
    
    protected override void OnInitialized()
    {
        _permisos = AuthService.ObtenerPermisosModulo("Empleados");
    }
    
    private bool PuedeCrear => _permisos.HasFlag(PermisosModulo.Crear);
    private bool PuedeEditar => _permisos.HasFlag(PermisosModulo.Editar);
    private bool PuedeEliminar => _permisos.HasFlag(PermisosModulo.Eliminar);
    private bool PuedeAprobar => _permisos.HasFlag(PermisosModulo.Aprobar);
    private bool PuedeEditarCriticos => _permisos.HasFlag(PermisosModulo.EditarDatosCriticos);
    private bool PuedeRetirar => _permisos.HasFlag(PermisosModulo.Retirar);
}

<!-- Botón Nuevo Empleado: visible según permiso -->
@if (PuedeCrear)
{
    <button class="btn-action" @onclick="MostrarFormularioNuevo">
        + Nuevo Empleado
    </button>
}

<!-- Columna de acciones en la tabla -->
<td class="actions-cell">
    @if (PuedeEditar)
    {
        <button @onclick="() => EditarEmpleado(emp)">Editar</button>
    }
    
    @if (PuedeAprobar && emp.Estado == EstadoEmpleado.PendienteAprobacion)
    {
        <button class="btn-success" @onclick="() => AprobarEmpleado(emp)">Aprobar</button>
        <button class="btn-danger" @onclick="() => RechazarEmpleado(emp)">Rechazar</button>
    }
    
    @if (PuedeRetirar && emp.Estado == EstadoEmpleado.Activo)
    {
        <button class="btn-warning" @onclick="() => RetirarEmpleado(emp)">Retirar</button>
    }
    
    @if (PuedeEliminar)
    {
        <button class="btn-danger" @onclick="() => EliminarEmpleado(emp)">Eliminar</button>
    }
</td>

<!-- Campos críticos (salario, cargo) solo editables si tiene permiso -->
@if (PuedeEditarCriticos)
{
    <div class="form-field">
        <label>Salario Base</label>
        <InputMoneda @bind-Value="empleadoEdit.SalarioBase" />
    </div>
}
else
{
    <div class="form-field">
        <label>Salario Base</label>
        <span class="readonly-value">@empleadoEdit.SalarioBase?.ToString("C")</span>
    </div>
}
```

### 2.4 Indicador Visual de Modo Corporativo

**Agregar en el Layout o NavMenu:**

```razor
@if (AuthService.ModoCorporativoActivo)
{
    <div class="modo-corporativo-badge" title="Restricciones de roles activas">
        🔒 MODO CORPORATIVO
    </div>
}
else
{
    <div class="modo-libre-badge" title="Acceso libre para todos">
        🔓 Acceso Libre
    </div>
}
```

### 2.5 Toggle en Configuración (Solo Admin)

**Agregar en:** `Components/Pages/Configuracion.razor`

```razor
@if (AuthService.CurrentUser?.Rol == RolUsuario.Administrador)
{
    <div class="config-section">
        <h3>Control de Acceso</h3>
        
        <div class="toggle-container">
            <label class="toggle-label">
                <input type="checkbox" 
                       checked="@AuthService.ModoCorporativoActivo"
                       @onchange="ToggleModoCorporativo" />
                <span class="toggle-text">
                    @if (AuthService.ModoCorporativoActivo)
                    {
                        <span>🔒 Modo Corporativo ACTIVO</span>
                        <small>Las restricciones de roles están aplicadas</small>
                    }
                    else
                    {
                        <span>🔓 Acceso Libre</span>
                        <small>Todos los usuarios tienen acceso completo</small>
                    }
                </span>
            </label>
        </div>
        
        <div class="info-box">
            <strong>¿Qué hace el Modo Corporativo?</strong>
            <ul>
                <li>Operadores (Secretaria): Solo pueden crear empleados en estado "Pendiente"</li>
                <li>Aprobadores (Ingeniera): Pueden aprobar/rechazar empleados pendientes</li>
                <li>Administradores: Acceso completo</li>
            </ul>
        </div>
    </div>
}

@code {
    private async Task ToggleModoCorporativo(ChangeEventArgs e)
    {
        var activar = (bool)e.Value!;
        var resultado = await AuthService.SetModoCorporativoAsync(activar);
        
        if (!resultado.IsSuccess)
        {
            MostrarError(resultado.Message);
        }
        else
        {
            MostrarExito(resultado.Message);
            StateHasChanged();
        }
    }
}
```

---

## 📁 ARCHIVOS A CREAR/MODIFICAR

### Nuevos Archivos:
1. `scripts/migration_modo_corporativo_v1.sql`
2. `SGRRHH.Local.Domain/Enums/PermisosModulo.cs`
3. `SGRRHH.Local.Domain/Configuration/ConfiguracionRoles.cs`

### Archivos a Modificar:
1. `SGRRHH.Local.Domain/Enums/RolUsuario.cs` - Quitar [Obsolete]
2. `SGRRHH.Local.Shared/Interfaces/IAuthService.cs` - Agregar métodos
3. `SGRRHH.Local.Infrastructure/Services/LocalAuthService.cs` - Implementar lógica
4. `SGRRHH.Local.Server/Components/Pages/Empleados.razor` - Aplicar permisos UI
5. `SGRRHH.Local.Server/Components/Pages/Configuracion.razor` - Toggle
6. `SGRRHH.Local.Server/Components/Layout/NavMenu.razor` - Indicador visual

---

## 🧪 CASOS DE PRUEBA

### Escenario 1: Modo Corporativo DESACTIVADO (default)
- [ ] Todos los usuarios ven todos los botones
- [ ] Todos pueden crear empleados directamente como Activo
- [ ] No hay flujo de aprobación
- [ ] Funciona exactamente como antes

### Escenario 2: Modo Corporativo ACTIVADO
- [ ] Secretaria (Operador):
  - [ ] Puede ver lista de empleados
  - [ ] Puede crear empleado (se crea como PendienteAprobacion)
  - [ ] Puede editar datos básicos
  - [ ] NO ve botón de Aprobar/Rechazar
  - [ ] NO puede editar salario/cargo
  - [ ] NO puede eliminar
  
- [ ] Ingeniera (Aprobador):
  - [ ] Ve todo lo de Secretaria
  - [ ] Ve botones Aprobar/Rechazar en empleados pendientes
  - [ ] Puede editar salario/cargo
  - [ ] Puede retirar empleados
  - [ ] NO puede eliminar
  
- [ ] Admin:
  - [ ] Acceso completo
  - [ ] Puede activar/desactivar Modo Corporativo
  - [ ] Puede eliminar empleados

### Escenario 3: Toggle del Modo
- [ ] Solo Admin ve el toggle en Configuración
- [ ] Al activar, se registra en auditoría
- [ ] Al desactivar, se registra en auditoría
- [ ] El estado persiste entre sesiones
- [ ] Otros usuarios ven el indicador pero no pueden cambiar

---

## 📋 ORDEN DE IMPLEMENTACIÓN

1. **Crear migración SQL** - Agregar columna modo_corporativo
2. **Crear PermisosModulo enum** - Flags de permisos
3. **Crear ConfiguracionRoles** - Matriz centralizada
4. **Actualizar RolUsuario** - Quitar Obsolete, agregar docs
5. **Actualizar IAuthService** - Nuevos métodos
6. **Implementar en LocalAuthService** - Lógica del toggle
7. **Modificar Empleados.razor** - Aplicar permisos UI
8. **Agregar toggle en Configuración** - Solo para Admin
9. **Agregar indicador en NavMenu** - Visual del modo activo
10. **Probar todos los escenarios**

---

## ⚠️ NOTAS IMPORTANTES

1. **Backwards Compatible:** Si modo_corporativo es NULL o 0, el sistema funciona exactamente como antes.

2. **Verificación Real de Admin:** Al verificar si puede cambiar el toggle, se usa `_currentUser?.Rol == RolUsuario.Administrador` directamente, NO la propiedad `IsAdmin` que puede estar afectada por el toggle.

3. **Persistencia:** El estado del Modo Corporativo se guarda en la BD y se carga al iniciar sesión.

4. **Auditoría:** Todos los cambios del toggle quedan registrados.

5. **Extensible:** Para agregar un nuevo módulo, solo hay que agregar entradas en `ConfiguracionRoles._permisos`.

---

*Prompt creado: Enero 2026*
*Versión: 1.0*
*Alcance: Módulo Empleados*
