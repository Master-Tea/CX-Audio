using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Animation;
using Windows.UI;

namespace CXAudioOutput;

public sealed partial class MainWindow
{
    private readonly StackPanel _overviewContent = new() { Spacing = 16 };
    private readonly StackPanel _settingsContent = new() { Spacing = 14 };
    private readonly TextBlock _overviewService = new();
    private readonly TextBlock _overviewAudio = new() { TextWrapping = TextWrapping.Wrap };
    private readonly TextBlock _overviewNetwork = new() { TextWrapping = TextWrapping.Wrap };
    private readonly TextBlock _overviewAndroid = new() { TextWrapping = TextWrapping.Wrap };
    private Button? _startServiceButton;
    private Button? _firewallButton;
    private ToggleSwitch? _micaSystemSwitch;
    private Slider? _micaOpacitySlider;
    private TextBlock? _micaOpacityText;
    private bool _settingsInitializing;
    private string _micaOpacitySetting = "System";
    private bool _navigationReady;
    private bool _navigationChanging;
    private bool _startingServer;

    internal FrameworkElement OverviewContent => _overviewContent;
    internal FrameworkElement AudioContent => _audioView;
    internal FrameworkElement ConnectionsContent => _connectionsView;
    internal FrameworkElement AndroidContent => _androidView;
    internal FrameworkElement SettingsContent => _settingsContent;
    internal FrameworkElement SystemContent => _systemView;

    private void InitializeNavigation()
    {
        // NavigationView owns the visible selection; clear the hidden Frame
        // history after each switch so it cannot grow indefinitely.
        var settings = _server?.Settings.Load() ?? new AppSettings();
        _micaOpacitySetting = NormalizeMicaOpacity(settings.MicaOpacity);
        ApplyTheme(settings.Theme);
        ApplyAccent(settings.AccentColor);
        ApplyMicaOpacity(_micaOpacitySetting);
        if (Navigation.SettingsItem is NavigationViewItem settingsItem)
        {
            settingsItem.Content = "设置";
            Microsoft.UI.Xaml.Automation.AutomationProperties.SetName(settingsItem, "设置");
            ToolTipService.SetToolTip(settingsItem, "设置");
        }

        _overviewContent.Children.Add(new TextBlock { Text = "服务状态", FontSize = 18, FontWeight = Microsoft.UI.Text.FontWeights.SemiBold });
        _overviewContent.Children.Add(_overviewService);
        _overviewContent.Children.Add(_overviewAudio);
        _overviewContent.Children.Add(_overviewNetwork);
        _overviewContent.Children.Add(_overviewAndroid);
        _overviewContent.Children.Add(_audioView);
        var actions = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 8 };
        _startServiceButton = new Button { Content = "启动服务" };
        _startServiceButton.Click += async (_, _) => await StartServerAsync();
        actions.Children.Add(_startServiceButton);
        var pause = new Button { Content = "暂停 / 继续" };
        pause.Click += Pause_Click;
        actions.Children.Add(pause);
        var connections = new Button { Content = "无线连接" };
        connections.Click += (_, _) => Navigation.SelectedItem = Navigation.MenuItems[1];
        actions.Children.Add(connections);
        var android = new Button { Content = "Android 有线" };
        android.Click += (_, _) => Navigation.SelectedItem = Navigation.MenuItems[2];
        actions.Children.Add(android);
        _overviewContent.Children.Add(actions);

        _settingsInitializing = true;
        var autoStart = new ToggleSwitch { Header = "启动时自动启动音频服务", OnContent = "开", OffContent = "关", IsOn = settings.AutoStartService };
        autoStart.Toggled += (_, _) =>
        {
            if (!_settingsInitializing) SaveSettings(value => value.AutoStartService = autoStart.IsOn);
        };
        _settingsContent.Children.Add(autoStart);
        InitializePortSettings(settings);

        var openBrowser = new Button
        {
            Content = "打开网页控制台",
            HorizontalAlignment = HorizontalAlignment.Left
        };
        openBrowser.Click += OpenBrowser_Click;
        _settingsContent.Children.Add(openBrowser);

        var theme = new ComboBox { Header = "外观", HorizontalAlignment = HorizontalAlignment.Stretch };
        theme.Items.Add(new ComboBoxItem { Content = "跟随系统", Tag = "System" });
        theme.Items.Add(new ComboBoxItem { Content = "浅色", Tag = "Light" });
        theme.Items.Add(new ComboBoxItem { Content = "深色", Tag = "Dark" });
        theme.SelectedIndex = settings.Theme switch { "Light" => 1, "Dark" => 2, _ => 0 };
        theme.SelectionChanged += (_, _) =>
        {
            if (_settingsInitializing) return;
            var selected = (theme.SelectedItem as ComboBoxItem)?.Tag as string ?? "System";
            ApplyTheme(selected);
            SaveSettings(value => value.Theme = selected);
        };
        _settingsContent.Children.Add(theme);

        InitializeAccentSettings(settings);

        _micaSystemSwitch = new ToggleSwitch
        {
            Header = "云母透明度跟随系统",
            OnContent = "开", OffContent = "关",
            IsOn = string.Equals(_micaOpacitySetting, "System", StringComparison.OrdinalIgnoreCase)
        };
        _micaSystemSwitch.Toggled += MicaSystem_Toggled;
        _settingsContent.Children.Add(_micaSystemSwitch);
        _micaOpacitySlider = new Slider
        {
            Header = "云母透明度",
            Minimum = 0.35,
            Maximum = 1,
            StepFrequency = 0.05,
            Value = TryParseMicaOpacity(_micaOpacitySetting, out var savedOpacity) ? savedOpacity : 0.82,
            IsEnabled = !_micaSystemSwitch.IsOn,
            Width = 280
        };
        _micaOpacitySlider.ValueChanged += MicaOpacity_ValueChanged;
        _settingsContent.Children.Add(_micaOpacitySlider);
        _micaOpacityText = new TextBlock { Text = FormatMicaOpacity(_micaOpacitySetting), Style = (Style)Application.Current.Resources["CaptionStyle"] };
        _settingsContent.Children.Add(_micaOpacityText);

        _firewallButton = new Button
        {
            Content = "允许局域网访问（高级功能，需要管理员权限）",
            HorizontalAlignment = HorizontalAlignment.Left
        };
        _firewallButton.Click += AllowFirewall_Click;
        _settingsContent.Children.Add(_firewallButton);
        _settingsContent.Children.Add(new TextBlock { Text = "高级功能只在你主动点击时请求 Windows 管理员权限。普通启动不会弹出 UAC。", Style = (Style)Application.Current.Resources["CaptionStyle"], TextWrapping = TextWrapping.Wrap });
        _settingsContent.Children.Add(new TextBlock { Text = "关于", FontSize = 18, FontWeight = Microsoft.UI.Text.FontWeights.SemiBold, Margin = new Thickness(0, 12, 0, 0) });
        _settingsContent.Children.Add(new TextBlock { Text = "CX Audio · " + (typeof(App).Assembly.GetName().Version?.ToString(3) ?? "1.3"), TextWrapping = TextWrapping.Wrap });
        _settingsInitializing = false;

        InitializeVirtualSpeaker();
        UpdateOverview();
    }

    private void StartNavigation()
    {
        if (_navigationReady) return;
        _navigationReady = true;
        StartupTrace.Write("StartNavigation: selecting overview");
        Navigation.SelectedItem = OverviewNavigationItem;
        if (ContentFrame.CurrentSourcePageType is null)
        {
            StartupTrace.Write("StartNavigation: navigating to overview");
            var navigated = ContentFrame.Navigate(typeof(OverviewPage), this, new EntranceNavigationTransitionInfo());
            if (navigated) ContentFrame.BackStack.Clear();
        }
        StartupTrace.Write("StartNavigation: selection completed");
        UpdateOverview();
    }

    private async void AllowFirewall_Click(object sender, RoutedEventArgs e)
    {
        if (_server is null || _firewallButton is null || _closed) return;
        _firewallButton.IsEnabled = false;
        try
        {
            var result = await FirewallRuleService.EnsureInboundPortAsync(_options.Port, _lifetimeCts.Token);
            if (!_closed)
            {
                ShowNotice(result.Allowed ? InfoBarSeverity.Success : InfoBarSeverity.Warning,
                    result.Allowed ? "局域网访问已允许" : "局域网访问未启用", result.Message);
            }
        }
        catch (OperationCanceledException) when (_closed || _lifetimeCts.IsCancellationRequested)
        {
        }
        catch (Exception ex)
        {
            if (!_closed) ShowNotice(InfoBarSeverity.Error, "无法配置 Windows 防火墙", ex.Message);
        }
        finally
        {
            if (!_closed && _firewallButton is not null) _firewallButton.IsEnabled = true;
        }
    }

    private void MicaSystem_Toggled(object sender, RoutedEventArgs e)
    {
        if (_settingsInitializing || _micaSystemSwitch is null || _micaOpacitySlider is null) return;
        _micaOpacitySlider.IsEnabled = !_micaSystemSwitch.IsOn;
        _micaOpacitySetting = _micaSystemSwitch.IsOn
            ? "System"
            : _micaOpacitySlider.Value.ToString("0.##", System.Globalization.CultureInfo.InvariantCulture);
        ApplyMicaOpacity(_micaOpacitySetting);
        SaveSettings(value => value.MicaOpacity = _micaOpacitySetting);
    }

    private void MicaOpacity_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
    {
        if (_settingsInitializing || _micaSystemSwitch?.IsOn == true) return;
        _micaOpacitySetting = e.NewValue.ToString("0.##", System.Globalization.CultureInfo.InvariantCulture);
        ApplyMicaOpacity(_micaOpacitySetting);
        SaveSettings(value => value.MicaOpacity = _micaOpacitySetting);
    }

    private void ApplyTheme(string? theme)
    {
        RootGrid.RequestedTheme = theme switch
        {
            "Light" => ElementTheme.Light,
            "Dark" => ElementTheme.Dark,
            _ => ElementTheme.Default
        };
        ApplyMicaOpacity(_micaOpacitySetting);
        ApplyAccent(_accentSetting);
    }

    private void ApplyMicaOpacity(string? value)
    {
        _micaOpacitySetting = NormalizeMicaOpacity(value);
        var isDark = RootGrid.ActualTheme == ElementTheme.Dark || RootGrid.RequestedTheme == ElementTheme.Dark;
        var baseColor = isDark ? Color.FromArgb(255, 32, 32, 32) : Color.FromArgb(255, 245, 246, 247);
        var alpha = TryParseMicaOpacity(_micaOpacitySetting, out var opacity)
            ? (byte)Math.Clamp((int)Math.Round(opacity * 255), 0, 255)
            : (byte)0;
        RootGrid.Background = new SolidColorBrush(Color.FromArgb(alpha, baseColor.R, baseColor.G, baseColor.B));
        if (_micaOpacityText is not null) _micaOpacityText.Text = FormatMicaOpacity(_micaOpacitySetting);
    }

    private static string NormalizeMicaOpacity(string? value)
    {
        if (string.IsNullOrWhiteSpace(value) || string.Equals(value, "System", StringComparison.OrdinalIgnoreCase)) return "System";
        return TryParseMicaOpacity(value, out var opacity)
            ? opacity.ToString("0.##", System.Globalization.CultureInfo.InvariantCulture)
            : "System";
    }

    private static bool TryParseMicaOpacity(string? value, out double opacity)
    {
        return double.TryParse(value, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out opacity)
            && opacity >= 0.35 && opacity <= 1;
    }

    private static string FormatMicaOpacity(string value)
    {
        return TryParseMicaOpacity(value, out var opacity)
            ? $"当前云母遮罩：{Math.Round(opacity * 100):0}%"
            : "当前云母遮罩：跟随系统";
    }

    private static bool TryParseColor(string? value, out Color color)
    {
        color = default;
        if (string.IsNullOrWhiteSpace(value) || string.Equals(value, "System", StringComparison.OrdinalIgnoreCase)) return false;
        var hex = value.Trim().TrimStart('#');
        if (hex.Length != 6 && hex.Length != 8) return false;
        try
        {
            var alpha = hex.Length == 8 ? Convert.ToByte(hex[..2], 16) : (byte)255;
            var redStart = hex.Length == 8 ? 2 : 0;
            color = Color.FromArgb(alpha,
                Convert.ToByte(hex.Substring(redStart, 2), 16),
                Convert.ToByte(hex.Substring(redStart + 2, 2), 16),
                Convert.ToByte(hex.Substring(redStart + 4, 2), 16));
            return true;
        }
        catch
        {
            return false;
        }
    }

    private static string ToColorHex(Color color) => $"#{color.R:X2}{color.G:X2}{color.B:X2}";

    private bool SaveSettings(Action<AppSettings> mutate)
    {
        if (_server is null) return false;
        if (_server.Settings.Update(mutate)) return true;
        if (!_closed) ShowNotice(InfoBarSeverity.Warning, "设置未保存", "无法写入设置文件，本次运行仍会使用当前选项。");
        return false;
    }

    private void Navigation_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
    {
        if (!_navigationReady || _closed || _navigationChanging) return;
        var page = args.IsSettingsSelected ? typeof(SettingsPage) : (args.SelectedItemContainer?.Tag as string) switch
        {
            "connections" => typeof(ConnectionsPage),
            "android" => typeof(AndroidPage),
            "virtual-speaker" => typeof(VirtualSpeakerPage),
            _ => typeof(OverviewPage)
        };
        if (ContentFrame.CurrentSourcePageType == page) return;

        _navigationChanging = true;
        try
        {
            StartupTrace.Write($"Navigation selection: navigating to {page.Name}");
            var navigated = ContentFrame.Navigate(page, this, args.RecommendedNavigationTransitionInfo);
            if (navigated) ContentFrame.BackStack.Clear();
            StartupTrace.Write($"Navigation selection: {page.Name} result={navigated}");
        }
        finally
        {
            _navigationChanging = false;
        }
    }

    private void UpdateOverview()
    {
        if (!_navigationReady || _closed) return;
        _overviewService.Text = "服务：" + ServicePillText.Text;
        _overviewAudio.Text = "音频：" + SourceText.Text;
        _overviewNetwork.Text = "网络：" + NetworkSummaryText.Text + " · 接收设备 " + ReceiverCountText.Text;
        _overviewAndroid.Text = "Android：" + AndroidWiredPillText.Text;
        if (_startServiceButton is not null) _startServiceButton.Visibility = _serverStarted ? Visibility.Collapsed : Visibility.Visible;
    }
}
