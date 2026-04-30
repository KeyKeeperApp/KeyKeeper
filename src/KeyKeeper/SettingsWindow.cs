using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;

namespace KeyKeeper.Views;

public class SettingsWindow : Window
{
    public SettingsWindow()
    {
        // Базовые параметры окна
        this.Title = "Settings";
        this.Width = 450;
        this.Height = 250;
        this.MinWidth = 450;
        this.MinHeight = 250;
        this.WindowStartupLocation = WindowStartupLocation.CenterOwner;
        this.Padding = new Thickness(25);

        // Контейнер, который выравнивает элементы по вертикали
        var mainStack = new StackPanel
        {
            Spacing = 15,
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Top
        };

        // Заголовок окна
        var titleText = new TextBlock
        {
            Text = "App Settings",
            Foreground = this.FindResource("KeyKeeperHeadingTextBrush") as IBrush,
            FontSize = 20,
            FontWeight = FontWeight.Bold,
            Margin = new Thickness(0, 0, 0, 10)
        };

        // Чекбокс (Галочка)
        var exitOnCloseCheckBox = new CheckBox
        {
            Content = "Exit KeyKeeper when closing vault",
            FontSize = 14,
            // Подгружаем сохраненное состояние из статического класса
            IsChecked = AppSettings.ExitOnRepositoryClose
        };

        // Событие: когда пользователь щелкает по галочке, данные сразу улетают в AppSettings
        exitOnCloseCheckBox.IsCheckedChanged += (s, e) =>
        {
            AppSettings.ExitOnRepositoryClose = exitOnCloseCheckBox.IsChecked ?? false;
            AppSettings.Save();
        };

        // Добавляем элементы в стек
        mainStack.Children.Add(titleText);
        mainStack.Children.Add(exitOnCloseCheckBox);

        // Назначаем стек основным контентом окна
        this.Content = mainStack;
    }
}
