using System.Runtime.InteropServices;
using CXAudioOutput;

if (IntPtr.Size != 8 || Marshal.SizeOf<PropVariant>() != 24 ||
    Marshal.OffsetOf<PropVariant>(nameof(PropVariant.PointerValue)).ToInt32() != 8)
{
    throw new InvalidOperationException("PROPVARIANT does not match the Windows x64 ABI.");
}

Console.WriteLine("PASS: PROPVARIANT size and value offset match Windows x64.");

IMMDeviceEnumerator? enumerator = null;
IMMDevice? device = null;
try
{
    enumerator = (IMMDeviceEnumerator)new WasapiInterop.MmDeviceEnumeratorComObject();
    var hr = enumerator.GetDefaultAudioEndpoint(EDataFlow.Render, ERole.Console, out device);
    if (hr != 0 || device is null)
    {
        Console.WriteLine($"SKIP: no accessible default output device (0x{hr:X8}).");
        return;
    }

    for (var i = 0; i < 100; i++)
    {
        _ = WasapiLoopbackSession.GetFriendlyName(device);
        hr = enumerator.GetDefaultAudioEndpoint(EDataFlow.Render, ERole.Console, out var current);
        if (hr != 0 || current is null) throw new InvalidOperationException($"GetDefaultAudioEndpoint: 0x{hr:X8}");
        Marshal.ReleaseComObject(current);
        hr = device.GetId(out var id);
        if (hr != 0 || string.IsNullOrWhiteSpace(id)) throw new InvalidOperationException($"Device no longer usable: 0x{hr:X8}");
    }

    Console.WriteLine("PASS: repeated device-name queries preserve the active endpoint.");
    var outputs = new WindowsAudioRoutePlatform().GetOutputs();
    if (outputs.Count == 0 || outputs.Any(output => string.IsNullOrWhiteSpace(output.Id)))
        throw new InvalidOperationException("Output endpoint collection returned invalid device IDs.");
    Console.WriteLine($"PASS: enumerated {outputs.Count} render endpoints with state and virtual driver detection (read-only).");
    if (args.Contains("--rename-virtual"))
    {
        var platform = new WindowsAudioRoutePlatform();
        var cable = outputs.First(output => output.IsVirtualSpeaker && output.IsActive);
        platform.SetDisplayName(cable.Id, "CX Audio");
        outputs = platform.GetOutputs();
        if (outputs.Single(output => output.Id == cable.Id).Name != "CX Audio")
            throw new InvalidOperationException("Windows did not persist the virtual endpoint display name.");
        Console.WriteLine("PASS: Windows virtual playback endpoint display name is CX Audio.");
    }
    if (args.Contains("--verify-protected"))
    {
        var platform = new WindowsAudioRoutePlatform();
        var virtualId = platform.GetDefault(ERole.Console);
        if (!outputs.Any(output => output.Id == virtualId && output.IsVirtualSpeaker && output.IsActive))
            throw new InvalidOperationException("The system default is not the virtual playback endpoint.");
        foreach (var role in Enum.GetValues<ERole>())
            if (platform.GetDefault(role) != virtualId) throw new InvalidOperationException("An audio role still routes elsewhere.");
        foreach (var output in outputs.Where(output => output.IsActive && output.Id != virtualId))
        {
            if (!platform.GetMuted(output.Id)) throw new InvalidOperationException("Unmuted local endpoint: " + output.Name);
            Console.WriteLine("PASS: independent process confirms muted endpoint: " + output.Name);
        }
        if (outputs.Single(output => output.Id == virtualId).Name != "CX Audio")
            throw new InvalidOperationException("Virtual playback endpoint was not renamed to CX Audio.");
        Console.WriteLine("PASS: all three Windows audio roles use the CX Audio endpoint; virtual driver identity preserved.");
    }
    if (args.Contains("--catalog"))
        foreach (var output in outputs) Console.WriteLine($"{output.State} | {output.IsVirtualSpeaker} | mute={(output.IsActive ? new WindowsAudioRoutePlatform().GetMuted(output.Id) : null)} | {output.Name} | {output.Id}");
}
finally
{
    if (device is not null) Marshal.ReleaseComObject(device);
    if (enumerator is not null) Marshal.ReleaseComObject(enumerator);
}
