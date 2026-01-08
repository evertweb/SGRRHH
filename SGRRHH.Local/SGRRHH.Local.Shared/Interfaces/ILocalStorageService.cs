using SGRRHH.Local.Shared;

namespace SGRRHH.Local.Shared.Interfaces;

public interface ILocalStorageService
{
    // ===== Fotos de Empleados =====
    Task<Result<string>> SaveEmpleadoFotoAsync(int empleadoId, byte[] imageBytes, string extension);
    Task<Result<string>> SaveEmpleadoFotoAsync(int empleadoId, Stream imageStream, string extension);
    Task<Result<byte[]>> GetEmpleadoFotoAsync(int empleadoId);
    Task<Result<string>> GetEmpleadoFotoPathAsync(int empleadoId);
    Task<Result> DeleteEmpleadoFotoAsync(int empleadoId);
    bool EmpleadoFotoExists(int empleadoId);

    // ===== Documentos de Permisos =====
    Task<Result<string>> SavePermisoDocumentoAsync(int permisoId, byte[] fileBytes, string fileName);
    Task<Result<byte[]>> GetPermisoDocumentoAsync(int permisoId);
    Task<Result> DeletePermisoDocumentoAsync(int permisoId);

    // ===== Documentos de Contratos =====
    Task<Result<string>> SaveContratoArchivoAsync(int contratoId, byte[] fileBytes, string fileName);
    Task<Result<byte[]>> GetContratoArchivoAsync(int contratoId);
    Task<Result> DeleteContratoArchivoAsync(int contratoId);
    
    // ===== Archivos genéricos (para formularios) =====
    Task<string> SaveFileAsync(Stream fileStream, string fileName, params string[] pathParts);
    Task<byte[]?> GetFileAsync(string relativePath);

    // ===== Documentos de Empleados (expediente) =====
    Task<Result<string>> SaveDocumentoEmpleadoAsync(int empleadoId, string tipoDocumento, byte[] fileBytes, string fileName);
    Task<Result<byte[]>> GetDocumentoEmpleadoAsync(int documentoId);
    Task<Result<IEnumerable<string>>> GetDocumentosEmpleadoAsync(int empleadoId);
    Task<Result> DeleteDocumentoEmpleadoAsync(int documentoId);

    // ===== Documentos Generados (PDFs) =====
    Task<Result<string>> SaveGeneratedPdfAsync(string tipo, string fileName, byte[] pdfBytes);
    Task<Result<byte[]>> GetGeneratedPdfAsync(string tipo, string fileName);
    string GetGeneratedPdfPath(string tipo, string fileName);

    // ===== Logo de Empresa =====
    Task<Result<string>> SaveLogoEmpresaAsync(byte[] imageBytes, string extension);
    Task<Result<byte[]>> GetLogoEmpresaAsync();
    string? GetLogoEmpresaPath();

    // ===== Utilidades =====
    Task<long> GetTotalStorageSizeAsync();
    Task<Result> CleanupOrphanFilesAsync();
    void EnsureDirectoriesExist();
}