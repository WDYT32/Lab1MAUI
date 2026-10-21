using Lab1MAUI.Models.UI;
using Lab1MAUI.Models.UI.Participants;
using Lab1MAUI.Models.UI.Constraints;
using Lab1MAUI.ViewModels;
using Lab1MAUI.ViewModels.Edit;
using System;
using System.Linq;
using Xunit;

namespace Lab1MAUI.Tests
{
    public class MultipleRegenerationTests
    {
        private static ScheduleUI CreateBaseUI(int days = 1, int cellsPerDay = 5)
        {
            var ui = new ScheduleUI();
            ui.Config.Days = days;
            ui.Config.CellsPerDay = cellsPerDay;
            return ui;
        }

        private static DisciplineUI AddDiscipline(ScheduleUI ui, string name)
        {
            var d = new DisciplineUI { Id = Guid.NewGuid(), Name = name };
            ui.Disciplines.Add(d);
            return d;
        }

        private static TeacherUI AddTeacher(ScheduleUI ui, string name, int targetLessons, params DisciplineUI[] competences)
        {
            var t = new TeacherUI { Id = Guid.NewGuid(), Name = name, TargetLessons = targetLessons };
            foreach (var d in competences)
                t.Competences.Add(new TeacherCompetenceUI { DisciplineId = d.Id });
            ui.Teachers.Add(t);
            return t;
        }

        private static RoomUI AddRoom(ScheduleUI ui, string name, int capacity = 30)
        {
            var r = new RoomUI { Id = Guid.NewGuid(), Name = name, Capacity = capacity };
            ui.Rooms.Add(r);
            return r;
        }

        private static StudentGroupUI AddGroup(ScheduleUI ui, string name, int studentsCount,
            params (DisciplineUI disc, int lessons)[] requirements)
        {
            var g = new StudentGroupUI { Id = Guid.NewGuid(), Name = name, StudentsCount = studentsCount };
            foreach (var (disc, lessons) in requirements)
                g.Requirements.Add(new GroupRequirementUI { DisciplineId = disc.Id, RequiredLessons = lessons });
            ui.Groups.Add(g);
            return g;
        }

        [Fact]
        public void Regeneration_DomainStateIsClean_FulfilledLessonsAndAssignedLessonsDoNotAccumulate()
        {
            var ui = CreateBaseUI();
            var math = AddDiscipline(ui, "Math");
            var teacher = AddTeacher(ui, "Smith", targetLessons: 5, math);
            AddRoom(ui, "101");
            var group = AddGroup(ui, "G1", 25, (math, 2));

            var result1 = ui.Generate();

            var placed1 = result1.Grid.SelectMany(c => c).ToList();
            Assert.Equal(2, placed1.Count);

            var domainGroup1  = result1.Groups.First(g => g.Id == group.Id);
            var domainTeach1  = result1.Teachers.First(t => t.Id == teacher.Id);
            Assert.Equal(2, domainGroup1.Requirements.First().FulfilledLessons); 
            Assert.Equal(2, domainTeach1.AssignedLessons);                       

            group.Requirements[0].RequiredLessons = 3;

            var result2 = ui.Generate();

            Assert.NotSame(result1, result2); 

            var placed2 = result2.Grid.SelectMany(c => c).ToList();

            Assert.Equal(3, placed2.Count);

            var domainGroup2  = result2.Groups.First(g => g.Id == group.Id);
            var domainTeach2  = result2.Teachers.First(t => t.Id == teacher.Id);
            Assert.Equal(3, domainGroup2.Requirements.First().FulfilledLessons); 
            Assert.Equal(3, domainTeach2.AssignedLessons);                       
        }

        [Fact]
        public void Regeneration_TeacherReplaced_NewLessonsUseOnlyNewTeacher()
        {
            var ui = CreateBaseUI();
            var math = AddDiscipline(ui, "Math");
            var oldTeacher = AddTeacher(ui, "OldTeacher", targetLessons: 5, math);
            AddRoom(ui, "101");
            var group = AddGroup(ui, "G1", 25, (math, 2));

            ui.Generate();
            var r1Lessons = ui.LastResult.Grid.SelectMany(c => c).ToList();
            Assert.All(r1Lessons, l => Assert.Equal(oldTeacher.Id, l.TeacherId));

            ui.Teachers.Remove(oldTeacher);
            var newTeacher = AddTeacher(ui, "NewTeacher", targetLessons: 5, math);

            ui.Generate();
            var r2Lessons = ui.LastResult.Grid.SelectMany(c => c).ToList();

            Assert.Equal(2, r2Lessons.Count);
            Assert.All(r2Lessons, l => Assert.Equal(newTeacher.Id, l.TeacherId));
        }

        [Fact]
        public void Regeneration_ConstraintAddedBetweenRuns_SecondRunRespectsNewConstraint()
        {
            var ui = CreateBaseUI(days: 3, cellsPerDay: 1);
            var math = AddDiscipline(ui, "Math");
            var teacher = AddTeacher(ui, "Smith", targetLessons: 5, math);
            AddRoom(ui, "101");
            AddGroup(ui, "G1", 25, (math, 2));

            ui.Generate();
            var slots1 = ui.LastResult.Grid.SelectMany(c => c).Select(l => l.CellIndex).OrderBy(x => x).ToList();
            Assert.Equal(2, slots1.Count);
            Assert.Contains(0, slots1);

            var constraintForm = new ConstraintFormViewModel(ConstraintOwnerType.Teacher);
            constraintForm.SelectType(FormConstraintType.TimeOff);
            constraintForm.TimeOffCellIndex = 0; 

            IConstraintUI? addedConstraint = null;
            constraintForm.OnConstraintAdded += c => addedConstraint = c;
            constraintForm.AddConstraintCommand.Execute(null);

            Assert.NotNull(addedConstraint);
            teacher.Constraints.Add(addedConstraint);

            ui.Generate();
            var slots2 = ui.LastResult.Grid.SelectMany(c => c).Select(l => l.CellIndex).OrderBy(x => x).ToList();

            Assert.Equal(2, slots2.Count);
            Assert.DoesNotContain(0, slots2); 
            Assert.Equal([1, 2], slots2);
        }

        [Fact]
        public void Regeneration_GroupAdded_NewScheduleIncludesBothGroups()
        {
            var ui = CreateBaseUI(days: 1, cellsPerDay: 4);
            var math = AddDiscipline(ui, "Math");
            AddTeacher(ui, "T1", targetLessons: 5, math);
            AddTeacher(ui, "T2", targetLessons: 5, math);
            AddRoom(ui, "R1");
            AddRoom(ui, "R2");
            AddGroup(ui, "G1", 25, (math, 2));

            ui.Generate();
            Assert.Equal(2, ui.LastResult.Grid.SelectMany(c => c).Count());

            AddGroup(ui, "G2", 20, (math, 2));

            ui.Generate();
            var lessons2 = ui.LastResult.Grid.SelectMany(c => c).ToList();

            Assert.Equal(4, lessons2.Count); 
            Assert.Equal(2, lessons2.Select(l => l.StudentGroupId).Distinct().Count()); 
        }

        [Fact]
        public void Regeneration_AfterFailure_ScheduleResultViewModelShowsCorrectUnassigned()
        {
            var ui = CreateBaseUI(days: 1, cellsPerDay: 3);
            var math = AddDiscipline(ui, "Math");
            var teacher = AddTeacher(ui, "Smith", targetLessons: 5, math);
            AddRoom(ui, "101");
            var group = AddGroup(ui, "G1", 25, (math, 2));

            ui.Generate();

            var vm1 = new ScheduleResultViewModel(ui);
            Assert.Equal(0, vm1.UnassignedLessons.Count);
            Assert.Equal(2, vm1.GroupSchedules.First().Days.SelectMany(d => d.Lessons).Count());

            group.Requirements[0].RequiredLessons = 10;

            ui.Generate();

            var vm2 = new ScheduleResultViewModel(ui);
            Assert.True(vm2.HasUnassignedLessons);

            var unassigned = vm2.UnassignedLessons.First();
            Assert.Equal("G1", unassigned.GroupName);
            Assert.Equal("Math", unassigned.SubjectName);
            Assert.Equal(7, unassigned.LessonsCount);
        }

        [Fact]
        public void FullUserWorkflow_ThreeRoundsOfEditAndGenerate_EachResultIsIndependent()
        {
            var ui = CreateBaseUI(days: 2, cellsPerDay: 4); 
            var math    = AddDiscipline(ui, "Math");
            var physics = AddDiscipline(ui, "Physics");
            var teacher = AddTeacher(ui, "Prof", targetLessons: 10, math, physics);
            AddRoom(ui, "101", capacity: 30);
            var group = AddGroup(ui, "G1", 25, (math, 2));

            var r1 = ui.Generate();
            Assert.Equal(2, r1.Grid.SelectMany(c => c).Count());

            group.Requirements.Add(new GroupRequirementUI { DisciplineId = physics.Id, RequiredLessons = 3 });

            var r2 = ui.Generate();

            Assert.NotSame(r1, r2);
            Assert.Equal(5, r2.Grid.SelectMany(c => c).Count()); 

            Assert.Equal(2, r1.Grid.SelectMany(c => c).Count());

            group.Requirements.First(r => r.DisciplineId == math.Id).RequiredLessons = 4;

            teacher.Constraints.Add(new DailyLimitConstraintUI { MaxLessonsPerDay = 3 });

            var r3 = ui.Generate();

            Assert.Equal(5, r2.Grid.SelectMany(c => c).Count()); 
            Assert.Equal(2, r1.Grid.SelectMany(c => c).Count()); 

            Assert.NotSame(r1, r3);
            Assert.NotSame(r2, r3);

            var vm = new ScheduleResultViewModel(ui);
            int totalRequired   = group.Requirements.Sum(r => r.RequiredLessons); 
            int totalPlaced     = r3.Grid.SelectMany(c => c).Count();
            int totalUnassigned = totalRequired - totalPlaced;

            Assert.True(totalUnassigned > 0, "Після провальної генерації мають бути нерозподілені уроки");
            Assert.Equal(totalUnassigned, vm.UnassignedLessons.Sum(u => u.LessonsCount));
        }
    }
}
