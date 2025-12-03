using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using KeyKeeper.Views;
using Avalonia.Controls;
using System.Threading.Tasks;
using KeyKeeper.Services;
using System.Collections.ObjectModel;
using KeyKeeper.Models;

namespace KeyKeeper.ViewModels;
public partial class MainWindowViewModel : ViewModelBase
{
    public string Greeting { get; } = "Welcome to KeyKeeper!";
    public ObservableCollection<RecentFile> RecentFiles => recentFilesService.RecentFiles;

    private IRecentFilesService recentFilesService;

    public MainWindowViewModel(IRecentFilesService recentFilesService)
    {
        this.recentFilesService = recentFilesService;
    }

    public void OpenVault(string filename)
    {
        recentFilesService.Remember(filename);
    }

    public void CreateVault(string filename)
    {
        recentFilesService.Remember(filename);
    }

    [RelayCommand]
    private async Task OpenSettings()
    {
        var settingsWindow = new SettingsWindow();
        await settingsWindow.ShowDialog(GetMainWindow()!);
    }

    [RelayCommand]
    private async Task OpenAbout()
    {
        var AboutWindow = new AboutWindow();
        await AboutWindow.ShowDialog(GetMainWindow()!);
    }
    private static Window? GetMainWindow()
    {
        return App.Current?.ApplicationLifetime is Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime desktop
            ? desktop.MainWindow
            : null;
    }
}
