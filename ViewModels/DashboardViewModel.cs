using System.Threading.Tasks;
using System.Windows.Input;
using Lab1MAUI.Models.Schedule;
using Microsoft.Maui.Controls;

namespace Lab1MAUI.ViewModels;

public class DashboardViewModel : BaseViewModel
{
    private readonly Schedule _schedule;

    public int TotalTeachers => _schedule.Teachers.Count;
    public int TotalGroups => _schedule.Groups.Count;

    private bool _isGenerating;
    public bool IsGenerating
    {
        get => _isGenerating;
        set => SetProperty(ref _isGenerating, value);
    }

    public ICommand GenerateScheduleCommand { get; }

    public DashboardViewModel(Schedule schedule)
    {
        _schedule = schedule;
        
        // Команда активна тільки якщо не відбувається генерація
        GenerateScheduleCommand = new Command(async () => await GenerateScheduleAsync(), () => !IsGenerating);
    }

    private async Task GenerateScheduleAsync()
    {
        IsGenerating = true;
        ((Command)GenerateScheduleCommand).ChangeCanExecute();

        // Імітація роботи алгоритму (щоб UI міг показати лоадер)
        await Task.Delay(500); 
        
        // Виклик нашого потужного ядра
        _schedule.Generate();

        IsGenerating = false;
        ((Command)GenerateScheduleCommand).ChangeCanExecute();

        // Перехід на сторінку результатів через Shell
        await Shell.Current.GoToAsync("ScheduleResultPage");
    }
}
