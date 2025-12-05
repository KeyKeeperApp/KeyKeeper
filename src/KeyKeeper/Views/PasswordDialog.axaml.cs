using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Interactivity;

namespace KeyKeeper.Views
{
    public partial class PasswordDialog : Window
    {
        public string Password { get; private set; } = "";
        public bool Created { get; private set; } = false;

        public PasswordDialog()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        private void CreateButton_Click(object sender, RoutedEventArgs e)
        {
            var passwordBox = this.FindControl<TextBox>("PasswordBox");
            var confirmBox = this.FindControl<TextBox>("ConfirmPasswordBox");
            var errorText = this.FindControl<TextBlock>("ErrorText");

            string password = passwordBox?.Text ?? "";
            string confirmPassword = confirmBox?.Text ?? "";

            if (string.IsNullOrEmpty(password) || string.IsNullOrEmpty(confirmPassword))
            {
                ShowError("Password cannot be empty");
                return;
            }

            if (password != confirmPassword)
            {
                ShowError("Passwords don't match");
                return;
            }

            Password = password;
            Created = true;
            Close();
        }

        private void ShowError(string message)
        {
            var errorText = this.FindControl<TextBlock>("ErrorText");
            if (errorText != null)
            {
                errorText.Text = message;
                errorText.IsVisible = true;
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            Created = false;
            Close();
        }
    }
}
