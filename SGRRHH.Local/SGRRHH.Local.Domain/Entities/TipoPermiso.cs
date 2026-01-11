using SGRRHH.Local.Domain.Enums;

namespace SGRRHH.Local.Domain.Entities;

public class TipoPermiso : EntidadBase
{
    public string Nombre { get; set; } = string.Empty;
    
    public string? Descripcion { get; set; }
    
    public string Color { get; set; } = "#1E88E5";
    
    public bool RequiereAprobacion { get; set; } = true;
    
    public bool RequiereDocumento { get; set; } = false;
    
    public int DiasPorDefecto { get; set; } = 1;
    
    public int DiasMaximos { get; set; } = 0;
    
    public bool EsCompensable { get; set; } = false;
    
    public new bool Activo { get; set; } = true;
    
    // ===== NUEVOS CAMPOS PARA SEGUIMIENTO =====
    
    /// <summary>Tipo de resolución por defecto (Remunerado/Descontado/Compensado)</summary>
    public TipoResolucionPermiso TipoResolucionPorDefecto { get; set; } = TipoResolucionPermiso.Remunerado;
    
    /// <summary>Días límite para entregar documento después de aprobar (0 = sin límite)</summary>
    public int DiasLimiteDocumento { get; set; } = 7;
    
    /// <summary>Días límite para completar compensación de horas (0 = sin límite)</summary>
    public int DiasLimiteCompensacion { get; set; } = 30;
    
    /// <summary>Horas a compensar por cada día de permiso (ej: 8 horas por día)</summary>
    public int HorasCompensarPorDia { get; set; } = 8;
    
    /// <summary>Indica si este tipo de permiso genera descuento automático</summary>
    public bool GeneraDescuento { get; set; } = false;
    
    /// <summary>Porcentaje del salario diario a descontar (si aplica)</summary>
    public decimal? PorcentajeDescuento { get; set; }
}


