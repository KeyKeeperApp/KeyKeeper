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
            var file = await StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
            {
                Title = "Create new password store",
                SuggestedFileName = "passwords.kkp",
                DefaultExtension = "kkp",
                FileTypeChoices = new[]
                {
                    new FilePickerFileType("KeyKeeper files")
                    {
                        Patterns = new[] { "*.kkp" }
                    }
                }
            });

            if (file != null)
            {
                if (file.TryGetLocalPath() is string path)
                {
                    var passwordDialog = new PasswordDialog();
                    await passwordDialog.ShowDialog(this);
                    if (passwordDialog.Created && !string.IsNullOrEmpty(passwordDialog.Password))
                    {
                        var compositeKey = new CompositeKey(passwordDialog.Password, null);
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
