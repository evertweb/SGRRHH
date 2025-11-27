namespace SGRRHH.Core.Models;

/// <summary>
/// Información básica de la empresa usada para los documentos oficiales.
/// </summary>
public class CompanyInfo
{
    public string Nombre { get; set; } = "Empresa SGRRHH";
    public string Nit { get; set; } = "800.000.000-0";
    public string Direccion { get; set; } = "Calle 00 #00-00";
    public string Ciudad { get; set; } = "Bogotá D.C.";
    public string Telefono { get; set; } = "+57 (1) 000 0000";
    public string Correo { get; set; } = "contacto@empresa.com";
    public string RepresentanteNombre { get; set; } = "María Pérez";
    public string RepresentanteCargo { get; set; } = "Gerente General";
    public string? LogoPath { get; set; }
        = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data", "config", "logo.png");

    /// <summary>
    /// Mezcla la información actual con otra instancia, usando los valores definidos en <paramref name="overrides"/>.
    /// </summary>
    public CompanyInfo Merge(CompanyInfo overrides)
    {
        Nombre = string.IsNullOrWhiteSpace(overrides.Nombre) ? Nombre : overrides.Nombre;
        Nit = string.IsNullOrWhiteSpace(overrides.Nit) ? Nit : overrides.Nit;
        Direccion = string.IsNullOrWhiteSpace(overrides.Direccion) ? Direccion : overrides.Direccion;
        Ciudad = string.IsNullOrWhiteSpace(overrides.Ciudad) ? Ciudad : overrides.Ciudad;
        Telefono = string.IsNullOrWhiteSpace(overrides.Telefono) ? Telefono : overrides.Telefono;
        Correo = string.IsNullOrWhiteSpace(overrides.Correo) ? Correo : overrides.Correo;
        RepresentanteNombre = string.IsNullOrWhiteSpace(overrides.RepresentanteNombre)
            ? RepresentanteNombre
            : overrides.RepresentanteNombre;
        RepresentanteCargo = string.IsNullOrWhiteSpace(overrides.RepresentanteCargo)
            ? RepresentanteCargo
            : overrides.RepresentanteCargo;
        LogoPath = string.IsNullOrWhiteSpace(overrides.LogoPath) ? LogoPath : overrides.LogoPath;
        return this;
    }
}
