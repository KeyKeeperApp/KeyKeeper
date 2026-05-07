using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Platform.Storage;
using System.Text.RegularExpressions;

namespace KeyKeeper.Views
{
    public partial class CreateVaultDialog : Window
    {
        public string FilePath { get; private set; } = string.Empty;
        public string Password { get; private set; } = string.Empty;
        public bool Success { get; private set; }

        // Добавлено для индикатора сложности пароля
        private Border? _strengthBorder;
        private TextBlock? _strengthText;

        public CreateVaultDialog()
        {
            InitializeComponent();
            MinWidth = 450;
            MinHeight = 350;
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

            // Добавлено: обновляем индикатор сложности пароля
            UpdatePasswordStrength(PasswordBox.Text ?? "");
        }

        // Добавлен метод для оценки сложности пароля
        private void UpdatePasswordStrength(string password)
        {
            if (_strengthBorder == null || _strengthText == null)
            {
                _strengthBorder = this.FindControl<Border>("StrengthBorder");
                _strengthText = this.FindControl<TextBlock>("StrengthText");
                if (_strengthBorder == null || _strengthText == null) return;
            }

            int score = 0;
            if (password.Length >= 8) score++;
            if (password.Length >= 12) score++;
            if (Regex.IsMatch(password, @"\d")) score++;
            if (Regex.IsMatch(password, @"[a-z]")) score++;
            if (Regex.IsMatch(password, @"[A-Z]")) score++;
            if (Regex.IsMatch(password, @"[!@#$%^&*(),.?"":{}|<>]")) score++;

            IBrush color;
            string text;
            double widthPercent;

            if (string.IsNullOrEmpty(password))
            {
                color = Brushes.Gray;
                text = "";
                widthPercent = 0;
            }
            else if (score < 3)
            {
                color = Brushes.Red;
                text = "Weak";
                widthPercent = 33;
            }
            else if (score < 5)
            {
                color = Brushes.Orange;
                text = "Medium";
                widthPercent = 66;
            }
            else
            {
                color = Brushes.LimeGreen;
                text = "Strong";
                widthPercent = 100;
            }

            _strengthBorder.Background = color;
            _strengthBorder.Width = 250 * (widthPercent / 100);
            _strengthText.Text = text;
            _strengthText.Foreground = color;
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