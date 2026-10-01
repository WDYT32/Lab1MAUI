using Microsoft.Maui.Controls;
using Lab1MAUI.ViewModels;
using Lab1MAUI.Models.Schedule;
using Lab1MAUI.Models.Schedule.Participants;
using Lab1MAUI.Models.Schedule.Constraints;

namespace Lab1MAUI.Views;

public partial class ScheduleResultPage : ContentPage
{
    public ScheduleResultPage(ScheduleResultViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
