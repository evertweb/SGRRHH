namespace SGRRHH.Local.Domain.DTOs;

public class DailySummaryDto
{
    public DateTime Date { get; set; }

    public int TotalRecords { get; set; }

    public int Completed { get; set; }

    public int Pending { get; set; }

    public decimal TotalHours { get; set; }
}
