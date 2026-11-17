using System.ComponentModel;
using System.Diagnostics;
using System.IO.Compression;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

namespace CXAudioOutput;

internal static class VirtualSpeakerInstaller
{
    internal const string Website = "https://vb-audio.com/Cable/";
    private const string DownloadUrl = "https://download.vb-audio.com/Download_CABLE/VBCABLE_Driver_Pack43.zip";

    public static async Task InstallAsync(IProgress<string> progress, CancellationToken cancellationToken)
    {
        progress.Report("正在从 VB-Audio 官网下载驱动…");
        var directory = Path.Combine(AppPaths.Root, "driver-install", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        var archive = Path.Combine(directory, "VBCABLE_Driver_Pack43.zip");
        using var client = new HttpClient { Timeout = TimeSpan.FromSeconds(90) };
        using (var response = await client.GetAsync(DownloadUrl, HttpCompletionOption.ResponseHeadersRead, cancellationToken))
        {
            response.EnsureSuccessStatusCode();
            if (response.Content.Headers.ContentLength is > 20_000_000)
                throw new InvalidOperationException("官方驱动包大小异常，安装已取消。");
            await using var source = await response.Content.ReadAsStreamAsync(cancellationToken);
            await using var destination = File.Create(archive);
            var buffer = new byte[65536];
            long total = 0;
            int read;
            while ((read = await source.ReadAsync(buffer, cancellationToken)) > 0)
            {
                total += read;
                if (total > 20_000_000) throw new InvalidOperationException("官方驱动包大小异常，安装已取消。");
                await destination.WriteAsync(buffer.AsMemory(0, read), cancellationToken);
            }
        }

        cancellationToken.ThrowIfCancellationRequested();
        await using (var package = File.OpenRead(archive))
        {
            var hash = Convert.ToHexString(await SHA256.HashDataAsync(package, cancellationToken));
            if (hash != "66FD0A4D9F4896FF41632B7E3D53892C085C4561F53E8AE8D0F0BC10EEDD1CDD")
                throw new InvalidOperationException("官方驱动包的 SHA256 校验不匹配，安装已取消。请从官网下载受支持的版本，或更新 CX Audio。");
        }
        using (var zip = ZipFile.OpenRead(archive))
        {
            if (zip.Entries.Sum(entry => entry.Length) > 80_000_000)
                throw new InvalidOperationException("驱动解压大小异常，安装已取消。");
            zip.ExtractToDirectory(directory);
        }
        var setup = Path.Combine(directory, "VBCABLE_Setup_x64.exe");
        progress.Report("正在验证驱动安装程序的数字签名…");
        await Task.Run(() => VerifyInstaller(setup), cancellationToken);
        cancellationToken.ThrowIfCancellationRequested();

        // The vendor installer can change Windows defaults. Preserve each role
        // independently so installing a driver doesn't silently change normal mode.
        var platform = new WindowsAudioRoutePlatform();
        var defaults = await Task.Run(() => Enum.GetValues<ERole>()
            .Select(role => (Role: role, Id: platform.GetDefault(role))).ToArray(), cancellationToken);
        try
        {
            progress.Report("请在 Windows 授权窗口中允许安装，然后在 VB-CABLE 安装器中点击 Install Driver。完成后可能需要重启电脑。");
            using var process = Process.Start(new ProcessStartInfo(setup)
            {
                UseShellExecute = true, Verb = "runas", WorkingDirectory = directory
            }) ?? throw new InvalidOperationException("未能打开虚拟音频驱动安装程序。");
            // Once the vendor installer is running, let it finish. Closing the app
            // must not interrupt a driver installation or skip restoring defaults.
            await process.WaitForExitAsync();
            if (process.ExitCode is not (0 or 3010))
                throw new InvalidOperationException($"驱动安装程序已退出（{process.ExitCode}）。请确认已完成安装，必要时重启电脑。");
        }
        catch (Win32Exception ex) when (ex.NativeErrorCode == 1223)
        {
            throw new InvalidOperationException("管理员授权已取消，虚拟扬声器尚未安装。", ex);
        }
        finally
        {
            await Task.Run(() =>
            {
                foreach (var (role, id) in defaults)
                    if (id is not null) platform.SetDefault(id, role);
            });
        }
        progress.Report("安装程序已结束。正在检查虚拟扬声器；如未出现，请重启电脑后再启用高级模式。");
    }

    internal static void VerifyInstaller(string path)
    {
        if (!File.Exists(path)) throw new InvalidOperationException("驱动包中缺少 Windows x64 安装程序。");
        var file = new WinTrustFile
        {
            Size = (uint)Marshal.SizeOf<WinTrustFile>(),
            Path = path
        };
        var filePointer = Marshal.AllocHGlobal(Marshal.SizeOf<WinTrustFile>());
        try
        {
            Marshal.StructureToPtr(file, filePointer, false);
            var data = new WinTrustData
            {
                Size = (uint)Marshal.SizeOf<WinTrustData>(),
                UiChoice = 2, // WTD_UI_NONE
                RevocationChecks = 0,
                UnionChoice = 1, // WTD_CHOICE_FILE
                File = filePointer,
                StateAction = 1, // WTD_STATEACTION_VERIFY
                ProviderFlags = 0
            };
            var action = new Guid("00AAC56B-CD44-11D0-8CC2-00C04FC295EE");
            try
            {
                var result = WinVerifyTrust(new IntPtr(-1), ref action, ref data);
                if (result != 0) throw new InvalidOperationException($"驱动安装程序的数字签名验证失败（0x{result:X8}），安装已取消。");
                using var certificate = new X509Certificate2(X509Certificate.CreateFromSignedFile(path));
                var signer = certificate.GetNameInfo(X509NameType.SimpleName, false);
                if (!signer.Equals("VB-Audio Software", StringComparison.OrdinalIgnoreCase)
                    && !signer.Equals("Vincent Burel", StringComparison.OrdinalIgnoreCase))
                    throw new InvalidOperationException("驱动安装程序并非由 VB-Audio / Vincent Burel 签名，安装已取消。");
            }
            finally
            {
                data.StateAction = 2; // WTD_STATEACTION_CLOSE
                WinVerifyTrust(new IntPtr(-1), ref action, ref data);
            }
        }
        finally
        {
            Marshal.DestroyStructure<WinTrustFile>(filePointer);
            Marshal.FreeHGlobal(filePointer);
        }
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    private struct WinTrustFile
    {
        public uint Size;
        [MarshalAs(UnmanagedType.LPWStr)] public string Path;
        public IntPtr FileHandle;
        public IntPtr KnownSubject;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    private struct WinTrustData
    {
        public uint Size;
        public IntPtr PolicyCallbackData;
        public IntPtr SipClientData;
        public uint UiChoice;
        public uint RevocationChecks;
        public uint UnionChoice;
        public IntPtr File;
        public uint StateAction;
        public IntPtr StateData;
        public IntPtr UrlReference;
        public uint ProviderFlags;
        public uint UiContext;
        public IntPtr SignatureSettings;
    }

    [DllImport("wintrust.dll", ExactSpelling = true, PreserveSig = true)]
    private static extern int WinVerifyTrust(IntPtr window, ref Guid action, ref WinTrustData data);
}
