using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace KeyKeeper
{
    public partial class PasswordEntryDialog : Window
    {
        public string Password { get; private set; } = "";
        public bool Opened { get; private set; } = false;

        public PasswordEntryDialog()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        private void OpenButton_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            var passwordBox = this.FindControl<TextBox>("PasswordBox");
            Password = passwordBox?.Text ?? "";
            Opened = true;
            Close();
        }

        private void CancelButton_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            Opened = false;
            Close();
        }
    }
}