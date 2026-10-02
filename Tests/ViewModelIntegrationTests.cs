using Lab1MAUI.Models.MTM;
using Lab1MAUI.Models.MTM.Constraints;
using Lab1MAUI.Models.Schedule;
using Lab1MAUI.ViewModels;
using System;
using System.Linq;
using Xunit;

namespace Lab1MAUI.Tests
{
    public class ViewModelIntegrationTests
    {
        private ScheduleMTM CreateEmptyScheduleMTM()
        {
            var mtm = new ScheduleMTM();
            mtm.Config.Days = 1;
            mtm.Config.CellsPerDay = 5;
            return mtm;
        }

        [Fact]
        public void GroupEditViewModel_AddsNewGroup()
        {
            var mtm = CreateEmptyScheduleMTM();
            var vm = new GroupEditViewModel(mtm);

            vm.GroupId = "new";
            vm.GroupName = "TestGroup";
            vm.StudentsCount = 25;
            vm.SaveCommand.Execute(null);

            Assert.Single(mtm.Groups);
            Assert.Equal("TestGroup", mtm.Groups[0].Name);
            Assert.Equal(25, mtm.Groups[0].StudentsCount);
        }

        [Fact]
        public void GroupEditViewModel_EditsExistingGroup()
        {
            var mtm = CreateEmptyScheduleMTM();
            var group = new StudentGroupMTM { Id = Guid.NewGuid(), Name = "OldName", StudentsCount = 10 };
            mtm.Groups.Add(group);

            var vm = new GroupEditViewModel(mtm);
            vm.GroupId = group.Id.ToString();
            vm.GroupName = "NewName";
            vm.StudentsCount = 30;
            vm.SaveCommand.Execute(null);

            Assert.Single(mtm.Groups);
            Assert.Equal("NewName", mtm.Groups[0].Name);
            Assert.Equal(30, mtm.Groups[0].StudentsCount);
        }

        [Fact]
        public void TeacherEditViewModel_AddsNewTeacher()
        {
            var mtm = CreateEmptyScheduleMTM();
            var vm = new TeacherEditViewModel(mtm);

            vm.TeacherId = "new";
            vm.FirstName = "John";
            vm.LastName = "Doe";
            vm.Patronymic = "Smith";
            vm.TargetHours = 2;
            vm.SaveCommand.Execute(null);

            Assert.Single(mtm.Teachers);
            Assert.Equal("Doe John Smith", mtm.Teachers[0].Name);
            Assert.Equal(2, mtm.Teachers[0].TargetHours);
        }

        [Fact]
        public void RoomEditViewModel_AddsNewRoom()
        {
            var mtm = CreateEmptyScheduleMTM();
            var vm = new RoomEditViewModel(mtm);

            vm.RoomId = "new";
            vm.Name = "202";
            vm.Capacity = 40;
            vm.SaveCommand.Execute(null);

            Assert.Single(mtm.Rooms);
            Assert.Equal("202", mtm.Rooms[0].Name);
            Assert.Equal(40, mtm.Rooms[0].Capacity);
        }

        [Fact]
        public void RoomsViewModel_DeletesRoom()
        {
            var mtm = CreateEmptyScheduleMTM();
            var room = new RoomMTM { Id = Guid.NewGuid(), Name = "202", Capacity = 40 };
            mtm.Rooms.Add(room);

            var vm = new RoomsViewModel(mtm);
            Assert.Single(vm.Rooms);

            vm.DeleteRoomCommand.Execute(room);

            Assert.Empty(mtm.Rooms);
            Assert.Empty(vm.Rooms);
        }

        [Fact]
        public void TeachersViewModel_DeletesTeacher()
        {
            var mtm = CreateEmptyScheduleMTM();
            var t = new TeacherMTM { Id = Guid.NewGuid(), Name = "T1" };
            mtm.Teachers.Add(t);

            var vm = new TeachersViewModel(mtm);
            Assert.Single(vm.Teachers);

            vm.DeleteTeacherCommand.Execute(t);

            Assert.Empty(mtm.Teachers);
            Assert.Empty(vm.Teachers);
        }

        [Fact]
        public void ConstraintFormViewModel_AddsDailyLimitConstraintMTM()
        {
            var mtm = CreateEmptyScheduleMTM();

            var vm = new ConstraintFormViewModel(ConstraintOwnerType.Teacher, mtm.Config);
            vm.SelectedConstraintType = "Ліміт занять на день (DailyLimit)";
            vm.DailyLimitValue = 2;

            ConstraintMTM added = null;
            vm.OnConstraintAdded += c => added = c;

            vm.AddConstraintCommand.Execute(null);

            Assert.NotNull(added);
            Assert.IsType<DailyLimitConstraintMTM>(added);
            Assert.Equal(2, ((DailyLimitConstraintMTM)added).MaxLessonsPerDay);
        }

        [Fact]
        public void DisciplineEditViewModel_SavesDiscipline()
        {
            var mtm = CreateEmptyScheduleMTM();
            var vm = new DisciplineEditViewModel(mtm);

            vm.DisciplineId = "new";
            vm.Name = "History";
            vm.SaveCommand.Execute(null);

            Assert.Single(mtm.Disciplines);
            Assert.Equal("History", mtm.Disciplines[0].Name);
        }
    }
}
