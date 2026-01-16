namespace SGRRHH.Local.Domain.Enums;

/// <summary>
/// Tipos de cuenta bancaria en Colombia
/// </summary>
public enum TipoCuentaBancaria
{
    /// <summary>
    /// Cuenta de ahorros
    /// </summary>
    Ahorros = 1,
    
    /// <summary>
    /// Cuenta corriente
    /// </summary>
    Corriente = 2,
    
    /// <summary>
    /// Depósito de bajo monto - Para Nequi y otras billeteras digitales con límites mensuales
    /// </summary>
    DepositoBajoMonto = 3
}
