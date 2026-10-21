using Lab1MAUI.Models.Schedule.Constraints;
using System;
using Lab1MAUI.Models.Schedule;
using Lab1MAUI.Models.Schedule.Participants;
using Xunit;

namespace Lab1MAUI.Tests
{
    public class EdgeCaseTests
    {
        [Fact]
        public void Generate_FailsIfRoomTooSmall()
        {
            var config = new ScheduleConfig { Days = 1, CellsPerDay = 1 };
            var schedule = new Schedule(config);
            
            var math = new Discipline(Guid.NewGuid(), "Math", config.TotalCells);
            schedule.Disciplines.Add(math);
            
            
            schedule.Rooms.Add(new Room(Guid.NewGuid(), "101", 10, config.TotalCells));
            
            var teacher1 = new Teacher(Guid.NewGuid(), "T1", config.TotalCells) { TargetLessons = 1 };
            teacher1.Competences.Add(new TeacherCompetence { DisciplineId = math.Id });
            schedule.Teachers.Add(teacher1);
            
            var group1 = new StudentGroup(Guid.NewGuid(), "G1", config.TotalCells) { StudentsCount = 20 };
            group1.Requirements.Add(new GroupRequirement { DisciplineId = math.Id, RequiredLessons = 1 });
            schedule.Groups.Add(group1);

            schedule.Generate();
            Assert.Empty(schedule.Grid[0]); 
        }
    }
}
