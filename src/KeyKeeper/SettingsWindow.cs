using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Media;

namespace KeyKeeper.Views;
public class SettingsWindow : Window
{
    private async void OpenAbout()
    {
        var AboutWindow = new AboutWindow();
        await AboutWindow.ShowDialog(this);
    }

    public SettingsWindow()
    {
        this.Title = "Настройки";
        this.Width = 400;
        this.Height = 300;
        var AboutButton = new Button
        {
            Content = "О приложении",
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
            FontSize = 16
        };

        AboutButton.Click += (sender, e) => OpenAbout();

        this.Content = AboutButton;
    }
}
