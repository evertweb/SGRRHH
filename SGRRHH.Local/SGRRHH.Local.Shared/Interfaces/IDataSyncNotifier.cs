namespace SGRRHH.Local.Shared.Interfaces;

/// <summary>
/// Servicio para notificar cambios de datos en tiempo real a través de SignalR.
/// Los componentes usan este servicio para notificar cuando crean, actualizan o eliminan entidades.
/// </summary>
public interface IDataSyncNotifier
{
    /// <summary>
    /// Notifica un cambio en una entidad a todos los usuarios conectados.
    /// </summary>
    /// <param name="entityType">Tipo de entidad (Cargo, Departamento, Empleado, etc.)</param>
    /// <param name="changeType">Tipo de cambio: "Created", "Updated", "Deleted"</param>
    /// <param name="entityId">ID opcional de la entidad afectada</param>
    Task NotifyChangeAsync(string entityType, string changeType, int? entityId = null);
}
