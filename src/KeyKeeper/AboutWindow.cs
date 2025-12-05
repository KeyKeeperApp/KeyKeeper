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

        this.Background = new SolidColorBrush(Color.Parse("#2328c4"));

        var AboutKeyKeeper = new TextBlock
        {
            Text = "About KeyKeeper",
            HorizontalAlignment = HorizontalAlignment.Left,
            FontSize = 45,
            TextAlignment = TextAlignment.Left
        };

        var AboutText = new TextBlock
        {
            Text = "KeyKeeper is a personal password and key manager\n" +
                   "where you can save passwords and other login\n" +
                   "information, configure one-time code generation,\n" +
                   "and create encryption keys for personal use.\n",
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
