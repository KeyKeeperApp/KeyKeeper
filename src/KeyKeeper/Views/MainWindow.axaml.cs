using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using KeyKeeper.Models;
using KeyKeeper.PasswordStore;
using KeyKeeper.PasswordStore.Crypto;
using KeyKeeper.Services;
using KeyKeeper.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace KeyKeeper.Views
{
    public partial class MainWindow : Window
    {
        private IRecentFilesService recentFilesService;

        public MainWindow(IRecentFilesService recentFilesService)
        {
            this.recentFilesService = recentFilesService;
            InitializeComponent();
            this.MinWidth = 550;
            this.MinHeight = 350;
        }

        private async void CreateNewVault_Click(object sender, RoutedEventArgs e)
        {
            var createVaultDialog = new CreateVaultDialog();
            await createVaultDialog.ShowDialog(this);

            if (createVaultDialog.Success &&
                !string.IsNullOrEmpty(createVaultDialog.FilePath) &&
                !string.IsNullOrEmpty(createVaultDialog.Password))
            {
                var path = createVaultDialog.FilePath;
                var password = createVaultDialog.Password;
                var compositeKey = new CompositeKey(password, null);

                try
                {
                    var passStoreAccessor = new PassStoreFileAccessor(
                        filename: path,
                        create: true,
                        createOptions: new StoreCreationOptions()
                        {
                            Key = compositeKey,
                            LockTimeoutSeconds = 800
                        });

                    recentFilesService.Remember(path);

                    IPassStore passStore = passStoreAccessor;
                    OpenRepositoryWindow(passStore);
                } catch (IOException exception)
                {
                    Console.WriteLine($"I/O error when creating \"{path}\": {exception}");
                    await new ErrorDialog("Cannot create the password store", "File error").ShowDialog(this);
                } catch (Exception exception)
                {
                    Console.WriteLine($"Unknown error when creating \"{path}\": {exception}");
                    await new ErrorDialog("Cannot create the password store", "Unknown error").ShowDialog(this);
                }
            }
        }

        private async void OpenExistingVault_Click(object sender, RoutedEventArgs e)
        {
            var files = await StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
            {
                Title = "Открыть хранилище паролей",
                AllowMultiple = false,
                FileTypeFilter =
                [
                    new FilePickerFileType("KeyKeeper files")
                    {
                        Patterns = ["*.kkp"]
                    },
                    new FilePickerFileType("All files")
                    {
                        Patterns = ["*.*"]
                    }
                ]
            });

            if (files.Count > 0)
            {
                var file = files[0];
                if (file.TryGetLocalPath() is string path)
                {
                    recentFilesService.Remember(path);
                    IPassStore? passStore;
                    try
                    {
                        passStore = new PassStoreFileAccessor(path, false, null);
                    } catch (PassStoreFileException exc)
                    {
                        await new ErrorDialog($"This password store file has a problem: {exc.Message}", "File format error").ShowDialog(this);
                        Console.WriteLine($"Format error when opening \"{path}\": {exc}");
                        recentFilesService.Forget(path);
                        return;
                    } catch (FileNotFoundException)
                    {
                        await new ErrorDialog("This password store no longer exists", "File error").ShowDialog(this);
                        recentFilesService.Forget(path);
                        return;
                    } catch (IOException exc)
                    {
                        Console.WriteLine($"I/O error when opening \"{path}\": {exc}");
                        await new ErrorDialog("Cannot open this password store", "File error").ShowDialog(this);
                        return;
                    } catch (Exception exc)
                    {
                        Console.WriteLine($"Unknown error when opening \"{path}\": {exc}");
                        await new ErrorDialog("Cannot open this password store", "File error").ShowDialog(this);
                        return;
                    }
                    OpenRepositoryWindow(passStore);
                }
            }
        }

        private async void RecentVaultsListItem_DoubleTapped(object sender, RoutedEventArgs e)
        {
            if (sender is Control c && c.DataContext is RecentFile recentFile)
            {
                recentFilesService.Remember(recentFile.Path);
                IPassStore? passStore;
                try
                {
                    passStore = new PassStoreFileAccessor(recentFile.Path, false, null);
                } catch (PassStoreFileException exc)
                {
                    await new ErrorDialog($"This password store file has a problem: {exc.Message}", "File format error").ShowDialog(this);
                    Console.WriteLine($"Format error when opening \"{recentFile.Path}\" from recents: {exc}");
                    recentFilesService.Forget(recentFile.Path);
                    return;
                } catch (FileNotFoundException)
                {
                    await new ErrorDialog("This password store no longer exists", "File error").ShowDialog(this);
                    recentFilesService.Forget(recentFile.Path);
                    return;
                } catch (IOException exc)
                {
                    Console.WriteLine($"I/O error when opening \"{recentFile.Path}\" from recents: {exc}");
                    await new ErrorDialog("Cannot open this password store", "File error").ShowDialog(this);
                    return;
                } catch (Exception exc)
                {
                    Console.WriteLine($"Unknown error when opening \"{recentFile.Path}\" from recents: {exc}");
                    await new ErrorDialog("Cannot open this password store", "File error").ShowDialog(this);
                    return;
                }
                OpenRepositoryWindow(passStore);
            }
        }

        private void OpenRepositoryWindow(IPassStore passStore)
        {
            var repositoryWindow = new RepositoryWindow(new RepositoryWindowViewModel(passStore))
            {
                WindowStartupLocation = WindowStartupLocation.CenterScreen
            };
            repositoryWindow.Closed += (s, e) =>
            {
                if (AppSettings.ExitOnRepositoryClose)
                {
                    Close();
                }
                else
                {
                    Show();
                }
            };
            repositoryWindow.Show();
            this.Hide(); 
        }
    }
}
