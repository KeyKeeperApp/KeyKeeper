using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using KeyKeeper.PasswordStore;
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
                Title = "Создать новое хранилище паролей",
                SuggestedFileName = "passwords.kkp",
                DefaultExtension = "kkp",
                FileTypeChoices = new[]
                {
                    new FilePickerFileType("Хранилище KeyKeeper")
                    {
                        Patterns = new[] { "*.kkp" }
                    }
                }
            });

            if (file != null)
            {
                if (file.TryGetLocalPath() is string path)
                {
                    (DataContext as MainWindowViewModel)!.CreateVault(path);
                    OpenRepositoryWindow(new PassStoreFileAccessor(path, true, new StoreCreationOptions()
                    {
                        Key = new PasswordStore.Crypto.CompositeKey("blablabla", null),
                        LockTimeoutSeconds = 800,
                    }));
                }
            }
        }

        private async void OpenExistingVault_Click(object sender, RoutedEventArgs e)
        {
            // Открываем диалог выбора файла
            var files = await StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
            {
                Title = "Открыть хранилище паролей",
                AllowMultiple = false,
                FileTypeFilter = new[]
                {
                    new FilePickerFileType("Хранилище KeyKeeper")
                    {
                        Patterns = new[] { "*.kkp" }
                    },
                    new FilePickerFileType("Все файлы")
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
                    (DataContext as MainWindowViewModel)!.OpenVault(path);
                    OpenRepositoryWindow(new PassStoreFileAccessor(path, false, null));
                }
            }
        }

        private void OpenRepositoryWindow(IPassStore store)
        {
            var repositoryWindow = new RepositoryWindow()
            {
                DataContext = new RepositoryWindowViewModel(),
                PassStore = store,
                WindowStartupLocation = WindowStartupLocation.CenterScreen
            };
            repositoryWindow.Closed += (s, e) => this.Show();
            repositoryWindow.Show();
            this.Hide();
        }
    }
}