using Google.Cloud.Firestore;

namespace SGRRHH.Core.Interfaces;

/// <summary>
/// Servicio para escuchar cambios en tiempo real de Firestore.
/// Permite sincronización automática entre usuarios sin refrescar manualmente.
/// </summary>
public interface IFirestoreListenerService : IDisposable
{
    /// <summary>
    /// Suscribe a cambios en una colección con query opcional.
    /// Los callbacks se ejecutan en el UI thread automáticamente.
    /// </summary>
    /// <typeparam name="T">Tipo de entidad a escuchar</typeparam>
    /// <param name="collectionName">Nombre de la colección en Firestore</param>
    /// <param name="onSnapshot">Callback cuando hay cambios. Recibe la lista completa actualizada.</param>
    /// <param name="documentToEntity">Función para convertir DocumentSnapshot a entidad T</param>
    /// <param name="onError">Callback opcional para errores</param>
    /// <param name="queryBuilder">Builder opcional para filtrar la query</param>
    /// <returns>ID de suscripción para cancelar después</returns>
    string Subscribe<T>(
        string collectionName,
        Action<IEnumerable<T>> onSnapshot,
        Func<DocumentSnapshot, T> documentToEntity,
        Action<Exception>? onError = null,
        Func<Query, Query>? queryBuilder = null)
        where T : class;

    /// <summary>
    /// Cancela una suscripción específica
    /// </summary>
    /// <param name="subscriptionId">ID retornado por Subscribe</param>
    void Unsubscribe(string subscriptionId);

    /// <summary>
    /// Cancela todas las suscripciones activas
    /// </summary>
    void UnsubscribeAll();

    /// <summary>
    /// Obtiene el número de suscripciones activas
    /// </summary>
    int ActiveSubscriptionsCount { get; }
}
