using Microsoft.Maui.Controls;
using Lab1MAUI.ViewModels;

namespace Lab1MAUI.Views;

public partial class DisciplineEditPage : ContentPage
{
    public DisciplineEditPage(DisciplineEditViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
