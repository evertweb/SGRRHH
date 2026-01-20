namespace SGRRHH.Local.Domain.Enums;

/// <summary>
/// Define cómo se resuelve el permiso en términos de pago
/// </summary>
public enum LeaveResolutionType
{
    /// <summary>Aún no se ha decidido</summary>
    PendingDefinition = 0,
    
    /// <summary>Se paga completo, no se descuenta (con documento médico, legal, etc.)</summary>
    Paid = 1,
    
    /// <summary>Se descuenta de la nómina</summary>
    Deducted = 2,
    
    /// <summary>Se compensa trabajando horas extra otro día</summary>
    Compensated = 3
}
