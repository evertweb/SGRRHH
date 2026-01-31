using SGRRHH.Local.Domain.Entities;
using SGRRHH.Local.Domain.Enums;

namespace SGRRHH.Local.Domain.Interfaces;

/// <summary>
/// Repositorio para gestión de PDFs de hojas de vida
/// </summary>
public interface IHojaVidaPdfRepositorio
{
    /// <summary>
    /// Obtiene un registro de PDF por ID
    /// </summary>
    Task<HojaVidaPdf?> ObtenerPorIdAsync(int id);

    /// <summary>
    /// Obtiene los PDFs de un aspirante
    /// </summary>
    Task<List<HojaVidaPdf>> ObtenerPorAspiranteAsync(int aspiranteId);

    /// <summary>
    /// Obtiene los PDFs de un empleado
    /// </summary>
    Task<List<HojaVidaPdf>> ObtenerPorEmpleadoAsync(int empleadoId);

    /// <summary>
    /// Obtiene la última versión del PDF de un aspirante
    /// </summary>
    Task<HojaVidaPdf?> ObtenerUltimaVersionAspiranteAsync(int aspiranteId);

    /// <summary>
    /// Obtiene la última versión del PDF de un empleado
    /// </summary>
    Task<HojaVidaPdf?> ObtenerUltimaVersionEmpleadoAsync(int empleadoId);

    /// <summary>
    /// Crea un nuevo registro de PDF
    /// </summary>
    Task<int> CrearAsync(HojaVidaPdf hojaVidaPdf);

    /// <summary>
    /// Actualiza un registro de PDF existente
    /// </summary>
    Task<bool> ActualizarAsync(HojaVidaPdf hojaVidaPdf);

    /// <summary>
    /// Marca un PDF como inválido
    /// </summary>
    Task<bool> MarcarInvalidoAsync(int id, string errores);

    /// <summary>
    /// Verifica si existe un PDF con el mismo hash
    /// </summary>
    Task<bool> ExisteHashAsync(string hashContenido);

    /// <summary>
    /// Obtiene el siguiente número de versión para un aspirante
    /// </summary>
    Task<int> ObtenerSiguienteVersionAsync(int? aspiranteId, int? empleadoId);

    /// <summary>
    /// Elimina (desactiva) un registro de PDF
    /// </summary>
    Task<bool> EliminarAsync(int id);

    /// <summary>
    /// Busca en el índice FTS5 de hojas de vida
    /// </summary>
    Task<List<HojaVidaPdf>> BuscarFtsAsync(string termino);

    /// <summary>
    /// Actualiza el índice FTS5 para un aspirante
    /// </summary>
    Task<bool> ActualizarIndiceFtsAsync(int aspiranteId);

    /// <summary>
    /// Obtiene PDFs por origen (Forestech, Externo, Manual)
    /// </summary>
    Task<List<HojaVidaPdf>> ObtenerPorOrigenAsync(OrigenHojaVida origen);
}
