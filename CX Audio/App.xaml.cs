using Microsoft.UI.Xaml;

namespace CXAudioOutput;

public partial class App : Application
{
    private MainWindow? _window;

    public App()
    {
        StartupTrace.Write("App constructor: before InitializeComponent");
        InitializeComponent();
        StartupTrace.Write("App constructor: InitializeComponent completed");
    }

    public static LaunchOptions Options { get; } = LaunchOptions.Parse(
        Environment.GetCommandLineArgs().Skip(1).ToArray());

    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {
        StartupTrace.Write($"OnLaunched: creating MainWindow, demo={Options.Demo}");
        _window = new MainWindow(Options);
        StartupTrace.Write("OnLaunched: activating MainWindow");
        _window.Activate();
        StartupTrace.Write("OnLaunched: MainWindow activated");
    }
}
