using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using SGRRHH.Local.Server.Hubs;
using SGRRHH.Local.Shared.Interfaces;

namespace SGRRHH.Local.Server.Services;

/// <summary>
/// Implementación del servicio de notificación de cambios de datos.
/// Propaga eventos de creación/actualización/eliminación a todos los usuarios conectados.
/// </summary>
public class DataSyncNotifier : IDataSyncNotifier
{
    private readonly IHubContext<DataSyncHub> _hubContext;
    private readonly ILogger<DataSyncNotifier> _logger;

    public DataSyncNotifier(
        IHubContext<DataSyncHub> hubContext,
        ILogger<DataSyncNotifier> logger)
    {
        _hubContext = hubContext;
        _logger = logger;
    }

    public async Task NotifyChangeAsync(string entityType, string changeType, int? entityId = null)
    {
        try
        {
            // Notificar a TODOS los clientes conectados
            await _hubContext.Clients.All.SendAsync("DataChanged", entityType, changeType, entityId);
            
            _logger.LogDebug(
                "Notificación de cambio enviada: {EntityType} {ChangeType} (ID: {EntityId})",
                entityType, changeType, entityId?.ToString() ?? "N/A");
        }
        catch (Exception ex)
        {
            // No fallar si la notificación falla, solo registrar el error
            _logger.LogWarning(ex, 
                "Error al enviar notificación SignalR para {EntityType} {ChangeType}",
                entityType, changeType);
        }
    }
}
