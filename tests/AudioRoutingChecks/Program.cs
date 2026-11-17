using System.Text.Json;
using System.Diagnostics;
using CXAudioOutput;

if (args.Length == 1 && args[0] == "--guard-parent")
{
    Thread.Sleep(Timeout.Infinite);
    return;
}

var testRoot = Path.Combine(Path.GetTempPath(), "CXAudioRoutingChecks-" + Guid.NewGuid().ToString("N"));
Directory.CreateDirectory(testRoot);
var passed = 0;

Run("ordinary mode leaves all endpoint states unchanged", (platform, route, journal) =>
{
    route.Recover();
    Check(platform.Mutations == 0 && !File.Exists(journal), "Normal startup modified the audio system.");
});
Run("advanced mode isolates all other endpoints and restores all three roles", (platform, route, journal) =>
{
    route.Enable("cable");
    Check(route.IsAdvanced && route.EndpointId == "cable", "Advanced endpoint wasn't pinned.");
    Check(platform.GetOutputs().Where(output => output.IsActive).Single().Id == "cable", "Physical output remained active.");
    Check(platform.Defaults.Values.All(id => id == "cable"), "Not every role uses the virtual endpoint.");
    route.Recover();
    AssertOriginal(platform, route, journal);
});
Run("new and reenabled outputs are isolated and added to recovery", (platform, route, journal) =>
{
    route.Enable("cable");
    platform.Outputs.Add(new AudioOutputEndpoint("hotplug", "HDMI hotplug", 1, false));
    platform.SetState("speaker", 1);
    platform.Defaults[ERole.Communications] = "hotplug";
    route.Enforce();
    Check(platform.GetOutputs().Where(output => output.IsActive).Single().Id == "cable", "Hotplug output escaped isolation.");
    route.Recover();
    Check(platform.Outputs.Single(output => output.Id == "hotplug").IsActive, "Hotplug output wasn't restored.");
    AssertOriginal(platform, route, journal);
});
Run("reenabling a previously disabled output does not change its original state", (platform, route, journal) =>
{
    route.Enable("cable");
    platform.SetState("disabled", 1);
    route.Enforce();
    route.Recover();
    AssertOriginal(platform, route, journal);
});
Run("virtual endpoint loss never reenables or selects physical output", (platform, route, journal) =>
{
    route.Enable("cable");
    platform.SetState("cable", 2);
    ExpectFailure(route.Enforce);
    Check(route.IsAdvanced && route.EndpointId == "cable", "Lost virtual endpoint fell back to normal mode.");
    Check(platform.GetOutputs().All(output => !output.IsActive), "Physical audio was reenabled after virtual device loss.");
    platform.SetState("cable", 1);
    route.Recover();
    AssertOriginal(platform, route, journal);
});
Run("partial activation failure rolls back the transaction", (platform, route, journal) =>
{
    platform.FailDisableOnce = "headphones";
    ExpectFailure(() => route.Enable("cable"));
    AssertOriginal(platform, route, journal);
});
Run("Windows ignoring disable is detected and rolled back", (platform, route, journal) =>
{
    platform.IgnoreDisable = true;
    ExpectFailure(() => route.Enable("cable"));
    AssertOriginal(platform, route, journal);
});
Run("failed restore keeps durable record and succeeds on retry", (platform, route, journal) =>
{
    route.Enable("cable");
    platform.FailEnableOnce = "speaker";
    ExpectFailure(route.Recover);
    Check(File.Exists(journal) && !route.IsAdvanced, "Failed restore discarded its journal.");
    var recovered = new AudioRouteController(platform, journal);
    recovered.Recover();
    AssertOriginal(platform, recovered, journal);
});
Run("crash recovery works from a fresh controller", (platform, route, journal) =>
{
    route.Enable("cable");
    var recovered = new AudioRouteController(platform, journal);
    recovered.Recover();
    recovered.Recover();
    AssertOriginal(platform, recovered, journal);
});
Run("removed original default retains a retryable recovery record", (platform, route, journal) =>
{
    route.Enable("cable");
    var removed = platform.Outputs.Single(output => output.Id == "headphones");
    platform.Outputs.Remove(removed);
    ExpectFailure(route.Recover);
    Check(File.Exists(journal), "Disconnected device lost its restore record.");
    platform.Outputs.Add(removed);
    route.Recover();
    AssertOriginal(platform, route, journal);
});
Run("physical endpoint cannot be selected as a virtual speaker", (platform, route, journal) =>
{
    ExpectFailure(() => route.Enable("speaker"));
    Check(platform.Mutations == 0 && !File.Exists(journal), "Invalid endpoint changed audio routing.");
});
Run("corrupt recovery records are retained without system mutation", (platform, route, journal) =>
{
    File.WriteAllText(journal, "{ invalid json");
    ExpectFailure(route.Recover);
    Check(File.Exists(journal) && platform.Mutations == 0, "Corrupt record was discarded or acted on.");
});
Run("no original default is also recoverable", (platform, route, journal) =>
{
    platform.Defaults.Clear();
    route.Enable("cable");
    route.Recover();
    Check(!route.NeedsRecovery && platform.Outputs.Single(output => output.Id == "speaker").IsActive,
        "Recovery requires a prior default endpoint.");
});

if (args.Length > 0)
{
    VirtualSpeakerInstaller.VerifyInstaller(Path.GetFullPath(args[0]));
    Console.WriteLine("PASS: official VB-CABLE installer passes Windows signature and publisher validation.");
    var unsigned = Path.Combine(testRoot, "unsigned.exe");
    File.WriteAllText(unsigned, "not a signed executable");
    ExpectFailure(() => VirtualSpeakerInstaller.VerifyInstaller(unsigned));
    ExpectFailure(() => VirtualSpeakerInstaller.VerifyInstaller(Path.Combine(Environment.SystemDirectory, "cmd.exe")));
    Console.WriteLine("PASS: unsigned files and a different publisher are rejected.");
}

if (args.Length > 1)
{
    var guardDirectory = Path.Combine(testRoot, "guard");
    Directory.CreateDirectory(guardDirectory);
    var journal = Path.Combine(guardDirectory, "audio-route-recovery.json");
    // An empty transaction exercises crash recovery without touching Windows audio.
    File.WriteAllText(journal, JsonSerializer.Serialize(new AudioRouteJournal { VirtualEndpointId = "test-only-empty-transaction" }));
    var eventName = "Local\\CXAudioRouteGuard-" + Guid.NewGuid().ToString("N");
    using var ready = new EventWaitHandle(false, EventResetMode.ManualReset, eventName);
    using var parent = Process.Start(new ProcessStartInfo(Environment.ProcessPath!, "--guard-parent")
        { UseShellExecute = false, CreateNoWindow = true })!;
    Process? guard = null;
    FileStream? lease = new(journal + ".lock", FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None);
    try
    {
        var start = new ProcessStartInfo(Path.GetFullPath(args[1])) { UseShellExecute = false, CreateNoWindow = true };
        foreach (var value in new[] { "--audio-route-guard", parent.Id.ToString(), parent.StartTime.ToUniversalTime().Ticks.ToString(), journal, eventName })
            start.ArgumentList.Add(value);
        guard = Process.Start(start)!;
        Check(ready.WaitOne(TimeSpan.FromSeconds(15)), "Real recovery guard didn't acknowledge readiness.");
        Check(!guard.HasExited && File.Exists(journal), "Guard acted while its parent was still alive.");
        parent.Kill();
        Check(parent.WaitForExit(5000), "Test parent didn't exit.");
        Thread.Sleep(350);
        Check(File.Exists(journal), "Guard ignored the transaction lease.");
        lease.Dispose();
        lease = null;
        Check(guard.WaitForExit(10000) && guard.ExitCode == 0, "Guard didn't finish after parent exit.");
        Check(!File.Exists(journal), "Guard didn't recover the empty transaction.");
        Console.WriteLine("PASS: real headless guard handshake, parent crash, lease ownership, and durable recovery.");
    }
    finally
    {
        lease?.Dispose();
        if (!parent.HasExited) parent.Kill();
        if (guard is not null) { if (!guard.HasExited) guard.Kill(); guard.Dispose(); }
    }
}

Console.WriteLine($"PASS: {passed} routing transaction checks; no real audio devices were changed.");
Console.WriteLine("Test artifacts: " + testRoot);

void Run(string name, Action<FakePlatform, AudioRouteController, string> test)
{
    var directory = Path.Combine(testRoot, passed.ToString());
    Directory.CreateDirectory(directory);
    var journal = Path.Combine(directory, "audio-route-recovery.json");
    var platform = new FakePlatform();
    platform.BeforeMutation = (id, enabled) =>
    {
        Check(File.Exists(journal), "System mutation preceded durable journal creation.");
        var saved = JsonSerializer.Deserialize<AudioRouteJournal>(File.ReadAllText(journal))!;
        if (enabled == false)
            Check(saved.DisabledEndpointIds.Contains(id) || saved.PreviouslyDisabledEndpointIds.Contains(id),
                "Disable preceded durable endpoint record.");
    };
    test(platform, new AudioRouteController(platform, journal), journal);
    passed++;
    Console.WriteLine("PASS: " + name);
}

static void Check(bool condition, string message)
{
    if (!condition) throw new InvalidOperationException(message);
}

static void ExpectFailure(Action action)
{
    try { action(); }
    catch { return; }
    throw new InvalidOperationException("Operation unexpectedly succeeded.");
}

static void AssertOriginal(FakePlatform platform, AudioRouteController route, string journal)
{
    Check(!route.IsAdvanced && route.EndpointId is null && !File.Exists(journal), "Recovery wasn't completed.");
    Check(platform.Outputs.Single(output => output.Id == "speaker").IsActive, "Speaker wasn't restored.");
    Check(platform.Outputs.Single(output => output.Id == "headphones").IsActive, "Headphones weren't restored.");
    Check(platform.Outputs.Single(output => output.Id == "disabled").State == 2, "Pre-disabled device was enabled.");
    Check(platform.Defaults[ERole.Console] == "speaker", "Console role wasn't restored.");
    Check(platform.Defaults[ERole.Multimedia] == "headphones", "Multimedia role wasn't restored.");
    Check(platform.Defaults[ERole.Communications] == "speaker", "Communications role wasn't restored.");
}

sealed class FakePlatform : IAudioRoutePlatform
{
    public List<AudioOutputEndpoint> Outputs { get; } = new()
    {
        new("speaker", "Physical speakers", 1, false),
        new("headphones", "USB headphones", 1, false),
        new("disabled", "Previously disabled output", 2, false),
        new("cable", "CABLE Input (VB-Audio Virtual Cable)", 1, true)
    };
    public Dictionary<ERole, string> Defaults { get; } = new()
    {
        [ERole.Console] = "speaker", [ERole.Multimedia] = "headphones", [ERole.Communications] = "speaker"
    };
    public Action<string, bool?>? BeforeMutation { get; set; }
    public string? FailDisableOnce { get; set; }
    public string? FailEnableOnce { get; set; }
    public bool IgnoreDisable { get; set; }
    public int Mutations { get; private set; }
    public IReadOnlyList<AudioOutputEndpoint> GetOutputs() => Outputs.ToArray();
    public string? GetDefault(ERole role) => Defaults.GetValueOrDefault(role);
    public void SetDefault(string id, ERole role)
    {
        BeforeMutation?.Invoke(id, null);
        Mutations++;
        if (!Outputs.Any(output => output.Id == id && output.IsActive)) throw new InvalidOperationException("Inactive endpoint.");
        Defaults[role] = id;
    }
    public void SetEnabled(string id, bool enabled)
    {
        BeforeMutation?.Invoke(id, enabled);
        Mutations++;
        if (!enabled && FailDisableOnce == id) { FailDisableOnce = null; throw new InvalidOperationException("Disable failed."); }
        if (enabled && FailEnableOnce == id) { FailEnableOnce = null; throw new InvalidOperationException("Restore failed."); }
        if (!enabled && IgnoreDisable) return;
        SetState(id, enabled ? 1 : 2);
    }
    public void SetState(string id, int state)
    {
        var index = Outputs.FindIndex(output => output.Id == id);
        if (index < 0) throw new InvalidOperationException("Endpoint missing.");
        Outputs[index] = Outputs[index] with { State = state };
    }
}
