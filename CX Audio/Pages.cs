using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace CXAudioOutput;

internal static class PageLayout
{
    public static void Populate(StackPanel body, string title, params FrameworkElement[] panels)
    {
        if (body.Children.Count != 0) return;
        body.Children.Add(new TextBlock
        {
            Text = title,
            FontSize = 28,
            FontWeight = Microsoft.UI.Text.FontWeights.SemiBold,
            Margin = new Thickness(0, 0, 0, 8)
        });

        foreach (var panel in panels)
        {
            StartupTrace.Write($"PageLayout: attaching {panel.Name}");
            if (panel.Parent is not null)
                throw new InvalidOperationException($"Page content {panel.Name} already has a parent.");
            Grid.SetColumn(panel, 0);
            Grid.SetRow(panel, 0);
            body.Children.Add(panel);
            StartupTrace.Write($"PageLayout: attached {panel.Name}");
        }
    }
}

public sealed partial class OverviewPage : Page
{
    public OverviewPage()
    {
        InitializeComponent();
        NavigationCacheMode = NavigationCacheMode.Required;
    }
    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        StartupTrace.Write("Page navigated: OverviewPage");
        var host = (MainWindow)e.Parameter;
        PageLayout.Populate(Body, "概览", host.OverviewContent);
    }
}

public sealed partial class ConnectionsPage : Page
{
    public ConnectionsPage()
    {
        InitializeComponent();
        NavigationCacheMode = NavigationCacheMode.Required;
    }
    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        StartupTrace.Write("Page navigated: ConnectionsPage");
        var host = (MainWindow)e.Parameter;
        PageLayout.Populate(Body, "无线连接", host.ConnectionsContent);
    }
}

public sealed partial class AndroidPage : Page
{
    public AndroidPage()
    {
        InitializeComponent();
        NavigationCacheMode = NavigationCacheMode.Required;
    }
    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        StartupTrace.Write("Page navigated: AndroidPage");
        var host = (MainWindow)e.Parameter;
        PageLayout.Populate(Body, "Android 有线", host.AndroidContent);
    }
}

public sealed partial class VirtualSpeakerPage : Page
{
    public VirtualSpeakerPage()
    {
        InitializeComponent();
        NavigationCacheMode = NavigationCacheMode.Required;
    }
    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        var host = (MainWindow)e.Parameter;
        PageLayout.Populate(Body, "虚拟扬声器（高级）", host.VirtualSpeakerContent);
    }
}

public sealed partial class SettingsPage : Page
{
    public SettingsPage()
    {
        InitializeComponent();
        NavigationCacheMode = NavigationCacheMode.Required;
    }
    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        StartupTrace.Write("Page navigated: SettingsPage");
        var host = (MainWindow)e.Parameter;
        PageLayout.Populate(Body, "设置", host.SettingsContent, host.SystemContent);
    }
}
