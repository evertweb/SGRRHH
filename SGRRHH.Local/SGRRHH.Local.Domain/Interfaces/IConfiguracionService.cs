namespace SGRRHH.Local.Domain.Interfaces;

public interface IConfiguracionService
{
    /// <summary>
    /// Obtiene un valor de configuración como string
    /// </summary>
    Task<string?> GetAsync(string clave);
    
    /// <summary>
    /// Obtiene un valor de configuración con tipo específico
    /// </summary>
    Task<T?> GetAsync<T>(string clave);
    
    /// <summary>
    /// Guarda un valor de configuración como string
    /// </summary>
    Task SetAsync(string clave, string valor);
    
    /// <summary>
    /// Guarda un valor de configuración con tipo específico
    /// </summary>
    Task SetAsync<T>(string clave, T valor);
    
    /// <summary>
    /// Obtiene todas las configuraciones
    /// </summary>
    Task<Dictionary<string, string>> GetAllAsync();
}

/// <summary>
/// Claves de configuración predefinidas
/// </summary>
public static class ConfigKeys
{
    // Empresa
    public const string EmpresaNombre = "empresa.nombre";
    public const string EmpresaNit = "empresa.nit";
    public const string EmpresaDireccion = "empresa.direccion";
    public const string EmpresaTelefono = "empresa.telefono";
    public const string EmpresaEmail = "empresa.email";
    public const string EmpresaRepresentante = "empresa.representante";
    public const string EmpresaLogo = "empresa.logo";
    
    // Sistema
    public const string SistemaVacacionesDiasAnuales = "sistema.vacaciones.dias_anuales";
    public const string SistemaBackupAutomatico = "sistema.backup.automatico";
    public const string SistemaBackupHora = "sistema.backup.hora";
    public const string SistemaBackupRetenerDias = "sistema.backup.retener_dias";
    
    // Notificaciones
    public const string NotificarContratosVencer = "notificar.contratos.dias_antes";
}
