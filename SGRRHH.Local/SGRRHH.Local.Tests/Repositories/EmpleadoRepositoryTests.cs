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
public class EmpleadoRepositoryTests
{
    private readonly TestDatabaseFixture _fixture;
    private readonly EmpleadoRepository _repository;

    public EmpleadoRepositoryTests(TestDatabaseFixture fixture)
    {
        _fixture = fixture;
        var logger = Substitute.For<ILogger<EmpleadoRepository>>();
        _repository = new EmpleadoRepository(_fixture.Context, logger);
        
        // Limpiar base de datos antes de cada test
        _fixture.CleanDatabaseAsync().Wait();
    }

    [Fact]
    public async Task AddAsync_WithValidEmpleado_ReturnsEmpleadoWithId()
    {
        // Arrange
        var empleado = new Empleado
        {
            Codigo = "EMP-001",
            Cedula = "1234567890",
            Nombres = "Juan Carlos",
            Apellidos = "Pérez García",
            FechaIngreso = DateTime.Today.AddYears(-2),
            Estado = EstadoEmpleado.Activo,
            Activo = true
        };

        // Act
        var result = await _repository.AddAsync(empleado);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().BeGreaterThan(0);
        result.Codigo.Should().Be("EMP-001");
        result.Nombres.Should().Be("Juan Carlos");
    }

    [Fact]
    public async Task AddAsync_WithDuplicateCedula_ThrowsException()
    {
        // Arrange
        var empleado1 = new Empleado
        {
            Codigo = "EMP-002",
            Cedula = "9999999999",
            Nombres = "Test",
            Apellidos = "Uno",
            FechaIngreso = DateTime.Today,
            Activo = true
        };

        var empleado2 = new Empleado
        {
            Codigo = "EMP-003",
            Cedula = "9999999999", // Cédula duplicada
            Nombres = "Test",
            Apellidos = "Dos",
            FechaIngreso = DateTime.Today,
            Activo = true
        };

        await _repository.AddAsync(empleado1);

        // Act & Assert
        await Assert.ThrowsAsync<SqliteException>(async () => await _repository.AddAsync(empleado2));
    }

    [Fact]
    public async Task GetByIdAsync_WithExistingId_ReturnsEmpleado()
    {
        // Arrange
        var empleado = new Empleado
        {
            Codigo = "EMP-004",
            Cedula = "1111111111",
            Nombres = "María",
            Apellidos = "López",
            FechaIngreso = DateTime.Today,
            Activo = true
        };
        await _repository.AddAsync(empleado);

        // Act
        var result = await _repository.GetByIdAsync(empleado.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Nombres.Should().Be("María");
        result.Apellidos.Should().Be("López");
        result.Cedula.Should().Be("1111111111");
    }

    [Fact]
    public async Task GetByIdAsync_WithNonExistingId_ReturnsNull()
    {
        // Act
        var result = await _repository.GetByIdAsync(99999);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task SearchAsync_WithCriterio_ReturnsFilteredResults()
    {
        // Arrange
        await _repository.AddAsync(new Empleado
        {
            Codigo = "EMP-005",
            Cedula = "2222222222",
            Nombres = "Carlos Alberto",
            Apellidos = "Mendez",
            FechaIngreso = DateTime.Today,
            Activo = true
        });

        await _repository.AddAsync(new Empleado
        {
            Codigo = "EMP-006",
            Cedula = "3333333333",
            Nombres = "Carlos Eduardo",
            Apellidos = "Torres",
            FechaIngreso = DateTime.Today,
            Activo = true
        });

        await _repository.AddAsync(new Empleado
        {
            Codigo = "EMP-007",
            Cedula = "4444444444",
            Nombres = "Ana",
            Apellidos = "García",
            FechaIngreso = DateTime.Today,
            Activo = true
        });

        // Act
        var results = await _repository.SearchAsync("Carlos");

        // Assert
        results.Should().HaveCount(2);
        results.Should().OnlyContain(e => e.Nombres.Contains("Carlos"));
    }

    [Fact]
    public async Task UpdateAsync_WithValidData_UpdatesEmpleado()
    {
        // Arrange
        var empleado = new Empleado
        {
            Codigo = "EMP-008",
            Cedula = "5555555555",
            Nombres = "Original",
            Apellidos = "Test",
            FechaIngreso = DateTime.Today,
            Activo = true
        };
        await _repository.AddAsync(empleado);

        empleado.Nombres = "Modificado";
        empleado.Telefono = "555-1234";

        // Act
        await _repository.UpdateAsync(empleado);

        // Assert
        var updated = await _repository.GetByIdAsync(empleado.Id);
        updated.Should().NotBeNull();
        updated!.Nombres.Should().Be("Modificado");
        updated.Telefono.Should().Be("555-1234");
    }

    [Fact]
    public async Task DeleteAsync_WithExistingId_PerformsSoftDelete()
    {
        // Arrange
        var empleado = new Empleado
        {
            Codigo = "EMP-009",
            Cedula = "6666666666",
            Nombres = "Para",
            Apellidos = "Eliminar",
            FechaIngreso = DateTime.Today,
            Activo = true
        };
        await _repository.AddAsync(empleado);

        // Act
        await _repository.DeleteAsync(empleado.Id);

        // Assert
        var deleted = await _repository.GetByIdAsync(empleado.Id);
        deleted.Should().NotBeNull();
        deleted!.Activo.Should().BeFalse();
    }

    [Fact]
    public async Task GetAllAsync_ReturnsAllEmpleados()
    {
        // Arrange
        await _repository.AddAsync(new Empleado
        {
            Codigo = "EMP-010",
            Cedula = "7777777777",
            Nombres = "Test1",
            Apellidos = "Empleado1",
            FechaIngreso = DateTime.Today,
            Activo = true
        });

        await _repository.AddAsync(new Empleado
        {
            Codigo = "EMP-011",
            Cedula = "8888888888",
            Nombres = "Test2",
            Apellidos = "Empleado2",
            FechaIngreso = DateTime.Today,
            Activo = true
        });

        // Act
        var results = await _repository.GetAllAsync();

        // Assert
        results.Should().HaveCountGreaterOrEqualTo(2);
    }

    [Fact]
    public async Task GetByCedulaAsync_WithExistingCedula_ReturnsEmpleado()
    {
        // Arrange
        var empleado = new Empleado
        {
            Codigo = "EMP-012",
            Cedula = "1010101010",
            Nombres = "Prueba",
            Apellidos = "Cédula",
            FechaIngreso = DateTime.Today,
            Activo = true
        };
        await _repository.AddAsync(empleado);

        // Act
        var result = await _repository.GetByCedulaAsync("1010101010");

        // Assert
        result.Should().NotBeNull();
        result!.Nombres.Should().Be("Prueba");
    }

    [Fact]
    public async Task NombreCompleto_Property_ReturnsCorrectFormat()
    {
        // Arrange
        var empleado = new Empleado
        {
            Codigo = "EMP-013",
            Cedula = "1212121212",
            Nombres = "Pedro José",
            Apellidos = "González Ramírez",
            FechaIngreso = DateTime.Today,
            Activo = true
        };

        // Act
        var nombreCompleto = empleado.NombreCompleto;

        // Assert
        nombreCompleto.Should().Be("Pedro José González Ramírez");
    }

    [Fact]
    public async Task Antiguedad_Property_CalculatesCorrectly()
    {
        // Arrange
        var empleado = new Empleado
        {
            Codigo = "EMP-014",
            Cedula = "1313131313",
            Nombres = "Test",
            Apellidos = "Antigüedad",
            FechaIngreso = DateTime.Today.AddYears(-5),
            Activo = true
        };

        // Act
        var antiguedad = empleado.Antiguedad;

        // Assert
        antiguedad.Should().Be(5);
    }
}
