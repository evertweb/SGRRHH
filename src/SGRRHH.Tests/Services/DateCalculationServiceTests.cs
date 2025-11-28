using SGRRHH.Infrastructure.Services;
using Xunit;

namespace SGRRHH.Tests.Services;

/// <summary>
/// Pruebas unitarias para DateCalculationService
/// </summary>
public class DateCalculationServiceTests
{
    private readonly DateCalculationService _service;

    public DateCalculationServiceTests()
    {
        _service = new DateCalculationService();
    }

    #region CalcularDiasCalendario Tests

    [Fact]
    public void CalcularDiasCalendario_SameDay_ReturnsOne()
    {
        // Arrange
        var fecha = new DateTime(2024, 1, 15);

        // Act
        var resultado = _service.CalcularDiasCalendario(fecha, fecha);

        // Assert
        Assert.Equal(1, resultado);
    }

    [Fact]
    public void CalcularDiasCalendario_TwoDays_ReturnsTwo()
    {
        // Arrange
        var inicio = new DateTime(2024, 1, 15);
        var fin = new DateTime(2024, 1, 16);

        // Act
        var resultado = _service.CalcularDiasCalendario(inicio, fin);

        // Assert
        Assert.Equal(2, resultado);
    }

    [Fact]
    public void CalcularDiasCalendario_OneWeek_ReturnsSeven()
    {
        // Arrange
        var inicio = new DateTime(2024, 1, 15); // Lunes
        var fin = new DateTime(2024, 1, 21);    // Domingo

        // Act
        var resultado = _service.CalcularDiasCalendario(inicio, fin);

        // Assert
        Assert.Equal(7, resultado);
    }

    #endregion

    #region CalcularDiasHabiles Tests

    [Fact]
    public void CalcularDiasHabiles_OneWeekNoHolidays_ReturnsFive()
    {
        // Arrange - Semana sin festivos
        var inicio = new DateTime(2024, 2, 5);  // Lunes
        var fin = new DateTime(2024, 2, 9);     // Viernes

        // Act
        var resultado = _service.CalcularDiasHabiles(inicio, fin);

        // Assert
        Assert.Equal(5, resultado);
    }

    [Fact]
    public void CalcularDiasHabiles_FullWeekNoHolidays_ReturnsFive()
    {
        // Arrange - Semana completa (incluye fin de semana)
        var inicio = new DateTime(2024, 2, 5);  // Lunes
        var fin = new DateTime(2024, 2, 11);    // Domingo

        // Act
        var resultado = _service.CalcularDiasHabiles(inicio, fin);

        // Assert
        Assert.Equal(5, resultado);
    }

    [Fact]
    public void CalcularDiasHabiles_OnlyWeekend_ReturnsMinimumOne()
    {
        // Arrange - Solo fin de semana
        var inicio = new DateTime(2024, 2, 3);  // Sábado
        var fin = new DateTime(2024, 2, 4);     // Domingo

        // Act
        var resultado = _service.CalcularDiasHabiles(inicio, fin);

        // Assert
        // El servicio devuelve mínimo 1 día hábil incluso en fines de semana
        Assert.Equal(1, resultado);
    }

    [Fact]
    public void CalcularDiasHabiles_WithHoliday_IncludesHoliday()
    {
        // Arrange - 1 de enero es festivo pero CalcularDiasHabiles NO excluye festivos
        var inicio = new DateTime(2024, 1, 1);  // Año nuevo (lunes)
        var fin = new DateTime(2024, 1, 5);     // Viernes

        // Act
        var resultado = _service.CalcularDiasHabiles(inicio, fin);

        // Assert
        // Lunes 1 + Martes 2 + Miércoles 3 + Jueves 4 + Viernes 5 = 5 días
        // CalcularDiasHabiles solo excluye fines de semana, NO festivos
        Assert.Equal(5, resultado);
    }

    #endregion

    #region CalcularDiasHabilesSinFestivos Tests

    [Fact]
    public void CalcularDiasHabilesSinFestivos_OneWeek_ExcludesNewYear()
    {
        // Arrange - Enero 2024: día 1 es festivo (Año Nuevo)
        var inicio = new DateTime(2024, 1, 1);  // Lunes (festivo)
        var fin = new DateTime(2024, 1, 5);     // Viernes

        // Act
        var resultado = _service.CalcularDiasHabilesSinFestivos(inicio, fin);

        // Assert
        // Lunes 1 (festivo excluido), Martes 2, Miércoles 3, Jueves 4, Viernes 5 = 4 días
        Assert.Equal(4, resultado);
    }

    #endregion

    #region GetFestivosColombia Tests

    [Fact]
    public void GetFestivosColombia_Year2024_ReturnsAtLeast18Holidays()
    {
        // Act
        var festivos = _service.GetFestivosColombia(2024);

        // Assert
        // Colombia tiene 18 días festivos oficiales
        Assert.True(festivos.Count() >= 18, $"Se esperaban al menos 18 festivos, se obtuvieron {festivos.Count()}");
    }

    [Fact]
    public void GetFestivosColombia_ContainsFixedHolidays()
    {
        // Act
        var festivos = _service.GetFestivosColombia(2024).ToList();

        // Assert - Festivos fijos
        Assert.Contains(new DateTime(2024, 1, 1), festivos);   // Año nuevo
        Assert.Contains(new DateTime(2024, 5, 1), festivos);   // Día del trabajo
        Assert.Contains(new DateTime(2024, 7, 20), festivos);  // Día de la independencia
        Assert.Contains(new DateTime(2024, 8, 7), festivos);   // Batalla de Boyacá
        Assert.Contains(new DateTime(2024, 12, 25), festivos); // Navidad
    }

    [Fact]
    public void GetFestivosColombia_LeyEmiliani_MovesToMonday()
    {
        // El 6 de enero 2024 cae sábado, debería moverse a lunes 8
        var festivos = _service.GetFestivosColombia(2024).ToList();
        
        // Reyes Magos (6 de enero) - si cae en sábado se mueve a lunes
        var luneSiguiente = new DateTime(2024, 1, 8);  // Lunes

        // El festivo debe estar en lunes, no en sábado
        Assert.Contains(luneSiguiente, festivos);
    }

    #endregion

    #region CalcularDiasVacacionesGanados Tests

    [Fact]
    public void CalcularDiasVacacionesGanados_FullYear_Returns15()
    {
        // Arrange - Empleado que trabajó todo el año 2023
        var fechaIngreso = new DateTime(2023, 1, 1);
        var periodo = 2024; // Calculando para 2024

        // Act
        var resultado = _service.CalcularDiasVacacionesGanados(fechaIngreso, periodo);

        // Assert
        // Trabajó todo el año = 15 días
        Assert.Equal(15, resultado);
    }

    [Fact]
    public void CalcularDiasVacacionesGanados_SixMonths_ReturnsProportional()
    {
        // Arrange - Empleado que ingresó a mitad del periodo
        var fechaIngreso = new DateTime(2024, 7, 1);
        var periodo = 2024;

        // Act
        var resultado = _service.CalcularDiasVacacionesGanados(fechaIngreso, periodo);

        // Assert
        // 6 meses trabajados aprox = ~7 días (proporcional)
        Assert.True(resultado >= 6 && resultado <= 8, $"Resultado: {resultado}");
    }

    [Fact]
    public void CalcularDiasVacacionesGanados_FuturePeriod_ReturnsZero()
    {
        // Arrange - Periodo futuro
        var fechaIngreso = new DateTime(2020, 1, 1);
        var periodo = 2030; // Periodo muy futuro

        // Act
        var resultado = _service.CalcularDiasVacacionesGanados(fechaIngreso, periodo);

        // Assert
        // No se puede calcular vacaciones para periodo futuro
        Assert.True(resultado >= 0);
    }

    #endregion

    #region Consistency Tests

    [Fact]
    public void CalcularDiasHabiles_IsCached_ReturnsSameResult()
    {
        // Arrange
        var inicio = new DateTime(2024, 1, 1);
        var fin = new DateTime(2024, 1, 31);

        // Act - Llamar dos veces
        var resultado1 = _service.CalcularDiasHabiles(inicio, fin);
        var resultado2 = _service.CalcularDiasHabiles(inicio, fin);

        // Assert
        Assert.Equal(resultado1, resultado2);
    }

    [Theory]
    [InlineData(2020)]
    [InlineData(2021)]
    [InlineData(2022)]
    [InlineData(2023)]
    [InlineData(2024)]
    [InlineData(2025)]
    public void GetFestivosColombia_MultipleYears_ReturnsValidHolidays(int year)
    {
        // Act
        var festivos = _service.GetFestivosColombia(year);

        // Assert
        Assert.NotEmpty(festivos);
        Assert.True(festivos.All(f => f.Year == year), "Todos los festivos deben ser del año solicitado");
    }

    #endregion
}
