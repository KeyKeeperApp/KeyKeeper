using Avalonia.Controls;
using Avalonia.Interactivity;
using KeyKeeper.PasswordStore;

namespace KeyKeeper.Views;

public partial class EntryEditWindow: Window
{
    public PassStoreEntryPassword? EditedEntry;

    public EntryEditWindow()
    {
        InitializeComponent();
    }
}