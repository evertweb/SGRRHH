using Dapper;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using SGRRHH.Local.Domain.Entities;
using SGRRHH.Local.Domain.Enums;
using SGRRHH.Local.Infrastructure.Repositories;
using Xunit;

namespace SGRRHH.Local.Tests.Repositories;

[Collection("Database")]
public class VacacionRepositoryTests
{
    private readonly TestDatabaseFixture _fixture;
    private readonly VacacionRepository _repository;

    public VacacionRepositoryTests(TestDatabaseFixture fixture)
    {
        _fixture = fixture;
        var logger = Substitute.For<ILogger<VacacionRepository>>();
        _repository = new VacacionRepository(_fixture.Context, logger);
        _fixture.CleanDatabaseAsync().Wait();
    }

    [Fact]
    public async Task AddAsync_WithValidData_PersistsVacacion()
    {
        // Arrange
        var empleadoId = await CreateEmpleadoAsync();
        var vacacion = new Vacacion
        {
            EmpleadoId = empleadoId,
            FechaInicio = DateTime.Today.AddDays(5),
            FechaFin = DateTime.Today.AddDays(10),
            DiasTomados = 5,
            PeriodoCorrespondiente = DateTime.Today.Year,
            Estado = EstadoVacacion.Pendiente,
            Activo = true,
            FechaSolicitud = DateTime.Today
        };

        // Act
        var saved = await _repository.AddAsync(vacacion);
        var stored = await _repository.GetByIdAsync(saved.Id);

        // Assert
        saved.Id.Should().BeGreaterThan(0);
        stored.Should().NotBeNull();
        stored!.EmpleadoId.Should().Be(empleadoId);
        stored.DiasTomados.Should().Be(5);
    }

    [Fact]
    public async Task ExisteTraslapeAsync_WithOverlappingDates_ReturnsTrue()
    {
        // Arrange
        var empleadoId = await CreateEmpleadoAsync();
        await _repository.AddAsync(new Vacacion
        {
            EmpleadoId = empleadoId,
            FechaInicio = DateTime.Today.AddDays(1),
            FechaFin = DateTime.Today.AddDays(4),
            DiasTomados = 3,
            PeriodoCorrespondiente = DateTime.Today.Year,
            Estado = EstadoVacacion.Aprobada,
            Activo = true,
            FechaSolicitud = DateTime.Today
        });

        // Act
        var overlap = await _repository.ExisteTraslapeAsync(empleadoId, DateTime.Today.AddDays(3), DateTime.Today.AddDays(6));
        var nonOverlap = await _repository.ExisteTraslapeAsync(empleadoId, DateTime.Today.AddDays(10), DateTime.Today.AddDays(12));

        // Assert
        overlap.Should().BeTrue();
        nonOverlap.Should().BeFalse();
    }

    [Fact]
    public async Task DeleteAsync_SoftDeletesRecord()
    {
        // Arrange
        var empleadoId = await CreateEmpleadoAsync();
        var vacacion = await _repository.AddAsync(new Vacacion
        {
            EmpleadoId = empleadoId,
            FechaInicio = DateTime.Today.AddDays(2),
            FechaFin = DateTime.Today.AddDays(4),
            DiasTomados = 2,
            PeriodoCorrespondiente = DateTime.Today.Year,
            Estado = EstadoVacacion.Pendiente,
            Activo = true,
            FechaSolicitud = DateTime.Today
        });

        // Act
        await _repository.DeleteAsync(vacacion.Id);

        // Assert
        var stored = await _repository.GetByIdAsync(vacacion.Id);
        stored.Should().NotBeNull();
        stored!.Activo.Should().BeFalse();

        var activos = await _repository.GetByEmpleadoIdAsync(empleadoId);
        activos.Should().BeEmpty();
    }

    private async Task<int> CreateEmpleadoAsync()
    {
        using var connection = _fixture.Context.CreateConnection();
        return await connection.ExecuteScalarAsync<int>(@"INSERT INTO empleados (codigo, cedula, nombres, apellidos, fecha_ingreso, activo)
VALUES ('EMP-VAC', '5555555555', 'Vacaciones', 'Tester', @FechaIngreso, 1);
SELECT last_insert_rowid();", new { FechaIngreso = DateTime.Today.AddYears(-1) });
    }
}
