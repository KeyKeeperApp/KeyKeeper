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
            }
        }

        private async void OpenExistingVault_Click(object sender, RoutedEventArgs e)
        {
            var files = await StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
            {
                Title = "Открыть хранилище паролей",
                AllowMultiple = false,
                FileTypeFilter = new[]
                {
                    new FilePickerFileType("KeyKeeper files")
                    {
                        Patterns = new[] { "*.kkp" }
                    },
                    new FilePickerFileType("All files")
                    {
                        Patterns = new[] { "*.*" }
                    }
                }
            });

            if (files.Count > 0)
            {
                var file = files[0];
                if (file.TryGetLocalPath() is string path)
                {
                    recentFilesService.Remember(path);
                    OpenRepositoryWindow(new PassStoreFileAccessor(path, false, null));
                }
            }
        }

        private void RecentVaultsListItem_DoubleTapped(object sender, RoutedEventArgs e)
        {
            if (sender is Control c && c.DataContext is RecentFile recentFile)
            {
                recentFilesService.Remember(recentFile.Path);
                OpenRepositoryWindow(new PassStoreFileAccessor(recentFile.Path, false, null));
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
                    this.Close();
                }
                else
                {
                    this.Show();
                }
            };
            repositoryWindow.Show();
            this.Hide(); 
        }
    }
}
