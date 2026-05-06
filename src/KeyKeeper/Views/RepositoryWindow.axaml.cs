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

    protected override void OnClosed(EventArgs e)
    {
        // Stop TOTP refresh timer when window closes
        if (DataContext is RepositoryWindowViewModel vm &&
            vm.CurrentPage is UnlockedRepositoryViewModel unlockedVm)
        {
            unlockedVm.StopTotpRefreshTimer();
        }
        base.OnClosed(e);
    }

    private void OnUserActivity(object? sender, RoutedEventArgs e)
    {
        if (DataContext is RepositoryWindowViewModel vm)
            vm.ResetLockTimer();
    }

    private void UnlockPasswordEdit_Loaded(object? sender, RoutedEventArgs e)
    {
        (sender as TextBox)?.Focus();
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

    private async void EditEntry(RepositoryWindowViewModel vm1, UnlockedRepositoryViewModel vm2, PassStoreEntry entry)
    {
        if (entry == null)
        {
            this.FindControlRecursive<ToastNotificationHost>("NotificationHost")?.Show("No entry selected");
            return;
        }

        EntryEditWindow dialog = new();

        PassStoreEntry realEntry = entry;
        if (realEntry is PassStoreEntryLink lnk)
            realEntry = lnk.LinkTarget!;

        dialog.SetEntry((realEntry as PassStoreEntryPassword)!);

        vm1.StopLockTimer();

        await dialog.ShowDialog(this);

        vm1.StartLockTimer();

        if (dialog.EditedEntry != null)
        {
            if (entry is PassStoreEntryLink l)
                l.LinkTarget = dialog.EditedEntry;
            vm2.UpdateEntry(dialog.EditedEntry);
            this.FindControlRecursive<ToastNotificationHost>("NotificationHost")?.Show("Entry updated");
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

            PassStoreEntry? entry = listBox.SelectedItem as PassStoreEntry;
            if (entry == null) return;
            EditEntry(vm_, vm, entry);
        }
    }

    private async void AddGroupButton_Click(object sender, RoutedEventArgs args)
    {
        if (DataContext is RepositoryWindowViewModel vm_ && vm_.CurrentPage is UnlockedRepositoryViewModel vm)
        {
            CreateGroupDialog dialog = new();

            vm_.StopLockTimer();

            await dialog.ShowDialog(this);

            vm_.StartLockTimer();

            if (dialog.Success)
            {
                var group = new PassStoreEntryGroup(
                    Guid.NewGuid(),
                    DateTime.UtcNow,
                    DateTime.UtcNow,
                    dialog.IconType,
                    dialog.GroupName,
                    FileFormatConstants.GROUP_TYPE_SIMPLE
                );
                vm.AddGroup(group);
                this.FindControlRecursive<ToastNotificationHost>("NotificationHost")?.Show("Group created");
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

    private void PasswordsListBox_KeyDown(object sender, KeyEventArgs args)
    {
        if (args.Key == Key.C && args.KeyModifiers == KeyModifiers.Control)
        {
            if (sender is ListBox list && list.SelectedItem is PassStoreEntryPassword pwd)
            {
                Clipboard!.SetTextAsync(pwd.Password.Value);
                this.FindControlRecursive<ToastNotificationHost>("NotificationHost")?.Show("Password copied to clipboard");
            }
        }
    }

    private async void EntryContextMenuItem_Click(object sender, RoutedEventArgs args)
    {
        if (args.Source is StyledElement s && s.DataContext is PassStoreEntry ent)
        {
            PassStoreEntryPassword? pwd = UnlockedRepositoryViewModel.FollowLinkIfNeeded(ent);
            if (pwd == null) return;

            if (s.Name == "entryCtxMenuCopyUsername")
            {
                await Clipboard!.SetTextAsync(pwd.Username.Value);
                this.FindControlRecursive<ToastNotificationHost>("NotificationHost")?.Show("Username copied to clipboard");
            }
            else if (s.Name == "entryCtxMenuCopyPassword")
            {
                await Clipboard!.SetTextAsync(pwd.Password.Value);
                this.FindControlRecursive<ToastNotificationHost>("NotificationHost")?.Show("Password copied to clipboard");
            }
            else if (s.Name == "entryCtxMenuEdit")
            {
                if (DataContext is RepositoryWindowViewModel vm && vm.CurrentPage is UnlockedRepositoryViewModel pageVm)
                {
                    EditEntry(vm, pageVm, ent);
                }
            }
            else if (s.Name == "entryCtxMenuDelete")
            {
                if (DataContext is RepositoryWindowViewModel vm && vm.CurrentPage is UnlockedRepositoryViewModel pageVm)
                {
                    pageVm.DeleteEntry(ent.Id);
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
