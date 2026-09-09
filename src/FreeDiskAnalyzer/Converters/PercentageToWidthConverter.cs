using System.Globalization;
using System.Windows.Data;

namespace FreeDiskAnalyzer.Converters;

/// <summary>
/// Converts a 0-100 percentage into a pixel width, given the max width as
/// ConverterParameter. Used for the storage-by-category bars, which sit in a
/// fixed-width track rather than a dynamically measured one, simpler and
/// more predictable than a multi-binding on ActualWidth.
/// </summary>
public sealed class PercentageToWidthConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not double percentage) return 0.0;

        var maxWidth = parameter is string s && double.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, out var parsed)
            ? parsed
            : 200.0;

        return Math.Clamp(percentage, 0, 100) / 100.0 * maxWidth;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}
