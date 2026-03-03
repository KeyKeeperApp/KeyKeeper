using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Input;
using KeyKeeper.PasswordStore;
using KeyKeeper.ViewModels;
using Avalonia.VisualTree;
using Avalonia.Controls.Presenters;

namespace KeyKeeper.Views;

public partial class RepositoryWindow: Window
{
    private bool allowClose;
    private bool closeConfirmationShown;

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

    private void SaveButton_Click(object sender, RoutedEventArgs args)
    {
        if (DataContext is RepositoryWindowViewModel vm && vm.CurrentPage is UnlockedRepositoryViewModel pageVm)
        {
            pageVm.Save();
        }
    }

    private void Entry_DoubleTapped(object sender, TappedEventArgs args)
    {
        if (args.Source is StyledElement s)
        {
            if (s.DataContext is PassStoreEntryPassword pwd)
            {
                Clipboard!.SetTextAsync(pwd.Password.Value);
                this.FindControlRecursive<ToastNotificationHost>("NotificationHost")?.Show("Password copied to clipboard");
            }
        }
    }

    private void EntryContextMenuItem_Click(object sender, RoutedEventArgs args) {
        if (args.Source is StyledElement s)
        {
            if (s.DataContext is PassStoreEntryPassword pwd)
            {
                if (s.Name == "entryCtxMenuCopyUsername")
                {
                    Clipboard!.SetTextAsync(pwd.Username.Value);
                    this.FindControlRecursive<ToastNotificationHost>("NotificationHost")?.Show("Username copied to clipboard");
                }
                else if (s.Name == "entryCtxMenuCopyPassword")
                {
                    Clipboard!.SetTextAsync(pwd.Password.Value);
                    this.FindControlRecursive<ToastNotificationHost>("NotificationHost")?.Show("Password copied to clipboard");
                }
                else if (s.Name == "entryCtxMenuDelete")
                {
                    if (s.DataContext is PassStoreEntryPassword entry)
                    {
                        if (DataContext is RepositoryWindowViewModel vm && vm.CurrentPage is UnlockedRepositoryViewModel pageVm)
                        {
                            pageVm.DeleteEntry(entry.Id);
                        }
                    }
                    this.FindControlRecursive<ToastNotificationHost>("NotificationHost")?.Show("Entry deleted");
                }
            }
        }
    }
}

public static class VisualTreeExtensions
{
    public static T? FindControlRecursive<T>(this Visual parent, string name) where T : Visual
    {
        return FindControlRecursiveInternal<T>(parent, name, 0);
    }

    private static T? FindControlRecursiveInternal<T>(Visual parent, string name, int depth) where T : Visual
    {
        if (parent == null)
            return null;

        if (parent is T t && parent.Name == name)
            return t;

        foreach (var child in parent.GetVisualChildren())
        {
            if (child == null)
                continue;

            var result = FindControlRecursiveInternal<T>(child, name, depth + 1);
            if (result != null)
                return result;
        }

        // Also check logical children if they're not in visual tree
        if (parent is ContentPresenter contentPresenter)
        {
            var content = contentPresenter.Content as Visual;
            if (content != null)
            {
                var result = FindControlRecursiveInternal<T>(content, name, depth + 1);
                if (result != null)
                    return result;
            }
        }

        return null;
    }
}