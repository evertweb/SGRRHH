using System;
using Microsoft.Data.Sqlite;

class CheckTables
{
    static void Main()
    {
        var dbPath = "C:\\SGRRHH\\Data\\sgrrhh.db";
        var tables = new[] { "departamentos", "tipos_permiso", "festivos_colombia", "configuracion_legal" };
        
        Console.WriteLine("\nVerificando datos en tablas semilla...\n");
        
        try
        {
            var connectionString = $"Data Source={dbPath}";
            using var connection = new SqliteConnection(connectionString);
            connection.Open();
            
            foreach (var table in tables)
            {
                using var cmd = connection.CreateCommand();
                cmd.CommandText = $"SELECT COUNT(*) FROM {table};";
                var count = Convert.ToInt32(cmd.ExecuteScalar());
                
                if (count > 0)
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine($"[!] {table}: {count} registros");
                    Console.ResetColor();
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($"[OK] {table}: vacia");
                    Console.ResetColor();
                }
            }
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"Error: {ex.Message}");
            Console.ResetColor();
        }
    }
}
