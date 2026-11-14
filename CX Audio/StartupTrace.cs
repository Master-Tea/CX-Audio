using System.Diagnostics;

namespace CXAudioOutput;

internal static class StartupTrace
{
    private static readonly object Sync = new();

    [Conditional("DEBUG")]
    public static void Write(string message)
    {
        var line = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff} [thread {Environment.CurrentManagedThreadId}] {message}";
        Debug.WriteLine("[CXAudio startup] " + line);
        try
        {
            lock (Sync)
            {
                File.AppendAllText(Path.Combine(AppContext.BaseDirectory, "startup-trace.log"), line + Environment.NewLine);
            }
        }
        catch (IOException) { }
        catch (UnauthorizedAccessException) { }
    }
}
