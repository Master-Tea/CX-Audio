using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Automation;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Windows.UI;
using Windows.UI.ViewManagement;

namespace CXAudioOutput;

public sealed partial class MainWindow
{
    private readonly UISettings _appearanceSettings = new();
    private readonly List<(Button Button, Border Outline, TextBlock Check, string Hex)> _accentSwatches = new();
    private ComboBox? _accentModeCombo;
    private ContentDialog? _accentDialog;
    private ColorPicker? _accentDialogPicker;
    private TextBlock? _accentSummary;
    private SolidColorBrush? _accentPreview;
    private string _accentSetting = "System";
    private string _lastManualAccent = string.Empty;
    private bool _updatingAccentControls;

    // Windows-style common colors, ordered by hue and tone. No preset is applied by default.
    private static readonly (string Name, string Hex)[] CommonAccentColors =
    [
        ("金色", "#FFB900"), ("暖金色", "#FF8C00"), ("橙色", "#F7630C"), ("深橙色", "#CA5010"),
        ("铁锈色", "#DA3B01"), ("红棕色", "#EF6950"), ("珊瑚红", "#D13438"), ("红色", "#E81123"),
        ("玫红色", "#EA005E"), ("深玫红", "#C30052"), ("紫红色", "#E3008C"), ("深紫红", "#BF0077"),
        ("兰花紫", "#C239B3"), ("深兰花紫", "#9A0089"), ("紫色", "#881798"), ("梅紫色", "#744DA9"),
        ("浅蓝色", "#00BCF2"), ("天蓝色", "#0099BC"), ("蓝色", "#0078D4"), ("深蓝色", "#0063B1"),
        ("鸢尾蓝", "#8E8CD8"), ("蓝紫色", "#6B69D6"), ("靛蓝色", "#4F6BED"), ("深靛蓝", "#6B4CC3"),
        ("青色", "#00B7C3"), ("深青色", "#038387"), ("蓝绿色", "#00B294"), ("深蓝绿", "#018574"),
        ("海绿色", "#00CC6A"), ("祖母绿", "#10893E"), ("草绿色", "#107C10"), ("翠绿色", "#498205"),
        ("浅灰蓝", "#7A7574"), ("石板灰", "#5D5A58"), ("蓝灰色", "#68768A"), ("深蓝灰", "#515C6B"),
        ("灰紫色", "#6B5B95"), ("深灰紫", "#5C2D91"), ("橄榄灰", "#7E735F"), ("棕灰色", "#4A5459"),
        ("中灰色", "#767676"), ("炭灰色", "#4C4A48"), ("苔绿色", "#647C64"), ("深苔绿", "#525E54"),
        ("鼠尾草绿", "#847545"), ("橄榄色", "#7E6B43"), ("可可色", "#8A6D3B"), ("深棕色", "#5D4037")
    ];

    // WinUI accent aliases can be materialized per theme; refresh each cached brush.
    private static readonly (string Source, string[] Targets)[] NativeAccentAliases =
    [
        ("AccentFillColorDefaultBrush",
        [
            "AccentButtonBackground", "AppBarToggleButtonBackgroundChecked", "CalendarViewSelectedBorderBrush",
            "CalendarViewTodayBackground", "CheckBoxCheckBackgroundFillChecked", "CheckBoxCheckBackgroundFillIndeterminate",
            "CheckBoxCheckBackgroundStrokeChecked", "CheckBoxCheckBackgroundStrokeIndeterminate", "ComboBoxItemPillFillBrush",
            "CommandBarFlyoutAppBarButtonBackgroundChecked", "GridViewItemCheckBoxSelectedBrush", "GridViewItemSelectedBorderBrush",
            "InfoBadgeBackground", "InkToolbarToggleButtonBackgroundChecked", "ItemContainerSelectionVisualBackground",
            "ItemContainerSelectionVisualPointerOverBackground", "ItemContainerSelectionVisualPressedBackground", "ListViewItemCheckBoxSelectedBrush",
            "ListViewItemMultiArrangeOverlayTextBackground", "ListViewItemSelectionIndicatorBrush", "ListViewItemSelectionIndicatorPointerOverBrush",
            "ListViewItemSelectionIndicatorPressedBrush", "NavigationViewSelectionIndicatorForeground", "PagerControlSelectionIndicatorForeground",
            "PersonPictureEllipseBadgeFillThemeBrush", "PivotHeaderItemSelectedPipeFill", "ProgressBarForeground",
            "ProgressRingForegroundThemeBrush", "RadioButtonOuterEllipseCheckedFill", "RadioButtonOuterEllipseCheckedStroke",
            "RatingControlPointerOverSelectedForeground", "RatingControlSelectedForeground", "SelectorBarItemPillFill",
            "SliderThumbBackground", "SliderTrackValueFill", "SplitButtonBackgroundChecked",
            "SwipeItemPostThresholdExecuteBackground", "ToggleButtonBackgroundChecked", "ToggleSwitchFillOn",
            "ToggleSwitchStrokeOn", "TreeViewItemSelectionIndicatorForeground", "TreeViewItemSelectionIndicatorForegroundPointerOver",
            "TreeViewItemSelectionIndicatorForegroundPressed", "VerticalThumbBrush",
        ]),
        ("AccentFillColorSecondaryBrush",
        [
            "AccentButtonBackgroundPointerOver", "AppBarToggleButtonBackgroundCheckedPointerOver", "CalendarViewFocusBorderBrush",
            "CalendarViewSelectedHoverBorderBrush", "CalendarViewTodayHoverBackground", "CheckBoxCheckBackgroundFillCheckedPointerOver",
            "CheckBoxCheckBackgroundFillIndeterminatePointerOver", "CheckBoxCheckBackgroundStrokeCheckedPointerOver", "CheckBoxCheckBackgroundStrokeIndeterminatePointerOver",
            "CommandBarFlyoutAppBarButtonBackgroundCheckedPointerOver", "GridViewItemCheckBoxSelectedPointerOverBrush", "GridViewItemSelectedPointerOverBorderBrush",
            "InkToolbarToggleButtonBackgroundCheckedPointerOver", "ListViewItemCheckBoxSelectedPointerOverBrush", "RadioButtonOuterEllipseCheckedFillPointerOver",
            "RadioButtonOuterEllipseCheckedStrokePointerOver", "SliderThumbBackgroundPointerOver", "SliderTrackValueFillPointerOver",
            "SplitButtonBackgroundCheckedPointerOver", "ToggleButtonBackgroundCheckedPointerOver", "ToggleSwitchFillOnPointerOver",
            "ToggleSwitchStrokeOnPointerOver",
        ]),
        ("AccentFillColorSelectedTextBackgroundBrush",
        [
            "TextControlSelectionHighlightColor",
        ]),
        ("AccentFillColorTertiaryBrush",
        [
            "AccentButtonBackgroundPressed", "AppBarToggleButtonBackgroundCheckedPressed", "CalendarViewTodayBlackoutBackground",
            "CalendarViewTodayPressedBackground", "CheckBoxCheckBackgroundFillCheckedPressed", "CheckBoxCheckBackgroundFillIndeterminatePressed",
            "CheckBoxCheckBackgroundStrokeCheckedPressed", "CheckBoxCheckBackgroundStrokeIndeterminatePressed", "CommandBarFlyoutAppBarButtonBackgroundCheckedPressed",
            "GridViewItemCheckBoxSelectedPressedBrush", "GridViewItemSelectedPressedBorderBrush", "InkToolbarToggleButtonBackgroundCheckedPressed",
            "ListViewItemCheckBoxSelectedPressedBrush", "RadioButtonOuterEllipseCheckedFillPressed", "RadioButtonOuterEllipseCheckedStrokePressed",
            "SliderThumbBackgroundPressed", "SliderTrackValueFillPressed", "SplitButtonBackgroundCheckedPressed",
            "ToggleButtonBackgroundCheckedPressed", "ToggleSwitchFillOnPressed", "ToggleSwitchStrokeOnPressed",
        ]),
        ("AccentTextFillColorPrimaryBrush",
        [
            "CalendarViewSelectedForeground", "CalendarViewSelectedHoverForeground", "HyperlinkButtonForeground",
            "HyperlinkForeground", "InfoBarHyperlinkButtonForeground", "InkToolbarAccentColorForegroundThemeBrush",
        ]),
        ("AccentTextFillColorSecondaryBrush",
        [
            "HyperlinkButtonForegroundPointerOver", "HyperlinkForegroundPointerOver", "InkToolbarAccentHoverColorForegroundThemeBrush",
        ]),
        ("AccentTextFillColorTertiaryBrush",
        [
            "CalendarViewSelectedPressedForeground", "HyperlinkButtonForegroundPressed", "HyperlinkForegroundPressed",
        ]),
    ];

    private static readonly Dictionary<string, SolidColorBrush> AccentBrushes = new();

    internal static void InitializeAccentResources()
    {
        var baseKeys = new[]
        {
            "AcidBrush", "AcidSoftBrush", "AccentFillColorDefaultBrush", "AccentFillColorSecondaryBrush",
            "AccentFillColorTertiaryBrush", "AccentTextFillColorPrimaryBrush", "AccentTextFillColorSecondaryBrush",
            "AccentTextFillColorTertiaryBrush", "AccentFillColorSelectedTextBackgroundBrush",
            "SystemControlForegroundAccentBrush", "SystemControlHighlightAccentBrush", "SystemControlBackgroundAccentBrush"
        };
        var resources = Application.Current.Resources;
        foreach (var key in baseKeys.Concat(NativeAccentAliases.SelectMany(group => group.Targets)).Distinct())
        {
            var brush = new SolidColorBrush();
            AccentBrushes[key] = brush;
            // A single window uses one active accent palette. Share the same object in
            // all lookup scopes so cached native styles cannot retain an old theme copy.
            resources[key] = brush;
            foreach (var theme in new[] { "Light", "Dark" })
                ((ResourceDictionary)resources.ThemeDictionaries[theme])[key] = brush;
        }
    }

    private void InitializeAccentSettings(AppSettings settings)
    {
        _lastManualAccent = TryParseColor(settings.AccentColor, out var saved)
            ? ToColorHex(saved) : ToColorHex(GetSystemAccent());
        var panel = new StackPanel { Spacing = 12 };
        panel.Children.Add(new TextBlock { Text = "主题色", FontSize = 18, FontWeight = Microsoft.UI.Text.FontWeights.SemiBold });
        panel.Children.Add(new TextBlock
        {
            Text = "跟随 Windows 强调色，或选择常用颜色和自定义颜色。选择后立即应用。",
            TextWrapping = TextWrapping.Wrap,
            Style = (Style)Application.Current.Resources["CaptionStyle"]
        });
        _accentModeCombo = new ComboBox { Header = "主题色来源", MinWidth = 240 };
        AutomationProperties.SetName(_accentModeCombo, "主题色来源");
        _accentModeCombo.Items.Add(new ComboBoxItem { Content = "跟随系统强调色", Tag = "System" });
        _accentModeCombo.Items.Add(new ComboBoxItem { Content = "手动选择", Tag = "Manual" });
        _accentModeCombo.SelectedIndex = _accentSetting == "System" ? 0 : 1;
        _accentModeCombo.SelectionChanged += (_, _) =>
        {
            if (_settingsInitializing || _updatingAccentControls) return;
            SelectAccent(_accentModeCombo.SelectedIndex == 0 ? "System" : _lastManualAccent);
        };
        panel.Children.Add(_accentModeCombo);

        var previewRow = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 10 };
        _accentPreview = new SolidColorBrush(GetSystemAccent());
        previewRow.Children.Add(new Border { Width = 28, Height = 28, CornerRadius = new CornerRadius(4), Background = _accentPreview });
        _accentSummary = new TextBlock { VerticalAlignment = VerticalAlignment.Center };
        previewRow.Children.Add(_accentSummary);
        panel.Children.Add(previewRow);
        panel.Children.Add(new TextBlock { Text = "常用颜色", FontWeight = Microsoft.UI.Text.FontWeights.SemiBold });
        var palette = new Grid { HorizontalAlignment = HorizontalAlignment.Left };
        for (var column = 0; column < 8; column++) palette.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(44) });
        for (var row = 0; row < 6; row++) palette.RowDefinitions.Add(new RowDefinition { Height = new GridLength(44) });
        for (var index = 0; index < CommonAccentColors.Length; index++)
        {
            var (name, hex) = CommonAccentColors[index];
            TryParseColor(hex, out var color);
            var check = new TextBlock
            {
                Text = "✓", FontSize = 20, FontWeight = Microsoft.UI.Text.FontWeights.Bold,
                Foreground = new SolidColorBrush(IsLightColor(color) ? Microsoft.UI.Colors.Black : Microsoft.UI.Colors.White),
                HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center,
                Visibility = Visibility.Collapsed
            };
            var button = new Button
            {
                Width = 36, Height = 36, Padding = new Thickness(0), CornerRadius = new CornerRadius(3),
                Background = new SolidColorBrush(color), Content = check, Tag = hex,
                BorderBrush = new SolidColorBrush(Color.FromArgb(60, 128, 128, 128)), BorderThickness = new Thickness(1)
            };
            // Keep the swatch color itself stable in pointer/pressed states.
            button.Resources["ButtonBackgroundPointerOver"] = button.Background;
            button.Resources["ButtonBackgroundPressed"] = button.Background;
            AutomationProperties.SetName(button, name + " " + hex);
            ToolTipService.SetToolTip(button, name + " " + hex);
            button.Click += (_, _) => SelectAccent(hex);
            var outline = new Border { Padding = new Thickness(2), BorderThickness = new Thickness(2), CornerRadius = new CornerRadius(6), Child = button };
            Grid.SetColumn(outline, index % 8);
            Grid.SetRow(outline, index / 8);
            palette.Children.Add(outline);
            _accentSwatches.Add((button, outline, check, hex));
        }
        panel.Children.Add(palette);
        var custom = new Button { Content = "自定义颜色…", HorizontalAlignment = HorizontalAlignment.Left };
        custom.Click += async (_, _) => await ShowCustomAccentDialogAsync();
        panel.Children.Add(custom);
        _settingsContent.Children.Add(new Border { Style = (Style)Application.Current.Resources["AppCardStyle"], Child = panel });
        _appearanceSettings.ColorValuesChanged += AppearanceSettings_ColorValuesChanged;
        RootGrid.ActualThemeChanged += AppearanceThemeChanged;
        RefreshAccentControls();
    }

    private async Task ShowCustomAccentDialogAsync()
    {
        if (_accentDialog is not null || _closed) return;
        var picker = new ColorPicker
        {
            Language = "zh-CN", MinWidth = 280, MaxWidth = 420, IsAlphaEnabled = false,
            IsAlphaSliderVisible = false, IsAlphaTextInputVisible = false, IsMoreButtonVisible = false,
            Color = TryParseColor(_lastManualAccent, out var color) ? color : GetSystemAccent()
        };
        var dialog = new ContentDialog
        {
            XamlRoot = RootGrid.XamlRoot, Language = "zh-CN", RequestedTheme = RootGrid.ActualTheme,
            Title = "选择自定义主题色", Content = picker,
            PrimaryButtonText = "完成", CloseButtonText = "取消", DefaultButton = ContentDialogButton.Primary
        };
        _accentDialog = dialog;
        _accentDialogPicker = picker;
        try
        {
            if (await dialog.ShowAsync() == ContentDialogResult.Primary && !_closed)
                SelectAccent(ToColorHex(picker.Color));
        }
        finally
        {
            _accentDialog = null;
            _accentDialogPicker = null;
        }
    }

    private void SelectAccent(string value)
    {
        ApplyAccent(value);
        if (_accentSetting != "System") _lastManualAccent = _accentSetting;
        SaveSettings(settings => settings.AccentColor = _accentSetting);
        RefreshAccentControls();
    }

    private void RefreshAccentControls()
    {
        var accent = TryParseColor(_accentSetting, out var custom) ? custom : GetSystemAccent();
        _updatingAccentControls = true;
        try
        {
            if (_accentModeCombo is not null) _accentModeCombo.SelectedIndex = _accentSetting == "System" ? 0 : 1;
            if (_accentPreview is not null) _accentPreview.Color = accent;
            if (_accentSummary is not null) _accentSummary.Text = (_accentSetting == "System" ? "当前系统强调色：" : "当前自选颜色：") + ToColorHex(accent);
            foreach (var swatch in _accentSwatches)
            {
                var selected = _accentSetting != "System" && string.Equals(swatch.Hex, _accentSetting, StringComparison.OrdinalIgnoreCase);
                swatch.Check.Visibility = selected ? Visibility.Visible : Visibility.Collapsed;
                swatch.Outline.BorderBrush = selected
                    ? new SolidColorBrush(RootGrid.ActualTheme == ElementTheme.Dark ? Microsoft.UI.Colors.White : Microsoft.UI.Colors.Black)
                    : new SolidColorBrush(Microsoft.UI.Colors.Transparent);
                AutomationProperties.SetItemStatus(swatch.Button, selected ? "已选择" : "未选择");
            }
        }
        finally { _updatingAccentControls = false; }
    }

    private void AppearanceSettings_ColorValuesChanged(UISettings sender, object args)
    {
        DispatcherQueue.TryEnqueue(() =>
        {
            if (_closed) return;
            ApplyAccent(_accentSetting);
            RefreshAccentControls();
        });
    }

    private void AppearanceThemeChanged(FrameworkElement sender, object args)
    {
        if (_closed) return;
        ApplyAccent(_accentSetting);
        ApplyMicaOpacity(_micaOpacitySetting);
        RefreshAccentControls();
    }

    private void ApplyAccent(string? value)
    {
        var manual = TryParseColor(value, out var color);
        _accentSetting = manual ? ToColorHex(color) : "System";
        var accent = manual ? color : GetSystemAccent();
        // System mode uses the Windows contrast-tuned shades. Manual mode produces the same tonal roles.
        var light1 = manual ? Mix(accent, Microsoft.UI.Colors.White, 0.25) : _appearanceSettings.GetColorValue(UIColorType.AccentLight1);
        var light2 = manual ? Mix(accent, Microsoft.UI.Colors.White, 0.45) : _appearanceSettings.GetColorValue(UIColorType.AccentLight2);
        var light3 = manual ? Mix(accent, Microsoft.UI.Colors.White, 0.65) : _appearanceSettings.GetColorValue(UIColorType.AccentLight3);
        var dark1 = manual ? Mix(accent, Microsoft.UI.Colors.Black, 0.20) : _appearanceSettings.GetColorValue(UIColorType.AccentDark1);
        var dark2 = manual ? Mix(accent, Microsoft.UI.Colors.Black, 0.40) : _appearanceSettings.GetColorValue(UIColorType.AccentDark2);
        var dark3 = manual ? Mix(accent, Microsoft.UI.Colors.Black, 0.60) : _appearanceSettings.GetColorValue(UIColorType.AccentDark3);
        var resources = Application.Current.Resources;
        var colors = new[] { accent, light1, light2, light3, dark1, dark2, dark3 };
        var keys = new[] { "SystemAccentColor", "SystemAccentColorLight1", "SystemAccentColorLight2", "SystemAccentColorLight3", "SystemAccentColorDark1", "SystemAccentColorDark2", "SystemAccentColorDark3" };
        for (var index = 0; index < keys.Length; index++) resources[keys[index]] = colors[index];
        var dictionary = AccentBrushes;
        var dark = RootGrid.ActualTheme == ElementTheme.Dark;
        var fill = dark ? light2 : dark1;
        var text = dark ? light3 : dark2;
        UpdateAccentBrush(dictionary, "AcidBrush", fill);
        UpdateAccentBrush(dictionary, "AcidSoftBrush", Color.FromArgb(dark ? (byte)38 : (byte)26, accent.R, accent.G, accent.B));
        UpdateAccentBrush(dictionary, "AccentFillColorDefaultBrush", fill);
        UpdateAccentBrush(dictionary, "AccentFillColorSecondaryBrush", fill, 0.9);
        UpdateAccentBrush(dictionary, "AccentFillColorTertiaryBrush", fill, 0.8);
        UpdateAccentBrush(dictionary, "AccentTextFillColorPrimaryBrush", text);
        UpdateAccentBrush(dictionary, "AccentTextFillColorSecondaryBrush", text);
        UpdateAccentBrush(dictionary, "AccentTextFillColorTertiaryBrush", dark ? light2 : dark1);
        UpdateAccentBrush(dictionary, "AccentFillColorSelectedTextBackgroundBrush", accent);
        foreach (var key in new[] { "SystemControlForegroundAccentBrush", "SystemControlHighlightAccentBrush", "SystemControlBackgroundAccentBrush" })
            UpdateAccentBrush(dictionary, key, accent);
        foreach (var group in NativeAccentAliases)
        {
            var source = (SolidColorBrush)dictionary[group.Source];
            foreach (var target in group.Targets)
                UpdateAccentBrush(dictionary, target, source.Color, source.Opacity);
        }
        var active = RootGrid.ActualTheme == ElementTheme.Dark ? light2 : dark1;
        _acidBrush.Color = active;
        _accentPillBrush.Color = Color.FromArgb(35, active.R, active.G, active.B);
        QuickVolumeSlider.Foreground = _acidBrush;
        MasterVolumeSlider.Foreground = _acidBrush;
        if (_micaOpacitySlider is not null) _micaOpacitySlider.Foreground = _acidBrush;
        _meterBrush.Color = Color.FromArgb(230, active.R, active.G, active.B);
        _meterDimBrush.Color = Color.FromArgb(55, active.R, active.G, active.B);
        foreach (var receiver in _receiverItems) receiver.SetAccent(active);
        foreach (var adapter in _adapterItems) adapter.SetAccent(active);
        _server?.State.SetAppearance(new AppearanceSnapshot(
            dark ? "Dark" : "Light",
            RootGrid.RequestedTheme == ElementTheme.Default ? "System" : RootGrid.RequestedTheme.ToString(),
            ToColorHex(accent), ToColorHex(active), ToColorHex(text),
            _accentSetting == "System" ? "System" : "Custom"));
    }

    private static void UpdateAccentBrush(Dictionary<string, SolidColorBrush> dictionary, string key, Color color, double opacity = 1)
    {
        if (dictionary.TryGetValue(key, out var brush))
        {
            // Mutate the existing brush so loaded and cached pages update immediately.
            brush.Color = color;
            brush.Opacity = opacity;
        }
        else dictionary[key] = new SolidColorBrush(color) { Opacity = opacity };
    }

    private Color GetSystemAccent() => _appearanceSettings.GetColorValue(UIColorType.Accent);

    private static Color Mix(Color source, Color target, double amount) => Color.FromArgb(255,
        (byte)Math.Round(source.R + (target.R - source.R) * amount),
        (byte)Math.Round(source.G + (target.G - source.G) * amount),
        (byte)Math.Round(source.B + (target.B - source.B) * amount));

    private static bool IsLightColor(Color color) => color.R * 0.299 + color.G * 0.587 + color.B * 0.114 > 160;
}
