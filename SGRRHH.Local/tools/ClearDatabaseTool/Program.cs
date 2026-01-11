using System;
using System.Data;
using Microsoft.Data.Sqlite;

class ClearDatabase
{
    static void Main(string[] args)
    {
        var dbPath = args.Length > 0 ? args[0] : "C:\\SGRRHH\\Data\\sgrrhh.db";
        
        if (!File.Exists(dbPath))
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"ERROR: No se encuentra la base de datos en: {dbPath}");
            Console.ResetColor();
            Environment.Exit(1);
        }
        
        var tables = new[] {
            "detalles_actividad", "registros_diarios", "documentos_empleado",
            "proyectos_empleados", "nominas", "prestaciones", "permisos",
            "vacaciones", "contratos", "empleados", "proyectos", "actividades",
            "cargos", "departamentos", "tipos_permiso", "festivos_colombia",
            "configuracion_legal", "configuracion_sistema", "audit_logs"
        };
        
        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("=================================================================");
        Console.WriteLine("  LIMPIEZA DE BASE DE DATOS - EXCEPTO USUARIOS");
        Console.WriteLine("=================================================================");
        Console.ResetColor();
        Console.WriteLine();
        Console.WriteLine($"Base de datos: {dbPath}");
        Console.WriteLine($"Tablas a limpiar: {tables.Length}");
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("Tabla preservada: usuarios");
        Console.ResetColor();
        Console.WriteLine();
        
        try
        {
            var connectionString = $"Data Source={dbPath}";
            using var connection = new SqliteConnection(connectionString);
            connection.Open();
            
            using var transaction = connection.BeginTransaction();
            
            // Deshabilitar foreign keys
            using (var cmd = connection.CreateCommand())
            {
                cmd.CommandText = "PRAGMA foreign_keys = OFF;";
                cmd.Transaction = transaction;
                cmd.ExecuteNonQuery();
            }
            
            int totalDeleted = 0;
            int successCount = 0;
            
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("Iniciando limpieza...");
            Console.ResetColor();
            Console.WriteLine();
            
            foreach (var table in tables)
            {
                try
                {
                    // Contar registros
                    int count;
                    using (var cmd = connection.CreateCommand())
                    {
                        cmd.CommandText = $"SELECT COUNT(*) FROM {table};";
                        cmd.Transaction = transaction;
                        count = Convert.ToInt32(cmd.ExecuteScalar());
                    }
                    
                    if (count == 0)
                    {
                        Console.ForegroundColor = ConsoleColor.DarkGray;
                        Console.WriteLine($"  [-] {table} (ya esta vacia)");
                        Console.ResetColor();
                    }
                    else
                    {
                        // Eliminar registros
                        using (var cmd = connection.CreateCommand())
                        {
                            cmd.CommandText = $"DELETE FROM {table};";
                            cmd.Transaction = transaction;
                            int deleted = cmd.ExecuteNonQuery();
                            
                            Console.ForegroundColor = ConsoleColor.Green;
                            Console.Write($"  [OK] {table}");
                            Console.ResetColor();
                            Console.ForegroundColor = ConsoleColor.Gray;
                            Console.WriteLine($" ({deleted} registros eliminados)");
                            Console.ResetColor();
                            
                            totalDeleted += deleted;
                            successCount++;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"  [ERROR] {table} ({ex.Message})");
                    Console.ResetColor();
                }
            }
            
            transaction.Commit();
            
            // Reactivar foreign keys
            using (var cmd = connection.CreateCommand())
            {
                cmd.CommandText = "PRAGMA foreign_keys = ON;";
                cmd.ExecuteNonQuery();
            }
            
            // Eliminar el flag de seed data para que no se reinicialicen automáticamente los datos semilla
            using (var cmd = connection.CreateCommand())
            {
                cmd.CommandText = "DELETE FROM configuracion_sistema WHERE clave = 'seed_data_initialized';";
                cmd.ExecuteNonQuery();
            }
            
            // Optimizar
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("Optimizando base de datos...");
            Console.ResetColor();
            using (var cmd = connection.CreateCommand())
            {
                cmd.CommandText = "VACUUM;";
                cmd.ExecuteNonQuery();
            }
            
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("  [OK] Base de datos optimizada");
            Console.ResetColor();
            
            // Contar usuarios
            int userCount;
            using (var cmd = connection.CreateCommand())
            {
                cmd.CommandText = "SELECT COUNT(*) FROM usuarios WHERE activo = 1;";
                userCount = Convert.ToInt32(cmd.ExecuteScalar());
            }
            
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("=================================================================");
            Console.WriteLine("  RESUMEN DE LIMPIEZA");
            Console.WriteLine("=================================================================");
            Console.ResetColor();
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write("Total de registros eliminados: ");
            Console.ResetColor();
            Console.WriteLine(totalDeleted);
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write("Tablas limpiadas exitosamente: ");
            Console.ResetColor();
            Console.WriteLine(successCount);
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write("Usuarios preservados: ");
            Console.ResetColor();
            Console.WriteLine($"{userCount} usuarios activos");
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("[OK] Limpieza completada exitosamente");
            Console.ResetColor();
            Console.WriteLine();
        }
        catch (Exception ex)
        {
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"ERROR: {ex.Message}");
            Console.WriteLine(ex.StackTrace);
            Console.ResetColor();
            Environment.Exit(1);
        }
    }
}
