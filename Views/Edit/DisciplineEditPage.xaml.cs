using Microsoft.Maui.Controls;
using Lab1MAUI.ViewModels;
using Lab1MAUI.ViewModels.Edit;

namespace Lab1MAUI.Views.Edit;

public partial class DisciplineEditPage : ContentPage
{
    public DisciplineEditPage(DisciplineEditViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
