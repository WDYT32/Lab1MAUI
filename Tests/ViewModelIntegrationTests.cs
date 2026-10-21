using Lab1MAUI.Models.UI;
using Lab1MAUI.Models.UI.Participants;
using Lab1MAUI.Models.UI.Constraints;
using Lab1MAUI.Models.Schedule;
using Lab1MAUI.ViewModels;
using Lab1MAUI.ViewModels.Edit;
using Lab1MAUI.ViewModels.Participants;
using System;
using System.Linq;
using Xunit;

namespace Lab1MAUI.Tests
{
    public class ViewModelIntegrationTests
    {
        private ScheduleUI CreateEmptyScheduleUI()
        {
            var ui = new ScheduleUI();
            ui.Config.Days = 1;
            ui.Config.CellsPerDay = 5;
            return ui;
        }

        [Fact]
        public void GroupEditViewModel_AddsNewGroup()
        {
            var ui = CreateEmptyScheduleUI();
            var vm = new GroupEditViewModel(ui);

            vm.GroupId = "new";
            vm.Name = "TestGroup";
            vm.StudentsCount = 25;
            vm.SaveCommand.Execute(null);

            Assert.Single(ui.Groups);
            Assert.Equal("TestGroup", ui.Groups[0].Name);
            Assert.Equal(25, ui.Groups[0].StudentsCount);
        }

        [Fact]
        public void GroupEditViewModel_EditsExistingGroup()
        {
            var ui = CreateEmptyScheduleUI();
            var group = new StudentGroupUI { Id = Guid.NewGuid(), Name = "OldName", StudentsCount = 10 };
            ui.Groups.Add(group);

            var vm = new GroupEditViewModel(ui);
            vm.GroupId = group.Id.ToString();
            vm.Name = "NewName";
            vm.StudentsCount = 30;
            vm.SaveCommand.Execute(null);

            Assert.Single(ui.Groups);
            Assert.Equal("NewName", ui.Groups[0].Name);
            Assert.Equal(30, ui.Groups[0].StudentsCount);
        }

        [Fact]
        public void TeacherEditViewModel_AddsNewTeacher()
        {
            var ui = CreateEmptyScheduleUI();
            var vm = new TeacherEditViewModel(ui);

            vm.TeacherId = "new";
            vm.FirstName = "John";
            vm.LastName = "Doe";
            vm.Patronymic = "Smith";
            vm.TargetLessons = 2;
            vm.SaveCommand.Execute(null);

            Assert.Single(ui.Teachers);
            Assert.Equal("Doe John Smith", ui.Teachers[0].Name);
            Assert.Equal(2, ui.Teachers[0].TargetLessons);
        }

        [Fact]
        public void RoomEditViewModel_AddsNewRoom()
        {
            var ui = CreateEmptyScheduleUI();
            var vm = new RoomEditViewModel(ui);

            vm.RoomId = "new";
            vm.Name = "202";
            vm.Capacity = 40;
            vm.SaveCommand.Execute(null);

            Assert.Single(ui.Rooms);
            Assert.Equal("202", ui.Rooms[0].Name);
            Assert.Equal(40, ui.Rooms[0].Capacity);
        }

        [Fact]
        public void RoomsViewModel_DeletesRoom()
        {
            var ui = CreateEmptyScheduleUI();
            var room = new RoomUI { Id = Guid.NewGuid(), Name = "202", Capacity = 40 };
            ui.Rooms.Add(room);

            var vm = new RoomsViewModel(ui);
            Assert.Single(vm.Items);

            vm.DeleteCommand.Execute(room);

            Assert.Empty(ui.Rooms);
            Assert.Empty(vm.Items);
        }

        [Fact]
        public void TeachersViewModel_DeletesTeacher()
        {
            var ui = CreateEmptyScheduleUI();
            var t = new TeacherUI { Id = Guid.NewGuid(), Name = "T1" };
            ui.Teachers.Add(t);

            var vm = new TeachersViewModel(ui);
            Assert.Single(vm.Items);

            vm.DeleteCommand.Execute(t);

            Assert.Empty(ui.Teachers);
            Assert.Empty(vm.Items);
        }

        [Fact]
        public void ConstraintFormViewModel_AddsDailyLimitConstraintUI()
        {
            var ui = CreateEmptyScheduleUI();

            var vm = new ConstraintFormViewModel(ConstraintOwnerType.Teacher, ui.Config);
            vm.SelectType(FormConstraintType.DailyLimit);
            vm.DailyLimitValue = 2;

            IConstraintUI? added = null;
            vm.OnConstraintAdded += c => added = c;

            vm.AddConstraintCommand.Execute(null);

            Assert.NotNull(added);
            Assert.IsType<DailyLimitConstraintUI>(added);
            Assert.Equal(2, ((DailyLimitConstraintUI)added).MaxLessonsPerDay);
        }

        [Fact]
        public void DisciplineEditViewModel_SavesDiscipline()
        {
            var ui = CreateEmptyScheduleUI();
            var vm = new DisciplineEditViewModel(ui);

            vm.DisciplineId = "new";
            vm.Name = "History";
            vm.SaveCommand.Execute(null);

            Assert.Single(ui.Disciplines);
            Assert.Equal("History", ui.Disciplines[0].Name);
        }

        [Fact]
        public void SettingsViewModel_UpdatesConfigAndRaisesNotifications()
        {
            var ui = CreateEmptyScheduleUI();
            var vm = new SettingsViewModel(ui);

            var changedProperties = new System.Collections.Generic.List<string>();
            vm.PropertyChanged += (s, e) => changedProperties.Add(e.PropertyName);

            vm.Days = 6;
            Assert.Equal(6, ui.Config.Days);
            Assert.Contains("Days", changedProperties);
            Assert.Contains("TotalCells", changedProperties);

            changedProperties.Clear();
            vm.CellsPerDay = 6;
            Assert.Equal(6, ui.Config.CellsPerDay);
            Assert.Equal(36, vm.TotalCells);
            Assert.Contains("CellsPerDay", changedProperties);
            Assert.Contains("TotalCells", changedProperties);

            changedProperties.Clear();
            vm.MinLessonsPerDay = 2;
            Assert.Equal(2, ui.Config.MinLessonsPerDay);
            Assert.Contains("MinLessonsPerDay", changedProperties);

            changedProperties.Clear();
            vm.LessonDurationMinutes = 45;
            Assert.Equal(45, ui.Config.LessonDurationMinutes);
            Assert.Contains("LessonDurationMinutes", changedProperties);

            // Setting same value should not raise event
            changedProperties.Clear();
            vm.LessonDurationMinutes = 45;
            Assert.Empty(changedProperties);
        }
    }
}
