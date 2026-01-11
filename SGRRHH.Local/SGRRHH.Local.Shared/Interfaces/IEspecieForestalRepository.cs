using SGRRHH.Local.Domain.Entities;

namespace SGRRHH.Local.Shared.Interfaces;

/// <summary>
/// Repositorio para gestión de especies forestales
/// </summary>
public interface IEspecieForestalRepository : IRepository<EspecieForestal>
{
    /// <summary>
    /// Busca especies por término (código, nombre común o científico)
    /// </summary>
    Task<IEnumerable<EspecieForestal>> SearchAsync(string searchTerm);

    /// <summary>
    /// Verifica si existe un código de especie
    /// </summary>
    Task<bool> ExistsCodigoAsync(string codigo, int? excludeId = null);

    /// <summary>
    /// Obtiene especie por su código
    /// </summary>
    Task<EspecieForestal?> GetByCodigoAsync(string codigo);

    /// <summary>
    /// Obtiene las especies más utilizadas (por cantidad de proyectos)
    /// </summary>
    Task<IEnumerable<EspecieForestal>> GetMasUtilizadasAsync(int top = 10);
}
