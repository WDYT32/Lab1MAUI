using Lab1MAUI.Models.Schedule.Constraints;
using System;
using System.Linq;
using Lab1MAUI.Models.Schedule;
using Lab1MAUI.Models.Schedule.Participants;
using Lab1MAUI.Models.Schedule.Constraints;
using Lab1MAUI.ViewModels;
using Xunit;

namespace Lab1MAUI.Tests
{
    public class ViewModelIntegrationTests
    {
        private Schedule CreateEmptySchedule() => 
            new Schedule(new ScheduleConfig { Days = 1, CellsPerDay = 5 });

        [Fact]
        public void GroupEditViewModel_AddsNewGroup()
        {
            var schedule = CreateEmptySchedule();
            var vm = new GroupEditViewModel(schedule);
            
            vm.GroupId = "new";
            vm.GroupName = "TestGroup";
            vm.StudentsCount = 25;
            vm.SaveCommand.Execute(null);

            Assert.Single(schedule.Groups);
            Assert.Equal("TestGroup", schedule.Groups[0].Name);
            Assert.Equal(25, schedule.Groups[0].StudentsCount);
        }

        [Fact]
        public void GroupEditViewModel_EditsExistingGroup()
        {
            var schedule = CreateEmptySchedule();
            var group = new StudentGroup(Guid.NewGuid(), "OldName", 5) { StudentsCount = 10 };
            schedule.Groups.Add(group);

            var vm = new GroupEditViewModel(schedule);
            vm.GroupId = group.Id.ToString();
            vm.GroupName = "NewName";
            vm.StudentsCount = 30;
            vm.SaveCommand.Execute(null);

            Assert.Single(schedule.Groups);
            Assert.Equal("NewName", schedule.Groups[0].Name);
            Assert.Equal(30, schedule.Groups[0].StudentsCount);
        }

        [Fact]
        public void TeacherEditViewModel_AddsNewTeacher()
        {
            var schedule = CreateEmptySchedule();
            var vm = new TeacherEditViewModel(schedule);
            
            vm.TeacherId = "new";
            vm.FirstName = "John";
            vm.LastName = "Doe";
            vm.Patronymic = "Smith";
            vm.TargetHours = 2;
            vm.SaveCommand.Execute(null);

            Assert.Single(schedule.Teachers);
            Assert.Equal("Doe John Smith", schedule.Teachers[0].Name);
            Assert.Equal(2, schedule.Teachers[0].TargetHours);
        }

        [Fact]
        public void RoomEditViewModel_AddsNewRoom()
        {
            var schedule = CreateEmptySchedule();
            var vm = new RoomEditViewModel(schedule);
            
            vm.RoomId = "new";
            vm.Name = "202";
            vm.Capacity = 40;
            vm.SaveCommand.Execute(null);

            Assert.Single(schedule.Rooms);
            Assert.Equal("202", schedule.Rooms[0].Name);
            Assert.Equal(40, schedule.Rooms[0].Capacity);
        }

        [Fact]
        public void RoomsViewModel_DeletesRoom()
        {
            var schedule = CreateEmptySchedule();
            var room = new Room(Guid.NewGuid(), "202", 40, 5);
            schedule.Rooms.Add(room);

            var vm = new RoomsViewModel(schedule);
            Assert.Single(vm.Rooms);

            vm.DeleteRoomCommand.Execute(room);

            Assert.Empty(schedule.Rooms);
            Assert.Empty(vm.Rooms);
        }

        [Fact]
        public void TeachersViewModel_DeletesTeacher()
        {
            var schedule = CreateEmptySchedule();
            var t = new Teacher(Guid.NewGuid(), "T1", 5);
            schedule.Teachers.Add(t);

            var vm = new TeachersViewModel(schedule);
            Assert.Single(vm.Teachers);

            vm.DeleteTeacherCommand.Execute(t);

            Assert.Empty(schedule.Teachers);
            Assert.Empty(vm.Teachers);
        }

        [Fact]
        public void ConstraintFormViewModel_AddsDailyLimitToTeacher()
        {
            var schedule = CreateEmptySchedule();
            var t = new Teacher(Guid.NewGuid(), "T1", 5);
            schedule.Teachers.Add(t);

            var vm = new ConstraintFormViewModel(ConstraintOwnerType.Teacher, t, schedule);
            vm.SelectedConstraintType = "Ліміт занять на день (DailyLimit)";
            vm.DailyLimitValue = 2;
            
            IConstraint added = null;
            vm.OnConstraintAdded += c => added = c;
            
            vm.AddConstraintCommand.Execute(null);

            Assert.NotNull(added);
            Assert.IsType<DailyLimitConstraint>(added);
        }

        [Fact]
        public void DisciplineEditViewModel_SavesDiscipline()
        {
            var schedule = CreateEmptySchedule();
            var vm = new DisciplineEditViewModel(schedule);
            
            vm.DisciplineId = "new";
            vm.Name = "History";
            vm.SaveCommand.Execute(null);

            Assert.Single(schedule.Disciplines);
            Assert.Equal("History", schedule.Disciplines[0].Name);
        }
    }
}
