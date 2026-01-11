using System.Globalization;

namespace SGRRHH.Local.Shared.Helpers;

/// <summary>
/// Helper para formateo de números según el contexto colombiano
/// - Moneda/Salarios: $ 1.750.905 (separador de miles con punto)
/// - Cédulas/Documentos: 1.192.208.848 (separador de miles con punto)
/// - Porcentajes: 12,5 % (decimal con coma)
/// - Horas: 8,5 (decimal con coma)
/// </summary>
public static class NumberFormatHelper
{
    // Cultura colombiana para formateo
    private static readonly CultureInfo CulturaColombia = new CultureInfo("es-CO");
    
    /// <summary>
    /// Formatea un valor monetario (salarios, presupuestos, valores) con símbolo de peso
    /// Ejemplo: 1750905 -> "$ 1.750.905"
    /// </summary>
    public static string FormatearMoneda(decimal? valor)
    {
        if (!valor.HasValue)
            return "-";
        
        return $"$ {valor.Value.ToString("N0", CulturaColombia)}";
    }
    
    /// <summary>
    /// Formatea un valor monetario sin símbolo de peso
    /// Ejemplo: 1750905 -> "1.750.905"
    /// </summary>
    public static string FormatearMonedaSinSimbolo(decimal? valor)
    {
        if (!valor.HasValue)
            return "-";
        
        return valor.Value.ToString("N0", CulturaColombia);
    }
    
    /// <summary>
    /// Formatea un valor monetario con decimales (para visualización de cálculos)
    /// Ejemplo: 1750905.50 -> "$ 1.750.905,50"
    /// </summary>
    public static string FormatearMonedaConDecimales(decimal? valor, int decimales = 2)
    {
        if (!valor.HasValue)
            return "-";
        
        return $"$ {valor.Value.ToString($"N{decimales}", CulturaColombia)}";
    }
    
    /// <summary>
    /// Formatea una cédula o documento de identidad con separadores de miles
    /// Ejemplo: "1192208848" -> "1.192.208.848"
    /// </summary>
    public static string FormatearCedula(string? cedula)
    {
        if (string.IsNullOrWhiteSpace(cedula))
            return "-";
        
        // Limpiar cualquier carácter no numérico
        var soloNumeros = new string(cedula.Where(char.IsDigit).ToArray());
        
        if (string.IsNullOrEmpty(soloNumeros))
            return cedula; // Devolver original si no tiene números
        
        // Intentar parsear como número largo para formatear
        if (long.TryParse(soloNumeros, out long numero))
        {
            return numero.ToString("N0", CulturaColombia);
        }
        
        return cedula; // Devolver original si no se puede parsear
    }
    
    /// <summary>
    /// Formatea un número de teléfono colombiano
    /// Ejemplo: "3001234567" -> "300 123 4567"
    /// Ejemplo: "6012345678" -> "601 234 5678"
    /// </summary>
    public static string FormatearTelefono(string? telefono)
    {
        if (string.IsNullOrWhiteSpace(telefono))
            return "-";
        
        // Limpiar cualquier carácter no numérico excepto +
        var limpio = new string(telefono.Where(c => char.IsDigit(c) || c == '+').ToArray());
        
        if (string.IsNullOrEmpty(limpio))
            return telefono;
        
        // Si tiene código de país +57, formatear diferente
        if (limpio.StartsWith("+57"))
        {
            var sinCodigo = limpio.Substring(3);
            return $"+57 {FormatearTelefonoLocal(sinCodigo)}";
        }
        else if (limpio.StartsWith("57") && limpio.Length >= 12)
        {
            var sinCodigo = limpio.Substring(2);
            return $"+57 {FormatearTelefonoLocal(sinCodigo)}";
        }
        
        return FormatearTelefonoLocal(limpio);
    }
    
    private static string FormatearTelefonoLocal(string telefono)
    {
        // Celular: 10 dígitos -> 300 123 4567
        if (telefono.Length == 10)
        {
            return $"{telefono.Substring(0, 3)} {telefono.Substring(3, 3)} {telefono.Substring(6, 4)}";
        }
        // Fijo con indicativo: 10 dígitos -> 601 234 5678
        else if (telefono.Length == 10 && (telefono.StartsWith("60") || telefono.StartsWith("61")))
        {
            return $"{telefono.Substring(0, 3)} {telefono.Substring(3, 3)} {telefono.Substring(6, 4)}";
        }
        // Fijo local: 7 dígitos -> 234 5678
        else if (telefono.Length == 7)
        {
            return $"{telefono.Substring(0, 3)} {telefono.Substring(3, 4)}";
        }
        
        // Si no coincide con ningún patrón, devolver como está
        return telefono;
    }
    
    /// <summary>
    /// Formatea un número de cuenta bancaria
    /// Ejemplo: "12345678901234" -> "1234-5678-9012-34"
    /// </summary>
    public static string FormatearNumeroCuenta(string? numeroCuenta)
    {
        if (string.IsNullOrWhiteSpace(numeroCuenta))
            return "-";
        
        // Limpiar cualquier carácter no numérico
        var soloNumeros = new string(numeroCuenta.Where(char.IsDigit).ToArray());
        
        if (string.IsNullOrEmpty(soloNumeros))
            return numeroCuenta;
        
        // Agrupar de 4 en 4 separados por guión
        var grupos = new List<string>();
        for (int i = 0; i < soloNumeros.Length; i += 4)
        {
            var longitud = Math.Min(4, soloNumeros.Length - i);
            grupos.Add(soloNumeros.Substring(i, longitud));
        }
        
        return string.Join("-", grupos);
    }
    
    /// <summary>
    /// Formatea un porcentaje
    /// Ejemplo: 12.5m -> "12,5 %"
    /// </summary>
    public static string FormatearPorcentaje(decimal? valor, int decimales = 1)
    {
        if (!valor.HasValue)
            return "-";
        
        return $"{valor.Value.ToString($"N{decimales}", CulturaColombia)} %";
    }
    
    /// <summary>
    /// Formatea horas trabajadas
    /// Ejemplo: 8.5m -> "8,5 h"
    /// </summary>
    public static string FormatearHoras(decimal? valor, int decimales = 1)
    {
        if (!valor.HasValue)
            return "-";
        
        if (valor.Value == 0)
            return "0 h";
        
        return $"{valor.Value.ToString($"N{decimales}", CulturaColombia)} h";
    }
    
    /// <summary>
    /// Formatea días (vacaciones, permisos, etc.)
    /// Ejemplo: 15 -> "15 días"
    /// </summary>
    public static string FormatearDias(int? valor)
    {
        if (!valor.HasValue)
            return "-";
        
        return valor.Value == 1 ? "1 día" : $"{valor.Value} días";
    }
    
    /// <summary>
    /// Formatea un número entero con separadores de miles
    /// Ejemplo: 1234567 -> "1.234.567"
    /// </summary>
    public static string FormatearEntero(int? valor)
    {
        if (!valor.HasValue)
            return "-";
        
        return valor.Value.ToString("N0", CulturaColombia);
    }
    
    /// <summary>
    /// Formatea un número entero largo con separadores de miles
    /// Ejemplo: 1234567890 -> "1.234.567.890"
    /// </summary>
    public static string FormatearEnteroLargo(long? valor)
    {
        if (!valor.HasValue)
            return "-";
        
        return valor.Value.ToString("N0", CulturaColombia);
    }
    
    /// <summary>
    /// Formatea un número decimal genérico con separadores de miles
    /// Ejemplo: 1234567.89 -> "1.234.567,89"
    /// </summary>
    public static string FormatearDecimal(decimal? valor, int decimales = 2)
    {
        if (!valor.HasValue)
            return "-";
        
        return valor.Value.ToString($"N{decimales}", CulturaColombia);
    }
    
    /// <summary>
    /// Formatea el salario mínimo vigente con contexto
    /// Ejemplo: 1750905 -> "$ 1.750.905 (SMMLV 2026)"
    /// </summary>
    public static string FormatearSalarioMinimo(decimal valor, int año = 2026)
    {
        return $"{FormatearMoneda(valor)} (SMMLV {año})";
    }
    
    /// <summary>
    /// Formatea un valor monetario indicando si es menor al salario mínimo
    /// </summary>
    public static (string Texto, bool EsMenorAlMinimo) FormatearSalarioConValidacion(decimal? valor, decimal salarioMinimo = 1750905m)
    {
        if (!valor.HasValue)
            return ("-", false);
        
        var esMenor = valor.Value < salarioMinimo;
        var texto = FormatearMoneda(valor);
        
        if (esMenor)
        {
            texto += " ⚠️";
        }
        
        return (texto, esMenor);
    }
}
