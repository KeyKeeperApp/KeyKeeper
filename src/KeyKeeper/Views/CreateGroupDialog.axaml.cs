using System;
using System.Collections.Generic;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using KeyKeeper.Models;
using KeyKeeper.PasswordStore;

namespace KeyKeeper.Views;

public partial class CreateGroupDialog : Window
{
    public GroupEditData? GroupData { get; private set; }

    private record IconChoice(Guid Id)
    {
        public string IconPath => $"avares://KeyKeeper/Assets/builtin-entry-icon-{Id}.svg";
    }

    public CreateGroupDialog()
    {
        InitializeComponent();

        var icons = new List<IconChoice>
        {
            new(BuiltinEntryIconType.KEY),
        };
        IconListBox.ItemsSource = icons;
        IconListBox.SelectedIndex = 0;

        NameTextBox.TextChanged += (_, _) => UpdateActionButtonState();
        KeyDown += OnKeyDown;
    }

    public void SetupForEdit(PassStoreEntryGroup group)
    {
        TitleText.Text = "Edit group";
        ActionButton.Content = "Save";
        NameTextBox.Text = group.Name;

        for (int i = 0; i < IconListBox.ItemCount; i++)
        {
            if (IconListBox.Items[i] is IconChoice choice && choice.Id == group.IconType)
            {
                IconListBox.SelectedIndex = i;
                break;
            }
        }
    }

    private void UpdateActionButtonState()
    {
        ActionButton.IsEnabled = !string.IsNullOrWhiteSpace(NameTextBox.Text);
    }

    private void OnKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
            Close();
        else if (e.Key == Key.Enter && ActionButton.IsEnabled)
            Submit();
    }

    private void ActionButton_Click(object? sender, RoutedEventArgs e) => Submit();

    private void CancelButton_Click(object? sender, RoutedEventArgs e)
    {
        GroupData = null;
        Close();
    }

    private void Submit()
    {
        var name = NameTextBox.Text?.Trim() ?? string.Empty;
        if (string.IsNullOrEmpty(name))
        {
            ErrorText.Text = "Name cannot be empty";
            ErrorText.IsVisible = true;
            return;
        }

        var iconType = BuiltinEntryIconType.DEFAULT;
        if (IconListBox.SelectedItem is IconChoice choice)
            iconType = choice.Id;

        GroupData = new GroupEditData(name, iconType);
        Close();
    }
}
