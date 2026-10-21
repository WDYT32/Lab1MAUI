using Lab1MAUI.Models.UI;
using Lab1MAUI.Models.UI.Constraints;
using Lab1MAUI.Models.Schedule;
using Microsoft.Maui.Controls;
using System;
using System.Windows.Input;

namespace Lab1MAUI.ViewModels;

public class DashboardViewModel : BaseViewModel
{
    private readonly ScheduleUI _scheduleUI;

    public int TotalTeachers => _scheduleUI.Teachers.Count;
    public int TotalGroups => _scheduleUI.Groups.Count;
    public int TotalDisciplines => _scheduleUI.Disciplines.Count;

    public bool IsGenerating
    {
        get;
        set => SetProperty(ref field, value);
    }

    public ICommand GenerateScheduleCommand { get; }

    public DashboardViewModel(ScheduleUI scheduleUI)
    {
        _scheduleUI = scheduleUI;

        _scheduleUI.Teachers.CollectionChanged += (s, e) => OnPropertyChanged(nameof(TotalTeachers));
        _scheduleUI.Groups.CollectionChanged += (s, e) => OnPropertyChanged(nameof(TotalGroups));
        _scheduleUI.Disciplines.CollectionChanged += (s, e) => OnPropertyChanged(nameof(TotalDisciplines));

        GenerateScheduleCommand = new Command(async () => await GenerateScheduleAsync(), () => !IsGenerating);
    }

    private async Task GenerateScheduleAsync()
    {
        IsGenerating = true;
        ((Command)GenerateScheduleCommand).ChangeCanExecute();

        await Task.Delay(500);

        bool success = _scheduleUI.TryGenerate(out string errorMessage);

        IsGenerating = false;
        ((Command)GenerateScheduleCommand).ChangeCanExecute();

        if (Shell.Current != null)
            await Shell.Current.GoToAsync("ScheduleResultPage");
    }
}
