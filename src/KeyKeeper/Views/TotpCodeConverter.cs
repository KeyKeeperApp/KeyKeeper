using System;
using System.Collections.Generic;
using System.Globalization;
using Avalonia.Data.Converters;
using KeyKeeper.PasswordStore;
using KeyKeeper.ViewModels;

namespace KeyKeeper.Views;

public class TotpCodeConverter : IMultiValueConverter
{
    public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
    {
        if (values.Count < 2)
            return "";

        var entry = values[0] as PassStoreEntryPassword;
        var viewModel = values[1] as UnlockedRepositoryViewModel;

        if (entry == null || viewModel == null)
            return "";

        return viewModel.GetTotpCode(entry);
    }
}
