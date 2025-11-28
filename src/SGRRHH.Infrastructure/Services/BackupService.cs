using SGRRHH.Core.Common;
using SGRRHH.Core.Interfaces;

namespace SGRRHH.Infrastructure.Services;

/// <summary>
/// Implementación del servicio de backup para Firebase.
/// Los backups en Firebase se manejan desde Firebase Console.
/// </summary>
public class BackupService : IBackupService
{
    public BackupService()
    {
    }
    
    public Task<ServiceResult<string>> CrearBackupAsync(string? rutaDestino = null)
    {
        return Task.FromResult(ServiceResult<string>.Fail(
            "Los backups de Firebase se gestionan desde Firebase Console.\n\n" +
            "Para crear un backup:\n" +
            "1. Ir a Firebase Console > Firestore Database\n" +
            "2. Hacer clic en los tres puntos (...) > Export Data\n" +
            "3. Seleccionar un bucket de Cloud Storage como destino"));
    }
    
    public Task<ServiceResult> RestaurarBackupAsync(string rutaBackup)
    {
        return Task.FromResult(ServiceResult.Fail(
            "Los backups de Firebase se restauran desde Firebase Console.\n\n" +
            "Para restaurar un backup:\n" +
            "1. Ir a Firebase Console > Firestore Database\n" +
            "2. Hacer clic en los tres puntos (...) > Import Data\n" +
            "3. Seleccionar el backup del bucket de Cloud Storage"));
    }
    
    public Task<List<BackupInfo>> ListarBackupsAsync()
    {
        // Retornar lista vacía - los backups se gestionan desde Firebase Console
        return Task.FromResult(new List<BackupInfo>());
    }
    
    public Task<ServiceResult> EliminarBackupAsync(string rutaBackup)
    {
        return Task.FromResult(ServiceResult.Fail(
            "Los backups de Firebase se eliminan desde Google Cloud Console."));
    }
    
    public string GetRutaBackups()
    {
        return "Firebase Console > Firestore Database > Export/Import";
    }
    
    public Task<ServiceResult> ValidarBackupAsync(string rutaBackup)
    {
        return Task.FromResult(ServiceResult.Fail(
            "La validación de backups de Firebase se realiza desde Firebase Console."));
    }
}
