namespace CXAudioOutput;

internal static class AudioLatency
{
    public const int PacketMilliseconds = 5;
    public const int PacketsPerSecond = 1000 / PacketMilliseconds;

    // Receiver jitter-buffer targets, not end-to-end acoustic measurements.
    public static int TargetMilliseconds(string profile, bool wired = false) => profile switch
    {
        "low" => wired ? 6 : 30,
        "stable" => 450,
        _ => 240
    };
}
