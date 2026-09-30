using System;
using System.Collections.Generic;
using Lab1MAUI.Models.Schedule;
using Xunit;

namespace Lab1MAUI.Tests
{
    public class FunctionalAlgorithmTests
    {
        [Fact]
        public void ConstraintRegistry_SortsByPriorityDesc_WithConflictResolution()
        {
            var registry = new ConstraintRegistry();
            var c1 = new DummyConstraint(10);
            var c2 = new DummyConstraint(20);
            var c3 = new DummyConstraint(20);

            registry.Register(c1);
            registry.Register(c2);
            registry.Register(c3);

            var ordered = registry.GetOrdered();

            Assert.Equal(3, ordered.Count);
            Assert.Equal(20, ordered[0].Priority);
            Assert.Equal(19, ordered[1].Priority); 
            Assert.Equal(10, ordered[2].Priority);
        }

        [Fact]
        public void InitializeData_AllocatesTimeCells_AndGeneratesTargetHours()
        {
            var schedule = new Schedule
            {
                Config = new ScheduleConfig { Days = 2, CellsPerDay = 5 },
                Teachers = new List<Teacher> { new Teacher() },
                Groups = new List<StudentGroup> { new StudentGroup() }
            };

            schedule.InitializeData();

            Assert.Equal(10, schedule.Teachers[0].Grid.Cells.Count);
            Assert.Equal(10, schedule.Groups[0].Grid.Cells.Count);
            Assert.True(schedule.Teachers[0].Grid.TargetHours >= 10);
            Assert.True(schedule.Groups[0].Grid.TargetHours >= 15);
        }

        [Fact]
        public void LessonPlacementConstraint_FindsFirstCommonFreeCell()
        {
            var config = new ScheduleConfig { Days = 1, CellsPerDay = 3 };
            var t1 = new Teacher();
            var g1 = new StudentGroup();
            t1.Grid.Initialize(3);
            g1.Grid.Initialize(3);
            
            var lesson = new Lesson { TeacherId = t1.Id, StudentGroupId = g1.Id };
            var schedule = new Schedule(config, new List<Teacher> { t1 }, new List<StudentGroup> { g1 }, new List<Lesson> { lesson });

            t1.Grid.Place(Guid.NewGuid(), 0);
            g1.Grid.Place(Guid.NewGuid(), 1);

            var constraint = new LessonPlacementConstraint(lesson);
            constraint.Apply(schedule);

            Assert.True(t1.Grid.IsFree(1));
            Assert.True(g1.Grid.IsFree(0));
            Assert.Equal(lesson.Id, t1.Grid.Cells[2]);
            Assert.Equal(lesson.Id, g1.Grid.Cells[2]);
        }

        [Fact]
        public void ValidateConsistency_ClearsAsymmetricCells()
        {
            var config = new ScheduleConfig { Days = 1, CellsPerDay = 3 };
            var t1 = new Teacher();
            var g1 = new StudentGroup();
            t1.Grid.Initialize(3);
            g1.Grid.Initialize(3);
            
            var lesson = new Lesson { TeacherId = t1.Id, StudentGroupId = g1.Id };
            var schedule = new Schedule(config, new List<Teacher> { t1 }, new List<StudentGroup> { g1 }, new List<Lesson> { lesson });

            t1.Grid.Place(lesson.Id, 1);
            // Група НЕ має цього уроку (асиметрія)

            schedule.ValidateConsistency();

            Assert.True(t1.Grid.IsFree(1)); // Має бути очищено!
        }

        [Fact]
        public void TeacherTimeOffConstraint_BlocksCellSuccessfully()
        {
            var config = new ScheduleConfig { Days = 1, CellsPerDay = 3 };
            var t1 = new Teacher();
            t1.Grid.Initialize(3);
            var schedule = new Schedule(config, new List<Teacher> { t1 }, new List<StudentGroup>(), new List<Lesson>());

            var constraint = new TeacherTimeOffConstraint(t1, 1); 
            constraint.Apply(schedule);

            Assert.False(t1.Grid.IsFree(1)); 
            Assert.Equal(Guid.Empty, t1.Grid.Cells[1]);
        }
        
        [Fact]
        public void MaxConsecutiveLessonsConstraint_PlacesBlocker()
        {
            var config = new ScheduleConfig { Days = 1, CellsPerDay = 5 };
            var t1 = new Teacher();
            t1.Grid.Initialize(5);
            var schedule = new Schedule(config, new List<Teacher> { t1 }, new List<StudentGroup>(), new List<Lesson>());

            // Імітуємо порожню сітку
            var constraint = new MaxConsecutiveLessonsConstraint(t1.Id, 3);
            constraint.Apply(schedule);

            // При ліміті 3, 4-та клітинка (індекс 3) має бути заблокована порожнім уроком (не null)
            Assert.True(t1.Grid.IsFree(0));
            Assert.True(t1.Grid.IsFree(1));
            Assert.True(t1.Grid.IsFree(2));
            Assert.False(t1.Grid.IsFree(3));
            Assert.NotEqual(Guid.Empty, t1.Grid.Cells[3]); 
        }
    }

    public class DummyConstraint : IConstraint
    {
        public int Priority { get; set; }
        public Guid OwnerId { get; } = Guid.NewGuid();

        public DummyConstraint(int priority)
        {
            Priority = priority;
        }

        public void Apply(Schedule schedule) { }
    }
}
