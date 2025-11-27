using SGRRHH.Core.Common;
using SGRRHH.Core.Entities;
using SGRRHH.Core.Interfaces;
using SGRRHH.Core.Models;

namespace SGRRHH.Infrastructure.Services;

/// <summary>
/// Implementación del servicio de configuración del sistema
/// </summary>
public class ConfiguracionService : IConfiguracionService
{
    private readonly IConfiguracionRepository _configuracionRepository;
    
    // Claves de configuración para datos de empresa
    private const string KEY_EMPRESA_NOMBRE = "Empresa.Nombre";
    private const string KEY_EMPRESA_NIT = "Empresa.Nit";
    private const string KEY_EMPRESA_DIRECCION = "Empresa.Direccion";
    private const string KEY_EMPRESA_CIUDAD = "Empresa.Ciudad";
    private const string KEY_EMPRESA_TELEFONO = "Empresa.Telefono";
    private const string KEY_EMPRESA_CORREO = "Empresa.Correo";
    private const string KEY_EMPRESA_REPRESENTANTE_NOMBRE = "Empresa.RepresentanteNombre";
    private const string KEY_EMPRESA_REPRESENTANTE_CARGO = "Empresa.RepresentanteCargo";
    private const string KEY_EMPRESA_LOGO_PATH = "Empresa.LogoPath";
    
    public ConfiguracionService(IConfiguracionRepository configuracionRepository)
    {
        _configuracionRepository = configuracionRepository;
    }
    
    public async Task<CompanyInfo> GetCompanyInfoAsync()
    {
        var companyInfo = new CompanyInfo();
        
        var nombre = await GetConfiguracionAsync(KEY_EMPRESA_NOMBRE);
        if (!string.IsNullOrWhiteSpace(nombre)) companyInfo.Nombre = nombre;
        
        var nit = await GetConfiguracionAsync(KEY_EMPRESA_NIT);
        if (!string.IsNullOrWhiteSpace(nit)) companyInfo.Nit = nit;
        
        var direccion = await GetConfiguracionAsync(KEY_EMPRESA_DIRECCION);
        if (!string.IsNullOrWhiteSpace(direccion)) companyInfo.Direccion = direccion;
        
        var ciudad = await GetConfiguracionAsync(KEY_EMPRESA_CIUDAD);
        if (!string.IsNullOrWhiteSpace(ciudad)) companyInfo.Ciudad = ciudad;
        
        var telefono = await GetConfiguracionAsync(KEY_EMPRESA_TELEFONO);
        if (!string.IsNullOrWhiteSpace(telefono)) companyInfo.Telefono = telefono;
        
        var correo = await GetConfiguracionAsync(KEY_EMPRESA_CORREO);
        if (!string.IsNullOrWhiteSpace(correo)) companyInfo.Correo = correo;
        
        var representanteNombre = await GetConfiguracionAsync(KEY_EMPRESA_REPRESENTANTE_NOMBRE);
        if (!string.IsNullOrWhiteSpace(representanteNombre)) companyInfo.RepresentanteNombre = representanteNombre;
        
        var representanteCargo = await GetConfiguracionAsync(KEY_EMPRESA_REPRESENTANTE_CARGO);
        if (!string.IsNullOrWhiteSpace(representanteCargo)) companyInfo.RepresentanteCargo = representanteCargo;
        
        var logoPath = await GetConfiguracionAsync(KEY_EMPRESA_LOGO_PATH);
        if (!string.IsNullOrWhiteSpace(logoPath)) companyInfo.LogoPath = logoPath;
        
        return companyInfo;
    }
    
    public async Task<ServiceResult> SaveCompanyInfoAsync(CompanyInfo companyInfo)
    {
        try
        {
            await SetConfiguracionAsync(KEY_EMPRESA_NOMBRE, companyInfo.Nombre, "Nombre de la empresa", "Empresa");
            await SetConfiguracionAsync(KEY_EMPRESA_NIT, companyInfo.Nit, "NIT de la empresa", "Empresa");
            await SetConfiguracionAsync(KEY_EMPRESA_DIRECCION, companyInfo.Direccion, "Dirección de la empresa", "Empresa");
            await SetConfiguracionAsync(KEY_EMPRESA_CIUDAD, companyInfo.Ciudad, "Ciudad de la empresa", "Empresa");
            await SetConfiguracionAsync(KEY_EMPRESA_TELEFONO, companyInfo.Telefono, "Teléfono de la empresa", "Empresa");
            await SetConfiguracionAsync(KEY_EMPRESA_CORREO, companyInfo.Correo, "Correo electrónico de la empresa", "Empresa");
            await SetConfiguracionAsync(KEY_EMPRESA_REPRESENTANTE_NOMBRE, companyInfo.RepresentanteNombre, "Nombre del representante legal", "Empresa");
            await SetConfiguracionAsync(KEY_EMPRESA_REPRESENTANTE_CARGO, companyInfo.RepresentanteCargo, "Cargo del representante legal", "Empresa");
            
            if (!string.IsNullOrWhiteSpace(companyInfo.LogoPath))
            {
                await SetConfiguracionAsync(KEY_EMPRESA_LOGO_PATH, companyInfo.LogoPath, "Ruta del logo de la empresa", "Empresa");
            }
            
            return ServiceResult.Ok("Configuración de empresa guardada exitosamente");
        }
        catch (Exception ex)
        {
            return ServiceResult.Fail($"Error al guardar configuración: {ex.Message}");
        }
    }
    
    public async Task<string?> GetConfiguracionAsync(string clave)
    {
        var config = await _configuracionRepository.GetByClaveAsync(clave);
        return config?.Valor;
    }
    
    public async Task<ServiceResult> SetConfiguracionAsync(string clave, string valor, string? descripcion = null, string categoria = "General")
    {
        try
        {
            var existente = await _configuracionRepository.GetByClaveAsync(clave);
            
            if (existente != null)
            {
                existente.Valor = valor;
                existente.FechaModificacion = DateTime.Now;
                if (descripcion != null) existente.Descripcion = descripcion;
                await _configuracionRepository.UpdateAsync(existente);
            }
            else
            {
                var nuevo = new ConfiguracionSistema
                {
                    Clave = clave,
                    Valor = valor,
                    Descripcion = descripcion,
                    Categoria = categoria,
                    FechaCreacion = DateTime.Now
                };
                await _configuracionRepository.AddAsync(nuevo);
            }
            
            await _configuracionRepository.SaveChangesAsync();
            return ServiceResult.Ok();
        }
        catch (Exception ex)
        {
            return ServiceResult.Fail($"Error al guardar configuración: {ex.Message}");
        }
    }
    
    public async Task<Dictionary<string, string>> GetConfiguracionesPorCategoriaAsync(string categoria)
    {
        var configs = await _configuracionRepository.GetByCategoriaAsync(categoria);
        return configs.ToDictionary(c => c.Clave, c => c.Valor);
    }
    
    public async Task<ServiceResult<string>> CopiarLogoAsync(string rutaOrigen)
    {
        try
        {
            if (!File.Exists(rutaOrigen))
            {
                return ServiceResult<string>.Fail("El archivo de origen no existe");
            }
            
            // Crear directorio de configuración si no existe
            var configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data", "config");
            if (!Directory.Exists(configPath))
            {
                Directory.CreateDirectory(configPath);
            }
            
            // Copiar archivo con extensión original
            var extension = Path.GetExtension(rutaOrigen);
            var destino = Path.Combine(configPath, $"logo{extension}");
            
            File.Copy(rutaOrigen, destino, true);
            
            // Guardar la ruta en configuración
            await SetConfiguracionAsync(KEY_EMPRESA_LOGO_PATH, destino, "Ruta del logo de la empresa", "Empresa");
            
            return ServiceResult<string>.Ok(destino, "Logo copiado exitosamente");
        }
        catch (Exception ex)
        {
            return ServiceResult<string>.Fail($"Error al copiar logo: {ex.Message}");
        }
    }
}
