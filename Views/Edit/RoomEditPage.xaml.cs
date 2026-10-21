using Microsoft.Maui.Controls;
using Lab1MAUI.ViewModels;
using Lab1MAUI.ViewModels.Edit;

namespace Lab1MAUI.Views.Edit;

public partial class RoomEditPage : ContentPage
{
    public RoomEditPage(RoomEditViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
