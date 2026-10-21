using Microsoft.Maui.Controls;
using Lab1MAUI.ViewModels.Participants;

namespace Lab1MAUI.Views.Participants;

public partial class DisciplinesPage : ContentPage
{
    public DisciplinesPage(DisciplinesViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
