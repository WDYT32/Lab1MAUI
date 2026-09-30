using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Lab1MAUI.ViewModels;
using Lab1MAUI.Models.Schedule;

namespace Lab1MAUI.Tests;

public class ComprehensiveJourneyTests
{
    public ComprehensiveJourneyTests()
    {
        // Блокуємо реальну навігацію для тестів
        BaseViewModel.NavigationAction = (route) => Task.CompletedTask;
    }

    [Fact]
    public async Task UserJourney_AddConfigureDelete_StrictScheduleValidation()
    {
        // 1. Початковий стан
        var schedule = new Schedule();
        // Налаштовуємо конфіг для передбачуваності (2 дні по 2 пари = 4 клітинки)
        schedule.Config.Days = 2;
        schedule.Config.CellsPerDay = 2;

        // 2. Створення дисциплін
        var d1Edit = new DisciplineEditViewModel(schedule) { DisciplineId = "new", Name = "Math" };
        d1Edit.SaveCommand.Execute(null);

        var d2Edit = new DisciplineEditViewModel(schedule) { DisciplineId = "new", Name = "Physics" };
        d2Edit.SaveCommand.Execute(null);
        
        var math = schedule.Disciplines.First(d => d.Name == "Math");
        var physics = schedule.Disciplines.First(d => d.Name == "Physics");

        // 3. Створення викладачів та налаштування компетенцій
        var t1Edit = new TeacherEditViewModel(schedule) { TeacherId = "new", FirstName = "Alan", LastName = "Turing" };
        t1Edit.DisciplineSelection.SearchQuery = "Math";
        t1Edit.DisciplineSelection.SelectCommand.Execute(t1Edit.DisciplineSelection.FilteredDisciplines.First());
        t1Edit.SaveCommand.Execute(null);

        var t2Edit = new TeacherEditViewModel(schedule) { TeacherId = "new", FirstName = "Isaac", LastName = "Newton" };
        t2Edit.DisciplineSelection.SearchQuery = "Physics";
        t2Edit.DisciplineSelection.SelectCommand.Execute(t2Edit.DisciplineSelection.FilteredDisciplines.First());
        t2Edit.SaveCommand.Execute(null);

        var turing = schedule.Teachers.First(t => t.LastName == "Turing");
        var newton = schedule.Teachers.First(t => t.LastName == "Newton");

        // Жорстко фіксуємо TargetHours, щоб не було рандому при генерації
        turing.Grid.TargetHours = 10;
        newton.Grid.TargetHours = 10;

        // 4. Створення групи та додавання вимог
        var gEdit = new GroupEditViewModel(schedule) { GroupId = "new", GroupName = "CS-101" };
        
        // Додаємо математику (2 години)
        gEdit.DisciplineSelection.SearchQuery = "Math";
        gEdit.DisciplineSelection.SelectCommand.Execute(gEdit.DisciplineSelection.FilteredDisciplines.First());
        gEdit.Requirements.First(r => r.Model.DisciplineId == math.Id).RequiredHours = 2;

        // Додаємо фізику (1 година)
        gEdit.DisciplineSelection.SearchQuery = "Physics";
        gEdit.DisciplineSelection.SelectCommand.Execute(gEdit.DisciplineSelection.FilteredDisciplines.First());
        gEdit.Requirements.First(r => r.Model.DisciplineId == physics.Id).RequiredHours = 1;

        gEdit.SaveCommand.Execute(null);

        var group = schedule.Groups.First(g => g.Name == "CS-101");
        group.Grid.TargetHours = 10;

        // 5. Перша генерація розкладу
        var dashboardVM = new DashboardViewModel(schedule);
        dashboardVM.GenerateScheduleCommand.Execute(null);
        await WaitForGeneration(dashboardVM);

        // 6. СТРОГА ПЕРЕВІРКА ПЕРШОГО РЕЗУЛЬТАТУ
        // Math (2 год) має бути розподілено Тьюрінгу. Physics (1 год) - Ньютону.
        // Пріоритет (порядок списку): Тьюрінг перший, Ньютон другий. Math перша, Physics друга.
        var resultVM = new ScheduleResultViewModel(schedule);
        
        Assert.Single(resultVM.GroupSchedules);
        var cs101Schedule = resultVM.GroupSchedules[0];
        Assert.Equal("CS-101", cs101Schedule.GroupName);
        Assert.Equal(2, cs101Schedule.Days.Count); // Day 0 and Day 1
        Assert.Equal(2, cs101Schedule.Days[0].Lessons.Count); // 2 Math on Day 0
        Assert.Single(cs101Schedule.Days[1].Lessons); // 1 Physics on Day 1

        Assert.Equal("Math", cs101Schedule.Days[0].Lessons[0].SubjectName);
        Assert.Equal("Turing Alan", cs101Schedule.Days[0].Lessons[0].TeacherName);
        Assert.Equal("1", cs101Schedule.Days[0].Lessons[0].TimeSlotName); 

        Assert.Equal("Physics", cs101Schedule.Days[0].Lessons[1].SubjectName);
        Assert.Equal("Newton Isaac", cs101Schedule.Days[0].Lessons[1].TeacherName);
        Assert.Equal("2", cs101Schedule.Days[0].Lessons[1].TimeSlotName); 

        Assert.Equal("Math", cs101Schedule.Days[1].Lessons[0].SubjectName);
        Assert.Equal("Turing Alan", cs101Schedule.Days[1].Lessons[0].TeacherName);
        Assert.Equal("1", cs101Schedule.Days[1].Lessons[0].TimeSlotName);

        // 7. РЕДАГУВАННЯ (Зміна конфігурації)
        // Видаляємо Math з вимог групи CS-101
        var gEdit2 = new GroupEditViewModel(schedule) { GroupId = group.Id.ToString() };
        var mathReq = gEdit2.Requirements.First(r => r.Model.DisciplineId == math.Id);
        gEdit2.DeleteRequirementCommand.Execute(mathReq);
        gEdit2.SaveCommand.Execute(null);

        // 8. ДРУГА ГЕНЕРАЦІЯ
        dashboardVM.GenerateScheduleCommand.Execute(null);
        await WaitForGeneration(dashboardVM);

        // 9. СТРОГА ПЕРЕВІРКА ДРУГОГО РЕЗУЛЬТАТУ
        // Тепер у розкладі має бути ЛИШЕ Physics (1 год) у Ньютона
        var resultVM2 = new ScheduleResultViewModel(schedule);
        
        Assert.Single(resultVM2.GroupSchedules);
        var cs101Schedule2 = resultVM2.GroupSchedules[0];
        Assert.Single(cs101Schedule2.Days); // Тільки Day 0
        Assert.Single(cs101Schedule2.Days[0].Lessons); // Тільки 1 урок

        // Він займе ПЕРШУ вільну комірку (i=0)
        Assert.Equal("Physics", cs101Schedule2.Days[0].Lessons[0].SubjectName);
        Assert.Equal("Newton Isaac", cs101Schedule2.Days[0].Lessons[0].TeacherName);
        Assert.Equal("1", cs101Schedule2.Days[0].Lessons[0].TimeSlotName);
    }

    private async Task WaitForGeneration(DashboardViewModel dashboardVM)
    {
        int maxWait = 50;
        while (dashboardVM.IsGenerating && maxWait > 0)
        {
            await Task.Delay(100);
            maxWait--;
        }
        await Task.Delay(100);
    }
}
