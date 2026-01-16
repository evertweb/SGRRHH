namespace SGRRHH.Local.Domain.DTOs;

public class ResumenDiarioDTO
{
    public DateTime Fecha { get; set; }

    public int TotalRegistros { get; set; }

    public int Completados { get; set; }

    public int Pendientes { get; set; }

    public decimal TotalHoras { get; set; }
}
