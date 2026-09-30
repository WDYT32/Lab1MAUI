using Microsoft.Maui.Controls;
using Lab1MAUI.ViewModels;

namespace Lab1MAUI.Views;

public partial class StudentGroupsPage : ContentPage
{
    public StudentGroupsPage(StudentGroupsViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is StudentGroupsViewModel vm)
        {
            vm.LoadGroups();
        }
    }
}
