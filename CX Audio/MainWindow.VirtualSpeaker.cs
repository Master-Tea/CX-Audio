using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Automation;
using Microsoft.UI.Xaml.Controls;

namespace CXAudioOutput;

public sealed partial class MainWindow
{
    private readonly StackPanel _virtualSpeakerContent = new() { Spacing = 16 };
    private readonly TextBlock _virtualModeText = new() { FontSize = 20, TextWrapping = TextWrapping.Wrap };
    private readonly TextBlock _virtualDeviceText = new() { TextWrapping = TextWrapping.Wrap };
    private readonly TextBlock _virtualProgressText = new() { TextWrapping = TextWrapping.Wrap };
    private readonly InfoBar _virtualInfoBar = new() { IsClosable = false };
    private readonly Button _virtualEnableButton = new() { Content = "启用高级模式", HorizontalAlignment = HorizontalAlignment.Left };
    private readonly Button _virtualNormalButton = new() { Content = "恢复普通模式", HorizontalAlignment = HorizontalAlignment.Left };
    private readonly Button _virtualInstallButton = new() { Content = "安装虚拟扬声器驱动（需要管理员权限）", HorizontalAlignment = HorizontalAlignment.Left };
    private readonly Button _virtualRefreshButton = new() { Content = "刷新设备", HorizontalAlignment = HorizontalAlignment.Left };
    private bool _virtualActionBusy;
    private bool _virtualInstallBusy;

    internal FrameworkElement VirtualSpeakerContent => _virtualSpeakerContent;

    private void InitializeVirtualSpeaker()
    {
        _virtualSpeakerContent.Children.Add(_virtualModeText);
        _virtualSpeakerContent.Children.Add(new TextBlock
        {
            Text = "选择声音从哪里播放。网络接收设备可继续使用无线连接或 Android 有线连接。",
            TextWrapping = TextWrapping.Wrap
        });
        var modes = new StackPanel { Spacing = 12 };
        modes.Children.Add(ModeDescription("普通模式", "电脑扬声器 / 耳机与网络扬声器可同时发声。系统音频从 Windows 默认输出设备采集。"));
        modes.Children.Add(ModeDescription("虚拟扬声器（高级）", "系统声音送入 VB-CABLE 虚拟扬声器，并禁用其余 Windows 播放端点。仅网络接收设备播放；暂停串流或接收设备断开时，电脑仍保持无声。"));
        _virtualSpeakerContent.Children.Add(modes);
        _virtualSpeakerContent.Children.Add(_virtualDeviceText);
        _virtualSpeakerContent.Children.Add(_virtualInfoBar);

        _virtualEnableButton.Click += async (_, _) => await ChangeVirtualModeAsync(true);
        _virtualNormalButton.Click += async (_, _) => await ChangeVirtualModeAsync(false);
        _virtualInstallButton.Click += InstallVirtualDriver_Click;
        _virtualRefreshButton.Click += async (_, _) =>
        {
            if (_server is null || !_serverStarted) return;
            try { await _server.VirtualSpeaker.RefreshAsync(); }
            catch (Exception ex) { _virtualProgressText.Text = ex.Message; }
            UpdateVirtualSpeaker();
        };
        AutomationProperties.SetName(_virtualEnableButton, "启用虚拟扬声器高级模式");
        AutomationProperties.SetName(_virtualNormalButton, "恢复普通模式和电脑声音");
        _virtualSpeakerContent.Children.Add(_virtualEnableButton);
        _virtualSpeakerContent.Children.Add(_virtualNormalButton);
        _virtualSpeakerContent.Children.Add(new TextBlock
        {
            Text = "首次使用：安装驱动 → 按安装器提示重启电脑 → 启用高级模式。驱动安装由 VB-Audio 官方安装器在 Windows 中注册真实音频设备。",
            TextWrapping = TextWrapping.Wrap, Margin = new Thickness(0, 8, 0, 0)
        });
        _virtualSpeakerContent.Children.Add(_virtualInstallButton);
        _virtualSpeakerContent.Children.Add(_virtualRefreshButton);
        _virtualSpeakerContent.Children.Add(_virtualProgressText);
        var website = new HyperlinkButton { Content = "VB-CABLE 官网与许可说明", NavigateUri = new Uri(VirtualSpeakerInstaller.Website) };
        _virtualSpeakerContent.Children.Add(website);
        _virtualSpeakerContent.Children.Add(new TextBlock
        {
            Text = "VB-CABLE 是 VB-Audio 的第三方 Donationware 驱动，按其许可使用。安装时从官网下载并验证数字签名。切回普通模式或退出应用会恢复此前启用的设备及默认输出；异常退出由守护进程恢复，未完成的恢复会在下次启动时重试。",
            TextWrapping = TextWrapping.Wrap,
            Style = (Style)Application.Current.Resources["CaptionStyle"]
        });
        _virtualSpeakerContent.Children.Add(new TextBlock
        {
            Text = "高级模式会持续检查新接入或被重新启用的播放设备并再次禁用。设备热插拔到检查生效之间可能存在短暂窗口；如需严格的硬件级零发声保证，请同时断开物理扬声器。",
            TextWrapping = TextWrapping.Wrap,
            Style = (Style)Application.Current.Resources["CaptionStyle"]
        });
        UpdateVirtualSpeaker();
    }

    private static Border ModeDescription(string title, string description)
    {
        var body = new StackPanel { Spacing = 6 };
        body.Children.Add(new TextBlock { Text = title, FontWeight = Microsoft.UI.Text.FontWeights.SemiBold });
        body.Children.Add(new TextBlock { Text = description, TextWrapping = TextWrapping.Wrap });
        return new Border { Style = (Style)Application.Current.Resources["AppCardStyle"], Child = body };
    }

    private void UpdateVirtualSpeaker()
    {
        var state = _server?.VirtualSpeaker.Snapshot;
        var busy = _virtualActionBusy || _virtualInstallBusy || state?.Busy == true;
        var ready = _serverStarted && _server is not null;
        _virtualModeText.Text = state?.Advanced == true
            ? state.Protected ? "当前模式：虚拟扬声器 · 输出隔离已验证" : "当前模式：虚拟扬声器 · 需要检查"
            : state?.RecoveryPending == true ? "当前模式：设备恢复待完成" : "当前模式：普通模式";
        _virtualDeviceText.Text = !ready ? "启动音频服务后可检查和切换音频模式。"
            : state?.Installed == true ? "虚拟播放设备：" + state.DeviceName
            : "尚未检测到可用的 VB-CABLE 播放设备。请安装驱动，或在 Windows 声音设置中启用设备后刷新。";
        _virtualEnableButton.IsEnabled = ready && !busy && !_options.Demo && state is { Installed: true, Advanced: false, RecoveryPending: false, Error: null };
        _virtualNormalButton.IsEnabled = ready && !busy && state is not null && (state.Advanced || state.RecoveryPending);
        _virtualInstallButton.IsEnabled = ready && !busy && !_options.Demo && state is { Advanced: false, RecoveryPending: false };
        _virtualRefreshButton.IsEnabled = ready && !busy;
        _virtualInfoBar.IsOpen = _options.Demo || !string.IsNullOrEmpty(state?.Error) || state?.Advanced == true;
        _virtualInfoBar.Severity = !string.IsNullOrEmpty(state?.Error) ? InfoBarSeverity.Error : InfoBarSeverity.Informational;
        _virtualInfoBar.Title = _options.Demo ? "演示模式" : state?.Error is not null ? "音频路由需要检查" : "电脑本地输出已禁用";
        _virtualInfoBar.Message = _options.Demo ? "请正常启动应用后使用高级模式。"
            : state?.Error ?? "电脑声音仅送入虚拟端点。要恢复本地声音，请点击恢复普通模式。";
    }

    private async Task ChangeVirtualModeAsync(bool advanced)
    {
        if (_server is null || !_serverStarted || _virtualActionBusy || _virtualInstallBusy) return;
        _virtualActionBusy = true;
        _virtualProgressText.Text = advanced ? "正在准备虚拟扬声器并隔离本地输出…" : "正在恢复原来的播放设备与默认输出…";
        UpdateVirtualSpeaker();
        try
        {
            if (advanced) await _server.VirtualSpeaker.EnableAsync();
            else await _server.VirtualSpeaker.DisableAsync();
            if (!_closed) _virtualProgressText.Text = advanced ? "高级模式已启用。请在接收设备上开始聆听。" : "普通模式已恢复。电脑与网络扬声器可同时播放。";
        }
        catch (Exception ex)
        {
            if (!_closed) _virtualProgressText.Text = ex.Message;
        }
        finally
        {
            _virtualActionBusy = false;
            if (!_closed) { UpdateVirtualSpeaker(); RefreshEverything(); }
        }
    }

    private async void InstallVirtualDriver_Click(object sender, RoutedEventArgs e)
    {
        if (_server is null || _virtualInstallBusy || _virtualActionBusy) return;
        _virtualInstallBusy = true;
        UpdateVirtualSpeaker();
        try
        {
            var progress = new Progress<string>(message => { if (!_closed) _virtualProgressText.Text = message; });
            await VirtualSpeakerInstaller.InstallAsync(progress, _lifetimeCts.Token);
            if (!_closed) await _server.VirtualSpeaker.RefreshAsync();
        }
        catch (OperationCanceledException) when (_closed) { }
        catch (Exception ex) { if (!_closed) _virtualProgressText.Text = "安装未完成：" + ex.Message; }
        finally
        {
            _virtualInstallBusy = false;
            if (!_closed) UpdateVirtualSpeaker();
        }
    }
}
