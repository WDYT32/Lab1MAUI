using Lab1MAUI.Models.Schedule;
using Lab1MAUI.Models.Schedule.Constraints;
using Lab1MAUI.Models.Schedule.Participants;
using System;
using System.Linq;
using Xunit;

namespace Lab1MAUI.Tests
{
    public class CombinatorialScheduleTests
    {
        // -------------------------------------------------------------------
        // Helpers for fast combinatorial setup
        // -------------------------------------------------------------------
        private Schedule Create(int days, int cellsPerDay) => new Schedule(new ScheduleConfig { Days = days, CellsPerDay = cellsPerDay });
        private Discipline AddD(Schedule s, string n) { var d = new Discipline(Guid.NewGuid(), n, s.Config.TotalCells); s.Disciplines.Add(d); return d; }
        private Room AddR(Schedule s, string n, int cap) { var r = new Room(Guid.NewGuid(), n, cap, s.Config.TotalCells); s.Rooms.Add(r); return r; }
        
        private Teacher AddT(Schedule s, string n, Discipline d, int target=10) 
        { 
            var t = new Teacher(Guid.NewGuid(), n, s.Config.TotalCells) { TargetHours = target }; 
            t.Competences.Add(new TeacherCompetence { DisciplineId=d.Id }); 
            s.Teachers.Add(t); 
            return t; 
        }
        
        private StudentGroup AddG(Schedule s, string n, int count, Discipline d, int req) 
        { 
            var g = new StudentGroup(Guid.NewGuid(), n, s.Config.TotalCells) { StudentsCount = count }; 
            g.Requirements.Add(new GroupRequirement { DisciplineId=d.Id, RequiredHours=req }); 
            s.Groups.Add(g); 
            return g; 
        }

        // ===================================================================
        // ГРУПА 1: Cross-Entity Конфлікти (Викладач проти Групи проти Дисципліни)
        // ===================================================================

        [Fact]
        public void CrossEntity_TeacherSpecificDay_Vs_GroupSpecificDay_Disjoint_YieldsEmpty()
        {
            var s = Create(3, 2); // 3 дні
            var math = AddD(s, "Math");
            var r = AddR(s, "101", 30);
            
            var t = AddT(s, "T1", math);
            t.Constraints.Add(new SpecificDayConstraint(new[] { 0 }, 2)); // Викладач тільки в День 0

            var g = AddG(s, "G1", 20, math, 2);
            g.Constraints.Add(new SpecificDayConstraint(new[] { 1 }, 2)); // Група тільки в День 1

            s.Generate();
            Assert.Empty(s.Grid.SelectMany(c => c));
        }

        [Fact]
        public void CrossEntity_TeacherTimeOff_Vs_GroupSpecificTime_Checkerboard_YieldsEmpty()
        {
            var s = Create(1, 4); // 4 слоти
            var math = AddD(s, "Math");
            AddR(s, "101", 30);
            
            var t = AddT(s, "T1", math);
            t.Constraints.Add(new TimeOffConstraint(new[] { 0, 2 })); // Викладач не може на парних

            var g = AddG(s, "G1", 20, math, 2);
            g.Constraints.Add(new SpecificTimeConstraint(new[] { 0, 2 }, 4)); // Група МОЖЕ ТІЛЬКИ на парних

            s.Generate();
            Assert.Empty(s.Grid.SelectMany(c => c));
        }

        [Fact]
        public void CrossEntity_TeacherDailyLimit_Vs_GroupSpecificDay_Bottleneck()
        {
            var s = Create(2, 4); // 8 слотів
            var math = AddD(s, "Math");
            AddR(s, "101", 30);

            var t = AddT(s, "T1", math);
            t.Constraints.Add(new DailyLimitConstraint(t, 2, 4)); // Викладач макс 2 на день

            var g = AddG(s, "G1", 20, math, 4);
            g.Constraints.Add(new SpecificDayConstraint(new[] { 0 }, 4)); // Група хоче всі 4 уроки в День 0

            s.Generate();
            
            var lessons = s.Grid.SelectMany(c => c).ToList();
            Assert.Equal(2, lessons.Count); // Через ліміт викладача вміститься тільки 2
            Assert.All(lessons, l => Assert.True(l.CellIndex < 4)); // Тільки День 0
        }

        [Fact]
        public void CrossEntity_DisciplineDailyLimit_Vs_GroupDailyLimit_MinWins()
        {
            var s = Create(2, 4); // 8 слотів
            var math = AddD(s, "Math");
            math.Constraints.Add(new DailyLimitConstraint(math, 1, 4)); // Дисципліна макс 1 на день
            AddR(s, "101", 30);

            var t = AddT(s, "T1", math);
            
            var g = AddG(s, "G1", 20, math, 4);
            g.Constraints.Add(new DailyLimitConstraint(g, 2, 4)); // Група макс 2 на день (переможе жорсткіший ліміт = 1)

            s.Generate();
            
            var lessons = s.Grid.SelectMany(c => c).ToList();
            Assert.Equal(2, lessons.Count); // По 1 уроку в кожен з 2-х днів
        }

        // ===================================================================
        // ГРУПА 2: Взаємодія з ресурсами (Rooms, Teachers)
        // ===================================================================

        [Fact]
        public void Resources_RoomsMixedCapacities_AlgorithmTriesAllCombinations()
        {
            var s = Create(1, 1); // Тільки 1 слот на весь тиждень!
            var math = AddD(s, "Math");
            
            // Є дві аудиторії: на 15 місць і на 30 місць
            AddR(s, "SmallRoom", 15);
            AddR(s, "BigRoom", 30);

            AddT(s, "T1", math);
            AddT(s, "T2", math);

            // Група на 30 людей і група на 10 людей
            var bigGroup = AddG(s, "BigGroup", 30, math, 1);
            var smallGroup = AddG(s, "SmallGroup", 10, math, 1);

            // Алгоритм має здогадатись поставити BigGroup в BigRoom, а SmallGroup в SmallRoom.
            // Якщо він спочатку спробує SmallGroup в BigRoom, то BigGroup не влізе в SmallRoom, і він має зробити Backtrack.
            s.Generate();

            var lessons = s.Grid[0].ToList();
            Assert.Equal(2, lessons.Count); // Обидва розміщено! Backtrack працює ідеально.
        }

        [Fact]
        public void Resources_TeacherTargetHours_CreatesBottleneck()
        {
            var s = Create(1, 5);
            var math = AddD(s, "Math");
            AddR(s, "101", 30);

            // Вчитель згоден викладати тільки 2 години
            AddT(s, "LazyTeacher", math, target: 2);

            // Групі треба 4 години
            AddG(s, "G1", 20, math, 4);

            s.Generate();
            Assert.Equal(2, s.Grid.SelectMany(c => c).Count()); // Згенерується тільки 2
        }

        [Fact]
        public void Resources_MultipleDisciplines_SameTeacherDailyLimit()
        {
            var s = Create(2, 4);
            var math = AddD(s, "Math");
            var phys = AddD(s, "Physics");
            AddR(s, "101", 30);

            var t = new Teacher(Guid.NewGuid(), "SuperGenius", 8) { TargetHours = 10 };
            t.Competences.Add(new TeacherCompetence { DisciplineId = math.Id });
            t.Competences.Add(new TeacherCompetence { DisciplineId = phys.Id });
            t.Constraints.Add(new DailyLimitConstraint(t, 2, 4)); // МАКСИМУМ 2 БУДЬ-ЯКИХ уроки на день!
            s.Teachers.Add(t);

            var g = new StudentGroup(Guid.NewGuid(), "G1", 8) { StudentsCount = 20 };
            g.Requirements.Add(new GroupRequirement { DisciplineId = math.Id, RequiredHours = 2 });
            g.Requirements.Add(new GroupRequirement { DisciplineId = phys.Id, RequiredHours = 2 });
            s.Groups.Add(g);

            s.Generate();
            
            var lessons = s.Grid.SelectMany(c => c).ToList();
            Assert.Equal(4, lessons.Count); // 4 уроки розкинуться на 2 дні рівномірно (по 2 в день)
            
            int day0 = lessons.Count(l => l.CellIndex < 4);
            int day1 = lessons.Count(l => l.CellIndex >= 4);
            Assert.Equal(2, day0);
            Assert.Equal(2, day1);
        }

        // ===================================================================
        // ГРУПА 3: Складні внутрішні обмеження
        // ===================================================================

        [Fact]
        public void Complex_SpecificTime_Vs_DailyLimit_PartialPlacement()
        {
            var s = Create(2, 4); // 8 слотів
            var math = AddD(s, "Math");
            AddR(s, "101", 30);
            AddT(s, "T1", math);

            var g = AddG(s, "G1", 20, math, 4);
            // Група МОЖЕ тільки в слоти 0, 1, 2 кожного дня
            g.Constraints.Add(new SpecificTimeConstraint(new[] { 0, 1, 2 }, 4));
            // Група МОЖЕ тільки в День 0
            g.Constraints.Add(new SpecificDayConstraint(new[] { 0 }, 4));
            // АЛЕ Група не може більше 2 уроків на день
            g.Constraints.Add(new DailyLimitConstraint(g, 2, 4));

            s.Generate();

            var lessons = s.Grid.SelectMany(c => c).ToList();
            Assert.Equal(2, lessons.Count); // Хоча SpecificTime дає 3 слоти, DailyLimit обрізає до 2
        }

        [Fact]
        public void Complex_MaxConsecutive_Forces_DaysToBeSkipped()
        {
            var s = Create(3, 2); // 3 дні по 2 слоти
            var math = AddD(s, "Math");
            AddR(s, "101", 30);
            AddT(s, "T1", math);

            var g = AddG(s, "G1", 20, math, 3);
            // Якщо MaxConsecutive = 1, то два підряд не можна. 
            // Оскільки в дні всього 2 слоти, на день може бути максимум 1 урок!
            g.Constraints.Add(new MaxConsecutiveLessonsConstraint(g, 1, 2));

            s.Generate();

            var lessons = s.Grid.SelectMany(c => c).OrderBy(l => l.CellIndex).ToList();
            Assert.Equal(3, lessons.Count);
            
            // Вони мають розміститись у різні дні! (один урок на кожен день)
            Assert.Equal(0, lessons[0].CellIndex / 2);
            Assert.Equal(1, lessons[1].CellIndex / 2);
            Assert.Equal(2, lessons[2].CellIndex / 2);
        }

        [Fact]
        public void Complex_SpecificDay_OutOfBounds_YieldsEmpty()
        {
            var s = Create(3, 3); // 3 дні
            var math = AddD(s, "Math");
            AddR(s, "101", 30);
            AddT(s, "T1", math);

            var g = AddG(s, "G1", 20, math, 2);
            // Група вимагає День 5, якого не існує (існують 0, 1, 2)
            g.Constraints.Add(new SpecificDayConstraint(new[] { 5 }, 3));

            s.Generate();
            Assert.Empty(s.Grid.SelectMany(c => c));
        }

        [Fact]
        public void Complex_AllConstraints_Simultaneously()
        {
            var s = Create(4, 4); // 4 дні, 4 слоти
            var math = AddD(s, "Math");
            AddR(s, "101", 30);
            
            var t = AddT(s, "T1", math, target: 4); // Target обмеження
            t.Constraints.Add(new MaxConsecutiveLessonsConstraint(t, 1, 4)); // MaxConsecutive
            t.Constraints.Add(new TimeOffConstraint(new[] { 0, 4, 8, 12 })); // TimeOff (ранки заблоковані)

            var g = AddG(s, "G1", 20, math, 8); // Хоче аж 8 уроків
            g.Constraints.Add(new SpecificDayConstraint(new[] { 0, 1, 2 }, 4)); // Тільки перші 3 дні
            g.Constraints.Add(new DailyLimitConstraint(g, 2, 4)); // Не більше 2 на день

            // Аналіз:
            // Всього доступно 3 дні. В кожному дні по 4 слоти. 
            // Ранки (0 слот) заблоковані. Доступні слоти 1, 2, 3 кожного дня.
            // DailyLimit = 2 -> 3 дні * 2 = 6 уроків макс.
            // TargetHours вчителя = 4 -> макс 4 уроки в принципі.
            // Отже, має бути розміщено РІВНО 4 уроки. І жодні два не стоять підряд!

            s.Generate();

            var lessons = s.Grid.SelectMany(c => c).ToList();
            Assert.Equal(4, lessons.Count); // TargetHours викладача спрацював як остаточний обмежувач
        }

        // ===================================================================
        // ГРУПА 4: Екстремальні значення (Нулі)
        // ===================================================================

        [Fact]
        public void Extreme_MaxConsecutive_Zero_BlocksAll()
        {
            var s = Create(2, 2);
            var math = AddD(s, "Math");
            AddR(s, "101", 30);
            AddT(s, "T1", math);

            var g = AddG(s, "G1", 20, math, 2);
            // 0 підряд означає взагалі не можна ставити уроки
            g.Constraints.Add(new MaxConsecutiveLessonsConstraint(g, 0, 2));

            s.Generate();
            Assert.Empty(s.Grid.SelectMany(c => c));
        }

        [Fact]
        public void Extreme_DailyLimit_Zero_BlocksAll()
        {
            var s = Create(2, 2);
            var math = AddD(s, "Math");
            AddR(s, "101", 30);
            AddT(s, "T1", math);

            var g = AddG(s, "G1", 20, math, 2);
            g.Constraints.Add(new DailyLimitConstraint(g, 0, 2));

            s.Generate();
            Assert.Empty(s.Grid.SelectMany(c => c));
        }

        [Fact]
        public void Extreme_SpecificDay_EmptyArray_YieldsEmpty()
        {
            var s = Create(2, 2);
            var math = AddD(s, "Math");
            AddR(s, "101", 30);
            AddT(s, "T1", math);

            var g = AddG(s, "G1", 20, math, 2);
            g.Constraints.Add(new SpecificDayConstraint(Array.Empty<int>(), 2));

            s.Generate();
            Assert.Empty(s.Grid.SelectMany(c => c));
        }
    }
}
