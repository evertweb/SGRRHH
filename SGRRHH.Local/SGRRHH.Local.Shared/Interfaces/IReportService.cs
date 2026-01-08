using SGRRHH.Local.Domain.Entities;
using SGRRHH.Local.Domain.Enums;

namespace SGRRHH.Local.Shared.Interfaces;

public interface IReportService
{
    // Documentos individuales
    Task<Result<byte[]>> GenerarActaPermisoAsync(int permisoId);
    Task<Result<byte[]>> GenerarCertificadoLaboralAsync(int empleadoId, string? tipoClasificado = null);
    Task<Result<byte[]>> GenerarCertificadoLaboralPdf(int empleadoId);
    
    // Métodos sincrónicos (legacy) - para compatibilidad
    byte[] GenerarListadoEmpleadosPdf(List<Empleado> empleados);
    byte[] GenerarListadoEmpleadosExcel(List<Empleado> empleados);
    byte[] GenerarReportePermisosPdf(List<Permiso> permisos, DateTime fechaInicio, DateTime fechaFin);
    byte[] GenerarReportePermisosExcel(List<Permiso> permisos);
    byte[] GenerarReporteVacacionesPdf(List<Vacacion> vacaciones, int periodo);
    byte[] GenerarReporteVacacionesExcel(List<Vacacion> vacaciones);
    byte[] GenerarCertificadoLaboralPdf(Empleado empleado, string proposito, bool incluirSalario);
    
    // Listados con options
    Task<Result<byte[]>> GenerarListadoEmpleadosAsync(ListadoEmpleadosOptions? options = null);
    Task<Result<byte[]>> GenerarListadoEmpleadosPdf(ListadoEmpleadosOptions? options = null);
    Task<Result<byte[]>> GenerarListadoEmpleadosExcel(ListadoEmpleadosOptions? options = null);
    
    Task<Result<byte[]>> GenerarReportePermisosAsync(ReportePermisosOptions options);
    Task<Result<byte[]>> GenerarReportePermisosPdf(ReportePermisosOptions options);
    Task<Result<byte[]>> GenerarReportePermisosExcel(ReportePermisosOptions options);
    
    Task<Result<byte[]>> GenerarReporteVacacionesAsync(ReporteVacacionesOptions options);
    Task<Result<byte[]>> GenerarReporteVacacionesPdf(ReporteVacacionesOptions options);
    Task<Result<byte[]>> GenerarReporteVacacionesExcel(ReporteVacacionesOptions options);
    
    Task<Result<byte[]>> GenerarReporteAsistenciaAsync(ReporteAsistenciaOptions options);
    
    // Utilidades
    byte[] GenerarPdfDesdeHtml(string html);
}

public class ListadoEmpleadosOptions
{
    public int? DepartamentoId { get; set; }
    public EstadoEmpleado? Estado { get; set; }
    public bool IncluirFotos { get; set; } = false;
}

public class ReportePermisosOptions
{
    public DateTime FechaInicio { get; set; }
    public DateTime FechaFin { get; set; }
    public int? EmpleadoId { get; set; }
    public int? TipoPermisoId { get; set; }
    public EstadoPermiso? Estado { get; set; }
}

public class ReporteVacacionesOptions
{
    public int Año { get; set; }
    public int? EmpleadoId { get; set; }
    public int? DepartamentoId { get; set; }
}

public class ReporteAsistenciaOptions
{
    public DateTime FechaInicio { get; set; }
    public DateTime FechaFin { get; set; }
    public int? EmpleadoId { get; set; }
    public int? DepartamentoId { get; set; }
}
