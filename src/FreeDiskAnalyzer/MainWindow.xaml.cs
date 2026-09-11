using System.ComponentModel;
using System.Windows;
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
        var animation = new DoubleAnimation(0, 1, new Duration(TimeSpan.FromSeconds(0.18)))
        {
            EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
        };
        PageContent.BeginAnimation(OpacityProperty, animation);
    }
}
