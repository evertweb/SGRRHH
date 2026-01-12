namespace SGRRHH.Local.Domain.Entities;

/// <summary>
/// Dispositivo autorizado para login automático sin contraseña.
/// Similar al funcionamiento de SSH con llaves públicas/privadas.
/// </summary>
public class DispositivoAutorizado : EntidadBase
{
    /// <summary>
    /// ID del usuario propietario del dispositivo autorizado.
    /// </summary>
    public int UsuarioId { get; set; }
    
    /// <summary>
    /// Token único del dispositivo (UUID + hash).
    /// Se almacena en localStorage del navegador.
    /// </summary>
    public string DeviceToken { get; set; } = string.Empty;
    
    /// <summary>
    /// Nombre amigable del dispositivo (ej: "PC Oficina", "Laptop Campo").
    /// </summary>
    public string NombreDispositivo { get; set; } = string.Empty;
    
    /// <summary>
    /// User-Agent del navegador para identificación adicional.
    /// </summary>
    public string? HuellaNavegador { get; set; }
    
    /// <summary>
    /// IP desde donde se autorizó el dispositivo.
    /// </summary>
    public string? IpAutorizacion { get; set; }
    
    /// <summary>
    /// Fecha del último uso exitoso del token.
    /// </summary>
    public DateTime? FechaUltimoUso { get; set; }
    
    /// <summary>
    /// Fecha de expiración del token (opcional).
    /// Si es null, el token no expira.
    /// </summary>
    public DateTime? FechaExpiracion { get; set; }
    
    // Navegación
    public Usuario? Usuario { get; set; }
}
