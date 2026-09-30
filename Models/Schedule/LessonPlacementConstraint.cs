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

        int cellsPerDay = schedule.Config.CellsPerDay;

        // Поступове послаблення обмежень: спочатку шукаємо день, де цього предмета ще немає (0),
        // потім де він є 1 раз, потім 2 рази і т.д.
        for (int allowedPerDay = 1; allowedPerDay <= cellsPerDay; allowedPerDay++)
        {
            for (int i = 0; i < schedule.Config.TotalCells; i++)
            {
                if (teacher.Grid.IsFree(i) && group.Grid.IsFree(i))
                {
                    int day = i / cellsPerDay;
                    int subjectCountInDay = 0;

                    for (int c = day * cellsPerDay; c < (day + 1) * cellsPerDay; c++)
                    {
                        var cellLessonId = group.Grid.Cells[c];
                        if (cellLessonId.HasValue)
                        {
                            var cellLesson = schedule.Lessons.FirstOrDefault(l => l.Id == cellLessonId.Value);
                            if (cellLesson != null && cellLesson.DisciplineId == _lesson.DisciplineId)
                            {
                                subjectCountInDay++;
                            }
                        }
                    }

                    if (subjectCountInDay < allowedPerDay)
                    {
                        teacher.Grid.Place(_lesson.Id, i);
                        group.Grid.Place(_lesson.Id, i);
                        return; // Застосовано успішно
                    }
                }
            }
        }
    }
}
