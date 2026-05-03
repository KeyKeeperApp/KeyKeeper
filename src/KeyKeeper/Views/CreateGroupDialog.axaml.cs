using System;
using System.Collections.Generic;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using KeyKeeper.PasswordStore;

namespace KeyKeeper.Views;

public partial class CreateGroupDialog : Window
{
    public string GroupName { get; private set; } = string.Empty;
    public Guid IconType { get; private set; } = BuiltinEntryIconType.DEFAULT;
    public bool Success { get; private set; }

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

        NameTextBox.TextChanged += (_, _) => UpdateCreateButtonState();
        KeyDown += OnKeyDown;
    }

    private void UpdateCreateButtonState()
    {
        CreateButton.IsEnabled = !string.IsNullOrWhiteSpace(NameTextBox.Text);
    }

    private void OnKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
            Close();
        else if (e.Key == Key.Enter && CreateButton.IsEnabled)
            Submit();
    }

    private void CreateButton_Click(object? sender, RoutedEventArgs e) => Submit();

    private void CancelButton_Click(object? sender, RoutedEventArgs e)
    {
        Success = false;
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

        GroupName = name;
        if (IconListBox.SelectedItem is IconChoice choice)
            IconType = choice.Id;

        Success = true;
        Close();
    }
}
