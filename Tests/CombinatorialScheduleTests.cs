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

        
        
        

        [Fact]
        public void CrossEntity_TeacherSpecificDay_Vs_GroupSpecificDay_Disjoint_YieldsEmpty()
        {
            var s = Create(3, 2); 
            var math = AddD(s, "Math");
            var r = AddR(s, "101", 30);
            
            var t = AddT(s, "T1", math);
            t.Constraints.Add(new SpecificDayConstraint(new[] { 0 }, 2)); 

            var g = AddG(s, "G1", 20, math, 2);
            g.Constraints.Add(new SpecificDayConstraint(new[] { 1 }, 2)); 

            s.Generate();
            Assert.Empty(s.Grid.SelectMany(c => c));
        }

        [Fact]
        public void CrossEntity_TeacherTimeOff_Vs_GroupSpecificTime_Checkerboard_YieldsEmpty()
        {
            var s = Create(1, 4); 
            var math = AddD(s, "Math");
            AddR(s, "101", 30);
            
            var t = AddT(s, "T1", math);
            t.Constraints.Add(new TimeOffConstraint(new[] { 0, 2 })); 

            var g = AddG(s, "G1", 20, math, 2);
            g.Constraints.Add(new SpecificTimeConstraint(new[] { 0, 2 }, 4)); 

            s.Generate();
            Assert.Empty(s.Grid.SelectMany(c => c));
        }

        [Fact]
        public void CrossEntity_TeacherDailyLimit_Vs_GroupSpecificDay_Bottleneck()
        {
            var s = Create(2, 4); 
            var math = AddD(s, "Math");
            AddR(s, "101", 30);

            var t = AddT(s, "T1", math);
            t.Constraints.Add(new DailyLimitConstraint(t, 2, 4)); 

            var g = AddG(s, "G1", 20, math, 4);
            g.Constraints.Add(new SpecificDayConstraint(new[] { 0 }, 4)); 

            s.Generate();
            
            var lessons = s.Grid.SelectMany(c => c).ToList();
            Assert.Equal(2, lessons.Count); 
            Assert.All(lessons, l => Assert.True(l.CellIndex < 4)); 
        }

        [Fact]
        public void CrossEntity_DisciplineDailyLimit_Vs_GroupDailyLimit_MinWins()
        {
            var s = Create(2, 4); 
            var math = AddD(s, "Math");
            math.Constraints.Add(new DailyLimitConstraint(math, 1, 4)); 
            AddR(s, "101", 30);

            var t = AddT(s, "T1", math);
            
            var g = AddG(s, "G1", 20, math, 4);
            g.Constraints.Add(new DailyLimitConstraint(g, 2, 4)); 

            s.Generate();
            
            var lessons = s.Grid.SelectMany(c => c).ToList();
            Assert.Equal(2, lessons.Count); 
        }

        
        
        

        [Fact]
        public void Resources_RoomsMixedCapacities_AlgorithmTriesAllCombinations()
        {
            var s = Create(1, 1); 
            var math = AddD(s, "Math");
            
            
            AddR(s, "SmallRoom", 15);
            AddR(s, "BigRoom", 30);

            AddT(s, "T1", math);
            AddT(s, "T2", math);

            
            var bigGroup = AddG(s, "BigGroup", 30, math, 1);
            var smallGroup = AddG(s, "SmallGroup", 10, math, 1);

            
            
            s.Generate();

            var lessons = s.Grid[0].ToList();
            Assert.Equal(2, lessons.Count); 
        }

        [Fact]
        public void Resources_TeacherTargetHours_CreatesBottleneck()
        {
            var s = Create(1, 5);
            var math = AddD(s, "Math");
            AddR(s, "101", 30);

            
            AddT(s, "LazyTeacher", math, target: 2);

            
            AddG(s, "G1", 20, math, 4);

            s.Generate();
            Assert.Equal(2, s.Grid.SelectMany(c => c).Count()); 
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
            t.Constraints.Add(new DailyLimitConstraint(t, 2, 4)); 
            s.Teachers.Add(t);

            var g = new StudentGroup(Guid.NewGuid(), "G1", 8) { StudentsCount = 20 };
            g.Requirements.Add(new GroupRequirement { DisciplineId = math.Id, RequiredHours = 2 });
            g.Requirements.Add(new GroupRequirement { DisciplineId = phys.Id, RequiredHours = 2 });
            s.Groups.Add(g);

            s.Generate();
            
            var lessons = s.Grid.SelectMany(c => c).ToList();
            Assert.Equal(4, lessons.Count); 
            
            int day0 = lessons.Count(l => l.CellIndex < 4);
            int day1 = lessons.Count(l => l.CellIndex >= 4);
            Assert.Equal(2, day0);
            Assert.Equal(2, day1);
        }

        
        
        

        [Fact]
        public void Complex_SpecificTime_Vs_DailyLimit_PartialPlacement()
        {
            var s = Create(2, 4); 
            var math = AddD(s, "Math");
            AddR(s, "101", 30);
            AddT(s, "T1", math);

            var g = AddG(s, "G1", 20, math, 4);
            
            g.Constraints.Add(new SpecificTimeConstraint(new[] { 0, 1, 2 }, 4));
            
            g.Constraints.Add(new SpecificDayConstraint(new[] { 0 }, 4));
            
            g.Constraints.Add(new DailyLimitConstraint(g, 2, 4));

            s.Generate();

            var lessons = s.Grid.SelectMany(c => c).ToList();
            Assert.Equal(2, lessons.Count); 
        }

        [Fact]
        public void Complex_MaxConsecutive_Forces_DaysToBeSkipped()
        {
            var s = Create(3, 2); 
            var math = AddD(s, "Math");
            AddR(s, "101", 30);
            AddT(s, "T1", math);

            var g = AddG(s, "G1", 20, math, 3);
            
            
            g.Constraints.Add(new MaxConsecutiveLessonsConstraint(g, 1, 2));

            s.Generate();

            var lessons = s.Grid.SelectMany(c => c).OrderBy(l => l.CellIndex).ToList();
            Assert.Equal(3, lessons.Count);
            
            
            Assert.Equal(0, lessons[0].CellIndex / 2);
            Assert.Equal(1, lessons[1].CellIndex / 2);
            Assert.Equal(2, lessons[2].CellIndex / 2);
        }

        [Fact]
        public void Complex_SpecificDay_OutOfBounds_YieldsEmpty()
        {
            var s = Create(3, 3); 
            var math = AddD(s, "Math");
            AddR(s, "101", 30);
            AddT(s, "T1", math);

            var g = AddG(s, "G1", 20, math, 2);
            
            g.Constraints.Add(new SpecificDayConstraint(new[] { 5 }, 3));

            s.Generate();
            Assert.Empty(s.Grid.SelectMany(c => c));
        }

        [Fact]
        public void Complex_AllConstraints_Simultaneously()
        {
            var s = Create(4, 4); 
            var math = AddD(s, "Math");
            AddR(s, "101", 30);
            
            var t = AddT(s, "T1", math, target: 4); 
            t.Constraints.Add(new MaxConsecutiveLessonsConstraint(t, 1, 4)); 
            t.Constraints.Add(new TimeOffConstraint(new[] { 0, 4, 8, 12 })); 

            var g = AddG(s, "G1", 20, math, 8); 
            g.Constraints.Add(new SpecificDayConstraint(new[] { 0, 1, 2 }, 4)); 
            g.Constraints.Add(new DailyLimitConstraint(g, 2, 4)); 

            
            
            
            
            
            

            s.Generate();

            var lessons = s.Grid.SelectMany(c => c).ToList();
            Assert.Equal(4, lessons.Count); 
        }

        
        
        

        [Fact]
        public void Extreme_MaxConsecutive_Zero_BlocksAll()
        {
            var s = Create(2, 2);
            var math = AddD(s, "Math");
            AddR(s, "101", 30);
            AddT(s, "T1", math);

            var g = AddG(s, "G1", 20, math, 2);
            
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
