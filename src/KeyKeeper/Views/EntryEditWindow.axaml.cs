using System;
using System.Text.RegularExpressions;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using KeyKeeper.PasswordStore;
using KeyKeeper.ViewModels;

namespace KeyKeeper.Views;

public partial class EntryEditWindow : Window
{
    private EntryEditViewModel _viewModel;

    public PassStoreEntryPassword? EditedEntry => _viewModel.EditedEntry;

    public EntryEditWindow()
    {
        InitializeComponent();
        _viewModel = new EntryEditViewModel();
        DataContext = _viewModel;

        if (PasswordEdit != null)
        {
            PasswordEdit.TextChanged += PasswordTextChanged;
        }

        if (ConfigureTotpButton != null)
        {
            ConfigureTotpButton.Click += ConfigureTotpButton_Click;
        }

        if (PasteUrlButton != null)
        {
            PasteUrlButton.Click += PasteUrlButton_Click;
        }

        if (RemoveTotpButton != null)
        {
            RemoveTotpButton.Click += RemoveTotpButton_Click;
        }

        if (DoneButton != null)
        {
            DoneButton.Click += DoneButton_Click;
        }
    }

    public void SetEntry(PassStoreEntryPassword entry)
    {
        _viewModel.LoadEntry(entry);
    }

    private void PasswordTextChanged(object? sender, TextChangedEventArgs e)
    {
        string password = PasswordEdit?.Text ?? "";
        UpdatePasswordStrengthIndicator(password);
    }

    private void UpdatePasswordStrengthIndicator(string password)
    {
        if (PasswordStrengthFill == null || PasswordStrengthIndicator == null)
            return;

        if (string.IsNullOrEmpty(password))
        {
            PasswordStrengthFill.Width = 0;
            return;
        }

        int strength = CalculatePasswordStrength(password);
        double maxWidth = PasswordStrengthIndicator.Bounds.Width;
        if (maxWidth <= 0) maxWidth = 300;

        PasswordStrengthFill.Width = (strength / 100.0) * maxWidth;

        if (strength < 20)
            PasswordStrengthFill.Background = new SolidColorBrush(Colors.Red);
        else if (strength < 50)
            PasswordStrengthFill.Background = new SolidColorBrush(Colors.Orange);
        else if (strength < 70)
            PasswordStrengthFill.Background = new SolidColorBrush(Colors.Gold);
        else
            PasswordStrengthFill.Background = new SolidColorBrush(Colors.Green);
    }

    private int CalculatePasswordStrength(string password)
    {
        int score = 0;
        if (password.Length >= 8) score += 20;
        if (password.Length >= 12) score += 20;
        if (password.Length >= 16) score += 15;
        if (Regex.IsMatch(password, @"\d")) score += 10;
        if (Regex.IsMatch(password, @"[a-z]")) score += 15;
        if (Regex.IsMatch(password, @"[A-Z]")) score += 15;
        if (Regex.IsMatch(password, @"[!@#$%^&*()_+\-=\[\]{};':""\\|,.<>\/?]")) score += 20;

        var uniqueChars = new System.Collections.Generic.HashSet<char>(password).Count;
        score += Math.Min(20, uniqueChars * 2);
        return Math.Min(100, score);
    }

    private void ConfigureTotpButton_Click(object? sender, RoutedEventArgs args)
    {
        _viewModel.ConfigureTotp();
    }

    private async void PasteUrlButton_Click(object? sender, RoutedEventArgs args)
    {
        try
        {
            string? clipboardText = await Clipboard!.GetTextAsync();
            if (!string.IsNullOrEmpty(clipboardText))
            {
                _viewModel.ParseOtpauthUrl(clipboardText);
            }
        }
        catch
        {
            // Silently fail if clipboard access fails
        }
    }

    private void RemoveTotpButton_Click(object? sender, RoutedEventArgs args)
    {
        _viewModel.RemoveTotp();
    }

    private void DoneButton_Click(object? sender, RoutedEventArgs args)
    {
        _viewModel.CreateEntry();
        Close();
    }
}
