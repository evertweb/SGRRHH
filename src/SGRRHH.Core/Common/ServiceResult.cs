namespace SGRRHH.Core.Common;

/// <summary>
/// Versión no genérica de ServiceResult para operaciones sin datos de retorno
/// </summary>
public class ServiceResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public List<string> Errors { get; set; } = new();

    // Métodos estilo Ok/Fail
    public static ServiceResult Ok(string message = "Operación exitosa")
        => new() { Success = true, Message = message };

    public static ServiceResult Fail(string message)
        => new() { Success = false, Message = message };

    public static ServiceResult Fail(List<string> errors)
        => new() { Success = false, Errors = errors, Message = string.Join(", ", errors) };

    // Métodos estilo SuccessResult/FailureResult (alias para compatibilidad)
    public static ServiceResult SuccessResult(string? message = null)
        => Ok(message ?? "Operación exitosa");

    public static ServiceResult FailureResult(string error)
        => Fail(error);

    public static ServiceResult FailureResult(List<string> errors)
        => Fail(errors);
}

/// <summary>
/// Resultado genérico de una operación de servicio.
/// Permite encapsular éxito/fracaso, datos y mensajes de error.
/// </summary>
public class ServiceResult<T> : ServiceResult
{
    public T? Data { get; set; }

    // Métodos estilo Ok/Fail
    public static ServiceResult<T> Ok(T data, string message = "Operación exitosa")
        => new() { Success = true, Data = data, Message = message };

    public new static ServiceResult<T> Fail(string message)
        => new() { Success = false, Message = message };

    public new static ServiceResult<T> Fail(List<string> errors)
        => new() { Success = false, Errors = errors, Message = string.Join(", ", errors) };

    // Métodos estilo SuccessResult/FailureResult (alias para compatibilidad)
    public static ServiceResult<T> SuccessResult(T data, string? message = null)
        => Ok(data, message ?? "Operación exitosa");

    public new static ServiceResult<T> FailureResult(string error)
        => Fail(error);

    public new static ServiceResult<T> FailureResult(List<string> errors)
        => Fail(errors);
}
