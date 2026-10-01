using System;
using System.Linq;
using Lab1MAUI.Models.Schedule;
using Xunit;

namespace Lab1MAUI.Tests;

public class ConstraintCombinationsTests
{
    [Fact]
    public void Combo_DailyLimit_And_MaxConsecutive()
    {
        var schedule = new Schedule();
        schedule.Config.Days = 1;
        schedule.Config.CellsPerDay = 5;

        var subject = new Discipline { Id = Guid.NewGuid(), Name = "Math" };
        schedule.Disciplines.Add(subject);

        var teacher = new Teacher { Id = Guid.NewGuid(), FirstName = "T1" };
        teacher.Competences.Add(new TeacherCompetence { DisciplineId = subject.Id });
        schedule.Teachers.Add(teacher);

        var group = new StudentGroup { Id = Guid.NewGuid(), Name = "G1" };
        group.Requirements.Add(new GroupRequirement { DisciplineId = subject.Id, RequiredHours = 5 });
        
        // Max 3 lessons per day, and max 1 consecutive (needs window between them).
        // For 5 cells:
        // Possible pattern: L - L - L
        // Max placed lessons: 3.
        group.Constraints.Add(new DailyLimitConstraint(group.Id, 3));
        group.Constraints.Add(new MaxConsecutiveLessonsConstraint(group.Id, 1));
        
        schedule.Groups.Add(group);
        schedule.Generate();

        var assigned = schedule.Lessons.Count(l => group.Grid.Cells.Contains(l.Id));
        Assert.Equal(2, assigned);
        
        // Ensure no two consecutive lessons are placed
        for (int i = 0; i < 4; i++)
        {
            bool currentHasLesson = !group.Grid.IsFree(i) && schedule.Lessons.Any(l => l.Id == group.Grid.Cells[i]);
            bool nextHasLesson = !group.Grid.IsFree(i + 1) && schedule.Lessons.Any(l => l.Id == group.Grid.Cells[i + 1]);
            Assert.False(currentHasLesson && nextHasLesson, $"Found consecutive lessons at index {i} and {i+1}");
        }
    }

    [Fact]
    public void Combo_TimeOff_And_DailyLimit()
    {
        var schedule = new Schedule();
        schedule.Config.Days = 1;
        schedule.Config.CellsPerDay = 5;

        var subject = new Discipline { Id = Guid.NewGuid(), Name = "Physics" };
        schedule.Disciplines.Add(subject);

        var teacher = new Teacher { Id = Guid.NewGuid(), FirstName = "T1" };
        teacher.Competences.Add(new TeacherCompetence { DisciplineId = subject.Id });
        
        // Teacher blocked at cell 0 and 1
        teacher.Constraints.Add(new TeacherTimeOffConstraint(teacher, 0));
        teacher.Constraints.Add(new TeacherTimeOffConstraint(teacher, 1));
        // Teacher daily limit: 2
        teacher.Constraints.Add(new DailyLimitConstraint(teacher.Id, 2));
        
        schedule.Teachers.Add(teacher);

        var group = new StudentGroup { Id = Guid.NewGuid(), Name = "G1" };
        group.Requirements.Add(new GroupRequirement { DisciplineId = subject.Id, RequiredHours = 5 });
        schedule.Groups.Add(group);

        schedule.Generate();

        // Check if TimeOff is respected
        Assert.False(schedule.Lessons.Any(l => l.Id == teacher.Grid.Cells[0]), "Lesson placed in blocked cell 0");
        Assert.False(schedule.Lessons.Any(l => l.Id == teacher.Grid.Cells[1]), "Lesson placed in blocked cell 1");

        // Total placed lessons should be 2 (because of daily limit)
        var assigned = schedule.Lessons.Count(l => teacher.Grid.Cells.Contains(l.Id));
        Assert.Equal(2, assigned);
    }

    [Fact]
    public void Combo_TeacherAssignment_And_DisciplineDailyLimit()
    {
        var schedule = new Schedule();
        schedule.Config.Days = 2;
        schedule.Config.CellsPerDay = 4;

        var math = new Discipline { Id = Guid.NewGuid(), Name = "Math" };
        schedule.Disciplines.Add(math);
        
        // Limit math to 1 per day
        math.Constraints.Add(new DisciplineDailyLimitConstraint(math.Id, 1));

        var t1 = new Teacher { Id = Guid.NewGuid(), FirstName = "Math Pro" };
        t1.Competences.Add(new TeacherCompetence { DisciplineId = math.Id });
        schedule.Teachers.Add(t1);

        var t2 = new Teacher { Id = Guid.NewGuid(), FirstName = "Math Junior" };
        t2.Competences.Add(new TeacherCompetence { DisciplineId = math.Id });
        schedule.Teachers.Add(t2);

        var group = new StudentGroup { Id = Guid.NewGuid(), Name = "G1" };
        group.Requirements.Add(new GroupRequirement { DisciplineId = math.Id, RequiredHours = 4 });
        
        // Force group to use T2
        group.Constraints.Add(new TeacherAssignmentConstraint(group, t2.Id));
        schedule.Groups.Add(group);

        schedule.Generate();

        // Group asked for 4 Math. Discipline limits to 1 per day. Days = 2.
        // Placed lessons = 2.
        // Teacher must be T2.
        
        var mathLessons = schedule.Lessons.Where(l => l.DisciplineId == math.Id).ToList();
        var placedWithT2 = mathLessons.Count(l => l.TeacherId == t2.Id && group.Grid.Cells.Contains(l.Id));
        var placedWithT1 = mathLessons.Count(l => l.TeacherId == t1.Id && group.Grid.Cells.Contains(l.Id));
        
        Assert.Equal(2, placedWithT2);
        Assert.Equal(0, placedWithT1);
        Assert.Equal(2, mathLessons.Count(l => !group.Grid.Cells.Contains(l.Id))); // Unassigned
    }

    [Fact]
    public void Combo_MaxConsecutive_And_DisciplineDailyLimit()
    {
        var schedule = new Schedule();
        schedule.Config.Days = 1;
        schedule.Config.CellsPerDay = 5;

        var hist = new Discipline { Id = Guid.NewGuid(), Name = "History" };
        hist.Constraints.Add(new DisciplineDailyLimitConstraint(hist.Id, 2)); // Post-placement dropper
        schedule.Disciplines.Add(hist);

        var teacher = new Teacher { Id = Guid.NewGuid(), FirstName = "T1" };
        teacher.Competences.Add(new TeacherCompetence { DisciplineId = hist.Id });
        schedule.Teachers.Add(teacher);

        var group = new StudentGroup { Id = Guid.NewGuid(), Name = "G1" };
        group.Requirements.Add(new GroupRequirement { DisciplineId = hist.Id, RequiredHours = 4 });
        // Max consecutive = 1 (pre-placement blocker)
        group.Constraints.Add(new MaxConsecutiveLessonsConstraint(group.Id, 1)); 
        schedule.Groups.Add(group);

        schedule.Generate();

        // 1. MaxConsecutive limits to 3 lessons in 5 cells (e.g. indices 0, 2, 4) during placement.
        // 2. DisciplineDailyLimit runs after, sees 3 lessons, drops 1.
        // 3. Final placed = 2 lessons.
        var assigned = schedule.Lessons.Count(l => group.Grid.Cells.Contains(l.Id));
        Assert.Equal(2, assigned);
    }
    
    [Fact]
    public void Combo_TimeOff_Blocks_All_Teachers()
    {
        // Tests scenario where all teachers for a subject have TimeOff on the same cell
        var schedule = new Schedule();
        schedule.Config.Days = 1;
        schedule.Config.CellsPerDay = 2;

        var subject = new Discipline { Id = Guid.NewGuid(), Name = "Art" };
        schedule.Disciplines.Add(subject);

        var t1 = new Teacher { Id = Guid.NewGuid(), FirstName = "Art1" };
        t1.Competences.Add(new TeacherCompetence { DisciplineId = subject.Id });
        t1.Constraints.Add(new TeacherTimeOffConstraint(t1, 0)); // Blocks cell 0
        schedule.Teachers.Add(t1);

        var t2 = new Teacher { Id = Guid.NewGuid(), FirstName = "Art2" };
        t2.Competences.Add(new TeacherCompetence { DisciplineId = subject.Id });
        t2.Constraints.Add(new TeacherTimeOffConstraint(t2, 0)); // Blocks cell 0
        schedule.Teachers.Add(t2);

        var group = new StudentGroup { Id = Guid.NewGuid(), Name = "G1" };
        group.Requirements.Add(new GroupRequirement { DisciplineId = subject.Id, RequiredHours = 2 });
        schedule.Groups.Add(group);

        schedule.Generate();

        // Cell 0 is completely blocked for Art. Only Cell 1 is available.
        // Group requested 2 hours, but only 1 can be placed.
        var assigned = schedule.Lessons.Count(l => group.Grid.Cells.Contains(l.Id));
        Assert.Equal(1, assigned);
        Assert.False(schedule.Lessons.Any(l => l.Id == group.Grid.Cells[0]));
        Assert.True(schedule.Lessons.Any(l => l.Id == group.Grid.Cells[1]));
    }
}
