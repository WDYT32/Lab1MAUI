using System;
using System.Linq;
using Lab1MAUI.Models.Schedule;
using Xunit;

namespace Lab1MAUI.Tests;

public class IndividualConstraintTests
{
    [Fact]
    public void Test_DailyLimitConstraint_OnGroup()
    {
        var schedule = new Schedule();
        schedule.Config.Days = 5;
        schedule.Config.CellsPerDay = 5; // 25 total cells

        var group = new StudentGroup { Id = Guid.NewGuid(), Name = "Group1" };
        var teacher = new Teacher { Id = Guid.NewGuid(), FirstName = "T1" };
        var subject = new Discipline { Id = Guid.NewGuid(), Name = "Math" };

        schedule.Groups.Add(group);
        schedule.Teachers.Add(teacher);
        schedule.Disciplines.Add(subject);

        teacher.Competences.Add(new TeacherCompetence { DisciplineId = subject.Id });
        
        // Group wants 10 Math lessons
        group.Requirements.Add(new GroupRequirement { DisciplineId = subject.Id, RequiredHours = 10 });

        // But limit is 2 per day!
        group.Constraints.Add(new DailyLimitConstraint(group.Id, 2));

        schedule.Generate();

        // Check if ANY day has more than 2 lessons for this group
        for (int day = 0; day < schedule.Config.Days; day++)
        {
            int lessonsInDay = 0;
            for (int i = 0; i < schedule.Config.CellsPerDay; i++)
            {
                int cellIndex = day * schedule.Config.CellsPerDay + i;
                if (!group.Grid.IsFree(cellIndex))
                {
                    var lessonId = group.Grid.Cells[cellIndex];
                    // Only count ACTUAL lessons, not blockers
                    if (schedule.Lessons.Any(l => l.Id == lessonId))
                    {
                        lessonsInDay++;
                    }
                }
            }
            Assert.True(lessonsInDay <= 2, $"Day {day} has {lessonsInDay} lessons, expected <= 2");
        }
        
        // 10 lessons total should be placed (5 days * 2 lessons/day = 10 capacity)
        var unassigned = schedule.Lessons.Count(l => !group.Grid.Cells.Contains(l.Id));
        Assert.Equal(0, unassigned);
    }
    
    [Fact]
    public void Test_DailyLimitConstraint_OnTeacher()
    {
        var schedule = new Schedule();
        schedule.Config.Days = 5;
        schedule.Config.CellsPerDay = 5;

        var group1 = new StudentGroup { Id = Guid.NewGuid(), Name = "Group1" };
        var group2 = new StudentGroup { Id = Guid.NewGuid(), Name = "Group2" };
        var teacher = new Teacher { Id = Guid.NewGuid(), FirstName = "T1" };
        var subject = new Discipline { Id = Guid.NewGuid(), Name = "Math" };

        schedule.Groups.Add(group1);
        schedule.Groups.Add(group2);
        schedule.Teachers.Add(teacher);
        schedule.Disciplines.Add(subject);

        teacher.Competences.Add(new TeacherCompetence { DisciplineId = subject.Id });
        
        // Each group wants 5 math lessons. Total 10 math lessons for Teacher1.
        group1.Requirements.Add(new GroupRequirement { DisciplineId = subject.Id, RequiredHours = 5 });
        group2.Requirements.Add(new GroupRequirement { DisciplineId = subject.Id, RequiredHours = 5 });

        // Teacher limit is 2 per day!
        teacher.Constraints.Add(new DailyLimitConstraint(teacher.Id, 2));

        schedule.Generate();

        for (int day = 0; day < schedule.Config.Days; day++)
        {
            int lessonsInDay = 0;
            for (int i = 0; i < schedule.Config.CellsPerDay; i++)
            {
                int cellIndex = day * schedule.Config.CellsPerDay + i;
                if (!teacher.Grid.IsFree(cellIndex))
                {
                    var lessonId = teacher.Grid.Cells[cellIndex];
                    if (schedule.Lessons.Any(l => l.Id == lessonId))
                    {
                        lessonsInDay++;
                    }
                }
            }
            Assert.True(lessonsInDay <= 2, $"Teacher has {lessonsInDay} lessons on day {day}, expected <= 2");
        }
    }
    [Fact]
    public void Test_DisciplineDailyLimitConstraint()
    {
        var schedule = new Schedule();
        schedule.Config.Days = 5;
        schedule.Config.CellsPerDay = 5;

        var group = new StudentGroup { Id = Guid.NewGuid(), Name = "Group1" };
        var teacher = new Teacher { Id = Guid.NewGuid(), FirstName = "T1" };
        var math = new Discipline { Id = Guid.NewGuid(), Name = "Math" };
        var physics = new Discipline { Id = Guid.NewGuid(), Name = "Physics" };

        schedule.Groups.Add(group);
        schedule.Teachers.Add(teacher);
        schedule.Disciplines.Add(math);
        schedule.Disciplines.Add(physics);

        teacher.Competences.Add(new TeacherCompetence { DisciplineId = math.Id });
        teacher.Competences.Add(new TeacherCompetence { DisciplineId = physics.Id });
        
        // Group wants 10 Math and 10 Physics
        group.Requirements.Add(new GroupRequirement { DisciplineId = math.Id, RequiredHours = 10 });
        group.Requirements.Add(new GroupRequirement { DisciplineId = physics.Id, RequiredHours = 10 });

        // Discipline limit: Max 1 Math per day per group
        math.Constraints.Add(new DisciplineDailyLimitConstraint(math.Id, 1));

        schedule.Generate();

        for (int day = 0; day < schedule.Config.Days; day++)
        {
            int mathInDay = 0;
            for (int i = 0; i < schedule.Config.CellsPerDay; i++)
            {
                int cellIndex = day * schedule.Config.CellsPerDay + i;
                if (!group.Grid.IsFree(cellIndex))
                {
                    var lesson = schedule.Lessons.FirstOrDefault(l => l.Id == group.Grid.Cells[cellIndex]);
                    if (lesson != null && lesson.DisciplineId == math.Id)
                    {
                        mathInDay++;
                    }
                }
            }
            Assert.True(mathInDay <= 1, $"Group has {mathInDay} Math lessons on day {day}, expected <= 1");
        }
        
        // 10 math lessons requested, max 1 per day across 5 days = 5 math lessons placed, 5 unassigned!
        var unassignedMath = schedule.Lessons.Count(l => l.DisciplineId == math.Id && !group.Grid.Cells.Contains(l.Id));
        Assert.Equal(5, unassignedMath);
    }
}
