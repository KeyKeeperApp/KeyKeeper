using System;
using Avalonia.Controls;
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
}