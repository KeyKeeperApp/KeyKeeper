using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Media;

namespace KeyKeeper.Views;
public class SettingsWindow : Window
{
    public SettingsWindow()
    {
        this.Title = "Настройки";
        this.Width = 400;
        this.Height = 300;
        var textBlock = new TextBlock
        {
            Text = "Окно настроек",
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
            FontSize = 16
        };

        this.Content = textBlock;
    }
}
