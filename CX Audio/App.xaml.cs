using Microsoft.UI.Xaml;

namespace CXAudioOutput;

public partial class App : Application
{
    private MainWindow? _window;

    public App()
    {
        StartupTrace.Write("App constructor: entered");
        UnhandledException += (_, error) => StartupTrace.Write("Unhandled XAML exception: " + error.Exception);
        // Resolve WinUI's own labels in Chinese even on an English Windows install.
        Microsoft.Windows.Globalization.ApplicationLanguages.PrimaryLanguageOverride = "zh-CN";
        System.Globalization.CultureInfo.DefaultThreadCurrentUICulture =
            System.Globalization.CultureInfo.GetCultureInfo("zh-CN");
        StartupTrace.Write("App constructor: before InitializeComponent");
        InitializeComponent();
        StartupTrace.Write("App constructor: InitializeComponent completed");
    }

    public static LaunchOptions Options { get; } = LaunchOptions.Parse(
        Environment.GetCommandLineArgs().Skip(1).ToArray());

    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {
        StartupTrace.Write($"OnLaunched: creating MainWindow, demo={Options.Demo}");
        MainWindow.InitializeAccentResources();
        _window = new MainWindow(Options);
        StartupTrace.Write("OnLaunched: activating MainWindow");
        _window.Activate();
        StartupTrace.Write("OnLaunched: MainWindow activated");
    }
}
