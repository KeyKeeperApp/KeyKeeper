using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using KeyKeeper.Views;
using Avalonia.Controls;

namespace KeyKeeper.ViewModels;
public partial class MainWindowViewModel : ViewModelBase
{
    public string Greeting { get; } = "Welcome to Avalonia!";
    [RelayCommand]
    private async void OpenSettings()
    {
        var settingsWindow = new SettingsWindow();
        await settingsWindow.ShowDialog(GetMainWindow());
    }
    private static Window? GetMainWindow()
    {
        return App.Current?.ApplicationLifetime is Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime desktop
            ? desktop.MainWindow
            : null;
    }
}
