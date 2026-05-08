using System;
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
        };

        // Настройка таймера блокировки
        var lockTimerDurationRow = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 12,
        };

        var lockTimerDurationLabel1 = new TextBlock
        {
            Text = "Lock the vault after",
            VerticalAlignment = VerticalAlignment.Center,
        };

        var lockTimerDurationInput = new NumericUpDown
        {
            Value = AppSettings.LockTimerMinutes,
            Increment = 1,
            Minimum = 1,
            Maximum = 90,
            ClipValueToMinMax = true,
            Width = 120,
        };

        var lockTimerDurationLabel2 = new TextBlock
        {
            Text = "minutes of inactivity",
            VerticalAlignment = VerticalAlignment.Center,
        };

        lockTimerDurationInput.ValueChanged += (_, _) =>
        {
            Console.WriteLine($"Set timer to {lockTimerDurationInput.Value} minutes");
            AppSettings.LockTimerMinutes = (int)(lockTimerDurationInput.Value ?? 5m);
        };

        lockTimerDurationRow.Children.Add(lockTimerDurationLabel1);
        lockTimerDurationRow.Children.Add(lockTimerDurationInput);
        lockTimerDurationRow.Children.Add(lockTimerDurationLabel2);

        // Добавляем элементы в стек
        mainStack.Children.Add(titleText);
        mainStack.Children.Add(exitOnCloseCheckBox);
        mainStack.Children.Add(lockTimerDurationRow);

        // Назначаем стек основным контентом окна
        this.Content = mainStack;
    }

    protected override void OnClosed(EventArgs e)
    {
        base.OnClosed(e);
        Console.WriteLine("Saving application settings");
        AppSettings.Save();
    }
}
