using System.Diagnostics;

namespace SGRRHH.Updater;

class Program
{
    // Archivos del propio Updater que NO deben copiarse mientras se ejecuta
    private static readonly string[] UpdaterFiles = new[]
    {
        "SGRRHH.Updater.exe",
        "SGRRHH.Updater.dll",
        "SGRRHH.Updater.pdb",
        "SGRRHH.Updater.deps.json",
        "SGRRHH.Updater.runtimeconfig.json"
    };

    static void Main(string[] args)
    {
        // Argumentos esperados:
        // 0: Directorio destino (donde está instalada la app)
        // 1: Directorio fuente (donde está la actualización extraída)
        // 2: Nombre del ejecutable a reiniciar (ej: SGRRHH.exe)
        // 3: PID del proceso a esperar (opcional)

        string logFile = "";
        
        try
        {
            if (args.Length < 3)
            {
                Console.WriteLine("Argumentos insuficientes.");
                Console.WriteLine("Uso: SGRRHH.Updater.exe <TargetDir> <SourceDir> <ExeName> [PID]");
                return;
            }

            string targetDir = args[0];
            string sourceDir = args[1];
            string exeName = args[2];
            int pid = args.Length > 3 && int.TryParse(args[3], out int p) ? p : 0;

            logFile = Path.Combine(targetDir, "updater_log.txt");
            
            // Limpiar log anterior
            if (File.Exists(logFile)) File.Delete(logFile);
            
            Log(logFile, "========================================");
            Log(logFile, "INICIANDO ACTUALIZADOR SGRRHH");
            Log(logFile, "========================================");
            Log(logFile, $"Target: {targetDir}");
            Log(logFile, $"Source: {sourceDir}");
            Log(logFile, $"Exe: {exeName}");
            Log(logFile, $"PID principal: {pid}");

            // 1. MATAR TODOS LOS PROCESOS SGRRHH (agresivo)
            KillAllSGRRHHProcesses(logFile, pid);

            // Espera extra para asegurar que Windows libere los handles
            Log(logFile, "Esperando 3 segundos para que Windows libere los archivos...");
            Thread.Sleep(3000);

            // 2. Verificar que la carpeta destino existe
            if (!Directory.Exists(targetDir))
            {
                Log(logFile, $"ERROR: La carpeta destino no existe: {targetDir}");
                return;
            }

            // 3. Crear Backup (opcional, pero útil)
            string backupDir = Path.Combine(targetDir, $"backup_{DateTime.Now:yyyyMMdd_HHmmss}");
            try
            {
                Log(logFile, $"Creando backup en {backupDir}...");
                CreateBackup(targetDir, backupDir, logFile);
                Log(logFile, "Backup creado exitosamente.");
            }
            catch (Exception ex)
            {
                Log(logFile, $"Advertencia: No se pudo crear backup completo: {ex.Message}");
                // Continuar aunque falle el backup
            }

            // 4. Copiar archivos nuevos (excluyendo archivos del Updater)
            try
            {
                Log(logFile, "Copiando archivos de actualización...");
                int copiedCount = CopyUpdateFiles(sourceDir, targetDir, logFile);
                Log(logFile, $"✓ {copiedCount} archivos copiados exitosamente.");
            }
            catch (Exception ex)
            {
                Log(logFile, $"ERROR CRÍTICO copiando archivos: {ex.Message}");
                Log(logFile, "La actualización falló. Intentando restaurar backup...");
                
                try
                {
                    RestoreBackup(backupDir, targetDir, logFile);
                    Log(logFile, "Backup restaurado.");
                }
                catch (Exception restoreEx)
                {
                    Log(logFile, $"FALLÓ LA RESTAURACIÓN: {restoreEx.Message}");
                    Log(logFile, "Por favor, reinstale la aplicación manualmente.");
                }
                
                Console.WriteLine("Presione Enter para salir...");
                Console.ReadLine();
                return;
            }

            // 5. Limpiar backups antiguos (mantener solo los últimos 2)
            CleanOldBackups(targetDir, logFile);

            // 6. Reiniciar aplicación
            Log(logFile, "========================================");
            Log(logFile, "ACTUALIZACIÓN COMPLETADA EXITOSAMENTE");
            Log(logFile, "========================================");
            
            try
            {
                string exePath = Path.Combine(targetDir, exeName);
                Log(logFile, $"Iniciando {exePath}...");
                
                Thread.Sleep(1000); // Pequeña pausa antes de reiniciar
                
                Process.Start(new ProcessStartInfo
                {
                    FileName = exePath,
                    UseShellExecute = true,
                    WorkingDirectory = targetDir
                });
                
                Log(logFile, "Aplicación reiniciada correctamente.");
            }
            catch (Exception ex)
            {
                Log(logFile, $"Error reiniciando app: {ex.Message}");
                Log(logFile, $"Por favor, inicie la aplicación manualmente desde: {targetDir}");
                Console.WriteLine("Presione Enter para salir...");
                Console.ReadLine();
            }
        }
        catch (Exception ex)
        {
            string errorMsg = $"Error global: {ex.Message}\n{ex.StackTrace}";
            Console.WriteLine(errorMsg);
            if (!string.IsNullOrEmpty(logFile))
            {
                Log(logFile, errorMsg);
            }
            Console.WriteLine("Presione Enter para salir...");
            Console.ReadLine();
        }
    }

    /// <summary>
    /// Mata TODOS los procesos relacionados con SGRRHH de forma agresiva
    /// </summary>
    static void KillAllSGRRHHProcesses(string logFile, int mainPid)
    {
        Log(logFile, "Buscando y terminando procesos SGRRHH...");
        
        var processNames = new[] { "SGRRHH", "SGRRHH.exe" };
        int killed = 0;
        int currentPid = Process.GetCurrentProcess().Id;

        foreach (var processName in processNames)
        {
            try
            {
                var processes = Process.GetProcessesByName(Path.GetFileNameWithoutExtension(processName));
                foreach (var proc in processes)
                {
                    // No matarse a sí mismo
                    if (proc.Id == currentPid) continue;
                    
                    try
                    {
                        Log(logFile, $"  Terminando proceso: {proc.ProcessName} (PID: {proc.Id})");
                        proc.Kill();
                        proc.WaitForExit(5000);
                        killed++;
                    }
                    catch (Exception ex)
                    {
                        Log(logFile, $"  No se pudo terminar {proc.ProcessName}: {ex.Message}");
                    }
                    finally
                    {
                        proc.Dispose();
                    }
                }
            }
            catch (Exception ex)
            {
                Log(logFile, $"Error buscando procesos {processName}: {ex.Message}");
            }
        }

        // También intentar matar el PID específico si aún existe
        if (mainPid > 0)
        {
            try
            {
                var mainProcess = Process.GetProcessById(mainPid);
                if (!mainProcess.HasExited)
                {
                    Log(logFile, $"  Forzando cierre del proceso principal (PID: {mainPid})...");
                    mainProcess.Kill();
                    mainProcess.WaitForExit(5000);
                    killed++;
                }
                mainProcess.Dispose();
            }
            catch (ArgumentException)
            {
                // El proceso ya no existe, está bien
            }
            catch (Exception ex)
            {
                Log(logFile, $"  Error con proceso principal: {ex.Message}");
            }
        }

        Log(logFile, $"Procesos terminados: {killed}");
    }

    /// <summary>
    /// Crea backup de los archivos actuales
    /// </summary>
    static void CreateBackup(string sourceDir, string backupDir, string logFile)
    {
        Directory.CreateDirectory(backupDir);
        
        var excludedFolders = new[] { "data", "logs", "backup", "updates" };
        
        foreach (var file in Directory.GetFiles(sourceDir))
        {
            string fileName = Path.GetFileName(file);
            // Ignorar archivos de log del updater y backups
            if (fileName.StartsWith("updater_") || fileName.StartsWith("backup_")) continue;
            
            string destFile = Path.Combine(backupDir, fileName);
            try
            {
                File.Copy(file, destFile, true);
            }
            catch
            {
                // Ignorar errores en backup individual
            }
        }

        // Copiar subdirectorios
        foreach (var dir in Directory.GetDirectories(sourceDir))
        {
            string dirName = Path.GetFileName(dir);
            if (excludedFolders.Any(e => dirName.StartsWith(e, StringComparison.OrdinalIgnoreCase))) continue;
            if (dirName.StartsWith("backup_")) continue;
            
            string destSubDir = Path.Combine(backupDir, dirName);
            try
            {
                CopyDirectorySimple(dir, destSubDir);
            }
            catch
            {
                // Ignorar errores en backup de subdirectorios
            }
        }
    }

    /// <summary>
    /// Copia los archivos de actualización, excluyendo los archivos del propio Updater
    /// </summary>
    static int CopyUpdateFiles(string sourceDir, string targetDir, string logFile)
    {
        int copiedCount = 0;
        var errors = new List<string>();

        // Copiar archivos en la raíz
        foreach (var file in Directory.GetFiles(sourceDir))
        {
            string fileName = Path.GetFileName(file);
            
            // IMPORTANTE: Excluir archivos del Updater (no puede reemplazarse a sí mismo)
            if (UpdaterFiles.Contains(fileName, StringComparer.OrdinalIgnoreCase))
            {
                Log(logFile, $"  Omitiendo (archivo del Updater): {fileName}");
                continue;
            }

            string targetFile = Path.Combine(targetDir, fileName);
            
            // Reintentar hasta 5 veces con espera creciente
            bool success = false;
            for (int attempt = 1; attempt <= 5; attempt++)
            {
                try
                {
                    File.Copy(file, targetFile, true);
                    copiedCount++;
                    success = true;
                    break;
                }
                catch (IOException ex) when (attempt < 5)
                {
                    Log(logFile, $"  Reintento {attempt}/5 para {fileName}: {ex.Message}");
                    Thread.Sleep(attempt * 1000); // Espera creciente: 1s, 2s, 3s, 4s
                }
            }

            if (!success)
            {
                errors.Add(fileName);
            }
        }

        // Copiar subdirectorios (excluyendo carpetas del sistema)
        var excludedFolders = new[] { "data", "logs", "backup", "updates" };
        foreach (var dir in Directory.GetDirectories(sourceDir))
        {
            string dirName = Path.GetFileName(dir);
            if (excludedFolders.Any(e => dirName.StartsWith(e, StringComparison.OrdinalIgnoreCase))) continue;
            
            string targetSubDir = Path.Combine(targetDir, dirName);
            try
            {
                CopyDirectorySimple(dir, targetSubDir);
                copiedCount += Directory.GetFiles(dir, "*", SearchOption.AllDirectories).Length;
            }
            catch (Exception ex)
            {
                Log(logFile, $"  Error copiando carpeta {dirName}: {ex.Message}");
            }
        }

        if (errors.Count > 0)
        {
            Log(logFile, $"Archivos con error (no crítico): {string.Join(", ", errors)}");
        }

        return copiedCount;
    }

    /// <summary>
    /// Restaura el backup en caso de error
    /// </summary>
    static void RestoreBackup(string backupDir, string targetDir, string logFile)
    {
        if (!Directory.Exists(backupDir))
        {
            throw new DirectoryNotFoundException("No se encontró el backup para restaurar.");
        }

        foreach (var file in Directory.GetFiles(backupDir))
        {
            string fileName = Path.GetFileName(file);
            string targetFile = Path.Combine(targetDir, fileName);
            
            for (int i = 0; i < 3; i++)
            {
                try
                {
                    File.Copy(file, targetFile, true);
                    break;
                }
                catch (IOException) when (i < 2)
                {
                    Thread.Sleep(1000);
                }
            }
        }
    }

    /// <summary>
    /// Limpia backups antiguos, manteniendo solo los 2 más recientes
    /// </summary>
    static void CleanOldBackups(string targetDir, string logFile)
    {
        try
        {
            var backupDirs = Directory.GetDirectories(targetDir, "backup_*")
                .OrderByDescending(d => d)
                .Skip(2)
                .ToList();

            foreach (var oldBackup in backupDirs)
            {
                try
                {
                    Directory.Delete(oldBackup, true);
                    Log(logFile, $"Backup antiguo eliminado: {Path.GetFileName(oldBackup)}");
                }
                catch
                {
                    // Ignorar errores al limpiar backups
                }
            }
        }
        catch
        {
            // Ignorar errores al limpiar
        }
    }

    /// <summary>
    /// Copia un directorio de forma simple
    /// </summary>
    static void CopyDirectorySimple(string sourceDir, string destDir)
    {
        Directory.CreateDirectory(destDir);

        foreach (var file in Directory.GetFiles(sourceDir))
        {
            string destFile = Path.Combine(destDir, Path.GetFileName(file));
            File.Copy(file, destFile, true);
        }

        foreach (var dir in Directory.GetDirectories(sourceDir))
        {
            string destSubDir = Path.Combine(destDir, Path.GetFileName(dir));
            CopyDirectorySimple(dir, destSubDir);
        }
    }

    static void Log(string path, string message)
    {
        try
        {
            string line = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {message}";
            Console.WriteLine(line);
            File.AppendAllText(path, line + Environment.NewLine);
        }
        catch { }
    }
}
