using Microsoft.Maui.Controls;
using Lab1MAUI.ViewModels.Participants;

namespace Lab1MAUI.Views.Participants;

public partial class RoomsPage : ContentPage
{
    public RoomsPage(RoomsViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
