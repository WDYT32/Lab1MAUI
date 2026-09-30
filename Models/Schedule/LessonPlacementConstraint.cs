using System;
using System.Linq;

namespace Lab1MAUI.Models.Schedule;

public class LessonPlacementConstraint : IConstraint
{
    public int Priority { get; set; } = 100;
    public Guid OwnerId { get; }
    
    private readonly Lesson _lesson;

    public LessonPlacementConstraint(Lesson lesson)
    {
        OwnerId = lesson.Id;
        _lesson = lesson;
    }

    public void Apply(Schedule schedule)
    {
        var teacher = schedule.Teachers.FirstOrDefault(t => t.Id == _lesson.TeacherId);
        var group = schedule.Groups.FirstOrDefault(g => g.Id == _lesson.StudentGroupId);

        if (teacher == null || group == null) return;

        // Шукаємо першу вільну комірку і застосовуємо
        for (int i = 0; i < schedule.Config.TotalCells; i++)
        {
            if (teacher.Grid.IsFree(i) && group.Grid.IsFree(i))
            {
                teacher.Grid.Place(_lesson.Id, i);
                group.Grid.Place(_lesson.Id, i);
                return; // Застосовано успішно
            }
        }
    }
}
