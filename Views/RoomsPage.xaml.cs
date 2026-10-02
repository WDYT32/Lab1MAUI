using Microsoft.Maui.Controls;
using Lab1MAUI.ViewModels;

namespace Lab1MAUI.Views;

public partial class RoomsPage : ContentPage
{
    public RoomsPage(RoomsViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
