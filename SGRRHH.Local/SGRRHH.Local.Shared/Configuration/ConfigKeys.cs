namespace SGRRHH.Local.Shared.Configuration;

public static class ConfigKeys
{
    // Empresa
    public const string EmpresaNombre = "empresa.nombre";
    public const string EmpresaNit = "empresa.nit";
    public const string EmpresaDireccion = "empresa.direccion";
    public const string EmpresaTelefono = "empresa.telefono";
    public const string EmpresaEmail = "empresa.email";
    public const string EmpresaRepresentante = "empresa.representante";
    public const string EmpresaLogo = "empresa.logo";
    
    // Sistema
    public const string SistemaVacacionesDiasAnuales = "sistema.vacaciones.dias_anuales"; // Default: 15
    public const string SistemaBackupAutomatico = "sistema.backup.automatico"; // true/false
    public const string SistemaBackupHora = "sistema.backup.hora"; // Default: "23:00"
    public const string SistemaBackupRetenerDias = "sistema.backup.retener_dias"; // Default: 30
    public const string SistemaAuditoriaActivada = "sistema.auditoria.activada"; // Default: true
    
    // Notificaciones
    public const string NotificarContratosVencer = "notificar.contratos.dias_antes"; // Default: 30
    public const string NotificarCumpleaños = "notificar.cumpleaños.dias_antes"; // Default: 7
    
    // Valores por defecto
    public static readonly Dictionary<string, string> DefaultValues = new()
    {
        { EmpresaNombre, "Mi Empresa" },
        { EmpresaNit, "" },
        { EmpresaDireccion, "" },
        { EmpresaTelefono, "" },
        { EmpresaEmail, "" },
        { EmpresaRepresentante, "" },
        { EmpresaLogo, "" },
        { SistemaVacacionesDiasAnuales, "15" },
        { SistemaBackupAutomatico, "false" },
        { SistemaBackupHora, "23:00" },
        { SistemaBackupRetenerDias, "30" },
        { SistemaAuditoriaActivada, "true" },
        { NotificarContratosVencer, "30" },
        { NotificarCumpleaños, "7" }
    };
}
