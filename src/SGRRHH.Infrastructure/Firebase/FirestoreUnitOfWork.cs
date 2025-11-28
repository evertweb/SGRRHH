using Google.Cloud.Firestore;
using Microsoft.Extensions.Logging;
using SGRRHH.Core.Interfaces;

namespace SGRRHH.Infrastructure.Firebase;

/// <summary>
/// Implementación de Unit of Work para Firestore.
/// Utiliza WriteBatch para operaciones atómicas.
/// </summary>
/// <remarks>
/// Nota: Firestore tiene un modelo de consistencia diferente a SQL.
/// Las "transacciones" aquí son WriteBatch, que permiten operaciones atómicas
/// pero sin aislamiento de lectura como en SQL.
/// 
/// Para transacciones completas con aislamiento de lectura, usar
/// RunTransactionAsync directamente.
/// </remarks>
public class FirestoreUnitOfWork : IUnitOfWork
{
    private readonly FirebaseInitializer _firebase;
    private readonly ILogger<FirestoreUnitOfWork>? _logger;
    private WriteBatch? _currentBatch;
    private bool _disposed;
    private int _operationsCount;
    
    /// <summary>
    /// Número máximo de operaciones por batch (límite de Firestore es 500)
    /// </summary>
    private const int MaxBatchSize = 500;
    
    /// <summary>
    /// Acceso a la base de datos Firestore
    /// </summary>
    private FirestoreDb Firestore => _firebase.Firestore 
        ?? throw new InvalidOperationException("Firestore no está inicializado");

    public FirestoreUnitOfWork(FirebaseInitializer firebase, ILogger<FirestoreUnitOfWork>? logger = null)
    {
        _firebase = firebase ?? throw new ArgumentNullException(nameof(firebase));
        _logger = logger;
    }
    
    /// <summary>
    /// Inicia un nuevo batch de operaciones.
    /// En Firestore, esto crea un WriteBatch que se confirmará al llamar CommitAsync.
    /// </summary>
    public Task BeginTransactionAsync()
    {
        if (_currentBatch != null)
        {
            throw new InvalidOperationException("Ya existe una transacción activa. Llame CommitAsync o RollbackAsync primero.");
        }
        
        _currentBatch = Firestore.StartBatch();
        _operationsCount = 0;
        _logger?.LogDebug("Iniciando nueva transacción de Firestore (WriteBatch)");
        
        return Task.CompletedTask;
    }
    
    /// <summary>
    /// Confirma todas las operaciones del batch de forma atómica.
    /// </summary>
    public async Task CommitAsync()
    {
        if (_currentBatch == null)
        {
            throw new InvalidOperationException("No hay una transacción activa. Llame BeginTransactionAsync primero.");
        }
        
        try
        {
            _logger?.LogDebug("Confirmando transacción de Firestore con {Count} operaciones", _operationsCount);
            await _currentBatch.CommitAsync();
            _logger?.LogInformation("Transacción de Firestore confirmada exitosamente con {Count} operaciones", _operationsCount);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error al confirmar transacción de Firestore");
            throw;
        }
        finally
        {
            _currentBatch = null;
            _operationsCount = 0;
        }
    }
    
    /// <summary>
    /// Descarta el batch actual sin aplicar ninguna operación.
    /// </summary>
    public Task RollbackAsync()
    {
        if (_currentBatch == null)
        {
            _logger?.LogWarning("Se intentó hacer rollback sin una transacción activa");
            return Task.CompletedTask;
        }
        
        _logger?.LogInformation("Descartando transacción de Firestore con {Count} operaciones", _operationsCount);
        _currentBatch = null;
        _operationsCount = 0;
        
        return Task.CompletedTask;
    }
    
    /// <summary>
    /// En Firestore las operaciones son atómicas, este método no aplica.
    /// Se mantiene para compatibilidad con la interfaz IUnitOfWork.
    /// </summary>
    public Task<int> SaveChangesAsync()
    {
        // En Firestore las operaciones son atómicas, no se necesita "guardar"
        // Las operaciones se confirman con CommitAsync
        return Task.FromResult(_operationsCount);
    }
    
    #region Métodos auxiliares para operaciones en batch
    
    /// <summary>
    /// Obtiene el batch actual para agregar operaciones.
    /// </summary>
    public WriteBatch? CurrentBatch => _currentBatch;
    
    /// <summary>
    /// Indica si hay una transacción activa.
    /// </summary>
    public bool HasActiveTransaction => _currentBatch != null;
    
    /// <summary>
    /// Agrega una operación Set al batch actual.
    /// </summary>
    public void BatchSet(DocumentReference docRef, Dictionary<string, object?> data)
    {
        EnsureActiveBatch();
        _currentBatch!.Set(docRef, data);
        IncrementAndCheckBatchSize();
    }
    
    /// <summary>
    /// Agrega una operación Update al batch actual.
    /// </summary>
    public void BatchUpdate(DocumentReference docRef, Dictionary<string, object> updates)
    {
        EnsureActiveBatch();
        _currentBatch!.Update(docRef, updates);
        IncrementAndCheckBatchSize();
    }
    
    /// <summary>
    /// Agrega una operación Delete al batch actual.
    /// </summary>
    public void BatchDelete(DocumentReference docRef)
    {
        EnsureActiveBatch();
        _currentBatch!.Delete(docRef);
        IncrementAndCheckBatchSize();
    }
    
    /// <summary>
    /// Ejecuta una transacción de Firestore con aislamiento de lectura.
    /// Esto es diferente del WriteBatch - permite lecturas y escrituras atómicas.
    /// </summary>
    public async Task<TResult> RunTransactionAsync<TResult>(Func<Transaction, Task<TResult>> updateFunction)
    {
        return await Firestore.RunTransactionAsync(updateFunction);
    }
    
    /// <summary>
    /// Ejecuta una transacción de Firestore con aislamiento de lectura (sin valor de retorno).
    /// </summary>
    public async Task RunTransactionAsync(Func<Transaction, Task> updateFunction)
    {
        await Firestore.RunTransactionAsync(async transaction =>
        {
            await updateFunction(transaction);
            return 0; // Valor dummy requerido por la API de Firestore
        });
    }
    
    private void EnsureActiveBatch()
    {
        if (_currentBatch == null)
        {
            throw new InvalidOperationException("No hay una transacción activa. Llame BeginTransactionAsync primero.");
        }
    }
    
    private void IncrementAndCheckBatchSize()
    {
        _operationsCount++;
        if (_operationsCount >= MaxBatchSize)
        {
            _logger?.LogWarning("El batch ha alcanzado el límite de {MaxSize} operaciones. Considere confirmar y crear un nuevo batch.", MaxBatchSize);
        }
    }
    
    #endregion
    
    #region IDisposable
    
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
    
    protected virtual void Dispose(bool disposing)
    {
        if (_disposed) return;
        
        if (disposing)
        {
            if (_currentBatch != null)
            {
                _logger?.LogWarning("Se está disponiendo FirestoreUnitOfWork con una transacción activa no confirmada");
                _currentBatch = null;
            }
        }
        
        _disposed = true;
    }
    
    #endregion
}
