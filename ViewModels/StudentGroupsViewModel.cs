using System;
using System.Collections.ObjectModel;
using System.Windows.Input;
using Lab1MAUI.Models.Schedule;
using Microsoft.Maui.Controls;

namespace Lab1MAUI.ViewModels;

public class StudentGroupsViewModel : BaseViewModel
{
    public ObservableCollection<StudentGroup> Groups { get; } = new();

    private string _groupName = string.Empty;
    public string GroupName
    {
        get => _groupName;
        set
        {
            if (SetProperty(ref _groupName, value))
                ((Command)AddGroupCommand).ChangeCanExecute();
        }
    }

    public ICommand AddGroupCommand { get; }

    public StudentGroupsViewModel()
    {
        AddGroupCommand = new Command(AddGroup, () => !string.IsNullOrWhiteSpace(GroupName));
    }

    private void AddGroup()
    {
        Groups.Add(new StudentGroup
        {
            Name = this.GroupName,
            CreationDate = DateTime.Now // Внутрішньо виміряна дата створення
        });

        GroupName = string.Empty;
    }
}
