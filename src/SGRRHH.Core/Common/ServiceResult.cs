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
}
