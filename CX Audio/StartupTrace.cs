using System.Diagnostics;

namespace CXAudioOutput;

internal static class StartupTrace
{
    private static readonly object Sync = new();

    public static void Write(string message)
    {
        var line = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff} [thread {Environment.CurrentManagedThreadId}] {message}";
        Debug.WriteLine("[CXAudio startup] " + line);
        try
        {
            lock (Sync)
            {
                var directory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "CXAudioOutput", "logs");
                Directory.CreateDirectory(directory);
                File.AppendAllText(Path.Combine(directory, "startup-trace.log"), line + Environment.NewLine);
#if DEBUG
                try { File.AppendAllText(Path.Combine(AppContext.BaseDirectory, "startup-trace.log"), line + Environment.NewLine); }
                catch (IOException) { }
                catch (UnauthorizedAccessException) { }
#endif
            }
        }
        catch (IOException) { }
        catch (UnauthorizedAccessException) { }
    }
}
