using Avalonia.Controls;
using Avalonia.Interactivity;

namespace KeyKeeper.Views;

public partial class ErrorDialog : Window
{
    public ErrorDialog(string message)
    {
        InitializeComponent();
        MinWidth = 400;
        MinHeight = 200;
        MessageText.Text = message;
    }

    private void Ok_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }
}