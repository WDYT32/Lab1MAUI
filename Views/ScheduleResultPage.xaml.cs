using Microsoft.Maui.Controls;
using Lab1MAUI.ViewModels;
using Lab1MAUI.Models.Schedule;

namespace Lab1MAUI.Views;

public partial class ScheduleResultPage : ContentPage
{
    public ScheduleResultPage(ScheduleResultViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
