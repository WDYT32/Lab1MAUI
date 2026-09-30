using System;
using System.Collections.Generic;
using Lab1MAUI.Models.Schedule;
using Xunit;

namespace Lab1MAUI.Tests
{
    public class EdgeCaseTests
    {
        [Fact]
        public void DailyLimitConstraint_WithZeroLimit_BlocksAllCells()
        {
            var config = new ScheduleConfig { Days = 2, CellsPerDay = 3 };
            var group = new StudentGroup();
            group.Grid.Initialize(6);
            var schedule = new Schedule(config, new List<Teacher>(), new List<StudentGroup> { group }, new List<Lesson>());

            var constraint = new DailyLimitConstraint(group.Id, 0);
            constraint.Apply(schedule);

            // Всі клітинки мають бути заблоковані
            for (int i = 0; i < 6; i++)
            {
                Assert.False(group.Grid.IsFree(i));
                Assert.NotEqual(Guid.Empty, group.Grid.Cells[i]);
            }
        }

        [Fact]
        public void MaxConsecutiveLessonsConstraint_WithLimitOne_BlocksAlternatingCells()
        {
            var config = new ScheduleConfig { Days = 1, CellsPerDay = 4 };
            var t1 = new Teacher();
            t1.Grid.Initialize(4);
            var schedule = new Schedule(config, new List<Teacher> { t1 }, new List<StudentGroup>(), new List<Lesson>());

            var constraint = new MaxConsecutiveLessonsConstraint(t1.Id, 1);
            constraint.Apply(schedule);

            // Клітинки мають бути: 0 вільна, 1 заблокована, 2 вільна, 3 заблокована
            Assert.True(t1.Grid.IsFree(0));
            Assert.False(t1.Grid.IsFree(1));
            Assert.True(t1.Grid.IsFree(2));
            Assert.False(t1.Grid.IsFree(3));
        }

        [Fact]
        public void TeacherAssignmentConstraint_NoCapableTeacher_LeavesGuidEmpty()
        {
            var schedule = new Schedule();
            var group = new StudentGroup();
            schedule.Groups.Add(group);
            
            var lesson = new Lesson 
            { 
                StudentGroupId = group.Id, 
                TeacherId = Guid.Empty,
                DisciplineId = Guid.NewGuid(), // Дисципліна, яку ніхто не знає
                Type = LessonType.Lecture
            };
            schedule.Lessons.Add(lesson);

            var constraint = new TeacherAssignmentConstraint(group);
            constraint.Apply(schedule);

            // Викладач не має бути знайдений, ID залишається Empty
            Assert.Equal(Guid.Empty, lesson.TeacherId);
        }

        [Fact]
        public void ValidateConsistency_MultipleConflicts_HandledCorrectly()
        {
            var config = new ScheduleConfig { Days = 1, CellsPerDay = 3 };
            var t1 = new Teacher();
            var g1 = new StudentGroup();
            t1.Grid.Initialize(3);
            g1.Grid.Initialize(3);
            
            var lesson1 = new Lesson { Id = Guid.NewGuid(), TeacherId = t1.Id, StudentGroupId = g1.Id };
            var lesson2 = new Lesson { Id = Guid.NewGuid(), TeacherId = t1.Id, StudentGroupId = g1.Id };
            
            var schedule = new Schedule(config, new List<Teacher> { t1 }, new List<StudentGroup> { g1 }, new List<Lesson> { lesson1, lesson2 });

            t1.Grid.Place(lesson1.Id, 0); // Урок 1 є у вчителя
            g1.Grid.Place(lesson2.Id, 0); // Урок 2 є у групи (Асиметрія: на одній клітинці різні уроки)

            schedule.ValidateConsistency();

            // Обидва уроки мають бути видалені з цієї клітинки, оскільки консистентність порушена для обох
            Assert.True(t1.Grid.IsFree(0));
            Assert.True(g1.Grid.IsFree(0));
        }
        
        [Fact]
        public void MaxConsecutiveLessonsConstraint_LimitsDoNotCarryOverToNextDay()
        {
            var config = new ScheduleConfig { Days = 2, CellsPerDay = 2 };
            var t1 = new Teacher();
            t1.Grid.Initialize(4); // Day 1: 0,1. Day 2: 2,3
            var schedule = new Schedule(config, new List<Teacher> { t1 }, new List<StudentGroup>(), new List<Lesson>());

            // Урок наприкінці 1-го дня (клітинка 1)
            t1.Grid.Place(Guid.NewGuid(), 1);

            // Ліміт 2 підряд
            var constraint = new MaxConsecutiveLessonsConstraint(t1.Id, 2);
            constraint.Apply(schedule);

            // Клітинка 2 (початок наступного дня) не повинна блокуватися, бо лічильник скидається щодня
            Assert.True(t1.Grid.IsFree(2));
        }
    }
}
