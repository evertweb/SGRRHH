using SGRRHH.Local.Domain.Enums;

namespace SGRRHH.Local.Domain.Entities;

public class Usuario : EntidadBase
{
    public string Username { get; set; } = string.Empty;
    
    public string PasswordHash { get; set; } = string.Empty;
    
    public string NombreCompleto { get; set; } = string.Empty;
    
    public string? Email { get; set; }
    
    public string? PhoneNumber { get; set; }
    
    public RolUsuario Rol { get; set; }
    
    public DateTime? UltimoAcceso { get; set; }
    
    public int? EmpleadoId { get; set; }
}


