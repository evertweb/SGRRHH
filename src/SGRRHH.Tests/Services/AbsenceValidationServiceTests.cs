using Moq;
using SGRRHH.Core.Interfaces;
using SGRRHH.Infrastructure.Services;
using Xunit;

namespace SGRRHH.Tests.Services;

/// <summary>
/// Pruebas unitarias para AbsenceValidationService
/// </summary>
public class AbsenceValidationServiceTests
{
    private readonly Mock<IPermisoRepository> _mockPermisoRepo;
    private readonly Mock<IVacacionRepository> _mockVacacionRepo;
    private readonly AbsenceValidationService _service;

    public AbsenceValidationServiceTests()
    {
        _mockPermisoRepo = new Mock<IPermisoRepository>();
        _mockVacacionRepo = new Mock<IVacacionRepository>();
        _service = new AbsenceValidationService(_mockPermisoRepo.Object, _mockVacacionRepo.Object);
    }

    #region ValidarDisponibilidadAsync Tests

    [Fact]
    public async Task ValidarDisponibilidadAsync_NoConflicts_ReturnsSuccess()
    {
        // Arrange
        var empleadoId = 1;
        var fechaInicio = new DateTime(2024, 3, 1);
        var fechaFin = new DateTime(2024, 3, 5);

        _mockPermisoRepo.Setup(r => r.ExisteSolapamientoAsync(empleadoId, fechaInicio, fechaFin, null))
            .ReturnsAsync(false);
        
        _mockVacacionRepo.Setup(r => r.ExisteTraslapeAsync(empleadoId, fechaInicio, fechaFin, null))
            .ReturnsAsync(false);

        // Act
        var result = await _service.ValidarDisponibilidadAsync(empleadoId, fechaInicio, fechaFin);

        // Assert
        Assert.True(result.Success);
    }

    [Fact]
    public async Task ValidarDisponibilidadAsync_WithPermiso_ReturnsFail()
    {
        // Arrange
        var empleadoId = 1;
        var fechaInicio = new DateTime(2024, 3, 1);
        var fechaFin = new DateTime(2024, 3, 5);

        _mockPermisoRepo.Setup(r => r.ExisteSolapamientoAsync(empleadoId, fechaInicio, fechaFin, null))
            .ReturnsAsync(true); // Hay solapamiento con permiso
        
        _mockVacacionRepo.Setup(r => r.ExisteTraslapeAsync(empleadoId, fechaInicio, fechaFin, null))
            .ReturnsAsync(false);

        // Act
        var result = await _service.ValidarDisponibilidadAsync(empleadoId, fechaInicio, fechaFin);

        // Assert
        Assert.False(result.Success);
        Assert.Contains("permiso", result.Message?.ToLower() ?? "");
    }

    [Fact]
    public async Task ValidarDisponibilidadAsync_WithVacacion_ReturnsFail()
    {
        // Arrange
        var empleadoId = 1;
        var fechaInicio = new DateTime(2024, 3, 1);
        var fechaFin = new DateTime(2024, 3, 5);

        _mockPermisoRepo.Setup(r => r.ExisteSolapamientoAsync(empleadoId, fechaInicio, fechaFin, null))
            .ReturnsAsync(false);
        
        _mockVacacionRepo.Setup(r => r.ExisteTraslapeAsync(empleadoId, fechaInicio, fechaFin, null))
            .ReturnsAsync(true); // Hay solapamiento con vacación

        // Act
        var result = await _service.ValidarDisponibilidadAsync(empleadoId, fechaInicio, fechaFin);

        // Assert
        Assert.False(result.Success);
        Assert.Contains("vacacion", result.Message?.ToLower() ?? "");
    }

    [Fact]
    public async Task ValidarDisponibilidadAsync_ExcludesCurrentPermiso()
    {
        // Arrange
        var empleadoId = 1;
        var permisoId = 1;
        var fechaInicio = new DateTime(2024, 3, 1);
        var fechaFin = new DateTime(2024, 3, 5);

        // Verifica que se pasa el excludePermisoId correctamente
        _mockPermisoRepo.Setup(r => r.ExisteSolapamientoAsync(empleadoId, fechaInicio, fechaFin, permisoId))
            .ReturnsAsync(false);
        
        _mockVacacionRepo.Setup(r => r.ExisteTraslapeAsync(empleadoId, fechaInicio, fechaFin, null))
            .ReturnsAsync(false);

        // Act
        var result = await _service.ValidarDisponibilidadAsync(
            empleadoId, fechaInicio, fechaFin, 
            excludePermisoId: permisoId);

        // Assert
        Assert.True(result.Success);
        _mockPermisoRepo.Verify(r => r.ExisteSolapamientoAsync(empleadoId, fechaInicio, fechaFin, permisoId), Times.Once);
    }

    [Fact]
    public async Task ValidarDisponibilidadAsync_ExcludesCurrentVacacion()
    {
        // Arrange
        var empleadoId = 1;
        var vacacionId = 1;
        var fechaInicio = new DateTime(2024, 3, 1);
        var fechaFin = new DateTime(2024, 3, 5);

        _mockPermisoRepo.Setup(r => r.ExisteSolapamientoAsync(empleadoId, fechaInicio, fechaFin, null))
            .ReturnsAsync(false);
        
        // Verifica que se pasa el excludeVacacionId correctamente
        _mockVacacionRepo.Setup(r => r.ExisteTraslapeAsync(empleadoId, fechaInicio, fechaFin, vacacionId))
            .ReturnsAsync(false);

        // Act
        var result = await _service.ValidarDisponibilidadAsync(
            empleadoId, fechaInicio, fechaFin, 
            excludeVacacionId: vacacionId);

        // Assert
        Assert.True(result.Success);
        _mockVacacionRepo.Verify(r => r.ExisteTraslapeAsync(empleadoId, fechaInicio, fechaFin, vacacionId), Times.Once);
    }

    #endregion

    #region TienePermisosEnRangoAsync Tests

    [Fact]
    public async Task TienePermisosEnRangoAsync_WithOverlap_ReturnsTrue()
    {
        // Arrange
        var empleadoId = 1;
        var fechaInicio = new DateTime(2024, 3, 1);
        var fechaFin = new DateTime(2024, 3, 10);

        _mockPermisoRepo.Setup(r => r.ExisteSolapamientoAsync(empleadoId, fechaInicio, fechaFin, null))
            .ReturnsAsync(true);

        // Act
        var result = await _service.TienePermisosEnRangoAsync(empleadoId, fechaInicio, fechaFin);

        // Assert
        Assert.True(result.Success);
        Assert.True(result.Data);
    }

    [Fact]
    public async Task TienePermisosEnRangoAsync_NoOverlap_ReturnsFalse()
    {
        // Arrange
        var empleadoId = 1;
        var fechaInicio = new DateTime(2024, 3, 1);
        var fechaFin = new DateTime(2024, 3, 5);

        _mockPermisoRepo.Setup(r => r.ExisteSolapamientoAsync(empleadoId, fechaInicio, fechaFin, null))
            .ReturnsAsync(false);

        // Act
        var result = await _service.TienePermisosEnRangoAsync(empleadoId, fechaInicio, fechaFin);

        // Assert
        Assert.True(result.Success);
        Assert.False(result.Data);
    }

    #endregion

    #region TieneVacacionesEnRangoAsync Tests

    [Fact]
    public async Task TieneVacacionesEnRangoAsync_WithOverlap_ReturnsTrue()
    {
        // Arrange
        var empleadoId = 1;
        var fechaInicio = new DateTime(2024, 3, 1);
        var fechaFin = new DateTime(2024, 3, 10);

        _mockVacacionRepo.Setup(r => r.ExisteTraslapeAsync(empleadoId, fechaInicio, fechaFin, null))
            .ReturnsAsync(true);

        // Act
        var result = await _service.TieneVacacionesEnRangoAsync(empleadoId, fechaInicio, fechaFin);

        // Assert
        Assert.True(result.Success);
        Assert.True(result.Data);
    }

    [Fact]
    public async Task TieneVacacionesEnRangoAsync_NoOverlap_ReturnsFalse()
    {
        // Arrange
        var empleadoId = 1;
        var fechaInicio = new DateTime(2024, 3, 1);
        var fechaFin = new DateTime(2024, 3, 5);

        _mockVacacionRepo.Setup(r => r.ExisteTraslapeAsync(empleadoId, fechaInicio, fechaFin, null))
            .ReturnsAsync(false);

        // Act
        var result = await _service.TieneVacacionesEnRangoAsync(empleadoId, fechaInicio, fechaFin);

        // Assert
        Assert.True(result.Success);
        Assert.False(result.Data);
    }

    #endregion

    #region Error Handling Tests

    [Fact]
    public async Task TienePermisosEnRangoAsync_OnException_ReturnsFailure()
    {
        // Arrange
        var empleadoId = 1;
        var fechaInicio = new DateTime(2024, 3, 1);
        var fechaFin = new DateTime(2024, 3, 5);

        _mockPermisoRepo.Setup(r => r.ExisteSolapamientoAsync(It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<int?>()))
            .ThrowsAsync(new Exception("Error de conexión"));

        // Act
        var result = await _service.TienePermisosEnRangoAsync(empleadoId, fechaInicio, fechaFin);

        // Assert
        Assert.False(result.Success);
        Assert.Contains("Error", result.Message ?? "");
    }

    [Fact]
    public async Task TieneVacacionesEnRangoAsync_OnException_ReturnsFailure()
    {
        // Arrange
        var empleadoId = 1;
        var fechaInicio = new DateTime(2024, 3, 1);
        var fechaFin = new DateTime(2024, 3, 5);

        _mockVacacionRepo.Setup(r => r.ExisteTraslapeAsync(It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<int?>()))
            .ThrowsAsync(new Exception("Error de conexión"));

        // Act
        var result = await _service.TieneVacacionesEnRangoAsync(empleadoId, fechaInicio, fechaFin);

        // Assert
        Assert.False(result.Success);
        Assert.Contains("Error", result.Message ?? "");
    }

    #endregion
}
