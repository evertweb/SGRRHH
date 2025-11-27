using Microsoft.EntityFrameworkCore;
using SGRRHH.Core.Entities;
using SGRRHH.Core.Interfaces;
using SGRRHH.Infrastructure.Data;

namespace SGRRHH.Infrastructure.Repositories;

/// <summary>
/// Implementación del repositorio de tipos de permiso
/// </summary>
public class TipoPermisoRepository : Repository<TipoPermiso>, ITipoPermisoRepository
{
    public TipoPermisoRepository(AppDbContext context) : base(context)
    {
    }

    /// <summary>
    /// Obtiene todos los tipos de permiso activos
    /// </summary>
    public async Task<IEnumerable<TipoPermiso>> GetActivosAsync()
    {
        return await _context.Set<TipoPermiso>()
            .Where(tp => tp.Activo)
            .OrderBy(tp => tp.Nombre)
            .ToListAsync();
    }

    /// <summary>
    /// Verifica si existe un tipo de permiso con el nombre especificado
    /// </summary>
    public async Task<bool> ExisteNombreAsync(string nombre, int? excludeId = null)
    {
        var query = _context.Set<TipoPermiso>()
            .Where(tp => tp.Nombre.ToLower() == nombre.ToLower());

        if (excludeId.HasValue)
        {
            query = query.Where(tp => tp.Id != excludeId.Value);
        }

        return await query.AnyAsync();
    }
}
