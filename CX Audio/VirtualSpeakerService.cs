using System.Collections.Concurrent;
using System.Diagnostics;

namespace CXAudioOutput;

public sealed record VirtualSpeakerSnapshot(bool Advanced, bool Protected, bool Installed,
    bool RecoveryPending, bool Busy, string? EndpointId, string? DeviceName, string? Error);

public sealed class VirtualSpeakerService : IHostedService, IDisposable
{
    private readonly LaunchOptions _options;
    private readonly ILogger<VirtualSpeakerService> _logger;
    private sealed record Command(Action Action, TaskCompletionSource Completion);
    private readonly BlockingCollection<Command> _commands = new();
    private readonly TaskCompletionSource _ready = new(TaskCreationOptions.RunContinuationsAsynchronously);
    private readonly TaskCompletionSource _stopped = new(TaskCreationOptions.RunContinuationsAsynchronously);
    private volatile bool _stopping;
    private VirtualSpeakerSnapshot _snapshot = new(false, false, false, false, false, null, null, null);
    private AudioRouteController? _controller;
    private Thread? _thread;
    private Process? _guard;
    private FileStream? _lease;
    private int _revision;

    public VirtualSpeakerService(LaunchOptions options, ILogger<VirtualSpeakerService> logger)
    {
        _options = options;
        _logger = logger;
    }

    public VirtualSpeakerSnapshot Snapshot => Volatile.Read(ref _snapshot);
    public int Revision => Volatile.Read(ref _revision);
    public event Action? RouteChanged;
    internal static string JournalPath => Path.Combine(AppPaths.Root, "audio-route-recovery.json");

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _thread = new Thread(Worker) { IsBackground = true, Name = "CX Audio output routing" };
        _thread.SetApartmentState(ApartmentState.MTA);
        _thread.Start();
        return _ready.Task.WaitAsync(cancellationToken);
    }

    public Task EnableAsync() => Dispatch(() =>
    {
        if (_options.Demo) throw new InvalidOperationException("演示信号模式不能启用系统音频隔离，请正常启动应用。");
        var target = _controller!.GetOutputs().FirstOrDefault(output => output.IsVirtualSpeaker && output.IsActive)
            ?? throw new InvalidOperationException("请先安装并启用 VB-CABLE 虚拟音频驱动，再刷新设备。");
        EnsureGuard();
        ChangeRoute(() => _controller.Enable(target.Id));
    });

    public Task DisableAsync() => Dispatch(() => ChangeRoute(() => _controller!.Recover()));
    public Task RefreshAsync() => Dispatch(Refresh);

    private Task Dispatch(Action action)
    {
        if (_thread is null || _stopping || _stopped.Task.IsCompleted)
            return Task.FromException(new InvalidOperationException("请先启动音频服务。"));
        var completion = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        try
        {
            if (!_commands.TryAdd(new Command(action, completion)))
                completion.SetException(new InvalidOperationException("音频路由服务已停止。"));
        }
        catch (InvalidOperationException)
        {
            completion.TrySetException(new InvalidOperationException("音频路由服务已停止。"));
        }
        return completion.Task;
    }

    private void Worker()
    {
        try
        {
            Directory.CreateDirectory(AppPaths.Root);
            // An exclusive lease prevents another CX Audio instance (or a recovery
            // guard) from restoring devices while this instance is using them.
            _lease = new FileStream(JournalPath + ".lock", FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None);
            _controller = new AudioRouteController(new WindowsAudioRoutePlatform(), JournalPath);
            try { _controller.Recover(); Refresh(); }
            catch (Exception ex) { Publish(ex.Message); }
            _ready.TrySetResult();
            var nextRefresh = Stopwatch.GetTimestamp();
            while (!_stopping)
            {
                if (_commands.TryTake(out var command, 100))
                {
                    try { command.Action(); command.Completion.SetResult(); }
                    catch (Exception ex) { Publish(ex.Message); command.Completion.SetException(ex); }
                }
                if (_stopping) break;
                if (_controller.IsAdvanced || Stopwatch.GetTimestamp() >= nextRefresh)
                {
                    Refresh();
                    nextRefresh = Stopwatch.GetTimestamp() + Stopwatch.Frequency * 2;
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Audio route worker failed");
            Publish(ex.Message);
            // Failure to own the lease must not stop the ordinary audio server.
            _ready.TrySetResult();
        }
        finally
        {
            if (_controller is not null)
            {
                try { _controller.Recover(); }
                catch (Exception ex) { _logger.LogError(ex, "Audio device recovery is pending"); }
            }
            _stopping = true;
            _commands.CompleteAdding();
            _lease?.Dispose();
            _lease = null;
            _stopped.TrySetResult();
            while (_commands.TryTake(out var remaining))
                remaining.Completion.TrySetException(new InvalidOperationException("音频路由服务已停止。"));
        }
    }

    private void ChangeRoute(Action change)
    {
        Volatile.Write(ref _snapshot, Snapshot with { Busy = true, Protected = false });
        NotifyCapture();
        try { change(); }
        finally { Refresh(); NotifyCapture(); }
    }

    private void Refresh()
    {
        try
        {
            if (_controller!.IsAdvanced)
            {
                if (_guard is null || _guard.HasExited)
                    throw new InvalidOperationException("音频恢复守护进程已停止。请恢复普通模式后重试。");
                _controller.Enforce();
            }
            Publish(null);
        }
        catch (Exception ex)
        {
            var previous = Snapshot;
            Publish(ex.Message);
            if (previous.Protected) NotifyCapture();
        }
    }

    private void Publish(string? error)
    {
        AudioOutputEndpoint? device = null;
        try
        {
            device = _controller?.GetOutputs().FirstOrDefault(output => output.IsVirtualSpeaker && output.IsActive);
        }
        catch (Exception ex) { error ??= ex.Message; }
        var advanced = _controller?.IsAdvanced == true;
        Volatile.Write(ref _snapshot, new VirtualSpeakerSnapshot(advanced, advanced && error is null,
            device is not null, File.Exists(JournalPath), false, _controller?.EndpointId, device?.Name, error));
    }

    private void NotifyCapture()
    {
        Interlocked.Increment(ref _revision);
        RouteChanged?.Invoke();
    }

    private void EnsureGuard()
    {
        if (_guard is not null && !_guard.HasExited) return;
        var eventName = "Local\\CXAudioRouteGuard-" + Guid.NewGuid().ToString("N");
        using var ready = new EventWaitHandle(false, EventResetMode.ManualReset, eventName);
        using var parent = Process.GetCurrentProcess();
        var info = new ProcessStartInfo(Environment.ProcessPath!) { UseShellExecute = false, CreateNoWindow = true };
        info.ArgumentList.Add("--audio-route-guard");
        info.ArgumentList.Add(parent.Id.ToString(System.Globalization.CultureInfo.InvariantCulture));
        info.ArgumentList.Add(parent.StartTime.ToUniversalTime().Ticks.ToString(System.Globalization.CultureInfo.InvariantCulture));
        info.ArgumentList.Add(JournalPath);
        info.ArgumentList.Add(eventName);
        _guard?.Dispose();
        _guard = Process.Start(info) ?? throw new InvalidOperationException("无法启动音频恢复守护进程。");
        if (!ready.WaitOne(TimeSpan.FromSeconds(15)) || _guard.HasExited)
            throw new InvalidOperationException("音频恢复守护进程未就绪，已取消高级模式。");
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        _stopping = true;
        if (_thread is not null) await _stopped.Task.WaitAsync(cancellationToken);
    }

    public void Dispose()
    {
        _stopping = true;
        _guard?.Dispose();
        // The worker owns the lease and COM lifetime. Do not dispose its queue while
        // it is finishing a durable restore after a host shutdown timeout.
    }
}
