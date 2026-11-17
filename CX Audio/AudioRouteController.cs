using System.Text.Json;

namespace CXAudioOutput;

internal sealed class AudioRouteJournal
{
    public int Version { get; set; } = 1;
    public string VirtualEndpointId { get; set; } = string.Empty;
    public Dictionary<ERole, string> OriginalDefaults { get; set; } = new();
    public List<string> DisabledEndpointIds { get; set; } = new();
    public List<string> PreviouslyDisabledEndpointIds { get; set; } = new();
    public Dictionary<string, bool> OriginalMuteStates { get; set; } = new();
}

// This class owns the transaction, independently of the UI and capture lifetime.
// Every system mutation is preceded by a durable recovery record.
internal sealed class AudioRouteController
{
    private readonly IAudioRoutePlatform _platform;
    private readonly string _journalPath;
    private AudioRouteJournal? _journal;

    public AudioRouteController(IAudioRoutePlatform platform, string journalPath)
    {
        _platform = platform;
        _journalPath = journalPath;
    }

    public bool IsAdvanced { get; private set; }
    public string? EndpointId => IsAdvanced ? _journal?.VirtualEndpointId : null;
    public bool NeedsRecovery => File.Exists(_journalPath);
    public IReadOnlyList<AudioOutputEndpoint> GetOutputs() => _platform.GetOutputs();

    public void Enable(string endpointId)
    {
        if (IsAdvanced) return;
        if (NeedsRecovery) Recover();
        var outputs = _platform.GetOutputs();
        var target = outputs.SingleOrDefault(output => output.Id == endpointId);
        if (target is not { IsActive: true, IsVirtualSpeaker: true })
            throw new InvalidOperationException("没有可用的 VB-CABLE 虚拟播放设备。请先安装驱动，必要时重启电脑。 ");

        _journal = new AudioRouteJournal
        {
            VirtualEndpointId = endpointId,
            PreviouslyDisabledEndpointIds = outputs.Where(output => (output.State & 2) != 0)
                .Select(output => output.Id).ToList()
        };
        foreach (var role in Enum.GetValues<ERole>())
        {
            if (_platform.GetDefault(role) is { } id) _journal.OriginalDefaults[role] = id;
        }
        Save();
        try
        {
            IsAdvanced = true;
            Enforce();
        }
        catch (Exception enableError)
        {
            try { Recover(); }
            catch (Exception recoveryError)
            {
                throw new AggregateException("高级模式启动失败，部分设备仍需恢复。请点击恢复普通模式。", enableError, recoveryError);
            }
            throw new InvalidOperationException("无法启用高级模式，已恢复原有输出设备。" + enableError.Message, enableError);
        }
    }

    public void Enforce()
    {
        if (!IsAdvanced || _journal is null) return;
        var outputs = _platform.GetOutputs();
        // Mute through the Windows Audio service before changing defaults. Endpoint
        // visibility is policy scoped and can report a hidden device in this process
        // while existing streams elsewhere continue playing. Never use visibility as
        // proof of silence. Previously disabled devices are not enabled or modified.
        foreach (var output in outputs.Where(output => output.IsActive && output.Id != _journal.VirtualEndpointId))
        {
            if (!_journal.OriginalMuteStates.ContainsKey(output.Id))
            {
                _journal.OriginalMuteStates.Add(output.Id, _platform.GetMuted(output.Id));
                Save();
            }
            _platform.SetMuted(output.Id, true);
        }

        outputs = _platform.GetOutputs();
        if (outputs.Any(output => output.IsActive && output.Id != _journal.VirtualEndpointId && !_platform.GetMuted(output.Id)))
            throw new InvalidOperationException("Windows 未能静音所有其他播放设备，本地静音保护尚未完成。请恢复普通模式后重试。");
        if (!outputs.Any(output => output.Id == _journal.VirtualEndpointId && output.IsActive))
            throw new InvalidOperationException("虚拟扬声器已断开或被禁用。其他输出保持静音；请恢复普通模式或重新连接虚拟设备。");

        foreach (var role in Enum.GetValues<ERole>())
        {
            if (!string.Equals(_platform.GetDefault(role), _journal.VirtualEndpointId, StringComparison.OrdinalIgnoreCase))
                _platform.SetDefault(_journal.VirtualEndpointId, role);
            if (!string.Equals(_platform.GetDefault(role), _journal.VirtualEndpointId, StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException("Windows 未能将默认输出切换到虚拟扬声器。");
        }
    }

    public void Recover()
    {
        IsAdvanced = false;
        if (!File.Exists(_journalPath)) { _journal = null; return; }
        _journal ??= JsonSerializer.Deserialize<AudioRouteJournal>(File.ReadAllText(_journalPath))
            ?? throw new InvalidOperationException("无法读取音频恢复记录，请保留记录并重试。");
        if (_journal.Version != 1 || string.IsNullOrWhiteSpace(_journal.VirtualEndpointId)
            || _journal.DisabledEndpointIds is null || _journal.PreviouslyDisabledEndpointIds is null
            || _journal.OriginalDefaults is null || _journal.OriginalMuteStates is null)
            throw new InvalidOperationException("音频恢复记录格式不受支持，请保留记录并重试。");

        var errors = new List<Exception>();
        foreach (var id in _journal.DisabledEndpointIds.ToArray())
        {
            try
            {
                // Disconnected devices may remain in the endpoint catalog. Enabling
                // them restores their state for the next plug-in as well.
                _platform.SetEnabled(id, true);
                var restored = _platform.GetOutputs().SingleOrDefault(output => output.Id == id);
                if (restored is null || (restored.State & 2) != 0)
                    throw new InvalidOperationException("设备暂不可恢复，请接回设备后重试：" + id);
                _journal.DisabledEndpointIds.Remove(id);
                Save();
            }
            catch (Exception ex) { errors.Add(ex); }
        }

        foreach (var pair in _journal.OriginalMuteStates.ToArray())
        {
            try
            {
                var output = _platform.GetOutputs().SingleOrDefault(output => output.Id == pair.Key);
                if (output is not { IsActive: true })
                    throw new InvalidOperationException("原播放设备暂不可用，请接回后恢复静音状态：" + pair.Key);
                _platform.SetMuted(pair.Key, pair.Value);
                if (_platform.GetMuted(pair.Key) != pair.Value)
                    throw new InvalidOperationException("Windows 尚未恢复原静音状态。");
                _journal.OriginalMuteStates.Remove(pair.Key);
                Save();
            }
            catch (Exception ex) { errors.Add(ex); }
        }

        foreach (var pair in _journal.OriginalDefaults.ToArray())
        {
            try
            {
                // An unplugged original default cannot be assigned yet. Keep the
                // journal so that reconnect + recovery can restore each role exactly.
                if (!_platform.GetOutputs().Any(output => output.Id == pair.Value && output.IsActive))
                    throw new InvalidOperationException("原默认输出暂不可用，请接回设备后重试。");
                _platform.SetDefault(pair.Value, pair.Key);
                if (!string.Equals(_platform.GetDefault(pair.Key), pair.Value, StringComparison.OrdinalIgnoreCase))
                    throw new InvalidOperationException("Windows 尚未恢复原默认输出。");
                _journal.OriginalDefaults.Remove(pair.Key);
                Save();
            }
            catch (Exception ex) { errors.Add(ex); }
        }

        if (errors.Count != 0)
            throw new AggregateException("部分音频设备恢复失败，恢复记录已保留。请连接原设备后点击恢复普通模式。", errors);
        // The absolute journal path is fixed by the owner; remove only this completed transaction.
        File.Delete(_journalPath);
        _journal = null;
    }

    private void Save()
    {
        Directory.CreateDirectory(Path.GetDirectoryName(_journalPath)!);
        var temporary = _journalPath + ".tmp";
        using (var stream = new FileStream(temporary, FileMode.Create, FileAccess.Write, FileShare.None))
        {
            JsonSerializer.Serialize(stream, _journal);
            stream.Flush(flushToDisk: true);
        }
        File.Move(temporary, _journalPath, overwrite: true);
    }
}
