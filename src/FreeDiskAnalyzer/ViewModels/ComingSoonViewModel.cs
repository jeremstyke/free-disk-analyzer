using CommunityToolkit.Mvvm.ComponentModel;

namespace FreeDiskAnalyzer.ViewModels;

/// <summary>
/// Placeholder shown for sidebar sections not implemented yet (Analyze,
/// Large Files, Folders, Utilities, Settings, Privacy, About). Each gets
/// replaced by a real view model in later phases per docs/ROADMAP.md.
/// </summary>
public sealed partial class ComingSoonViewModel : ObservableObject
{
    public string Title { get; }

    public ComingSoonViewModel(string title)
    {
        Title = title;
    }
}
