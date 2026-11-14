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
                        var theme = _settingsContent.Children.OfType<ComboBox>().Single();
                        theme.SelectedIndex = round % 3;
                        var autoStart = _settingsContent.Children.OfType<ToggleSwitch>().First(toggle =>
                            string.Equals(toggle.Header?.ToString(), "启动时自动启动音频服务", StringComparison.Ordinal));
                        autoStart.IsOn = round % 2 == 0;
                        if (_accentSystemSwitch is null || _micaSystemSwitch is null || _micaOpacitySlider is null)
                            throw new InvalidOperationException("Appearance controls are missing.");
                        _accentSystemSwitch.IsOn = true;
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
#endif
}
