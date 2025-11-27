using Moq;
using SGRRHH.Core.Entities;
using SGRRHH.Core.Enums;
using SGRRHH.Core.Interfaces;
using SGRRHH.Infrastructure.Services;
using Xunit;

namespace SGRRHH.Tests.Services;

/// <summary>
/// Pruebas unitarias para EmpleadoService
/// </summary>
public class EmpleadoServiceTests
{
    private readonly Mock<IEmpleadoRepository> _mockRepository;
    private readonly EmpleadoService _service;

    public EmpleadoServiceTests()
    {
        _mockRepository = new Mock<IEmpleadoRepository>();
        _service = new EmpleadoService(_mockRepository.Object);
    }

    [Fact]
    public async Task CreateAsync_WithValidEmpleado_ReturnsSuccess()
    {
        // Arrange
        var empleado = new Empleado
        {
            Cedula = "12345678",
            Nombres = "Juan",
            Apellidos = "Pérez",
            FechaIngreso = DateTime.Today
        };

        _mockRepository.Setup(r => r.ExistsCedulaAsync(It.IsAny<string>(), It.IsAny<int?>()))
            .ReturnsAsync(false);
        _mockRepository.Setup(r => r.ExistsCodigoAsync(It.IsAny<string>(), It.IsAny<int?>()))
            .ReturnsAsync(false);
        _mockRepository.Setup(r => r.GetNextCodigoAsync())
            .ReturnsAsync("EMP-0001");
        _mockRepository.Setup(r => r.AddAsync(It.IsAny<Empleado>()))
            .ReturnsAsync((Empleado e) => e);
        _mockRepository.Setup(r => r.SaveChangesAsync())
            .ReturnsAsync(1);

        // Act
        var result = await _service.CreateAsync(empleado);

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(result.Data);
        Assert.Equal("EMP-0001", result.Data.Codigo);
    }

    [Fact]
    public async Task CreateAsync_WithDuplicateCedula_ReturnsError()
    {
        // Arrange
        var empleado = new Empleado
        {
            Cedula = "12345678",
            Nombres = "Juan",
            Apellidos = "Pérez",
            FechaIngreso = DateTime.Today
        };

        _mockRepository.Setup(r => r.ExistsCedulaAsync("12345678", null))
            .ReturnsAsync(true);
        _mockRepository.Setup(r => r.GetNextCodigoAsync())
            .ReturnsAsync("EMP-0001");

        // Act
        var result = await _service.CreateAsync(empleado);

        // Assert
        Assert.False(result.Success);
        Assert.Contains("cédula", result.Message.ToLower());
    }

    [Fact]
    public async Task CreateAsync_WithMissingCedula_ReturnsError()
    {
        // Arrange
        var empleado = new Empleado
        {
            Cedula = "",
            Nombres = "Juan",
            Apellidos = "Pérez",
            FechaIngreso = DateTime.Today
        };

        // Act
        var result = await _service.CreateAsync(empleado);

        // Assert
        Assert.False(result.Success);
        Assert.Contains("cédula", result.Message.ToLower());
    }

    [Fact]
    public async Task CreateAsync_WithMissingNombres_ReturnsError()
    {
        // Arrange
        var empleado = new Empleado
        {
            Cedula = "12345678",
            Nombres = "",
            Apellidos = "Pérez",
            FechaIngreso = DateTime.Today
        };

        // Act
        var result = await _service.CreateAsync(empleado);

        // Assert
        Assert.False(result.Success);
        Assert.Contains("nombres", result.Message.ToLower());
    }

    [Fact]
    public async Task GetAllAsync_ReturnsEmpleados()
    {
        // Arrange
        var empleados = new List<Empleado>
        {
            new Empleado { Id = 1, Codigo = "EMP-001", Nombres = "Juan", Apellidos = "Pérez", Cedula = "111" },
            new Empleado { Id = 2, Codigo = "EMP-002", Nombres = "María", Apellidos = "García", Cedula = "222" }
        };

        _mockRepository.Setup(r => r.GetAllActiveWithRelationsAsync())
            .ReturnsAsync(empleados);

        // Act
        var result = await _service.GetAllAsync();

        // Assert
        Assert.Equal(2, result.Count());
    }

    [Fact]
    public async Task CountActiveAsync_ReturnsCorrectCount()
    {
        // Arrange
        _mockRepository.Setup(r => r.CountActiveAsync())
            .ReturnsAsync(15);

        // Act
        var result = await _service.CountActiveAsync();

        // Assert
        Assert.Equal(15, result);
    }

    [Fact]
    public async Task GetCumpleaniosProximosAsync_ReturnsBirthdaysWithinRange()
    {
        // Arrange
        var hoy = DateTime.Today;
        var empleados = new List<Empleado>
        {
            // Cumpleaños en 3 días
            new Empleado 
            { 
                Id = 1, 
                Nombres = "Juan", 
                Apellidos = "Pérez",
                Cedula = "111",
                FechaNacimiento = hoy.AddDays(3).AddYears(-30)
            },
            // Cumpleaños en 10 días (fuera del rango de 7)
            new Empleado 
            { 
                Id = 2, 
                Nombres = "María", 
                Apellidos = "García",
                Cedula = "222",
                FechaNacimiento = hoy.AddDays(10).AddYears(-25)
            }
        };

        _mockRepository.Setup(r => r.GetAllActiveWithRelationsAsync())
            .ReturnsAsync(empleados);

        // Act
        var result = await _service.GetCumpleaniosProximosAsync(7);

        // Assert
        Assert.Single(result); // Solo 1 cumpleaños en los próximos 7 días
        Assert.Equal("Juan", result.First().Nombres);
    }

    [Fact]
    public async Task DeactivateAsync_WithValidId_ReturnsSuccess()
    {
        // Arrange
        var empleado = new Empleado 
        { 
            Id = 1, 
            Nombres = "Juan", 
            Apellidos = "Pérez",
            Cedula = "111",
            Estado = EstadoEmpleado.Activo 
        };

        _mockRepository.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(empleado);
        _mockRepository.Setup(r => r.UpdateAsync(It.IsAny<Empleado>()))
            .Returns(Task.CompletedTask);
        _mockRepository.Setup(r => r.SaveChangesAsync())
            .ReturnsAsync(1);

        // Act
        var result = await _service.DeactivateAsync(1);

        // Assert
        Assert.True(result.Success);
        Assert.Equal(EstadoEmpleado.Retirado, empleado.Estado);
        Assert.False(empleado.Activo);
    }

    [Fact]
    public async Task DeactivateAsync_WithInvalidId_ReturnsError()
    {
        // Arrange
        _mockRepository.Setup(r => r.GetByIdAsync(999))
            .ReturnsAsync((Empleado?)null);

        // Act
        var result = await _service.DeactivateAsync(999);

        // Assert
        Assert.False(result.Success);
        Assert.Contains("no encontrado", result.Message.ToLower());
    }

    [Fact]
    public async Task GetEmpleadosPorDepartamentoAsync_ReturnsGroupedData()
    {
        // Arrange
        var empleados = new List<Empleado>
        {
            new Empleado 
            { 
                Id = 1, 
                Nombres = "Juan", 
                Apellidos = "Pérez",
                Cedula = "111",
                Departamento = new Departamento { Id = 1, Nombre = "TI" }
            },
            new Empleado 
            { 
                Id = 2, 
                Nombres = "María", 
                Apellidos = "García",
                Cedula = "222",
                Departamento = new Departamento { Id = 1, Nombre = "TI" }
            },
            new Empleado 
            { 
                Id = 3, 
                Nombres = "Pedro", 
                Apellidos = "López",
                Cedula = "333",
                Departamento = new Departamento { Id = 2, Nombre = "RRHH" }
            }
        };

        _mockRepository.Setup(r => r.GetAllActiveWithRelationsAsync())
            .ReturnsAsync(empleados);

        // Act
        var result = await _service.GetEmpleadosPorDepartamentoAsync();

        // Assert
        Assert.Equal(2, result.Count()); // 2 departamentos
        var tiDepto = result.FirstOrDefault(e => e.Etiqueta == "TI");
        Assert.NotNull(tiDepto);
        Assert.Equal(2, tiDepto.Cantidad);
    }
    
    [Fact]
    public async Task ReactivateAsync_WithValidId_ReturnsSuccess()
    {
        // Arrange
        var empleado = new Empleado 
        { 
            Id = 1, 
            Nombres = "Juan", 
            Apellidos = "Pérez",
            Cedula = "111",
            Estado = EstadoEmpleado.Retirado,
            Activo = false
        };

        _mockRepository.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(empleado);
        _mockRepository.Setup(r => r.UpdateAsync(It.IsAny<Empleado>()))
            .Returns(Task.CompletedTask);
        _mockRepository.Setup(r => r.SaveChangesAsync())
            .ReturnsAsync(1);

        // Act
        var result = await _service.ReactivateAsync(1);

        // Assert
        Assert.True(result.Success);
        Assert.Equal(EstadoEmpleado.Activo, empleado.Estado);
        Assert.True(empleado.Activo);
    }
    
    [Fact]
    public async Task SearchAsync_ReturnsMatchingEmpleados()
    {
        // Arrange
        var empleados = new List<Empleado>
        {
            new Empleado { Id = 1, Nombres = "Juan Carlos", Apellidos = "Pérez", Cedula = "111" },
            new Empleado { Id = 2, Nombres = "María", Apellidos = "García", Cedula = "222" }
        };

        _mockRepository.Setup(r => r.SearchAsync("Juan"))
            .ReturnsAsync(empleados.Where(e => e.Nombres.Contains("Juan")));

        // Act
        var result = await _service.SearchAsync("Juan");

        // Assert
        Assert.Single(result);
        Assert.Contains("Juan", result.First().Nombres);
    }
}