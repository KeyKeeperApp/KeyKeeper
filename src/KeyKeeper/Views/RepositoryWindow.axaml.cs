using System;
using Avalonia.Controls;
using Avalonia.Interactivity;
using KeyKeeper.ViewModels;

namespace KeyKeeper.Views;

public partial class RepositoryWindow: Window
{
    public RepositoryWindow(RepositoryWindowViewModel model)
    {
        InitializeComponent();
        DataContext = model;
        model.ShowErrorPopup = async (string message) =>
        {
            await new ErrorDialog(message).ShowDialog(this);
        };
    }

    protected override void OnOpened(EventArgs e)
    {
        base.OnOpened(e);
    }

    private void SaveButton_Click(object sender, RoutedEventArgs args)
    {
        if (DataContext is RepositoryWindowViewModel vm && vm.CurrentPage is UnlockedRepositoryViewModel pageVm)
        {
            pageVm.Save();
        }
    }
}