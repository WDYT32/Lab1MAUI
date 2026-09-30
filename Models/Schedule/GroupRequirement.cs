using System;

namespace Lab1MAUI.Models.Schedule;

public class GroupRequirement
{
    public Guid DisciplineId { get; set; }
    public LessonType Type { get; set; }
    public int RequiredHours { get; set; }
}
