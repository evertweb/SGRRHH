using SGRRHH.Local.Domain.Entities;

namespace SGRRHH.Local.Shared.Interfaces;

public interface IRegistroDiarioService
{
    Task<List<RegistroDiario>> GetRegistrosByFechaAsync(DateTime fecha);

    Task<RegistroDiario?> GetRegistroByIdWithDetallesAsync(int registroId);

    Task<List<RegistroDiario>> CrearRegistrosParaEmpleadosAsync(IEnumerable<Empleado> empleados, DateTime fecha, int creadoPorId);

    Task CrearRegistroAsync(RegistroDiario registro);

    Task ActualizarRegistroAsync(RegistroDiario registro);

    Task<DetalleActividad> GuardarDetalleAsync(int registroId, DetalleActividad detalle, bool esEdicion);

    Task EliminarDetalleAsync(int registroId, int detalleId);

    Task<bool> ValidarHorasTotalesAsync(IEnumerable<DetalleActividad> detalles);
}
