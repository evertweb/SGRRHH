namespace SGRRHH.Local.Domain.Enums;

/// <summary>
/// Define cómo se resuelve el permiso en términos de pago
/// </summary>
public enum TipoResolucionPermiso
{
    /// <summary>Aún no se ha decidido</summary>
    PendienteDefinir = 0,
    
    /// <summary>Se paga completo, no se descuenta (con documento médico, legal, etc.)</summary>
    Remunerado = 1,
    
    /// <summary>Se descuenta de la nómina</summary>
    Descontado = 2,
    
    /// <summary>Se compensa trabajando horas extra otro día</summary>
    Compensado = 3
}
