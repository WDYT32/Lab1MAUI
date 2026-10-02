using Lab1MAUI.Models.MTM;
using Lab1MAUI.Models.MTM.Constraints;
using Lab1MAUI.ViewModels;
using System;
using System.Linq;
using Xunit;

namespace Lab1MAUI.Tests
{
    
    
    
    
    
    
    
    
    
    
    
    
    public class MultipleRegenerationTests
    {
        
        
        

        
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

        
        
        

        [Fact]
        public void Regeneration_DomainStateIsClean_FulfilledHoursAndAssignedHoursDoNotAccumulate()
        {
            
            var mtm = CreateBaseMTM();
            var math = AddDiscipline(mtm, "Math");
            var teacher = AddTeacher(mtm, "Smith", targetHours: 5, math);
            AddRoom(mtm, "101");
            var group = AddGroup(mtm, "G1", 25, (math, 2));

            
            bool ok1 = mtm.TryGenerate(out _);
            var result1 = mtm.LastResult;

            Assert.True(ok1, "Перша генерація повинна бути успішною");
            var placed1 = result1.Grid.SelectMany(c => c).ToList();
            Assert.Equal(2, placed1.Count);

            
            var domainGroup1  = result1.Groups.First(g => g.Id == group.Id);
            var domainTeach1  = result1.Teachers.First(t => t.Id == teacher.Id);
            Assert.Equal(2, domainGroup1.Requirements.First().FulfilledHours); 
            Assert.Equal(2, domainTeach1.AssignedHours);                       

            
            group.Requirements[0].RequiredHours = 3;

            bool ok2 = mtm.TryGenerate(out _);
            var result2 = mtm.LastResult;

            Assert.True(ok2, "Друга генерація повинна бути успішною");
            Assert.NotSame(result1, result2); 

            var placed2 = result2.Grid.SelectMany(c => c).ToList();

            
            
            
            Assert.Equal(3, placed2.Count);

            var domainGroup2  = result2.Groups.First(g => g.Id == group.Id);
            var domainTeach2  = result2.Teachers.First(t => t.Id == teacher.Id);
            Assert.Equal(3, domainGroup2.Requirements.First().FulfilledHours); 
            Assert.Equal(3, domainTeach2.AssignedHours);                       
        }

        
        
        

        [Fact]
        public void Regeneration_TeacherReplaced_NewLessonsUseOnlyNewTeacher()
        {
            
            var mtm = CreateBaseMTM();
            var math = AddDiscipline(mtm, "Math");
            var oldTeacher = AddTeacher(mtm, "OldTeacher", targetHours: 5, math);
            AddRoom(mtm, "101");
            AddGroup(mtm, "G1", 25, (math, 2));

            
            Assert.True(mtm.TryGenerate(out _));
            var r1Lessons = mtm.LastResult.Grid.SelectMany(c => c).ToList();
            Assert.All(r1Lessons, l => Assert.Equal(oldTeacher.Id, l.TeacherId));

            
            mtm.Teachers.Remove(oldTeacher);
            var newTeacher = AddTeacher(mtm, "NewTeacher", targetHours: 5, math);

            
            Assert.True(mtm.TryGenerate(out _));
            var r2Lessons = mtm.LastResult.Grid.SelectMany(c => c).ToList();

            Assert.Equal(2, r2Lessons.Count);
            
            Assert.All(r2Lessons, l => Assert.Equal(newTeacher.Id, l.TeacherId));
            
            Assert.DoesNotContain(r2Lessons, l => l.TeacherId == oldTeacher.Id);
        }

        
        
        

        [Fact]
        public void Regeneration_TimeOffConstraintAdded_LessonsAvoidBlockedSlot()
        {
            
            var mtm = CreateBaseMTM(days: 3, cellsPerDay: 1);
            var math = AddDiscipline(mtm, "Math");
            var teacher = AddTeacher(mtm, "Smith", targetHours: 5, math);
            AddRoom(mtm, "101");
            AddGroup(mtm, "G1", 25, (math, 2));

            
            Assert.True(mtm.TryGenerate(out _));
            var slots1 = mtm.LastResult.Grid.SelectMany(c => c).Select(l => l.CellIndex).OrderBy(x => x).ToList();
            Assert.Equal(2, slots1.Count);
            
            Assert.Contains(0, slots1);

            
            var constraintForm = new ConstraintFormViewModel(ConstraintOwnerType.Teacher, mtm.Config);
            constraintForm.SelectedConstraintType = "Вихідний час (TimeOff)";
            constraintForm.TimeOffCellIndex = 0; 

            ConstraintMTM addedConstraint = null;
            constraintForm.OnConstraintAdded += c => addedConstraint = c;
            constraintForm.AddConstraintCommand.Execute(null);

            Assert.NotNull(addedConstraint);
            
            teacher.Constraints.Add(addedConstraint);

            
            Assert.True(mtm.TryGenerate(out _));
            var slots2 = mtm.LastResult.Grid.SelectMany(c => c).Select(l => l.CellIndex).OrderBy(x => x).ToList();

            Assert.Equal(2, slots2.Count);
            Assert.DoesNotContain(0, slots2); 
            Assert.Equal([1, 2], slots2);
        }

        
        
        

        [Fact]
        public void Regeneration_GroupAdded_NewScheduleIncludesBothGroups()
        {
            
            var mtm = CreateBaseMTM(days: 1, cellsPerDay: 4);
            var math = AddDiscipline(mtm, "Math");
            AddTeacher(mtm, "T1", targetHours: 5, math);
            AddTeacher(mtm, "T2", targetHours: 5, math);
            AddRoom(mtm, "R1");
            AddRoom(mtm, "R2");
            AddGroup(mtm, "G1", 25, (math, 2));

            
            Assert.True(mtm.TryGenerate(out _));
            Assert.Equal(2, mtm.LastResult.Grid.SelectMany(c => c).Count());

            
            AddGroup(mtm, "G2", 20, (math, 2));

            
            Assert.True(mtm.TryGenerate(out _));
            var lessons2 = mtm.LastResult.Grid.SelectMany(c => c).ToList();

            Assert.Equal(4, lessons2.Count); 
            Assert.Equal(2, lessons2.Select(l => l.StudentGroupId).Distinct().Count()); 
        }

        
        
        
        

        [Fact]
        public void Regeneration_AfterFailure_ScheduleResultViewModelShowsCorrectUnassigned()
        {
            
            var mtm = CreateBaseMTM(days: 1, cellsPerDay: 3);
            var math = AddDiscipline(mtm, "Math");
            var teacher = AddTeacher(mtm, "Smith", targetHours: 5, math);
            AddRoom(mtm, "101");
            var group = AddGroup(mtm, "G1", 25, (math, 2));

            Assert.True(mtm.TryGenerate(out _));

            
            var vm1 = new ScheduleResultViewModel(mtm);
            Assert.Equal(0, vm1.UnassignedLessons.Count);
            Assert.Equal(2, vm1.GroupSchedules.First().Days.SelectMany(d => d.Lessons).Count());

            
            group.Requirements[0].RequiredHours = 10;

            bool ok = mtm.TryGenerate(out _);
            Assert.False(ok); 

            
            var vm2 = new ScheduleResultViewModel(mtm);
            Assert.True(vm2.HasUnassignedLessons);

            var unassigned = vm2.UnassignedLessons.First();
            Assert.Equal("G1", unassigned.GroupName);
            Assert.Equal("Math", unassigned.SubjectName);
            
            Assert.Equal(7, unassigned.Hours);
        }

        
        
        
        

        [Fact]
        public void FullUserWorkflow_ThreeRoundsOfEditAndGenerate_EachResultIsIndependent()
        {
            var mtm = CreateBaseMTM(days: 2, cellsPerDay: 4); 
            var math    = AddDiscipline(mtm, "Math");
            var physics = AddDiscipline(mtm, "Physics");
            var teacher = AddTeacher(mtm, "Prof", targetHours: 10, math, physics);
            AddRoom(mtm, "101", capacity: 30);
            var group = AddGroup(mtm, "G1", 25, (math, 2));

            
            Assert.True(mtm.TryGenerate(out _));
            var r1 = mtm.LastResult;
            Assert.Equal(2, r1.Grid.SelectMany(c => c).Count());

            
            group.Requirements.Add(new GroupRequirementMTM { DisciplineId = physics.Id, RequiredHours = 3 });

            Assert.True(mtm.TryGenerate(out _));
            var r2 = mtm.LastResult;

            Assert.NotSame(r1, r2);
            Assert.Equal(5, r2.Grid.SelectMany(c => c).Count()); 

            
            Assert.Equal(2, r1.Grid.SelectMany(c => c).Count());

            
            group.Requirements.First(r => r.DisciplineId == math.Id).RequiredHours = 4;

            
            
            teacher.Constraints.Add(new DailyLimitConstraintMTM { MaxLessonsPerDay = 3 });

            bool ok3 = mtm.TryGenerate(out string err3);
            var r3 = mtm.LastResult;

            Assert.False(ok3, "Раунд 3 повинен провалитися: 7 уроків не вміщаються при ліміті 3/день × 2 дні");
            Assert.NotEmpty(err3);

            
            Assert.Equal(5, r2.Grid.SelectMany(c => c).Count()); 
            Assert.Equal(2, r1.Grid.SelectMany(c => c).Count()); 

            
            
            Assert.NotSame(r1, r3);
            Assert.NotSame(r2, r3);

            
            var vm = new ScheduleResultViewModel(mtm);
            int totalRequired   = group.Requirements.Sum(r => r.RequiredHours); 
            int totalPlaced     = r3.Grid.SelectMany(c => c).Count();
            int totalUnassigned = totalRequired - totalPlaced;

            Assert.True(totalUnassigned > 0, "Після провальної генерації мають бути нерозподілені уроки");
            Assert.Equal(totalUnassigned, vm.UnassignedLessons.Sum(u => u.Hours));
        }
    }
}
