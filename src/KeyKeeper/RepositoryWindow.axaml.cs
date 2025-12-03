using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using KeyKeeper.PasswordStore;
using KeyKeeper.PasswordStore.Crypto;

namespace KeyKeeper;

public partial class RepositoryWindow: Window
{
    public IPassStore? PassStore { private get; init; }

    public RepositoryWindow()
    {
        InitializeComponent();
    }

    protected override void OnOpened(EventArgs e)
    {
        base.OnOpened(e);
        if (PassStore!.Locked)
            PassStore.Unlock(new CompositeKey("blablabla", null));
    }
}