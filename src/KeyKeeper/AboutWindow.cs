using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;

namespace KeyKeeper.Views;
public class AboutWindow : Window
{
    public AboutWindow()
    {
        this.Title = "О приложении";
        this.Width = 600;
        this.Height = 400;

        var AboutKeyKeeper = new TextBlock
        {
            Text = "About Keykeeper",
            HorizontalAlignment = HorizontalAlignment.Left,
            FontSize = 50,
            TextAlignment = TextAlignment.Left
        };

        var AboutText = new TextBlock
        {
            Text = "KeyKeeper is a personal password and key manager\nwhere you can save passwords and other login\ninformation, configure one-time code generation,\nand create encryption keys for personal use",
            HorizontalAlignment = HorizontalAlignment.Left,
            FontSize = 16,
            TextAlignment = TextAlignment.Left,
            Margin = new Thickness(0, 20, 0, 0)
        };

        var mainGrid = new Grid
        {
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Center
        };

        var innerStack = new StackPanel
        {
            Width = 400
        };

        innerStack.Children.Add(AboutKeyKeeper);
        innerStack.Children.Add(AboutText);

        mainGrid.Children.Add(innerStack);

        this.Content = mainGrid;
    }
}