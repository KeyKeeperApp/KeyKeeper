using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using KeyKeeper.PasswordStore;
using KeyKeeper.PasswordStore.Crypto;

namespace KeyKeeper;

public partial class RepositoryWindow : Window
{
    public IPassStore? PassStore { get; init; }
    public string? MasterPassword { get; init; }  // Для хранения пароля

    public RepositoryWindow()
    {
        InitializeComponent();
    }

    protected override void OnOpened(EventArgs e)
    {
        base.OnOpened(e);

        // Разблокируем хранилище если нужно
        if (PassStore != null && PassStore.Locked && !string.IsNullOrEmpty(MasterPassword))
        {
            var compositeKey = new CompositeKey(MasterPassword, null);
            PassStore.Unlock(compositeKey);
        }
    }
}