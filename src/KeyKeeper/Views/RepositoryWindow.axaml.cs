using System;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Input;
using KeyKeeper.PasswordStore;
using KeyKeeper.Models;
using KeyKeeper.ViewModels;
using Avalonia.VisualTree;
using Avalonia.Controls.Presenters;

namespace KeyKeeper.Views;

public partial class RepositoryWindow : Window
{
    private bool allowClose;
    private bool closeConfirmationShown;
    private PassStoreEntry? _contextMenuEntry;

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

            if (dialog.GroupData != null)
            {
                var group = new PassStoreEntryGroup(
                    Guid.NewGuid(),
                    DateTime.UtcNow,
                    DateTime.UtcNow,
                    dialog.GroupData.IconType,
                    dialog.GroupData.Name,
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
        if (args.Source is StyledElement s && s.DataContext is PassStoreEntry ent)
        {
            CopyPassword(ent);
        }
    }

    private void PasswordsListBox_KeyDown(object sender, KeyEventArgs args)
    {
        if (args.Key == Key.C && args.KeyModifiers == KeyModifiers.Control)
        {
            if (sender is ListBox list && list.SelectedItem is PassStoreEntry ent)
            {
                CopyPassword(ent);
            }
        }
    }

    private void CopyPassword(PassStoreEntry ent)
    {
        PassStoreEntryPassword? pwd = null;
        if (ent is PassStoreEntryPassword p) pwd = p;
        else if (ent is PassStoreEntryLink lnk && lnk.LinkTarget is PassStoreEntryPassword p1) pwd = p1;
        if (pwd == null) return;

        Clipboard!.SetTextAsync(pwd.Password.Value);
        this.FindControlRecursive<ToastNotificationHost>("NotificationHost")?.Show("Password copied to clipboard");
    }

    private void EntryContextMenu_Opening(object? sender, RoutedEventArgs args)
    {
        if (sender is not ContextMenu contextMenu || DataContext is not RepositoryWindowViewModel vm ||
            vm.CurrentPage is not UnlockedRepositoryViewModel pageVm)
            return;

        _contextMenuEntry = null;

        if (contextMenu.Parent?.Parent is Border border && border.DataContext is PassStoreEntry entry)
        {
            _contextMenuEntry = entry;
        }

        var addToGroupItem = contextMenu.Items
            .OfType<MenuItem>()
            .FirstOrDefault(m => m.Name == "entryCtxMenuAddToGroup");

        var removeFromGroupItem = contextMenu.Items
            .OfType<MenuItem>()
            .FirstOrDefault(m => m.Name == "entryCtxMenuRemoveFromGroup");

        var isNonDefaultGroup = pageVm.SelectedPasswordGroup.GroupType != FileFormatConstants.GROUP_TYPE_DEFAULT;
        if (removeFromGroupItem != null)
        {
            removeFromGroupItem.IsVisible = isNonDefaultGroup;
        }

        if (addToGroupItem == null)
            return;

        addToGroupItem.Items.Clear();

        var nonDefaultGroups = pageVm.PasswordGroups
            .Where(g => g.GroupType != FileFormatConstants.GROUP_TYPE_DEFAULT)
            .ToList();

        EventHandler<RoutedEventArgs> onSubmenuClick = (sender, args) => AddToGroup_Click(sender, args, _contextMenuEntry!);
        foreach (var group in nonDefaultGroups)
        {
            var menuItem = new MenuItem
            {
                Header = group.DisplayName,
                Tag = group
            };
            menuItem.Click += onSubmenuClick;
            addToGroupItem.Items.Add(menuItem);
        }
    }

    private void AddToGroup_Click(object? sender, RoutedEventArgs args, PassStoreEntry entry)
    {
        if (sender is not MenuItem item || item.Tag is not PassStoreEntryGroup targetGroup)
            return;

        if (entry == null)
            return;

        if (DataContext is not RepositoryWindowViewModel vm ||
            vm.CurrentPage is not UnlockedRepositoryViewModel pageVm)
            return;

        var notificationHost = this.FindControlRecursive<ToastNotificationHost>("NotificationHost");

        if (pageVm.AddEntryToGroup(entry, targetGroup))
            notificationHost?.Show($"Added to {targetGroup.DisplayName}");
        else
            notificationHost?.Show($"This entry is already in {targetGroup.DisplayName}!");
        _contextMenuEntry = null;
    }

    private async void GroupContextMenuItem_Click(object sender, RoutedEventArgs args)
    {
        if (args.Source is not StyledElement s || s.DataContext is not PassStoreEntryGroup group)
            return;

        if (DataContext is not RepositoryWindowViewModel vm ||
            vm.CurrentPage is not UnlockedRepositoryViewModel pageVm)
            return;

        if (group.GroupType == FileFormatConstants.GROUP_TYPE_DEFAULT ||
            group.GroupType == FileFormatConstants.GROUP_TYPE_FAVOURITES ||
            group.GroupType == FileFormatConstants.GROUP_TYPE_ROOT)
            return;

        if (s.Name == "groupCtxMenuEdit")
        {
            await EditGroup(vm, pageVm, group);
        }
        else if (s.Name == "groupCtxMenuDelete")
        {
            await DeleteGroup(group);
        }
    }

    private async Task EditGroup(RepositoryWindowViewModel vm, UnlockedRepositoryViewModel pageVm, PassStoreEntryGroup group)
    {
        CreateGroupDialog dialog = new();
        dialog.SetupForEdit(group);

        vm.StopLockTimer();

        await dialog.ShowDialog(this);

        vm.StartLockTimer();

        if (dialog.GroupData != null)
        {
            pageVm.UpdateGroup(group, dialog.GroupData.Name, dialog.GroupData.IconType);
            this.FindControlRecursive<ToastNotificationHost>("NotificationHost")?.Show("Group updated");
        }
    }

    private async Task DeleteGroup(PassStoreEntryGroup group)
    {
        ConfirmationDialog confirmDialog = new();
        confirmDialog.SetContent(
            "Delete Group",
            $"Are you sure you want to delete the group '{group.DisplayName}'? This action cannot be undone.",
            "Delete"
        );

        if (DataContext is not RepositoryWindowViewModel vm)
            return;

        vm.StopLockTimer();

        await confirmDialog.ShowDialog(this);

        vm.StartLockTimer();

        if (confirmDialog.Confirmed)
        {
            if (vm.CurrentPage is UnlockedRepositoryViewModel pageVm)
            {
                pageVm.DeleteGroup(group);
                this.FindControlRecursive<ToastNotificationHost>("NotificationHost")?.Show("Group deleted");
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
            else if (s.Name == "entryCtxMenuRemoveFromGroup")
            {
                if (DataContext is RepositoryWindowViewModel vm && vm.CurrentPage is UnlockedRepositoryViewModel pageVm)
                {
                    pageVm.RemoveEntryFromGroup(ent);
                    this.FindControlRecursive<ToastNotificationHost>("NotificationHost")?.Show("Removed from group");
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
