namespace Lab1MAUI.Models.Schedule;

public class ScheduleConfig
{
    public int Days { get; set; } = 5;
    public int CellsPerDay { get; set; } = 8;
    public int TotalCells => Days * CellsPerDay;
}
