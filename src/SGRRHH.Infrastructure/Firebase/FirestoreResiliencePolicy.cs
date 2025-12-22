using Grpc.Core;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Retry;

namespace SGRRHH.Infrastructure.Firebase;

/// <summary>
/// Políticas de resiliencia para operaciones de Firestore.
/// Implementa retry con backoff exponencial para errores transitorios.
/// </summary>
public static class FirestoreResiliencePolicy
{
    /// <summary>
    /// Crea una política de reintentos para operaciones de Firestore.
    /// Reintenta 3 veces con backoff exponencial (1s, 2s, 4s).
    /// </summary>
    public static ResiliencePipeline CreateRetryPipeline(ILogger? logger = null)
    {
        return new ResiliencePipelineBuilder()
            .AddRetry(new RetryStrategyOptions
            {
                ShouldHandle = new PredicateBuilder().Handle<RpcException>(ex =>
                    ex.StatusCode == StatusCode.Unavailable ||
                    ex.StatusCode == StatusCode.DeadlineExceeded ||
                    ex.StatusCode == StatusCode.ResourceExhausted ||
                    ex.StatusCode == StatusCode.Aborted),
                MaxRetryAttempts = 3,
                Delay = TimeSpan.FromSeconds(1),
                BackoffType = DelayBackoffType.Exponential,
                OnRetry = args =>
                {
                    logger?.LogWarning(
                        "Reintento {Attempt} después de {Delay}ms. Error: {Exception}",
                        args.AttemptNumber,
                        args.RetryDelay.TotalMilliseconds,
                        args.Outcome.Exception?.Message);
                    return default;
                }
            })
            .Build();
    }

    /// <summary>
    /// Crea una política de reintentos asíncrona para operaciones de Firestore.
    /// </summary>
    public static ResiliencePipeline<T> CreateRetryPipeline<T>(ILogger? logger = null)
    {
        return new ResiliencePipelineBuilder<T>()
            .AddRetry(new RetryStrategyOptions<T>
            {
                ShouldHandle = new PredicateBuilder<T>().Handle<RpcException>(ex =>
                    ex.StatusCode == StatusCode.Unavailable ||
                    ex.StatusCode == StatusCode.DeadlineExceeded ||
                    ex.StatusCode == StatusCode.ResourceExhausted ||
                    ex.StatusCode == StatusCode.Aborted),
                MaxRetryAttempts = 3,
                Delay = TimeSpan.FromSeconds(1),
                BackoffType = DelayBackoffType.Exponential,
                OnRetry = args =>
                {
                    logger?.LogWarning(
                        "Reintento {Attempt} después de {Delay}ms. Error: {Exception}",
                        args.AttemptNumber,
                        args.RetryDelay.TotalMilliseconds,
                        args.Outcome.Exception?.Message);
                    return default;
                }
            })
            .Build();
    }

    /// <summary>
    /// Extensión helper para ejecutar operaciones con resiliencia.
    /// </summary>
    public static async Task<T> ExecuteWithRetryAsync<T>(
        Func<CancellationToken, ValueTask<T>> operation,
        ILogger? logger = null,
        CancellationToken cancellationToken = default)
    {
        var pipeline = CreateRetryPipeline<T>(logger);
        return await pipeline.ExecuteAsync(operation, cancellationToken);
    }

    /// <summary>
    /// Extensión helper para ejecutar operaciones void con resiliencia.
    /// </summary>
    public static async Task ExecuteWithRetryAsync(
        Func<CancellationToken, ValueTask> operation,
        ILogger? logger = null,
        CancellationToken cancellationToken = default)
    {
        var pipeline = CreateRetryPipeline(logger);
        await pipeline.ExecuteAsync(operation, cancellationToken);
    }
}
