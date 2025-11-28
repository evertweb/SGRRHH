namespace SGRRHH.Core.Interfaces;

/// <summary>
/// Unit of Work para manejar transacciones
/// </summary>
public interface IUnitOfWork : IDisposable
{
    /// <summary>
    /// Inicia una transacción
    /// </summary>
    Task BeginTransactionAsync();
    
    /// <summary>
    /// Confirma la transacción
    /// </summary>
    Task CommitAsync();
    
    /// <summary>
    /// Revierte la transacción
    /// </summary>
    Task RollbackAsync();
    
    /// <summary>
    /// Guarda los cambios
    /// </summary>
    Task<int> SaveChangesAsync();
}
