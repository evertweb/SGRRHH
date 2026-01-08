using Dapper;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using SGRRHH.Local.Domain.Entities;
using SGRRHH.Local.Domain.Enums;
using SGRRHH.Local.Infrastructure.Repositories;
using Xunit;

namespace SGRRHH.Local.Tests.Integration;

[Collection("Database")]
public class WorkflowTests
{
    private readonly TestDatabaseFixture _fixture;
    private readonly PermisoRepository _permisoRepository;
    private readonly VacacionRepository _vacacionRepository;

    public WorkflowTests(TestDatabaseFixture fixture)
    {
        _fixture = fixture;
        _fixture.CleanDatabaseAsync().Wait();

        _permisoRepository = new PermisoRepository(_fixture.Context, Substitute.For<ILogger<PermisoRepository>>());
        _vacacionRepository = new VacacionRepository(_fixture.Context, Substitute.For<ILogger<VacacionRepository>>());
    }

    [Fact]
    public async Task PermisoWorkflow_CreateAndApprove_UpdatesEstado()
    {
        // Arrange
        var ctx = await CreateWorkflowDataAsync();
        var numeroActa = await _permisoRepository.GetProximoNumeroActaAsync();

        var permiso = new Permiso
        {
            NumeroActa = numeroActa,
            EmpleadoId = ctx.EmpleadoId,
            TipoPermisoId = ctx.TipoPermisoId,
            Motivo = "Cita médica",
            FechaSolicitud = DateTime.Today,
            FechaInicio = DateTime.Today.AddDays(1),
            FechaFin = DateTime.Today.AddDays(1),
            TotalDias = 1,
            Estado = EstadoPermiso.Pendiente,
            SolicitadoPorId = ctx.UsuarioId,
            Activo = true
        };

        var created = await _permisoRepository.AddAsync(permiso);

        // Act: aprobar permiso
        created.Estado = EstadoPermiso.Aprobado;
        created.AprobadoPorId = ctx.UsuarioId;
        created.FechaAprobacion = DateTime.Today;
        await _permisoRepository.UpdateAsync(created);

        var aprobados = await _permisoRepository.GetByEstadoAsync(EstadoPermiso.Aprobado);

        // Assert
        aprobados.Should().ContainSingle(p => p.Id == created.Id);
    }

    [Fact]
    public async Task VacacionWorkflow_PreventsOverlappingRequests()
    {
        // Arrange
        var ctx = await CreateWorkflowDataAsync();
        await _vacacionRepository.AddAsync(new Vacacion
        {
            EmpleadoId = ctx.EmpleadoId,
            FechaInicio = DateTime.Today.AddDays(5),
            FechaFin = DateTime.Today.AddDays(12),
            DiasTomados = 7,
            PeriodoCorrespondiente = DateTime.Today.Year,
            Estado = EstadoVacacion.Aprobada,
            Activo = true,
            FechaSolicitud = DateTime.Today
        });

        // Act
        var traslape = await _vacacionRepository.ExisteTraslapeAsync(ctx.EmpleadoId, DateTime.Today.AddDays(10), DateTime.Today.AddDays(15));

        // Assert
        traslape.Should().BeTrue();
    }

    private async Task<(int EmpleadoId, int UsuarioId, int TipoPermisoId)> CreateWorkflowDataAsync()
    {
        using var connection = _fixture.Context.CreateConnection();

        var tipoPermisoId = await connection.ExecuteScalarAsync<int>(@"INSERT INTO tipos_permiso (nombre, descripcion, color, requiere_aprobacion)
VALUES ('Día libre', 'Workflow', '#1E88E5', 1);
SELECT last_insert_rowid();");

        var empleadoId = await connection.ExecuteScalarAsync<int>(@"INSERT INTO empleados (codigo, cedula, nombres, apellidos, fecha_ingreso, activo)
VALUES ('EMP-WF', '2020202020', 'Workflow', 'Tester', @FechaIngreso, 1);
SELECT last_insert_rowid();", new { FechaIngreso = DateTime.Today.AddYears(-1) });

        var usuarioId = await connection.ExecuteScalarAsync<int>(@"INSERT INTO usuarios (username, password_hash, nombre_completo, rol, activo)
VALUES ('wfuser', 'hash', 'Workflow User', 1, 1);
SELECT last_insert_rowid();");

        return (empleadoId, usuarioId, tipoPermisoId);
    }
}
