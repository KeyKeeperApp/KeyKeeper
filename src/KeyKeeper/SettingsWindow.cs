using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Media;

namespace KeyKeeper.Views;
public class SettingsWindow : Window
{
    public SettingsWindow()
    {
        this.Title = "Settings";
        this.MinWidth = 500;
        this.MinHeight = 400;
        this.Width = 400;
        this.Height = 300;
        var textBlock = new TextBlock
        {
            Text = "Settings window",
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
            FontSize = 16
        };

        this.Content = textBlock;
    }
}
