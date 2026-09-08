using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace FreeDiskAnalyzer.Controls;

/// <summary>
/// Used/free donut ring. Draws the "used" arc via a rotated Ellipse and a
/// StrokeDashArray computed from the percentage, a standard lightweight way
/// to fake an arc in WPF without geometry math.
/// </summary>
public partial class DonutProgressRing : UserControl
{
    private const double Diameter = 140;
    private const double RingStrokeThickness = 14;

    public static readonly DependencyProperty UsedPercentageProperty = DependencyProperty.Register(
        nameof(UsedPercentage),
        typeof(double),
        typeof(DonutProgressRing),
        new PropertyMetadata(0.0, OnUsedPercentageChanged));

    public double UsedPercentage
    {
        get => (double)GetValue(UsedPercentageProperty);
        set => SetValue(UsedPercentageProperty, value);
    }

    public DonutProgressRing()
    {
        InitializeComponent();
        UpdateVisual(UsedPercentage);
    }

    private static void OnUsedPercentageChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is DonutProgressRing ring)
        {
            ring.UpdateVisual((double)e.NewValue);
        }
    }

    private void UpdateVisual(double percentage)
    {
        percentage = Math.Clamp(percentage, 0, 100);

        // StrokeDashArray values are expressed in units of the stroke
        // thickness, not pixels, hence dividing the circumference by it.
        var circumference = Math.PI * Diameter;
        var totalDashUnits = circumference / RingStrokeThickness;
        var usedUnits = totalDashUnits * (percentage / 100.0);
        var remainingUnits = Math.Max(totalDashUnits - usedUnits, 0);

        ProgressArc.StrokeDashArray = new DoubleCollection { usedUnits, remainingUnits };
        PercentageText.Text = $"{percentage:0.#}%";
    }
}
