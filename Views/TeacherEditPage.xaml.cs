using Microsoft.Maui.Controls;
using Lab1MAUI.ViewModels;

namespace Lab1MAUI.Views;

public partial class TeacherEditPage : ContentPage
{
    public TeacherEditPage(TeacherEditViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
