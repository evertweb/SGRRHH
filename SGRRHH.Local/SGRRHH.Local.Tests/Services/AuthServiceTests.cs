using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using SGRRHH.Local.Domain.Entities;
using SGRRHH.Local.Domain.Enums;
using SGRRHH.Local.Infrastructure.Repositories;
using SGRRHH.Local.Infrastructure.Services;
using SGRRHH.Local.Shared.Interfaces;
using Xunit;

namespace SGRRHH.Local.Tests.Services;

[Collection("Database")]
public class AuthServiceTests
{
    private readonly TestDatabaseFixture _fixture;
    private readonly UsuarioRepository _usuarioRepository;
    private readonly LocalAuthService _authService;

    public AuthServiceTests(TestDatabaseFixture fixture)
    {
        _fixture = fixture;
        _fixture.CleanDatabaseAsync().Wait();

        var userLogger = Substitute.For<ILogger<UsuarioRepository>>();
        _usuarioRepository = new UsuarioRepository(_fixture.Context, userLogger);

        var auditLogger = Substitute.For<ILogger<LocalAuthService>>();
        var auditRepo = Substitute.For<IAuditLogRepository>();
        auditRepo.AddAsync(Arg.Any<AuditLog>()).Returns(ci => ci.Arg<AuditLog>());

        _authService = new LocalAuthService(_usuarioRepository, auditRepo, auditLogger);
    }

    [Fact]
    public async Task LoginAsync_WithValidCredentials_Succeeds()
    {
        // Arrange
        var user = await CreateUserAsync("testadmin", "Admin123!", RolUsuario.Administrador);

        // Act
        var result = await _authService.LoginAsync(user.Username, "Admin123!");

        // Assert
        result.IsSuccess.Should().BeTrue();
        _authService.IsAuthenticated.Should().BeTrue();
        _authService.CurrentUser!.Id.Should().Be(user.Id);
    }

    [Fact]
    public async Task LoginAsync_WithWrongPassword_Fails()
    {
        // Arrange
        await CreateUserAsync("testuser", "Correct123", RolUsuario.Operador);

        // Act
        var result = await _authService.LoginAsync("testuser", "Wrong!");

        // Assert
        result.IsSuccess.Should().BeFalse();
        _authService.IsAuthenticated.Should().BeFalse();
    }

    [Fact]
    public async Task ChangePasswordAsync_WithValidOldPassword_UpdatesHash()
    {
        // Arrange
        var user = await CreateUserAsync("testchangepass", "OldPass1", RolUsuario.Operador);
        await _authService.LoginAsync(user.Username, "OldPass1");

        // Act
        var changeResult = await _authService.ChangePasswordAsync(user.Id, "OldPass1", "NewPass2");

        // Assert
        changeResult.IsSuccess.Should().BeTrue();

        await _authService.LogoutAsync();
        var loginNew = await _authService.LoginAsync(user.Username, "NewPass2");
        loginNew.IsSuccess.Should().BeTrue();
    }

    private async Task<Usuario> CreateUserAsync(string username, string password, RolUsuario rol, bool activo = true)
    {
        var usuario = new Usuario
        {
            Username = username,
            PasswordHash = _authService.HashPassword(password),
            NombreCompleto = "Tester",
            Rol = rol,
            Activo = activo
        };

        return await _usuarioRepository.AddAsync(usuario);
    }
}
