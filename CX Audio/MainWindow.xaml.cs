using System.Collections.ObjectModel;
using System.Diagnostics;
using CXAudioOutput.ViewModels;
using Microsoft.UI;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Shapes;
using Windows.ApplicationModel.DataTransfer;
using Windows.Graphics;
using Windows.Storage.Pickers;
using Windows.UI;
using Path = System.IO.Path;

namespace CXAudioOutput;

public sealed partial class MainWindow : Window
{
    private readonly LaunchOptions _options;
    private readonly AppServer? _server;
    private readonly string? _serverConstructionError;
    private readonly ObservableCollection<NetworkAdapterViewModel> _adapterItems = new();
    private readonly ObservableCollection<ReceiverViewModel> _receiverItems = new();
    private readonly Dictionary<string, NetworkAdapterViewModel> _adapterMap =
        new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, ReceiverViewModel> _receiverMap =
        new(StringComparer.OrdinalIgnoreCase);
    private readonly DispatcherQueueTimer _refreshTimer;
    private readonly DispatcherQueueTimer? _noticeTimer;
    private readonly CancellationTokenSource _lifetimeCts = new();

    private readonly SolidColorBrush _acidBrush = new();
    private readonly SolidColorBrush _accentPillBrush = new();
    private readonly SolidColorBrush _warningBrush = new(Color.FromArgb(255, 255, 193, 102));
    private readonly SolidColorBrush _dangerBrush = new(Color.FromArgb(255, 255, 116, 108));
    private readonly SolidColorBrush _mutedBrush = new(Color.FromArgb(255, 128, 128, 128));
    private readonly SolidColorBrush _meterBrush = new();
    private readonly SolidColorBrush _meterDimBrush = new();
    private readonly SolidColorBrush _baselineBrush = new(Color.FromArgb(40, 255, 255, 255));

    private bool _updating;
    private bool _loaded;
    private bool _serverStarted;
    private bool _closed;
    private string _networkSignature = string.Empty;
    private string _shareSignature = string.Empty;
    private string _qrUrl = string.Empty;
    private string _currentShareUrl = string.Empty;
    private int _qrGeneration;
    private List<ShareLink> _shareLinks = new();
    private double[]? _lastLevels;

    private bool _androidBusy;
    private bool _androidActionBusy;
    private bool _updatingAndroid;
    private DateTimeOffset _lastAndroidRefresh = DateTimeOffset.MinValue;
    private string _androidSelectedSerial = string.Empty;
    private string _androidReverseSerial = string.Empty;
    private string _androidActiveUrl = string.Empty;
    private string _androidQrUrl = string.Empty;
    private int _androidQrGeneration;

    public MainWindow(LaunchOptions options)
    {
        StartupTrace.Write("MainWindow constructor: before InitializeComponent");
        _options = options;
        InitializeComponent();
        StartupTrace.Write("MainWindow constructor: InitializeComponent completed");

        InitializeOwnedViews();
        Title = "CX Audio";
        ConfigureWindow();
        PortText.Text = options.Port.ToString();
        SettingsPathText.Text = AppPaths.SettingsFile;
        WebUrlText.Text = $"http://127.0.0.1:{options.Port}/studio";

        AdapterList.ItemsSource = _adapterItems;
        ReceiverList.ItemsSource = _receiverItems;

        try
        {
            StartupTrace.Write("MainWindow constructor: creating AppServer");
            _server = new AppServer(options);
            StartupTrace.Write("MainWindow constructor: AppServer created");
            _server.Network.Changed += Network_Changed;
            _server.Android.Changed += Android_Changed;
        }
        catch (Exception ex)
        {
            _serverConstructionError = ex.Message;
            ShowNotice(InfoBarSeverity.Error, "服务未能启动", ex.Message);
            SetServicePill(false, "启动失败");
        }

        _refreshTimer = DispatcherQueue.CreateTimer();
        _refreshTimer.Interval = TimeSpan.FromMilliseconds(500);
        _refreshTimer.Tick += (_, _) => RefreshEverything();

        _noticeTimer = DispatcherQueue.CreateTimer();
        _noticeTimer.Interval = TimeSpan.FromSeconds(4);
        _noticeTimer.IsRepeating = false;
        _noticeTimer.Tick += (_, _) => ErrorBar.IsOpen = false;

        RootGrid.Loaded += RootGrid_Loaded;
        Closed += Window_Closed;

        _updating = true;
        LatencyComboBox.SelectedIndex = 1;
        _updating = false;
        StartupTrace.Write("MainWindow constructor: updating initial state");
        UpdateNetwork(force: true);
        UpdateShare(force: true);
        UpdateAndroidUi();
        StartupTrace.Write("MainWindow constructor: initializing navigation");
        InitializeNavigation();
        StartupTrace.Write("MainWindow constructor: completed");
    }

    private void ConfigureWindow()
    {
        try
        {
            ExtendsContentIntoTitleBar = true;
            SetTitleBar(AppTitleBar);
            SystemBackdrop = new Microsoft.UI.Xaml.Media.MicaBackdrop();

            var appWindow = AppWindow;
            var display = DisplayArea.GetFromWindowId(appWindow.Id, DisplayAreaFallback.Primary);
            var width = Math.Min(1280, Math.Max(960, display.WorkArea.Width - 80));
            var height = Math.Min(860, Math.Max(700, display.WorkArea.Height - 80));
            var x = display.WorkArea.X + (display.WorkArea.Width - width) / 2;
            var y = display.WorkArea.Y + (display.WorkArea.Height - height) / 2;
            appWindow.MoveAndResize(new RectInt32(x, y, width, height));
            appWindow.Title = "CX Audio";
            appWindow.SetIcon(Path.Combine(AppContext.BaseDirectory, "Assets", "app.ico"));

            var titleBar = appWindow.TitleBar;
            titleBar.ExtendsContentIntoTitleBar = true;
            titleBar.ButtonBackgroundColor = Colors.Transparent;
            titleBar.ButtonInactiveBackgroundColor = Colors.Transparent;
            // Let Windows choose caption button colors for the active theme.
        }
        catch
        {
            // If the OS doesn't support a customized title bar, the default one is fine.
        }
    }

    private async void RootGrid_Loaded(object sender, RoutedEventArgs e)
    {
        if (_loaded) return;
        StartupTrace.Write("RootGrid loaded");
        _loaded = true;
        try
        {
            StartNavigation();
            _refreshTimer.Start();
            if (UiRegression.Enabled || _server?.Settings.Load().AutoStartService != false)
                await StartServerAsync();
            else if (!_closed)
            {
                SetServicePill(false, "未启动");
                UpdateOverview();
            }
#if DEBUG
            if (UiRegression.Enabled && !_closed) await RunUiRegressionAsync();
#endif
        }
        catch (OperationCanceledException) when (_closed || _lifetimeCts.IsCancellationRequested)
        {
            // Window shutdown cancels startup work deliberately.
        }
        catch (Exception ex)
        {
            StartupTrace.Write("RootGrid loaded failed: " + ex);
            if (!_closed) ShowNotice(InfoBarSeverity.Error, "界面初始化失败", ex.Message);
        }
    }

    private async Task StartServerAsync()
    {
        if (_serverStarted || _startingServer || _closed) return;
        if (_server is null)
        {
            if (!_closed && !string.IsNullOrWhiteSpace(_serverConstructionError))
            {
                ShowNotice(InfoBarSeverity.Error, "服务未能启动", _serverConstructionError);
            }

            return;
        }

        _startingServer = true;
        var cancellationToken = _lifetimeCts.Token;
        try
        {
            SetServicePill(false, "正在启动…");
            StartupTrace.Write("Starting AppServer and audio capture");
            await _server.StartAsync(cancellationToken);
            if (_closed || cancellationToken.IsCancellationRequested) return;

            StartupTrace.Write("AppServer started");
            _serverStarted = true;
            SetServicePill(true, $"运行中 · :{_options.Port}");
            ErrorBar.IsOpen = false;

            if (_closed || cancellationToken.IsCancellationRequested) return;

            UpdateNetwork(force: true);
            UpdateShare(force: true);
            RefreshEverything();
        }
        catch (OperationCanceledException) when (_closed || cancellationToken.IsCancellationRequested)
        {
            // Window shutdown cancels an in-flight server start.
        }
        catch (Exception ex)
        {
            _serverStarted = false;
            if (!_closed)
            {
                SetServicePill(false, "启动失败");
                ShowNotice(InfoBarSeverity.Error,
                    $"端口 {_options.Port} 无法监听",
                    "请检查端口是否被占用，或使用 --port 指定另一个端口。\n" + ex.Message);
            }
        }
        finally
        {
            _startingServer = false;
            if (!_closed) UpdateOverview();
        }
    }

    private void RefreshEverything()
    {
        if (_closed) return;
        if (_server is null) return;

        UpdateAudioStatus();
        UpdateNetwork();
        UpdateReceivers();
        UpdateShare();
        UpdateOverview();

        if (!UiRegression.Enabled && DateTimeOffset.UtcNow - _lastAndroidRefresh >= TimeSpan.FromSeconds(4))
        {
            _lastAndroidRefresh = DateTimeOffset.UtcNow;
            _ = RefreshAndroidAsync();
        }
    }

    private void UpdateAudioStatus()
    {
        var snapshot = _server!.State.Snapshot();
        var capture = snapshot.Capture;
        var status = _serverStarted ? capture.Status : "stopped";

        var (eyebrow, headline) = status switch
        {
            "stopped" => ("SYSTEM AUDIO · STOPPED", "音频服务未启动。"),
            "live" => ("SYSTEM AUDIO · LIVE", "正在把声音送出去。"),
            "paused" => ("SYSTEM AUDIO · PAUSED", "串流已暂停。"),
            "starting" => ("SYSTEM AUDIO · STARTING", "正在准备系统音频。"),
            "no-device" => ("SYSTEM AUDIO · NO DEVICE", "没有找到输出设备。"),
            _ => ("SYSTEM AUDIO · ATTENTION", "音频设备需要检查。")
        };

        StatusEyebrow.Text = eyebrow;
        StatusHeadline.Text = headline;
        LiveDot.Fill = status switch
        {
            "live" => _acidBrush,
            "paused" or "starting" => _warningBrush,
            _ => _dangerBrush
        };

        if (status == "stopped")
        {
            SourceText.Text = "可在概览页面启动服务。";
        }
        else if (status == "live")
        {
            SourceText.Text = "正在从 " + (capture.DeviceName ?? "默认输出设备") + " 捕获系统声音";
        }
        else if (status == "paused")
        {
            SourceText.Text = "串流已暂停，接收设备会显示暂停提示。";
        }
        else if (status == "no-device")
        {
            SourceText.Text = "请连接扬声器或耳机，应用会自动重试。";
        }
        else if (status == "error")
        {
            SourceText.Text = capture.Error ?? "音频捕获失败，正在自动重试。";
        }
        else
        {
            SourceText.Text = "正在连接默认音频输出设备…";
        }

        var rate = capture.SampleRate > 0 ? capture.SampleRate : 48000;
        SampleText.Text = $"{rate / 1000.0:0.#} kHz · STEREO · 16-BIT";
        var uptime = DateTimeOffset.UtcNow - _server.State.StartedAt;
        UptimeText.Text = _serverStarted ? $"已运行 {uptime:hh\\:mm\\:ss}" : "服务未启动";
        AudioDeviceText.Text = capture.DeviceName ?? "未检测到输出设备";

        var paused = status == "paused";
        PauseText.Text = paused ? "继续串流" : "暂停串流";
        PauseIcon.Glyph = paused ? "\uE768" : "\uE769";

        if (Math.Abs(MasterVolumeSlider.Value - snapshot.MasterVolume) > 0.5 ||
            Math.Abs(QuickVolumeSlider.Value - snapshot.MasterVolume) > 0.5)
        {
            _updating = true;
            MasterVolumeSlider.Value = snapshot.MasterVolume;
            QuickVolumeSlider.Value = snapshot.MasterVolume;
            _updating = false;
        }

        MasterVolumeText.Text = snapshot.MasterVolume + "%";
        MuteIcon.Glyph = snapshot.Muted ? "\uE74F" : "\uE767";
        MuteButton.Opacity = snapshot.Muted ? 0.55 : 1;

        _updating = true;
        LatencyComboBox.SelectedIndex = snapshot.Profile switch
        {
            "low" => 0,
            "stable" => 2,
            _ => 1
        };
        _updating = false;
        LatencyText.Text = snapshot.TargetMs + " ms";

        var peak = snapshot.Peak;
        PeakText.Text = peak <= 0.00001
            ? "−∞ dB"
            : (20 * Math.Log10(peak)).ToString("0.0").Replace("-", "−") + " dB";
        _lastLevels = snapshot.Levels;
        DrawAudioMeter();
    }

    private void UpdateNetwork(bool force = false)
    {
        if (_server is null) return;
        var snapshot = _server.Network.GetSnapshot();
        UpdateStampText.Text = "硬件状态 · " + DateTime.Now.ToString("HH:mm:ss") + " 已更新";

        var signature = snapshot.Revision + "|" + snapshot.MultiSelect + "|" +
                        string.Join(",", snapshot.SelectedIds.OrderBy(id => id));
        if (!force && signature == _networkSignature) return;
        _networkSignature = signature;

        var liveIds = new HashSet<string>(snapshot.Adapters.Select(adapter => adapter.Id),
            StringComparer.OrdinalIgnoreCase);
        foreach (var existing in _adapterItems.Select(item => item.Id).ToList())
        {
            if (liveIds.Contains(existing)) continue;
            if (_adapterMap.Remove(existing, out var removed)) _adapterItems.Remove(removed);
        }

        foreach (var adapter in snapshot.Adapters)
        {
            if (!_adapterMap.TryGetValue(adapter.Id, out var viewModel))
            {
                viewModel = new NetworkAdapterViewModel();
                _adapterMap[adapter.Id] = viewModel;
                _adapterItems.Add(viewModel);
            }

            viewModel.SetAccent(_acidBrush.Color);
            viewModel.Update(adapter, snapshot.SelectedIds.Contains(adapter.Id));
        }

        _updating = true;
        MultiSelectSwitch.IsOn = snapshot.MultiSelect;
        _updating = false;
        SelectAllButton.IsEnabled = snapshot.MultiSelect;

        var selectedCount = snapshot.SelectedIds.Count;
        var activeSelected = snapshot.Adapters.Count(adapter =>
            adapter.IsUp && snapshot.SelectedIds.Contains(adapter.Id));
        var addressCount = snapshot.Addresses.Count;

        NetworkSummaryText.Text = selectedCount == 0
            ? "仅本机可访问 · 其他设备无法连接"
            : $"已选择 {selectedCount} 个网卡 · {activeSelected} 个正在广播 · {addressCount} 个可用地址";

        if (selectedCount == 0)
        {
            ShowNetworkWarning("未选择网卡。其他设备无法连接，但本机控制台仍可使用。");
        }
        else if (addressCount == 0)
        {
            ShowNetworkWarning("所选网卡当前没有可用 IPv4 地址，或网卡已断开。请检查硬件状态。");
        }
        else
        {
            NetworkWarningBar.IsOpen = false;
        }

        UpdateShare(force: true);
    }

    private void UpdateReceivers()
    {
        if (_server is null) return;
        var snapshots = _server.Hub.Receivers
            .Select(receiver => receiver.Snapshot())
            .OrderBy(receiver => receiver.ConnectedAt)
            .ToList();

        var liveIds = new HashSet<string>(snapshots.Select(receiver => receiver.Id),
            StringComparer.OrdinalIgnoreCase);
        foreach (var existing in _receiverItems.Select(item => item.Id).ToList())
        {
            if (liveIds.Contains(existing)) continue;
            if (_receiverMap.Remove(existing, out var removed)) _receiverItems.Remove(removed);
        }

        foreach (var snapshot in snapshots)
        {
            if (!_receiverMap.TryGetValue(snapshot.Id, out var viewModel))
            {
                viewModel = new ReceiverViewModel();
                _receiverMap[snapshot.Id] = viewModel;
                _receiverItems.Add(viewModel);
            }

            viewModel.SetAccent(_acidBrush.Color);
            viewModel.Update(snapshot);
        }

        ReceiverCountText.Text = snapshots.Count + " 台";
        ReceiverEmptyText.Visibility = snapshots.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
        ReceiverList.Visibility = snapshots.Count == 0 ? Visibility.Collapsed : Visibility.Visible;
    }

    private void UpdateShare(bool force = false)
    {
        if (_server is null) return;
        var code = _server.State.AccessCode;
        var addresses = _server.Network.GetAdvertisedAddresses();
        var key = code + "|" + string.Join(",", addresses.Select(address => address.Ip)) + "|" + _options.Port;
        if (!force && key == _shareSignature) return;
        _shareSignature = key;

        _shareLinks = addresses
            .Select(address => new ShareLink(
                address.Ip + "  ·  " + address.InterfaceName,
                $"http://{address.Ip}:{_options.Port}/listen?code={code}",
                address.Ip))
            .ToList();

        AccessCodeText.Text = code;
        _updating = true;
        AddressComboBox.ItemsSource = _shareLinks;
        if (_shareLinks.Count > 0)
        {
            var currentIndex = _shareLinks.FindIndex(link => link.Ip == _currentShareIp);
            AddressComboBox.SelectedIndex = currentIndex >= 0 ? currentIndex : 0;
        }
        else
        {
            AddressComboBox.SelectedIndex = -1;
        }
        _updating = false;

        AddressComboBox.IsEnabled = _shareLinks.Count > 0;
        CopyLinkButton.IsEnabled = _shareLinks.Count > 0;

        if (_shareLinks.Count == 0)
        {
            _currentShareUrl = string.Empty;
            ShareUrlText.Text = "没有可用网卡地址";
            QrPlaceholder.Visibility = Visibility.Visible;
            QrImage.Source = null;
            _qrGeneration += 1;
            _qrUrl = string.Empty;
            return;
        }

        var selected = AddressComboBox.SelectedIndex >= 0
            ? _shareLinks[AddressComboBox.SelectedIndex]
            : _shareLinks[0];
        _currentShareIp = selected.Ip;
        _currentShareUrl = selected.Url;
        ShareUrlText.Text = selected.Url;
        _ = UpdateQrAsync(selected.Url);
    }

    private string _currentShareIp = string.Empty;

    private async Task UpdateQrAsync(string url)
    {
        if (string.IsNullOrWhiteSpace(url) || url == _qrUrl) return;
        var generation = ++_qrGeneration;
        try
        {
            var image = await QrCodeService.CreateAsync(url);
            if (generation != _qrGeneration || _closed) return;
            QrImage.Source = image;
            QrPlaceholder.Visibility = image is null ? Visibility.Visible : Visibility.Collapsed;
            _qrUrl = image is null ? string.Empty : url;
        }
        catch
        {
            if (generation != _qrGeneration) return;
            QrPlaceholder.Visibility = Visibility.Visible;
        }
    }

    private void Android_Changed(object? sender, EventArgs e)
    {
        DispatcherQueue.TryEnqueue(() =>
        {
            if (!_closed) UpdateAndroidUi();
        });
    }

    private async Task RefreshAndroidAsync()
    {
        if (_server is null || _closed || _androidBusy) return;
        _androidBusy = true;
        try
        {
            await _server.Android.RefreshAsync();
        }
        catch
        {
            // The service records its own error state.
        }
        finally
        {
            _androidBusy = false;
        }

        if (!_closed) UpdateAndroidUi();
    }

    private void UpdateAndroidUi()
    {
        if (_closed) return;
        var server = _server;
        if (server is null)
        {
            AndroidWiredPillText.Text = "服务未启动";
            return;
        }

        var snapshot = server.Android.GetSnapshot();
        // adb is shipped with the application; keep manual path selection as a
        // hidden fallback for development builds only.
        AndroidPickAdbButton.Visibility = Visibility.Collapsed;
        var code = server.State.AccessCode;
        var tether = snapshot.UsbTetherAdapter;
        var reverse = snapshot.ReverseActive;
        _androidReverseSerial = reverse ? snapshot.ReverseSerial ?? string.Empty : string.Empty;
        var tetherSelected = tether is not null && server.Network.IsSelected(tether.Id);
        var tetherReady = tether is { IsUp: true } && tether.Ipv4Addresses.Count > 0;

        if (reverse)
        {
            AndroidWiredDot.Fill = _acidBrush;
            AndroidWiredPillText.Text = "USB 有线 · 已连接";
        }
        else if (tetherReady)
        {
            AndroidWiredDot.Fill = tetherSelected ? _acidBrush : _warningBrush;
            AndroidWiredPillText.Text = tetherSelected ? "USB 网络共享 · 已启用" : "USB 网络共享 · 待启用";
        }
        else if (snapshot.AdbAvailable)
        {
            AndroidWiredDot.Fill = snapshot.ReadyDevice is null ? _warningBrush : _acidBrush;
            AndroidWiredPillText.Text = snapshot.ReadyDevice is null ? "等待设备授权" : "已发现 Android 设备";
        }
        else
        {
            AndroidWiredDot.Fill = _mutedBrush;
            AndroidWiredPillText.Text = "未检测到设备";
        }

        if (tether is null)
        {
            AndroidTetherStatusText.Text = "未检测到 USB 网络共享网卡。在手机上开启“USB 网络共享”后会出现。";
            UseTetherButton.IsEnabled = false;
            UseTetherButtonText.Text = "使用 USB 网卡";
        }
        else
        {
            var ipText = tether.Ipv4Addresses.Count > 0
                ? string.Join(" · ", tether.Ipv4Addresses.Select(address => address.ToString()))
                : "等待 IPv4";
            AndroidTetherStatusText.Text = $"{tether.Name} · {ipText}" +
                                           (tetherSelected ? " · 已选中" : string.Empty);
            UseTetherButton.IsEnabled = tetherReady && !tetherSelected;
            UseTetherButtonText.Text = tetherSelected
                ? (tetherReady ? "已在广播" : "已选中，等待地址")
                : "使用 USB 网卡";
        }

        _updatingAndroid = true;
        try
        {
            var adbPath = snapshot.AdbPath;
            if (!snapshot.AdbAvailable)
            {
                AndroidAdbStatusText.Text = "未找到内置 adb。请检查发布目录是否完整，或使用左侧 USB 网络共享。";
                SetAndroidDevices(Array.Empty<AndroidDeviceItem>(), null);
                AndroidDeviceComboBox.IsEnabled = false;
                AndroidReverseButton.IsEnabled = false;
                AndroidStopReverseButton.IsEnabled = false;
                AndroidOpenPhoneButton.IsEnabled = false;
            }
            else
            {
                var errorSuffix = string.IsNullOrWhiteSpace(snapshot.Error)
                    ? string.Empty
                    : " · " + snapshot.Error;
                AndroidAdbStatusText.Text = "adb：" + ShortenPath(adbPath) + errorSuffix;

                var items = snapshot.Devices
                    .Select(device => new AndroidDeviceItem(device.Serial, device.Display, device.IsReady))
                    .ToList();
                var wanted = items.Any(item => item.Serial == _androidSelectedSerial)
                    ? _androidSelectedSerial
                    : items.FirstOrDefault(item => item.Ready)?.Serial
                      ?? items.FirstOrDefault()?.Serial
                      ?? string.Empty;
                SetAndroidDevices(items, wanted);
                AndroidDeviceComboBox.IsEnabled = !reverse;

                AndroidReverseButton.IsEnabled = !reverse && snapshot.ReadyDevice is not null;
                AndroidStopReverseButton.IsEnabled = reverse;
                AndroidOpenPhoneButton.IsEnabled = reverse;
            }
        }
        finally
        {
            _updatingAndroid = false;
        }

        AndroidReverseButtonText.Text = reverse ? "通道已建立" : "建立有线通道";
        if (reverse)
        {
            AndroidAdbStatusText.Text = "USB 有线通道已建立" +
                                        (string.IsNullOrWhiteSpace(snapshot.ReverseSerial)
                                            ? string.Empty
                                            : " · " + snapshot.ReverseSerial) +
                                        (string.IsNullOrWhiteSpace(snapshot.Error)
                                            ? string.Empty
                                            : " · " + snapshot.Error);
        }

        string? url = null;
        var hint = string.Empty;
        if (reverse)
        {
            url = server.Android.BuildPhoneUrl(code);
            hint = "已建立 ADB 通道；在手机浏览器打开此地址（localhost 为安全上下文）。";
        }
        else if (tetherReady && tetherSelected)
        {
            url = server.Android.BuildTetherUrl(code);
            hint = "手机与电脑位于 USB 网段，打开此地址即可。";
        }
        else if (tetherReady)
        {
            hint = "先点“使用 USB 网卡”，再复制或扫描地址。";
        }
        else
        {
            hint = "建立任一有线通道后显示收听地址。";
        }

        _androidActiveUrl = url ?? string.Empty;
        AndroidWiredUrlText.Text = string.IsNullOrWhiteSpace(url) ? "建立通道后显示" : url;
        AndroidWiredHintText.Text = hint;
        CopyAndroidUrlButton.IsEnabled = !string.IsNullOrWhiteSpace(url);
        AndroidQrPlaceholder.Visibility = string.IsNullOrWhiteSpace(url)
            ? Visibility.Visible
            : Visibility.Collapsed;
        _ = UpdateAndroidQrAsync(_androidActiveUrl);
    }

    private void SetAndroidDevices(IReadOnlyList<AndroidDeviceItem> items, string? wantedSerial)
    {
        var current = AndroidDeviceComboBox.ItemsSource as IReadOnlyList<AndroidDeviceItem>;
        var same = current is not null
                   && current.Count == items.Count
                   && current.Zip(items, (left, right) =>
                       string.Equals(left.Serial, right.Serial, StringComparison.Ordinal)
                       && string.Equals(left.Display, right.Display, StringComparison.Ordinal)).All(equal => equal);

        if (!same)
        {
            AndroidDeviceComboBox.ItemsSource = items;
        }

        if (items.Count == 0)
        {
            AndroidDeviceComboBox.SelectedIndex = -1;
            _androidSelectedSerial = string.Empty;
            return;
        }

        var index = 0;
        for (var i = 0; i < items.Count; i += 1)
        {
            if (!string.Equals(items[i].Serial, wantedSerial, StringComparison.Ordinal)) continue;
            index = i;
            break;
        }

        if (AndroidDeviceComboBox.SelectedIndex != index)
        {
            AndroidDeviceComboBox.SelectedIndex = index;
        }

        _androidSelectedSerial = items[index].Serial;
    }

    private async Task UpdateAndroidQrAsync(string url)
    {
        if (string.IsNullOrWhiteSpace(url))
        {
            _androidQrGeneration += 1;
            _androidQrUrl = string.Empty;
            AndroidQrImage.Source = null;
            AndroidQrPlaceholder.Visibility = Visibility.Visible;
            return;
        }

        if (string.Equals(url, _androidQrUrl, StringComparison.Ordinal)) return;

        var generation = ++_androidQrGeneration;
        try
        {
            var image = await QrCodeService.CreateAsync(url);
            if (generation != _androidQrGeneration || _closed) return;
            AndroidQrImage.Source = image;
            AndroidQrPlaceholder.Visibility = image is null ? Visibility.Visible : Visibility.Collapsed;
            _androidQrUrl = image is null ? string.Empty : url;
        }
        catch
        {
            if (generation != _androidQrGeneration) return;
            AndroidQrPlaceholder.Visibility = Visibility.Visible;
        }
    }

    internal async void UseAndroidTether_Click(object sender, RoutedEventArgs e)
    {
        if (_server is null || _androidActionBusy) return;
        var tether = _server.Android.GetSnapshot().UsbTetherAdapter;
        if (tether is null)
        {
            ShowAndroidInfo(InfoBarSeverity.Warning, "没有检测到 USB 网卡",
                "请先在手机上开启“USB 网络共享”，等待电脑出现 USB 网卡后再试。");
            return;
        }

        if (tether.Ipv4Addresses.Count == 0)
        {
            ShowAndroidInfo(InfoBarSeverity.Warning, "USB 网卡还没有地址",
                "Windows 正在向手机获取 IPv4 地址，请稍等几秒后刷新。");
            return;
        }

        _androidActionBusy = true;
        try
        {
            _server.Network.SetSelected(tether.Id, true);
            UpdateNetwork(force: true);
            UpdateShare(force: true);
            UpdateAndroidUi();
            _lastAndroidRefresh = DateTimeOffset.MinValue;
            await RefreshAndroidAsync();
            if (!_closed)
            {
                ShowAndroidInfo(InfoBarSeverity.Success, "USB 网卡已启用",
                    "用手机扫描右侧二维码，或打开下方的有线收听地址。");
            }
        }
        catch (Exception ex)
        {
            if (!_closed) ShowAndroidInfo(InfoBarSeverity.Error, "无法启用 USB 网卡", ex.Message);
        }
        finally
        {
            _androidActionBusy = false;
        }
    }

    internal async void RefreshAndroid_Click(object sender, RoutedEventArgs e)
    {
        if (_closed) return;
        try
        {
            _lastAndroidRefresh = DateTimeOffset.MinValue;
            await RefreshAndroidAsync();
        }
        catch (Exception ex)
        {
            if (!_closed) ShowAndroidInfo(InfoBarSeverity.Error, "刷新 Android 设备失败", ex.Message);
        }
    }

    internal async void AndroidReverse_Click(object sender, RoutedEventArgs e)
    {
        if (_server is null || _androidActionBusy) return;
        _androidActionBusy = true;
        AndroidReverseButton.IsEnabled = false;
        AndroidReverseButtonText.Text = "正在建立…";
        try
        {
            var (ok, message) = await _server.Android.EnableReverseAsync(_androidSelectedSerial);
            if (!_closed)
            {
                ShowAndroidInfo(ok ? InfoBarSeverity.Success : InfoBarSeverity.Error,
                    ok ? "USB 有线通道已建立" : "无法建立 USB 通道", message);
            }
            _lastAndroidRefresh = DateTimeOffset.MinValue;
            await RefreshAndroidAsync();
        }
        catch (Exception ex)
        {
            if (!_closed) ShowAndroidInfo(InfoBarSeverity.Error, "无法建立 USB 通道", ex.Message);
        }
        finally
        {
            _androidActionBusy = false;
            if (!_closed)
            {
                AndroidReverseButtonText.Text = "建立有线通道";
                UpdateAndroidUi();
            }
        }
    }

    internal async void AndroidStopReverse_Click(object sender, RoutedEventArgs e)
    {
        if (_server is null || _androidActionBusy) return;
        _androidActionBusy = true;
        try
        {
            var (ok, message) = await _server.Android.DisableReverseAsync();
            if (!_closed)
            {
                ShowAndroidInfo(ok ? InfoBarSeverity.Informational : InfoBarSeverity.Warning,
                    ok ? "USB 有线通道已关闭" : "关闭通道时出现问题", message);
            }
            _lastAndroidRefresh = DateTimeOffset.MinValue;
            await RefreshAndroidAsync();
        }
        catch (Exception ex)
        {
            if (!_closed) ShowAndroidInfo(InfoBarSeverity.Error, "关闭 USB 通道失败", ex.Message);
        }
        finally
        {
            _androidActionBusy = false;
            if (!_closed) UpdateAndroidUi();
        }
    }

    internal async void AndroidPickAdb_Click(object sender, RoutedEventArgs e)
    {
        if (_server is null) return;
        try
        {
            var picker = new FileOpenPicker
            {
                SuggestedStartLocation = PickerLocationId.ComputerFolder
            };
            picker.FileTypeFilter.Add(".exe");
            var handle = WinRT.Interop.WindowNative.GetWindowHandle(this);
            WinRT.Interop.InitializeWithWindow.Initialize(picker, handle);
            var file = await picker.PickSingleFileAsync();
            if (_closed || file is null) return;

            if (!_server.Android.SetAdbPath(file.Path))
            {
                ShowAndroidInfo(InfoBarSeverity.Error, "无法使用这个 adb",
                    "请选择 Android platform-tools 目录中的 adb.exe。");
                return;
            }

            ShowAndroidInfo(InfoBarSeverity.Success, "已选择 adb", file.Path);
            _lastAndroidRefresh = DateTimeOffset.MinValue;
            await RefreshAndroidAsync();
        }
        catch (Exception ex)
        {
            if (!_closed) ShowAndroidInfo(InfoBarSeverity.Error, "无法打开文件选择器", ex.Message);
        }
    }

    internal async void AndroidOpenPhone_Click(object sender, RoutedEventArgs e)
    {
        if (_server is null || _androidActionBusy || string.IsNullOrWhiteSpace(_androidActiveUrl)) return;
        _androidActionBusy = true;
        try
        {
            var serial = string.IsNullOrWhiteSpace(_androidReverseSerial)
                ? _androidSelectedSerial
                : _androidReverseSerial;
            var (ok, message) = await _server.Android.OpenOnPhoneAsync(_androidActiveUrl, serial);
            if (!_closed)
            {
                ShowAndroidInfo(ok ? InfoBarSeverity.Success : InfoBarSeverity.Error,
                    ok ? "已发送到手机" : "无法打开手机浏览器", message);
            }
        }
        catch (Exception ex)
        {
            if (!_closed) ShowAndroidInfo(InfoBarSeverity.Error, "无法打开手机浏览器", ex.Message);
        }
        finally
        {
            _androidActionBusy = false;
        }
    }

    internal void CopyAndroidUrl_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(_androidActiveUrl))
        {
            ShowAndroidInfo(InfoBarSeverity.Warning, "还没有有线地址",
                "请先建立 ADB 通道，或启用 USB 网络共享网卡。");
            return;
        }

        CopyText(_androidActiveUrl, "有线收听链接已复制");
    }

    internal void AndroidDevice_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_updatingAndroid) return;
        if (AndroidDeviceComboBox.SelectedItem is AndroidDeviceItem item)
        {
            _androidSelectedSerial = item.Serial;
        }
    }

    private void ShowAndroidInfo(InfoBarSeverity severity, string title, string message)
    {
        AndroidWiredInfoBar.Severity = severity;
        AndroidWiredInfoBar.Title = title;
        AndroidWiredInfoBar.Message = message;
        AndroidWiredInfoBar.IsOpen = true;
    }

    private static string ShortenPath(string? path)
    {
        if (string.IsNullOrWhiteSpace(path)) return "—";
        var local = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        if (!string.IsNullOrEmpty(local)
            && path.StartsWith(local, StringComparison.OrdinalIgnoreCase))
        {
            return "%LOCALAPPDATA%" + path[local.Length..];
        }

        return path;
    }

    private void Network_Changed(object? sender, EventArgs e)
    {
        DispatcherQueue.TryEnqueue(() =>
        {
            if (_closed) return;
            UpdateNetwork(force: true);
            UpdateShare(force: true);
        });
    }

    private void SetServicePill(bool online, string text)
    {
        ServicePillText.Text = text;
        ServiceDot.Fill = online ? _acidBrush : _warningBrush;
        ServicePill.Background = online
            ? _accentPillBrush
            : new SolidColorBrush(Color.FromArgb(20, 255, 193, 102));
    }

    private void ShowNotice(InfoBarSeverity severity, string title, string message)
    {
        ErrorBar.Severity = severity;
        ErrorBar.Title = title;
        ErrorBar.Message = message;
        ErrorBar.IsOpen = true;
        if (severity == InfoBarSeverity.Success || severity == InfoBarSeverity.Informational)
        {
            _noticeTimer?.Start();
        }
    }

    private void ShowNetworkWarning(string message)
    {
        NetworkWarningBar.Message = message;
        NetworkWarningBar.IsOpen = true;
    }


    internal async void Pause_Click(object sender, RoutedEventArgs e)
    {
        if (_closed || _server is null) return;
        if (!_serverStarted)
        {
            ShowNotice(InfoBarSeverity.Informational, "服务尚未启动", "启动音频服务后才能暂停串流。");
            return;
        }

        try
        {
            _server.Capture.SetPaused(!_server.Capture.IsPaused);
            await Task.Delay(250, _lifetimeCts.Token);
            if (!_closed) RefreshEverything();
        }
        catch (OperationCanceledException) when (_closed || _lifetimeCts.IsCancellationRequested)
        {
            // Window shutdown cancels pending UI work.
        }
        catch (Exception ex)
        {
            if (!_closed) ShowNotice(InfoBarSeverity.Error, "无法切换串流状态", ex.Message);
        }
    }

    internal void Mute_Click(object sender, RoutedEventArgs e)
    {
        if (_closed || _server is null) return;
        if (!_serverStarted)
        {
            ShowNotice(InfoBarSeverity.Informational, "服务尚未启动", "启动音频服务后才能修改静音状态。");
            return;
        }

        try
        {
            _server.Hub.SetMuted(!_server.State.Muted);
            RefreshEverything();
        }
        catch (Exception ex)
        {
            if (!_closed) ShowNotice(InfoBarSeverity.Error, "无法切换静音状态", ex.Message);
        }
    }

    internal void MasterVolume_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
    {
        if (_updating || _server is null) return;
        var volume = (int)Math.Round(e.NewValue);
        _server.Hub.SetMasterVolume(volume);
        _updating = true;
        QuickVolumeSlider.Value = volume;
        _updating = false;
        MasterVolumeText.Text = volume + "%";
    }

    internal void QuickVolume_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
    {
        if (_updating || _server is null) return;
        var volume = (int)Math.Round(e.NewValue);
        _server.Hub.SetMasterVolume(volume);
        _updating = true;
        MasterVolumeSlider.Value = volume;
        _updating = false;
        MasterVolumeText.Text = volume + "%";
    }

    internal void Latency_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_updating || _server is null) return;
        var profile = (LatencyComboBox.SelectedItem as ComboBoxItem)?.Tag as string ?? "balanced";
        _server.Hub.SetProfile(profile);
        LatencyText.Text = _server.State.TargetMs + " ms";
    }

    internal async void AdapterCheckBox_Click(object sender, RoutedEventArgs e)
    {
        if (_closed || _server is null) return;
        if (sender is not CheckBox checkBox || checkBox.Tag is not string id) return;
        try
        {
            _server.Network.SetSelected(id, checkBox.IsChecked == true);
            await Task.Delay(30, _lifetimeCts.Token);
            if (_closed) return;
            UpdateNetwork(force: true);
            UpdateShare(force: true);
        }
        catch (OperationCanceledException) when (_closed || _lifetimeCts.IsCancellationRequested)
        {
            // Window shutdown cancels pending UI work.
        }
        catch (Exception ex)
        {
            if (!_closed) ShowNotice(InfoBarSeverity.Error, "无法更新网卡选择", ex.Message);
        }
    }

    internal void MultiSelect_Toggled(object sender, RoutedEventArgs e)
    {
        if (_updating || _server is null) return;
        _server.Network.SetMultiSelect(MultiSelectSwitch.IsOn);
        UpdateNetwork(force: true);
        UpdateShare(force: true);
    }

    internal void SelectAll_Click(object sender, RoutedEventArgs e)
    {
        if (_server is null) return;
        _server.Network.SelectAllActive();
        UpdateNetwork(force: true);
        UpdateShare(force: true);
    }

    internal void ClearNetwork_Click(object sender, RoutedEventArgs e)
    {
        if (_server is null) return;
        _server.Network.ClearSelection();
        UpdateNetwork(force: true);
        UpdateShare(force: true);
    }

    internal void RefreshNetwork_Click(object sender, RoutedEventArgs e)
    {
        if (_server is null) return;
        _server.Network.Refresh();
        UpdateNetwork(force: true);
        UpdateShare(force: true);
    }

    internal void Address_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_updating) return;
        if (AddressComboBox.SelectedItem is not ShareLink link) return;
        _currentShareIp = link.Ip;
        _currentShareUrl = link.Url;
        ShareUrlText.Text = link.Url;
        _ = UpdateQrAsync(link.Url);
    }

    internal void CopyLink_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(_currentShareUrl))
        {
            ShowNotice(InfoBarSeverity.Warning, "没有可用链接", "请先选择一个有 IPv4 地址的网卡。");
            return;
        }

        CopyText(_currentShareUrl, "收听链接已复制");
    }

    internal void CopyCode_Click(object sender, RoutedEventArgs e)
    {
        if (_server is null) return;
        CopyText(_server.State.AccessCode, "访问码已复制");
    }

    internal void RotateCode_Click(object sender, RoutedEventArgs e)
    {
        if (_server is null) return;
        _server.Hub.RotateCode();
        UpdateShare(force: true);
        UpdateAndroidUi();
        ShowNotice(InfoBarSeverity.Success, "访问码已更换", "已经连接的设备不会断开，新设备需要使用新访问码。");
    }

    private void CopyText(string text, string notice)
    {
        try
        {
            var package = new DataPackage();
            package.SetText(text);
            Clipboard.SetContent(package);
            Clipboard.Flush();
            ShowNotice(InfoBarSeverity.Success, "已复制", notice);
        }
        catch
        {
            ShowNotice(InfoBarSeverity.Error, "复制失败", "系统剪贴板当前不可用。");
        }
    }

    private void OpenBrowser_Click(object sender, RoutedEventArgs e)
    {
        if (_server is null || !_serverStarted)
        {
            ShowNotice(InfoBarSeverity.Warning, "服务尚未启动", "请等待服务启动完成后再打开网页控制台。");
            return;
        }

        OpenExternal(_server.LocalStudioUrl);
    }

    internal void OpenLogFolder_Click(object sender, RoutedEventArgs e)
    {
        AppPaths.EnsureDirectories();
        OpenFolder(AppPaths.LogsDirectory);
    }

    internal void OpenSettingsFolder_Click(object sender, RoutedEventArgs e)
    {
        AppPaths.EnsureDirectories();
        OpenFolder(AppPaths.Root);
    }

    private void OpenExternal(string target)
    {
        try
        {
            Process.Start(new ProcessStartInfo(target) { UseShellExecute = true });
        }
        catch (Exception ex)
        {
            ShowNotice(InfoBarSeverity.Error, "无法打开", ex.Message);
        }
    }

    private void OpenFolder(string path)
    {
        try
        {
            Process.Start(new ProcessStartInfo("explorer.exe", $"\"{path}\"") { UseShellExecute = true });
        }
        catch (Exception ex)
        {
            ShowNotice(InfoBarSeverity.Error, "无法打开文件夹", ex.Message);
        }
    }

    internal void IdentifyReceiver_Click(object sender, RoutedEventArgs e)
    {
        if (_server is null) return;
        if (sender is not Button button || button.Tag is not string id) return;
        _server.Hub.Identify(id);
    }

    internal async void KickReceiver_Click(object sender, RoutedEventArgs e)
    {
        if (_server is null) return;
        if (sender is not Button button || button.Tag is not string id) return;

        var dialog = new ContentDialog
        {
            Title = "断开这台设备？",
            Content = "该设备将立即停止播放，但可以重新扫码连接。",
            PrimaryButtonText = "断开",
            CloseButtonText = "取消",
            DefaultButton = ContentDialogButton.Close,
            XamlRoot = RootGrid.XamlRoot
        };

        try
        {
            if (await dialog.ShowAsync() == ContentDialogResult.Primary && !_closed)
            {
                await _server.Hub.KickAsync(id);
            }
        }
        catch (Exception ex)
        {
            if (!_closed) ShowNotice(InfoBarSeverity.Error, "无法断开设备", ex.Message);
        }
    }

    private async void Window_Closed(object sender, WindowEventArgs args)
    {
        if (_closed) return;
        _closed = true;
        _appearanceSettings.ColorValuesChanged -= AppearanceSettings_ColorValuesChanged;
        RootGrid.ActualThemeChanged -= AppearanceThemeChanged;
        _lifetimeCts.Cancel();
        _refreshTimer.Stop();
        _noticeTimer?.Stop();
        _serverStarted = false;
        if (_server is not null)
        {
            _server.Network.Changed -= Network_Changed;
            _server.Android.Changed -= Android_Changed;
            try
            {
                await _server.DisposeAsync();
            }
            catch (Exception ex)
            {
                StartupTrace.Write("Server shutdown failed: " + ex);
            }
        }
    }

    private sealed record ShareLink(string Display, string Url, string Ip);

    private sealed record AndroidDeviceItem(string Serial, string Display, bool Ready);
}
