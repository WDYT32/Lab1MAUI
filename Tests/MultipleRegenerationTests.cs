using Lab1MAUI.Models.MTM;
using Lab1MAUI.Models.MTM.Constraints;
using Lab1MAUI.ViewModels;
using System;
using System.Linq;
using Xunit;

namespace Lab1MAUI.Tests
{
    /// <summary>
    /// Тести, що імітують реальну поведінку користувача:
    ///   1. Налаштовує дані через ViewModels (MTM).
    ///   2. Натискає "Генерувати" (TryGenerate).
    ///   3. Редагує щось.
    ///   4. Натискає "Генерувати" знову.
    ///
    /// Ключова інваріанта архітектури MTM:
    ///   Кожна генерація створює *новий* Schedule з нульовим станом.
    ///   Лічильники FulfilledHours, AssignedHours, bookedCells та Grid
    ///   НЕ повинні накопичуватись між запусками.
    /// </summary>
    public class MultipleRegenerationTests
    {
        // -------------------------------------------------------------------
        // Допоміжні методи (будуємо типовий сценарій через MTM ViewModels)
        // -------------------------------------------------------------------

        /// <summary>Мінімальна сітка: 1 день, 5 слотів.</summary>
        private static ScheduleMTM CreateBaseMTM(int days = 1, int cellsPerDay = 5)
        {
            var mtm = new ScheduleMTM();
            mtm.Config.Days = days;
            mtm.Config.CellsPerDay = cellsPerDay;
            return mtm;
        }

        private static DisciplineMTM AddDiscipline(ScheduleMTM mtm, string name)
        {
            var d = new DisciplineMTM { Id = Guid.NewGuid(), Name = name };
            mtm.Disciplines.Add(d);
            return d;
        }

        private static TeacherMTM AddTeacher(ScheduleMTM mtm, string name, int targetHours, params DisciplineMTM[] competences)
        {
            var t = new TeacherMTM { Id = Guid.NewGuid(), Name = name, TargetHours = targetHours };
            foreach (var d in competences)
                t.Competences.Add(new TeacherCompetenceMTM { DisciplineId = d.Id });
            mtm.Teachers.Add(t);
            return t;
        }

        private static RoomMTM AddRoom(ScheduleMTM mtm, string name, int capacity = 30)
        {
            var r = new RoomMTM { Id = Guid.NewGuid(), Name = name, Capacity = capacity };
            mtm.Rooms.Add(r);
            return r;
        }

        private static StudentGroupMTM AddGroup(ScheduleMTM mtm, string name, int studentsCount,
            params (DisciplineMTM disc, int hours)[] requirements)
        {
            var g = new StudentGroupMTM { Id = Guid.NewGuid(), Name = name, StudentsCount = studentsCount };
            foreach (var (disc, hours) in requirements)
                g.Requirements.Add(new GroupRequirementMTM { DisciplineId = disc.Id, RequiredHours = hours });
            mtm.Groups.Add(g);
            return g;
        }

        // ===================================================================
        // TEST 1 — Основний: стан повністю скидається між генераціями
        // ===================================================================

        [Fact]
        public void Regeneration_DomainStateIsClean_FulfilledHoursAndAssignedHoursDoNotAccumulate()
        {
            // Arrange: 1 дисципліна, 1 вчитель, 1 аудиторія, 1 група (2 год)
            var mtm = CreateBaseMTM();
            var math = AddDiscipline(mtm, "Math");
            var teacher = AddTeacher(mtm, "Smith", targetHours: 5, math);
            AddRoom(mtm, "101");
            var group = AddGroup(mtm, "G1", 25, (math, 2));

            // Act: перша генерація
            bool ok1 = mtm.TryGenerate(out _);
            var result1 = mtm.LastResult;

            Assert.True(ok1, "Перша генерація повинна бути успішною");
            var placed1 = result1.Grid.SelectMany(c => c).ToList();
            Assert.Equal(2, placed1.Count);

            // Перевіряємо домен першого запуску
            var domainGroup1  = result1.Groups.First(g => g.Id == group.Id);
            var domainTeach1  = result1.Teachers.First(t => t.Id == teacher.Id);
            Assert.Equal(2, domainGroup1.Requirements.First().FulfilledHours); // 2 з 2 виконано
            Assert.Equal(2, domainTeach1.AssignedHours);                       // 2 уроки призначено

            // Act: змінюємо вимогу і генеруємо ВДРУГЕ (без жодного явного скидання!)
            group.Requirements[0].RequiredHours = 3;

            bool ok2 = mtm.TryGenerate(out _);
            var result2 = mtm.LastResult;

            Assert.True(ok2, "Друга генерація повинна бути успішною");
            Assert.NotSame(result1, result2); // Обов'язково новий екземпляр

            var placed2 = result2.Grid.SelectMany(c => c).ToList();

            // Якби стан не скидався: FulfilledHours = 2 (зі старого запуску),
            // тоді алгоритм думав би що залишилось 3-2=1 урок і розмістив би лише 1.
            // Правильна поведінка: 3 нових урока.
            Assert.Equal(3, placed2.Count);

            var domainGroup2  = result2.Groups.First(g => g.Id == group.Id);
            var domainTeach2  = result2.Teachers.First(t => t.Id == teacher.Id);
            Assert.Equal(3, domainGroup2.Requirements.First().FulfilledHours); // рівно 3, не 2+3=5
            Assert.Equal(3, domainTeach2.AssignedHours);                       // рівно 3, не 2+3=5
        }

        // ===================================================================
        // TEST 2 — Заміна вчителя між генераціями
        // ===================================================================

        [Fact]
        public void Regeneration_TeacherReplaced_NewLessonsUseOnlyNewTeacher()
        {
            // Arrange
            var mtm = CreateBaseMTM();
            var math = AddDiscipline(mtm, "Math");
            var oldTeacher = AddTeacher(mtm, "OldTeacher", targetHours: 5, math);
            AddRoom(mtm, "101");
            AddGroup(mtm, "G1", 25, (math, 2));

            // Act: перша генерація зі старим вчителем
            Assert.True(mtm.TryGenerate(out _));
            var r1Lessons = mtm.LastResult.Grid.SelectMany(c => c).ToList();
            Assert.All(r1Lessons, l => Assert.Equal(oldTeacher.Id, l.TeacherId));

            // Користувач видаляє старого вчителя і додає нового
            mtm.Teachers.Remove(oldTeacher);
            var newTeacher = AddTeacher(mtm, "NewTeacher", targetHours: 5, math);

            // Act: друга генерація
            Assert.True(mtm.TryGenerate(out _));
            var r2Lessons = mtm.LastResult.Grid.SelectMany(c => c).ToList();

            Assert.Equal(2, r2Lessons.Count);
            // Всі уроки — лише нового вчителя
            Assert.All(r2Lessons, l => Assert.Equal(newTeacher.Id, l.TeacherId));
            // Старий вчитель більше не присутній
            Assert.DoesNotContain(r2Lessons, l => l.TeacherId == oldTeacher.Id);
        }

        // ===================================================================
        // TEST 3 — Додавання обмеження між генераціями через ConstraintFormViewModel
        // ===================================================================

        [Fact]
        public void Regeneration_TimeOffConstraintAdded_LessonsAvoidBlockedSlot()
        {
            // Arrange: 1 слот в день, 3 дні → слоти 0, 1, 2
            var mtm = CreateBaseMTM(days: 3, cellsPerDay: 1);
            var math = AddDiscipline(mtm, "Math");
            var teacher = AddTeacher(mtm, "Smith", targetHours: 5, math);
            AddRoom(mtm, "101");
            AddGroup(mtm, "G1", 25, (math, 2));

            // Перша генерація: без обмежень → слоти 0 і 1
            Assert.True(mtm.TryGenerate(out _));
            var slots1 = mtm.LastResult.Grid.SelectMany(c => c).Select(l => l.CellIndex).OrderBy(x => x).ToList();
            Assert.Equal(2, slots1.Count);
            // Слот 0 точно зайнятий
            Assert.Contains(0, slots1);

            // Користувач додає TimeOff обмеження на вчителя (через ConstraintFormViewModel)
            var constraintForm = new ConstraintFormViewModel(ConstraintOwnerType.Teacher, mtm.Config);
            constraintForm.SelectedConstraintType = "Вихідний час (TimeOff)";
            constraintForm.TimeOffCellIndex = 0; // забороняємо слот 0

            ConstraintMTM addedConstraint = null;
            constraintForm.OnConstraintAdded += c => addedConstraint = c;
            constraintForm.AddConstraintCommand.Execute(null);

            Assert.NotNull(addedConstraint);
            // Зберігаємо обмеження у MTM вчителя
            teacher.Constraints.Add(addedConstraint);

            // Друга генерація: слот 0 заблокований для вчителя → уроки у слотах 1 і 2
            Assert.True(mtm.TryGenerate(out _));
            var slots2 = mtm.LastResult.Grid.SelectMany(c => c).Select(l => l.CellIndex).OrderBy(x => x).ToList();

            Assert.Equal(2, slots2.Count);
            Assert.DoesNotContain(0, slots2); // слот 0 більше не використовується
            Assert.Equal([1, 2], slots2);
        }

        // ===================================================================
        // TEST 4 — Зміна кількості груп між генераціями
        // ===================================================================

        [Fact]
        public void Regeneration_GroupAdded_NewScheduleIncludesBothGroups()
        {
            // Arrange: 1 день, 4 слоти, 2 вчителі (щоб вмістити 2 групи в 1 слоті)
            var mtm = CreateBaseMTM(days: 1, cellsPerDay: 4);
            var math = AddDiscipline(mtm, "Math");
            AddTeacher(mtm, "T1", targetHours: 5, math);
            AddTeacher(mtm, "T2", targetHours: 5, math);
            AddRoom(mtm, "R1");
            AddRoom(mtm, "R2");
            AddGroup(mtm, "G1", 25, (math, 2));

            // Перша генерація: 1 група → 2 уроки
            Assert.True(mtm.TryGenerate(out _));
            Assert.Equal(2, mtm.LastResult.Grid.SelectMany(c => c).Count());

            // Користувач додає другу групу
            AddGroup(mtm, "G2", 20, (math, 2));

            // Друга генерація: 2 групи → 4 уроки
            Assert.True(mtm.TryGenerate(out _));
            var lessons2 = mtm.LastResult.Grid.SelectMany(c => c).ToList();

            Assert.Equal(4, lessons2.Count); // не 2+4=6, а рівно 4
            Assert.Equal(2, lessons2.Select(l => l.StudentGroupId).Distinct().Count()); // обидві групи
        }

        // ===================================================================
        // TEST 5 — ScheduleResultViewModel коректно показує нерозподілені уроки
        //          після повторної (невдалої) генерації
        // ===================================================================

        [Fact]
        public void Regeneration_AfterFailure_ScheduleResultViewModelShowsCorrectUnassigned()
        {
            // Arrange: спочатку успішна генерація (2 год)
            var mtm = CreateBaseMTM(days: 1, cellsPerDay: 3);
            var math = AddDiscipline(mtm, "Math");
            var teacher = AddTeacher(mtm, "Smith", targetHours: 5, math);
            AddRoom(mtm, "101");
            var group = AddGroup(mtm, "G1", 25, (math, 2));

            Assert.True(mtm.TryGenerate(out _));

            // Перша перевірка через ScheduleResultViewModel: 0 нерозподілених
            var vm1 = new ScheduleResultViewModel(mtm);
            Assert.Equal(0, vm1.UnassignedLessons.Count);
            Assert.Equal(2, vm1.GroupSchedules.First().Days.SelectMany(d => d.Lessons).Count());

            // Крок 3: Користувач ставить вимогу більшу, ніж є слотів (10 > 3 слотів)
            group.Requirements[0].RequiredHours = 10;

            bool ok = mtm.TryGenerate(out _);
            Assert.False(ok); // Очікуємо невдачу, бо повністю згенерувати неможливо

            // Крок 4: Перевірка через ScheduleResultViewModel
            var vm2 = new ScheduleResultViewModel(mtm);
            Assert.True(vm2.HasUnassignedLessons);

            var unassigned = vm2.UnassignedLessons.First();
            Assert.Equal("G1", unassigned.GroupName);
            Assert.Equal("Math", unassigned.SubjectName);
            // Розміщено 3 з 10 можливих. Нерозподілених має бути 7 (а не 10, бо тепер є підтримка часткового розкладу)
            Assert.Equal(7, unassigned.Hours);
        }

        // ===================================================================
        // TEST 6 — Повний користувацький сценарій (end-to-end симуляція)
        //          Редагування → Генерація → Редагування → Генерація × 3
        // ===================================================================

        [Fact]
        public void FullUserWorkflow_ThreeRoundsOfEditAndGenerate_EachResultIsIndependent()
        {
            var mtm = CreateBaseMTM(days: 2, cellsPerDay: 4); // 8 слотів загалом
            var math    = AddDiscipline(mtm, "Math");
            var physics = AddDiscipline(mtm, "Physics");
            var teacher = AddTeacher(mtm, "Prof", targetHours: 10, math, physics);
            AddRoom(mtm, "101", capacity: 30);
            var group = AddGroup(mtm, "G1", 25, (math, 2));

            // === Раунд 1: базовий розклад ===
            Assert.True(mtm.TryGenerate(out _));
            var r1 = mtm.LastResult;
            Assert.Equal(2, r1.Grid.SelectMany(c => c).Count());

            // === Раунд 2: додаємо вимогу з фізики ===
            group.Requirements.Add(new GroupRequirementMTM { DisciplineId = physics.Id, RequiredHours = 3 });

            Assert.True(mtm.TryGenerate(out _));
            var r2 = mtm.LastResult;

            Assert.NotSame(r1, r2);
            Assert.Equal(5, r2.Grid.SelectMany(c => c).Count()); // 2 + 3 = 5

            // Перевіряємо що домен r1 НЕ змінився (він є окремим frozen snapshot)
            Assert.Equal(2, r1.Grid.SelectMany(c => c).Count());

            // === Раунд 3: збільшуємо вимогу з математики, додаємо DailyLimit обмеження ===
            group.Requirements.First(r => r.DisciplineId == math.Id).RequiredHours = 4;

            // Додаємо обмеження "не більше 3 уроків на день" на вчителя.
            // DailyLimit(3) × 2 дні = макс 6 слотів, але потрібно 4+3=7 → генерація ПРОВАЛИТЬСЯ.
            teacher.Constraints.Add(new DailyLimitConstraintMTM { MaxLessonsPerDay = 3 });

            bool ok3 = mtm.TryGenerate(out string err3);
            var r3 = mtm.LastResult;

            Assert.False(ok3, "Раунд 3 повинен провалитися: 7 уроків не вміщаються при ліміті 3/день × 2 дні");
            Assert.NotEmpty(err3);

            // Попередні snapshots НЕ змінились — вони є frozen об'єктами
            Assert.Equal(5, r2.Grid.SelectMany(c => c).Count()); // r2 незмінний
            Assert.Equal(2, r1.Grid.SelectMany(c => c).Count()); // r1 незмінний

            // r3 може бути порожнім (backtracking відкатив все)
            // Але він точно не є r1 або r2
            Assert.NotSame(r1, r3);
            Assert.NotSame(r2, r3);

            // ScheduleResultViewModel показує нерозподілені уроки для r3
            var vm = new ScheduleResultViewModel(mtm);
            int totalRequired   = group.Requirements.Sum(r => r.RequiredHours); // 4 + 3 = 7
            int totalPlaced     = r3.Grid.SelectMany(c => c).Count();
            int totalUnassigned = totalRequired - totalPlaced;

            Assert.True(totalUnassigned > 0, "Після провальної генерації мають бути нерозподілені уроки");
            Assert.Equal(totalUnassigned, vm.UnassignedLessons.Sum(u => u.Hours));
        }
    }
}
