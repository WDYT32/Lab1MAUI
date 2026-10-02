using Lab1MAUI.Models.MTM;
using Lab1MAUI.Models.MTM.Constraints;
using Lab1MAUI.Models.Schedule;
using Microsoft.Maui.Controls;
using System;
using System.Windows.Input;

namespace Lab1MAUI.ViewModels;

public class DashboardViewModel : BaseViewModel
{
    private readonly ScheduleMTM _scheduleMTM;

    public int TotalTeachers => _scheduleMTM.Teachers.Count;
    public int TotalGroups => _scheduleMTM.Groups.Count;
    public int TotalDisciplines => _scheduleMTM.Disciplines.Count;

    private bool _isGenerating;
    public bool IsGenerating
    {
        get => _isGenerating;
        set => SetProperty(ref _isGenerating, value);
    }

    public ICommand GenerateScheduleCommand { get; }

    public DashboardViewModel(ScheduleMTM scheduleMTM)
    {
        _scheduleMTM = scheduleMTM;

        // Команда активна тільки якщо не відбувається генерація
        GenerateScheduleCommand = new Command(async () => await GenerateScheduleAsync(), () => !IsGenerating);
    }

    public void RefreshStats()
    {
        OnPropertyChanged(nameof(TotalTeachers));
        OnPropertyChanged(nameof(TotalGroups));
        OnPropertyChanged(nameof(TotalDisciplines));
    }

    private async System.Threading.Tasks.Task GenerateScheduleAsync()
    {
        IsGenerating = true;
        ((Command)GenerateScheduleCommand).ChangeCanExecute();

        // Імітація роботи алгоритму (щоб UI міг показати лоадер)
        await System.Threading.Tasks.Task.Delay(500);

        // Запускаємо генерацію через MTM — кожен раз новий Schedule
        bool success = _scheduleMTM.TryGenerate(out string errorMessage);

        IsGenerating = false;
        ((Command)GenerateScheduleCommand).ChangeCanExecute();

        // Переходимо до результатів незалежно від успіху — там покажемо що розмістилось
        await BaseViewModel.NavigationAction("ScheduleResultPage");
    }
}
