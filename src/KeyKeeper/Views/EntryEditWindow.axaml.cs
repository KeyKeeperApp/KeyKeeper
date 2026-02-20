using System;
using Avalonia.Controls;
using Avalonia.Interactivity;
using KeyKeeper.PasswordStore;
using static KeyKeeper.PasswordStore.FileFormatConstants;

namespace KeyKeeper.Views;

public partial class EntryEditWindow: Window
{
    public PassStoreEntryPassword? EditedEntry;

    public EntryEditWindow()
    {
        InitializeComponent();
    }

    private void AddButton_Click(object sender, RoutedEventArgs args)
    {
        string name = EntryNameEdit.Text ?? "";
        name = name.Trim();
        if (name.Length == 0) return;

        string username = UsernameEdit.Text ?? "";
        username = username.Trim();
        if (username.Length == 0) return;

        string password = PasswordEdit.Text ?? "";
        password = password.Trim();
        if (password.Length == 0) return;

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