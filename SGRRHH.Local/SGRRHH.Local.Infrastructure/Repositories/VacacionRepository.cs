using Dapper;
using Microsoft.Extensions.Logging;
using SGRRHH.Local.Domain.DTOs;
using SGRRHH.Local.Domain.Entities;
using SGRRHH.Local.Domain.Enums;
using SGRRHH.Local.Shared.Interfaces;

namespace SGRRHH.Local.Infrastructure.Repositories;

public class VacacionRepository : IVacacionRepository
{
    private readonly Data.DapperContext _context;
    private readonly ILogger<VacacionRepository> _logger;
    private readonly IEmpleadoRepository _empleadoRepository;

    public VacacionRepository(
        Data.DapperContext context, 
        ILogger<VacacionRepository> logger,
        IEmpleadoRepository empleadoRepository)
    {
        _context = context;
        _logger = logger;
        _empleadoRepository = empleadoRepository;
    }

    public async Task<Vacacion> AddAsync(Vacacion entity)
    {
        entity.FechaCreacion = DateTime.Now;
        const string sql = @"INSERT INTO vacaciones (
    empleado_id, fecha_inicio, fecha_fin, dias_tomados, periodo_correspondiente, estado, observaciones,
    fecha_solicitud, solicitado_por_id, aprobado_por_id, fecha_aprobacion, motivo_rechazo, activo, fecha_creacion)
VALUES (@EmpleadoId, @FechaInicio, @FechaFin, @DiasTomados, @PeriodoCorrespondiente, @Estado, @Observaciones,
        @FechaSolicitud, @SolicitadoPorId, @AprobadoPorId, @FechaAprobacion, @MotivoRechazo, @Activo, @FechaCreacion);
SELECT last_insert_rowid();";

        using var connection = _context.CreateConnection();
        entity.Id = await connection.ExecuteScalarAsync<int>(sql, entity);
        return entity;
    }

    public async Task UpdateAsync(Vacacion entity)
    {
        entity.FechaModificacion = DateTime.Now;
        const string sql = @"UPDATE vacaciones
SET empleado_id = @EmpleadoId,
    fecha_inicio = @FechaInicio,
    fecha_fin = @FechaFin,
    dias_tomados = @DiasTomados,
    periodo_correspondiente = @PeriodoCorrespondiente,
    estado = @Estado,
    observaciones = @Observaciones,
    fecha_solicitud = @FechaSolicitud,
    solicitado_por_id = @SolicitadoPorId,
    aprobado_por_id = @AprobadoPorId,
    fecha_aprobacion = @FechaAprobacion,
    motivo_rechazo = @MotivoRechazo,
    fecha_modificacion = @FechaModificacion
WHERE id = @Id";

        using var connection = _context.CreateConnection();
        await connection.ExecuteAsync(sql, entity);
    }

    public async Task DeleteAsync(int id)
    {
        // Hard delete - elimina permanentemente el registro
        const string sql = "DELETE FROM vacaciones WHERE id = @Id";
        using var connection = _context.CreateConnection();
        await connection.ExecuteAsync(sql, new { Id = id });
    }

    public async Task<Vacacion?> GetByIdAsync(int id)
    {
        const string sql = @"
SELECT v.*, 
       e.id, e.codigo, e.cedula, e.nombres, e.apellidos, e.fecha_ingreso, e.estado,
       c.id, c.nombre,
       d.id, d.nombre,
       us.id, us.nombre_completo, us.email,
       ua.id, ua.nombre_completo, ua.email
FROM vacaciones v
LEFT JOIN empleados e ON v.empleado_id = e.id
LEFT JOIN cargos c ON e.cargo_id = c.id
LEFT JOIN departamentos d ON e.departamento_id = d.id
LEFT JOIN usuarios us ON v.solicitado_por_id = us.id
LEFT JOIN usuarios ua ON v.aprobado_por_id = ua.id
WHERE v.id = @Id";
        
        using var connection = _context.CreateConnection();
        var vacaciones = await connection.QueryAsync<Vacacion, Empleado, Cargo, Departamento, Usuario, Usuario, Vacacion>(
            sql,
            (vacacion, empleado, cargo, departamento, solicitadoPor, aprobadoPor) =>
            {
                if (empleado != null)
                {
                    empleado.Cargo = cargo;
                    empleado.Departamento = departamento;
                    vacacion.Empleado = empleado;
                }
                vacacion.SolicitadoPor = solicitadoPor;
                vacacion.AprobadoPor = aprobadoPor;
                return vacacion;
            },
            new { Id = id },
            splitOn: "id,id,id,id,id"
        );
        return vacaciones.FirstOrDefault();
    }

    public async Task<IEnumerable<Vacacion>> GetAllAsync()
    {
        const string sql = @"
SELECT v.*, 
       e.id, e.codigo, e.cedula, e.nombres, e.apellidos, e.fecha_ingreso, e.estado,
       c.id, c.nombre,
       d.id, d.nombre,
       us.id, us.nombre_completo, us.email,
       ua.id, ua.nombre_completo, ua.email
FROM vacaciones v
LEFT JOIN empleados e ON v.empleado_id = e.id
LEFT JOIN cargos c ON e.cargo_id = c.id
LEFT JOIN departamentos d ON e.departamento_id = d.id
LEFT JOIN usuarios us ON v.solicitado_por_id = us.id
LEFT JOIN usuarios ua ON v.aprobado_por_id = ua.id
ORDER BY v.fecha_solicitud DESC";
        
        using var connection = _context.CreateConnection();
        return await connection.QueryAsync<Vacacion, Empleado, Cargo, Departamento, Usuario, Usuario, Vacacion>(
            sql,
            (vacacion, empleado, cargo, departamento, solicitadoPor, aprobadoPor) =>
            {
                if (empleado != null)
                {
                    empleado.Cargo = cargo;
                    empleado.Departamento = departamento;
                    vacacion.Empleado = empleado;
                }
                vacacion.SolicitadoPor = solicitadoPor;
                vacacion.AprobadoPor = aprobadoPor;
                return vacacion;
            },
            splitOn: "id,id,id,id,id"
        );
    }

    public async Task<IEnumerable<Vacacion>> GetAllActiveAsync()
    {
        const string sql = @"
SELECT v.*, 
       e.id, e.codigo, e.cedula, e.nombres, e.apellidos, e.fecha_ingreso, e.estado,
       c.id, c.nombre,
       d.id, d.nombre,
       us.id, us.nombre_completo, us.email,
       ua.id, ua.nombre_completo, ua.email
FROM vacaciones v
LEFT JOIN empleados e ON v.empleado_id = e.id
LEFT JOIN cargos c ON e.cargo_id = c.id
LEFT JOIN departamentos d ON e.departamento_id = d.id
LEFT JOIN usuarios us ON v.solicitado_por_id = us.id
LEFT JOIN usuarios ua ON v.aprobado_por_id = ua.id
WHERE v.activo = 1
ORDER BY v.fecha_solicitud DESC";
        
        using var connection = _context.CreateConnection();
        return await connection.QueryAsync<Vacacion, Empleado, Cargo, Departamento, Usuario, Usuario, Vacacion>(
            sql,
            (vacacion, empleado, cargo, departamento, solicitadoPor, aprobadoPor) =>
            {
                if (empleado != null)
                {
                    empleado.Cargo = cargo;
                    empleado.Departamento = departamento;
                    vacacion.Empleado = empleado;
                }
                vacacion.SolicitadoPor = solicitadoPor;
                vacacion.AprobadoPor = aprobadoPor;
                return vacacion;
            },
            splitOn: "id,id,id,id,id"
        );
    }

    public async Task<IEnumerable<Vacacion>> GetByEmpleadoIdAsync(int empleadoId)
    {
        const string sql = @"
SELECT v.*, 
       e.id, e.codigo, e.cedula, e.nombres, e.apellidos, e.fecha_ingreso, e.estado,
       c.id, c.nombre,
       d.id, d.nombre,
       us.id, us.nombre_completo, us.email,
       ua.id, ua.nombre_completo, ua.email
FROM vacaciones v
LEFT JOIN empleados e ON v.empleado_id = e.id
LEFT JOIN cargos c ON e.cargo_id = c.id
LEFT JOIN departamentos d ON e.departamento_id = d.id
LEFT JOIN usuarios us ON v.solicitado_por_id = us.id
LEFT JOIN usuarios ua ON v.aprobado_por_id = ua.id
WHERE v.empleado_id = @EmpleadoId AND v.activo = 1
ORDER BY v.fecha_solicitud DESC";
        
        using var connection = _context.CreateConnection();
        return await connection.QueryAsync<Vacacion, Empleado, Cargo, Departamento, Usuario, Usuario, Vacacion>(
            sql,
            (vacacion, empleado, cargo, departamento, solicitadoPor, aprobadoPor) =>
            {
                if (empleado != null)
                {
                    empleado.Cargo = cargo;
                    empleado.Departamento = departamento;
                    vacacion.Empleado = empleado;
                }
                vacacion.SolicitadoPor = solicitadoPor;
                vacacion.AprobadoPor = aprobadoPor;
                return vacacion;
            },
            new { EmpleadoId = empleadoId },
            splitOn: "id,id,id,id,id"
        );
    }

    public async Task<IEnumerable<Vacacion>> GetByEmpleadoYPeriodoAsync(int empleadoId, int periodo)
    {
        const string sql = @"
SELECT v.*, 
       e.id, e.codigo, e.cedula, e.nombres, e.apellidos, e.fecha_ingreso, e.estado,
       c.id, c.nombre,
       d.id, d.nombre,
       us.id, us.nombre_completo, us.email,
       ua.id, ua.nombre_completo, ua.email
FROM vacaciones v
LEFT JOIN empleados e ON v.empleado_id = e.id
LEFT JOIN cargos c ON e.cargo_id = c.id
LEFT JOIN departamentos d ON e.departamento_id = d.id
LEFT JOIN usuarios us ON v.solicitado_por_id = us.id
LEFT JOIN usuarios ua ON v.aprobado_por_id = ua.id
WHERE v.empleado_id = @EmpleadoId AND v.periodo_correspondiente = @Periodo AND v.activo = 1
ORDER BY v.fecha_solicitud DESC";
        
        using var connection = _context.CreateConnection();
        return await connection.QueryAsync<Vacacion, Empleado, Cargo, Departamento, Usuario, Usuario, Vacacion>(
            sql,
            (vacacion, empleado, cargo, departamento, solicitadoPor, aprobadoPor) =>
            {
                if (empleado != null)
                {
                    empleado.Cargo = cargo;
                    empleado.Departamento = departamento;
                    vacacion.Empleado = empleado;
                }
                vacacion.SolicitadoPor = solicitadoPor;
                vacacion.AprobadoPor = aprobadoPor;
                return vacacion;
            },
            new { EmpleadoId = empleadoId, Periodo = periodo },
            splitOn: "id,id,id,id,id"
        );
    }

    public async Task<IEnumerable<Vacacion>> GetByRangoFechasAsync(DateTime fechaInicio, DateTime fechaFin)
    {
        const string sql = @"
SELECT v.*, 
       e.id, e.codigo, e.cedula, e.nombres, e.apellidos, e.fecha_ingreso, e.estado,
       c.id, c.nombre,
       d.id, d.nombre,
       us.id, us.nombre_completo, us.email,
       ua.id, ua.nombre_completo, ua.email
FROM vacaciones v
LEFT JOIN empleados e ON v.empleado_id = e.id
LEFT JOIN cargos c ON e.cargo_id = c.id
LEFT JOIN departamentos d ON e.departamento_id = d.id
LEFT JOIN usuarios us ON v.solicitado_por_id = us.id
LEFT JOIN usuarios ua ON v.aprobado_por_id = ua.id
WHERE v.fecha_inicio >= @FechaInicio AND v.fecha_fin <= @FechaFin
ORDER BY v.fecha_solicitud DESC";
        
        using var connection = _context.CreateConnection();
        return await connection.QueryAsync<Vacacion, Empleado, Cargo, Departamento, Usuario, Usuario, Vacacion>(
            sql,
            (vacacion, empleado, cargo, departamento, solicitadoPor, aprobadoPor) =>
            {
                if (empleado != null)
                {
                    empleado.Cargo = cargo;
                    empleado.Departamento = departamento;
                    vacacion.Empleado = empleado;
                }
                vacacion.SolicitadoPor = solicitadoPor;
                vacacion.AprobadoPor = aprobadoPor;
                return vacacion;
            },
            new { FechaInicio = fechaInicio, FechaFin = fechaFin },
            splitOn: "id,id,id,id,id"
        );
    }

    public async Task<bool> ExisteTraslapeAsync(int empleadoId, DateTime fechaInicio, DateTime fechaFin, int? vacacionIdExcluir = null)
    {
        const string sql = @"SELECT COUNT(1) FROM vacaciones
WHERE empleado_id = @EmpleadoId
  AND activo = 1
  AND estado NOT IN (@Rechazada, @Cancelada)
  AND fecha_inicio <= @FechaFin
  AND fecha_fin >= @FechaInicio
  AND (@ExcludeId IS NULL OR id <> @ExcludeId)";

        using var connection = _context.CreateConnection();
        var count = await connection.ExecuteScalarAsync<int>(sql, new 
        { 
            EmpleadoId = empleadoId, 
            FechaInicio = fechaInicio, 
            FechaFin = fechaFin, 
            ExcludeId = vacacionIdExcluir,
            Rechazada = EstadoVacacion.Rechazada,
            Cancelada = EstadoVacacion.Cancelada
        });
        return count > 0;
    }

    public async Task<IEnumerable<Vacacion>> GetByEstadoAsync(EstadoVacacion estado)
    {
        const string sql = @"
SELECT v.*, 
       e.id, e.codigo, e.cedula, e.nombres, e.apellidos, e.fecha_ingreso, e.estado,
       c.id, c.nombre,
       d.id, d.nombre,
       us.id, us.nombre_completo, us.email,
       ua.id, ua.nombre_completo, ua.email
FROM vacaciones v
LEFT JOIN empleados e ON v.empleado_id = e.id
LEFT JOIN cargos c ON e.cargo_id = c.id
LEFT JOIN departamentos d ON e.departamento_id = d.id
LEFT JOIN usuarios us ON v.solicitado_por_id = us.id
LEFT JOIN usuarios ua ON v.aprobado_por_id = ua.id
WHERE v.estado = @Estado
ORDER BY v.fecha_solicitud DESC";
        
        using var connection = _context.CreateConnection();
        return await connection.QueryAsync<Vacacion, Empleado, Cargo, Departamento, Usuario, Usuario, Vacacion>(
            sql,
            (vacacion, empleado, cargo, departamento, solicitadoPor, aprobadoPor) =>
            {
                if (empleado != null)
                {
                    empleado.Cargo = cargo;
                    empleado.Departamento = departamento;
                    vacacion.Empleado = empleado;
                }
                vacacion.SolicitadoPor = solicitadoPor;
                vacacion.AprobadoPor = aprobadoPor;
                return vacacion;
            },
            new { Estado = estado },
            splitOn: "id,id,id,id,id"
        );
    }

    public async Task<IEnumerable<Vacacion>> GetPendientesAsync()
    {
        return await GetByEstadoAsync(EstadoVacacion.Pendiente);
    }

    public async Task<ResumenVacaciones?> GetResumenVacacionesAsync(int empleadoId)
    {
        try
        {
            // Obtener empleado
            var empleado = await _empleadoRepository.GetByIdAsync(empleadoId);
            if (empleado == null) return null;

            // Calcular antigüedad
            var antiguedadAnos = DateTime.Now.Year - empleado.FechaIngreso.Year;
            var periodoActual = DateTime.Now.Year;

            // Calcular días ganados según antigüedad (15 días base + 1 día cada 5 años hasta máximo 5 adicionales)
            int diasBase = 15;
            int diasAdicionales = Math.Min(antiguedadAnos / 5, 5);
            int diasGanadosPeriodo = diasBase + diasAdicionales;

            // Obtener vacaciones del período actual
            var vacacionesPeriodo = await GetByEmpleadoYPeriodoAsync(empleadoId, periodoActual);
            
            // Calcular días tomados en el período (solo aprobadas, programadas o disfrutadas)
            int diasTomadosPeriodo = vacacionesPeriodo
                .Where(v => v.Estado == EstadoVacacion.Aprobada || 
                           v.Estado == EstadoVacacion.Programada || 
                           v.Estado == EstadoVacacion.Disfrutada ||
                           v.Estado == EstadoVacacion.Pendiente)
                .Sum(v => v.DiasTomados);

            // Calcular días disponibles en el período
            int diasDisponiblesPeriodo = diasGanadosPeriodo - diasTomadosPeriodo;

            // Obtener vacaciones de años anteriores no tomadas (esto es una simplificación)
            var vacacionesAnteriores = (await GetByEmpleadoIdAsync(empleadoId))
                .Where(v => v.PeriodoCorrespondiente < periodoActual);
            
            // Calcular días acumulados de años anteriores
            int diasAcumuladosAnteriores = 0;
            foreach (var periodoAnterior in vacacionesAnteriores.Select(v => v.PeriodoCorrespondiente).Distinct())
            {
                var diasGanadosAnterior = diasBase + Math.Min((periodoAnterior - empleado.FechaIngreso.Year) / 5, 5);
                var diasUsadosAnterior = vacacionesAnteriores
                    .Where(v => v.PeriodoCorrespondiente == periodoAnterior && 
                               (v.Estado == EstadoVacacion.Aprobada || 
                                v.Estado == EstadoVacacion.Programada || 
                                v.Estado == EstadoVacacion.Disfrutada))
                    .Sum(v => v.DiasTomados);
                
                var saldoAnterior = diasGanadosAnterior - diasUsadosAnterior;
                if (saldoAnterior > 0)
                {
                    diasAcumuladosAnteriores += saldoAnterior;
                }
            }

            var resumen = new ResumenVacaciones
            {
                EmpleadoId = empleadoId,
                NombreEmpleado = empleado.NombreCompleto,
                FechaIngreso = empleado.FechaIngreso,
                AntiguedadAnios = antiguedadAnos,
                PeriodoActual = periodoActual,
                DiasGanadosPeriodo = diasGanadosPeriodo,
                DiasTomadosPeriodo = diasTomadosPeriodo,
                DiasDisponiblesPeriodo = diasDisponiblesPeriodo,
                DiasAcumuladosAnteriores = diasAcumuladosAnteriores,
                TotalDiasDisponibles = diasDisponiblesPeriodo + diasAcumuladosAnteriores
            };

            return resumen;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error al calcular resumen de vacaciones para empleado {empleadoId}");
            return null;
        }
    }

    public Task<int> SaveChangesAsync() => Task.FromResult(0);
}
