using System.Windows;
using FreeDiskAnalyzer.Models;

namespace FreeDiskAnalyzer.Themes;

/// <summary>
/// Swaps between light and dark color dictionaries at runtime. The colors
/// dictionary must stay at index 0 of Application.Resources.MergedDictionaries
/// (set up that way in App.xaml) so this can replace it directly. The actual
/// Settings UI to let the user pick a theme is added in a later phase; this
/// is the mechanism it will call into.
/// </summary>
public static class ThemeManager
{
    private const int ColorsDictionaryIndex = 0;

    public static void ApplyTheme(ThemeMode mode)
    {
        var app = Application.Current;
        if (app is null) return;

        var source = mode switch
        {
            ThemeMode.Dark => new Uri("Themes/Colors.Dark.xaml", UriKind.Relative),
            _ => new Uri("Themes/Colors.Light.xaml", UriKind.Relative)
        };

        var newDictionary = new ResourceDictionary { Source = source };

        var merged = app.Resources.MergedDictionaries;
        if (merged.Count > ColorsDictionaryIndex)
        {
            merged[ColorsDictionaryIndex] = newDictionary;
        }
        else
        {
            merged.Add(newDictionary);
        }
    }
}
