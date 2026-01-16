using Microsoft.Extensions.Logging;
using SGRRHH.Local.Domain.Entities;
using SGRRHH.Local.Shared.Interfaces;

namespace SGRRHH.Local.Infrastructure.Services;

public class RegistroDiarioService : IRegistroDiarioService
{
    private readonly IRegistroDiarioRepository _registroRepository;
    private readonly IDetalleActividadRepository _detalleRepository;
    private readonly ILogger<RegistroDiarioService> _logger;

    public RegistroDiarioService(
        IRegistroDiarioRepository registroRepository,
        IDetalleActividadRepository detalleRepository,
        ILogger<RegistroDiarioService> logger)
    {
        _registroRepository = registroRepository;
        _detalleRepository = detalleRepository;
        _logger = logger;
    }

    public async Task<List<RegistroDiario>> GetRegistrosByFechaAsync(DateTime fecha)
    {
        var registros = (await _registroRepository.GetByFechaAsync(fecha.Date)).ToList();
        if (!registros.Any())
        {
            return registros;
        }

        var registroIds = registros.Select(r => r.Id).ToList();
        var detalles = (await _detalleRepository.GetByRegistroIdsAsync(registroIds)).ToList();

        var detallesPorRegistro = detalles
            .GroupBy(d => d.RegistroDiarioId)
            .ToDictionary(g => g.Key, g => g.ToList());

        foreach (var registro in registros)
        {
            if (detallesPorRegistro.TryGetValue(registro.Id, out var detalleList))
            {
                registro.DetallesActividades = detalleList;
            }
            else
            {
                registro.DetallesActividades = new List<DetalleActividad>();
            }
        }

        return registros;
    }

    public async Task<RegistroDiario?> GetRegistroByIdWithDetallesAsync(int registroId)
    {
        return await _registroRepository.GetByIdWithDetallesAsync(registroId);
    }

    public async Task<List<RegistroDiario>> CrearRegistrosParaEmpleadosAsync(IEnumerable<Empleado> empleados, DateTime fecha, int creadoPorId)
    {
        var registrosCreados = new List<RegistroDiario>();
        foreach (var empleado in empleados)
        {
            var registro = new RegistroDiario
            {
                Fecha = fecha.Date,
                EmpleadoId = empleado.Id,
                Estado = EstadoRegistroDiario.Borrador,
                FechaCreacion = DateTime.Now,
                CreadoPorId = creadoPorId
            };

            await _registroRepository.AddAsync(registro);
            registrosCreados.Add(registro);
        }

        _logger.LogInformation("Creados {Count} registros diarios para fecha {Fecha}", registrosCreados.Count, fecha.ToString("yyyy-MM-dd"));
        return registrosCreados;
    }

    public async Task CrearRegistroAsync(RegistroDiario registro)
    {
        await _registroRepository.AddAsync(registro);
    }

    public async Task ActualizarRegistroAsync(RegistroDiario registro)
    {
        await _registroRepository.UpdateAsync(registro);
    }

    public async Task<DetalleActividad> GuardarDetalleAsync(int registroId, DetalleActividad detalle, bool esEdicion)
    {
        if (esEdicion)
        {
            await _registroRepository.UpdateDetalleAsync(registroId, detalle);
            return detalle;
        }

        return await _registroRepository.AddDetalleAsync(registroId, detalle);
    }

    public async Task EliminarDetalleAsync(int registroId, int detalleId)
    {
        await _registroRepository.DeleteDetalleAsync(registroId, detalleId);
    }

    public Task<bool> ValidarHorasTotalesAsync(IEnumerable<DetalleActividad> detalles)
    {
        var totalHoras = detalles?.Sum(d => d.Horas) ?? 0;
        return Task.FromResult(totalHoras <= 24);
    }
}
