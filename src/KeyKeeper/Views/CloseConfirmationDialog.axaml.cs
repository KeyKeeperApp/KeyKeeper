using Avalonia.Controls;
using Avalonia.Interactivity;

namespace KeyKeeper.Views;

public enum CloseConfirmationResult
{
    Save,
    Discard,
    Cancel,
}

public partial class CloseConfirmationDialog : Window
{
    private bool closingWithResult;

    public CloseConfirmationDialog()
    {
        InitializeComponent();
    }

    protected override void OnClosing(WindowClosingEventArgs e)
    {
        if (!closingWithResult)
        {
            e.Cancel = true;
            closingWithResult = true;
            Close(CloseConfirmationResult.Cancel);
            return;
        }

        base.OnClosing(e);
    }

    private void Save_Click(object? sender, RoutedEventArgs e)
    {
        closingWithResult = true;
        Close(CloseConfirmationResult.Save);
    }

    private void Discard_Click(object? sender, RoutedEventArgs e)
    {
        closingWithResult = true;
        Close(CloseConfirmationResult.Discard);
    }

    private void Cancel_Click(object? sender, RoutedEventArgs e)
    {
        closingWithResult = true;
        Close(CloseConfirmationResult.Cancel);
    }
}