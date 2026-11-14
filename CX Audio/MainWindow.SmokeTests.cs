using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System.Text.Json;
using Windows.Graphics;

namespace CXAudioOutput;

internal static class UiRegression
{
#if DEBUG
    internal static bool Enabled => Environment.GetCommandLineArgs().Contains("--ui-smoke-test", StringComparer.Ordinal);
#else
    internal static bool Enabled => false;
#endif
}

public sealed partial class MainWindow
{
#if DEBUG
    private async Task RunUiRegressionAsync()
    {
        if (Environment.GetEnvironmentVariable("CXAUDIO_WEB_SMOKE") == "1")
        {
            await RunWebAppearanceBridgeAsync();
            return;
        }
        var checks = new List<string>();
        var resultPath = System.IO.Path.Combine(AppPaths.Root, "ui-smoke-results.json");
        try
        {
            await Task.Delay(700);
            if (_server is null || !_serverStarted) throw new InvalidOperationException("Server did not start.");
            var server = _server;
            var cachedPages = new Dictionary<Type, object>();
            var items = Navigation.MenuItems.Cast<NavigationViewItem>().Cast<object>()
                .Append(Navigation.SettingsItem).ToArray();
            var types = new[] { typeof(OverviewPage), typeof(ConnectionsPage), typeof(AndroidPage), typeof(SettingsPage) };
            for (var round = 0; round < 4; round++)
            {
                for (var index = 0; index < items.Length; index++)
                {
                    Navigation.SelectedItem = items[index];
                    await Task.Delay(250);
                    if (ContentFrame.CurrentSourcePageType != types[index])
                        throw new InvalidOperationException("Wrong page: " + types[index].Name);
                    if (ContentFrame.BackStack.Count != 0)
                        throw new InvalidOperationException("Navigation back stack grew.");
                    if (!ReferenceEquals(server, _server)) throw new InvalidOperationException("Server was recreated.");
                    var page = ContentFrame.Content;
                    if (cachedPages.TryGetValue(types[index], out var previous) && !ReferenceEquals(previous, page))
                        throw new InvalidOperationException("Page cache lost ownership: " + types[index].Name);
                    cachedPages[types[index]] = page;
                    if (index == 0)
                    {
                        AppWindow.Resize(new SizeInt32(round % 2 == 0 ? 720 : 1240, 780));
                        MasterVolumeSlider.Value = round % 2 == 0 ? 47 : 85;
                        QuickVolumeSlider.Value = round % 2 == 0 ? 47 : 85;
                        LatencyComboBox.SelectedIndex = round % 3;
                        Mute_Click(this, new RoutedEventArgs());
                        Pause_Click(this, new RoutedEventArgs());
                        await Task.Delay(350);
                        DrawAudioMeter();
                        if (AudioMeterCanvas.Children.Count != 49) throw new InvalidOperationException("Meter visual collection changed.");
                        if (AudioPanel.Parent is null) throw new InvalidOperationException("Audio controls are not hosted on Overview.");
                    }
                    if (index == 1)
                    {
                        RefreshNetwork_Click(this, new RoutedEventArgs());
                        await Task.Delay(200);
                        if (!ReferenceEquals(AdapterList.ItemsSource, _adapterItems)) throw new InvalidOperationException("Adapter binding lost.");
                        var expectedWide = round % 2 != 0;
                        if ((Grid.GetColumn(NetworkPanel) == 1) != expectedWide)
                            throw new InvalidOperationException("Wireless columns did not adapt to window width.");
                    }
                    if (index == 2)
                    {
                        if (AndroidContent.Parent is null)
                            throw new InvalidOperationException("Android wired view is not independently hosted.");
                        if (AndroidDeviceComboBox.ItemsSource is null)
                            throw new InvalidOperationException("Android device binding is missing.");
                    }
                    if (index == 3)
                    {
                        if (!_settingsContent.Children.OfType<Button>().Any(button =>
                                string.Equals(button.Content?.ToString(), "打开网页控制台", StringComparison.Ordinal)))
                            throw new InvalidOperationException("Browser console action is missing from Settings.");
                        if (_firewallButton is null) throw new InvalidOperationException("Advanced firewall action is missing.");
                        if (round == 0)
                        {
                            if (_portNumberBox is null || _portSaveButton is null || _portRestartButton is null)
                                throw new InvalidOperationException("Port GUI is missing.");
                            var previousPort = server.Settings.Load().Port;
                            var previousEnvironment = Environment.GetEnvironmentVariable("CXAUDIO_PORT");
                            try
                            {
                                _portNumberBox.Value = double.NaN;
                                if (_portSaveButton.IsEnabled) throw new InvalidOperationException("Empty port input was accepted.");
                                _portNumberBox.Value = 15443;
                                if (!_portSaveButton.IsEnabled || _portRestartButton.IsEnabled)
                                    throw new InvalidOperationException("Unsaved port controls are inconsistent.");
                                var portPeer = new Microsoft.UI.Xaml.Automation.Peers.ButtonAutomationPeer(_portSaveButton);
                                ((Microsoft.UI.Xaml.Automation.Provider.IInvokeProvider)portPeer.GetPattern(
                                    Microsoft.UI.Xaml.Automation.Peers.PatternInterface.Invoke)).Invoke();
                                if (server.Settings.Load().Port != 15443 || server.Settings.Load().Clone().Port != 15443 || !_portRestartButton.IsEnabled)
                                    throw new InvalidOperationException("GUI port was not persisted or marked pending restart.");
                                Environment.SetEnvironmentVariable("CXAUDIO_PORT", null);
                                if (LaunchOptions.Parse(Array.Empty<string>()).Port != 15443)
                                    throw new InvalidOperationException("Saved GUI port was ignored at startup.");
                                Environment.SetEnvironmentVariable("CXAUDIO_PORT", "15444");
                                if (LaunchOptions.Parse(Array.Empty<string>()).Port != 15444 || LaunchOptions.Parse(new[] { "--port=15445" }).Port != 15445)
                                    throw new InvalidOperationException("Port override precedence is incorrect.");
                                server.Settings.Update(value => value.Port = 0);
                                if (server.Settings.Load().Port != LaunchOptions.DefaultPort)
                                    throw new InvalidOperationException("Invalid saved port was not normalized.");
                                checks.Add("Port GUI: validation, save action, persistence and startup precedence passed");
                            }
                            finally
                            {
                                Environment.SetEnvironmentVariable("CXAUDIO_PORT", previousEnvironment);
                                server.Settings.Update(value => value.Port = previousPort);
                                _savedPort = previousPort;
                                _portNumberBox.Value = previousPort;
                                RefreshPortControls();
                            }
                        }
                        var theme = _settingsContent.Children.OfType<ComboBox>().Single();
                        theme.SelectedIndex = round % 3;
                        var autoStart = _settingsContent.Children.OfType<ToggleSwitch>().First(toggle =>
                            string.Equals(toggle.Header?.ToString(), "启动时自动启动音频服务", StringComparison.Ordinal));
                        autoStart.IsOn = round % 2 == 0;
                        if (_accentModeCombo is null || _micaSystemSwitch is null || _micaOpacitySlider is null)
                            throw new InvalidOperationException("Appearance controls are missing.");
                        if (_accentSwatches.Count != 48) throw new InvalidOperationException("Common accent palette is incomplete.");
                        var preset = _accentSwatches[round * 8 + 2];
                        var peer = new Microsoft.UI.Xaml.Automation.Peers.ButtonAutomationPeer(preset.Button);
                        var invoke = (Microsoft.UI.Xaml.Automation.Provider.IInvokeProvider)peer.GetPattern(
                            Microsoft.UI.Xaml.Automation.Peers.PatternInterface.Invoke);
                        invoke.Invoke();
                        await Task.Delay(100);
                        if (_accentSetting != preset.Hex || _accentModeCombo.SelectedIndex != 1 || preset.Check.Visibility != Visibility.Visible)
                            throw new InvalidOperationException("Swatch click did not apply and mark the selected color.");
                        if (_server.Settings.Load().AccentColor != preset.Hex) throw new InvalidOperationException("Preset accent was not saved.");
                        SelectAccent("#7C3AED");
                        if (_server.Settings.Load().AccentColor != "#7C3AED") throw new InvalidOperationException("Custom accent was not saved.");
                        var customResources = (ResourceDictionary)Application.Current.Resources.ThemeDictionaries[RootGrid.ActualTheme == ElementTheme.Dark ? "Dark" : "Light"];
                        if (((Microsoft.UI.Xaml.Media.SolidColorBrush)customResources["ToggleSwitchFillOn"]).Color != _acidBrush.Color ||
                            ((Microsoft.UI.Xaml.Media.SolidColorBrush)QuickVolumeSlider.Foreground).Color != _acidBrush.Color ||
                            ((Microsoft.UI.Xaml.Media.SolidColorBrush)ServicePill.Background).Color.R != _acidBrush.Color.R ||
                            _adapterItems.Where(adapter => adapter.IsUp).Any(adapter => ((Microsoft.UI.Xaml.Media.SolidColorBrush)adapter.StatusBrush).Color != _acidBrush.Color))
                            throw new InvalidOperationException("Native controls or live status indicators did not refresh their custom accent.");
                        _accentModeCombo.SelectedIndex = 0;
                        var systemAccent = GetSystemAccent();
                        if (_accentPreview?.Color != systemAccent) throw new InvalidOperationException("System accent preview is stale.");
                        var themeResources = (ResourceDictionary)Application.Current.Resources.ThemeDictionaries[RootGrid.ActualTheme == ElementTheme.Dark ? "Dark" : "Light"];
                        var nativeBrush = (Microsoft.UI.Xaml.Media.SolidColorBrush)themeResources["AccentFillColorDefaultBrush"];
                        var toggleBrush = (Microsoft.UI.Xaml.Media.SolidColorBrush)themeResources["ToggleSwitchFillOn"];
                        if (nativeBrush.Color != _acidBrush.Color || _meterBrush.Color.R != _acidBrush.Color.R || toggleBrush.Color != nativeBrush.Color)
                            throw new InvalidOperationException($"Accent mismatch: theme={RootGrid.ActualTheme}, native={nativeBrush.Color}, toggle={toggleBrush.Color}, app={_acidBrush.Color}, meter={_meterBrush.Color}.");
                        if (((NavigationViewItem)Navigation.SettingsItem).Content?.ToString() != "设置" ||
                            autoStart.OnContent?.ToString() != "开" || autoStart.OffContent?.ToString() != "关" ||
                            MultiSelectSwitch.OnContent?.ToString() != "开" || MultiSelectSwitch.OffContent?.ToString() != "关" ||
                            Microsoft.Windows.Globalization.ApplicationLanguages.PrimaryLanguageOverride != "zh-CN")
                            throw new InvalidOperationException("Chinese labels or resource language were not applied.");
                        checks.Add($"round {round + 1}: accent palette, custom/system persistence and Chinese controls passed");
                        _micaSystemSwitch.IsOn = true;
                        var saved = _server.Settings.Load();
                        if (saved.AutoStartService != autoStart.IsOn) throw new InvalidOperationException("Startup setting not saved.");
                        if (saved.Theme != (string)((ComboBoxItem)theme.SelectedItem).Tag) throw new InvalidOperationException("Theme setting not saved.");
                        if (!string.Equals(saved.AccentColor, "System", StringComparison.OrdinalIgnoreCase)) throw new InvalidOperationException("Accent setting not saved.");
                        if (!string.Equals(saved.MicaOpacity, "System", StringComparison.OrdinalIgnoreCase)) throw new InvalidOperationException("Mica setting not saved.");
                    }
                    checks.Add($"round {round + 1}: {types[index].Name} passed");
                    StartupTrace.Write(checks[^1]);
                }
            }
            Navigation.SelectedItem = items[3];
            var themeControl = _settingsContent.Children.OfType<ComboBox>().Single();
            themeControl.SelectedIndex = 1;
            SelectAccent("#0078D4");
            AppWindow.Resize(new SizeInt32(1060, 980));
            await Task.Delay(250);
            await CaptureUiElementAsync(RootGrid, "settings-light.png");
            themeControl.SelectedIndex = 2;
            await Task.Delay(150);
            var beforeDialog = _server.Settings.Load().AccentColor;
            var cancelled = ShowCustomAccentDialogAsync();
            await Task.Delay(300);
            if (_accentDialog is null || _accentDialogPicker is null) throw new InvalidOperationException("Custom color dialog did not open.");
            _accentDialogPicker.Color = Microsoft.UI.Colors.Red;
            _accentDialog.Hide();
            await cancelled;
            if (_server.Settings.Load().AccentColor != beforeDialog) throw new InvalidOperationException("Cancelling changed the accent.");
            var confirmed = ShowCustomAccentDialogAsync();
            await Task.Delay(300);
            if (_accentDialog is null || _accentDialogPicker is null) throw new InvalidOperationException("Custom color dialog did not reopen.");
            _accentDialogPicker.Color = Microsoft.UI.ColorHelper.FromArgb(255, 124, 58, 237);
            var pickerLabels = VisualDescendants<TextBlock>(_accentDialogPicker).Select(text => text.Text).ToArray();
            var untranslated = new[] { "Red", "Green", "Blue", "Hue", "Saturation", "Value", "More", "Less" };
            if (pickerLabels.Any(label => untranslated.Contains(label))) throw new InvalidOperationException("ColorPicker contains English UI strings.");
            await CaptureUiElementAsync(_accentDialog, "custom-color-dark.png");
            var primary = VisualDescendants<Button>(_accentDialog).First(button => button.Name == "PrimaryButton");
            var primaryPeer = new Microsoft.UI.Xaml.Automation.Peers.ButtonAutomationPeer(primary);
            ((Microsoft.UI.Xaml.Automation.Provider.IInvokeProvider)primaryPeer.GetPattern(Microsoft.UI.Xaml.Automation.Peers.PatternInterface.Invoke)).Invoke();
            await confirmed;
            if (_server.Settings.Load().AccentColor != "#7C3AED") throw new InvalidOperationException("Dialog confirmation did not save the chosen color.");
            checks.Add("custom color dialog: Chinese controls, cancel and confirm passed");
            SelectAccent("System");
            themeControl.SelectedIndex = 0;
            Navigation.SelectedItem = items[0];
            await Task.Delay(300);
            File.WriteAllText(resultPath, JsonSerializer.Serialize(new { passed = true, checks, demo = _options.Demo }));
            StartupTrace.Write("UI REGRESSION PASS");
        }
        catch (Exception exception)
        {
            File.WriteAllText(resultPath, JsonSerializer.Serialize(new { passed = false, checks, error = exception.ToString() }));
            StartupTrace.Write("UI REGRESSION FAIL: " + exception);
        }
        finally
        {
            if (_server is not null) await _server.StopAsync();
            Close();
        }
    }

    // Local, explicit opt-in debug harness. No writable appearance HTTP endpoint.
    private async Task RunWebAppearanceBridgeAsync()
    {
        var commandPath = System.IO.Path.Combine(AppPaths.Root, "appearance-command.json");
        var resultPath = System.IO.Path.Combine(AppPaths.Root, "appearance-result.json");
        string? lastCommand = null;
        while (!_closed)
        {
            try
            {
                if (File.Exists(commandPath))
                {
                    var json = await File.ReadAllTextAsync(commandPath);
                    if (json != lastCommand)
                    {
                        using var command = JsonDocument.Parse(json);
                        var value = command.RootElement;
                        if (value.TryGetProperty("theme", out var theme)) ApplyTheme(theme.GetString());
                        if (value.TryGetProperty("accentColor", out var accent)) SelectAccent(accent.GetString() ?? "System");
                        lastCommand = json;
                        await File.WriteAllTextAsync(resultPath, JsonSerializer.Serialize(new
                        {
                            id = value.GetProperty("id").GetInt32(),
                            appearance = _server?.State.Appearance
                        }, new JsonSerializerOptions(JsonSerializerDefaults.Web)));
                    }
                }
                await Task.Delay(100, _lifetimeCts.Token);
            }
            catch (OperationCanceledException) when (_closed || _lifetimeCts.IsCancellationRequested) { break; }
            catch (IOException) { await Task.Delay(100); }
            catch (JsonException) { await Task.Delay(100); }
        }
    }

    private static IEnumerable<T> VisualDescendants<T>(DependencyObject parent) where T : DependencyObject
    {
        for (var index = 0; index < Microsoft.UI.Xaml.Media.VisualTreeHelper.GetChildrenCount(parent); index++)
        {
            var child = Microsoft.UI.Xaml.Media.VisualTreeHelper.GetChild(parent, index);
            if (child is T match) yield return match;
            foreach (var nested in VisualDescendants<T>(child)) yield return nested;
        }
    }

    private static async Task CaptureUiElementAsync(FrameworkElement element, string fileName)
    {
        var bitmap = new Microsoft.UI.Xaml.Media.Imaging.RenderTargetBitmap();
        await bitmap.RenderAsync(element);
        var pixels = await bitmap.GetPixelsAsync();
        using var stream = new Windows.Storage.Streams.InMemoryRandomAccessStream();
        var encoder = await Windows.Graphics.Imaging.BitmapEncoder.CreateAsync(Windows.Graphics.Imaging.BitmapEncoder.PngEncoderId, stream);
        var bytes = new byte[pixels.Length];
        using (var reader = Windows.Storage.Streams.DataReader.FromBuffer(pixels)) reader.ReadBytes(bytes);
        encoder.SetPixelData(Windows.Graphics.Imaging.BitmapPixelFormat.Bgra8, Windows.Graphics.Imaging.BitmapAlphaMode.Premultiplied,
            (uint)bitmap.PixelWidth, (uint)bitmap.PixelHeight, 96, 96, bytes);
        await encoder.FlushAsync();
        using var input = stream.GetInputStreamAt(0);
        using var dataReader = new Windows.Storage.Streams.DataReader(input);
        await dataReader.LoadAsync((uint)stream.Size);
        var png = new byte[(int)stream.Size];
        dataReader.ReadBytes(png);
        await File.WriteAllBytesAsync(System.IO.Path.Combine(AppPaths.Root, fileName), png);
    }
#endif
}
