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
            var listVm = new StudentGroupsViewModel(ui);
            var vm = new GroupEditViewModel(ui);

            vm.ApplyQueryAttributes(new System.Collections.Generic.Dictionary<string, object>
            {
                { "Entity", new StudentGroupUI() }
            });
            vm.Name = "TestGroup";
            vm.StudentsCount = 25;
            vm.SaveCommand.Execute(null);

            listVm.ApplyQueryAttributes(new System.Collections.Generic.Dictionary<string, object>
            {
                { "SavedItem", vm.Entity }
            });

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

            var listVm = new StudentGroupsViewModel(ui);
            var vm = new GroupEditViewModel(ui);
            vm.ApplyQueryAttributes(new System.Collections.Generic.Dictionary<string, object>
            {
                { "Entity", group }
            });
            vm.Name = "NewName";
            vm.StudentsCount = 30;
            vm.SaveCommand.Execute(null);

            listVm.ApplyQueryAttributes(new System.Collections.Generic.Dictionary<string, object>
            {
                { "SavedItem", vm.Entity }
            });

            Assert.Single(ui.Groups);
            Assert.Equal("NewName", ui.Groups[0].Name);
            Assert.Equal(30, ui.Groups[0].StudentsCount);
        }

        [Fact]
        public void TeacherEditViewModel_AddsNewTeacher()
        {
            var ui = CreateEmptyScheduleUI();
            var listVm = new TeachersViewModel(ui);
            var vm = new TeacherEditViewModel(ui);

            vm.ApplyQueryAttributes(new System.Collections.Generic.Dictionary<string, object>
            {
                { "Entity", new TeacherUI() }
            });
            vm.FirstName = "John";
            vm.LastName = "Doe";
            vm.Patronymic = "Smith";
            vm.TargetLessons = 2;
            vm.SaveCommand.Execute(null);

            listVm.ApplyQueryAttributes(new System.Collections.Generic.Dictionary<string, object>
            {
                { "SavedItem", vm.Entity }
            });

            Assert.Single(ui.Teachers);
            Assert.Equal("Doe John Smith", ui.Teachers[0].Name);
            Assert.Equal(2, ui.Teachers[0].TargetLessons);
        }

        [Fact]
        public void RoomEditViewModel_AddsNewRoom()
        {
            var ui = CreateEmptyScheduleUI();
            var listVm = new RoomsViewModel(ui);
            var vm = new RoomEditViewModel();

            vm.ApplyQueryAttributes(new System.Collections.Generic.Dictionary<string, object>
            {
                { "Entity", new RoomUI() }
            });
            vm.Name = "202";
            vm.Capacity = 40;
            vm.SaveCommand.Execute(null);

            listVm.ApplyQueryAttributes(new System.Collections.Generic.Dictionary<string, object>
            {
                { "SavedItem", vm.Entity }
            });

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
            var vm = new ConstraintFormViewModel(ConstraintOwnerType.Teacher);
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
            var listVm = new DisciplinesViewModel(ui);
            var vm = new DisciplineEditViewModel();

            vm.ApplyQueryAttributes(new System.Collections.Generic.Dictionary<string, object>
            {
                { "Entity", new DisciplineUI() }
            });
            vm.Name = "History";
            vm.SaveCommand.Execute(null);

            listVm.ApplyQueryAttributes(new System.Collections.Generic.Dictionary<string, object>
            {
                { "SavedItem", vm.Entity }
            });

            Assert.Single(ui.Disciplines);
            Assert.Equal("History", ui.Disciplines[0].Name);
        }

        [Fact]
        public void SettingsViewModel_UpdatesConfigAndRaisesNotifications()
        {
            var ui = CreateEmptyScheduleUI();
            var vm = new SettingsViewModel(ui);

            var changedProperties = new System.Collections.Generic.List<string>();
            vm.PropertyChanged += (s, e) => { if (e.PropertyName != null) changedProperties.Add(e.PropertyName); };

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

        [Fact]
        public void BaseEditViewModel_ApplyQueryAttributes_LoadsEntityDirectly()
        {
            var ui = CreateEmptyScheduleUI();
            var teacher = new TeacherUI { Id = Guid.NewGuid(), FirstName = "John", LastName = "Doe", TargetLessons = 4 };
            var vm = new TeacherEditViewModel(ui);

            vm.ApplyQueryAttributes(new System.Collections.Generic.Dictionary<string, object>
            {
                { "Entity", teacher }
            });

            Assert.Same(teacher, vm.Entity);
            Assert.Equal("John", vm.FirstName);
            Assert.Equal("Doe", vm.LastName);
            Assert.Equal(4, vm.TargetLessons);
        }

        [Fact]
        public void BaseEditViewModel_ApplyQueryAttributes_SetsEntity()
        {
            var ui = CreateEmptyScheduleUI();
            var vm = new TeacherEditViewModel(ui);
            var newTeacher = new TeacherUI();

            vm.ApplyQueryAttributes(new System.Collections.Generic.Dictionary<string, object>
            {
                { "Entity", newTeacher }
            });

            Assert.Same(newTeacher, vm.Entity);
            Assert.Empty(vm.FirstName);
            Assert.Empty(vm.LastName);
        }

        [Fact]
        public void BaseListViewModel_ApplyQueryAttributes_AddsNewSavedItem()
        {
            var ui = CreateEmptyScheduleUI();
            var listVm = new TeachersViewModel(ui);
            var newTeacher = new TeacherUI { Id = Guid.NewGuid(), FirstName = "Anna", LastName = "Karenina" };

            listVm.ApplyQueryAttributes(new System.Collections.Generic.Dictionary<string, object>
            {
                { "SavedItem", newTeacher }
            });

            Assert.Single(listVm.Items);
            Assert.Same(newTeacher, listVm.Items[0]);
        }

        [Fact]
        public void BaseListViewModel_ApplyQueryAttributes_UpdatesExistingSavedItem()
        {
            var ui = CreateEmptyScheduleUI();
            var teacher = new TeacherUI { Id = Guid.NewGuid(), FirstName = "Old", LastName = "Name" };
            ui.Teachers.Add(teacher);

            var listVm = new TeachersViewModel(ui);
            teacher.FirstName = "Updated";

            listVm.ApplyQueryAttributes(new System.Collections.Generic.Dictionary<string, object>
            {
                { "SavedItem", teacher }
            });

            Assert.Single(listVm.Items);
            Assert.Equal("Updated", listVm.Items[0].FirstName);
        }

        [Fact]
        public void BaseEditViewModel_GoBackCommand_CanExecute()
        {
            var vm = new RoomEditViewModel();

            Assert.NotNull(vm.GoBackCommand);
            Assert.True(vm.GoBackCommand.CanExecute(null));
        }

        [Fact]
        public void BaseEditViewModel_ApplyQueryAttributes_RemovesEntityFromQuery()
        {
            var vm = new RoomEditViewModel();
            var room = new RoomUI { Name = "101", Capacity = 20 };
            var query = new System.Collections.Generic.Dictionary<string, object>
            {
                { "Entity", room }
            };

            vm.ApplyQueryAttributes(query);

            Assert.Same(room, vm.Entity);
            Assert.False(query.ContainsKey("Entity"));
        }
    }
}
