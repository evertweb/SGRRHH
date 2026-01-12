namespace SGRRHH.Local.Tests.E2E.Helpers;

/// <summary>
/// Helper para generar datos de prueba únicos
/// </summary>
public static class TestDataHelper
{
    private static readonly Random _random = new();
    
    private static readonly string[] Nombres = 
    {
        "Juan", "Carlos", "Pedro", "Luis", "Miguel", 
        "María", "Ana", "Laura", "Carmen", "Patricia"
    };
    
    private static readonly string[] Apellidos = 
    {
        "García", "Rodríguez", "Martínez", "López", "González",
        "Hernández", "Pérez", "Sánchez", "Ramírez", "Torres"
    };

    /// <summary>
    /// Genera una cédula única basada en timestamp
    /// </summary>
    public static string GenerarCedula()
    {
        // Formato: 1XXXXXXXXX (10 dígitos, empieza con 1)
        var timestamp = DateTime.Now.Ticks % 100000000;
        var random = _random.Next(10, 99);
        return $"1{timestamp:D8}{random:D1}";
    }

    /// <summary>
    /// Genera un nombre completo aleatorio
    /// </summary>
    public static (string Nombre, string Apellido) GenerarNombreCompleto()
    {
        var nombre = Nombres[_random.Next(Nombres.Length)];
        var apellido = Apellidos[_random.Next(Apellidos.Length)];
        return (nombre, apellido);
    }

    /// <summary>
    /// Genera un email único basado en nombre y timestamp
    /// </summary>
    public static string GenerarEmail(string nombre, string apellido)
    {
        var timestamp = DateTime.Now.Ticks % 10000;
        return $"{nombre.ToLower()}.{apellido.ToLower()}{timestamp}@test.com";
    }

    /// <summary>
    /// Genera un número de teléfono colombiano válido
    /// </summary>
    public static string GenerarTelefono()
    {
        // Formato: 3XXXXXXXXX (celular colombiano)
        var numero = _random.Next(100000000, 999999999);
        return $"3{numero}";
    }

    /// <summary>
    /// Genera una fecha de nacimiento para un adulto (entre 18 y 60 años)
    /// </summary>
    public static DateTime GenerarFechaNacimiento()
    {
        var edad = _random.Next(18, 60);
        return DateTime.Today.AddYears(-edad).AddDays(-_random.Next(0, 365));
    }

    /// <summary>
    /// Genera datos completos para un empleado de prueba
    /// </summary>
    public static EmpleadoTestData GenerarEmpleado()
    {
        var (nombre, apellido) = GenerarNombreCompleto();
        return new EmpleadoTestData
        {
            Cedula = GenerarCedula(),
            Nombre = nombre,
            Apellido = apellido,
            Email = GenerarEmail(nombre, apellido),
            Telefono = GenerarTelefono(),
            FechaNacimiento = GenerarFechaNacimiento(),
            FechaIngreso = DateTime.Today
        };
    }
}

/// <summary>
/// Datos de un empleado para pruebas
/// </summary>
public class EmpleadoTestData
{
    public string Cedula { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string Apellido { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Telefono { get; set; } = string.Empty;
    public DateTime FechaNacimiento { get; set; }
    public DateTime FechaIngreso { get; set; }
    
    public string NombreCompleto => $"{Nombre} {Apellido}";
}
