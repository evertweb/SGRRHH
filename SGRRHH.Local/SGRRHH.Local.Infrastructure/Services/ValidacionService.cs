using System.Globalization;
using Microsoft.Extensions.Logging;
using SGRRHH.Local.Domain.Entities;
using SGRRHH.Local.Domain.Enums;
using SGRRHH.Local.Shared;
using SGRRHH.Local.Shared.Interfaces;

namespace SGRRHH.Local.Infrastructure.Services;

/// <summary>
/// Servicio de validaciones según legislación laboral colombiana.
/// Basado en el Código Sustantivo del Trabajo (CST) y normativa vigente.
/// </summary>
public class ValidacionService : IValidacionService
{
    private readonly IConfiguracionLegalRepository _configLegalRepository;
    private readonly ILogger<ValidacionService> _logger;

    public ValidacionService(
        IConfiguracionLegalRepository configLegalRepository,
        ILogger<ValidacionService> logger)
    {
        _configLegalRepository = configLegalRepository;
        _logger = logger;
    }

    /// <summary>
    /// Valida que el salario no sea inferior al SMLMV
    /// </summary>
    public async Task<Result> ValidarSalarioMinimoAsync(decimal salario)
    {
        try
        {
            var config = await _configLegalRepository.GetVigenteAsync();
            if (config == null)
                return Result.Fail("No se encontró configuración legal vigente");

            if (salario < config.SalarioMinimoMensual)
            {
                return Result.Fail(
                    $"El salario no puede ser menor al Salario Mínimo Legal Mensual Vigente (SMLMV): " +
                    $"{config.SalarioMinimoMensual:C0}. Salario ingresado: {salario:C0}");
            }

            return Result.Ok("Salario válido según legislación colombiana");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al validar salario mínimo");
            return Result.Fail($"Error al validar salario: {ex.Message}");
        }
    }

    /// <summary>
    /// Valida que el empleado tenga la edad mínima laboral (18 años).
    /// Según Art. 30 del CST, la edad mínima para trabajar es 18 años.
    /// Menores de 18 años requieren autorización especial del Ministerio del Trabajo.
    /// </summary>
    public async Task<Result> ValidarEdadLaboralAsync(DateTime fechaNacimiento)
    {
        try
        {
            var config = await _configLegalRepository.GetVigenteAsync();
            if (config == null)
                return Result.Fail("No se encontró configuración legal vigente");

            var hoy = DateTime.Today;
            var edad = hoy.Year - fechaNacimiento.Year;

            // Ajustar si aún no ha cumplido años este año
            if (hoy.DayOfYear < fechaNacimiento.DayOfYear)
                edad--;

            if (edad < config.EdadMinimaLaboral)
            {
                return Result.Fail(
                    $"El empleado debe tener al menos {config.EdadMinimaLaboral} años para trabajar. " +
                    $"Edad actual: {edad} años. Fecha de nacimiento: {fechaNacimiento:dd/MM/yyyy}. " +
                    $"Los menores de 18 años requieren autorización especial del Ministerio del Trabajo.");
            }

            return Result.Ok("Edad laboral válida");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al validar edad laboral");
            return Result.Fail($"Error al validar edad: {ex.Message}");
        }
    }

    /// <summary>
    /// Valida que la jornada laboral no exceda las 48 horas semanales.
    /// Según Art. 161 del CST modificado por Ley 2101 de 2021.
    /// </summary>
    public async Task<Result> ValidarJornadaLaboralAsync(List<RegistroDiario> registros)
    {
        try
        {
            var config = await _configLegalRepository.GetVigenteAsync();
            if (config == null)
                return Result.Fail("No se encontró configuración legal vigente");

            if (registros == null || !registros.Any())
                return Result.Ok("Sin registros para validar");

            // Agrupar registros por semana
            var registrosPorSemana = registros
                .GroupBy(r => CultureInfo.CurrentCulture.Calendar.GetWeekOfYear(
                    r.Fecha, CalendarWeekRule.FirstDay, DayOfWeek.Monday))
                .ToList();

            foreach (var semana in registrosPorSemana)
            {
                var horasSemana = semana.Sum(r =>
                {
                    if (!r.HoraEntrada.HasValue || !r.HoraSalida.HasValue)
                        return 0;
                    return (r.HoraSalida.Value - r.HoraEntrada.Value).TotalHours;
                });

                if (horasSemana > config.HorasMaximasSemanales)
                {
                    var fechaInicioSemana = semana.Min(r => r.Fecha);
                    return Result.Fail(
                        $"La jornada de la semana del {fechaInicioSemana:dd/MM/yyyy} excede el límite legal de " +
                        $"{config.HorasMaximasSemanales} horas semanales. Horas trabajadas: {horasSemana:F1}. " +
                        $"Según Art. 161 del CST, la jornada máxima es de 48 horas semanales.");
                }
            }

            return Result.Ok("Jornada laboral dentro del límite legal");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al validar jornada laboral");
            return Result.Fail($"Error al validar jornada: {ex.Message}");
        }
    }

    /// <summary>
    /// Valida el formato de cédula colombiana.
    /// La cédula de ciudadanía colombiana tiene entre 6 y 10 dígitos numéricos.
    /// </summary>
    public Task<Result> ValidarCedulaColombiaAsync(string cedula)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(cedula))
                return Task.FromResult(Result.Fail("La cédula es requerida"));

            // Remover puntos, espacios y guiones
            cedula = cedula.Replace(".", "").Replace(" ", "").Replace("-", "").Trim();

            // Validar que sea numérica
            if (!long.TryParse(cedula, out var numeroCedula))
                return Task.FromResult(Result.Fail("La cédula debe contener solo números"));

            // Validar que no sea negativa
            if (numeroCedula <= 0)
                return Task.FromResult(Result.Fail("La cédula debe ser un número positivo"));

            // Validar longitud (6-10 dígitos para cédula de ciudadanía)
            if (cedula.Length < 6 || cedula.Length > 10)
                return Task.FromResult(Result.Fail(
                    $"La cédula de ciudadanía colombiana debe tener entre 6 y 10 dígitos. " +
                    $"Longitud ingresada: {cedula.Length} dígitos"));

            return Task.FromResult(Result.Ok("Formato de cédula válido"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al validar cédula");
            return Task.FromResult(Result.Fail($"Error al validar cédula: {ex.Message}"));
        }
    }

    /// <summary>
    /// Valida un contrato según la legislación colombiana.
    /// Verifica fechas, tipo de contrato, salario y requisitos legales.
    /// </summary>
    public async Task<Result> ValidarContratoAsync(Contrato contrato)
    {
        try
        {
            var errores = new List<string>();

            // 1. Validar fechas
            if (contrato.FechaInicio > DateTime.Now.AddYears(1))
                errores.Add("La fecha de inicio no puede ser mayor a un año en el futuro");

            if (contrato.FechaFin.HasValue && contrato.FechaFin < contrato.FechaInicio)
                errores.Add("La fecha de finalización no puede ser anterior a la fecha de inicio");

            // 2. Validar salario mínimo
            var resultSalario = await ValidarSalarioMinimoAsync(contrato.Salario);
            if (!resultSalario.IsSuccess)
                errores.Add(resultSalario.Error!);

            // 3. Validar tipo de contrato vs fechas
            switch (contrato.TipoContrato)
            {
                case TipoContrato.TerminoFijo:
                    if (!contrato.FechaFin.HasValue)
                        errores.Add("Los contratos a término fijo requieren fecha de finalización");
                    else
                    {
                        // Contratos a término fijo no pueden exceder 3 años (Art. 46 CST)
                        var duracion = (contrato.FechaFin.Value - contrato.FechaInicio).Days;
                        if (duracion > 365 * 3)
                            errores.Add("Los contratos a término fijo no pueden exceder 3 años según Art. 46 del CST");
                    }
                    break;

                case TipoContrato.Indefinido:
                    if (contrato.FechaFin.HasValue)
                        errores.Add("Los contratos a término indefinido no deben tener fecha de finalización");
                    break;

                case TipoContrato.ObraOLabor:
                    // Debe especificar la obra o labor en observaciones
                    break;

                case TipoContrato.Aprendizaje:
                    // Contratos de aprendizaje tienen regulación especial
                    break;
            }

            // 4. Validar período de prueba implícito
            // Para contratos indefinidos: máximo 2 meses
            // Para contratos a término fijo < 1 año: máximo 1/5 del término

            if (errores.Any())
            {
                return Result.Fail(string.Join(". ", errores));
            }

            return Result.Ok("Contrato válido según legislación colombiana");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al validar contrato");
            return Result.Fail($"Error al validar contrato: {ex.Message}");
        }
    }

    /// <summary>
    /// Valida si un empleado tiene derecho al auxilio de transporte.
    /// Según legislación colombiana, tienen derecho quienes ganan menos de 2 SMLMV.
    /// </summary>
    public async Task<Result<bool>> ValidarDerechoAuxilioTransporteAsync(decimal salario)
    {
        try
        {
            var config = await _configLegalRepository.GetVigenteAsync();
            if (config == null)
                return Result<bool>.Fail("No se encontró configuración legal vigente");

            var limiteSalario = config.SalarioMinimoMensual * 2;
            var tieneDerechoAuxilio = salario < limiteSalario;

            var mensaje = tieneDerechoAuxilio
                ? $"El empleado tiene derecho al auxilio de transporte ({config.AuxilioTransporte:C0}) " +
                  $"ya que su salario ({salario:C0}) es menor a 2 SMLMV ({limiteSalario:C0})"
                : $"El empleado NO tiene derecho al auxilio de transporte " +
                  $"ya que su salario ({salario:C0}) es igual o mayor a 2 SMLMV ({limiteSalario:C0})";

            return Result<bool>.Ok(tieneDerechoAuxilio, mensaje);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al validar derecho a auxilio de transporte");
            return Result<bool>.Fail($"Error al validar auxilio de transporte: {ex.Message}");
        }
    }

    /// <summary>
    /// Valida que las horas extras no excedan el máximo legal.
    /// Según Art. 22 Ley 50 de 1990: máximo 2 horas extras diarias, 12 semanales.
    /// </summary>
    public async Task<Result> ValidarHorasExtrasAsync(List<RegistroDiario> registros)
    {
        try
        {
            if (registros == null || !registros.Any())
                return Result.Ok("Sin registros para validar");

            const int maxHorasExtrasDiarias = 2;
            const int maxHorasExtrasSemanales = 12;

            // Validar horas extras diarias
            foreach (var registro in registros)
            {
                var totalHorasExtrasDia = registro.HorasExtrasDiurnas + registro.HorasExtrasNocturnas;
                if (totalHorasExtrasDia > maxHorasExtrasDiarias)
                {
                    return Result.Fail(
                        $"El día {registro.Fecha:dd/MM/yyyy} excede el límite de {maxHorasExtrasDiarias} horas extras diarias. " +
                        $"Horas extras registradas: {totalHorasExtrasDia:F1}. " +
                        $"Según Art. 22 de la Ley 50 de 1990.");
                }
            }

            // Validar horas extras semanales
            var registrosPorSemana = registros
                .GroupBy(r => CultureInfo.CurrentCulture.Calendar.GetWeekOfYear(
                    r.Fecha, CalendarWeekRule.FirstDay, DayOfWeek.Monday))
                .ToList();

            foreach (var semana in registrosPorSemana)
            {
                var totalHorasExtrasSemana = semana.Sum(r => r.HorasExtrasDiurnas + r.HorasExtrasNocturnas);
                if (totalHorasExtrasSemana > maxHorasExtrasSemanales)
                {
                    var fechaInicioSemana = semana.Min(r => r.Fecha);
                    return Result.Fail(
                        $"La semana del {fechaInicioSemana:dd/MM/yyyy} excede el límite de {maxHorasExtrasSemanales} horas extras semanales. " +
                        $"Horas extras totales: {totalHorasExtrasSemana:F1}. " +
                        $"Según Art. 22 de la Ley 50 de 1990.");
                }
            }

            return Result.Ok("Horas extras dentro del límite legal");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al validar horas extras");
            return Result.Fail($"Error al validar horas extras: {ex.Message}");
        }
    }

    /// <summary>
    /// Valida los porcentajes de aportes a seguridad social.
    /// Empleado: 4% salud + 4% pensión = 8%
    /// </summary>
    public async Task<Result> ValidarAportesAsync(decimal baseAportes, decimal salud, decimal pension)
    {
        try
        {
            var config = await _configLegalRepository.GetVigenteAsync();
            if (config == null)
                return Result.Fail("No se encontró configuración legal vigente");

            var saludEsperada = Math.Round(baseAportes * (config.PorcentajeSaludEmpleado / 100), 2);
            var pensionEsperada = Math.Round(baseAportes * (config.PorcentajePensionEmpleado / 100), 2);

            var errores = new List<string>();

            // Permitir una tolerancia de $1 por redondeos
            if (Math.Abs(salud - saludEsperada) > 1)
            {
                errores.Add(
                    $"Aporte a salud incorrecto. Esperado: {saludEsperada:C0} ({config.PorcentajeSaludEmpleado}%), " +
                    $"Registrado: {salud:C0}");
            }

            if (Math.Abs(pension - pensionEsperada) > 1)
            {
                errores.Add(
                    $"Aporte a pensión incorrecto. Esperado: {pensionEsperada:C0} ({config.PorcentajePensionEmpleado}%), " +
                    $"Registrado: {pension:C0}");
            }

            if (errores.Any())
                return Result.Fail(string.Join(". ", errores));

            return Result.Ok("Aportes a seguridad social correctos");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al validar aportes");
            return Result.Fail($"Error al validar aportes: {ex.Message}");
        }
    }
}
