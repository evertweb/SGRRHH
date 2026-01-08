using System.Collections.Generic;
using System.IO.Compression;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NSubstitute;
using SGRRHH.Local.Infrastructure.Data;
using SGRRHH.Local.Infrastructure.Services;
using Xunit;

namespace SGRRHH.Local.Tests.Services;

public class BackupServiceTests
{
    [Fact]
    public async Task CreateBackupAsync_CreatesDatabaseCopyAndMetadata()
    {
        await WithTempPaths(async (service, paths) =>
        {
            // Arrange: crear archivos simulados
            Directory.CreateDirectory(Path.GetDirectoryName(paths.DatabasePath)!);
            await File.WriteAllTextAsync(paths.DatabasePath, "DB-CONTENT");

            var fotos = Path.Combine(paths.StoragePath, "Fotos");
            var docs = Path.Combine(paths.StoragePath, "Documentos");
            Directory.CreateDirectory(fotos);
            Directory.CreateDirectory(docs);
            await File.WriteAllTextAsync(Path.Combine(fotos, "foto.png"), "img");
            await File.WriteAllTextAsync(Path.Combine(docs, "doc.pdf"), "pdf");

            // Act
            var result = await service.CreateBackupAsync("backup de prueba");

            // Assert
            result.IsSuccess.Should().BeTrue();
            var backupDir = Directory.GetDirectories(paths.BackupPath).Single();
            File.Exists(Path.Combine(backupDir, "sgrrhh.db")).Should().BeTrue();
            File.Exists(Path.Combine(backupDir, "backup.json")).Should().BeTrue();
            File.Exists(Path.Combine(backupDir, "Fotos.zip")).Should().BeTrue();
            File.Exists(Path.Combine(backupDir, "Documentos.zip")).Should().BeTrue();
        });
    }

    [Fact]
    public async Task RestoreFromBackupAsync_ReplacesDatabaseFile()
    {
        await WithTempPaths(async (service, paths) =>
        {
            // Arrange: preparar backup existente
            Directory.CreateDirectory(Path.GetDirectoryName(paths.DatabasePath)!);
            await File.WriteAllTextAsync(paths.DatabasePath, "OLD-DB");

            var backupFolderName = "2026-restore-test";
            var backupDir = Path.Combine(paths.BackupPath, backupFolderName);
            Directory.CreateDirectory(backupDir);
            await File.WriteAllTextAsync(Path.Combine(backupDir, "sgrrhh.db"), "NEW-DB");

            // Crear archivos comprimidos mínimos
            var fotosSource = Path.Combine(paths.Root, "seed-fotos");
            Directory.CreateDirectory(fotosSource);
            await File.WriteAllTextAsync(Path.Combine(fotosSource, "img.png"), "img");
            ZipFile.CreateFromDirectory(fotosSource, Path.Combine(backupDir, "Fotos.zip"));

            var docsSource = Path.Combine(paths.Root, "seed-docs");
            Directory.CreateDirectory(docsSource);
            await File.WriteAllTextAsync(Path.Combine(docsSource, "doc.txt"), "doc");
            ZipFile.CreateFromDirectory(docsSource, Path.Combine(backupDir, "Documentos.zip"));

            // Act
            var result = await service.RestoreFromBackupAsync(backupFolderName);

            // Assert
            result.IsSuccess.Should().BeTrue();
            File.ReadAllText(paths.DatabasePath).Should().Be("NEW-DB");

            Directory.Exists(Path.Combine(paths.StoragePath, "Fotos")).Should().BeTrue();
            Directory.Exists(Path.Combine(paths.StoragePath, "Documentos")).Should().BeTrue();
        });
    }

    private async Task WithTempPaths(Func<BackupService, BackupPaths, Task> action)
    {
        var root = Path.Combine(Path.GetTempPath(), $"sgrrhh_backup_test_{Guid.NewGuid():N}");
        var databasePath = Path.Combine(root, "data", "sgrrhh.db");
        var storagePath = Path.Combine(root, "storage");
        var backupPath = Path.Combine(root, "backups");

        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "LocalDatabase:DatabasePath", databasePath },
                { "LocalDatabase:StoragePath", storagePath },
                { "LocalDatabase:BackupPath", backupPath }
            })
            .Build();

        var pathResolver = new DatabasePathResolver(config, Substitute.For<ILogger<DatabasePathResolver>>());
        var service = new BackupService(pathResolver, config, Substitute.For<ILogger<BackupService>>());

        try
        {
            await action(service, new BackupPaths(root, databasePath, storagePath, backupPath));
        }
        finally
        {
            if (Directory.Exists(root))
            {
                Directory.Delete(root, recursive: true);
            }
        }
    }

    private record BackupPaths(string Root, string DatabasePath, string StoragePath, string BackupPath);
}
