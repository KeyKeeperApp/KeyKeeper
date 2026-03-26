using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using KeyKeeper.PasswordStore;
using KeyKeeper.PasswordStore.Crypto;
using KeyKeeper.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KeyKeeper.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
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
                    OpenRepositoryWindow(new PassStoreFileAccessor(path, false, null));
                }
            }
        }

        private void OpenRepositoryWindow(IPassStore passStore)
        {
            var repositoryWindow = new RepositoryWindow(new RepositoryWindowViewModel(passStore))
            {
                WindowStartupLocation = WindowStartupLocation.CenterScreen
            };

            repositoryWindow.Closed += (s, e) => this.Show();
            repositoryWindow.Show();
            this.Hide();
        }
    }
}
