using Microsoft.Extensions.Logging;
using SGRRHH.Local.Domain.Entities;
using SGRRHH.Local.Domain.Enums;
using SGRRHH.Local.Shared;
using SGRRHH.Local.Shared.Interfaces;

namespace SGRRHH.Local.Infrastructure.Services;

/// <summary>
/// Servicio para cálculo de liquidación de prestaciones sociales según legislación colombiana.
/// Basado en el Código Sustantivo del Trabajo (CST) y normativa vigente.
/// </summary>
public class LiquidacionService : ILiquidacionService
{
    private readonly IEmpleadoRepository _empleadoRepository;
    private readonly IContratoRepository _contratoRepository;
    private readonly IConfiguracionLegalRepository _configLegalRepository;
    private readonly IPrestacionRepository _prestacionRepository;
    private readonly ILogger<LiquidacionService> _logger;

    public LiquidacionService(
        IEmpleadoRepository empleadoRepository,
        IContratoRepository contratoRepository,
        IConfiguracionLegalRepository configLegalRepository,
        IPrestacionRepository prestacionRepository,
        ILogger<LiquidacionService> logger)
    {
        _empleadoRepository = empleadoRepository;
        _contratoRepository = contratoRepository;
        _configLegalRepository = configLegalRepository;
        _prestacionRepository = prestacionRepository;
        _logger = logger;
    }

    /// <summary>
    /// Calcula las cesantías según la legislación colombiana.
    /// Fórmula: (Salario mensual × Días trabajados) / 360
    /// El salario incluye auxilio de transporte si el empleado gana menos de 2 SMLMV.
    /// </summary>
    public async Task<Result<decimal>> CalcularCesantiasAsync(int empleadoId, DateTime fechaInicio, DateTime fechaFin)
    {
        try
        {
            var empleado = await _empleadoRepository.GetByIdAsync(empleadoId);
            if (empleado == null)
                return Result<decimal>.Fail("Empleado no encontrado");

            var configLegal = await _configLegalRepository.GetVigenteAsync();
            if (configLegal == null)
                return Result<decimal>.Fail("Configuración legal no encontrada");

            // Calcular días trabajados en el período
            var diasTrabajados = (fechaFin - fechaInicio).Days;
            if (diasTrabajados < 0)
                return Result<decimal>.Fail("La fecha fin debe ser posterior a la fecha inicio");

            // Obtener salario base para prestaciones (incluye auxilio si aplica)
            var salarioResult = await GetSalarioBaseParaPrestacionesAsync(empleadoId);
            if (!salarioResult.IsSuccess)
                return Result<decimal>.Fail(salarioResult.Error!);

            var salarioMensual = salarioResult.Value;

            // Fórmula de cesantías: (Salario × Días) / 360
            var cesantias = Math.Round((salarioMensual * diasTrabajados) / 360, 2);

            _logger.LogInformation(
                "Cesantías calculadas: EmpleadoId={EmpleadoId}, Días={Dias}, Salario={Salario}, Cesantías={Cesantias}",
                empleadoId, diasTrabajados, salarioMensual, cesantias);

            return Result<decimal>.Ok(cesantias);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al calcular cesantías para empleado {EmpleadoId}", empleadoId);
            return Result<decimal>.Fail($"Error al calcular cesantías: {ex.Message}");
        }
    }

    /// <summary>
    /// Calcula los intereses sobre cesantías (12% anual).
    /// Fórmula: (Cesantías × Días × 0.12) / 360
    /// </summary>
    public async Task<Result<decimal>> CalcularInteresesCesantiasAsync(int empleadoId, int año)
    {
        try
        {
            var empleado = await _empleadoRepository.GetByIdAsync(empleadoId);
            if (empleado == null)
                return Result<decimal>.Fail("Empleado no encontrado");

            var configLegal = await _configLegalRepository.GetVigenteAsync();
            if (configLegal == null)
                return Result<decimal>.Fail("Configuración legal no encontrada");

            // Buscar las cesantías calculadas para el año
            var cesantiasPrestacion = await _prestacionRepository.GetByEmpleadoAndPeriodoAsync(
                empleadoId, año, TipoPrestacion.Cesantias);

            decimal valorCesantias;
            int diasParaIntereses;

            if (cesantiasPrestacion != null)
            {
                valorCesantias = cesantiasPrestacion.ValorCalculado;
                diasParaIntereses = cesantiasPrestacion.DiasProporcionales ?? 360;
            }
            else
            {
                // Si no hay cesantías registradas, calcular para el año completo
                var fechaInicio = new DateTime(año, 1, 1);
                var fechaFin = new DateTime(año, 12, 31);

                // Ajustar fechas según fecha de ingreso del empleado
                if (empleado.FechaIngreso > fechaInicio)
                    fechaInicio = empleado.FechaIngreso;
                if (empleado.FechaRetiro.HasValue && empleado.FechaRetiro < fechaFin)
                    fechaFin = empleado.FechaRetiro.Value;

                var cesantiasResult = await CalcularCesantiasAsync(empleadoId, fechaInicio, fechaFin);
                if (!cesantiasResult.IsSuccess)
                    return Result<decimal>.Fail(cesantiasResult.Error!);

                valorCesantias = cesantiasResult.Value;
                diasParaIntereses = (fechaFin - fechaInicio).Days;
            }

            // Fórmula de intereses: (Cesantías × Días × 12%) / 360
            var porcentajeIntereses = configLegal.PorcentajeInteresesCesantias / 100m;
            var intereses = Math.Round((valorCesantias * diasParaIntereses * porcentajeIntereses) / 360, 2);

            _logger.LogInformation(
                "Intereses cesantías calculados: EmpleadoId={EmpleadoId}, Cesantías={Cesantias}, Días={Dias}, Intereses={Intereses}",
                empleadoId, valorCesantias, diasParaIntereses, intereses);

            return Result<decimal>.Ok(intereses);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al calcular intereses cesantías para empleado {EmpleadoId}", empleadoId);
            return Result<decimal>.Fail($"Error al calcular intereses sobre cesantías: {ex.Message}");
        }
    }

    /// <summary>
    /// Calcula la prima de servicios.
    /// Fórmula: (Salario mensual × Días trabajados) / 360
    /// Se paga en junio (primer semestre) y diciembre (segundo semestre)
    /// </summary>
    public async Task<Result<decimal>> CalcularPrimaServiciosAsync(int empleadoId, DateTime fechaInicio, DateTime fechaFin)
    {
        try
        {
            var empleado = await _empleadoRepository.GetByIdAsync(empleadoId);
            if (empleado == null)
                return Result<decimal>.Fail("Empleado no encontrado");

            var diasTrabajados = (fechaFin - fechaInicio).Days;
            if (diasTrabajados < 0)
                return Result<decimal>.Fail("La fecha fin debe ser posterior a la fecha inicio");

            // Obtener salario base para prestaciones (incluye auxilio si aplica)
            var salarioResult = await GetSalarioBaseParaPrestacionesAsync(empleadoId);
            if (!salarioResult.IsSuccess)
                return Result<decimal>.Fail(salarioResult.Error!);

            var salarioMensual = salarioResult.Value;

            // Fórmula de prima: (Salario × Días) / 360
            var prima = Math.Round((salarioMensual * diasTrabajados) / 360, 2);

            _logger.LogInformation(
                "Prima de servicios calculada: EmpleadoId={EmpleadoId}, Días={Dias}, Salario={Salario}, Prima={Prima}",
                empleadoId, diasTrabajados, salarioMensual, prima);

            return Result<decimal>.Ok(prima);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al calcular prima de servicios para empleado {EmpleadoId}", empleadoId);
            return Result<decimal>.Fail($"Error al calcular prima de servicios: {ex.Message}");
        }
    }

    /// <summary>
    /// Calcula las vacaciones proporcionales.
    /// Fórmula: (Salario mensual × Días causados) / 720
    /// 15 días hábiles por año = factor 720 (24 meses × 30 días)
    /// </summary>
    public async Task<Result<decimal>> CalcularVacacionesProporcionalesAsync(int empleadoId, DateTime fechaInicio, DateTime fechaFin)
    {
        try
        {
            var empleado = await _empleadoRepository.GetByIdAsync(empleadoId);
            if (empleado == null)
                return Result<decimal>.Fail("Empleado no encontrado");

            var diasTrabajados = (fechaFin - fechaInicio).Days;
            if (diasTrabajados < 0)
                return Result<decimal>.Fail("La fecha fin debe ser posterior a la fecha inicio");

            var salarioMensual = empleado.SalarioBase ?? 0;
            if (salarioMensual <= 0)
                return Result<decimal>.Fail("El empleado no tiene salario base configurado");

            // Fórmula de vacaciones: (Salario × Días) / 720
            // Nota: Para vacaciones NO se incluye el auxilio de transporte
            var vacaciones = Math.Round((salarioMensual * diasTrabajados) / 720, 2);

            _logger.LogInformation(
                "Vacaciones proporcionales calculadas: EmpleadoId={EmpleadoId}, Días={Dias}, Salario={Salario}, Vacaciones={Vacaciones}",
                empleadoId, diasTrabajados, salarioMensual, vacaciones);

            return Result<decimal>.Ok(vacaciones);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al calcular vacaciones proporcionales para empleado {EmpleadoId}", empleadoId);
            return Result<decimal>.Fail($"Error al calcular vacaciones proporcionales: {ex.Message}");
        }
    }

    /// <summary>
    /// Calcula la indemnización por despido sin justa causa según tipo de contrato.
    /// Según Art. 64 del CST:
    /// - Contrato indefinido < 1 año: 30 días de salario
    /// - Contrato indefinido > 1 año: 30 días + 20 días por cada año adicional
    /// - Contrato a término fijo: Días restantes del contrato
    /// </summary>
    public async Task<Result<decimal>> CalcularIndemnizacionAsync(int empleadoId, int contratoId, MotivoTerminacionContrato motivo)
    {
        try
        {
            // Solo aplica para despido sin justa causa o terminación del trabajador con justa causa
            if (motivo != MotivoTerminacionContrato.DespidoSinJustaCausa &&
                motivo != MotivoTerminacionContrato.TerminacionTrabajadorJustaCausa)
            {
                _logger.LogInformation("No aplica indemnización para motivo {Motivo}", motivo);
                return Result<decimal>.Ok(0);
            }

            var contrato = await _contratoRepository.GetByIdAsync(contratoId);
            if (contrato == null)
                return Result<decimal>.Fail("Contrato no encontrado");

            var empleado = await _empleadoRepository.GetByIdAsync(empleadoId);
            if (empleado == null)
                return Result<decimal>.Fail("Empleado no encontrado");

            var fechaTerminacion = contrato.FechaTerminacion ?? DateTime.Now;
            var diasTrabajados = (fechaTerminacion - contrato.FechaInicio).Days;
            var salarioMensual = contrato.Salario;
            var salarioDiario = salarioMensual / 30;

            decimal indemnizacion = 0;

            switch (contrato.TipoContrato)
            {
                case TipoContrato.Indefinido:
                    if (diasTrabajados < 365)
                    {
                        // Menos de 1 año: 30 días de salario
                        indemnizacion = salarioDiario * 30;
                    }
                    else
                    {
                        // Más de 1 año: 30 días por primer año + 20 días por cada año adicional
                        var añosCompletos = diasTrabajados / 365;
                        var diasPrimerAño = 30;
                        var diasAdicionales = (añosCompletos - 1) * 20;
                        indemnizacion = salarioDiario * (diasPrimerAño + diasAdicionales);
                    }
                    break;

                case TipoContrato.TerminoFijo:
                case TipoContrato.ObraOLabor:
                    // Contrato a término fijo: Días restantes del contrato
                    if (contrato.FechaFin.HasValue && contrato.FechaFin > fechaTerminacion)
                    {
                        var diasRestantes = (contrato.FechaFin.Value - fechaTerminacion).Days;
                        indemnizacion = salarioDiario * diasRestantes;
                    }
                    break;

                case TipoContrato.Aprendizaje:
                    // Los contratos de aprendizaje no generan indemnización
                    indemnizacion = 0;
                    break;
            }

            indemnizacion = Math.Round(indemnizacion, 2);

            _logger.LogInformation(
                "Indemnización calculada: EmpleadoId={EmpleadoId}, TipoContrato={TipoContrato}, Días={Dias}, Indemnización={Indemnizacion}",
                empleadoId, contrato.TipoContrato, diasTrabajados, indemnizacion);

            return Result<decimal>.Ok(indemnizacion);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al calcular indemnización para empleado {EmpleadoId}", empleadoId);
            return Result<decimal>.Fail($"Error al calcular indemnización: {ex.Message}");
        }
    }

    /// <summary>
    /// Genera la liquidación final completa de un empleado
    /// </summary>
    public async Task<Result<LiquidacionCompleta>> GenerarLiquidacionFinalAsync(int empleadoId, DateTime fechaRetiro, MotivoTerminacionContrato motivo)
    {
        try
        {
            var empleado = await _empleadoRepository.GetByIdAsync(empleadoId);
            if (empleado == null)
                return Result<LiquidacionCompleta>.Fail("Empleado no encontrado");

            var contrato = await _contratoRepository.GetContratoActivoByEmpleadoIdAsync(empleadoId);
            if (contrato == null)
                return Result<LiquidacionCompleta>.Fail("El empleado no tiene un contrato activo");

            var fechaIngreso = empleado.FechaIngreso;
            var diasLaborados = (fechaRetiro - fechaIngreso).Days;
            var año = fechaRetiro.Year;

            // Calcular cada componente de la liquidación
            var cesantiasResult = await CalcularCesantiasAsync(empleadoId, fechaIngreso, fechaRetiro);
            var interesesResult = await CalcularInteresesCesantiasAsync(empleadoId, año);
            
            // Prima: Calcular proporcional desde inicio de semestre
            var inicioSemestre = fechaRetiro.Month <= 6 
                ? new DateTime(año, 1, 1) 
                : new DateTime(año, 7, 1);
            if (fechaIngreso > inicioSemestre)
                inicioSemestre = fechaIngreso;
            var primaResult = await CalcularPrimaServiciosAsync(empleadoId, inicioSemestre, fechaRetiro);
            
            var vacacionesResult = await CalcularVacacionesProporcionalesAsync(empleadoId, fechaIngreso, fechaRetiro);
            var indemnizacionResult = await CalcularIndemnizacionAsync(empleadoId, contrato.Id, motivo);

            var liquidacion = new LiquidacionCompleta
            {
                EmpleadoId = empleadoId,
                EmpleadoNombre = empleado.NombreCompleto,
                ContratoId = contrato.Id,
                FechaIngreso = fechaIngreso,
                FechaRetiro = fechaRetiro,
                SalarioBase = empleado.SalarioBase ?? 0,
                Motivo = motivo,
                DiasLaborados = diasLaborados,
                FechaCalculo = DateTime.Now,
                Cesantias = cesantiasResult.IsSuccess ? cesantiasResult.Value : 0,
                InteresesCesantias = interesesResult.IsSuccess ? interesesResult.Value : 0,
                PrimaServicios = primaResult.IsSuccess ? primaResult.Value : 0,
                VacacionesProporcionales = vacacionesResult.IsSuccess ? vacacionesResult.Value : 0,
                Indemnizacion = indemnizacionResult.IsSuccess ? indemnizacionResult.Value : 0,
            };

            _logger.LogInformation(
                "Liquidación final generada: EmpleadoId={EmpleadoId}, Total={Total}",
                empleadoId, liquidacion.TotalLiquidacion);

            return Result<LiquidacionCompleta>.Ok(liquidacion);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al generar liquidación final para empleado {EmpleadoId}", empleadoId);
            return Result<LiquidacionCompleta>.Fail($"Error al generar liquidación final: {ex.Message}");
        }
    }

    /// <summary>
    /// Calcula el salario base para prestaciones (incluye auxilio de transporte si aplica)
    /// </summary>
    public async Task<Result<decimal>> GetSalarioBaseParaPrestacionesAsync(int empleadoId)
    {
        try
        {
            var empleado = await _empleadoRepository.GetByIdAsync(empleadoId);
            if (empleado == null)
                return Result<decimal>.Fail("Empleado no encontrado");

            var configLegal = await _configLegalRepository.GetVigenteAsync();
            if (configLegal == null)
                return Result<decimal>.Fail("Configuración legal no encontrada");

            var salarioMensual = empleado.SalarioBase ?? 0;
            if (salarioMensual <= 0)
                return Result<decimal>.Fail("El empleado no tiene salario base configurado");

            // Incluir auxilio de transporte si el salario es menor a 2 SMLMV
            if (salarioMensual < (configLegal.SalarioMinimoMensual * 2))
            {
                salarioMensual += configLegal.AuxilioTransporte;
            }

            return Result<decimal>.Ok(salarioMensual);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener salario base para prestaciones del empleado {EmpleadoId}", empleadoId);
            return Result<decimal>.Fail($"Error al obtener salario base: {ex.Message}");
        }
    }
}
