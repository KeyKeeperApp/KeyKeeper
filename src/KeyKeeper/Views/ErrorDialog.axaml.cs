using Avalonia.Controls;
using Avalonia.Interactivity;

namespace KeyKeeper.Views;

public partial class ErrorDialog : Window
{
    public ErrorDialog(string message)
    {
        InitializeComponent();
        MessageText.Text = message;
    }

    private void Ok_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }
}