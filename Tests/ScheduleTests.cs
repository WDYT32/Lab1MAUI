using Lab1MAUI.Models.Schedule.Constraints;
using System;
using System.Linq;
using Lab1MAUI.Models.Schedule;
using Lab1MAUI.Models.Schedule.Participants;
using Xunit;

namespace Lab1MAUI.Tests
{
    public class ScheduleTests
    {
        [Fact]
        public void Generate_AssignsLessonsCorrectly_WithoutOverlaps()
        {
            var config = new ScheduleConfig { Days = 5, CellsPerDay = 8 };
            var schedule = new Schedule(config);
            
            var math = new Discipline(Guid.NewGuid(), "Math", config.TotalCells);
            schedule.Disciplines.Add(math);
            var room = new Room(Guid.NewGuid(), "101", 30, config.TotalCells);
            schedule.Rooms.Add(room);
            
            var teacher1 = new Teacher(Guid.NewGuid(), "T1", config.TotalCells) { TargetLessons = 2 };
            teacher1.Competences.Add(new TeacherCompetence { DisciplineId = math.Id });
            schedule.Teachers.Add(teacher1);
            
            var group1 = new StudentGroup(Guid.NewGuid(), "G1", config.TotalCells) { StudentsCount = 20 };
            group1.Requirements.Add(new GroupRequirement { DisciplineId = math.Id, RequiredLessons = 2 });
            schedule.Groups.Add(group1);

            schedule.Generate();

            var flatGrid = schedule.Grid.SelectMany(c => c).ToList();
            Assert.Equal(2, flatGrid.Count);
            Assert.NotEqual(flatGrid[0].CellIndex, flatGrid[1].CellIndex);
        }
    }
}
