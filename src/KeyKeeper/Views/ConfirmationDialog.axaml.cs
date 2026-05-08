using Avalonia.Controls;
using Avalonia.Interactivity;

namespace KeyKeeper.Views;

public partial class ConfirmationDialog : Window
{
    public bool Confirmed { get; private set; }

    public ConfirmationDialog()
    {
        InitializeComponent();
    }

    public void SetContent(string title, string message)
    {
        TitleText.Text = title;
        MessageText.Text = message;
        ConfirmButton.Content = "Confirm";
    }

    public void SetContent(string title, string message, string confirmButtonText)
    {
        TitleText.Text = title;
        MessageText.Text = message;
        ConfirmButton.Content = confirmButtonText;
    }

    private void CancelButton_Click(object? sender, RoutedEventArgs e)
    {
        Confirmed = false;
        Close();
    }

    private void ConfirmButton_Click(object? sender, RoutedEventArgs e)
    {
        Confirmed = true;
        Close();
    }
}
