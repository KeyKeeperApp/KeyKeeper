using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
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

            LoadRecentVaults();
        }

        private void LoadRecentVaults()
        {
            var recentFiles = new List<RecentVault>
            {
                new RecentVault { Path = "C:\\Users\\User\\Documents\\passwords.kdbx", LastOpened = DateTime.Now.AddDays(-1) },
                new RecentVault { Path = "C:\\Users\\User\\Desktop\\work_passwords.kdbx", LastOpened = DateTime.Now.AddDays(-3) }
            };

            RecentVaultsList.ItemsSource = recentFiles;
        }

        private async void CreateNewVault_Click(object sender, RoutedEventArgs e)
        {
            var topLevel = TopLevel.GetTopLevel(this);
            if (topLevel == null) return;

            var file = await topLevel.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
            {
                Title = "Создать новое хранилище паролей",
                SuggestedFileName = "passwords.kdbx",
                DefaultExtension = "kdbx",
                FileTypeChoices = new[]
                {
                    new FilePickerFileType("KeyKeeper Database")
                    {
                        Patterns = new[] { "*.kdbx" }
                    }
                }
            });

            if (file != null)
            {
                // Здесь будет логика создания нового хранилища
                ShowMessage($"Создание нового хранилища: {file.Name}");
            }
        }

        private async void OpenExistingVault_Click(object sender, RoutedEventArgs e)
        {
            var topLevel = TopLevel.GetTopLevel(this);
            if (topLevel == null) return;

            // Открываем диалог выбора файла
            var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
            {
                Title = "Открыть хранилище паролей",
                AllowMultiple = false,
                FileTypeFilter = new[]
                {
                    new FilePickerFileType("KeyKeeper Database")
                    {
                        Patterns = new[] { "*.kdbx", "*.kkdb" }
                    },
                    new FilePickerFileType("KeePass Database")
                    {
                        Patterns = new[] { "*.kdbx" }
                    },
                    new FilePickerFileType("All Files")
                    {
                        Patterns = new[] { "*.*" }
                    }
                }
            });

            if (files.Count > 0)
            {
                var file = files[0];
                ShowMessage($"Открытие хранилища: {file.Name}");
            }
        }

        private void ShowMessage(string message)
        {
            // Временное решение для показа сообщений
            var messageBox = new Window
            {
                Title = "KeyKeeper",
                Content = new TextBlock { Text = message, Margin = new Thickness(20) },
                SizeToContent = SizeToContent.WidthAndHeight,
                WindowStartupLocation = WindowStartupLocation.CenterOwner
            };
            messageBox.ShowDialog(this);
        }
    }

    public class RecentVault
    {
        public string Path { get; set; } = string.Empty;
        public DateTime LastOpened { get; set; }

        public string DisplayPath => System.IO.Path.GetFileName(Path);

        public override string ToString()
        {
            return DisplayPath;
        }
    }
}