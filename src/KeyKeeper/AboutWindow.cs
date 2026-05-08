using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;

namespace KeyKeeper.Views;
public class AboutWindow : Window
{
    public AboutWindow()
    {
        this.Title = "About";
        this.Width = 600;
        this.Height = 400;

        this.MinWidth = 450;
        this.MinHeight = 250;

        this.Background = this.FindResource("KeyKeeperAboutWindowBackgroundBrush") as IBrush;
        this.Foreground = this.FindResource("KeyKeeperAboutWindowForegroundBrush") as IBrush;

        var AboutKeyKeeper = new TextBlock
        {
            Text = "About KeyKeeper",
            HorizontalAlignment = HorizontalAlignment.Left,
            FontSize = 45,
            TextAlignment = TextAlignment.Left
        };

        var AboutText = new TextBlock
        {
            Text = "KeyKeeper is a personal local password manager\n" +
                   "and a TOTP (Time-based One-Time Password) generator.\n",
            HorizontalAlignment = HorizontalAlignment.Left,
            FontSize = 16,
            TextAlignment = TextAlignment.Left,
            Margin = new Thickness(0, 20, 0, 0)
        };

        var AboutSmallText = new TextBlock
        {
            Text = "Authors: The KeyKeeper Team\nVersion: 2.0 (08.05.2026)",
            HorizontalAlignment = HorizontalAlignment.Left,
            FontSize = 12,
            TextAlignment = TextAlignment.Left,
            Margin = new Thickness(0, 30, 0, 0)
        };

        var mainGrid = new Grid
        {
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Center
        };

        var innerStack = new StackPanel
        {
            Width = 550
        };

        innerStack.Children.Add(AboutKeyKeeper);
        innerStack.Children.Add(AboutText);
        innerStack.Children.Add(AboutSmallText);

        mainGrid.Children.Add(innerStack);

        this.Content = mainGrid;
    }
}
