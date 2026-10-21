using Lab1MAUI.Models.Schedule.Constraints;
using System;
using System.Linq;
using Lab1MAUI.Models.Schedule;
using Lab1MAUI.Models.Schedule.Participants;
using Xunit;

namespace Lab1MAUI.Tests
{
    public class ScheduleGenerationTests
    {
        private Schedule CreateEmptySchedule(int days = 5, int cellsPerDay = 5) => 
            new Schedule(new ScheduleConfig { Days = days, CellsPerDay = cellsPerDay });

        [Fact]
        public void Generate_OneLesson_AssignsSuccessfully()
        {
            var schedule = CreateEmptySchedule(1, 1);
            var math = new Discipline(Guid.NewGuid(), "Math", 1);
            var room = new Room(Guid.NewGuid(), "101", 30, 1);
            var t1 = new Teacher(Guid.NewGuid(), "T1", 1) { TargetLessons = 1 };
            t1.Competences.Add(new TeacherCompetence { DisciplineId = math.Id });
            var g1 = new StudentGroup(Guid.NewGuid(), "G1", 1) { StudentsCount = 20 };
            g1.Requirements.Add(new GroupRequirement { DisciplineId = math.Id, RequiredLessons = 1 });

            schedule.Disciplines.Add(math);
            schedule.Rooms.Add(room);
            schedule.Teachers.Add(t1);
            schedule.Groups.Add(g1);

            schedule.Generate();

            var lessons = schedule.Grid.SelectMany(c => c).ToList();
            Assert.Single(lessons);
            Assert.Equal(0, lessons[0].CellIndex);
        }

        [Fact]
        public void Generate_FailsIfNoCompetentTeacher()
        {
            var schedule = CreateEmptySchedule(1, 1);
            var math = new Discipline(Guid.NewGuid(), "Math", 1);
            var room = new Room(Guid.NewGuid(), "101", 30, 1);
            var t1 = new Teacher(Guid.NewGuid(), "T1", 1) { TargetLessons = 1 }; 
            var g1 = new StudentGroup(Guid.NewGuid(), "G1", 1) { StudentsCount = 20 };
            g1.Requirements.Add(new GroupRequirement { DisciplineId = math.Id, RequiredLessons = 1 });

            schedule.Disciplines.Add(math);
            schedule.Rooms.Add(room);
            schedule.Teachers.Add(t1);
            schedule.Groups.Add(g1);

            schedule.Generate();
            Assert.Empty(schedule.Grid.SelectMany(c => c)); 
        }

        [Fact]
        public void Generate_FailsIfRoomTooSmall()
        {
            var schedule = CreateEmptySchedule(1, 1);
            var math = new Discipline(Guid.NewGuid(), "Math", 1);
            var room = new Room(Guid.NewGuid(), "101", 10, 1); 
            var t1 = new Teacher(Guid.NewGuid(), "T1", 1) { TargetLessons = 1 };
            t1.Competences.Add(new TeacherCompetence { DisciplineId = math.Id });
            var g1 = new StudentGroup(Guid.NewGuid(), "G1", 1) { StudentsCount = 20 }; 
            g1.Requirements.Add(new GroupRequirement { DisciplineId = math.Id, RequiredLessons = 1 });

            schedule.Disciplines.Add(math);
            schedule.Rooms.Add(room);
            schedule.Teachers.Add(t1);
            schedule.Groups.Add(g1);

            schedule.Generate();
            Assert.Empty(schedule.Grid.SelectMany(c => c));
        }

        [Fact]
        public void Generate_TwoGroups_ShareTeacher_DifferentSlots()
        {
            var schedule = CreateEmptySchedule(1, 2);
            var math = new Discipline(Guid.NewGuid(), "Math", 2);
            var room = new Room(Guid.NewGuid(), "101", 30, 2);
            var t1 = new Teacher(Guid.NewGuid(), "T1", 2) { TargetLessons = 2 };
            t1.Competences.Add(new TeacherCompetence { DisciplineId = math.Id });
            
            var g1 = new StudentGroup(Guid.NewGuid(), "G1", 2) { StudentsCount = 20 };
            g1.Requirements.Add(new GroupRequirement { DisciplineId = math.Id, RequiredLessons = 1 });
            var g2 = new StudentGroup(Guid.NewGuid(), "G2", 2) { StudentsCount = 20 };
            g2.Requirements.Add(new GroupRequirement { DisciplineId = math.Id, RequiredLessons = 1 });

            schedule.Disciplines.Add(math);
            schedule.Rooms.Add(room);
            schedule.Teachers.Add(t1);
            schedule.Groups.Add(g1);
            schedule.Groups.Add(g2);

            schedule.Generate();

            var lessons = schedule.Grid.SelectMany(c => c).ToList();
            Assert.Equal(2, lessons.Count);
            Assert.NotEqual(lessons[0].CellIndex, lessons[1].CellIndex); 
        }

        [Fact]
        public void Generate_TwoGroups_DifferentTeachers_SameSlot_DifferentRooms()
        {
            var schedule = CreateEmptySchedule(1, 1); 
            var math = new Discipline(Guid.NewGuid(), "Math", 1);
            
            var r1 = new Room(Guid.NewGuid(), "101", 30, 1);
            var r2 = new Room(Guid.NewGuid(), "102", 30, 1);
            
            var t1 = new Teacher(Guid.NewGuid(), "T1", 1) { TargetLessons = 1 };
            t1.Competences.Add(new TeacherCompetence { DisciplineId = math.Id });
            var t2 = new Teacher(Guid.NewGuid(), "T2", 1) { TargetLessons = 1 };
            t2.Competences.Add(new TeacherCompetence { DisciplineId = math.Id });
            
            var g1 = new StudentGroup(Guid.NewGuid(), "G1", 1) { StudentsCount = 20 };
            g1.Requirements.Add(new GroupRequirement { DisciplineId = math.Id, RequiredLessons = 1 });
            var g2 = new StudentGroup(Guid.NewGuid(), "G2", 1) { StudentsCount = 20 };
            g2.Requirements.Add(new GroupRequirement { DisciplineId = math.Id, RequiredLessons = 1 });

            schedule.Disciplines.Add(math);
            schedule.Rooms.Add(r1);
            schedule.Rooms.Add(r2);
            schedule.Teachers.Add(t1);
            schedule.Teachers.Add(t2);
            schedule.Groups.Add(g1);
            schedule.Groups.Add(g2);

            schedule.Generate(); 

            var lessons = schedule.Grid.SelectMany(c => c).ToList();
            Assert.Equal(2, lessons.Count);
            Assert.Equal(lessons[0].CellIndex, lessons[1].CellIndex);
            Assert.NotEqual(lessons[0].RoomId, lessons[1].RoomId);
        }

        [Fact]
        public void Generate_FailsIfNotEnoughSlotsForRequirements()
        {
            var schedule = CreateEmptySchedule(1, 2); 
            var math = new Discipline(Guid.NewGuid(), "Math", 2);
            var room = new Room(Guid.NewGuid(), "101", 30, 2);
            var t1 = new Teacher(Guid.NewGuid(), "T1", 2) { TargetLessons = 3 };
            t1.Competences.Add(new TeacherCompetence { DisciplineId = math.Id });
            
            var g1 = new StudentGroup(Guid.NewGuid(), "G1", 2) { StudentsCount = 20 };
            g1.Requirements.Add(new GroupRequirement { DisciplineId = math.Id, RequiredLessons = 3 }); 

            schedule.Disciplines.Add(math);
            schedule.Rooms.Add(room);
            schedule.Teachers.Add(t1);
            schedule.Groups.Add(g1);

            schedule.Generate();
            Assert.Equal(2, schedule.Grid.SelectMany(c => c).Count()); 
        }
    }
}
