using SGRRHH.Local.Domain.Entities;
using SGRRHH.Local.Domain.Enums;

namespace SGRRHH.Local.Domain.Interfaces;

/// <summary>
/// Repositorio para gestión de aspirantes
/// </summary>
public interface IAspiranteRepositorio
{
    /// <summary>
    /// Obtiene un aspirante por ID
    /// </summary>
    Task<Aspirante?> ObtenerPorIdAsync(int id);

    /// <summary>
    /// Obtiene un aspirante por cédula
    /// </summary>
    Task<Aspirante?> ObtenerPorCedulaAsync(string cedula);

    /// <summary>
    /// Obtiene todos los aspirantes
    /// </summary>
    Task<List<Aspirante>> ObtenerTodosAsync(bool incluirInactivos = false);

    /// <summary>
    /// Obtiene aspirantes por vacante
    /// </summary>
    Task<List<Aspirante>> ObtenerPorVacanteAsync(int vacanteId);

    /// <summary>
    /// Obtiene aspirantes por estado
    /// </summary>
    Task<List<Aspirante>> ObtenerPorEstadoAsync(EstadoAspirante estado);

    /// <summary>
    /// Busca aspirantes por texto (nombre, cédula, etc.)
    /// </summary>
    Task<List<Aspirante>> BuscarAsync(string termino);

    /// <summary>
    /// Crea un nuevo aspirante
    /// </summary>
    Task<int> CrearAsync(Aspirante aspirante);

    /// <summary>
    /// Actualiza un aspirante existente
    /// </summary>
    Task<bool> ActualizarAsync(Aspirante aspirante);

    /// <summary>
    /// Cambia el estado de un aspirante
    /// </summary>
    Task<bool> CambiarEstadoAsync(int id, EstadoAspirante nuevoEstado, string? notas = null);

    /// <summary>
    /// Actualiza el puntaje de evaluación
    /// </summary>
    Task<bool> ActualizarPuntajeAsync(int id, int puntaje);

    /// <summary>
    /// Elimina (desactiva) un aspirante
    /// </summary>
    Task<bool> EliminarAsync(int id);

    /// <summary>
    /// Verifica si existe un aspirante con la cédula dada
    /// </summary>
    Task<bool> ExisteCedulaAsync(string cedula, int? excluirId = null);

    /// <summary>
    /// Obtiene la formación académica de un aspirante
    /// </summary>
    Task<List<FormacionAspirante>> ObtenerFormacionAsync(int aspiranteId);

    /// <summary>
    /// Obtiene la experiencia laboral de un aspirante
    /// </summary>
    Task<List<ExperienciaAspirante>> ObtenerExperienciaAsync(int aspiranteId);

    /// <summary>
    /// Obtiene las referencias de un aspirante
    /// </summary>
    Task<List<ReferenciaAspirante>> ObtenerReferenciasAsync(int aspiranteId);

    /// <summary>
    /// Guarda la formación de un aspirante (reemplaza las existentes)
    /// </summary>
    Task<bool> GuardarFormacionAsync(int aspiranteId, List<FormacionAspirante> formaciones);

    /// <summary>
    /// Guarda la experiencia de un aspirante (reemplaza las existentes)
    /// </summary>
    Task<bool> GuardarExperienciaAsync(int aspiranteId, List<ExperienciaAspirante> experiencias);

    /// <summary>
    /// Guarda las referencias de un aspirante (reemplaza las existentes)
    /// </summary>
    Task<bool> GuardarReferenciasAsync(int aspiranteId, List<ReferenciaAspirante> referencias);
}
