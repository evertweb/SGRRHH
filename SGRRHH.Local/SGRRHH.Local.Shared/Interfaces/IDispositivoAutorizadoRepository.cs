using SGRRHH.Local.Domain.Entities;

namespace SGRRHH.Local.Shared.Interfaces;

/// <summary>
/// Repositorio para gestión de dispositivos autorizados.
/// </summary>
public interface IDispositivoAutorizadoRepository
{
    /// <summary>
    /// Agrega un nuevo dispositivo autorizado.
    /// </summary>
    Task<DispositivoAutorizado> AddAsync(DispositivoAutorizado entity);
    
    /// <summary>
    /// Actualiza un dispositivo existente.
    /// </summary>
    Task UpdateAsync(DispositivoAutorizado entity);
    
    /// <summary>
    /// Elimina (hard delete) un dispositivo.
    /// </summary>
    Task DeleteAsync(int id);
    
    /// <summary>
    /// Obtiene un dispositivo por su ID.
    /// </summary>
    Task<DispositivoAutorizado?> GetByIdAsync(int id);
    
    /// <summary>
    /// Obtiene un dispositivo por su token único.
    /// </summary>
    Task<DispositivoAutorizado?> GetByTokenAsync(string deviceToken);
    
    /// <summary>
    /// Obtiene todos los dispositivos activos de un usuario.
    /// </summary>
    Task<IEnumerable<DispositivoAutorizado>> GetByUsuarioIdAsync(int usuarioId);
    
    /// <summary>
    /// Obtiene todos los dispositivos (para administración).
    /// </summary>
    Task<IEnumerable<DispositivoAutorizado>> GetAllAsync();
    
    /// <summary>
    /// Desactiva (revoca) un dispositivo.
    /// </summary>
    Task RevocarAsync(int id);
    
    /// <summary>
    /// Desactiva todos los dispositivos de un usuario.
    /// </summary>
    Task RevocarTodosDeUsuarioAsync(int usuarioId);
    
    /// <summary>
    /// Actualiza la fecha de último uso de un token.
    /// </summary>
    Task ActualizarUltimoUsoAsync(int id);
    
    /// <summary>
    /// Verifica si un token existe y está activo.
    /// </summary>
    Task<bool> TokenActivoExisteAsync(string deviceToken);
    
    /// <summary>
    /// Cuenta dispositivos activos de un usuario.
    /// </summary>
    Task<int> ContarDispositivosActivosAsync(int usuarioId);
}
