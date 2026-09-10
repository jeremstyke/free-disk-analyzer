using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using FreeDiskAnalyzer.Core.Models;

namespace FreeDiskAnalyzer.Converters;

public sealed class RiskLevelToBrushConverter : IValueConverter
{
    private static readonly SolidColorBrush LowBrush = new(Color.FromRgb(0x16, 0xA3, 0x4A));    // green
    private static readonly SolidColorBrush MediumBrush = new(Color.FromRgb(0xF5, 0x9E, 0x0B));  // orange
    private static readonly SolidColorBrush HighBrush = new(Color.FromRgb(0xDC, 0x26, 0x26));    // red

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        value is RiskLevel level
            ? level switch
            {
                RiskLevel.Low => LowBrush,
                RiskLevel.Medium => MediumBrush,
                RiskLevel.High => HighBrush,
                _ => MediumBrush
            }
            : MediumBrush;

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}
