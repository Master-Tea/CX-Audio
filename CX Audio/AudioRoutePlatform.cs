using System.Runtime.InteropServices;

namespace CXAudioOutput;

internal sealed record AudioOutputEndpoint(string Id, string Name, int State, bool IsVirtualSpeaker)
{
    public bool IsActive => (State & 1) != 0;
}

internal interface IAudioRoutePlatform
{
    IReadOnlyList<AudioOutputEndpoint> GetOutputs();
    string? GetDefault(ERole role);
    void SetDefault(string id, ERole role);
    void SetEnabled(string id, bool enabled);
    bool GetMuted(string id);
    void SetMuted(string id, bool muted);
    void SetDisplayName(string id, string name);
}

// All COM objects are created and released on the route worker (or recovery process).
internal sealed class WindowsAudioRoutePlatform : IAudioRoutePlatform
{
    public IReadOnlyList<AudioOutputEndpoint> GetOutputs()
    {
        var enumerator = (IMMDeviceEnumerator)new WasapiInterop.MmDeviceEnumeratorComObject();
        IMMDeviceCollection? collection = null;
        IntPtr pointer = IntPtr.Zero;
        try
        {
            Marshal.ThrowExceptionForHR(enumerator.EnumAudioEndpoints(EDataFlow.Render, 0xF, out pointer));
            collection = (IMMDeviceCollection)Marshal.GetObjectForIUnknown(pointer);
            Marshal.ThrowExceptionForHR(collection.GetCount(out var count));
            var endpoints = new List<AudioOutputEndpoint>();
            for (uint i = 0; i < count; i++)
            {
                Marshal.ThrowExceptionForHR(collection.Item(i, out var device));
                try
                {
                    Marshal.ThrowExceptionForHR(device.GetId(out var id));
                    Marshal.ThrowExceptionForHR(device.GetState(out var state));
                    var name = WasapiLoopbackSession.GetFriendlyName(device);
                    var description = ReadString(device, new PropertyKey(
                        new Guid("B3F8FA53-0004-438E-9003-51A46E139BFC"), 6));
                    // Match the driver interface as well as the endpoint name. A random
                    // user-selected physical endpoint must never become the isolated sink.
                    var isCable = description.Contains("VB-Audio Virtual Cable", StringComparison.OrdinalIgnoreCase)
                        || name.Contains("VB-Audio Virtual Cable", StringComparison.OrdinalIgnoreCase);
                    var label = ReadString(device, new PropertyKey(
                        new Guid("A45C254E-DF1C-4EFD-8020-67D146A850E0"), 2));
                    if (isCable && label == "CX Audio") name = label;
                    endpoints.Add(new AudioOutputEndpoint(id, name, state, isCable));
                }
                finally { Marshal.ReleaseComObject(device); }
            }
            return endpoints;
        }
        finally
        {
            if (collection is not null) Marshal.ReleaseComObject(collection);
            if (pointer != IntPtr.Zero) Marshal.Release(pointer);
            Marshal.ReleaseComObject(enumerator);
        }
    }

    public string? GetDefault(ERole role)
    {
        var enumerator = (IMMDeviceEnumerator)new WasapiInterop.MmDeviceEnumeratorComObject();
        try
        {
            var hr = enumerator.GetDefaultAudioEndpoint(EDataFlow.Render, role, out var device);
            if (hr == unchecked((int)0x80070490)) return null;
            Marshal.ThrowExceptionForHR(hr);
            try
            {
                Marshal.ThrowExceptionForHR(device.GetId(out var id));
                return id;
            }
            finally { Marshal.ReleaseComObject(device); }
        }
        finally { Marshal.ReleaseComObject(enumerator); }
    }

    public void SetDefault(string id, ERole role) => WithPolicy(policy => policy.SetDefaultEndpoint(id, role));
    public void SetEnabled(string id, bool enabled) => WithPolicy(policy => policy.SetEndpointVisibility(id, enabled));

    public bool GetMuted(string id) => WithVolume(id, volume =>
    {
        Marshal.ThrowExceptionForHR(volume.GetMute(out var muted));
        return muted;
    });

    public void SetMuted(string id, bool muted) => WithVolume(id, volume =>
    {
        var context = Guid.Empty;
        Marshal.ThrowExceptionForHR(volume.SetMute(muted, ref context));
        Marshal.ThrowExceptionForHR(volume.GetMute(out var actual));
        if (actual != muted) throw new InvalidOperationException("Windows 未能应用本地播放设备的静音状态。");
        return true;
    });

    public void SetDisplayName(string id, string name)
    {
        // DeviceDesc is the editable endpoint label shown in Sound settings.
        // DeviceFriendlyName (pid 14) is a derived, read-only full name.
        var key = new PropertyKey(new Guid("A45C254E-DF1C-4EFD-8020-67D146A850E0"), 2);
        var value = new PropVariant { ValueType = 31, PointerValue = Marshal.StringToCoTaskMemUni(name) };
        try
        {
            var policy = (IAudioPolicyConfig)new AudioPolicyConfigComObject();
            try { Marshal.ThrowExceptionForHR(policy.SetPropertyValue(id, 0, ref key, ref value)); }
            finally { Marshal.ReleaseComObject(policy); }
        }
        finally { WasapiInterop.PropVariantClear(ref value); }
    }

    private static T WithVolume<T>(string id, Func<IAudioEndpointVolume, T> action)
    {
        var enumerator = (IMMDeviceEnumerator)new WasapiInterop.MmDeviceEnumeratorComObject();
        IMMDevice? device = null;
        object? activated = null;
        try
        {
            Marshal.ThrowExceptionForHR(enumerator.GetDevice(id, out device));
            var iid = typeof(IAudioEndpointVolume).GUID;
            Marshal.ThrowExceptionForHR(device.Activate(ref iid, 23, IntPtr.Zero, out activated));
            return action((IAudioEndpointVolume)activated);
        }
        finally
        {
            if (activated is not null) Marshal.ReleaseComObject(activated);
            if (device is not null) Marshal.ReleaseComObject(device);
            Marshal.ReleaseComObject(enumerator);
        }
    }

    private static void WithPolicy(Func<IAudioPolicyConfig, int> action)
    {
        var policy = (IAudioPolicyConfig)new AudioPolicyConfigComObject();
        try { Marshal.ThrowExceptionForHR(action(policy)); }
        finally { Marshal.ReleaseComObject(policy); }
    }

    private static string ReadString(IMMDevice device, PropertyKey key)
    {
        if (device.OpenPropertyStore(WasapiInterop.StgmRead, out var store) != 0) return string.Empty;
        try
        {
            var hr = store.GetValue(ref key, out var value);
            try
            {
                return hr == 0 && value.ValueType == 31 && value.PointerValue != IntPtr.Zero
                    ? Marshal.PtrToStringUni(value.PointerValue) ?? string.Empty : string.Empty;
            }
            finally { WasapiInterop.PropVariantClear(ref value); }
        }
        finally { Marshal.ReleaseComObject(store); }
    }
}

[ComImport, Guid("0BD7A1BE-7A1A-44DB-8397-CC5392387B5E"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
internal interface IMMDeviceCollection
{
    [PreserveSig] int GetCount(out uint count);
    [PreserveSig] int Item(uint index, out IMMDevice device);
}

[ComImport, Guid("870AF99C-171D-4F9E-AF0D-E63DF40C2BC9")]
internal class AudioPolicyConfigComObject { }

// Windows desktop policy interface. Keep every preceding vtable slot, even for
// methods we don't call. Any unsupported policy call fails closed in the controller.
[ComImport, Guid("F8679F50-850A-41CF-9C72-430F290290C8"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
internal interface IAudioPolicyConfig
{
    [PreserveSig] int GetMixFormat([MarshalAs(UnmanagedType.LPWStr)] string id, IntPtr format);
    [PreserveSig] int GetDeviceFormat([MarshalAs(UnmanagedType.LPWStr)] string id, int @default, IntPtr format);
    [PreserveSig] int ResetDeviceFormat([MarshalAs(UnmanagedType.LPWStr)] string id);
    [PreserveSig] int SetDeviceFormat([MarshalAs(UnmanagedType.LPWStr)] string id, IntPtr endpointFormat, IntPtr mixFormat);
    [PreserveSig] int GetProcessingPeriod([MarshalAs(UnmanagedType.LPWStr)] string id, int @default, IntPtr period, IntPtr minimum);
    [PreserveSig] int SetProcessingPeriod([MarshalAs(UnmanagedType.LPWStr)] string id, IntPtr period);
    [PreserveSig] int GetShareMode([MarshalAs(UnmanagedType.LPWStr)] string id, IntPtr mode);
    [PreserveSig] int SetShareMode([MarshalAs(UnmanagedType.LPWStr)] string id, IntPtr mode);
    [PreserveSig] int GetPropertyValue([MarshalAs(UnmanagedType.LPWStr)] string id, int effects, ref PropertyKey key, out PropVariant value);
    [PreserveSig] int SetPropertyValue([MarshalAs(UnmanagedType.LPWStr)] string id, int effects, ref PropertyKey key, ref PropVariant value);
    [PreserveSig] int SetDefaultEndpoint([MarshalAs(UnmanagedType.LPWStr)] string id, ERole role);
    [PreserveSig] int SetEndpointVisibility([MarshalAs(UnmanagedType.LPWStr)] string id, [MarshalAs(UnmanagedType.Bool)] bool visible);
}
