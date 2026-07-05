using System.Globalization;
using Avalonia.Data.Converters;

namespace DiskGalaxy.UI.Converters;

public static class BoolConverters
{
    public static readonly IValueConverter Not = new NotConverter();
}

public static class ObjectConverters
{
    public static readonly IValueConverter IsNotNull = new IsNotNullConverter();
    public static readonly IValueConverter IsPositive = new IsPositiveConverter();
}

public sealed class NotConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value is bool b ? !b : value;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value is bool b ? !b : value;
    }
}

public sealed class IsNotNullConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value is not null;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}

public sealed class IsPositiveConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value is int i ? i > 0 : value is long l ? l > 0 : value is not null;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
