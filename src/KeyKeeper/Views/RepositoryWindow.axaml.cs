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

public partial class RepositoryWindow : Window
{
    private bool allowClose;
    private bool closeConfirmationShown;

    public RepositoryWindow(RepositoryWindowViewModel model)
    {
        InitializeComponent();

        MinWidth = 650;
        MinHeight = 500;

        DataContext = model;
        model.ShowErrorPopup = async (string message) =>
        {
            await new ErrorDialog(message).ShowDialog(this);
        };
    }

    protected override void OnOpened(EventArgs e)
    {
        base.OnOpened(e);
        AddHandler(PointerMovedEvent, OnUserActivity, RoutingStrategies.Tunnel);
        AddHandler(PointerPressedEvent, OnUserActivity, RoutingStrategies.Tunnel);
        AddHandler(KeyDownEvent, OnUserActivity, RoutingStrategies.Tunnel);
    }

    private void OnUserActivity(object? sender, RoutedEventArgs e)
    {
        if (DataContext is RepositoryWindowViewModel vm)
            vm.ResetLockTimer();
    }

    private async void RepositoryWindow_Closing(object? sender, WindowClosingEventArgs e)
    {
        if (allowClose || closeConfirmationShown)
        {
            return;
        }

        if (DataContext is RepositoryWindowViewModel checkVm)
        {
            if ((checkVm.CurrentPage is UnlockedRepositoryViewModel unlockedVm &&
                !unlockedVm.HasUnsavedChanges)
                || checkVm.CurrentPage is LockedRepositoryViewModel)
            {
                allowClose = true;
                return;
            }
        }

        e.Cancel = true;
        closeConfirmationShown = true;

        var dialog = new CloseConfirmationDialog();
        var result = await dialog.ShowDialog<CloseConfirmationResult?>(this);

        closeConfirmationShown = false;

        if (result == null || result == CloseConfirmationResult.Cancel)
        {
            return;
        }

        if (result == CloseConfirmationResult.Save &&
            DataContext is RepositoryWindowViewModel vm &&
            vm.CurrentPage is UnlockedRepositoryViewModel pageVm)
        {
            pageVm.Save();
        }

        allowClose = true;
        Close();
    }

    private async void AddEntryButton_Click(object sender, RoutedEventArgs args)
    {
        if (DataContext is RepositoryWindowViewModel vm_ && vm_.CurrentPage is UnlockedRepositoryViewModel vm)
        {
            EntryEditWindow dialog = new();

            vm_.StopLockTimer();

            await dialog.ShowDialog(this);

            vm_.StartLockTimer();

            if (dialog.EditedEntry != null)
                vm.AddEntry(dialog.EditedEntry);
        }
    }

    private async void EditEntryButton_Click(object sender, RoutedEventArgs args)
    {
        if (DataContext is RepositoryWindowViewModel vm_ && vm_.CurrentPage is UnlockedRepositoryViewModel vm)
        {
            var listBox = this.FindControlRecursive<ListBox>("PasswordsListBox");
            if (listBox == null)
            {
                this.FindControlRecursive<ToastNotificationHost>("NotificationHost")?.Show("ListBox not found");
                return;
            }

            var selectedEntry = listBox.SelectedItem as PassStoreEntryPassword;
            if (selectedEntry == null)
            {
                this.FindControlRecursive<ToastNotificationHost>("NotificationHost")?.Show("No entry selected");
                return;
            }

            EntryEditWindow dialog = new();
            dialog.SetEntry(selectedEntry);

            vm_.StopLockTimer();

            await dialog.ShowDialog(this);

            vm_.StartLockTimer();

            if (dialog.EditedEntry != null)
            {
                vm.UpdateEntry(dialog.EditedEntry);
                this.FindControlRecursive<ToastNotificationHost>("NotificationHost")?.Show("Entry updated");
            }
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
        if (args.Source is StyledElement s && s.DataContext is PassStoreEntryPassword pwd)
        {
            Clipboard!.SetTextAsync(pwd.Password.Value);
            this.FindControlRecursive<ToastNotificationHost>("NotificationHost")?.Show("Password copied to clipboard");
        }
    }

    private async void EntryContextMenuItem_Click(object sender, RoutedEventArgs args)
    {
        if (args.Source is StyledElement s && s.DataContext is PassStoreEntryPassword pwd)
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
            else if (s.Name == "entryCtxMenuEdit")
            {
                if (DataContext is RepositoryWindowViewModel vm && vm.CurrentPage is UnlockedRepositoryViewModel pageVm)
                {
                    EntryEditWindow dialog = new();
                    dialog.SetEntry(pwd);
                    vm.StopLockTimer();
                    await dialog.ShowDialog(this);
                    vm.StartLockTimer();
                    if (dialog.EditedEntry != null)
                    {
                        pageVm.UpdateEntry(dialog.EditedEntry);
                        this.FindControlRecursive<ToastNotificationHost>("NotificationHost")?.Show("Entry updated");
                    }
                }
            }
            else if (s.Name == "entryCtxMenuDelete")
            {
                if (DataContext is RepositoryWindowViewModel vm && vm.CurrentPage is UnlockedRepositoryViewModel pageVm)
                {
                    pageVm.DeleteEntry(pwd.Id);
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
        if (parent == null) return null;
        if (parent is T t && parent.Name == name) return t;

        foreach (var child in parent.GetVisualChildren())
        {
            if (child == null) continue;
            var result = FindControlRecursiveInternal<T>(child, name, depth + 1);
            if (result != null) return result;
        }

        if (parent is ContentPresenter contentPresenter)
        {
            var content = contentPresenter.Content as Visual;
            if (content != null)
            {
                var result = FindControlRecursiveInternal<T>(content, name, depth + 1);
                if (result != null) return result;
            }
        }

        return null;
    }
}
