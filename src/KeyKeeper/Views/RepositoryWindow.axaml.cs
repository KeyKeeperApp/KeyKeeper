using System;
using System.Threading.Tasks;
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

    private async void AddEntryButton_Click(object sender, RoutedEventArgs args)
    {
        if (DataContext is RepositoryWindowViewModel vm_ && vm_.CurrentPage is UnlockedRepositoryViewModel vm)
        {
            EntryEditWindow dialog = new();
            await dialog.ShowDialog(this);

            if (dialog.EditedEntry != null)
                vm.AddEntry(dialog.EditedEntry);
        }
    }
}