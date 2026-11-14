using System.Text.Json;

namespace CXAudioOutput;

public sealed class AppSettings
{
    public List<string> SelectedAdapterIds { get; set; } = new();
    public bool MultiSelect { get; set; } = true;
    public bool HasNetworkSetup { get; set; }
    public string AdbPath { get; set; } = string.Empty;
    public bool AutoStartService { get; set; } = true;
    public int Port { get; set; } = LaunchOptions.DefaultPort;
    public string Theme { get; set; } = "System";
    public string AccentColor { get; set; } = "System";
    public string MicaOpacity { get; set; } = "System";

    public AppSettings Clone() => new()
    {
        SelectedAdapterIds = new List<string>(SelectedAdapterIds),
        MultiSelect = MultiSelect,
        HasNetworkSetup = HasNetworkSetup,
        AdbPath = AdbPath,
        AutoStartService = AutoStartService,
        Port = Port,
        Theme = Theme,
        AccentColor = AccentColor,
        MicaOpacity = MicaOpacity
    };
}

public sealed class AppSettingsStore
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true
    };

    private readonly object _sync = new();

    public AppSettingsStore()
    {
        try
        {
            AppPaths.EnsureDirectories();
        }
        catch
        {
            // Settings are optional; the app can still stream without persistence.
        }
    }

    public string FilePath => AppPaths.SettingsFile;

    public AppSettings Load()
    {
        lock (_sync) return LoadNoLock();
    }

    public bool TrySave(AppSettings settings)
    {
        lock (_sync) return SaveNoLock(settings);
    }

    /// <summary>
    /// Read-modify-write helper. Network selection and the adb path are stored
    /// in the same file, so saving one must not discard the other.
    /// </summary>
    public bool Update(Action<AppSettings> mutate)
    {
        lock (_sync)
        {
            var settings = LoadNoLock();
            mutate(settings);
            return SaveNoLock(settings);
        }
    }

    private AppSettings LoadNoLock()
    {
        try
        {
            if (!File.Exists(AppPaths.SettingsFile)) return new AppSettings();
            var json = File.ReadAllText(AppPaths.SettingsFile);
            var settings = JsonSerializer.Deserialize<AppSettings>(json, JsonOptions);
            if (settings is null) return new AppSettings();
            settings.SelectedAdapterIds = (settings.SelectedAdapterIds ?? new List<string>())
                .Where(id => !string.IsNullOrWhiteSpace(id))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();
            settings.AdbPath ??= string.Empty;
            if (settings.Port is < 1 or > 65535) settings.Port = LaunchOptions.DefaultPort;
            if (settings.Theme is not ("System" or "Light" or "Dark")) settings.Theme = "System";
            settings.AccentColor = string.IsNullOrWhiteSpace(settings.AccentColor) ? "System" : settings.AccentColor;
            settings.MicaOpacity = string.IsNullOrWhiteSpace(settings.MicaOpacity) ? "System" : settings.MicaOpacity;
            return settings;
        }
        catch
        {
            return new AppSettings();
        }
    }

    private bool SaveNoLock(AppSettings settings)
    {
        try
        {
            AppPaths.EnsureDirectories();
            var json = JsonSerializer.Serialize(settings, JsonOptions);
            var temporary = AppPaths.SettingsFile + ".tmp";
            File.WriteAllText(temporary, json);
            File.Move(temporary, AppPaths.SettingsFile, true);
            return true;
        }
        catch
        {
            return false;
        }
    }
}
