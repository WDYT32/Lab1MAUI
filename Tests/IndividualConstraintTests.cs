using Lab1MAUI.Models.Schedule.Constraints;
using System;
using System.Linq;
using Lab1MAUI.Models.Schedule;
using Lab1MAUI.Models.Schedule.Participants;
using Lab1MAUI.Models.Schedule.Constraints;
using Xunit;

namespace Lab1MAUI.Tests
{
    public class IndividualConstraintTests
    {
        [Fact]
        public void DailyLimitConstraint_Works()
        {
            var config = new ScheduleConfig { Days = 2, CellsPerDay = 2 };
            var schedule = new Schedule(config);
            
            var math = new Discipline(Guid.NewGuid(), "Math", config.TotalCells);
            schedule.Disciplines.Add(math);
            schedule.Rooms.Add(new Room(Guid.NewGuid(), "101", 30, config.TotalCells));
            
            var t1 = new Teacher(Guid.NewGuid(), "T1", config.TotalCells) { TargetHours = 2 };
            t1.Competences.Add(new TeacherCompetence { DisciplineId = math.Id });
            schedule.Teachers.Add(t1);
            
            var g1 = new StudentGroup(Guid.NewGuid(), "G1", config.TotalCells) { StudentsCount = 20 };
            g1.Requirements.Add(new GroupRequirement { DisciplineId = math.Id, RequiredHours = 2 });
            
            // Limit to 1 per day
            g1.Constraints.Add(new DailyLimitConstraint(g1, 1, config.CellsPerDay));
            schedule.Groups.Add(g1);

            schedule.Generate();

            var lessons = schedule.Grid.SelectMany(c => c).OrderBy(l => l.CellIndex).ToList();
            
            // Should be on different days
            int day1 = lessons[0].CellIndex / config.CellsPerDay;
            int day2 = lessons[1].CellIndex / config.CellsPerDay;
            
            Assert.NotEqual(day1, day2);
        }
    }
}
