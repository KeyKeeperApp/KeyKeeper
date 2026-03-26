using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;

namespace KeyKeeper.Views
{
    public partial class CreateVaultFileWindow : Window
    {
        public string FilePath { get; private set; } = string.Empty;
        public string Password { get; private set; } = string.Empty;
        public bool Success { get; private set; }

        public CreateVaultFileWindow()
        {
            InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
            FilePathTextBox.TextChanged += OnTextChanged;
            PasswordBox.TextChanged += OnPasswordTextChanged;
            ConfirmPasswordBox.TextChanged += OnPasswordTextChanged;

            KeyDown += CreateVaultDialog_KeyDown;
        }

        private async void OnTextChanged(object? sender, TextChangedEventArgs e)
        {
            UpdateCreateButtonState();
            PathWarning.Text = "";

            string path = FilePathTextBox.Text ?? "";
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

        private void OnPasswordTextChanged(object? sender, TextChangedEventArgs e)
        {
            UpdateCreateButtonState();
            PasswordErrorText.IsVisible = false;
        }

        private void UpdateCreateButtonState()
        {
            bool pathValid = !string.IsNullOrWhiteSpace(FilePathTextBox.Text);
            bool passwordsEntered = !string.IsNullOrWhiteSpace(PasswordBox.Text) &&
                                    !string.IsNullOrWhiteSpace(ConfirmPasswordBox.Text);
            CreateButton.IsEnabled = pathValid && passwordsEntered;
        }

        private void CreateVaultDialog_KeyDown(object? sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                Close();
            }
            else if (e.Key == Key.Enter && CreateButton.IsEnabled)
            {
                Submit();
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

            if (file?.TryGetLocalPath() is string path)
            {
                FilePathTextBox.Text = path;
            }
        }

        private void CreateButton_Click(object? sender, RoutedEventArgs e)
        {
            Submit();
        }

        private void Submit()
        {
            string path = FilePathTextBox.Text ?? "";
            if (string.IsNullOrWhiteSpace(path))
                return;

            string password = PasswordBox.Text ?? "";
            string confirm = ConfirmPasswordBox.Text ?? "";

            if (string.IsNullOrEmpty(password) || string.IsNullOrEmpty(confirm))
            {
                ShowPasswordError("Password cannot be empty");
                return;
            }

            if (password != confirm)
            {
                ShowPasswordError("Passwords don't match");
                return;
            }

            FilePath = path;
            Password = password;
            Success = true;
            Close();
        }

        private void ShowPasswordError(string message)
        {
            PasswordErrorText.Text = message;
            PasswordErrorText.IsVisible = true;
        }

        private void CancelButton_Click(object? sender, RoutedEventArgs e)
        {
            Success = false;
            Close();
        }
    }
}