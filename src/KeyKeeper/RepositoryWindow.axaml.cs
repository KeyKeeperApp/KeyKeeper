using System;
using Avalonia.Controls;
using KeyKeeper.PasswordStore;
using KeyKeeper.PasswordStore.Crypto;

namespace KeyKeeper;

public partial class RepositoryWindow: Window
{
    public RepositoryWindow()
    {
        InitializeComponent();
    }

    protected override void OnOpened(EventArgs e)
    {
        base.OnOpened(e);
    }
}