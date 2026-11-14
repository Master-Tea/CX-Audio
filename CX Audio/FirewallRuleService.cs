using System.ComponentModel;
using System.Diagnostics;

namespace CXAudioOutput;

public sealed record FirewallRuleResult(bool Allowed, bool Changed, string Message);

/// <summary>
/// Makes the selected HTTP port reachable from other devices on the LAN.
/// The rule is scoped to this port and private/public network profiles.
/// </summary>
public static class FirewallRuleService
{
    public static async Task<FirewallRuleResult> EnsureInboundPortAsync(
        int port, CancellationToken cancellationToken = default)
    {
        var ruleName = $"CX Audio TCP {port}";
        NetshResult existing;
        try
        {
            existing = await RunNetshAsync(
                new[] { "advfirewall", "firewall", "show", "rule", $"name={ruleName}" },
                useShellExecute: false, cancellationToken);
        }
        catch (Win32Exception ex) when (ex.NativeErrorCode == 1223)
        {
            return new FirewallRuleResult(false, false, "用户取消了 Windows 防火墙授权。");
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            return new FirewallRuleResult(false, false, "无法检查 Windows 防火墙：" + ex.Message);
        }

        if (existing.ExitCode == 0 && existing.StdOut.Contains(ruleName, StringComparison.OrdinalIgnoreCase))
        {
            return new FirewallRuleResult(true, false, "Windows 防火墙规则已存在。");
        }

        try
        {
            var result = await RunNetshAsync(
                new[]
                {
                    "advfirewall", "firewall", "add", "rule",
                    $"name={ruleName}",
                    "dir=in",
                    "action=allow",
                    "protocol=TCP",
                    $"localport={port}",
                    "profile=private,public"
                },
                useShellExecute: true, cancellationToken);

            if (result.ExitCode == 0)
            {
                return new FirewallRuleResult(true, true, "已允许局域网设备访问音频服务端口。");
            }

            return new FirewallRuleResult(false, false,
                "Windows 防火墙没有允许此端口：" + Summarize(result));
        }
        catch (Win32Exception ex) when (ex.NativeErrorCode == 1223)
        {
            return new FirewallRuleResult(false, false, "用户取消了 Windows 防火墙授权。");
        }
        catch (Exception ex)
        {
            return new FirewallRuleResult(false, false, "无法配置 Windows 防火墙：" + ex.Message);
        }
    }

    private static async Task<NetshResult> RunNetshAsync(
        IReadOnlyList<string> arguments, bool useShellExecute, CancellationToken cancellationToken)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = "netsh.exe",
            UseShellExecute = useShellExecute,
            CreateNoWindow = !useShellExecute,
            RedirectStandardOutput = !useShellExecute,
            RedirectStandardError = !useShellExecute,
            WorkingDirectory = AppContext.BaseDirectory
        };

        if (useShellExecute)
        {
            startInfo.Verb = "runas";
        }

        if (useShellExecute)
        {
            startInfo.Arguments = string.Join(" ", arguments.Select(QuoteArgument));
        }
        else
        {
            foreach (var argument in arguments)
            {
                startInfo.ArgumentList.Add(argument);
            }
        }

        using var process = new Process { StartInfo = startInfo };
        if (!process.Start())
        {
            return new NetshResult(-1, string.Empty, "无法启动 netsh.exe。");
        }

        if (!useShellExecute)
        {
            var stdoutTask = process.StandardOutput.ReadToEndAsync(cancellationToken);
            var stderrTask = process.StandardError.ReadToEndAsync(cancellationToken);
            await process.WaitForExitAsync(cancellationToken);
            return new NetshResult(process.ExitCode, await stdoutTask, await stderrTask);
        }

        await process.WaitForExitAsync(cancellationToken);
        return new NetshResult(process.ExitCode, string.Empty, string.Empty);
    }

    private static string QuoteArgument(string value)
    {
        return "\"" + value.Replace("\\", "\\\\", StringComparison.Ordinal)
            .Replace("\"", "\\\"", StringComparison.Ordinal) + "\"";
    }

    private static string Summarize(NetshResult result)
    {
        var text = string.Join(" ", new[] { result.StdErr, result.StdOut }
            .Where(value => !string.IsNullOrWhiteSpace(value))).Trim();
        return text.Length == 0 ? $"退出码 {result.ExitCode}" : text;
    }

    private readonly record struct NetshResult(int ExitCode, string StdOut, string StdErr);
}
