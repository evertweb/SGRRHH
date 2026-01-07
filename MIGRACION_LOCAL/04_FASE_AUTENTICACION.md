# 🔐 FASE 3: Autenticación Local

## 📋 Contexto

Fases anteriores completadas:
- ✅ Estructura base del proyecto
- ✅ Infraestructura SQLite con Dapper
- ✅ Sistema de archivos local

**Objetivo:** Implementar autenticación local con BCrypt, reemplazando Firebase Auth.

---

## 🎯 Objetivo de esta Fase

Crear LocalAuthService con login/logout, hashing de contraseñas con BCrypt, y gestión de sesión en memoria.

---

## 📝 PROMPT PARA CLAUDE

```
Necesito que implementes el sistema de autenticación local para SGRRHH.Local que reemplaza a Firebase Auth.

**PROYECTO:** SGRRHH.Local.Infrastructure/Services/

**CARACTERÍSTICAS REQUERIDAS:**
- Login con username/password
- Hashing de contraseñas con BCrypt
- Sesión en memoria (no tokens JWT)
- Roles de usuario (Admin, Aprobador, Operador)
- Cambio de contraseña
- Último acceso tracking

---

## ARCHIVOS A CREAR:

### 1. Interfaz IAuthService.cs (en Shared/Interfaces/)

```csharp
public interface IAuthService
{
    // Autenticación
    Task<Result<Usuario>> LoginAsync(string username, string password);
    Task<Result> LogoutAsync();
    
    // Estado de sesión
    Usuario? CurrentUser { get; }
    bool IsAuthenticated { get; }
    bool IsAdmin { get; }
    bool IsAprobador { get; }
    bool IsOperador { get; }
    
    // Gestión de contraseñas
    Task<Result> ChangePasswordAsync(int userId, string currentPassword, string newPassword);
    Task<Result> ResetPasswordAsync(int userId, string newPassword); // Solo admin
    string HashPassword(string password);
    bool VerifyPassword(string password, string hash);
    
    // Eventos
    event EventHandler<Usuario?> OnAuthStateChanged;
}
```

---

### 2. LocalAuthService.cs (en Infrastructure/Services/)

```csharp
using BCrypt.Net;

public class LocalAuthService : IAuthService
{
    private readonly IUsuarioRepository _usuarioRepository;
    private readonly IAuditLogRepository _auditRepository;
    private readonly ILogger<LocalAuthService> _logger;
    
    private Usuario? _currentUser;
    
    public Usuario? CurrentUser => _currentUser;
    public bool IsAuthenticated => _currentUser != null;
    public bool IsAdmin => _currentUser?.Rol == RolUsuario.Administrador;
    public bool IsAprobador => _currentUser?.Rol == RolUsuario.Aprobador || IsAdmin;
    public bool IsOperador => _currentUser?.Rol == RolUsuario.Operador || IsAdmin;
    
    public event EventHandler<Usuario?>? OnAuthStateChanged;
    
    public LocalAuthService(
        IUsuarioRepository usuarioRepository,
        IAuditLogRepository auditRepository,
        ILogger<LocalAuthService> logger)
    {
        _usuarioRepository = usuarioRepository;
        _auditRepository = auditRepository;
        _logger = logger;
    }
    
    public async Task<Result<Usuario>> LoginAsync(string username, string password)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                return Result<Usuario>.Fail("Usuario y contraseña son requeridos");
            }
            
            var usuario = await _usuarioRepository.GetByUsernameAsync(username);
            
            if (usuario == null)
            {
                _logger.LogWarning("Intento de login fallido: usuario no existe ({Username})", username);
                await RegistrarAuditoria("LOGIN_FALLIDO", "Usuario", null, $"Usuario no existe: {username}");
                return Result<Usuario>.Fail("Usuario o contraseña incorrectos");
            }
            
            if (!usuario.Activo)
            {
                _logger.LogWarning("Intento de login con usuario inactivo: {Username}", username);
                return Result<Usuario>.Fail("Usuario deshabilitado. Contacte al administrador.");
            }
            
            if (!VerifyPassword(password, usuario.PasswordHash))
            {
                _logger.LogWarning("Intento de login fallido: contraseña incorrecta ({Username})", username);
                await RegistrarAuditoria("LOGIN_FALLIDO", "Usuario", usuario.Id, "Contraseña incorrecta");
                return Result<Usuario>.Fail("Usuario o contraseña incorrectos");
            }
            
            // Login exitoso
            _currentUser = usuario;
            await _usuarioRepository.UpdateLastAccessAsync(usuario.Id);
            
            _logger.LogInformation("Login exitoso: {Username} (Rol: {Rol})", username, usuario.Rol);
            await RegistrarAuditoria("LOGIN_EXITOSO", "Usuario", usuario.Id, $"Login exitoso como {usuario.Rol}");
            
            OnAuthStateChanged?.Invoke(this, _currentUser);
            
            return Result<Usuario>.Ok(usuario, "Bienvenido, " + usuario.NombreCompleto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error durante login de {Username}", username);
            return Result<Usuario>.Fail("Error interno. Intente nuevamente.");
        }
    }
    
    public Task<Result> LogoutAsync()
    {
        if (_currentUser != null)
        {
            _logger.LogInformation("Logout: {Username}", _currentUser.Username);
            _ = RegistrarAuditoria("LOGOUT", "Usuario", _currentUser.Id, "Cierre de sesión");
        }
        
        _currentUser = null;
        OnAuthStateChanged?.Invoke(this, null);
        
        return Task.FromResult(Result.Ok("Sesión cerrada"));
    }
    
    public async Task<Result> ChangePasswordAsync(int userId, string currentPassword, string newPassword)
    {
        try
        {
            var usuario = await _usuarioRepository.GetByIdAsync(userId);
            if (usuario == null)
                return Result.Fail("Usuario no encontrado");
            
            // Solo el propio usuario o admin puede cambiar contraseña
            if (_currentUser?.Id != userId && !IsAdmin)
                return Result.Fail("No tiene permisos para cambiar esta contraseña");
            
            // Verificar contraseña actual (excepto si es admin reseteando)
            if (_currentUser?.Id == userId)
            {
                if (!VerifyPassword(currentPassword, usuario.PasswordHash))
                    return Result.Fail("Contraseña actual incorrecta");
            }
            
            // Validar nueva contraseña
            var validacion = ValidarPassword(newPassword);
            if (!validacion.Success)
                return validacion;
            
            // Actualizar contraseña
            usuario.PasswordHash = HashPassword(newPassword);
            await _usuarioRepository.UpdateAsync(usuario);
            
            _logger.LogInformation("Contraseña cambiada para usuario {UserId}", userId);
            await RegistrarAuditoria("CAMBIO_PASSWORD", "Usuario", userId, "Contraseña actualizada");
            
            return Result.Ok("Contraseña actualizada correctamente");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cambiando contraseña de usuario {UserId}", userId);
            return Result.Fail("Error al cambiar contraseña");
        }
    }
    
    public async Task<Result> ResetPasswordAsync(int userId, string newPassword)
    {
        if (!IsAdmin)
            return Result.Fail("Solo administradores pueden resetear contraseñas");
        
        return await ChangePasswordAsync(userId, "", newPassword);
    }
    
    public string HashPassword(string password)
    {
        return BCrypt.Net.BCrypt.HashPassword(password, workFactor: 11);
    }
    
    public bool VerifyPassword(string password, string hash)
    {
        try
        {
            return BCrypt.Net.BCrypt.Verify(password, hash);
        }
        catch
        {
            return false;
        }
    }
    
    private Result ValidarPassword(string password)
    {
        if (string.IsNullOrWhiteSpace(password))
            return Result.Fail("La contraseña es requerida");
        
        if (password.Length < 6)
            return Result.Fail("La contraseña debe tener al menos 6 caracteres");
        
        // Agregar más validaciones si se requiere (mayúsculas, números, etc.)
        
        return Result.Ok();
    }
    
    private async Task RegistrarAuditoria(string accion, string entidad, int? entidadId, string descripcion)
    {
        try
        {
            var log = new AuditLog
            {
                FechaHora = DateTime.Now,
                UsuarioId = _currentUser?.Id,
                UsuarioNombre = _currentUser?.NombreCompleto ?? "Sistema",
                Accion = accion,
                Entidad = entidad,
                EntidadId = entidadId,
                Descripcion = descripcion
            };
            
            await _auditRepository.CreateAsync(log);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error registrando auditoría: {Accion}", accion);
        }
    }
}
```

---

### 3. AuthorizeViewLocal.cs (Componente de autorización para Blazor)

```csharp
// En SGRRHH.Local.Server/Components/Shared/AuthorizeViewLocal.razor

@inject IAuthService AuthService

@if (IsAuthorized)
{
    @Authorized
}
else
{
    @NotAuthorized
}

@code {
    [Parameter] public RenderFragment? Authorized { get; set; }
    [Parameter] public RenderFragment? NotAuthorized { get; set; }
    [Parameter] public RolUsuario? RequiredRole { get; set; }
    
    private bool IsAuthorized
    {
        get
        {
            if (!AuthService.IsAuthenticated)
                return false;
            
            if (RequiredRole == null)
                return true;
            
            return RequiredRole switch
            {
                RolUsuario.Administrador => AuthService.IsAdmin,
                RolUsuario.Aprobador => AuthService.IsAprobador,
                RolUsuario.Operador => AuthService.IsOperador,
                _ => false
            };
        }
    }
}
```

---

### 4. Actualizar Usuario para hash inicial:

Para crear el usuario admin por defecto, en DatabaseInitializer:

```csharp
// Password: admin123
// Hash generado con BCrypt (work factor 11):
private const string ADMIN_PASSWORD_HASH = "$2a$11$8K1p/a0dR8l6DxN8w.OQl.6c3vD8qN5Y2aK9mF3hG1jL2sT4wU6vC";

// En el SQL de inicialización:
INSERT INTO usuarios (username, password_hash, nombre_completo, rol, activo, created_at)
VALUES ('admin', '$2a$11$...hash...', 'Administrador del Sistema', 1, 1, datetime('now'));
```

---

### 5. Login.razor (Página de login)

```razor
@page "/login"
@page "/"
@layout EmptyLayout
@inject IAuthService AuthService
@inject NavigationManager Navigation

<div class="login-container">
    <div class="login-box">
        <h2>SGRRHH LOCAL</h2>
        <h3>Sistema de Gestión de Recursos Humanos</h3>
        
        @if (!string.IsNullOrEmpty(errorMessage))
        {
            <div class="error-block">
                <p>@errorMessage</p>
            </div>
        }
        
        <div class="form-group">
            <label>Usuario:</label>
            <input type="text" 
                   @bind="username" 
                   @onkeyup="HandleKeyUp"
                   placeholder="Ingrese su usuario"
                   autofocus />
        </div>
        
        <div class="form-group">
            <label>Contraseña:</label>
            <input type="password" 
                   @bind="password" 
                   @onkeyup="HandleKeyUp"
                   placeholder="Ingrese su contraseña" />
        </div>
        
        <div style="margin-top: 16px;">
            <button @onclick="DoLogin" disabled="@isLoading">
                @if (isLoading)
                {
                    <span>INGRESANDO...</span>
                }
                else
                {
                    <span>INGRESAR</span>
                }
            </button>
        </div>
        
        <div class="login-footer">
            <small>Versión 1.0.0 | © 2026 SGRRHH</small>
        </div>
    </div>
</div>

@code {
    private string username = "";
    private string password = "";
    private string? errorMessage;
    private bool isLoading;
    
    protected override void OnInitialized()
    {
        // Si ya está autenticado, ir al dashboard
        if (AuthService.IsAuthenticated)
        {
            Navigation.NavigateTo("/dashboard");
        }
    }
    
    private async Task HandleKeyUp(KeyboardEventArgs e)
    {
        if (e.Key == "Enter")
        {
            await DoLogin();
        }
    }
    
    private async Task DoLogin()
    {
        if (isLoading) return;
        
        isLoading = true;
        errorMessage = null;
        StateHasChanged();
        
        try
        {
            var result = await AuthService.LoginAsync(username, password);
            
            if (result.Success)
            {
                Navigation.NavigateTo("/dashboard");
            }
            else
            {
                errorMessage = result.Message;
            }
        }
        finally
        {
            isLoading = false;
            StateHasChanged();
        }
    }
}
```

---

### 6. EmptyLayout.razor (Layout sin menú para login)

```razor
@inherits LayoutComponentBase

<div class="empty-layout">
    @Body
</div>

<style>
.empty-layout {
    min-height: 100vh;
    display: flex;
    align-items: center;
    justify-content: center;
    background-color: #E0E0E0;
}

.login-container {
    width: 100%;
    max-width: 400px;
}

.login-box {
    background-color: #FFFFFF;
    border: 2px solid #000000;
    padding: 32px;
}

.login-box h2 {
    margin: 0 0 8px 0;
    font-size: 24px;
    text-align: center;
}

.login-box h3 {
    margin: 0 0 24px 0;
    font-size: 14px;
    font-weight: normal;
    text-align: center;
    color: #666666;
}

.login-footer {
    margin-top: 24px;
    text-align: center;
    color: #808080;
}
</style>
```

---

## FLUJO DE AUTENTICACIÓN:

1. Usuario accede a la app → Redirige a /login si no autenticado
2. Ingresa credenciales → LocalAuthService.LoginAsync()
3. Verifica hash con BCrypt → Si OK, guarda usuario en memoria
4. Dispara evento OnAuthStateChanged
5. MainLayout escucha evento y actualiza UI
6. Logout limpia _currentUser y redirige a /login

---

## PROTECCIÓN DE RUTAS:

En cada página protegida:

```razor
@page "/empleados"
@inject IAuthService AuthService
@inject NavigationManager Navigation

@code {
    protected override void OnInitialized()
    {
        if (!AuthService.IsAuthenticated)
        {
            Navigation.NavigateTo("/login");
            return;
        }
        
        // Para páginas solo de admin:
        if (!AuthService.IsAdmin)
        {
            Navigation.NavigateTo("/acceso-denegado");
            return;
        }
    }
}
```

---

**IMPORTANTE:**
- NO usar JWT ni tokens, es una app local single-user
- La sesión vive mientras la app esté abierta
- Al cerrar/reiniciar la app, el usuario debe loguearse de nuevo
- El admin por defecto debe poder crear otros usuarios
- Loggear todos los intentos de login (exitosos y fallidos)
```

---

## ✅ Checklist de Entregables

- [ ] Shared/Interfaces/IAuthService.cs
- [ ] Infrastructure/Services/LocalAuthService.cs
- [ ] Server/Components/Shared/AuthorizeViewLocal.razor
- [ ] Server/Components/Pages/Login.razor
- [ ] Server/Components/Layout/EmptyLayout.razor
- [ ] Actualizar DatabaseInitializer con usuario admin
- [ ] Registrar servicios en Program.cs

---

## 🔗 Dependencias NuGet

```xml
<!-- Ya debe estar instalado en Infrastructure -->
<PackageReference Include="BCrypt.Net-Next" Version="4.0.3" />
```

---

## 🔒 Consideraciones de Seguridad

1. **Work Factor de BCrypt:** Usar 11 (balance entre seguridad y performance)
2. **No almacenar contraseñas en logs**
3. **Mensaje genérico en login fallido** ("Usuario o contraseña incorrectos")
4. **Registro de auditoría** para todos los intentos de login
5. **Validación de contraseña** mínimo 6 caracteres
