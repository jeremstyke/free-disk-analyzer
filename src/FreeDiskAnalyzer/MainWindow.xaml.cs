using System.ComponentModel;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;
using FreeDiskAnalyzer.ViewModels;

namespace FreeDiskAnalyzer;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        DataContextChanged += OnDataContextChanged;
    }

    private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        if (e.OldValue is MainViewModel oldVm)
        {
            oldVm.PropertyChanged -= OnMainViewModelPropertyChanged;
        }

        if (e.NewValue is MainViewModel newVm)
        {
            newVm.PropertyChanged += OnMainViewModelPropertyChanged;
        }
    }

    private void OnMainViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(MainViewModel.CurrentPage))
        {
            AnimatePageTransition();
        }
    }

    private void AnimatePageTransition()
    {
        PageContent.Opacity = 0;
        var slideTransform = new TranslateTransform(24, 0);
        PageContent.RenderTransform = slideTransform;

        var fade = new DoubleAnimation(0, 1, new Duration(TimeSpan.FromSeconds(0.22)))
        {
            EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
        };
        var slide = new DoubleAnimation(24, 0, new Duration(TimeSpan.FromSeconds(0.28)))
        {
            EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
        };

        PageContent.BeginAnimation(OpacityProperty, fade);
        slideTransform.BeginAnimation(TranslateTransform.XProperty, slide);
    }
}
