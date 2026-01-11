using SGRRHH.Local.Domain.DTOs;

namespace SGRRHH.Local.Shared.Interfaces;

/// <summary>
/// Repositorio para gestionar perfiles de escaneo guardados
/// </summary>
public interface IScanProfileRepository
{
    /// <summary>
    /// Obtiene todos los perfiles de escaneo
    /// </summary>
    Task<List<ScanProfileDto>> GetAllAsync();
    
    /// <summary>
    /// Obtiene un perfil por su ID
    /// </summary>
    Task<ScanProfileDto?> GetByIdAsync(int id);
    
    /// <summary>
    /// Obtiene el perfil marcado como predeterminado
    /// </summary>
    Task<ScanProfileDto?> GetDefaultAsync();
    
    /// <summary>
    /// Guarda un nuevo perfil o actualiza uno existente
    /// </summary>
    Task<ScanProfileDto> SaveAsync(ScanProfileDto profile);
    
    /// <summary>
    /// Elimina un perfil
    /// </summary>
    Task DeleteAsync(int id);
    
    /// <summary>
    /// Establece un perfil como predeterminado (desmarca los demás)
    /// </summary>
    Task SetDefaultAsync(int id);
    
    /// <summary>
    /// Actualiza la fecha de último uso del perfil
    /// </summary>
    Task UpdateLastUsedAsync(int id);
    
    /// <summary>
    /// Inicializa perfiles predeterminados si no existen
    /// </summary>
    Task InitializeDefaultProfilesAsync();
}
