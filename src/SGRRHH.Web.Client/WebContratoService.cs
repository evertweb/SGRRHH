using SGRRHH.Core.Common;
using SGRRHH.Core.Entities;
using SGRRHH.Core.Enums;
using SGRRHH.Core.Interfaces;

namespace SGRRHH.Web.Client;

public class WebContratoService : IContratoService
{
    private readonly IContratoRepository _contratoRepo;

    public WebContratoService(IContratoRepository contratoRepo)
    {
        _contratoRepo = contratoRepo;
    }

    public async Task<ServiceResult<IEnumerable<Contrato>>> GetByEmpleadoIdAsync(int empleadoId)
    {
        try
        {
            var contratos = await _contratoRepo.GetByEmpleadoIdAsync(empleadoId);
            return ServiceResult<IEnumerable<Contrato>>.Ok(contratos);
        }
        catch (Exception ex)
        {
            return ServiceResult<IEnumerable<Contrato>>.Fail($"Error: {ex.Message}");
        }
    }

    public async Task<ServiceResult<Contrato>> GetByIdAsync(int id)
    {
        try
        {
            var contrato = await _contratoRepo.GetByIdAsync(id);
            if (contrato == null)
                return ServiceResult<Contrato>.Fail("Contrato no encontrado");
            return ServiceResult<Contrato>.Ok(contrato);
        }
        catch (Exception ex)
        {
            return ServiceResult<Contrato>.Fail($"Error: {ex.Message}");
        }
    }

    public async Task<ServiceResult<Contrato>> CreateAsync(Contrato contrato)
    {
        try
        {
            contrato.FechaCreacion = DateTime.Now;
            contrato.Activo = true;
            contrato.Estado = EstadoContrato.Activo;
            await _contratoRepo.AddAsync(contrato);
            return ServiceResult<Contrato>.Ok(contrato, "Contrato creado exitosamente");
        }
        catch (Exception ex)
        {
            return ServiceResult<Contrato>.Fail($"Error: {ex.Message}");
        }
    }

    public async Task<ServiceResult<Contrato>> UpdateAsync(Contrato contrato)
    {
        try
        {
            contrato.FechaModificacion = DateTime.Now;
            await _contratoRepo.UpdateAsync(contrato);
            return ServiceResult<Contrato>.Ok(contrato, "Contrato actualizado exitosamente");
        }
        catch (Exception ex)
        {
            return ServiceResult<Contrato>.Fail($"Error: {ex.Message}");
        }
    }

    public async Task<ServiceResult<bool>> DeleteAsync(int id)
    {
        try
        {
            var contrato = await _contratoRepo.GetByIdAsync(id);
            if (contrato == null)
                return ServiceResult<bool>.Fail("Contrato no encontrado");

            await _contratoRepo.DeleteAsync(id);
            return ServiceResult<bool>.Ok(true, "Contrato eliminado");
        }
        catch (Exception ex)
        {
            return ServiceResult<bool>.Fail($"Error: {ex.Message}");
        }
    }

    public async Task<ServiceResult<Contrato?>> GetContratoActivoAsync(int empleadoId)
    {
        try
        {
            var contrato = await _contratoRepo.GetContratoActivoByEmpleadoIdAsync(empleadoId);
            return ServiceResult<Contrato?>.Ok(contrato);
        }
        catch (Exception ex)
        {
            return ServiceResult<Contrato?>.Fail($"Error: {ex.Message}");
        }
    }

    public async Task<ServiceResult<Contrato>> RenovarContratoAsync(int contratoActualId, Contrato nuevoContrato)
    {
        try
        {
            var contratoActual = await _contratoRepo.GetByIdAsync(contratoActualId);
            if (contratoActual == null)
                return ServiceResult<Contrato>.Fail("Contrato actual no encontrado");

            // Mark old contract as renewed
            contratoActual.Estado = EstadoContrato.Renovado;
            contratoActual.FechaModificacion = DateTime.Now;
            await _contratoRepo.UpdateAsync(contratoActual);

            // Create new contract
            nuevoContrato.FechaCreacion = DateTime.Now;
            nuevoContrato.Estado = EstadoContrato.Activo;
            nuevoContrato.Activo = true;
            await _contratoRepo.AddAsync(nuevoContrato);

            return ServiceResult<Contrato>.Ok(nuevoContrato, "Contrato renovado exitosamente");
        }
        catch (Exception ex)
        {
            return ServiceResult<Contrato>.Fail($"Error: {ex.Message}");
        }
    }

    public async Task<ServiceResult<bool>> FinalizarContratoAsync(int contratoId, DateTime fechaFin)
    {
        try
        {
            var contrato = await _contratoRepo.GetByIdAsync(contratoId);
            if (contrato == null)
                return ServiceResult<bool>.Fail("Contrato no encontrado");

            contrato.Estado = EstadoContrato.Finalizado;
            contrato.FechaFin = fechaFin;
            contrato.FechaModificacion = DateTime.Now;
            await _contratoRepo.UpdateAsync(contrato);

            return ServiceResult<bool>.Ok(true, "Contrato finalizado");
        }
        catch (Exception ex)
        {
            return ServiceResult<bool>.Fail($"Error: {ex.Message}");
        }
    }

    public async Task<ServiceResult<IEnumerable<Contrato>>> GetContratosProximosAVencerAsync(int diasAnticipacion = 30)
    {
        try
        {
            var contratos = await _contratoRepo.GetContratosProximosAVencerAsync(diasAnticipacion);
            return ServiceResult<IEnumerable<Contrato>>.Ok(contratos);
        }
        catch (Exception ex)
        {
            return ServiceResult<IEnumerable<Contrato>>.Fail($"Error: {ex.Message}");
        }
    }
}
