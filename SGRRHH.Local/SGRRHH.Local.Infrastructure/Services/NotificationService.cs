using Microsoft.Extensions.Logging;
using SGRRHH.Local.Domain.DTOs;
using SGRRHH.Local.Domain.Enums;
using SGRRHH.Local.Domain.Interfaces;
using SGRRHH.Local.Shared.Interfaces;

namespace SGRRHH.Local.Infrastructure.Services;

/// <summary>
/// Servicio de notificaciones con persistencia en base de datos
/// </summary>
public class NotificationService : INotificationService
{
    private readonly INotificacionRepository _repository;
    private readonly IPermisoRepository _permisoRepository;
    private readonly IVacacionRepository _vacacionRepository;
    private readonly IAuthService _authService;
    private readonly ILogger<NotificationService> _logger;
    
    // Caché local para evitar consultas constantes
    private int _cachedCount = 0;
    private NotificacionResumenDto? _cachedResumen;
    private DateTime _lastRefresh = DateTime.MinValue;
    private DateTime _lastCheck = DateTime.MinValue;
    private readonly TimeSpan _cacheExpiration = TimeSpan.FromSeconds(30);
    
    public event EventHandler<NotificationEventArgs>? OnNotification;
    public event EventHandler? OnCountChanged;
    
    public int UnreadCount => _cachedCount;
    public NotificacionResumenDto? Resumen => _cachedResumen;
    
    public NotificationService(
        INotificacionRepository repository,
        IPermisoRepository permisoRepository,
        IVacacionRepository vacacionRepository,
        IAuthService authService,
        ILogger<NotificationService> logger)
    {
        _repository = repository;
        _permisoRepository = permisoRepository;
        _vacacionRepository = vacacionRepository;
        _authService = authService;
        _logger = logger;
    }
    
    public async Task InitializeAsync()
    {
        if (!_authService.IsAuthenticated) return;
        
        try
        {
            await RefreshAsync();
            _logger.LogDebug("NotificationService inicializado para usuario {User}", _authService.CurrentUser?.NombreCompleto);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error al inicializar NotificationService");
        }
    }
    
    public async Task<List<NotificacionDto>> GetNotificacionesAsync(int limite = 20)
    {
        if (!_authService.IsAuthenticated) return new List<NotificacionDto>();
        
        try
        {
            var filtro = new NotificacionFiltroDto
            {
                UsuarioId = _authService.CurrentUser?.Id,
                SoloNoLeidas = true,
                Limite = limite
            };
            
            return await _repository.GetNotificacionesAsync(filtro);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener notificaciones");
            return new List<NotificacionDto>();
        }
    }
    
    public async Task<NotificacionResumenDto> GetResumenAsync()
    {
        if (!_authService.IsAuthenticated)
            return new NotificacionResumenDto();
        
        try
        {
            // Verificar si el caché está fresco
            if (_cachedResumen != null && DateTime.Now - _lastRefresh < _cacheExpiration)
            {
                return _cachedResumen;
            }
            
            _cachedResumen = await _repository.GetResumenAsync(_authService.CurrentUser?.Id);
            _cachedCount = _cachedResumen.TotalNoLeidas;
            _lastRefresh = DateTime.Now;
            
            return _cachedResumen;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener resumen de notificaciones");
            return new NotificacionResumenDto();
        }
    }
    
    public async Task<int> SendNotificationAsync(CrearNotificacionDto dto)
    {
        try
        {
            var creadoPor = _authService.CurrentUser?.Username ?? "Sistema";
            var id = await _repository.CreateAsync(dto, creadoPor);
            
            // Invalidar caché y notificar
            await RefreshAsync();
            
            var notificacionDto = new NotificacionDto
            {
                Id = id,
                Titulo = dto.Titulo,
                Mensaje = dto.Mensaje,
                Tipo = dto.Tipo,
                Categoria = dto.Categoria,
                Prioridad = dto.Prioridad,
                Link = dto.Link,
                FechaCreacion = DateTime.Now,
                TiempoRelativo = "Ahora"
            };
            
            OnNotification?.Invoke(this, new NotificationEventArgs { Notificacion = notificacionDto });
            
            return id;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al enviar notificación: {Titulo}", dto.Titulo);
            return 0;
        }
    }
    
    public async Task SendNotificationAsync(string titulo, string mensaje, string tipo, string? link = null)
    {
        await SendNotificationAsync(new CrearNotificacionDto
        {
            Titulo = titulo,
            Mensaje = mensaje,
            Tipo = tipo,
            Categoria = "Sistema",
            Link = link
        });
    }
    
    public async Task NotificarPermisoNuevoAsync(int permisoId, string empleadoNombre, string tipoPermiso)
    {
        // Evitar duplicados
        if (await _repository.ExisteNotificacionEntidadAsync("Permiso", permisoId, "PermisoNuevo"))
            return;
        
        await SendNotificationAsync(new CrearNotificacionDto
        {
            Titulo = "Nuevo Permiso Pendiente",
            Mensaje = $"{empleadoNombre} ha solicitado un permiso de {tipoPermiso}",
            Tipo = "PermisoNuevo",
            Categoria = "Permiso",
            Prioridad = 1,
            Link = $"/permisos/{permisoId}",
            EntidadTipo = "Permiso",
            EntidadId = permisoId,
            FechaExpiracion = DateTime.Now.AddDays(7)
        });
    }
    
    public async Task NotificarVacacionNuevaAsync(int vacacionId, string empleadoNombre, int diasSolicitados)
    {
        if (await _repository.ExisteNotificacionEntidadAsync("Vacacion", vacacionId, "VacacionNueva"))
            return;
        
        await SendNotificationAsync(new CrearNotificacionDto
        {
            Titulo = "Nueva Solicitud de Vacaciones",
            Mensaje = $"{empleadoNombre} solicita {diasSolicitados} día(s) de vacaciones",
            Tipo = "VacacionNueva",
            Categoria = "Vacacion",
            Prioridad = 1,
            Link = $"/vacaciones/{vacacionId}",
            EntidadTipo = "Vacacion",
            EntidadId = vacacionId,
            FechaExpiracion = DateTime.Now.AddDays(14)
        });
    }
    
    public async Task NotificarIncapacidadNuevaAsync(int incapacidadId, string empleadoNombre, string tipoIncapacidad)
    {
        if (await _repository.ExisteNotificacionEntidadAsync("Incapacidad", incapacidadId, "IncapacidadNueva"))
            return;
        
        await SendNotificationAsync(new CrearNotificacionDto
        {
            Titulo = "Nueva Incapacidad Registrada",
            Mensaje = $"{empleadoNombre} - {tipoIncapacidad}",
            Tipo = "IncapacidadNueva",
            Categoria = "Incapacidad",
            Prioridad = 1,
            Link = $"/incapacidades/{incapacidadId}",
            EntidadTipo = "Incapacidad",
            EntidadId = incapacidadId,
            FechaExpiracion = DateTime.Now.AddDays(30)
        });
    }
    
    public async Task MarkAsReadAsync(int notificationId)
    {
        try
        {
            await _repository.MarcarLeidaAsync(notificationId);
            await RefreshAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al marcar notificación {Id} como leída", notificationId);
        }
    }
    
    public async Task MarkAllAsReadAsync()
    {
        try
        {
            var usuarioId = _authService.CurrentUser?.Id;
            await _repository.MarcarTodasLeidasAsync(usuarioId);
            _cachedCount = 0;
            _cachedResumen = null;
            OnCountChanged?.Invoke(this, EventArgs.Empty);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al marcar todas las notificaciones como leídas");
        }
    }
    
    public async Task RefreshAsync()
    {
        try
        {
            var usuarioId = _authService.CurrentUser?.Id;
            var newCount = await _repository.GetCountNoLeidasAsync(usuarioId);
            
            if (newCount != _cachedCount)
            {
                _cachedCount = newCount;
                _cachedResumen = null; // Invalidar resumen para forzar recarga
                OnCountChanged?.Invoke(this, EventArgs.Empty);
            }
            
            _lastRefresh = DateTime.Now;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error al refrescar contador de notificaciones");
        }
    }
    
    public async Task CheckForNewNotificationsAsync()
    {
        if (!_authService.IsAuthenticated || !_authService.IsAprobador) return;
        
        try
        {
            // Verificar permisos pendientes nuevos
            await CheckPendingPermisosAsync();
            
            // Verificar vacaciones pendientes nuevas
            await CheckPendingVacacionesAsync();
            
            // Refrescar contador
            await RefreshAsync();
            
            _lastCheck = DateTime.Now;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error al verificar nuevas notificaciones");
        }
    }
    
    public async Task LimpiarAntiguasAsync(int diasAntiguedad = 30)
    {
        try
        {
            var eliminadas = await _repository.LimpiarAntiguasAsync(diasAntiguedad);
            if (eliminadas > 0)
            {
                _logger.LogInformation("Limpieza de notificaciones: {Count} eliminadas", eliminadas);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error al limpiar notificaciones antiguas");
        }
    }
    
    #region Private Methods
    
    private async Task CheckPendingPermisosAsync()
    {
        try
        {
            var permisos = await _permisoRepository.GetAllAsync();
            var pendientes = permisos.Where(p => 
                p.Estado == EstadoPermiso.Pendiente && 
                p.FechaSolicitud > _lastCheck).ToList();
            
            foreach (var permiso in pendientes)
            {
                await NotificarPermisoNuevoAsync(
                    permiso.Id,
                    permiso.Empleado?.NombreCompleto ?? "Empleado",
                    permiso.TipoPermiso?.Nombre ?? "N/A"
                );
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error al verificar permisos pendientes");
        }
    }
    
    private async Task CheckPendingVacacionesAsync()
    {
        try
        {
            var vacaciones = await _vacacionRepository.GetAllAsync();
            var pendientes = vacaciones.Where(v => 
                v.Estado == EstadoVacacion.Pendiente && 
                v.FechaSolicitud > _lastCheck).ToList();
            
            foreach (var vacacion in pendientes)
            {
                await NotificarVacacionNuevaAsync(
                    vacacion.Id,
                    vacacion.Empleado?.NombreCompleto ?? "Empleado",
                    vacacion.DiasTomados
                );
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error al verificar vacaciones pendientes");
        }
    }
    
    #endregion
}
