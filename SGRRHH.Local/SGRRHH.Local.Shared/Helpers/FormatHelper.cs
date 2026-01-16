using SGRRHH.Local.Domain.Enums;

namespace SGRRHH.Local.Shared.Helpers;

/// <summary>
/// Helper para formateo de datos diversos
/// </summary>
public static class FormatHelper
{
    /// <summary>
    /// Formatea tamaño de archivo en bytes a formato legible (KB, MB, GB)
    /// Extraído de EmpleadoExpediente.razor líneas 930-941
    /// </summary>
    public static string FormatFileSize(long bytes)
    {
        string[] sizes = { "B", "KB", "MB", "GB" };
        double len = bytes;
        int order = 0;
        while (len >= 1024 && order < sizes.Length - 1)
        {
            order++;
            len = len / 1024;
        }
        return $"{len:0.##} {sizes[order]}";
    }
    
    /// <summary>
    /// Obtiene nombre legible de tipo de contrato
    /// Extraído de ContratosTab.razor.cs líneas 25-38
    /// </summary>
    public static string GetTipoContratoDisplay(TipoContrato tipo)
    {
        return tipo switch
        {
            TipoContrato.Indefinido => "Indefinido",
            TipoContrato.TerminoFijo => "Término Fijo",
            TipoContrato.ObraOLabor => "Obra o Labor",
            TipoContrato.PrestacionServicios => "Prestación Servicios",
            TipoContrato.Aprendizaje => "Aprendizaje",
            TipoContrato.Ocasional => "Ocasional",
            TipoContrato.Temporal => "Temporal",
            _ => tipo.ToString()
        };
    }
}
