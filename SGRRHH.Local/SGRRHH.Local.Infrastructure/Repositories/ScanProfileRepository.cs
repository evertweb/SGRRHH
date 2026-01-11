using Dapper;
using Microsoft.Extensions.Logging;
using SGRRHH.Local.Domain.DTOs;
using SGRRHH.Local.Shared.Interfaces;

namespace SGRRHH.Local.Infrastructure.Repositories;

/// <summary>
/// Repositorio para gestionar perfiles de escaneo guardados en la base de datos.
/// Usa tabla scan_profiles (snake_case estandarizado).
/// </summary>
public class ScanProfileRepository : IScanProfileRepository
{
    private readonly Data.DapperContext _context;
    private readonly ILogger<ScanProfileRepository> _logger;

    public ScanProfileRepository(Data.DapperContext context, ILogger<ScanProfileRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<List<ScanProfileDto>> GetAllAsync()
    {
        const string sql = @"
            SELECT id, name, description, is_default, dpi, color_mode, source, page_size,
                   brightness, contrast, gamma, sharpness, black_white_threshold,
                   auto_deskew, auto_crop, created_at, last_used_at
            FROM scan_profiles
            ORDER BY is_default DESC, name";
        
        using var connection = _context.CreateConnection();
        var results = await connection.QueryAsync<ScanProfileDbRow>(sql);
        
        return results.Select(MapToDto).ToList();
    }

    public async Task<ScanProfileDto?> GetByIdAsync(int id)
    {
        const string sql = @"
            SELECT id, name, description, is_default, dpi, color_mode, source, page_size,
                   brightness, contrast, gamma, sharpness, black_white_threshold,
                   auto_deskew, auto_crop, created_at, last_used_at
            FROM scan_profiles
            WHERE id = @Id";
        
        using var connection = _context.CreateConnection();
        var row = await connection.QuerySingleOrDefaultAsync<ScanProfileDbRow>(sql, new { Id = id });
        
        return row != null ? MapToDto(row) : null;
    }

    public async Task<ScanProfileDto?> GetDefaultAsync()
    {
        const string sql = @"
            SELECT id, name, description, is_default, dpi, color_mode, source, page_size,
                   brightness, contrast, gamma, sharpness, black_white_threshold,
                   auto_deskew, auto_crop, created_at, last_used_at
            FROM scan_profiles
            WHERE is_default = 1
            LIMIT 1";
        
        using var connection = _context.CreateConnection();
        var row = await connection.QuerySingleOrDefaultAsync<ScanProfileDbRow>(sql);
        
        return row != null ? MapToDto(row) : null;
    }

    public async Task<ScanProfileDto> SaveAsync(ScanProfileDto profile)
    {
        using var connection = _context.CreateConnection();
        
        if (profile.Id == 0)
        {
            // Insertar nuevo
            const string insertSql = @"
                INSERT INTO scan_profiles 
                    (name, description, is_default, dpi, color_mode, source, page_size,
                     brightness, contrast, gamma, sharpness, black_white_threshold,
                     auto_deskew, auto_crop, created_at)
                VALUES 
                    (@Name, @Description, @IsDefault, @Dpi, @ColorMode, @Source, @PageSize,
                     @Brightness, @Contrast, @Gamma, @Sharpness, @BlackWhiteThreshold,
                     @AutoDeskew, @AutoCrop, @CreatedAt);
                SELECT last_insert_rowid();";
            
            profile.CreatedAt = DateTime.Now;
            profile.Id = await connection.ExecuteScalarAsync<int>(insertSql, MapToRow(profile));
            
            _logger.LogInformation("Perfil de escaneo creado: {Name} (ID: {Id})", profile.Name, profile.Id);
        }
        else
        {
            // Actualizar existente
            const string updateSql = @"
                UPDATE scan_profiles SET
                    name = @Name,
                    description = @Description,
                    is_default = @IsDefault,
                    dpi = @Dpi,
                    color_mode = @ColorMode,
                    source = @Source,
                    page_size = @PageSize,
                    brightness = @Brightness,
                    contrast = @Contrast,
                    gamma = @Gamma,
                    sharpness = @Sharpness,
                    black_white_threshold = @BlackWhiteThreshold,
                    auto_deskew = @AutoDeskew,
                    auto_crop = @AutoCrop
                WHERE id = @Id";
            
            await connection.ExecuteAsync(updateSql, MapToRow(profile));
            
            _logger.LogInformation("Perfil de escaneo actualizado: {Name} (ID: {Id})", profile.Name, profile.Id);
        }
        
        return profile;
    }

    public async Task DeleteAsync(int id)
    {
        const string sql = "DELETE FROM scan_profiles WHERE id = @Id";
        
        using var connection = _context.CreateConnection();
        await connection.ExecuteAsync(sql, new { Id = id });
        
        _logger.LogInformation("Perfil de escaneo eliminado: ID {Id}", id);
    }

    public async Task SetDefaultAsync(int id)
    {
        using var connection = _context.CreateConnection();
        
        // Primero quitar el default de todos
        await connection.ExecuteAsync("UPDATE scan_profiles SET is_default = 0");
        
        // Establecer el nuevo default
        await connection.ExecuteAsync("UPDATE scan_profiles SET is_default = 1 WHERE id = @Id", new { Id = id });
        
        _logger.LogInformation("Perfil de escaneo establecido como predeterminado: ID {Id}", id);
    }

    public async Task UpdateLastUsedAsync(int id)
    {
        const string sql = "UPDATE scan_profiles SET last_used_at = @LastUsedAt WHERE id = @Id";
        
        using var connection = _context.CreateConnection();
        await connection.ExecuteAsync(sql, new { Id = id, LastUsedAt = DateTime.Now.ToString("o") });
    }

    /// <summary>
    /// Inicializa el perfil predeterminado DOCUMENTO si no existe
    /// </summary>
    public async Task InitializeDefaultProfilesAsync()
    {
        try
        {
            var existingProfiles = await GetAllAsync();
            if (existingProfiles.Count > 0)
            {
                _logger.LogDebug("Ya existen {Count} perfiles de escaneo, omitiendo inicialización", existingProfiles.Count);
                return;
            }
            
            _logger.LogInformation("Inicializando perfil de escaneo DOCUMENTO...");
            
            // Un solo perfil basado en las opciones de IJ Scan (Canon)
            var documentoProfile = new ScanProfileDto
            {
                Name = "DOCUMENTO",
                Description = "Perfil estándar para escaneo de documentos",
                IsDefault = true,
                Dpi = 300,
                ColorMode = ScanColorMode.Color,
                Source = ScanSource.Flatbed,
                PageSize = ScanPageSize.Letter,
                Brightness = 0,
                Contrast = 0,
                Gamma = 1.0f,
                Sharpness = 0,
                AutoDeskew = false,
                AutoCrop = false
            };
            
            await SaveAsync(documentoProfile);
            _logger.LogInformation("Perfil DOCUMENTO creado exitosamente");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al inicializar perfil de escaneo predeterminado");
        }
    }

    #region Mapeo DB <-> DTO

    private static ScanProfileDto MapToDto(ScanProfileDbRow row)
    {
        return new ScanProfileDto
        {
            Id = row.Id,
            Name = row.Name,
            Description = row.Description,
            IsDefault = row.IsDefault == 1,
            Dpi = row.Dpi,
            ColorMode = Enum.TryParse<ScanColorMode>(row.ColorMode, out var cm) ? cm : ScanColorMode.Color,
            Source = Enum.TryParse<ScanSource>(row.Source, out var src) ? src : ScanSource.Flatbed,
            PageSize = Enum.TryParse<ScanPageSize>(row.PageSize, out var ps) ? ps : ScanPageSize.Letter,
            Brightness = row.Brightness,
            Contrast = row.Contrast,
            Gamma = row.Gamma,
            Sharpness = row.Sharpness,
            BlackWhiteThreshold = row.BlackWhiteThreshold,
            AutoDeskew = row.AutoDeskew == 1,
            AutoCrop = row.AutoCrop == 1,
            CreatedAt = DateTime.TryParse(row.CreatedAt, out var ca) ? ca : DateTime.Now,
            LastUsedAt = string.IsNullOrEmpty(row.LastUsedAt) ? null : DateTime.TryParse(row.LastUsedAt, out var lu) ? lu : null
        };
    }

    private static object MapToRow(ScanProfileDto dto)
    {
        return new
        {
            dto.Id,
            dto.Name,
            dto.Description,
            IsDefault = dto.IsDefault ? 1 : 0,
            dto.Dpi,
            ColorMode = dto.ColorMode.ToString(),
            Source = dto.Source.ToString(),
            PageSize = dto.PageSize.ToString(),
            dto.Brightness,
            dto.Contrast,
            dto.Gamma,
            dto.Sharpness,
            dto.BlackWhiteThreshold,
            AutoDeskew = dto.AutoDeskew ? 1 : 0,
            AutoCrop = dto.AutoCrop ? 1 : 0,
            CreatedAt = dto.CreatedAt.ToString("o")
        };
    }

    /// <summary>
    /// Clase auxiliar para mapear desde la base de datos (snake_case)
    /// </summary>
    private class ScanProfileDbRow
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int IsDefault { get; set; }
        public int Dpi { get; set; }
        public string ColorMode { get; set; } = "Color";
        public string Source { get; set; } = "Flatbed";
        public string PageSize { get; set; } = "Letter";
        public int Brightness { get; set; }
        public int Contrast { get; set; }
        public float Gamma { get; set; } = 1.0f;
        public int Sharpness { get; set; }
        public int BlackWhiteThreshold { get; set; } = 128;
        public int AutoDeskew { get; set; }
        public int AutoCrop { get; set; }
        public string CreatedAt { get; set; } = string.Empty;
        public string? LastUsedAt { get; set; }
    }

    #endregion
}
