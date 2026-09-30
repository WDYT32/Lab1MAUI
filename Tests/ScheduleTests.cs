using System.Collections.Generic;
using System.Linq;
using Lab1MAUI.Models.Schedule;
using Xunit;

namespace Lab1MAUI.Tests
{
    public class ScheduleTests
    {
        [Fact]
        public void Generate_AssignsLessonsCorrectly_WithoutOverlaps()
        {
            var config = new ScheduleConfig { Days = 5, CellsPerDay = 8 };
            
            var teacher1 = new Teacher { Name = "T1" };
            var teacher2 = new Teacher { Name = "T2" };
            
            var group1 = new StudentGroup { Name = "G1" };
            var group2 = new StudentGroup { Name = "G2" };

            var lessons = new List<Lesson>
            {
                new Lesson { TeacherId = teacher1.Id, StudentGroupId = group1.Id, SubjectName = "Math" },
                new Lesson { TeacherId = teacher1.Id, StudentGroupId = group2.Id, SubjectName = "Math" },
                new Lesson { TeacherId = teacher2.Id, StudentGroupId = group1.Id, SubjectName = "Physics" }
            };

            var schedule = new Schedule(config, new List<Teacher> { teacher1, teacher2 }, new List<StudentGroup> { group1, group2 }, lessons);

            schedule.Generate();

            var t1 = schedule.Teachers.First(t => t.Id == teacher1.Id);
            var t2 = schedule.Teachers.First(t => t.Id == teacher2.Id);
            var g1 = schedule.Groups.First(g => g.Id == group1.Id);
            var g2 = schedule.Groups.First(g => g.Id == group2.Id);

            Assert.Contains(t1.Grid.Cells, cell => cell != null);
            Assert.Contains(g1.Grid.Cells, cell => cell != null);

            for (int i = 0; i < schedule.Config.TotalCells; i++)
            {
                if (t1.Grid.Cells[i] != null)
                {
                    var lessonId = t1.Grid.Cells[i].Value;
                    var lesson = schedule.Lessons.First(l => l.Id == lessonId);
                    
                    if (lesson.StudentGroupId == g1.Id)
                    {
                        Assert.Equal(lessonId, g1.Grid.Cells[i]);
                    }
                    else if (lesson.StudentGroupId == g2.Id)
                    {
                        Assert.Equal(lessonId, g2.Grid.Cells[i]);
                    }
                }
            }

            var assignedCellsT1 = t1.Grid.Cells.Where(c => c != null).ToList();
            Assert.Equal(2, assignedCellsT1.Count);
        }
    }
}
