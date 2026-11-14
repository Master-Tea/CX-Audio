using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Shapes;
using CXAudioOutput.Views;

namespace CXAudioOutput;

public sealed partial class MainWindow
{
    private AudioView _audioView = null!;
    private ConnectionsView _connectionsView = null!;
    private AndroidView _androidView = null!;
    private SystemView _systemView = null!;

    private void InitializeOwnedViews()
    {
        // Each view owns its XAML namescope and compiled bindings permanently.
        // Pages attach a detached view once, then remain in the Frame cache.
        _audioView = new AudioView { Host = this };
        _connectionsView = new ConnectionsView { Host = this };
        _androidView = new AndroidView { Host = this };
        _systemView = new SystemView { Host = this };
        InitializeMeter();
    }

    private Border AudioPanel => _audioView.AudioPanel;
    private Ellipse LiveDot => _audioView.LiveDot;
    private TextBlock StatusEyebrow => _audioView.StatusEyebrow;
    private TextBlock StatusHeadline => _audioView.StatusHeadline;
    private TextBlock SourceText => _audioView.SourceText;
    private TextBlock SampleText => _audioView.SampleText;
    private TextBlock UptimeText => _audioView.UptimeText;
    private Button PauseButton => _audioView.PauseButton;
    private FontIcon PauseIcon => _audioView.PauseIcon;
    private TextBlock PauseText => _audioView.PauseText;
    private Canvas AudioMeterCanvas => _audioView.AudioMeterCanvas;
    private TextBlock PeakText => _audioView.PeakText;
    private TextBlock MasterVolumeText => _audioView.MasterVolumeText;
    private Button MuteButton => _audioView.MuteButton;
    private FontIcon MuteIcon => _audioView.MuteIcon;
    private Slider MasterVolumeSlider => _audioView.MasterVolumeSlider;
    private TextBlock LatencyText => _audioView.LatencyText;
    private ComboBox LatencyComboBox => _audioView.LatencyComboBox;
    private Border SharePanel => _connectionsView.SharePanel;
    private Image QrImage => _connectionsView.QrImage;
    private StackPanel QrPlaceholder => _connectionsView.QrPlaceholder;
    private TextBlock AccessCodeText => _connectionsView.AccessCodeText;
    private ComboBox AddressComboBox => _connectionsView.AddressComboBox;
    private TextBlock ShareUrlText => _connectionsView.ShareUrlText;
    private Button CopyLinkButton => _connectionsView.CopyLinkButton;
    private Border NetworkPanel => _connectionsView.NetworkPanel;
    private ToggleSwitch MultiSelectSwitch => _connectionsView.MultiSelectSwitch;
    private TextBlock NetworkSummaryText => _connectionsView.NetworkSummaryText;
    private TextBlock UpdateStampText => _connectionsView.UpdateStampText;
    private Button SelectAllButton => _connectionsView.SelectAllButton;
    private Button ClearNetworkButton => _connectionsView.ClearNetworkButton;
    private InfoBar NetworkWarningBar => _connectionsView.NetworkWarningBar;
    private ListView AdapterList => _connectionsView.AdapterList;
    private Border ReceiversPanel => _connectionsView.ReceiversPanel;
    private TextBlock ReceiverCountText => _connectionsView.ReceiverCountText;
    private ListView ReceiverList => _connectionsView.ReceiverList;
    private TextBlock ReceiverEmptyText => _connectionsView.ReceiverEmptyText;
    private Border AndroidPanel => _androidView.AndroidPanel;
    private Ellipse AndroidWiredDot => _androidView.AndroidWiredDot;
    private TextBlock AndroidWiredPillText => _androidView.AndroidWiredPillText;
    private TextBlock AndroidTetherStatusText => _androidView.AndroidTetherStatusText;
    private Button UseTetherButton => _androidView.UseTetherButton;
    private TextBlock UseTetherButtonText => _androidView.UseTetherButtonText;
    private ComboBox AndroidDeviceComboBox => _androidView.AndroidDeviceComboBox;
    private TextBlock AndroidAdbStatusText => _androidView.AndroidAdbStatusText;
    private Button AndroidReverseButton => _androidView.AndroidReverseButton;
    private TextBlock AndroidReverseButtonText => _androidView.AndroidReverseButtonText;
    private Button AndroidStopReverseButton => _androidView.AndroidStopReverseButton;
    private Button AndroidPickAdbButton => _androidView.AndroidPickAdbButton;
    private Button AndroidOpenPhoneButton => _androidView.AndroidOpenPhoneButton;
    private TextBlock AndroidWiredUrlText => _androidView.AndroidWiredUrlText;
    private Button CopyAndroidUrlButton => _androidView.CopyAndroidUrlButton;
    private TextBlock AndroidWiredHintText => _androidView.AndroidWiredHintText;
    private Image AndroidQrImage => _androidView.AndroidQrImage;
    private TextBlock AndroidQrPlaceholder => _androidView.AndroidQrPlaceholder;
    private InfoBar AndroidWiredInfoBar => _androidView.AndroidWiredInfoBar;
    private Border SystemPanel => _systemView.SystemPanel;
    private TextBlock PortText => _systemView.PortText;
    private TextBlock WebUrlText => _systemView.WebUrlText;
    private TextBlock AudioDeviceText => _systemView.AudioDeviceText;
    private TextBlock SettingsPathText => _systemView.SettingsPathText;
}
