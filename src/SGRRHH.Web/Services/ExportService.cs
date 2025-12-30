using ClosedXML.Excel;
using SGRRHH.Core.Entities;
using SGRRHH.Core.Enums;
using System.IO;

namespace SGRRHH.Web.Services;

/// <summary>
/// Servicio para exportar datos a Excel y PDF
/// </summary>
public class ExportService
{
    #region Excel Exports
    
    public byte[] GenerateEmpleadosExcel(IEnumerable<Empleado> data)
    {
        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Empleados");
        
        // Headers
        worksheet.Cell(1, 1).Value = "Cédula";
        worksheet.Cell(1, 2).Value = "Nombre Completo";
        worksheet.Cell(1, 3).Value = "Departamento";
        worksheet.Cell(1, 4).Value = "Cargo";
        worksheet.Cell(1, 5).Value = "Fecha Ingreso";
        worksheet.Cell(1, 6).Value = "Antigüedad (años)";
        worksheet.Cell(1, 7).Value = "Estado";
        
        StyleHeaderRow(worksheet, 1, 7);
        
        int row = 2;
        foreach (var emp in data)
        {
            worksheet.Cell(row, 1).Value = emp.Cedula;
            worksheet.Cell(row, 2).Value = emp.NombreCompleto;
            worksheet.Cell(row, 3).Value = emp.Departamento?.Nombre ?? "-";
            worksheet.Cell(row, 4).Value = emp.Cargo?.Nombre ?? "-";
            worksheet.Cell(row, 5).Value = emp.FechaIngreso.ToString("dd/MM/yyyy");
            worksheet.Cell(row, 6).Value = emp.Antiguedad;
            worksheet.Cell(row, 7).Value = emp.Estado.ToString();
            row++;
        }
        
        worksheet.Columns().AdjustToContents();
        
        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }
    
    public byte[] GenerateVacacionesExcel(IEnumerable<Vacacion> data)
    {
        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Vacaciones");
        
        // Headers
        worksheet.Cell(1, 1).Value = "Empleado";
        worksheet.Cell(1, 2).Value = "Período";
        worksheet.Cell(1, 3).Value = "Fecha Inicio";
        worksheet.Cell(1, 4).Value = "Fecha Fin";
        worksheet.Cell(1, 5).Value = "Días";
        worksheet.Cell(1, 6).Value = "Estado";
        
        StyleHeaderRow(worksheet, 1, 6);
        
        int row = 2;
        foreach (var vac in data)
        {
            worksheet.Cell(row, 1).Value = vac.Empleado?.NombreCompleto ?? "-";
            worksheet.Cell(row, 2).Value = vac.PeriodoCorrespondiente;
            worksheet.Cell(row, 3).Value = vac.FechaInicio.ToString("dd/MM/yyyy");
            worksheet.Cell(row, 4).Value = vac.FechaFin.ToString("dd/MM/yyyy");
            worksheet.Cell(row, 5).Value = vac.DiasTomados;
            worksheet.Cell(row, 6).Value = vac.Estado.ToString();
            row++;
        }
        
        worksheet.Columns().AdjustToContents();
        
        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }
    
    public byte[] GenerateContratosExcel(IEnumerable<Contrato> data)
    {
        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Contratos");
        
        // Headers
        worksheet.Cell(1, 1).Value = "Empleado";
        worksheet.Cell(1, 2).Value = "Tipo Contrato";
        worksheet.Cell(1, 3).Value = "Fecha Inicio";
        worksheet.Cell(1, 4).Value = "Fecha Fin";
        worksheet.Cell(1, 5).Value = "Días Restantes";
        worksheet.Cell(1, 6).Value = "Estado";
        
        StyleHeaderRow(worksheet, 1, 6);
        
        int row = 2;
        foreach (var cont in data)
        {
            var diasRestantes = cont.FechaFin.HasValue ? (cont.FechaFin.Value - DateTime.Today).Days : 0;
            worksheet.Cell(row, 1).Value = cont.Empleado?.NombreCompleto ?? "-";
            worksheet.Cell(row, 2).Value = GetTipoContratoLabel(cont.TipoContrato);
            worksheet.Cell(row, 3).Value = cont.FechaInicio.ToString("dd/MM/yyyy");
            worksheet.Cell(row, 4).Value = cont.FechaFin?.ToString("dd/MM/yyyy") ?? "Indefinido";
            worksheet.Cell(row, 5).Value = diasRestantes;
            worksheet.Cell(row, 6).Value = cont.Activo ? "Vigente" : "Finalizado";
            row++;
        }
        
        worksheet.Columns().AdjustToContents();
        
        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }
    
    public byte[] GeneratePermisosExcel(IEnumerable<Permiso> data)
    {
        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Permisos");
        
        // Headers
        worksheet.Cell(1, 1).Value = "Empleado";
        worksheet.Cell(1, 2).Value = "Tipo";
        worksheet.Cell(1, 3).Value = "Fecha Inicio";
        worksheet.Cell(1, 4).Value = "Fecha Fin";
        worksheet.Cell(1, 5).Value = "Días";
        worksheet.Cell(1, 6).Value = "Estado";
        worksheet.Cell(1, 7).Value = "Motivo";
        
        StyleHeaderRow(worksheet, 1, 7);
        
        int row = 2;
        foreach (var perm in data)
        {
            worksheet.Cell(row, 1).Value = perm.Empleado?.NombreCompleto ?? "-";
            worksheet.Cell(row, 2).Value = perm.TipoPermiso?.Nombre ?? "-";
            worksheet.Cell(row, 3).Value = perm.FechaInicio.ToString("dd/MM/yyyy");
            worksheet.Cell(row, 4).Value = perm.FechaFin.ToString("dd/MM/yyyy");
            worksheet.Cell(row, 5).Value = perm.TotalDias;
            worksheet.Cell(row, 6).Value = perm.Estado.ToString();
            worksheet.Cell(row, 7).Value = perm.Motivo ?? "-";
            row++;
        }
        
        worksheet.Columns().AdjustToContents();
        
        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }
    
    #endregion
    
    #region CSV Exports (Alternative to PDF for Blazor WASM)
    
    public byte[] GenerateEmpleadosCsv(IEnumerable<Empleado> data)
    {
        var lines = new List<string>
        {
            "Cédula,Nombre Completo,Departamento,Cargo,Fecha Ingreso,Antigüedad,Estado"
        };
        
        foreach (var emp in data)
        {
            lines.Add($"\"{emp.Cedula}\",\"{emp.NombreCompleto}\",\"{emp.Departamento?.Nombre ?? "-"}\",\"{emp.Cargo?.Nombre ?? "-"}\",\"{emp.FechaIngreso:dd/MM/yyyy}\",{emp.Antiguedad},\"{emp.Estado}\"");
        }
        
        return System.Text.Encoding.UTF8.GetBytes(string.Join("\r\n", lines));
    }
    
    public byte[] GenerateVacacionesCsv(IEnumerable<Vacacion> data)
    {
        var lines = new List<string>
        {
            "Empleado,Período,Fecha Inicio,Fecha Fin,Días,Estado"
        };
        
        foreach (var vac in data)
        {
            lines.Add($"\"{vac.Empleado?.NombreCompleto ?? "-"}\",\"{vac.PeriodoCorrespondiente}\",\"{vac.FechaInicio:dd/MM/yyyy}\",\"{vac.FechaFin:dd/MM/yyyy}\",{vac.DiasTomados},\"{vac.Estado}\"");
        }
        
        return System.Text.Encoding.UTF8.GetBytes(string.Join("\r\n", lines));
    }
    
    public byte[] GenerateContratosCsv(IEnumerable<Contrato> data)
    {
        var lines = new List<string>
        {
            "Empleado,Tipo Contrato,Fecha Inicio,Fecha Fin,Días Restantes,Estado"
        };
        
        foreach (var cont in data)
        {
            var diasRestantes = cont.FechaFin.HasValue ? (cont.FechaFin.Value - DateTime.Today).Days : 0;
            lines.Add($"\"{cont.Empleado?.NombreCompleto ?? "-"}\",\"{GetTipoContratoLabel(cont.TipoContrato)}\",\"{cont.FechaInicio:dd/MM/yyyy}\",\"{cont.FechaFin?.ToString("dd/MM/yyyy") ?? "Indefinido"}\",{diasRestantes},\"{(cont.Activo ? "Vigente" : "Finalizado")}\"");
        }
        
        return System.Text.Encoding.UTF8.GetBytes(string.Join("\r\n", lines));
    }
    
    public byte[] GeneratePermisosCsv(IEnumerable<Permiso> data)
    {
        var lines = new List<string>
        {
            "Empleado,Tipo,Fecha Inicio,Fecha Fin,Días,Estado,Motivo"
        };
        
        foreach (var perm in data)
        {
            lines.Add($"\"{perm.Empleado?.NombreCompleto ?? "-"}\",\"{perm.TipoPermiso?.Nombre ?? "-"}\",\"{perm.FechaInicio:dd/MM/yyyy}\",\"{perm.FechaFin:dd/MM/yyyy}\",{perm.TotalDias},\"{perm.Estado}\",\"{perm.Motivo ?? "-"}\"");
        }
        
        return System.Text.Encoding.UTF8.GetBytes(string.Join("\r\n", lines));
    }
    
    #endregion
    
    #region Private Helpers
    
    private static void StyleHeaderRow(IXLWorksheet worksheet, int row, int columnCount)
    {
        var headerRange = worksheet.Range(row, 1, row, columnCount);
        headerRange.Style.Font.Bold = true;
        headerRange.Style.Fill.BackgroundColor = XLColor.FromHtml("#000080");
        headerRange.Style.Font.FontColor = XLColor.White;
        headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
    }
    
    private static string GetTipoContratoLabel(TipoContrato tipo)
    {
        return tipo switch
        {
            TipoContrato.Indefinido => "Indefinido",
            TipoContrato.Fijo => "Término Fijo",
            TipoContrato.ObraLabor => "Obra/Labor",
            TipoContrato.PrestacionServicios => "Prestación Servicios",
            TipoContrato.Aprendizaje => "Aprendizaje",
            _ => tipo.ToString()
        };
    }
    
    #endregion
}
