using System.Diagnostics;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;

namespace CXAudioOutput;

internal static class Program
{
    [STAThread]
    private static void Main(string[] args)
    {
        // Recovery runs before WinUI initialization and never creates a window,
        // HTTP listener, settings store, or another recovery guard.
        if (args.Length > 0 && args[0] == "--audio-route-guard")
        {
            RunRouteGuard(args);
            return;
        }

        AppDomain.CurrentDomain.UnhandledException += (_, error) =>
            StartupTrace.Write("Unhandled process exception: " + error.ExceptionObject);
        try
        {
            StartupTrace.Write("Main: initializing WinUI");
            WinRT.ComWrappersSupport.InitializeComWrappers();
            Application.Start(parameters =>
            {
                SynchronizationContext.SetSynchronizationContext(
                    new DispatcherQueueSynchronizationContext(DispatcherQueue.GetForCurrentThread()));
                _ = new App();
            });
        }
        catch (Exception error)
        {
            StartupTrace.Write("Startup failed: " + error);
            throw;
        }
    }

    private static void RunRouteGuard(string[] args)
    {
        if (args.Length != 5 || !int.TryParse(args[1], out var pid)
            || !long.TryParse(args[2], out var startTicks) || !Path.IsPathFullyQualified(args[3])
            || Path.GetFileName(args[3]) != "audio-route-recovery.json"
            || !args[4].StartsWith("Local\\CXAudioRouteGuard-", StringComparison.Ordinal)) return;
        var journal = Path.GetFullPath(args[3]);
        try
        {
            using var parent = Process.GetProcessById(pid);
            if (parent.StartTime.ToUniversalTime().Ticks != startTicks) return;
            using (var ready = EventWaitHandle.OpenExisting(args[4])) ready.Set();
            parent.WaitForExit();
        }
        catch (ArgumentException) { /* The owner has already exited. */ }
        catch (Exception ex)
        {
            LogGuardError(journal, ex);
            return;
        }

        // A newly started app can acquire the lease first and perform recovery.
        // Never interfere with that instance's transaction.
        for (var attempt = 0; attempt < 20; attempt++)
        {
            try
            {
                using var lease = new FileStream(journal + ".lock", FileMode.OpenOrCreate,
                    FileAccess.ReadWrite, FileShare.None);
                new AudioRouteController(new WindowsAudioRoutePlatform(), journal).Recover();
                return;
            }
            catch (IOException) { Thread.Sleep(250); }
            catch (Exception ex) { LogGuardError(journal, ex); return; }
        }
    }

    private static void LogGuardError(string journal, Exception error)
    {
        try { File.AppendAllText(journal + ".log", DateTimeOffset.Now + " " + error + Environment.NewLine); }
        catch { /* Recovery journal itself remains available for the next startup. */ }
    }
}
