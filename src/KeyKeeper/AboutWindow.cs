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

        this.Background = new SolidColorBrush(Color.Parse("#2328c4"));

        var AboutKeyKeeper = new TextBlock
        {
            Text = "About Keykeeper",
            HorizontalAlignment = HorizontalAlignment.Left,
            FontSize = 50,
            TextAlignment = TextAlignment.Left
        };

        var AboutText = new TextBlock
        {
            Text = "KeyKeeper - личный менеджер паролей и ключей,\nгде пользователь сможет сохранять свои логины, пароли, адреса электронной почты, ключи восстановления с различных веб-сайтов и\nиз приложений и управлять ими. Ключи и пароли\nбудут храниться исключительно локально на\nкомпьютере пользователя и всё хранилище\nцеликом можно будет защитить мастер-паролем.",
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