using System.Collections.Concurrent;
using Google.Cloud.Firestore;
using Microsoft.Extensions.Logging;
using SGRRHH.Core.Interfaces;

namespace SGRRHH.Infrastructure.Firebase;

/// <summary>
/// Implementación del servicio de listeners en tiempo real para Firestore.
/// Escucha cambios en colecciones y notifica a los suscriptores automáticamente.
/// Los callbacks se ejecutan en el contexto de sincronización original (UI thread si aplica).
/// </summary>
public class FirestoreListenerService : IFirestoreListenerService
{
    private readonly FirestoreDb _firestore;
    private readonly ILogger<FirestoreListenerService>? _logger;
    private readonly ConcurrentDictionary<string, FirestoreChangeListener> _listeners = new();
    private readonly SynchronizationContext? _syncContext;
    private bool _disposed;

    public FirestoreListenerService(FirebaseInitializer firebase, ILogger<FirestoreListenerService>? logger = null)
    {
        _firestore = firebase.Firestore ?? throw new InvalidOperationException("FirestoreDb no inicializado");
        _logger = logger;

        // Capturar el SynchronizationContext actual (UI thread si se instancia desde allí)
        _syncContext = SynchronizationContext.Current;

        _logger?.LogInformation("FirestoreListenerService inicializado");
    }

    public int ActiveSubscriptionsCount => _listeners.Count;

    public string Subscribe<T>(
        string collectionName,
        Action<IEnumerable<T>> onSnapshot,
        Func<DocumentSnapshot, T> documentToEntity,
        Action<Exception>? onError = null,
        Func<Query, Query>? queryBuilder = null)
        where T : class
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(FirestoreListenerService));

        var subscriptionId = Guid.NewGuid().ToString();

        try
        {
            // Crear query base
            Query query = _firestore.Collection(collectionName);

            // Aplicar filtros adicionales si existen
            if (queryBuilder != null)
            {
                query = queryBuilder(query);
            }

            // Crear el listener
            var listener = query.Listen(snapshot =>
            {
                try
                {
                    // Convertir documentos a entidades
                    var entities = snapshot.Documents
                        .Select(doc =>
                        {
                            try
                            {
                                return documentToEntity(doc);
                            }
                            catch (Exception ex)
                            {
                                _logger?.LogError(ex, "Error al convertir documento {DocId} a entidad", doc.Id);
                                return null;
                            }
                        })
                        .Where(e => e != null)
                        .Cast<T>()
                        .ToList();

                    _logger?.LogDebug(
                        "Listener {SubscriptionId}: Recibidos {Count} documentos de {Collection}",
                        subscriptionId, entities.Count, collectionName);

                    // Ejecutar callback en el UI thread
                    InvokeOnUIThread(() => onSnapshot(entities));
                }
                catch (Exception ex)
                {
                    _logger?.LogError(ex, "Error procesando snapshot de {Collection}", collectionName);
                    InvokeOnUIThread(() => onError?.Invoke(ex));
                }
            });

            // Guardar el listener
            _listeners[subscriptionId] = listener;

            _logger?.LogInformation(
                "Suscripción {SubscriptionId} creada para colección {Collection}. Total activas: {Count}",
                subscriptionId, collectionName, _listeners.Count);

            return subscriptionId;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error al crear suscripción para {Collection}", collectionName);
            throw;
        }
    }

    public void Unsubscribe(string subscriptionId)
    {
        if (string.IsNullOrEmpty(subscriptionId))
            return;

        if (_listeners.TryRemove(subscriptionId, out var listener))
        {
            try
            {
                listener.StopAsync().GetAwaiter().GetResult();
                _logger?.LogInformation(
                    "Suscripción {SubscriptionId} cancelada. Total activas: {Count}",
                    subscriptionId, _listeners.Count);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error al detener listener {SubscriptionId}", subscriptionId);
            }
        }
    }

    public void UnsubscribeAll()
    {
        var count = _listeners.Count;

        foreach (var kvp in _listeners.ToArray())
        {
            try
            {
                kvp.Value.StopAsync().GetAwaiter().GetResult();
                _listeners.TryRemove(kvp.Key, out _);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error al detener listener {SubscriptionId}", kvp.Key);
            }
        }

        _logger?.LogInformation("Todas las suscripciones canceladas ({Count} total)", count);
    }

    /// <summary>
    /// Ejecuta una acción en el contexto de sincronización original (UI thread si aplica).
    /// </summary>
    private void InvokeOnUIThread(Action action)
    {
        if (_syncContext == null)
        {
            // No hay contexto de sincronización, ejecutar directamente
            action();
        }
        else
        {
            // Ejecutar en el contexto de sincronización original
            _syncContext.Post(_ => action(), null);
        }
    }

    public void Dispose()
    {
        if (_disposed)
            return;

        _disposed = true;
        UnsubscribeAll();

        _logger?.LogInformation("FirestoreListenerService disposed");
    }
}
