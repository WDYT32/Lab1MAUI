using Microsoft.Maui.Controls;
using Lab1MAUI.ViewModels;

namespace Lab1MAUI.Views;

public partial class GroupEditPage : ContentPage
{
    public GroupEditPage(GroupEditViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
