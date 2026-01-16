using SGRRHH.Local.Domain.Enums;

namespace SGRRHH.Local.Domain.Entities;

/// <summary>
/// Cuenta bancaria de un empleado
/// Un empleado puede tener múltiples cuentas (personal, nómina, ahorros, etc.)
/// </summary>
public class CuentaBancaria : EntidadBase
{
    /// <summary>
    /// ID del empleado dueño de la cuenta
    /// </summary>
    public int EmpleadoId { get; set; }
    
    /// <summary>
    /// Referencia al empleado
    /// </summary>
    public Empleado? Empleado { get; set; }
    
    /// <summary>
    /// Nombre del banco (Bancolombia, Davivienda, etc.)
    /// </summary>
    public string Banco { get; set; } = string.Empty;
    
    /// <summary>
    /// Tipo de cuenta (Ahorros, Corriente)
    /// </summary>
    public TipoCuentaBancaria TipoCuenta { get; set; }
    
    /// <summary>
    /// Número de cuenta
    /// </summary>
    public string NumeroCuenta { get; set; } = string.Empty;
    
    /// <summary>
    /// Nombre del titular de la cuenta (puede diferir del empleado)
    /// </summary>
    public string? NombreTitular { get; set; }
    
    /// <summary>
    /// Documento de identidad del titular
    /// </summary>
    public string? DocumentoTitular { get; set; }
    
    /// <summary>
    /// Indica si es la cuenta de nómina principal
    /// </summary>
    public bool EsCuentaNomina { get; set; }
    
    /// <summary>
    /// Indica si la cuenta está activa
    /// </summary>
    public bool EstaActiva { get; set; } = true;
    
    /// <summary>
    /// Fecha desde la cual se usa esta cuenta
    /// </summary>
    public DateTime? FechaApertura { get; set; }
    
    /// <summary>
    /// ID del documento de certificación bancaria (si existe)
    /// </summary>
    public int? DocumentoCertificacionId { get; set; }
    
    /// <summary>
    /// Referencia al documento de certificación bancaria
    /// </summary>
    public DocumentoEmpleado? DocumentoCertificacion { get; set; }
    
    /// <summary>
    /// Observaciones adicionales
    /// </summary>
    public string? Observaciones { get; set; }
}
