using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using System;
using System.IO;

namespace KeyKeeper.Views
{
    public partial class CreateVaultFileWindow : Window
    {
        public string FilePath { get; private set; } = string.Empty;
        public bool Success { get; private set; }

        public CreateVaultFileWindow()
        {
            InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
            FilePathTextBox.TextChanged += OnTextChanged;
        }

        private async void OnTextChanged(object? sender, TextChangedEventArgs e)
        {
            string path = FilePathTextBox.Text ?? "";
            CreateButton.IsEnabled = !string.IsNullOrWhiteSpace(path);
            PathWarning.Text = "";

            if (string.IsNullOrWhiteSpace(path))
                return;

            try
            {
                var storageFile = await StorageProvider.TryGetFileFromPathAsync(path);
                if (storageFile != null)
                {
                    PathWarning.Text = "File already exists. It will be overwritten.";
                }
            }
            catch
            {
            }
        }

        private async void BrowseButton_Click(object? sender, RoutedEventArgs e)
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

            if (file != null && file.TryGetLocalPath() is string path)
            {
                FilePathTextBox.Text = path;
            }
        }

        private void CreateButton_Click(object? sender, RoutedEventArgs e)
        {
            FilePath = FilePathTextBox.Text ?? "";
            if (string.IsNullOrWhiteSpace(FilePath)) return;

            Success = true;
            Close();
        }

        private void CancelButton_Click(object? sender, RoutedEventArgs e)
        {
            Success = false;
            Close();
        }
    }
}