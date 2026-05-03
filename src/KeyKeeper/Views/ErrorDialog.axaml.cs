using System;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;

namespace KeyKeeper.Views;

public partial class ErrorDialog : Window
{
    public ErrorDialog(string message, string title = "Oops! Something went wrong")
    {
        InitializeComponent();
        MessageText.Text = message;
        MessageTitle.Text = title;
    }

    private void Ok_Click(object? sender, RoutedEventArgs e)
    {
        Close();
    }

    protected override void OnOpened(EventArgs e)
    {
        base.OnOpened(e);
        OkButton.Focus();
    }
}
