using Lab1MAUI.Models.Schedule;
using Lab1MAUI.Models.Schedule.Constraints;
using Lab1MAUI.Models.Schedule.Participants;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Lab1MAUI.Tests
{
    public class ComplexScheduleTests
    {
        private Schedule CreateSchedule(int days, int cellsPerDay) =>
            new Schedule(new ScheduleConfig { Days = days, CellsPerDay = cellsPerDay });

        [Fact]
        public void Bottleneck_NotEnoughRooms_ResultsInPartialSchedule()
        {
            
            var schedule = CreateSchedule(1, 1);
            var math = new Discipline(Guid.NewGuid(), "Math", 1);
            schedule.Disciplines.Add(math);

            
            for (int i = 0; i < 3; i++)
            {
                var t = new Teacher(Guid.NewGuid(), $"T{i}", 1) { TargetLessons = 1 };
                t.Competences.Add(new TeacherCompetence { DisciplineId = math.Id });
                schedule.Teachers.Add(t);

                var g = new StudentGroup(Guid.NewGuid(), $"G{i}", 1) { StudentsCount = 20 };
                g.Requirements.Add(new GroupRequirement { DisciplineId = math.Id, RequiredLessons = 1 });
                schedule.Groups.Add(g);
            }

            
            schedule.Rooms.Add(new Room(Guid.NewGuid(), "101", 30, 1));
            schedule.Rooms.Add(new Room(Guid.NewGuid(), "102", 30, 1));

            schedule.Generate();

            var placedLessons = schedule.Grid.SelectMany(c => c).ToList();
            
            
            Assert.Equal(2, placedLessons.Count);
        }

        [Fact]
        public void Combinatorial_MaxConsecutive_And_DailyLimit_CorrectlySpacesLessons()
        {
            
            
            
            var schedule = CreateSchedule(2, 4);
            var physics = new Discipline(Guid.NewGuid(), "Physics", 8);
            
            
            physics.Constraints.Add(new DailyLimitConstraint(physics, 2, 4));
            physics.Constraints.Add(new MaxConsecutiveLessonsConstraint(physics, 1, 4));
            
            schedule.Disciplines.Add(physics);
            schedule.Rooms.Add(new Room(Guid.NewGuid(), "101", 30, 8));
            
            var t1 = new Teacher(Guid.NewGuid(), "Einstein", 8) { TargetLessons = 10 };
            t1.Competences.Add(new TeacherCompetence { DisciplineId = physics.Id });
            schedule.Teachers.Add(t1);

            var g1 = new StudentGroup(Guid.NewGuid(), "G1", 8) { StudentsCount = 20 };
            
            g1.Requirements.Add(new GroupRequirement { DisciplineId = physics.Id, RequiredLessons = 4 });
            schedule.Groups.Add(g1);

            schedule.Generate();

            var lessons = schedule.Grid.SelectMany(c => c).OrderBy(l => l.CellIndex).ToList();
            
            
            Assert.Equal(4, lessons.Count);

            
            for (int i = 0; i < lessons.Count - 1; i++)
            {
                int currentCell = lessons[i].CellIndex;
                int nextCell = lessons[i + 1].CellIndex;

                
                if (currentCell / 4 == nextCell / 4)
                {
                    Assert.True(nextCell - currentCell >= 2, $"Lessons are consecutive at slots {currentCell} and {nextCell}");
                }
            }

            
            Assert.True(lessons.Count(l => l.CellIndex < 4) <= 2);
            Assert.True(lessons.Count(l => l.CellIndex >= 4) <= 2);
        }

        [Fact]
        public void Combinatorial_TeacherTimeOff_And_GroupSpecificDay_YieldsPartial()
        {
            
            
            var schedule = CreateSchedule(2, 2);
            var math = new Discipline(Guid.NewGuid(), "Math", 4);
            schedule.Disciplines.Add(math);
            schedule.Rooms.Add(new Room(Guid.NewGuid(), "101", 30, 4));

            var t1 = new Teacher(Guid.NewGuid(), "T1", 4) { TargetLessons = 5 };
            t1.Competences.Add(new TeacherCompetence { DisciplineId = math.Id });
            
            t1.Constraints.Add(new TimeOffConstraint(new[] { 1 }));
            schedule.Teachers.Add(t1);

            var g1 = new StudentGroup(Guid.NewGuid(), "G1", 4) { StudentsCount = 20 };
            
            g1.Requirements.Add(new GroupRequirement { DisciplineId = math.Id, RequiredLessons = 2 });
            
            g1.Constraints.Add(new SpecificDayConstraint(new[] { 0 }, 2));
            schedule.Groups.Add(g1);

            schedule.Generate();

            var lessons = schedule.Grid.SelectMany(c => c).ToList();

            
            
            
            
            
            Assert.Single(lessons);
            Assert.Equal(0, lessons[0].CellIndex);
        }

        [Fact]
        public void StressTest_MultipleGroupsTeachersAndRooms_FullySatisfied()
        {
            
            var schedule = CreateSchedule(5, 5);

            
            var disciplines = new List<Discipline>();
            for (int i = 0; i < 5; i++)
            {
                var d = new Discipline(Guid.NewGuid(), $"Disc_{i}", 25);
                disciplines.Add(d);
                schedule.Disciplines.Add(d);
            }

            
            for (int i = 0; i < 10; i++)
            {
                schedule.Rooms.Add(new Room(Guid.NewGuid(), $"Room_{i}", 30, 25));
            }

            
            for (int i = 0; i < 10; i++)
            {
                var t = new Teacher(Guid.NewGuid(), $"T_{i}", 25) { TargetLessons = 25 };
                
                t.Competences.Add(new TeacherCompetence { DisciplineId = disciplines[i / 2].Id });
                schedule.Teachers.Add(t);
            }

            
            for (int i = 0; i < 10; i++)
            {
                var g = new StudentGroup(Guid.NewGuid(), $"G_{i}", 25) { StudentsCount = 20 };
                foreach (var d in disciplines)
                {
                    g.Requirements.Add(new GroupRequirement { DisciplineId = d.Id, RequiredLessons = 2 });
                }
                schedule.Groups.Add(g);
            }

            
            
            

            schedule.Generate();

            var placedLessons = schedule.Grid.SelectMany(c => c).ToList();
            Assert.Equal(100, placedLessons.Count);

            
            foreach (var g in schedule.Groups)
            {
                foreach (var req in g.Requirements)
                {
                    Assert.Equal(2, req.FulfilledLessons);
                }
            }
        }
    }
}
