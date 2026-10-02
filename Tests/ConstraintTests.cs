using Lab1MAUI.Models.Schedule.Constraints;
using System;
using System.Linq;
using Lab1MAUI.Models.Schedule;
using Lab1MAUI.Models.Schedule.Participants;
using Lab1MAUI.Models.Schedule.Constraints;
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
            
            var t1 = new Teacher(Guid.NewGuid(), "T1", 2) { TargetHours = 1 };
            t1.Competences.Add(new TeacherCompetence { DisciplineId = math.Id });
            t1.Constraints.Add(new TimeOffConstraint(new[] { 0 })); // Block slot 0
            
            var g1 = new StudentGroup(Guid.NewGuid(), "G1", 2) { StudentsCount = 20 };
            g1.Requirements.Add(new GroupRequirement { DisciplineId = math.Id, RequiredHours = 1 });

            schedule.Disciplines.Add(math);
            schedule.Rooms.Add(room);
            schedule.Teachers.Add(t1);
            schedule.Groups.Add(g1);

            schedule.Generate();

            var lessons = schedule.Grid.SelectMany(c => c).ToList();
            Assert.Single(lessons);
            Assert.Equal(1, lessons[0].CellIndex); // Must be in slot 1
        }

        [Fact]
        public void DailyLimitConstraint_SpreadsLessonsAcrossDays()
        {
            var schedule = CreateEmptySchedule(3, 3); // 3 days, 3 slots each
            var math = new Discipline(Guid.NewGuid(), "Math", 9);
            var room = new Room(Guid.NewGuid(), "101", 30, 9);
            
            var t1 = new Teacher(Guid.NewGuid(), "T1", 9) { TargetHours = 3 };
            t1.Competences.Add(new TeacherCompetence { DisciplineId = math.Id });
            
            var g1 = new StudentGroup(Guid.NewGuid(), "G1", 9) { StudentsCount = 20 };
            g1.Requirements.Add(new GroupRequirement { DisciplineId = math.Id, RequiredHours = 3 });
            g1.Constraints.Add(new DailyLimitConstraint(g1, 1, 3)); // Max 1 per day

            schedule.Disciplines.Add(math);
            schedule.Rooms.Add(room);
            schedule.Teachers.Add(t1);
            schedule.Groups.Add(g1);

            schedule.Generate();

            var lessons = schedule.Grid.SelectMany(c => c).ToList();
            Assert.Equal(3, lessons.Count);
            
            var days = lessons.Select(l => l.CellIndex / 3).Distinct().ToList();
            Assert.Equal(3, days.Count); // Must be on 3 different days
        }

        [Fact]
        public void MaxConsecutiveLessonsConstraint_ForcesBreak()
        {
            var schedule = CreateEmptySchedule(1, 4); // 1 day, 4 slots
            var math = new Discipline(Guid.NewGuid(), "Math", 4);
            var room = new Room(Guid.NewGuid(), "101", 30, 4);
            
            var t1 = new Teacher(Guid.NewGuid(), "T1", 4) { TargetHours = 3 };
            t1.Competences.Add(new TeacherCompetence { DisciplineId = math.Id });
            t1.Constraints.Add(new MaxConsecutiveLessonsConstraint(t1, 2, 4)); // Max 2 consecutive
            
            var g1 = new StudentGroup(Guid.NewGuid(), "G1", 4) { StudentsCount = 20 };
            g1.Requirements.Add(new GroupRequirement { DisciplineId = math.Id, RequiredHours = 3 });

            schedule.Disciplines.Add(math);
            schedule.Rooms.Add(room);
            schedule.Teachers.Add(t1);
            schedule.Groups.Add(g1);

            schedule.Generate();

            var lessons = schedule.Grid.SelectMany(c => c).OrderBy(l => l.CellIndex).ToList();
            Assert.Equal(3, lessons.Count);
            
            // Expected slots: 0, 1, 3 OR 0, 2, 3 (a break is forced)
            bool hasBreak = (lessons[1].CellIndex - lessons[0].CellIndex > 1) || 
                            (lessons[2].CellIndex - lessons[1].CellIndex > 1);
            Assert.True(hasBreak);
        }

        [Fact]
        public void SpecificDayConstraint_ForcesLessonsOnAllowedDays()
        {
            var schedule = CreateEmptySchedule(5, 5); // 5 days
            var math = new Discipline(Guid.NewGuid(), "Math", 25);
            var room = new Room(Guid.NewGuid(), "101", 30, 25);
            
            var t1 = new Teacher(Guid.NewGuid(), "T1", 25) { TargetHours = 2 };
            t1.Competences.Add(new TeacherCompetence { DisciplineId = math.Id });
            // Only allowed on Thursday (day 3)
            t1.Constraints.Add(new SpecificDayConstraint(new[] { 3 }, 5));
            
            var g1 = new StudentGroup(Guid.NewGuid(), "G1", 25) { StudentsCount = 20 };
            g1.Requirements.Add(new GroupRequirement { DisciplineId = math.Id, RequiredHours = 2 });

            schedule.Disciplines.Add(math);
            schedule.Rooms.Add(room);
            schedule.Teachers.Add(t1);
            schedule.Groups.Add(g1);

            schedule.Generate();

            var lessons = schedule.Grid.SelectMany(c => c).ToList();
            Assert.Equal(2, lessons.Count);
            Assert.All(lessons, l => Assert.Equal(3, l.CellIndex / 5)); // Day must be 3
        }

        [Fact]
        public void SpecificTimeConstraint_ForcesLessonsOnAllowedTimeSlots()
        {
            var schedule = CreateEmptySchedule(2, 5); // 2 days, 5 slots each
            var math = new Discipline(Guid.NewGuid(), "Math", 10);
            var room = new Room(Guid.NewGuid(), "101", 30, 10);
            
            var t1 = new Teacher(Guid.NewGuid(), "T1", 10) { TargetHours = 2 };
            t1.Competences.Add(new TeacherCompetence { DisciplineId = math.Id });
            // Only allowed in the 4th slot of the day (index 3)
            t1.Constraints.Add(new SpecificTimeConstraint(new[] { 3 }, 5));
            
            var g1 = new StudentGroup(Guid.NewGuid(), "G1", 10) { StudentsCount = 20 };
            g1.Requirements.Add(new GroupRequirement { DisciplineId = math.Id, RequiredHours = 2 });

            schedule.Disciplines.Add(math);
            schedule.Rooms.Add(room);
            schedule.Teachers.Add(t1);
            schedule.Groups.Add(g1);

            schedule.Generate();

            var lessons = schedule.Grid.SelectMany(c => c).ToList();
            Assert.Equal(2, lessons.Count);
            Assert.All(lessons, l => Assert.Equal(3, l.CellIndex % 5)); // Cell in day must be 3
        }

        [Fact]
        public void ConstraintCombinations_ConflictingConstraints_Throws()
        {
            var schedule = CreateEmptySchedule(2, 5); 
            var math = new Discipline(Guid.NewGuid(), "Math", 10);
            var room = new Room(Guid.NewGuid(), "101", 30, 10);
            
            var t1 = new Teacher(Guid.NewGuid(), "T1", 10) { TargetHours = 1 };
            t1.Competences.Add(new TeacherCompetence { DisciplineId = math.Id });
            t1.Constraints.Add(new SpecificDayConstraint(new[] { 0 }, 5)); // Wants Day 0
            
            var g1 = new StudentGroup(Guid.NewGuid(), "G1", 10) { StudentsCount = 20 };
            g1.Requirements.Add(new GroupRequirement { DisciplineId = math.Id, RequiredHours = 1 });
            g1.Constraints.Add(new SpecificDayConstraint(new[] { 1 }, 5)); // Wants Day 1

            schedule.Disciplines.Add(math);
            schedule.Rooms.Add(room);
            schedule.Teachers.Add(t1);
            schedule.Groups.Add(g1);

            // Group and Teacher constraints completely conflict, no common slot possible.
            schedule.Generate();
            Assert.Empty(schedule.Grid.SelectMany(c => c)); // Schedule should be empty
        }

        [Fact]
        public void DisciplineConstraint_AppliesToAllGroupsAndTeachers()
        {
            var schedule = CreateEmptySchedule(2, 2); 
            var math = new Discipline(Guid.NewGuid(), "Math", 4);
            math.Constraints.Add(new SpecificDayConstraint(new[] { 1 }, 2)); // Math ONLY on Day 1
            var room = new Room(Guid.NewGuid(), "101", 30, 4);
            
            var t1 = new Teacher(Guid.NewGuid(), "T1", 4) { TargetHours = 1 };
            t1.Competences.Add(new TeacherCompetence { DisciplineId = math.Id });
            
            var g1 = new StudentGroup(Guid.NewGuid(), "G1", 4) { StudentsCount = 20 };
            g1.Requirements.Add(new GroupRequirement { DisciplineId = math.Id, RequiredHours = 1 });

            schedule.Disciplines.Add(math);
            schedule.Rooms.Add(room);
            schedule.Teachers.Add(t1);
            schedule.Groups.Add(g1);

            schedule.Generate();

            var lessons = schedule.Grid.SelectMany(c => c).ToList();
            Assert.Single(lessons);
            Assert.Equal(1, lessons[0].CellIndex / 2); // Must be Day 1
        }

        [Fact]
        public void DisciplineConstraint_DailyLimit_LimitsLessonsPerGroup()
        {
            // 2 дні, по 3 слоти на день = 6 слотів
            var schedule = CreateEmptySchedule(2, 3); 
            
            var math = new Discipline(Guid.NewGuid(), "Math", 6);
            // Додаємо ліміт: не більше 2 уроків математики на день (для однієї групи)
            math.Constraints.Add(new DailyLimitConstraint(math, 2, 3));
            
            var room1 = new Room(Guid.NewGuid(), "101", 30, 6);
            var room2 = new Room(Guid.NewGuid(), "102", 30, 6);
            
            var t1 = new Teacher(Guid.NewGuid(), "T1", 6) { TargetHours = 10 };
            t1.Competences.Add(new TeacherCompetence { DisciplineId = math.Id });
            var t2 = new Teacher(Guid.NewGuid(), "T2", 6) { TargetHours = 10 };
            t2.Competences.Add(new TeacherCompetence { DisciplineId = math.Id });
            
            // Група 1 хоче 3 математики. Через ліміт 2/день, вони мають розбитися на 2 дні.
            var g1 = new StudentGroup(Guid.NewGuid(), "G1", 6) { StudentsCount = 20 };
            g1.Requirements.Add(new GroupRequirement { DisciplineId = math.Id, RequiredHours = 3 });
            
            // Група 2 також хоче 3 математики.
            var g2 = new StudentGroup(Guid.NewGuid(), "G2", 6) { StudentsCount = 20 };
            g2.Requirements.Add(new GroupRequirement { DisciplineId = math.Id, RequiredHours = 3 });

            schedule.Disciplines.Add(math);
            schedule.Rooms.Add(room1);
            schedule.Rooms.Add(room2);
            schedule.Teachers.Add(t1);
            schedule.Teachers.Add(t2);
            schedule.Groups.Add(g1);
            schedule.Groups.Add(g2);

            schedule.Generate();

            var lessons = schedule.Grid.SelectMany(c => c).ToList();
            
            // Загалом має бути 6 уроків
            Assert.Equal(6, lessons.Count);

            // Перевіряємо Групу 1: не більше 2 уроків математики в День 0 (слоти 0,1,2) і День 1 (слоти 3,4,5)
            var g1Lessons = lessons.Where(l => l.StudentGroupId == g1.Id).ToList();
            Assert.Equal(3, g1Lessons.Count);
            int g1Day0 = g1Lessons.Count(l => l.CellIndex < 3);
            int g1Day1 = g1Lessons.Count(l => l.CellIndex >= 3);
            Assert.True(g1Day0 <= 2);
            Assert.True(g1Day1 <= 2);

            // Перевіряємо Групу 2: аналогічно
            var g2Lessons = lessons.Where(l => l.StudentGroupId == g2.Id).ToList();
            Assert.Equal(3, g2Lessons.Count);
            int g2Day0 = g2Lessons.Count(l => l.CellIndex < 3);
            int g2Day1 = g2Lessons.Count(l => l.CellIndex >= 3);
            Assert.True(g2Day0 <= 2);
            Assert.True(g2Day1 <= 2);
        }
    }
}
