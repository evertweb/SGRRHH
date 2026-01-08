using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SGRRHH.Local.Shared.Configuration;
using SGRRHH.Local.Shared.Interfaces;

namespace SGRRHH.Local.Infrastructure.Services;

public class BackupSchedulerService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<BackupSchedulerService> _logger;
    private DateTime _lastBackupDate = DateTime.MinValue;
    
    public BackupSchedulerService(
        IServiceScopeFactory scopeFactory,
        ILogger<BackupSchedulerService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Servicio de backup automático iniciado");
        
        // Esperar un minuto antes de empezar a verificar
        await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
        
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await CheckAndExecuteBackupAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en el servicio de backup automático");
            }
            
            // Verificar cada minuto
            await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
        }
        
        _logger.LogInformation("Servicio de backup automático detenido");
    }
    
    private async Task CheckAndExecuteBackupAsync(CancellationToken stoppingToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var configService = scope.ServiceProvider.GetRequiredService<IConfiguracionService>();
        var backupService = scope.ServiceProvider.GetRequiredService<IBackupService>();
        
        // Verificar si el backup automático está activado
        var autoBackup = await configService.GetAsync<bool>(ConfigKeys.SistemaBackupAutomatico);
        if (!autoBackup)
        {
            return;
        }
        
        // Obtener hora programada
        var backupHora = await configService.GetAsync(ConfigKeys.SistemaBackupHora) ?? "23:00";
        var horaActual = DateTime.Now.ToString("HH:mm");
        
        // Verificar si es la hora del backup y no se ha ejecutado hoy
        if (horaActual == backupHora && _lastBackupDate.Date != DateTime.Today)
        {
            _logger.LogInformation("Ejecutando backup automático programado");
            
            // Crear backup
            var backupResult = await backupService.CreateScheduledBackupAsync();
            
            if (backupResult.IsSuccess)
            {
                _logger.LogInformation("Backup automático completado exitosamente");
                _lastBackupDate = DateTime.Now;
                
                // Limpiar backups antiguos
                var retenerDias = await configService.GetAsync<int>(ConfigKeys.SistemaBackupRetenerDias);
                if (retenerDias <= 0) retenerDias = 30;
                
                var cleanupResult = await backupService.CleanupOldBackupsAsync(retenerDias);
                if (cleanupResult.IsSuccess)
                {
                    _logger.LogInformation("Limpieza de backups completada: {Count} eliminados", 
                        cleanupResult.Value);
                }
                
                // Esperar un minuto para no ejecutar dos veces
                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
            }
            else
            {
                _logger.LogError("Error en backup automático: {Error}", backupResult.Error);
            }
        }
    }
}
