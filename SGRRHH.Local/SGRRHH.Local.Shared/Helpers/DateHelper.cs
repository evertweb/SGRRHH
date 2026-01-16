namespace SGRRHH.Local.Shared.Helpers;

/// <summary>
/// Helper para cálculos y operaciones con fechas
/// </summary>
public static class DateHelper
{
    /// <summary>
    /// Calcula la edad a partir de fecha de nacimiento
    /// Extraído de EmpleadoExpediente.razor líneas 871-877
    /// </summary>
    public static int CalcularEdad(DateTime fechaNacimiento)
    {
        var hoy = DateTime.Today;
        var edad = hoy.Year - fechaNacimiento.Year;
        if (fechaNacimiento.Date > hoy.AddYears(-edad)) edad--;
        return edad;
    }
    
    /// <summary>
    /// Calcula la antigüedad laboral formateada (X años, Y meses)
    /// Extraído de EmpleadoExpediente.razor líneas 879-889
    /// </summary>
    public static string CalcularAntiguedad(DateTime fechaIngreso)
    {
        var hoy = DateTime.Today;
        var años = hoy.Year - fechaIngreso.Year;
        if (fechaIngreso.Date > hoy.AddYears(-años)) años--;
        var meses = hoy.Month - fechaIngreso.Month;
        if (meses < 0) { años--; meses += 12; }
        if (años > 0 && meses > 0) return $"{años} año(s) {meses} mes(es)";
        else if (años > 0) return $"{años} año(s)";
        else return $"{meses} mes(es)";
    }
}
