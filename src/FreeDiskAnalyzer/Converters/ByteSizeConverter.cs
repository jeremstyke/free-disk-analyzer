using System.Globalization;
using System.Windows.Data;
using FreeDiskAnalyzer.Core.Utilities;

namespace FreeDiskAnalyzer.Converters;

public sealed class ByteSizeConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        value is long bytes ? ByteSizeFormatter.Format(bytes) : string.Empty;

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}
