using System;
using System.Text.RegularExpressions;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using KeyKeeper.PasswordStore;
using static KeyKeeper.PasswordStore.FileFormatConstants;

namespace KeyKeeper.Views;

public partial class EntryEditWindow : Window
{
    public PassStoreEntryPassword? EditedEntry;

    public EntryEditWindow()
    {
        InitializeComponent();
        if (PasswordEdit != null)
        {
            PasswordEdit.TextChanged += PasswordTextChanged;
        }
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
        if (maxWidth <= 0) maxWidth = 200; 

        PasswordStrengthFill.Width = (strength / 100.0) * maxWidth;

        if (strength < 20)
        {
            PasswordStrengthFill.Background = new SolidColorBrush(Colors.Red);
        }
        else if (strength < 50)
        {
            PasswordStrengthFill.Background = new SolidColorBrush(Colors.Orange);
        }
        else if (strength < 70)
        {
            PasswordStrengthFill.Background = new SolidColorBrush(Colors.Gold);
        }
        else
        {
            PasswordStrengthFill.Background = new SolidColorBrush(Colors.Green);
        }
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

    private void AddButton_Click(object sender, RoutedEventArgs args)
    {
        string name = EntryNameEdit?.Text?.Trim() ?? "";
        if (string.IsNullOrEmpty(name)) return;

        string username = UsernameEdit?.Text?.Trim() ?? "";
        if (string.IsNullOrEmpty(username)) return;

        string password = PasswordEdit?.Text ?? "";
        if (string.IsNullOrEmpty(password)) return;

        EditedEntry = new PassStoreEntryPassword(
            Guid.NewGuid(),
            DateTime.UtcNow,
            DateTime.UtcNow,
            EntryIconType.DEFAULT,
            name,
            new LoginField()
            {
                Type = LOGIN_FIELD_USERNAME_ID,
                Value = username
            },
            new LoginField()
            {
                Type = LOGIN_FIELD_PASSWORD_ID,
                Value = password
            },
            null
        );
        Close();
    }
}