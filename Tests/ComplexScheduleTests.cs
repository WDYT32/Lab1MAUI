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
            // 1 день, 1 слот = всього 1 чарунка часу на весь розклад
            var schedule = CreateSchedule(1, 1);
            var math = new Discipline(Guid.NewGuid(), "Math", 1);
            schedule.Disciplines.Add(math);

            // 3 викладачі і 3 групи (кожна хоче 1 урок)
            for (int i = 0; i < 3; i++)
            {
                var t = new Teacher(Guid.NewGuid(), $"T{i}", 1) { TargetHours = 1 };
                t.Competences.Add(new TeacherCompetence { DisciplineId = math.Id });
                schedule.Teachers.Add(t);

                var g = new StudentGroup(Guid.NewGuid(), $"G{i}", 1) { StudentsCount = 20 };
                g.Requirements.Add(new GroupRequirement { DisciplineId = math.Id, RequiredHours = 1 });
                schedule.Groups.Add(g);
            }

            // АЛЕ ТІЛЬКИ 2 АУДИТОРІЇ!
            schedule.Rooms.Add(new Room(Guid.NewGuid(), "101", 30, 1));
            schedule.Rooms.Add(new Room(Guid.NewGuid(), "102", 30, 1));

            schedule.Generate();

            var placedLessons = schedule.Grid.SelectMany(c => c).ToList();
            
            // В єдиний слот можна поставити лише 2 уроки, бо лише 2 аудиторії
            Assert.Equal(2, placedLessons.Count);
        }

        [Fact]
        public void Combinatorial_MaxConsecutive_And_DailyLimit_CorrectlySpacesLessons()
        {
            // 2 дні, 4 слоти на день (всього 8 слотів)
            // День 0: слоти 0, 1, 2, 3
            // День 1: слоти 4, 5, 6, 7
            var schedule = CreateSchedule(2, 4);
            var physics = new Discipline(Guid.NewGuid(), "Physics", 8);
            
            // Комбінація: не більше 2 уроків фізики на день, і не більше 1 підряд (вікно обов'язкове)
            physics.Constraints.Add(new DailyLimitConstraint(physics, 2, 4));
            physics.Constraints.Add(new MaxConsecutiveLessonsConstraint(physics, 1, 4));
            
            schedule.Disciplines.Add(physics);
            schedule.Rooms.Add(new Room(Guid.NewGuid(), "101", 30, 8));
            
            var t1 = new Teacher(Guid.NewGuid(), "Einstein", 8) { TargetHours = 10 };
            t1.Competences.Add(new TeacherCompetence { DisciplineId = physics.Id });
            schedule.Teachers.Add(t1);

            var g1 = new StudentGroup(Guid.NewGuid(), "G1", 8) { StudentsCount = 20 };
            // Хочуть 4 уроки
            g1.Requirements.Add(new GroupRequirement { DisciplineId = physics.Id, RequiredHours = 4 });
            schedule.Groups.Add(g1);

            schedule.Generate();

            var lessons = schedule.Grid.SelectMany(c => c).OrderBy(l => l.CellIndex).ToList();
            
            // Всі 4 уроки мають бути розміщені успішно
            Assert.Equal(4, lessons.Count);

            // Перевіряємо, що немає жодних уроків підряд
            for (int i = 0; i < lessons.Count - 1; i++)
            {
                int currentCell = lessons[i].CellIndex;
                int nextCell = lessons[i + 1].CellIndex;

                // Якщо уроки в один день, між ними має бути мінімум 1 порожній слот (різниця >= 2)
                if (currentCell / 4 == nextCell / 4)
                {
                    Assert.True(nextCell - currentCell >= 2, $"Lessons are consecutive at slots {currentCell} and {nextCell}");
                }
            }

            // Перевіряємо Daily Limit (не більше 2 в день)
            Assert.True(lessons.Count(l => l.CellIndex < 4) <= 2);
            Assert.True(lessons.Count(l => l.CellIndex >= 4) <= 2);
        }

        [Fact]
        public void Combinatorial_TeacherTimeOff_And_GroupSpecificDay_YieldsPartial()
        {
            // 2 дні, 2 слоти на день
            // День 0: 0, 1. День 1: 2, 3.
            var schedule = CreateSchedule(2, 2);
            var math = new Discipline(Guid.NewGuid(), "Math", 4);
            schedule.Disciplines.Add(math);
            schedule.Rooms.Add(new Room(Guid.NewGuid(), "101", 30, 4));

            var t1 = new Teacher(Guid.NewGuid(), "T1", 4) { TargetHours = 5 };
            t1.Competences.Add(new TeacherCompetence { DisciplineId = math.Id });
            // Викладач не може викладати на слоті 1 (кінець першого дня)
            t1.Constraints.Add(new TimeOffConstraint(new[] { 1 }));
            schedule.Teachers.Add(t1);

            var g1 = new StudentGroup(Guid.NewGuid(), "G1", 4) { StudentsCount = 20 };
            // Група вимагає 2 математики
            g1.Requirements.Add(new GroupRequirement { DisciplineId = math.Id, RequiredHours = 2 });
            // АЛЕ група може мати уроки ТІЛЬКИ в День 0 (слоти 0 і 1)
            g1.Constraints.Add(new SpecificDayConstraint(new[] { 0 }, 2));
            schedule.Groups.Add(g1);

            schedule.Generate();

            var lessons = schedule.Grid.SelectMany(c => c).ToList();

            // Перетин можливих слотів:
            // Група: [0, 1]
            // Викладач: [0, 2, 3]
            // Загальні вільні слоти: тільки [0]
            // Тому буде розміщено лише 1 урок.
            Assert.Single(lessons);
            Assert.Equal(0, lessons[0].CellIndex);
        }

        [Fact]
        public void StressTest_MultipleGroupsTeachersAndRooms_FullySatisfied()
        {
            // 5 днів по 5 слотів = 25 слотів
            var schedule = CreateSchedule(5, 5);

            // 5 дисциплін
            var disciplines = new List<Discipline>();
            for (int i = 0; i < 5; i++)
            {
                var d = new Discipline(Guid.NewGuid(), $"Disc_{i}", 25);
                disciplines.Add(d);
                schedule.Disciplines.Add(d);
            }

            // 10 Аудиторій
            for (int i = 0; i < 10; i++)
            {
                schedule.Rooms.Add(new Room(Guid.NewGuid(), $"Room_{i}", 30, 25));
            }

            // 10 Викладачів (по 2 на кожну дисципліну)
            for (int i = 0; i < 10; i++)
            {
                var t = new Teacher(Guid.NewGuid(), $"T_{i}", 25) { TargetHours = 25 };
                // Кожен викладач веде одну свою дисципліну (0-0, 1-0, 2-1, 3-1, etc)
                t.Competences.Add(new TeacherCompetence { DisciplineId = disciplines[i / 2].Id });
                schedule.Teachers.Add(t);
            }

            // 10 Груп. Кожна хоче по 2 уроки кожної з 5 дисциплін (всього 10 уроків на групу)
            for (int i = 0; i < 10; i++)
            {
                var g = new StudentGroup(Guid.NewGuid(), $"G_{i}", 25) { StudentsCount = 20 };
                foreach (var d in disciplines)
                {
                    g.Requirements.Add(new GroupRequirement { DisciplineId = d.Id, RequiredHours = 2 });
                }
                schedule.Groups.Add(g);
            }

            // Всього потрібно поставити: 10 груп * 10 уроків = 100 уроків.
            // Маємо 25 слотів * 10 аудиторій = 250 місць для уроків.
            // Ресурсів більш ніж достатньо, конфліктів немає.

            schedule.Generate();

            var placedLessons = schedule.Grid.SelectMany(c => c).ToList();
            Assert.Equal(100, placedLessons.Count);

            // Переконаємось, що всі вимоги виконані
            foreach (var g in schedule.Groups)
            {
                foreach (var req in g.Requirements)
                {
                    Assert.Equal(2, req.FulfilledHours);
                }
            }
        }
    }
}
