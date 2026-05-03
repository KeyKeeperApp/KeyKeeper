using Avalonia.Controls;
using Avalonia.Interactivity;

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
}