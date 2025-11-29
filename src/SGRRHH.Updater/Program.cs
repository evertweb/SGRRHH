using System.Diagnostics;

namespace SGRRHH.Updater;

class Program
{
    static void Main(string[] args)
    {
        // Argumentos esperados:
        // 0: Directorio destino (donde está instalada la app)
        // 1: Directorio fuente (donde está la actualización extraída)
        // 2: Nombre del ejecutable a reiniciar (ej: SGRRHH.exe)
        // 3: PID del proceso a esperar (opcional)

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

            string logFile = Path.Combine(targetDir, "updater_log.txt");
            Log(logFile, "Iniciando actualizador...");
            Log(logFile, $"Target: {targetDir}");
            Log(logFile, $"Source: {sourceDir}");
            Log(logFile, $"Exe: {exeName}");
            Log(logFile, $"PID: {pid}");

            // 1. Esperar a que el proceso termine
            if (pid > 0)
            {
                try
                {
                    var process = Process.GetProcessById(pid);
                    Log(logFile, $"Esperando a que termine el proceso {pid}...");
                    process.WaitForExit(10000); // Esperar máximo 10 segundos
                    if (!process.HasExited)
                    {
                        Log(logFile, "El proceso no terminó a tiempo. Forzando cierre...");
                        process.Kill();
                    }
                }
                catch (ArgumentException)
                {
                    Log(logFile, "El proceso ya había terminado.");
                }
                catch (Exception ex)
                {
                    Log(logFile, $"Error al esperar proceso: {ex.Message}");
                }
            }

            // Espera extra para asegurar que .NET libere todos los recursos (DLLs, etc.)
            Log(logFile, "Esperando 5 segundos adicionales para que se liberen todos los archivos...");
            Thread.Sleep(5000);

            // 2. Crear Backup
            string backupDir = Path.Combine(targetDir, $"backup_{DateTime.Now:yyyyMMdd_HHmmss}");
            try
            {
                Log(logFile, $"Creando backup en {backupDir}...");
                Directory.CreateDirectory(backupDir);
                
                // Copiar archivos actuales al backup (excluyendo carpetas de datos/logs/backups)
                foreach (var file in Directory.GetFiles(targetDir))
                {
                    string fileName = Path.GetFileName(file);
                    // Ignorar archivos temporales o de log
                    if (fileName.StartsWith("updater_") || fileName.StartsWith("backup_")) continue;

                    string destFile = Path.Combine(backupDir, fileName);
                    File.Copy(file, destFile, true);
                }
                // Copiar subdirectorios si es necesario (aquí simplificado a solo raíz por ahora, o recursivo si se requiere)
                // Para SGRRHH parece que todo importante está en raíz o libs.
                // Implementación recursiva simple para backup completo
                CopyDirectory(targetDir, backupDir, true, new[] { "data", "logs", "backup", "updates" });
            }
            catch (Exception ex)
            {
                Log(logFile, $"Error creando backup: {ex.Message}");
                // Continuamos aunque falle el backup? Mejor sí, para intentar actualizar.
            }

            // 3. Copiar archivos nuevos
            try
            {
                Log(logFile, "Copiando nuevos archivos...");
                CopyDirectory(sourceDir, targetDir, true, null);
                Log(logFile, "Archivos copiados exitosamente.");
            }
            catch (Exception ex)
            {
                Log(logFile, $"ERROR CRÍTICO copiando archivos: {ex.Message}");
                Log(logFile, "Intentando restaurar backup...");
                try
                {
                    CopyDirectory(backupDir, targetDir, true, null);
                    Log(logFile, "Backup restaurado.");
                }
                catch (Exception restoreEx)
                {
                    Log(logFile, $"FALLÓ LA RESTAURACIÓN: {restoreEx.Message}");
                }
                return; // No reiniciar si falló la copia
            }

            // 4. Reiniciar aplicación
            try
            {
                string exePath = Path.Combine(targetDir, exeName);
                Log(logFile, $"Reiniciando {exePath}...");
                Process.Start(new ProcessStartInfo
                {
                    FileName = exePath,
                    UseShellExecute = true,
                    WorkingDirectory = targetDir
                });
            }
            catch (Exception ex)
            {
                Log(logFile, $"Error reiniciando app: {ex.Message}");
            }

        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error global: {ex.Message}");
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

    static void CopyDirectory(string sourceDir, string destDir, bool recursive, string[]? excludedFolders)
    {
        var dir = new DirectoryInfo(sourceDir);
        if (!dir.Exists) throw new DirectoryNotFoundException($"Source directory not found: {dir.FullName}");

        DirectoryInfo[] dirs = dir.GetDirectories();
        Directory.CreateDirectory(destDir);

        foreach (FileInfo file in dir.GetFiles())
        {
            string targetFilePath = Path.Combine(destDir, file.Name);

            // Reintentar hasta 3 veces si el archivo está bloqueado
            int retries = 3;
            for (int i = 0; i < retries; i++)
            {
                try
                {
                    file.CopyTo(targetFilePath, true);
                    break; // Éxito, salir del loop
                }
                catch (IOException) when (i < retries - 1)
                {
                    // Archivo bloqueado, esperar y reintentar
                    Thread.Sleep(1000);
                }
            }
        }

        if (recursive)
        {
            foreach (DirectoryInfo subDir in dirs)
            {
                if (excludedFolders != null && excludedFolders.Any(e => subDir.Name.StartsWith(e, StringComparison.OrdinalIgnoreCase)))
                    continue;

                string newDestDir = Path.Combine(destDir, subDir.Name);
                CopyDirectory(subDir.FullName, newDestDir, true, excludedFolders);
            }
        }
    }
}
