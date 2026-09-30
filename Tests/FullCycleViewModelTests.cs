using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Lab1MAUI.ViewModels;
using Lab1MAUI.Models.Schedule;

namespace Tests
{
    public class FullCycleViewModelTests
    {
        public FullCycleViewModelTests()
        {
            // Блокуємо реальну навігацію для тестів
            BaseViewModel.NavigationAction = (route) => Task.CompletedTask;
        }

        [Fact]
        public async Task Test_FullCycle_AddEntities_And_GenerateSchedule()
        {
            var schedule = new Schedule();

            // 1. Додавання дисциплін
            var disciplinesVM = new DisciplinesViewModel(schedule);
            
            var mathEditVM = new DisciplineEditViewModel(schedule) { DisciplineId = "new", Name = "Math [Лк]" };
            mathEditVM.SaveCommand.Execute(null);

            var progEditVM = new DisciplineEditViewModel(schedule) { DisciplineId = "new", Name = "Programming [Пр]" };
            progEditVM.SaveCommand.Execute(null);

            Assert.Equal(2, schedule.Disciplines.Count);
            var math = schedule.Disciplines.First(d => d.Name == "Math [Лк]");
            var prog = schedule.Disciplines.First(d => d.Name == "Programming [Пр]");

            // 2. Додавання викладача з компетенціями
            var teacherEditVM = new TeacherEditViewModel(schedule);
            teacherEditVM.TeacherId = "new";
            teacherEditVM.FirstName = "John";
            teacherEditVM.LastName = "Doe";
            teacherEditVM.TargetHours = 20;

            // Імітація вибору дисципліни через SelectionViewModel
            teacherEditVM.DisciplineSelection.SearchQuery = "Math"; // пошук
            teacherEditVM.DisciplineSelection.SelectCommand.Execute(teacherEditVM.DisciplineSelection.FilteredDisciplines.First());
            
            teacherEditVM.SaveCommand.Execute(null);
            
            Assert.Single(schedule.Teachers);
            Assert.Single(schedule.Teachers[0].Competences);
            Assert.Equal(math.Id, schedule.Teachers[0].Competences[0].DisciplineId);

            // 3. Додавання групи з вимогами
            var groupEditVM = new GroupEditViewModel(schedule);
            groupEditVM.GroupId = "new";
            groupEditVM.GroupName = "Group-1";

            groupEditVM.DisciplineSelection.SearchQuery = "Math";
            groupEditVM.DisciplineSelection.SelectCommand.Execute(groupEditVM.DisciplineSelection.FilteredDisciplines.First());
            
            groupEditVM.SaveCommand.Execute(null);

            Assert.Single(schedule.Groups);
            Assert.Single(schedule.Groups[0].Requirements);
            Assert.Equal(math.Id, schedule.Groups[0].Requirements[0].DisciplineId);

            // 4. Імітація запуску генерації на Dashboard
            var dashboardVM = new DashboardViewModel(schedule);
            dashboardVM.GenerateScheduleCommand.Execute(null);

            // Оскільки команда є async void (або async Task, яка не await-иться напряму через ICommand),
            // ми чекаємо завершення генерації.
            int maxWait = 50; // 50 * 100ms = 5 sec
            while (dashboardVM.IsGenerating && maxWait > 0)
            {
                await Task.Delay(100);
                maxWait--;
            }

            // Щоб упевнитися, що контекст виконався:
            await Task.Delay(100);

            // 5. Перевірка результатів
            Assert.NotEmpty(schedule.Lessons);
            
            // За замовчуванням GroupRequirement створюється з RequiredHours = 2
            // Перевіряємо, що згенеровано 2 уроки математики
            Assert.Equal(2, schedule.Lessons.Count(l => l.DisciplineId == math.Id));
            Assert.Equal(schedule.Groups[0].Id, schedule.Lessons[0].StudentGroupId);
            Assert.Equal(schedule.Teachers[0].Id, schedule.Lessons[0].TeacherId);
            
            // ПЕРЕВІРКА, ЩО УРОКИ ДІЙСНО ПОТРАПИЛИ В РОЗКЛАД (Grid)
            int lessonsInGrid = schedule.Groups[0].Grid.Cells.Count(c => c != null);
            Assert.Equal(2, lessonsInGrid);

            // 6. Перевірка View Model результатів
            var resultVM = new ScheduleResultViewModel(schedule);
            Assert.NotEmpty(resultVM.GroupSchedules);
            Assert.Equal(2, resultVM.GroupSchedules[0].Days.Count);

            // 7. Перевірка подвійної генерації (щоб виключити стан "привидів" у Registry)
            dashboardVM.GenerateScheduleCommand.Execute(null);
            maxWait = 50;
            while (dashboardVM.IsGenerating && maxWait > 0)
            {
                await Task.Delay(100);
                maxWait--;
            }
            await Task.Delay(100);

            var secondResultVM = new ScheduleResultViewModel(schedule);
            Assert.NotEmpty(secondResultVM.GroupSchedules);
            Assert.Equal(2, secondResultVM.GroupSchedules[0].Days.Count);
        }
    }
}
