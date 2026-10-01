using Microsoft.Maui.Controls;
using Lab1MAUI.ViewModels;

namespace Lab1MAUI.Views;

public partial class RoomEditPage : ContentPage
{
    public RoomEditPage(RoomEditViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
