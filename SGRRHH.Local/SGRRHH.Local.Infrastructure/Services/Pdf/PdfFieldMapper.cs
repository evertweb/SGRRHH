using System.Globalization;
using iText.Forms;
using iText.Forms.Fields;
using Microsoft.Extensions.Logging;
using SGRRHH.Local.Domain.DTOs;
using SGRRHH.Local.Domain.Entities;

namespace SGRRHH.Local.Infrastructure.Services.Pdf;

/// <summary>
/// Mapea campos AcroForm del PDF a entidades y viceversa.
/// </summary>
public class PdfFieldMapper
{
    private readonly ILogger<PdfFieldMapper> _logger;
    
    // Formato de fecha usado en los PDFs
    private const string FORMATO_FECHA = "dd/MM/yyyy";
    
    public PdfFieldMapper(ILogger<PdfFieldMapper> logger)
    {
        _logger = logger;
    }
    
    /// <summary>
    /// Extrae los datos del formulario AcroForm a un DTO.
    /// </summary>
    public DatosHojaVida ExtraerDatos(PdfAcroForm form)
    {
        var datos = new DatosHojaVida();
        var campos = form.GetAllFormFields();
        
        _logger.LogDebug("Extrayendo datos de {Count} campos del formulario", campos.Count);
        
        // Datos personales
        datos.Nombres = ObtenerValorCampo(campos, "Nombres");
        datos.Apellidos = ObtenerValorCampo(campos, "Apellidos");
        datos.Cedula = ObtenerValorCampo(campos, "Cedula");
        datos.FechaNacimiento = ParsearFecha(ObtenerValorCampo(campos, "FechaNacimiento"));
        datos.Genero = ObtenerValorCampo(campos, "Genero");
        datos.EstadoCivil = ObtenerValorCampo(campos, "EstadoCivil");
        
        // Ubicación
        datos.Direccion = ObtenerValorCampo(campos, "Direccion");
        datos.Ciudad = ObtenerValorCampo(campos, "Ciudad");
        datos.Departamento = ObtenerValorCampo(campos, "Departamento");
        
        // Contacto
        datos.Telefono = ObtenerValorCampo(campos, "Telefono");
        datos.Email = ObtenerValorCampo(campos, "Email");
        
        // Tallas EPP
        datos.TallaCasco = ObtenerValorCampo(campos, "TallaCasco");
        datos.TallaBotas = ObtenerValorCampo(campos, "TallaBotas");
        
        // Formaciones (3 bloques)
        for (int i = 1; i <= 3; i++)
        {
            var formacion = ExtraerFormacion(campos, i);
            if (formacion != null)
            {
                datos.Formaciones.Add(formacion);
            }
        }
        
        // Experiencias (3 bloques)
        for (int i = 1; i <= 3; i++)
        {
            var experiencia = ExtraerExperiencia(campos, i);
            if (experiencia != null)
            {
                datos.Experiencias.Add(experiencia);
            }
        }
        
        // Referencias (4 bloques: 2 personales + 2 laborales)
        for (int i = 1; i <= 4; i++)
        {
            var referencia = ExtraerReferencia(campos, i);
            if (referencia != null)
            {
                datos.Referencias.Add(referencia);
            }
        }
        
        // Metadatos
        datos.FechaFirma = ParsearFecha(ObtenerValorCampo(campos, "FechaFirma"));
        
        _logger.LogInformation("Extraídos: {Formaciones} formaciones, {Experiencias} experiencias, {Referencias} referencias",
            datos.Formaciones.Count, datos.Experiencias.Count, datos.Referencias.Count);
        
        return datos;
    }
    
    /// <summary>
    /// Rellena el formulario AcroForm con datos del aspirante.
    /// </summary>
    public void RellenarFormulario(PdfAcroForm form, Aspirante aspirante)
    {
        var campos = form.GetAllFormFields();
        
        // Datos personales
        EstablecerValorCampo(campos, "Nombres", aspirante.Nombres);
        EstablecerValorCampo(campos, "Apellidos", aspirante.Apellidos);
        EstablecerValorCampo(campos, "Cedula", aspirante.Cedula);
        EstablecerValorCampo(campos, "FechaNacimiento", aspirante.FechaNacimiento.ToString(FORMATO_FECHA));
        EstablecerValorCampo(campos, "Genero", aspirante.Genero.ToString());
        EstablecerValorCampo(campos, "EstadoCivil", aspirante.EstadoCivil.ToString());
        
        // Ubicación
        EstablecerValorCampo(campos, "Direccion", aspirante.Direccion);
        EstablecerValorCampo(campos, "Ciudad", aspirante.Ciudad);
        EstablecerValorCampo(campos, "Departamento", aspirante.Departamento);
        
        // Contacto
        EstablecerValorCampo(campos, "Telefono", aspirante.Telefono);
        EstablecerValorCampo(campos, "Email", aspirante.Email ?? "");
        
        // Tallas EPP
        EstablecerValorCampo(campos, "TallaCasco", aspirante.TallasCasco ?? "");
        EstablecerValorCampo(campos, "TallaBotas", aspirante.TallasBotas ?? "");
        
        // Formaciones
        if (aspirante.Formaciones != null)
        {
            int i = 1;
            foreach (var formacion in aspirante.Formaciones.Take(3))
            {
                RellenarFormacion(campos, i, formacion);
                i++;
            }
        }
        
        // Experiencias
        if (aspirante.Experiencias != null)
        {
            int i = 1;
            foreach (var experiencia in aspirante.Experiencias.Take(3))
            {
                RellenarExperiencia(campos, i, experiencia);
                i++;
            }
        }
        
        // Referencias
        if (aspirante.Referencias != null)
        {
            int i = 1;
            foreach (var referencia in aspirante.Referencias.Take(4))
            {
                RellenarReferencia(campos, i, referencia);
                i++;
            }
        }
        
        _logger.LogDebug("Formulario rellenado con datos del aspirante {Cedula}", aspirante.Cedula);
    }
    
    /// <summary>
    /// Rellena el formulario AcroForm con datos del empleado.
    /// </summary>
    public void RellenarFormulario(PdfAcroForm form, Empleado empleado)
    {
        var campos = form.GetAllFormFields();
        
        // Datos personales
        EstablecerValorCampo(campos, "Nombres", empleado.Nombres);
        EstablecerValorCampo(campos, "Apellidos", empleado.Apellidos);
        EstablecerValorCampo(campos, "Cedula", empleado.Cedula);
        
        if (empleado.FechaNacimiento.HasValue)
        {
            EstablecerValorCampo(campos, "FechaNacimiento", empleado.FechaNacimiento.Value.ToString(FORMATO_FECHA));
        }
        
        EstablecerValorCampo(campos, "Genero", empleado.Genero?.ToString() ?? "");
        EstablecerValorCampo(campos, "EstadoCivil", empleado.EstadoCivil?.ToString() ?? "");
        
        // Ubicación
        EstablecerValorCampo(campos, "Direccion", empleado.Direccion ?? "");
        EstablecerValorCampo(campos, "Ciudad", empleado.Municipio ?? "");
        EstablecerValorCampo(campos, "Departamento", ""); // El empleado no tiene departamento de ubicación directo
        
        // Contacto
        EstablecerValorCampo(campos, "Telefono", empleado.TelefonoCelular ?? empleado.Telefono ?? "");
        EstablecerValorCampo(campos, "Email", empleado.Email ?? "");
        
        _logger.LogDebug("Formulario rellenado con datos del empleado {Cedula}", empleado.Cedula);
    }
    
    // ========== MÉTODOS PRIVADOS ==========
    
    private string? ObtenerValorCampo(IDictionary<string, PdfFormField> campos, string nombreCampo)
    {
        if (campos.TryGetValue(nombreCampo, out var campo))
        {
            var valor = campo.GetValueAsString();
            return string.IsNullOrWhiteSpace(valor) ? null : valor.Trim();
        }
        return null;
    }
    
    private void EstablecerValorCampo(IDictionary<string, PdfFormField> campos, string nombreCampo, string valor)
    {
        if (campos.TryGetValue(nombreCampo, out var campo) && !string.IsNullOrEmpty(valor))
        {
            try
            {
                campo.SetValue(valor);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error al establecer valor en campo {Campo}", nombreCampo);
            }
        }
    }
    
    private DateTime? ParsearFecha(string? valor)
    {
        if (string.IsNullOrWhiteSpace(valor))
            return null;
        
        // Intentar varios formatos
        string[] formatos = { FORMATO_FECHA, "yyyy-MM-dd", "d/M/yyyy", "dd-MM-yyyy" };
        
        foreach (var formato in formatos)
        {
            if (DateTime.TryParseExact(valor, formato, CultureInfo.InvariantCulture, DateTimeStyles.None, out var fecha))
            {
                return fecha;
            }
        }
        
        // Intentar parseo genérico
        if (DateTime.TryParse(valor, out var fechaGenerica))
        {
            return fechaGenerica;
        }
        
        _logger.LogWarning("No se pudo parsear la fecha: {Valor}", valor);
        return null;
    }
    
    private DatosFormacion? ExtraerFormacion(IDictionary<string, PdfFormField> campos, int indice)
    {
        var nivel = ObtenerValorCampo(campos, $"Form{indice}_Nivel");
        var titulo = ObtenerValorCampo(campos, $"Form{indice}_Titulo");
        
        // Si no hay datos principales, retornar null
        if (string.IsNullOrWhiteSpace(nivel) && string.IsNullOrWhiteSpace(titulo))
            return null;
        
        return new DatosFormacion
        {
            Nivel = nivel,
            Titulo = titulo,
            Institucion = ObtenerValorCampo(campos, $"Form{indice}_Institucion"),
            FechaInicio = ParsearFecha(ObtenerValorCampo(campos, $"Form{indice}_FechaInicio")),
            FechaFin = ParsearFecha(ObtenerValorCampo(campos, $"Form{indice}_FechaFin")),
            EnCurso = ObtenerValorCheckbox(campos, $"Form{indice}_EnCurso")
        };
    }
    
    private DatosExperiencia? ExtraerExperiencia(IDictionary<string, PdfFormField> campos, int indice)
    {
        var empresa = ObtenerValorCampo(campos, $"Exp{indice}_Empresa");
        var cargo = ObtenerValorCampo(campos, $"Exp{indice}_Cargo");
        
        // Si no hay datos principales, retornar null
        if (string.IsNullOrWhiteSpace(empresa) && string.IsNullOrWhiteSpace(cargo))
            return null;
        
        return new DatosExperiencia
        {
            Empresa = empresa,
            Cargo = cargo,
            FechaInicio = ParsearFecha(ObtenerValorCampo(campos, $"Exp{indice}_FechaInicio")),
            FechaFin = ParsearFecha(ObtenerValorCampo(campos, $"Exp{indice}_FechaFin")),
            TrabajoActual = ObtenerValorCheckbox(campos, $"Exp{indice}_TrabajoActual"),
            Funciones = ObtenerValorCampo(campos, $"Exp{indice}_Funciones"),
            MotivoRetiro = ObtenerValorCampo(campos, $"Exp{indice}_MotivoRetiro")
        };
    }
    
    private DatosReferencia? ExtraerReferencia(IDictionary<string, PdfFormField> campos, int indice)
    {
        var nombre = ObtenerValorCampo(campos, $"Ref{indice}_Nombre");
        
        // Si no hay nombre, retornar null
        if (string.IsNullOrWhiteSpace(nombre))
            return null;
        
        return new DatosReferencia
        {
            Tipo = ObtenerValorCampo(campos, $"Ref{indice}_Tipo"),
            NombreCompleto = nombre,
            Telefono = ObtenerValorCampo(campos, $"Ref{indice}_Telefono"),
            Relacion = ObtenerValorCampo(campos, $"Ref{indice}_Relacion"),
            Empresa = ObtenerValorCampo(campos, $"Ref{indice}_Empresa"),
            Cargo = ObtenerValorCampo(campos, $"Ref{indice}_Cargo")
        };
    }
    
    private bool ObtenerValorCheckbox(IDictionary<string, PdfFormField> campos, string nombreCampo)
    {
        if (campos.TryGetValue(nombreCampo, out var campo))
        {
            var valor = campo.GetValueAsString();
            return valor?.ToLowerInvariant() is "yes" or "on" or "true" or "1" or "sí" or "si";
        }
        return false;
    }
    
    private void RellenarFormacion(IDictionary<string, PdfFormField> campos, int indice, FormacionAspirante formacion)
    {
        EstablecerValorCampo(campos, $"Form{indice}_Nivel", formacion.Nivel);
        EstablecerValorCampo(campos, $"Form{indice}_Titulo", formacion.Titulo);
        EstablecerValorCampo(campos, $"Form{indice}_Institucion", formacion.Institucion);
        EstablecerValorCampo(campos, $"Form{indice}_FechaInicio", formacion.FechaInicio.ToString(FORMATO_FECHA));
        
        if (formacion.FechaFin.HasValue)
        {
            EstablecerValorCampo(campos, $"Form{indice}_FechaFin", formacion.FechaFin.Value.ToString(FORMATO_FECHA));
        }
        
        EstablecerValorCheckbox(campos, $"Form{indice}_EnCurso", formacion.EnCurso);
    }
    
    private void RellenarExperiencia(IDictionary<string, PdfFormField> campos, int indice, ExperienciaAspirante experiencia)
    {
        EstablecerValorCampo(campos, $"Exp{indice}_Empresa", experiencia.Empresa);
        EstablecerValorCampo(campos, $"Exp{indice}_Cargo", experiencia.Cargo);
        EstablecerValorCampo(campos, $"Exp{indice}_FechaInicio", experiencia.FechaInicio.ToString(FORMATO_FECHA));
        
        if (experiencia.FechaFin.HasValue)
        {
            EstablecerValorCampo(campos, $"Exp{indice}_FechaFin", experiencia.FechaFin.Value.ToString(FORMATO_FECHA));
        }
        
        EstablecerValorCheckbox(campos, $"Exp{indice}_TrabajoActual", experiencia.TrabajoActual);
        EstablecerValorCampo(campos, $"Exp{indice}_Funciones", experiencia.Funciones ?? "");
        EstablecerValorCampo(campos, $"Exp{indice}_MotivoRetiro", experiencia.MotivoRetiro ?? "");
    }
    
    private void RellenarReferencia(IDictionary<string, PdfFormField> campos, int indice, ReferenciaAspirante referencia)
    {
        EstablecerValorCampo(campos, $"Ref{indice}_Tipo", referencia.Tipo);
        EstablecerValorCampo(campos, $"Ref{indice}_Nombre", referencia.NombreCompleto);
        EstablecerValorCampo(campos, $"Ref{indice}_Telefono", referencia.Telefono);
        EstablecerValorCampo(campos, $"Ref{indice}_Relacion", referencia.Relacion);
        EstablecerValorCampo(campos, $"Ref{indice}_Empresa", referencia.Empresa ?? "");
        EstablecerValorCampo(campos, $"Ref{indice}_Cargo", referencia.Cargo ?? "");
    }
    
    private void EstablecerValorCheckbox(IDictionary<string, PdfFormField> campos, string nombreCampo, bool valor)
    {
        if (campos.TryGetValue(nombreCampo, out var campo))
        {
            try
            {
                campo.SetValue(valor ? "Yes" : "Off");
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error al establecer checkbox {Campo}", nombreCampo);
            }
        }
    }
}
