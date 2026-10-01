using Lab1MAUI.Models.Schedule.Participants;
namespace Lab1MAUI.Models.Schedule
{
    public class ScheduleConfig
    {
        public int Days { get; set; } = 5;
        public int CellsPerDay { get; set; } = 4;
        public int MinLessonsPerDay { get; set; } = 1;
        public int LessonDurationMinutes { get; set; } = 90;
        
        public int TotalCells => Days * CellsPerDay;
    }
}
