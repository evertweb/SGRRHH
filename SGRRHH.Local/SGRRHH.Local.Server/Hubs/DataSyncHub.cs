using Microsoft.AspNetCore.SignalR;

namespace SGRRHH.Local.Server.Hubs;

/// <summary>
/// Hub centralizado para sincronización de datos en tiempo real.
/// Notifica a todos los usuarios conectados cuando hay cambios en entidades.
/// </summary>
public class DataSyncHub : Hub
{
    /// <summary>
    /// Notifica cambios de datos a todos los clientes conectados EXCEPTO al emisor.
    /// </summary>
    /// <param name="entityType">Tipo de entidad (Cargo, Departamento, Empleado, etc.)</param>
    /// <param name="changeType">Tipo de cambio (Created, Updated, Deleted)</param>
    /// <param name="entityId">ID opcional de la entidad afectada</param>
    public async Task NotifyDataChange(string entityType, string changeType, int? entityId = null)
    {
        // Notificar a TODOS los clientes EXCEPTO quien hizo el cambio
        // Esto previene que el usuario que guardó vea una recarga redundante
        await Clients.Others.SendAsync("DataChanged", entityType, changeType, entityId);
    }
}
