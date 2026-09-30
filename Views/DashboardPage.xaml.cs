using Microsoft.Maui.Controls;
using Lab1MAUI.ViewModels;
using Lab1MAUI.Models.Schedule;

namespace Lab1MAUI.Views;

public partial class DashboardPage : ContentPage
{
    public DashboardPage(DashboardViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is DashboardViewModel vm)
        {
            vm.RefreshStats();
        }
    }
}
