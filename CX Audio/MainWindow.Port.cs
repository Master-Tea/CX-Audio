using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Automation;
using Microsoft.UI.Xaml.Controls;
using Microsoft.Windows.AppLifecycle;
using Windows.ApplicationModel.Core;
using Windows.Globalization.NumberFormatting;

namespace CXAudioOutput;

public sealed partial class MainWindow
{
    private NumberBox? _portNumberBox;
    private Button? _portSaveButton;
    private Button? _portRestartButton;
    private TextBlock? _portStatus;
    private int _savedPort;
    private bool _restartingForPort;

    private void InitializePortSettings(AppSettings settings)
    {
        _savedPort = settings.Port;
        var group = new StackPanel { Spacing = 8 };
        group.Children.Add(new TextBlock { Text = "音频服务端口", FontWeight = Microsoft.UI.Text.FontWeights.SemiBold });
        var row = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 8 };
        _portNumberBox = new NumberBox
        {
            Value = settings.Port,
            Minimum = 1, Maximum = 65535, SmallChange = 1, LargeChange = 100,
            NumberFormatter = new DecimalFormatter { FractionDigits = 0, IsGrouped = false },
            SpinButtonPlacementMode = NumberBoxSpinButtonPlacementMode.Compact,
            Width = 148
        };
        AutomationProperties.SetName(_portNumberBox, "音频服务端口，范围 1 到 65535");
        _portSaveButton = new Button { Content = "保存端口", VerticalAlignment = VerticalAlignment.Center };
        _portRestartButton = new Button { Content = "重启应用以生效", VerticalAlignment = VerticalAlignment.Center };
        _portSaveButton.Click += (_, _) => SavePortSetting();
        _portRestartButton.Click += async (_, _) => await RestartForPortAsync();
        _portNumberBox.ValueChanged += (_, _) => RefreshPortControls();
        row.Children.Add(_portNumberBox);
        row.Children.Add(_portSaveButton);
        row.Children.Add(_portRestartButton);
        group.Children.Add(row);
        _portStatus = new TextBlock
        {
            Style = (Style)Application.Current.Resources["CaptionStyle"],
            TextWrapping = TextWrapping.Wrap
        };
        group.Children.Add(_portStatus);
        group.Children.Add(new TextBlock
        {
            Text = "可用范围 1–65535。保存后重启应用生效，接收设备需使用新的链接重新连接。",
            Style = (Style)Application.Current.Resources["CaptionStyle"],
            TextWrapping = TextWrapping.Wrap
        });
        _settingsContent.Children.Add(group);
        RefreshPortControls();
    }

    private bool TryGetPortInput(out int port)
    {
        var value = _portNumberBox?.Value ?? double.NaN;
        if (double.IsFinite(value) && value is >= 1 and <= 65535 && value == Math.Truncate(value))
        {
            port = (int)value;
            return true;
        }
        port = 0;
        return false;
    }

    private void RefreshPortControls()
    {
        if (_portSaveButton is null || _portRestartButton is null || _portStatus is null) return;
        var valid = TryGetPortInput(out var selected);
        _portSaveButton.IsEnabled = valid && selected != _savedPort && !_restartingForPort;
        _portRestartButton.IsEnabled = valid && selected == _savedPort && _savedPort != _options.Port && !_restartingForPort;
        _portStatus.Text = !valid ? "请输入 1 到 65535 之间的整数端口。"
            : selected != _savedPort ? $"当前端口：{_options.Port}；新端口尚未保存。"
            : _savedPort != _options.Port ? $"当前端口：{_options.Port}；已保存端口：{_savedPort}（重启后生效）。"
            : $"当前端口：{_options.Port}";
    }

    private bool SavePortSetting()
    {
        if (!TryGetPortInput(out var port) || _server is null) return false;
        if (!_server.Settings.Update(settings => settings.Port = port))
        {
            ShowNotice(InfoBarSeverity.Warning, "端口未保存", "无法写入设置文件，请检查目录写入权限后重试。");
            return false;
        }
        _savedPort = port;
        RefreshPortControls();
        return true;
    }

    private async Task RestartForPortAsync()
    {
        if (_restartingForPort || _savedPort == _options.Port || !TryGetPortInput(out var selected) || selected != _savedPort) return;
        // The restart command must override any previous environment/CLI port.
        var arguments = $"--port={_savedPort}" + (_options.Demo ? " --demo" : string.Empty);
        _restartingForPort = true;
        RefreshPortControls();
        try
        {
            var reason = await Task.Run(() => AppInstance.Restart(arguments));
            var description = reason switch
            {
                AppRestartFailureReason.RestartPending => "已有重启任务正在进行。",
                AppRestartFailureReason.InvalidUser => "当前 Windows 用户状态不允许自动重启。",
                _ => "Windows 未能自动重启应用。"
            };
            ShowNotice(InfoBarSeverity.Warning, "请手动重启应用", description + "端口已保存，关闭并重新打开后生效。");
        }
        catch (Exception exception)
        {
            StartupTrace.Write("Port restart failed: " + exception.Message);
            ShowNotice(InfoBarSeverity.Warning, "请手动重启应用", "端口已保存，关闭并重新打开后生效。");
        }
        finally
        {
            _restartingForPort = false;
            if (!_closed) RefreshPortControls();
        }
    }
}
