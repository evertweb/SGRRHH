using BCrypt.Net;
using Dapper;
using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;
using NSubstitute;
using SGRRHH.Local.Domain.Entities;
using SGRRHH.Local.Domain.Enums;
using SGRRHH.Local.Infrastructure.Repositories;
using Xunit;

namespace SGRRHH.Local.Tests.Repositories;

[Collection("Database")]
public class PermisoRepositoryTests
{
    private readonly TestDatabaseFixture _fixture;
    private readonly PermisoRepository _repository;

    public PermisoRepositoryTests(TestDatabaseFixture fixture)
    {
        _fixture = fixture;
        var logger = Substitute.For<ILogger<PermisoRepository>>();
        _repository = new PermisoRepository(_fixture.Context, logger);
        _fixture.CleanDatabaseAsync().Wait();
    }

    [Fact]
    public async Task AddAsync_WithValidData_PersistsPermiso()
    {
        // Arrange
        var (empleadoId, usuarioId, tipoPermisoId) = await CreateDependenciesAsync();
        var numeroActa = await _repository.GetProximoNumeroActaAsync();

        var permiso = new Permiso
        {
            NumeroActa = numeroActa,
            EmpleadoId = empleadoId,
            TipoPermisoId = tipoPermisoId,
            Motivo = "Permiso de prueba",
            FechaSolicitud = DateTime.Today,
            FechaInicio = DateTime.Today.AddDays(1),
            FechaFin = DateTime.Today.AddDays(1),
            TotalDias = 1,
            Estado = EstadoPermiso.Pendiente,
            SolicitadoPorId = usuarioId,
            Activo = true
        };

        // Act
        var saved = await _repository.AddAsync(permiso);
        var stored = await _repository.GetByIdAsync(saved.Id);

        // Assert
        saved.Id.Should().BeGreaterThan(0);
        stored.Should().NotBeNull();
        stored!.NumeroActa.Should().Be(numeroActa);
        stored.EmpleadoId.Should().Be(empleadoId);
    }

    [Fact]
    public async Task ExisteSolapamientoAsync_WithOverlap_ReturnsTrue()
    {
        // Arrange
        var (empleadoId, usuarioId, tipoPermisoId) = await CreateDependenciesAsync();
        var basePermiso = new Permiso
        {
            NumeroActa = "PERM-BASE-0001",
            EmpleadoId = empleadoId,
            TipoPermisoId = tipoPermisoId,
            Motivo = "Base",
            FechaSolicitud = DateTime.Today,
            FechaInicio = DateTime.Today.AddDays(2),
            FechaFin = DateTime.Today.AddDays(3),
            TotalDias = 2,
            Estado = EstadoPermiso.Aprobado,
            SolicitadoPorId = usuarioId,
            Activo = true
        };
        await _repository.AddAsync(basePermiso);

        // Act
        var overlap = await _repository.ExisteSolapamientoAsync(empleadoId, DateTime.Today.AddDays(3), DateTime.Today.AddDays(4));
        var nonOverlap = await _repository.ExisteSolapamientoAsync(empleadoId, DateTime.Today.AddDays(10), DateTime.Today.AddDays(11));

        // Assert
        overlap.Should().BeTrue();
        nonOverlap.Should().BeFalse();
    }

    [Fact]
    public async Task GetProximoNumeroActaAsync_WithExistingActas_IncrementsSequence()
    {
        // Arrange
        var (empleadoId, usuarioId, tipoPermisoId) = await CreateDependenciesAsync();
        var currentYear = DateTime.Now.Year;
        var acta = $"PERM-{currentYear}-0009";

        await _repository.AddAsync(new Permiso
        {
            NumeroActa = acta,
            EmpleadoId = empleadoId,
            TipoPermisoId = tipoPermisoId,
            Motivo = "Secuencia",
            FechaSolicitud = DateTime.Today,
            FechaInicio = DateTime.Today,
            FechaFin = DateTime.Today,
            TotalDias = 1,
            Estado = EstadoPermiso.Pendiente,
            SolicitadoPorId = usuarioId,
            Activo = true
        });

        // Act
        var nextActa = await _repository.GetProximoNumeroActaAsync();

        // Assert
        nextActa.Should().Be($"PERM-{currentYear}-0010");
    }

    [Fact]
    public async Task AddAsync_WithMissingEmpleado_ThrowsForeignKeyException()
    {
        // Arrange: crear dependencias mínimas sin empleado
        using var connection = _fixture.Context.CreateConnection();

        var tipoPermisoId = await connection.ExecuteScalarAsync<int>(@"INSERT INTO tipos_permiso (nombre, descripcion, color, requiere_aprobacion)
VALUES ('FK Test', 'Valida FK', '#FF0000', 1);
SELECT last_insert_rowid();");

        var usuarioId = await connection.ExecuteScalarAsync<int>(@"INSERT INTO usuarios (username, password_hash, nombre_completo, rol, activo)
VALUES ('fk_tester', @PasswordHash, 'FK Tester', 1, 1);
SELECT last_insert_rowid();", new { PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin123!", workFactor: 11) });

        var permiso = new Permiso
        {
            NumeroActa = await _repository.GetProximoNumeroActaAsync(),
            EmpleadoId = 9999, // No existe
            TipoPermisoId = tipoPermisoId,
            Motivo = "FK debe fallar",
            FechaSolicitud = DateTime.Today,
            FechaInicio = DateTime.Today,
            FechaFin = DateTime.Today,
            TotalDias = 1,
            Estado = EstadoPermiso.Pendiente,
            SolicitadoPorId = usuarioId,
            Activo = true
        };

        // Act & Assert
        await Assert.ThrowsAsync<SqliteException>(() => _repository.AddAsync(permiso));
    }

    private async Task<(int empleadoId, int usuarioId, int tipoPermisoId)> CreateDependenciesAsync()
    {
        using var connection = _fixture.Context.CreateConnection();

        var tipoPermisoId = await connection.ExecuteScalarAsync<int>(@"INSERT INTO tipos_permiso (nombre, descripcion, color, requiere_aprobacion)
VALUES ('Médico', 'Cita médica', '#4CAF50', 1);
SELECT last_insert_rowid();");

        var empleadoId = await connection.ExecuteScalarAsync<int>(@"INSERT INTO empleados (codigo, cedula, nombres, apellidos, fecha_ingreso, activo)
VALUES ('EMP-100', '1234567890', 'Empleado', 'Prueba', @FechaIngreso, 1);
SELECT last_insert_rowid();", new { FechaIngreso = DateTime.Today.AddYears(-1) });

        var usuarioId = await connection.ExecuteScalarAsync<int>(@"INSERT INTO usuarios (username, password_hash, nombre_completo, rol, activo)
VALUES ('tester', @PasswordHash, 'Usuario Tester', 1, 1);
SELECT last_insert_rowid();", new { PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin123!", workFactor: 11) });

        return (empleadoId, usuarioId, tipoPermisoId);
    }
}
