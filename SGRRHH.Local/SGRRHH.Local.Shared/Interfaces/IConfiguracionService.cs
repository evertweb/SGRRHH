using SGRRHH.Local.Shared;

namespace SGRRHH.Local.Shared.Interfaces;

public interface IConfiguracionService
{
    Task<string?> GetAsync(string clave);
    Task<T?> GetAsync<T>(string clave);
    Task<Result> SetAsync(string clave, string valor);
    Task<Result> SetAsync<T>(string clave, T valor);
    Task<Dictionary<string, string>> GetAllAsync();
    Task<Dictionary<string, string>> GetByCategoriaAsync(string categoria);
}
