namespace CXAudioOutput;

// Signature verification is exercised using real files. Driver installation
// itself must never run as a side effect of these non-destructive checks.
internal static class AppPaths
{
    public static string Root => throw new NotSupportedException("Driver installation is not allowed in routing checks.");
}
