using System;
using Xunit;
using Lab1MAUI.Models.Schedule.Constraints;
using Lab1MAUI.Models.Schedule;
using Lab1MAUI.Models.Schedule.Participants;
using System.Linq;

namespace Lab1MAUI.Tests
{
    public class DebugTemp2
    {
        [Fact]
        public void RunTest()
        {
            var s = new Schedule(new ScheduleConfig { Days = 1, CellsPerDay = 4 });
            var math = new Discipline(Guid.NewGuid(), "Math", 4);
            s.Disciplines.Add(math);
            s.Rooms.Add(new Room(Guid.NewGuid(), "101", 30, 4));

            var t = new Teacher(Guid.NewGuid(), "T1", 4) { TargetHours = 10 };
            t.Competences.Add(new TeacherCompetence { DisciplineId = math.Id });
            t.Constraints.Add(new TimeOffConstraint(new[] { 0, 2 }));
            s.Teachers.Add(t);

            var g = new StudentGroup(Guid.NewGuid(), "G1", 4) { StudentsCount = 20 };
            g.Requirements.Add(new GroupRequirement { DisciplineId = math.Id, RequiredHours = 2 });
            g.Constraints.Add(new SpecificTimeConstraint(new[] { 0, 2 }, 4));
            s.Groups.Add(g);

            var gMask = g.GetAvailabilityMask();
            Console.WriteLine($"Group Mask: {string.Join(", ", gMask)}");
            var tMask = t.GetAvailabilityMask();
            Console.WriteLine($"Teacher Mask: {string.Join(", ", tMask)}");

            s.Generate();
            var lessons = s.Grid.SelectMany(c => c).ToList();
            Console.WriteLine($"Placed lessons: {lessons.Count}");
            foreach (var l in lessons) {
                Console.WriteLine($"Placed at: {l.CellIndex}");
            }
            Assert.Empty(lessons);
        }
    }
}
