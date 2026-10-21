using Microsoft.Maui.Controls;
using Lab1MAUI.ViewModels;
using Lab1MAUI.ViewModels.Edit;

namespace Lab1MAUI.Views.Edit;

public partial class GroupEditPage : ContentPage
{
    public GroupEditPage(GroupEditViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
