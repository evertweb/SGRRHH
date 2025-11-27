using SGRRHH.Core.Common;
using SGRRHH.Core.Models;

namespace SGRRHH.Core.Interfaces;

/// <summary>
/// Interfaz para el servicio de configuración del sistema
/// </summary>
public interface IConfiguracionService
{
    /// <summary>
    /// Obtiene la información de la empresa
    /// </summary>
    Task<CompanyInfo> GetCompanyInfoAsync();
    
    /// <summary>
    /// Guarda la información de la empresa
    /// </summary>
    Task<ServiceResult> SaveCompanyInfoAsync(CompanyInfo companyInfo);
    
    /// <summary>
    /// Obtiene el valor de una configuración específica
    /// </summary>
    Task<string?> GetConfiguracionAsync(string clave);
    
    /// <summary>
    /// Guarda el valor de una configuración
    /// </summary>
    Task<ServiceResult> SetConfiguracionAsync(string clave, string valor, string? descripcion = null, string categoria = "General");
    
    /// <summary>
    /// Obtiene todas las configuraciones de una categoría
    /// </summary>
    Task<Dictionary<string, string>> GetConfiguracionesPorCategoriaAsync(string categoria);
    
    /// <summary>
    /// Copia el logo desde una ruta origen al directorio de configuración
    /// </summary>
    Task<ServiceResult<string>> CopiarLogoAsync(string rutaOrigen);
}
