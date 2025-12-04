using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

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

        private void OnCreateClick(object sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            var passwordBox = this.FindControl<TextBox>("PasswordBox");
            Password = passwordBox?.Text ?? "";
            Created = true;
            Close();
        }

        private void OnCancelClick(object sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            Created = false;
            Close();
        }
    }
}