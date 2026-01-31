using SGRRHH.Local.Domain.Entities;
using SGRRHH.Local.Domain.Enums;

namespace SGRRHH.Local.Domain.Interfaces;

/// <summary>
/// Repositorio para gestión de vacantes
/// </summary>
public interface IVacanteRepositorio
{
    /// <summary>
    /// Obtiene una vacante por ID
    /// </summary>
    Task<Vacante?> ObtenerPorIdAsync(int id);

    /// <summary>
    /// Obtiene todas las vacantes activas
    /// </summary>
    Task<List<Vacante>> ObtenerTodosAsync(bool incluirInactivos = false);

    /// <summary>
    /// Obtiene vacantes por estado
    /// </summary>
    Task<List<Vacante>> ObtenerPorEstadoAsync(EstadoVacante estado);

    /// <summary>
    /// Obtiene vacantes abiertas (para selector)
    /// </summary>
    Task<List<Vacante>> ObtenerAbiertasAsync();

    /// <summary>
    /// Crea una nueva vacante
    /// </summary>
    Task<int> CrearAsync(Vacante vacante);

    /// <summary>
    /// Actualiza una vacante existente
    /// </summary>
    Task<bool> ActualizarAsync(Vacante vacante);

    /// <summary>
    /// Cambia el estado de una vacante
    /// </summary>
    Task<bool> CambiarEstadoAsync(int id, EstadoVacante nuevoEstado);

    /// <summary>
    /// Elimina (desactiva) una vacante
    /// </summary>
    Task<bool> EliminarAsync(int id);

    /// <summary>
    /// Obtiene el conteo de aspirantes por vacante
    /// </summary>
    Task<int> ContarAspirantesAsync(int vacanteId);
}
