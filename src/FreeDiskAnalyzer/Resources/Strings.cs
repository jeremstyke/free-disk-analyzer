using System.Globalization;
using System.Resources;

namespace FreeDiskAnalyzer.Resources;

/// <summary>
/// Wraps <see cref="ResourceManager"/> over Strings.resx / Strings.fr.resx.
/// Hand-written rather than the Visual Studio auto-generated Designer.cs, so
/// it builds correctly with a plain `dotnet build` with no design-time
/// tooling involved. Language changes apply on next launch, see
/// AppSettings.Language and App.xaml.cs.
/// </summary>
public static class Strings
{
    private static readonly ResourceManager ResourceManager =
        new("FreeDiskAnalyzer.Resources.Strings", typeof(Strings).Assembly);

    public static string AppTitle => Get(nameof(AppTitle));

    public static string Nav_Dashboard => Get(nameof(Nav_Dashboard));
    public static string Nav_Analyze => Get(nameof(Nav_Analyze));
    public static string Nav_LargeFiles => Get(nameof(Nav_LargeFiles));
    public static string Nav_Folders => Get(nameof(Nav_Folders));
    public static string Nav_Settings => Get(nameof(Nav_Settings));
    public static string Nav_Privacy => Get(nameof(Nav_Privacy));
    public static string Nav_About => Get(nameof(Nav_About));

    public static string Dashboard_Title => Get(nameof(Dashboard_Title));
    public static string Dashboard_Subtitle => Get(nameof(Dashboard_Subtitle));
    public static string Dashboard_Storage => Get(nameof(Dashboard_Storage));
    public static string Dashboard_Used => Get(nameof(Dashboard_Used));
    public static string Dashboard_Free => Get(nameof(Dashboard_Free));
    public static string Dashboard_LastScan => Get(nameof(Dashboard_LastScan));
    public static string Dashboard_NoScanYet => Get(nameof(Dashboard_NoScanYet));
    public static string Dashboard_StorageByCategory => Get(nameof(Dashboard_StorageByCategory));
    public static string Dashboard_NordVpnTitle => Get(nameof(Dashboard_NordVpnTitle));
    public static string Dashboard_NordVpnBody => Get(nameof(Dashboard_NordVpnBody));
    public static string Dashboard_LearnMore => Get(nameof(Dashboard_LearnMore));
    public static string Dashboard_AffiliateDisclosure => Get(nameof(Dashboard_AffiliateDisclosure));
    public static string Dashboard_UsedOf => Get(nameof(Dashboard_UsedOf));

    public static string Analyze_Title => Get(nameof(Analyze_Title));
    public static string Analyze_Subtitle => Get(nameof(Analyze_Subtitle));
    public static string Analyze_Drive => Get(nameof(Analyze_Drive));
    public static string Analyze_StartScan => Get(nameof(Analyze_StartScan));
    public static string Analyze_Cancel => Get(nameof(Analyze_Cancel));
    public static string Analyze_Scanning => Get(nameof(Analyze_Scanning));
    public static string Analyze_Files => Get(nameof(Analyze_Files));
    public static string Analyze_Folders => Get(nameof(Analyze_Folders));
    public static string Analyze_Size => Get(nameof(Analyze_Size));
    public static string Analyze_Elapsed => Get(nameof(Analyze_Elapsed));
    public static string Analyze_DeleteMeTitle => Get(nameof(Analyze_DeleteMeTitle));
    public static string Analyze_DeleteMeBody => Get(nameof(Analyze_DeleteMeBody));

    public static string LargeFiles_Title => Get(nameof(LargeFiles_Title));
    public static string LargeFiles_Subtitle => Get(nameof(LargeFiles_Subtitle));
    public static string LargeFiles_MinSize => Get(nameof(LargeFiles_MinSize));
    public static string LargeFiles_Custom => Get(nameof(LargeFiles_Custom));
    public static string LargeFiles_NoScan => Get(nameof(LargeFiles_NoScan));
    public static string LargeFiles_Open => Get(nameof(LargeFiles_Open));
    public static string LargeFiles_ShowInExplorer => Get(nameof(LargeFiles_ShowInExplorer));

    public static string Folders_Title => Get(nameof(Folders_Title));
    public static string Folders_Subtitle => Get(nameof(Folders_Subtitle));
    public static string Folders_NoScan => Get(nameof(Folders_NoScan));

    public static string Settings_Title => Get(nameof(Settings_Title));
    public static string Settings_Subtitle => Get(nameof(Settings_Subtitle));
    public static string Settings_Language => Get(nameof(Settings_Language));
    public static string Settings_LanguageRestartNote => Get(nameof(Settings_LanguageRestartNote));
    public static string Settings_Theme => Get(nameof(Settings_Theme));
    public static string Settings_StartWithWindows => Get(nameof(Settings_StartWithWindows));
    public static string Settings_Analytics => Get(nameof(Settings_Analytics));
    public static string Settings_AnalyticsNote => Get(nameof(Settings_AnalyticsNote));
    public static string Settings_Reset => Get(nameof(Settings_Reset));

    public static string Privacy_Title => Get(nameof(Privacy_Title));
    public static string Privacy_Tagline => Get(nameof(Privacy_Tagline));

    public static string About_Tagline => Get(nameof(About_Tagline));
    public static string About_ViewGitHub => Get(nameof(About_ViewGitHub));
    public static string About_Version => Get(nameof(About_Version));
    public static string About_License => Get(nameof(About_License));

    public static string ComingSoon_Subtitle => Get(nameof(ComingSoon_Subtitle));

    private static string Get(string name) =>
        ResourceManager.GetString(name, CultureInfo.CurrentUICulture) ?? name;
}
