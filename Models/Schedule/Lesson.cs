using System;
using System.Collections.Generic;

namespace Lab1MAUI.Models.Schedule;

public class Lesson
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid TeacherId { get; set; }
    public Guid StudentGroupId { get; set; }
    public Guid DisciplineId { get; set; }
    public string SubjectName { get; set; } = string.Empty;
    
    public List<IConstraint> GetLessonConstraints()
    {
        return new List<IConstraint>
        {
            new LessonPlacementConstraint(this)
        };
    }
}
