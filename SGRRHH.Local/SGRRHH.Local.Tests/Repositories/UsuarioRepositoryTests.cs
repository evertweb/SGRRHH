using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using SGRRHH.Local.Domain.Entities;
using SGRRHH.Local.Domain.Enums;
using SGRRHH.Local.Infrastructure.Repositories;
using Xunit;

namespace SGRRHH.Local.Tests.Repositories;

[Collection("Database")]
public class UsuarioRepositoryTests
{
    private readonly TestDatabaseFixture _fixture;
    private readonly UsuarioRepository _repository;

    public UsuarioRepositoryTests(TestDatabaseFixture fixture)
    {
        _fixture = fixture;
        var logger = Substitute.For<ILogger<UsuarioRepository>>();
        _repository = new UsuarioRepository(_fixture.Context, logger);
        _fixture.CleanDatabaseAsync().Wait();
    }

    [Fact]
    public async Task AddAsync_WithValidUser_ReturnsUserWithId()
    {
        // Arrange
        var usuario = new Usuario
        {
            Username = "admin",
            PasswordHash = "hash",
            NombreCompleto = "Admin User",
            Rol = RolUsuario.Administrador,
            Activo = true
        };

        // Act
        var saved = await _repository.AddAsync(usuario);
        var stored = await _repository.GetByIdAsync(saved.Id);

        // Assert
        saved.Id.Should().BeGreaterThan(0);
        stored.Should().NotBeNull();
        stored!.Username.Should().Be("admin");
    }

    [Fact]
    public async Task ExistsUsernameAsync_IsCaseInsensitive()
    {
        // Arrange
        await _repository.AddAsync(new Usuario
        {
            Username = "MiUsuario",
            PasswordHash = "hash",
            NombreCompleto = "Tester",
            Rol = RolUsuario.Operador,
            Activo = true
        });

        // Act
        var exists = await _repository.ExistsUsernameAsync("miusuario");

        // Assert
        exists.Should().BeTrue();
    }

    [Fact]
    public async Task DeleteAsync_SoftDeletesUser()
    {
        // Arrange
        var usuario = await _repository.AddAsync(new Usuario
        {
            Username = "softdelete",
            PasswordHash = "hash",
            NombreCompleto = "To Delete",
            Rol = RolUsuario.Operador,
            Activo = true
        });

        // Act
        await _repository.DeleteAsync(usuario.Id);

        // Assert
        var active = await _repository.GetAllActiveAsync();
        active.Should().BeEmpty();

        var stored = await _repository.GetByIdAsync(usuario.Id);
        stored.Should().NotBeNull();
        stored!.Activo.Should().BeFalse();
    }

    [Fact]
    public async Task UpdateAsync_ChangesUserFields()
    {
        // Arrange
        var usuario = await _repository.AddAsync(new Usuario
        {
            Username = "toedit",
            PasswordHash = "hash",
            NombreCompleto = "Original",
            Rol = RolUsuario.Operador,
            Activo = true
        });

        usuario.NombreCompleto = "Modificado";
        usuario.Email = "mail@test.com";

        // Act
        await _repository.UpdateAsync(usuario);
        var stored = await _repository.GetByIdAsync(usuario.Id);

        // Assert
        stored!.NombreCompleto.Should().Be("Modificado");
        stored.Email.Should().Be("mail@test.com");
    }
}
