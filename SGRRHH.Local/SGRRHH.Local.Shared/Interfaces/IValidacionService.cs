using SGRRHH.Local.Domain.Entities;
using SGRRHH.Local.Shared;

namespace SGRRHH.Local.Shared.Interfaces;

/// <summary>
/// Servicio de validaciones según legislación laboral colombiana
/// </summary>
public interface IValidacionService
{
    /// <summary>
    /// Valida que el salario no sea inferior al SMLMV
    /// </summary>
    Task<Result> ValidarSalarioMinimoAsync(decimal salario);
    
    /// <summary>
    /// Valida que el empleado tenga la edad mínima laboral (18 años)
    /// </summary>
    Task<Result> ValidarEdadLaboralAsync(DateTime fechaNacimiento);
    
    /// <summary>
    /// Valida que la jornada laboral no exceda las 48 horas semanales
    /// </summary>
    Task<Result> ValidarJornadaLaboralAsync(List<RegistroDiario> registros);
    
    /// <summary>
    /// Valida el formato de cédula colombiana
    /// </summary>
    Task<Result> ValidarCedulaColombiaAsync(string cedula);
    
    /// <summary>
    /// Valida un contrato según la legislación colombiana
    /// </summary>
    Task<Result> ValidarContratoAsync(Contrato contrato);
    
    /// <summary>
    /// Valida que un empleado puede recibir auxilio de transporte (salario < 2 SMLMV)
    /// </summary>
    Task<Result<bool>> ValidarDerechoAuxilioTransporteAsync(decimal salario);
    
    /// <summary>
    /// Valida que las horas extras no excedan el máximo legal (2 horas diarias, 12 semanales)
    /// </summary>
    Task<Result> ValidarHorasExtrasAsync(List<RegistroDiario> registros);
    
    /// <summary>
    /// Valida los porcentajes de seguridad social
    /// </summary>
    Task<Result> ValidarAportesAsync(decimal baseAportes, decimal salud, decimal pension);
}
