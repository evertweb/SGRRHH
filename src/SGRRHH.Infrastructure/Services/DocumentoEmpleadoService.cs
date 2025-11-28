using SGRRHH.Core.Common;
using SGRRHH.Core.Entities;
using SGRRHH.Core.Enums;
using SGRRHH.Core.Interfaces;

namespace SGRRHH.Infrastructure.Services;

/// <summary>
/// Implementación del servicio de documentos de empleados
/// Incluye control de permisos por rol:
/// - Administrador: Puede subir, editar y eliminar documentos
/// - Operador: Puede subir y editar documentos
/// - Aprobador: Solo puede ver documentos
/// </summary>
public class DocumentoEmpleadoService : IDocumentoEmpleadoService
{
    private readonly IDocumentoEmpleadoRepository _repository;
    private readonly IEmpleadoRepository _empleadoRepository;
    private readonly string _documentosPath;
    
    /// <summary>
    /// Lista de tipos de documentos considerados requeridos/obligatorios
    /// </summary>
    private static readonly TipoDocumentoEmpleado[] DocumentosRequeridos = new[]
    {
        TipoDocumentoEmpleado.Cedula,
        TipoDocumentoEmpleado.HojaVida,
        TipoDocumentoEmpleado.ExamenMedicoIngreso,
        TipoDocumentoEmpleado.AfiliacionEPS,
        TipoDocumentoEmpleado.AfiliacionAFP,
        TipoDocumentoEmpleado.AfiliacionARL
    };
    
    /// <summary>
    /// Lista de tipos de documentos opcionales/recomendados
    /// </summary>
    private static readonly TipoDocumentoEmpleado[] DocumentosOpcionales = new[]
    {
        TipoDocumentoEmpleado.CertificadoEstudios,
        TipoDocumentoEmpleado.CertificadoLaboral,
        TipoDocumentoEmpleado.ReferenciasPersonales,
        TipoDocumentoEmpleado.ReferenciasLaborales,
        TipoDocumentoEmpleado.Antecedentes,
        TipoDocumentoEmpleado.AfiliacionCajaCompensacion,
        TipoDocumentoEmpleado.RUT,
        TipoDocumentoEmpleado.CertificadoBancario,
        TipoDocumentoEmpleado.LicenciaConduccion,
        TipoDocumentoEmpleado.LibretaMilitar
    };
    
    public DocumentoEmpleadoService(
        IDocumentoEmpleadoRepository repository,
        IEmpleadoRepository empleadoRepository)
    {
        _repository = repository;
        _empleadoRepository = empleadoRepository;
        
        // Configurar ruta de documentos
        _documentosPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data", "documentos_empleados");
        Directory.CreateDirectory(_documentosPath);
    }
    
    /// <summary>
    /// Verifica si el usuario tiene permisos para gestionar documentos
    /// Administrador y Operador pueden gestionar
    /// </summary>
    public bool PuedeGestionarDocumentos(RolUsuario rol)
    {
        return rol == RolUsuario.Administrador || rol == RolUsuario.Operador;
    }
    
    /// <summary>
    /// Verifica si el usuario puede eliminar documentos
    /// Solo Administrador puede eliminar
    /// </summary>
    public bool PuedeEliminarDocumentos(RolUsuario rol)
    {
        return rol == RolUsuario.Administrador;
    }
    
    /// <summary>
    /// Obtiene todos los documentos de un empleado
    /// </summary>
    public async Task<ServiceResult<IEnumerable<DocumentoEmpleado>>> GetByEmpleadoIdAsync(int empleadoId)
    {
        try
        {
            var documentos = await _repository.GetByEmpleadoIdAsync(empleadoId);
            return ServiceResult<IEnumerable<DocumentoEmpleado>>.Ok(documentos);
        }
        catch (Exception ex)
        {
            return ServiceResult<IEnumerable<DocumentoEmpleado>>.Fail($"Error al obtener documentos: {ex.Message}");
        }
    }
    
    /// <summary>
    /// Obtiene documentos de un empleado por tipo
    /// </summary>
    public async Task<ServiceResult<IEnumerable<DocumentoEmpleado>>> GetByEmpleadoIdAndTipoAsync(int empleadoId, TipoDocumentoEmpleado tipo)
    {
        try
        {
            var documentos = await _repository.GetByEmpleadoIdAndTipoAsync(empleadoId, tipo);
            return ServiceResult<IEnumerable<DocumentoEmpleado>>.Ok(documentos);
        }
        catch (Exception ex)
        {
            return ServiceResult<IEnumerable<DocumentoEmpleado>>.Fail($"Error al obtener documentos: {ex.Message}");
        }
    }
    
    /// <summary>
    /// Obtiene un documento por su ID
    /// </summary>
    public async Task<ServiceResult<DocumentoEmpleado>> GetByIdAsync(int id)
    {
        try
        {
            var documento = await _repository.GetByIdAsync(id);
            if (documento == null)
            {
                return ServiceResult<DocumentoEmpleado>.Fail("Documento no encontrado");
            }
            return ServiceResult<DocumentoEmpleado>.Ok(documento);
        }
        catch (Exception ex)
        {
            return ServiceResult<DocumentoEmpleado>.Fail($"Error al obtener documento: {ex.Message}");
        }
    }
    
    /// <summary>
    /// Sube y registra un nuevo documento
    /// Requiere rol Administrador u Operador
    /// </summary>
    public async Task<ServiceResult<DocumentoEmpleado>> SubirDocumentoAsync(
        DocumentoEmpleado documento,
        byte[] archivoBytes,
        int usuarioId,
        RolUsuario rolUsuario)
    {
        try
        {
            // Validar permisos
            if (!PuedeGestionarDocumentos(rolUsuario))
            {
                return ServiceResult<DocumentoEmpleado>.Fail("No tiene permisos para subir documentos");
            }
            
            // Validar datos
            if (documento.EmpleadoId <= 0)
            {
                return ServiceResult<DocumentoEmpleado>.Fail("Debe especificar un empleado");
            }
            
            if (string.IsNullOrWhiteSpace(documento.Nombre))
            {
                return ServiceResult<DocumentoEmpleado>.Fail("El nombre del documento es requerido");
            }
            
            if (archivoBytes == null || archivoBytes.Length == 0)
            {
                return ServiceResult<DocumentoEmpleado>.Fail("El archivo es requerido");
            }
            
            // Verificar que el empleado existe
            var empleado = await _empleadoRepository.GetByIdAsync(documento.EmpleadoId);
            if (empleado == null)
            {
                return ServiceResult<DocumentoEmpleado>.Fail("Empleado no encontrado");
            }
            
            // Crear carpeta del empleado
            var empleadoFolder = Path.Combine(_documentosPath, $"emp_{empleado.Id}_{empleado.Cedula}");
            Directory.CreateDirectory(empleadoFolder);
            
            // Generar nombre de archivo único
            var extension = Path.GetExtension(documento.NombreArchivoOriginal);
            var nuevoNombre = $"{documento.TipoDocumento}_{DateTime.Now:yyyyMMdd_HHmmss}{extension}";
            var rutaCompleta = Path.Combine(empleadoFolder, nuevoNombre);
            
            // Guardar archivo
            await File.WriteAllBytesAsync(rutaCompleta, archivoBytes);
            
            // Actualizar datos del documento
            documento.ArchivoPath = rutaCompleta;
            documento.TamanoArchivo = archivoBytes.Length;
            documento.SubidoPorUsuarioId = usuarioId;
            documento.FechaCreacion = DateTime.Now;
            documento.Empleado = empleado;
            
            // Guardar en base de datos
            await _repository.AddAsync(documento);
            await _repository.SaveChangesAsync();
            
            return ServiceResult<DocumentoEmpleado>.Ok(documento, "Documento subido exitosamente");
        }
        catch (Exception ex)
        {
            return ServiceResult<DocumentoEmpleado>.Fail($"Error al subir documento: {ex.Message}");
        }
    }
    
    /// <summary>
    /// Actualiza la información de un documento (no el archivo)
    /// Requiere rol Administrador u Operador
    /// </summary>
    public async Task<ServiceResult<DocumentoEmpleado>> UpdateAsync(
        DocumentoEmpleado documento,
        RolUsuario rolUsuario)
    {
        try
        {
            // Validar permisos
            if (!PuedeGestionarDocumentos(rolUsuario))
            {
                return ServiceResult<DocumentoEmpleado>.Fail("No tiene permisos para editar documentos");
            }
            
            var existente = await _repository.GetByIdAsync(documento.Id);
            if (existente == null)
            {
                return ServiceResult<DocumentoEmpleado>.Fail("Documento no encontrado");
            }
            
            // Actualizar campos editables
            existente.Nombre = documento.Nombre;
            existente.Descripcion = documento.Descripcion;
            existente.TipoDocumento = documento.TipoDocumento;
            existente.FechaVencimiento = documento.FechaVencimiento;
            existente.FechaEmision = documento.FechaEmision;
            existente.FechaModificacion = DateTime.Now;
            
            await _repository.UpdateAsync(existente);
            await _repository.SaveChangesAsync();
            
            return ServiceResult<DocumentoEmpleado>.Ok(existente, "Documento actualizado exitosamente");
        }
        catch (Exception ex)
        {
            return ServiceResult<DocumentoEmpleado>.Fail($"Error al actualizar documento: {ex.Message}");
        }
    }
    
    /// <summary>
    /// Elimina un documento
    /// Requiere rol Administrador
    /// </summary>
    public async Task<ServiceResult<bool>> DeleteAsync(int id, RolUsuario rolUsuario)
    {
        try
        {
            // Validar permisos - solo Admin puede eliminar
            if (!PuedeEliminarDocumentos(rolUsuario))
            {
                return ServiceResult<bool>.Fail("Solo el administrador puede eliminar documentos");
            }
            
            var documento = await _repository.GetByIdAsync(id);
            if (documento == null)
            {
                return ServiceResult<bool>.Fail("Documento no encontrado");
            }
            
            // Eliminar archivo físico
            if (File.Exists(documento.ArchivoPath))
            {
                try
                {
                    File.Delete(documento.ArchivoPath);
                }
                catch
                {
                    // Log pero continuar con la eliminación del registro
                }
            }
            
            // Eliminar de la base de datos (soft delete)
            await _repository.DeleteAsync(id);
            await _repository.SaveChangesAsync();
            
            return ServiceResult<bool>.Ok(true, "Documento eliminado exitosamente");
        }
        catch (Exception ex)
        {
            return ServiceResult<bool>.Fail($"Error al eliminar documento: {ex.Message}");
        }
    }
    
    /// <summary>
    /// Obtiene los bytes del archivo de un documento
    /// </summary>
    public async Task<ServiceResult<byte[]>> DescargarArchivoAsync(int documentoId)
    {
        try
        {
            var documento = await _repository.GetByIdAsync(documentoId);
            if (documento == null)
            {
                return ServiceResult<byte[]>.Fail("Documento no encontrado");
            }
            
            if (!File.Exists(documento.ArchivoPath))
            {
                return ServiceResult<byte[]>.Fail("El archivo no existe en el sistema");
            }
            
            var bytes = await File.ReadAllBytesAsync(documento.ArchivoPath);
            return ServiceResult<byte[]>.Ok(bytes);
        }
        catch (Exception ex)
        {
            return ServiceResult<byte[]>.Fail($"Error al descargar archivo: {ex.Message}");
        }
    }
    
    /// <summary>
    /// Obtiene documentos próximos a vencer (exámenes médicos, etc.)
    /// </summary>
    public async Task<ServiceResult<IEnumerable<DocumentoEmpleado>>> GetDocumentosProximosAVencerAsync(int diasAnticipacion = 30)
    {
        try
        {
            var documentos = await _repository.GetDocumentosProximosAVencerAsync(diasAnticipacion);
            return ServiceResult<IEnumerable<DocumentoEmpleado>>.Ok(documentos);
        }
        catch (Exception ex)
        {
            return ServiceResult<IEnumerable<DocumentoEmpleado>>.Fail($"Error al obtener documentos: {ex.Message}");
        }
    }
    
    /// <summary>
    /// Obtiene el checklist de documentos requeridos y opcionales para un empleado
    /// </summary>
    public async Task<ServiceResult<IEnumerable<DocumentoChecklistItem>>> GetChecklistDocumentosAsync(int empleadoId)
    {
        try
        {
            var documentosEmpleado = await _repository.GetByEmpleadoIdAsync(empleadoId);
            var documentosLookup = documentosEmpleado.ToLookup(d => d.TipoDocumento);
            
            var checklist = new List<DocumentoChecklistItem>();
            
            // Agregar documentos requeridos
            foreach (var tipo in DocumentosRequeridos)
            {
                var docs = documentosLookup[tipo].ToList();
                var docMasReciente = docs.OrderByDescending(d => d.FechaCreacion).FirstOrDefault();
                
                checklist.Add(new DocumentoChecklistItem
                {
                    Tipo = tipo,
                    NombreTipo = GetNombreTipoDocumento(tipo),
                    EsRequerido = true,
                    TieneDocumento = docs.Any(),
                    Documento = docMasReciente,
                    EstaVencido = docMasReciente?.FechaVencimiento.HasValue == true && 
                                  docMasReciente.FechaVencimiento.Value < DateTime.Today
                });
            }
            
            // Agregar documentos opcionales
            foreach (var tipo in DocumentosOpcionales)
            {
                var docs = documentosLookup[tipo].ToList();
                var docMasReciente = docs.OrderByDescending(d => d.FechaCreacion).FirstOrDefault();
                
                checklist.Add(new DocumentoChecklistItem
                {
                    Tipo = tipo,
                    NombreTipo = GetNombreTipoDocumento(tipo),
                    EsRequerido = false,
                    TieneDocumento = docs.Any(),
                    Documento = docMasReciente,
                    EstaVencido = docMasReciente?.FechaVencimiento.HasValue == true && 
                                  docMasReciente.FechaVencimiento.Value < DateTime.Today
                });
            }
            
            return ServiceResult<IEnumerable<DocumentoChecklistItem>>.Ok(checklist);
        }
        catch (Exception ex)
        {
            return ServiceResult<IEnumerable<DocumentoChecklistItem>>.Fail($"Error al obtener checklist: {ex.Message}");
        }
    }
    
    /// <summary>
    /// Obtiene el nombre legible de un tipo de documento
    /// </summary>
    public static string GetNombreTipoDocumento(TipoDocumentoEmpleado tipo)
    {
        return tipo switch
        {
            TipoDocumentoEmpleado.Cedula => "Cédula de Ciudadanía",
            TipoDocumentoEmpleado.HojaVida => "Hoja de Vida",
            TipoDocumentoEmpleado.CertificadoEstudios => "Certificado de Estudios",
            TipoDocumentoEmpleado.CertificadoLaboral => "Certificado Laboral",
            TipoDocumentoEmpleado.ExamenMedicoIngreso => "Examen Médico de Ingreso",
            TipoDocumentoEmpleado.ExamenMedicoPeriodico => "Examen Médico Periódico",
            TipoDocumentoEmpleado.ExamenMedicoEgreso => "Examen Médico de Egreso",
            TipoDocumentoEmpleado.AfiliacionEPS => "Afiliación EPS (Salud)",
            TipoDocumentoEmpleado.AfiliacionAFP => "Afiliación AFP (Pensión)",
            TipoDocumentoEmpleado.AfiliacionARL => "Afiliación ARL (Riesgos)",
            TipoDocumentoEmpleado.AfiliacionCajaCompensacion => "Caja de Compensación",
            TipoDocumentoEmpleado.ReferenciasPersonales => "Referencias Personales",
            TipoDocumentoEmpleado.ReferenciasLaborales => "Referencias Laborales",
            TipoDocumentoEmpleado.Antecedentes => "Antecedentes",
            TipoDocumentoEmpleado.LicenciaConduccion => "Licencia de Conducción",
            TipoDocumentoEmpleado.LibretaMilitar => "Libreta Militar",
            TipoDocumentoEmpleado.RUT => "RUT",
            TipoDocumentoEmpleado.CertificadoBancario => "Certificado Bancario",
            TipoDocumentoEmpleado.ActaEntregaDotacion => "Acta Entrega Dotación",
            TipoDocumentoEmpleado.Capacitacion => "Capacitación",
            TipoDocumentoEmpleado.Otro => "Otro Documento",
            _ => tipo.ToString()
        };
    }
}
