using Microsoft.Maui.Controls;
using Lab1MAUI.ViewModels;

namespace Lab1MAUI.Views;

public partial class TeachersPage : ContentPage
{
    public TeachersPage(TeachersViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
