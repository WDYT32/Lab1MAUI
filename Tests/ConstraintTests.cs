using Lab1MAUI.Models.Schedule.Constraints;
using System;
using System.Linq;
using Lab1MAUI.Models.Schedule;
using Lab1MAUI.Models.Schedule.Participants;
using Xunit;

namespace Lab1MAUI.Tests
{
    public class ConstraintTests
    {
        private Schedule CreateEmptySchedule(int days = 5, int cellsPerDay = 5) => 
            new Schedule(new ScheduleConfig { Days = days, CellsPerDay = cellsPerDay });

        [Fact]
        public void TimeOffConstraint_BlocksSpecificCell()
        {
            var schedule = CreateEmptySchedule(1, 2);
            var math = new Discipline(Guid.NewGuid(), "Math", 2);
            var room = new Room(Guid.NewGuid(), "101", 30, 2);
            
            var t1 = new Teacher(Guid.NewGuid(), "T1", 2) { TargetLessons = 1 };
            t1.Competences.Add(new TeacherCompetence { DisciplineId = math.Id });
            t1.Constraints.Add(new TimeOffConstraint(new[] { 0 })); 
            
            var g1 = new StudentGroup(Guid.NewGuid(), "G1", 2) { StudentsCount = 20 };
            g1.Requirements.Add(new GroupRequirement { DisciplineId = math.Id, RequiredLessons = 1 });

            schedule.Disciplines.Add(math);
            schedule.Rooms.Add(room);
            schedule.Teachers.Add(t1);
            schedule.Groups.Add(g1);

            schedule.Generate();

            var lessons = schedule.Grid.SelectMany(c => c).ToList();
            Assert.Single(lessons);
            Assert.Equal(1, lessons[0].CellIndex); 
        }

        [Fact]
        public void DailyLimitConstraint_SpreadsLessonsAcrossDays()
        {
            var schedule = CreateEmptySchedule(3, 3); 
            var math = new Discipline(Guid.NewGuid(), "Math", 9);
            var room = new Room(Guid.NewGuid(), "101", 30, 9);
            
            var t1 = new Teacher(Guid.NewGuid(), "T1", 9) { TargetLessons = 3 };
            t1.Competences.Add(new TeacherCompetence { DisciplineId = math.Id });
            
            var g1 = new StudentGroup(Guid.NewGuid(), "G1", 9) { StudentsCount = 20 };
            g1.Requirements.Add(new GroupRequirement { DisciplineId = math.Id, RequiredLessons = 3 });
            g1.Constraints.Add(new DailyLimitConstraint(g1, 1, 3)); 

            schedule.Disciplines.Add(math);
            schedule.Rooms.Add(room);
            schedule.Teachers.Add(t1);
            schedule.Groups.Add(g1);

            schedule.Generate();

            var lessons = schedule.Grid.SelectMany(c => c).ToList();
            Assert.Equal(3, lessons.Count);
            
            var days = lessons.Select(l => l.CellIndex / 3).Distinct().ToList();
            Assert.Equal(3, days.Count); 
        }

        [Fact]
        public void MaxConsecutiveLessonsConstraint_ForcesBreak()
        {
            var schedule = CreateEmptySchedule(1, 4); 
            var math = new Discipline(Guid.NewGuid(), "Math", 4);
            var room = new Room(Guid.NewGuid(), "101", 30, 4);
            
            var t1 = new Teacher(Guid.NewGuid(), "T1", 4) { TargetLessons = 3 };
            t1.Competences.Add(new TeacherCompetence { DisciplineId = math.Id });
            t1.Constraints.Add(new MaxConsecutiveLessonsConstraint(t1, 2, 4)); 
            
            var g1 = new StudentGroup(Guid.NewGuid(), "G1", 4) { StudentsCount = 20 };
            g1.Requirements.Add(new GroupRequirement { DisciplineId = math.Id, RequiredLessons = 3 });

            schedule.Disciplines.Add(math);
            schedule.Rooms.Add(room);
            schedule.Teachers.Add(t1);
            schedule.Groups.Add(g1);

            schedule.Generate();

            var lessons = schedule.Grid.SelectMany(c => c).OrderBy(l => l.CellIndex).ToList();
            Assert.Equal(3, lessons.Count);
            
            
            bool hasBreak = (lessons[1].CellIndex - lessons[0].CellIndex > 1) || 
                            (lessons[2].CellIndex - lessons[1].CellIndex > 1);
            Assert.True(hasBreak);
        }

        [Fact]
        public void SpecificDayConstraint_ForcesLessonsOnAllowedDays()
        {
            var schedule = CreateEmptySchedule(5, 5); 
            var math = new Discipline(Guid.NewGuid(), "Math", 25);
            var room = new Room(Guid.NewGuid(), "101", 30, 25);
            
            var t1 = new Teacher(Guid.NewGuid(), "T1", 25) { TargetLessons = 2 };
            t1.Competences.Add(new TeacherCompetence { DisciplineId = math.Id });
            
            t1.Constraints.Add(new SpecificDayConstraint(new[] { 3 }, 5));
            
            var g1 = new StudentGroup(Guid.NewGuid(), "G1", 25) { StudentsCount = 20 };
            g1.Requirements.Add(new GroupRequirement { DisciplineId = math.Id, RequiredLessons = 2 });

            schedule.Disciplines.Add(math);
            schedule.Rooms.Add(room);
            schedule.Teachers.Add(t1);
            schedule.Groups.Add(g1);

            schedule.Generate();

            var lessons = schedule.Grid.SelectMany(c => c).ToList();
            Assert.Equal(2, lessons.Count);
            Assert.All(lessons, l => Assert.Equal(3, l.CellIndex / 5)); 
        }

        [Fact]
        public void SpecificTimeConstraint_ForcesLessonsOnAllowedTimeSlots()
        {
            var schedule = CreateEmptySchedule(2, 5); 
            var math = new Discipline(Guid.NewGuid(), "Math", 10);
            var room = new Room(Guid.NewGuid(), "101", 30, 10);
            
            var t1 = new Teacher(Guid.NewGuid(), "T1", 10) { TargetLessons = 2 };
            t1.Competences.Add(new TeacherCompetence { DisciplineId = math.Id });
            
            t1.Constraints.Add(new SpecificTimeConstraint(new[] { 3 }, 5));
            
            var g1 = new StudentGroup(Guid.NewGuid(), "G1", 10) { StudentsCount = 20 };
            g1.Requirements.Add(new GroupRequirement { DisciplineId = math.Id, RequiredLessons = 2 });

            schedule.Disciplines.Add(math);
            schedule.Rooms.Add(room);
            schedule.Teachers.Add(t1);
            schedule.Groups.Add(g1);

            schedule.Generate();

            var lessons = schedule.Grid.SelectMany(c => c).ToList();
            Assert.Equal(2, lessons.Count);
            Assert.All(lessons, l => Assert.Equal(3, l.CellIndex % 5)); 
        }

        [Fact]
        public void ConstraintCombinations_ConflictingConstraints_Throws()
        {
            var schedule = CreateEmptySchedule(2, 5); 
            var math = new Discipline(Guid.NewGuid(), "Math", 10);
            var room = new Room(Guid.NewGuid(), "101", 30, 10);
            
            var t1 = new Teacher(Guid.NewGuid(), "T1", 10) { TargetLessons = 1 };
            t1.Competences.Add(new TeacherCompetence { DisciplineId = math.Id });
            t1.Constraints.Add(new SpecificDayConstraint(new[] { 0 }, 5)); 
            
            var g1 = new StudentGroup(Guid.NewGuid(), "G1", 10) { StudentsCount = 20 };
            g1.Requirements.Add(new GroupRequirement { DisciplineId = math.Id, RequiredLessons = 1 });
            g1.Constraints.Add(new SpecificDayConstraint(new[] { 1 }, 5)); 

            schedule.Disciplines.Add(math);
            schedule.Rooms.Add(room);
            schedule.Teachers.Add(t1);
            schedule.Groups.Add(g1);

            
            schedule.Generate();
            Assert.Empty(schedule.Grid.SelectMany(c => c)); 
        }

        [Fact]
        public void DisciplineConstraint_AppliesToAllGroupsAndTeachers()
        {
            var schedule = CreateEmptySchedule(2, 2); 
            var math = new Discipline(Guid.NewGuid(), "Math", 4);
            math.Constraints.Add(new SpecificDayConstraint(new[] { 1 }, 2)); 
            var room = new Room(Guid.NewGuid(), "101", 30, 4);
            
            var t1 = new Teacher(Guid.NewGuid(), "T1", 4) { TargetLessons = 1 };
            t1.Competences.Add(new TeacherCompetence { DisciplineId = math.Id });
            
            var g1 = new StudentGroup(Guid.NewGuid(), "G1", 4) { StudentsCount = 20 };
            g1.Requirements.Add(new GroupRequirement { DisciplineId = math.Id, RequiredLessons = 1 });

            schedule.Disciplines.Add(math);
            schedule.Rooms.Add(room);
            schedule.Teachers.Add(t1);
            schedule.Groups.Add(g1);

            schedule.Generate();

            var lessons = schedule.Grid.SelectMany(c => c).ToList();
            Assert.Single(lessons);
            Assert.Equal(1, lessons[0].CellIndex / 2); 
        }

        [Fact]
        public void DisciplineConstraint_DailyLimit_LimitsLessonsPerGroup()
        {
            
            var schedule = CreateEmptySchedule(2, 3); 
            
            var math = new Discipline(Guid.NewGuid(), "Math", 6);
            
            math.Constraints.Add(new DailyLimitConstraint(math, 2, 3));
            
            var room1 = new Room(Guid.NewGuid(), "101", 30, 6);
            var room2 = new Room(Guid.NewGuid(), "102", 30, 6);
            
            var t1 = new Teacher(Guid.NewGuid(), "T1", 6) { TargetLessons = 10 };
            t1.Competences.Add(new TeacherCompetence { DisciplineId = math.Id });
            var t2 = new Teacher(Guid.NewGuid(), "T2", 6) { TargetLessons = 10 };
            t2.Competences.Add(new TeacherCompetence { DisciplineId = math.Id });
            
            
            var g1 = new StudentGroup(Guid.NewGuid(), "G1", 6) { StudentsCount = 20 };
            g1.Requirements.Add(new GroupRequirement { DisciplineId = math.Id, RequiredLessons = 3 });
            
            
            var g2 = new StudentGroup(Guid.NewGuid(), "G2", 6) { StudentsCount = 20 };
            g2.Requirements.Add(new GroupRequirement { DisciplineId = math.Id, RequiredLessons = 3 });

            schedule.Disciplines.Add(math);
            schedule.Rooms.Add(room1);
            schedule.Rooms.Add(room2);
            schedule.Teachers.Add(t1);
            schedule.Teachers.Add(t2);
            schedule.Groups.Add(g1);
            schedule.Groups.Add(g2);

            schedule.Generate();

            var lessons = schedule.Grid.SelectMany(c => c).ToList();
            
            
            Assert.Equal(6, lessons.Count);

            
            var g1Lessons = lessons.Where(l => l.StudentGroupId == g1.Id).ToList();
            Assert.Equal(3, g1Lessons.Count);
            int g1Day0 = g1Lessons.Count(l => l.CellIndex < 3);
            int g1Day1 = g1Lessons.Count(l => l.CellIndex >= 3);
            Assert.True(g1Day0 <= 2);
            Assert.True(g1Day1 <= 2);

            
            var g2Lessons = lessons.Where(l => l.StudentGroupId == g2.Id).ToList();
            Assert.Equal(3, g2Lessons.Count);
            int g2Day0 = g2Lessons.Count(l => l.CellIndex < 3);
            int g2Day1 = g2Lessons.Count(l => l.CellIndex >= 3);
            Assert.True(g2Day0 <= 2);
            Assert.True(g2Day1 <= 2);
        }
    }
}
