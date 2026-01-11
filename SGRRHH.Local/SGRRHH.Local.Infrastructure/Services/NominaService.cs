using Microsoft.Extensions.Logging;
using SGRRHH.Local.Domain.Entities;
using SGRRHH.Local.Domain.Enums;
using SGRRHH.Local.Shared;
using SGRRHH.Local.Shared.Interfaces;

namespace SGRRHH.Local.Infrastructure.Services;

/// <summary>
/// Servicio para generación y gestión de nómina según legislación colombiana.
/// Implementa cálculos de devengos, deducciones y aportes patronales.
/// </summary>
public class NominaService : INominaService
{
    private readonly IEmpleadoRepository _empleadoRepository;
    private readonly INominaRepository _nominaRepository;
    private readonly IConfiguracionLegalRepository _configLegalRepository;
    private readonly IRegistroDiarioRepository _registroDiarioRepository;
    private readonly IFestivoColombiaRepository _festivoRepository;
    private readonly ILogger<NominaService> _logger;

    public NominaService(
        IEmpleadoRepository empleadoRepository,
        INominaRepository nominaRepository,
        IConfiguracionLegalRepository configLegalRepository,
        IRegistroDiarioRepository registroDiarioRepository,
        IFestivoColombiaRepository festivoRepository,
        ILogger<NominaService> logger)
    {
        _empleadoRepository = empleadoRepository;
        _nominaRepository = nominaRepository;
        _configLegalRepository = configLegalRepository;
        _registroDiarioRepository = registroDiarioRepository;
        _festivoRepository = festivoRepository;
        _logger = logger;
    }

    /// <summary>
    /// Calcula la nómina de un empleado para un período específico
    /// </summary>
    public async Task<Result<Nomina>> CalcularNominaAsync(int empleadoId, DateTime periodo)
    {
        try
        {
            var empleado = await _empleadoRepository.GetByIdAsync(empleadoId);
            if (empleado == null)
                return Result<Nomina>.Fail("Empleado no encontrado");

            if (empleado.Estado != EstadoEmpleado.Activo)
                return Result<Nomina>.Fail("El empleado no está activo");

            var configLegal = await _configLegalRepository.GetVigenteAsync();
            if (configLegal == null)
                return Result<Nomina>.Fail("Configuración legal no encontrada");

            // Verificar si ya existe nómina para este período
            var nominaExistente = await _nominaRepository.GetByEmpleadoAndPeriodoAsync(empleadoId, periodo);

            var nomina = nominaExistente ?? new Nomina
            {
                EmpleadoId = empleadoId,
                Periodo = new DateTime(periodo.Year, periodo.Month, 1),
                Estado = EstadoNomina.Borrador,
                FechaCreacion = DateTime.Now
            };

            // 1. SALARIO BASE
            nomina.SalarioBase = empleado.SalarioBase ?? 0;

            // 2. AUXILIO DE TRANSPORTE (si aplica)
            // Empleados con salario menor a 2 SMLMV tienen derecho al auxilio
            if (nomina.SalarioBase < (configLegal.SalarioMinimoMensual * 2))
            {
                nomina.AuxilioTransporte = configLegal.AuxilioTransporte;
            }
            else
            {
                nomina.AuxilioTransporte = 0;
            }

            // 3. HORAS EXTRAS Y RECARGOS
            var horasExtrasResult = await CalcularHorasExtrasAsync(empleadoId, periodo);
            if (horasExtrasResult.IsSuccess)
            {
                // Los valores de horas extras ya vienen calculados con los recargos
                var registros = await GetRegistrosDiariosPeriodoAsync(empleadoId, periodo);
                var valorHora = nomina.SalarioBase / 240m; // 30 días × 8 horas

                decimal horasExtrasDiurnas = 0, horasExtrasNocturnas = 0, horasNocturnas = 0, horasDominicalesFestivos = 0;

                foreach (var registro in registros)
                {
                    horasExtrasDiurnas += registro.HorasExtrasDiurnas;
                    horasExtrasNocturnas += registro.HorasExtrasNocturnas;
                    horasNocturnas += registro.HorasNocturnas;
                    horasDominicalesFestivos += registro.HorasDominicalesFestivos;
                }

                // Aplicar recargos según legislación colombiana
                nomina.HorasExtrasDiurnas = Math.Round(horasExtrasDiurnas * valorHora * (1 + configLegal.RecargoHoraExtraDiurna / 100), 2);
                nomina.HorasExtrasNocturnas = Math.Round(horasExtrasNocturnas * valorHora * (1 + configLegal.RecargoHoraExtraNocturna / 100), 2);
                nomina.HorasNocturnas = Math.Round(horasNocturnas * valorHora * (configLegal.RecargoHoraNocturna / 100), 2); // Solo el recargo
                nomina.HorasDominicalesFestivos = Math.Round(horasDominicalesFestivos * valorHora * (1 + configLegal.RecargoHoraDominicalFestivo / 100), 2);
            }

            // 4. DEDUCCIONES DEL EMPLEADO
            // Base para aportes: Salario base (NO incluye auxilio de transporte)
            var baseAportes = nomina.SalarioBase;

            // Salud empleado: 4%
            nomina.DeduccionSalud = Math.Round(baseAportes * (configLegal.PorcentajeSaludEmpleado / 100), 2);

            // Pensión empleado: 4%
            nomina.DeduccionPension = Math.Round(baseAportes * (configLegal.PorcentajePensionEmpleado / 100), 2);

            // Retención en la fuente (simplificado - debería calcularse según tabla)
            // Por ahora se deja en 0, se debe implementar tabla de retención
            nomina.RetencionFuente = 0;

            // 5. APORTES PATRONALES (No se descuentan al empleado)
            // Salud empleador: 8.5%
            nomina.AporteSaludEmpleador = Math.Round(baseAportes * (configLegal.PorcentajeSaludEmpleador / 100), 2);

            // Pensión empleador: 12%
            nomina.AportePensionEmpleador = Math.Round(baseAportes * (configLegal.PorcentajePensionEmpleador / 100), 2);

            // ARL según clase de riesgo (I-V)
            var claseRiesgo = Math.Clamp(empleado.ClaseRiesgoARL, 1, 5);
            var porcentajeARL = GetPorcentajeARL(claseRiesgo, configLegal);
            nomina.AporteARL = Math.Round(baseAportes * (porcentajeARL / 100), 2);

            // Parafiscales
            nomina.AporteCajaCompensacion = Math.Round(baseAportes * (configLegal.PorcentajeCajaCompensacion / 100), 2);
            nomina.AporteICBF = Math.Round(baseAportes * (configLegal.PorcentajeICBF / 100), 2);
            nomina.AporteSENA = Math.Round(baseAportes * (configLegal.PorcentajeSENA / 100), 2);

            // 6. ESTADO
            nomina.Estado = EstadoNomina.Calculada;
            nomina.FechaModificacion = DateTime.Now;

            // Guardar o actualizar
            if (nominaExistente != null)
            {
                await _nominaRepository.UpdateAsync(nomina);
            }
            else
            {
                await _nominaRepository.AddAsync(nomina);
            }

            _logger.LogInformation(
                "Nómina calculada: EmpleadoId={EmpleadoId}, Periodo={Periodo}, Neto={Neto}",
                empleadoId, periodo.ToString("yyyy-MM"), nomina.NetoPagar);

            return Result<Nomina>.Ok(nomina);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al calcular nómina para empleado {EmpleadoId}", empleadoId);
            return Result<Nomina>.Fail($"Error al calcular nómina: {ex.Message}");
        }
    }

    /// <summary>
    /// Genera la nómina masiva para múltiples empleados
    /// </summary>
    public async Task<Result<int>> GenerarNominaMasivaAsync(DateTime periodo, List<int> empleadosIds)
    {
        try
        {
            int nominasGeneradas = 0;
            var errores = new List<string>();

            foreach (var empleadoId in empleadosIds)
            {
                var resultado = await CalcularNominaAsync(empleadoId, periodo);
                if (resultado.IsSuccess)
                {
                    nominasGeneradas++;
                }
                else
                {
                    errores.Add($"Empleado {empleadoId}: {resultado.Error}");
                }
            }

            if (errores.Any())
            {
                _logger.LogWarning("Nómina masiva con errores: {Errores}", string.Join("; ", errores));
            }

            _logger.LogInformation(
                "Nómina masiva generada: Periodo={Periodo}, Generadas={Generadas}, Errores={Errores}",
                periodo.ToString("yyyy-MM"), nominasGeneradas, errores.Count);

            return Result<int>.Ok(nominasGeneradas, 
                errores.Any() ? $"Se generaron {nominasGeneradas} nóminas. Errores: {errores.Count}" : null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al generar nómina masiva");
            return Result<int>.Fail($"Error al generar nómina masiva: {ex.Message}");
        }
    }

    /// <summary>
    /// Genera la nómina para todos los empleados activos
    /// </summary>
    public async Task<Result<int>> GenerarNominaTodosEmpleadosAsync(DateTime periodo)
    {
        try
        {
            var empleadosActivos = await _empleadoRepository.GetByEstadoAsync(EstadoEmpleado.Activo);
            var empleadosIds = empleadosActivos.Select(e => e.Id).ToList();

            return await GenerarNominaMasivaAsync(periodo, empleadosIds);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al generar nómina para todos los empleados");
            return Result<int>.Fail($"Error al generar nómina: {ex.Message}");
        }
    }

    /// <summary>
    /// Aprueba una nómina específica
    /// </summary>
    public async Task<Result<Nomina>> AprobarNominaAsync(int nominaId, int aprobadorId)
    {
        try
        {
            var nomina = await _nominaRepository.GetByIdAsync(nominaId);
            if (nomina == null)
                return Result<Nomina>.Fail("Nómina no encontrada");

            if (nomina.Estado != EstadoNomina.Calculada)
                return Result<Nomina>.Fail("Solo se pueden aprobar nóminas en estado 'Calculada'");

            nomina.Estado = EstadoNomina.Aprobada;
            nomina.AprobadoPorId = aprobadorId;
            nomina.FechaAprobacion = DateTime.Now;
            nomina.FechaModificacion = DateTime.Now;

            await _nominaRepository.UpdateAsync(nomina);

            _logger.LogInformation("Nómina aprobada: ID={Id}, AprobadoPor={AprobadorId}", nominaId, aprobadorId);

            return Result<Nomina>.Ok(nomina);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al aprobar nómina {NominaId}", nominaId);
            return Result<Nomina>.Fail($"Error al aprobar nómina: {ex.Message}");
        }
    }

    /// <summary>
    /// Aprueba todas las nóminas de un período
    /// </summary>
    public async Task<Result<int>> AprobarNominasPeriodoAsync(DateTime periodo, int aprobadorId)
    {
        try
        {
            var nominas = await _nominaRepository.GetByPeriodoAsync(periodo);
            var nominasCalculadas = nominas.Where(n => n.Estado == EstadoNomina.Calculada).ToList();
            int aprobadas = 0;

            foreach (var nomina in nominasCalculadas)
            {
                var resultado = await AprobarNominaAsync(nomina.Id, aprobadorId);
                if (resultado.IsSuccess)
                    aprobadas++;
            }

            _logger.LogInformation(
                "Nóminas del período aprobadas: Periodo={Periodo}, Aprobadas={Aprobadas}",
                periodo.ToString("yyyy-MM"), aprobadas);

            return Result<int>.Ok(aprobadas);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al aprobar nóminas del período {Periodo}", periodo);
            return Result<int>.Fail($"Error al aprobar nóminas: {ex.Message}");
        }
    }

    /// <summary>
    /// Genera el desprendible de pago en PDF
    /// </summary>
    public async Task<Result<byte[]>> GenerarDesprendiblePagoAsync(int nominaId)
    {
        try
        {
            var nomina = await _nominaRepository.GetByIdAsync(nominaId);
            if (nomina == null)
                return Result<byte[]>.Fail("Nómina no encontrada");

            var empleado = await _empleadoRepository.GetByIdAsync(nomina.EmpleadoId);
            if (empleado == null)
                return Result<byte[]>.Fail("Empleado no encontrado");

            // TODO: Implementar generación de PDF con QuestPDF
            // Por ahora retornamos un placeholder
            _logger.LogWarning("Generación de desprendible PDF no implementada completamente");

            return Result<byte[]>.Fail("Funcionalidad de generación de PDF en desarrollo");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al generar desprendible de pago para nómina {NominaId}", nominaId);
            return Result<byte[]>.Fail($"Error al generar desprendible: {ex.Message}");
        }
    }

    /// <summary>
    /// Obtiene las nóminas pendientes de aprobación
    /// </summary>
    public async Task<Result<List<Nomina>>> GetNominasPendientesAsync()
    {
        try
        {
            var nominas = await _nominaRepository.GetPendientesAprobacionAsync();
            return Result<List<Nomina>>.Ok(nominas.ToList());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener nóminas pendientes");
            return Result<List<Nomina>>.Fail($"Error al obtener nóminas pendientes: {ex.Message}");
        }
    }

    /// <summary>
    /// Recalcula una nómina existente
    /// </summary>
    public async Task<Result<Nomina>> RecalcularNominaAsync(int nominaId)
    {
        try
        {
            var nomina = await _nominaRepository.GetByIdAsync(nominaId);
            if (nomina == null)
                return Result<Nomina>.Fail("Nómina no encontrada");

            if (nomina.Estado == EstadoNomina.Pagada || nomina.Estado == EstadoNomina.Contabilizada)
                return Result<Nomina>.Fail("No se puede recalcular una nómina pagada o contabilizada");

            return await CalcularNominaAsync(nomina.EmpleadoId, nomina.Periodo);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al recalcular nómina {NominaId}", nominaId);
            return Result<Nomina>.Fail($"Error al recalcular nómina: {ex.Message}");
        }
    }

    /// <summary>
    /// Calcula el valor de horas extras según recargos legales
    /// </summary>
    public async Task<Result<decimal>> CalcularHorasExtrasAsync(int empleadoId, DateTime periodo)
    {
        try
        {
            var registros = await GetRegistrosDiariosPeriodoAsync(empleadoId, periodo);
            if (!registros.Any())
                return Result<decimal>.Ok(0);

            var empleado = await _empleadoRepository.GetByIdAsync(empleadoId);
            if (empleado == null)
                return Result<decimal>.Fail("Empleado no encontrado");

            var configLegal = await _configLegalRepository.GetVigenteAsync();
            if (configLegal == null)
                return Result<decimal>.Fail("Configuración legal no encontrada");

            var valorHora = (empleado.SalarioBase ?? 0) / 240m;
            decimal totalHorasExtras = 0;

            foreach (var registro in registros)
            {
                // Hora extra diurna: Recargo 25%
                totalHorasExtras += registro.HorasExtrasDiurnas * valorHora * (1 + configLegal.RecargoHoraExtraDiurna / 100);
                
                // Hora extra nocturna: Recargo 75%
                totalHorasExtras += registro.HorasExtrasNocturnas * valorHora * (1 + configLegal.RecargoHoraExtraNocturna / 100);
                
                // Hora nocturna ordinaria: Solo recargo 35% (la hora base ya está pagada)
                totalHorasExtras += registro.HorasNocturnas * valorHora * (configLegal.RecargoHoraNocturna / 100);
                
                // Hora dominical/festivo: Recargo 75%
                totalHorasExtras += registro.HorasDominicalesFestivos * valorHora * (1 + configLegal.RecargoHoraDominicalFestivo / 100);
            }

            return Result<decimal>.Ok(Math.Round(totalHorasExtras, 2));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al calcular horas extras para empleado {EmpleadoId}", empleadoId);
            return Result<decimal>.Fail($"Error al calcular horas extras: {ex.Message}");
        }
    }

    #region Métodos privados auxiliares

    /// <summary>
    /// Obtiene el porcentaje de ARL según la clase de riesgo
    /// </summary>
    private static decimal GetPorcentajeARL(int claseRiesgo, ConfiguracionLegal config)
    {
        // Porcentajes aproximados según clase de riesgo laboral en Colombia
        return claseRiesgo switch
        {
            1 => config.ARLClase1Min,           // Clase I: 0.522%
            2 => 1.044m,                         // Clase II: 1.044%
            3 => 2.436m,                         // Clase III: 2.436%
            4 => 4.350m,                         // Clase IV: 4.350%
            5 => config.ARLClase5Max,           // Clase V: 6.960%
            _ => config.ARLClase1Min
        };
    }

    /// <summary>
    /// Obtiene los registros diarios de un empleado para un período (mes)
    /// </summary>
    private async Task<IEnumerable<RegistroDiario>> GetRegistrosDiariosPeriodoAsync(int empleadoId, DateTime periodo)
    {
        var fechaInicio = new DateTime(periodo.Year, periodo.Month, 1);
        var fechaFin = fechaInicio.AddMonths(1).AddDays(-1);

        return await _registroDiarioRepository.GetByEmpleadoRangoFechasAsync(empleadoId, fechaInicio, fechaFin);
    }

    #endregion
}
