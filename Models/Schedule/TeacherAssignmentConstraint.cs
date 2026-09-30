using System;
using System.Linq;

namespace Lab1MAUI.Models.Schedule;

public class TeacherAssignmentConstraint : IConstraint
{
    public int Priority { get; set; } = 900; // Виконується після спавну (1000), але до розміщення (100)
    public Guid OwnerId { get; } 
    
    private readonly Guid? _specificTeacherId;
    private readonly Guid? _disciplineId;

    public TeacherAssignmentConstraint(StudentGroup group, Guid? specificTeacherId = null, Guid? disciplineId = null)
    {
        OwnerId = group.Id;
        _specificTeacherId = specificTeacherId;
        _disciplineId = disciplineId;
    }

    public void Apply(Schedule schedule)
    {
        var unassignedLessons = schedule.Lessons
            .Where(l => l.StudentGroupId == OwnerId && l.TeacherId == Guid.Empty)
            .ToList();

        if (_disciplineId.HasValue) unassignedLessons = unassignedLessons.Where(l => l.DisciplineId == _disciplineId.Value).ToList();

        foreach (var lesson in unassignedLessons)
        {
            Teacher assignedTeacher = null;

            if (_specificTeacherId.HasValue)
            {
                assignedTeacher = schedule.Teachers.FirstOrDefault(t => t.Id == _specificTeacherId.Value);
            }
            else
            {
                var capable = schedule.Teachers.Where(t => 
                    t.Competences.Any(c => c.DisciplineId == lesson.DisciplineId)).ToList();

                if (capable.Any())
                {
                    // Детермінований вибір: викладач з найбільшим залишком годин
                    assignedTeacher = capable
                        .OrderByDescending(t => t.Grid.TargetHours - schedule.Lessons.Count(l => l.TeacherId == t.Id))
                        .First();
                }
            }

            if (assignedTeacher != null)
            {
                lesson.TeacherId = assignedTeacher.Id;
            }
        }
    }
}
