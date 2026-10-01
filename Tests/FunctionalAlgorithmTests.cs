using System;
using System.Linq;
using Lab1MAUI.Models.Schedule;
using Xunit;

namespace Lab1MAUI.Tests
{
    public class FunctionalAlgorithmTests
    {
        [Fact]
        public void Generate_BacktracksSuccessfully()
        {
            var config = new ScheduleConfig { Days = 1, CellsPerDay = 2 }; // Only 2 slots
            var schedule = new Schedule(config);
            
            var math = new Discipline(Guid.NewGuid(), "Math", config.TotalCells);
            schedule.Disciplines.Add(math);
            schedule.Rooms.Add(new Room(Guid.NewGuid(), "101", 30, config.TotalCells));
            
            var t1 = new Teacher(Guid.NewGuid(), "T1", config.TotalCells) { TargetHours = 2 };
            t1.Competences.Add(new TeacherCompetence { DisciplineId = math.Id });
            
            // Teacher can't work slot 0
            t1.Constraints.Add(new TimeOffConstraint(new[] { 0 }));
            schedule.Teachers.Add(t1);
            
            var g1 = new StudentGroup(Guid.NewGuid(), "G1", config.TotalCells) { StudentsCount = 20 };
            g1.Requirements.Add(new GroupRequirement { DisciplineId = math.Id, RequiredHours = 1 });
            schedule.Groups.Add(g1);

            schedule.Generate();

            var lessons = schedule.Grid.SelectMany(c => c).ToList();
            Assert.Single(lessons);
            Assert.Equal(1, lessons[0].CellIndex); // Must be placed in slot 1 because slot 0 is blocked
        }
    }
}
