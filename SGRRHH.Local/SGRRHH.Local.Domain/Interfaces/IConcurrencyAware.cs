namespace SGRRHH.Local.Domain.Interfaces;

/// <summary>
/// Interface para entidades que soportan control de concurrencia optimista.
/// Usar FechaModificacion como token de versión.
/// </summary>
public interface IConcurrencyAware
{
    /// <summary>
    /// Fecha de última modificación. Se usa como token de concurrencia.
    /// Al actualizar, verificar que este valor coincida con el de la DB.
    /// </summary>
    DateTime? FechaModificacion { get; set; }
}

/// <summary>
/// Excepción lanzada cuando se detecta un conflicto de concurrencia.
/// Indica que otro usuario modificó el registro mientras este usuario lo editaba.
/// </summary>
public class ConcurrencyConflictException : Exception
{
    public string EntidadNombre { get; }
    public int EntidadId { get; }
    public DateTime? VersionEsperada { get; }
    public DateTime? VersionActual { get; }

    public ConcurrencyConflictException(
        string entidadNombre, 
        int entidadId, 
        DateTime? versionEsperada, 
        DateTime? versionActual)
        : base($"El registro de {entidadNombre} (ID: {entidadId}) fue modificado por otro usuario. " +
               $"Por favor, recargue los datos e intente nuevamente.")
    {
        EntidadNombre = entidadNombre;
        EntidadId = entidadId;
        VersionEsperada = versionEsperada;
        VersionActual = versionActual;
    }
}
