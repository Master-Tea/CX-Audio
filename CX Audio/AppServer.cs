using System.Net;
using Microsoft.Extensions.Logging;

namespace CXAudioOutput;

public sealed class AppServer : IAsyncDisposable
{
    private readonly WebApplication _app;
    private readonly object _sync = new();
    private Task? _startTask;
    private bool _started;
    private bool _disposed;

    public AppServer(LaunchOptions options)
    {
        Options = options;

        var builder = WebApplication.CreateBuilder(new WebApplicationOptions
        {
            Args = Array.Empty<string>(),
            ContentRootPath = AppContext.BaseDirectory
        });

        builder.WebHost.ConfigureKestrel(kestrel =>
        {
            kestrel.AddServerHeader = false;
            kestrel.Limits.MaxRequestBodySize = 64 * 1024;
            kestrel.ListenAnyIP(options.Port);
        });

        builder.Services.Configure<Microsoft.AspNetCore.Server.Kestrel.Transport.Sockets.SocketTransportOptions>(
            transport => transport.NoDelay = true);

        builder.Logging.ClearProviders();
        builder.Logging.SetMinimumLevel(LogLevel.Information);
        builder.Logging.AddProvider(new FileLoggerProvider(AppPaths.LogFile));
        builder.Logging.AddFilter("Microsoft.AspNetCore", LogLevel.Warning);

        builder.Services.AddSingleton(options);
        builder.Services.AddSingleton<AppSettingsStore>();
        builder.Services.AddSingleton<AppState>();
        builder.Services.AddSingleton<NetworkSelectionService>();
        builder.Services.AddSingleton<AndroidWiredService>();
        builder.Services.AddSingleton<AudioHub>();
        builder.Services.AddSingleton<AccessGuard>();
        builder.Services.AddSingleton<VirtualSpeakerService>();
        builder.Services.AddHostedService(provider => provider.GetRequiredService<VirtualSpeakerService>());
        builder.Services.AddSingleton<AudioCaptureService>();
        builder.Services.AddHostedService(provider => provider.GetRequiredService<AudioCaptureService>());
        builder.Services.AddHostedService<StateBroadcaster>();
        builder.Services.AddHostedService<HardwareMonitorService>();

        _app = builder.Build();

        if (!Directory.Exists(Path.Combine(AppContext.BaseDirectory, "wwwroot")))
        {
            throw new InvalidOperationException(
                "找不到 wwwroot 文件夹。请从完整发布目录运行 CX Audio。");
        }

        _app.Use(async (context, next) =>
        {
            context.Response.Headers["X-Content-Type-Options"] = "nosniff";
            context.Response.Headers["Referrer-Policy"] = "no-referrer";
            context.Response.Headers["X-Frame-Options"] = "DENY";
            context.Response.Headers["Cross-Origin-Resource-Policy"] = "same-origin";
            context.Response.Headers["Permissions-Policy"] = "camera=(), microphone=(), geolocation=()";
            context.Response.Headers["Content-Security-Policy"] =
                "default-src 'self'; img-src 'self' data:; media-src 'self'; " +
                "style-src 'self' 'unsafe-inline'; " +
                "script-src 'self'; connect-src 'self' ws: wss:; font-src 'self'; " +
                "base-uri 'none'; form-action 'self'; frame-ancestors 'none'";
            await next(context);
        });

        _app.Use(async (context, next) =>
        {
            if (!NetTools.IsLocal(context))
            {
                var network = context.RequestServices.GetRequiredService<NetworkSelectionService>();
                var localAddress = context.Connection.LocalIpAddress;
                var localAddressAllowed = network.IsAllowedLocalAddress(localAddress);
                if (!localAddressAllowed
                    && (localAddress is null
                        || IPAddress.Any.Equals(localAddress)
                        || IPAddress.IPv6Any.Equals(localAddress)))
                {
                    // Some Kestrel transports report the wildcard listener
                    // instead of the concrete destination address. In that
                    // case, use the IP host the client requested as a fallback.
                    localAddressAllowed = network.IsAllowedHostAddress(context.Request.Host.Host);
                }

                if (!localAddressAllowed)
                {
                    context.Response.StatusCode = StatusCodes.Status403Forbidden;
                    return;
                }
            }

            await next(context);
        });

        _app.Use(async (context, next) =>
        {
            if (!NetTools.IsLocal(context)
                && (context.Request.Path == "/studio" || context.Request.Path == "/studio.html"))
            {
                context.Response.StatusCode = StatusCodes.Status404NotFound;
                return;
            }

            await next(context);
        });

        _app.UseWebSockets(new WebSocketOptions
        {
            KeepAliveInterval = TimeSpan.FromSeconds(20)
        });

        _app.UseStaticFiles(new StaticFileOptions
        {
            OnPrepareResponse = responseContext =>
            {
                var path = responseContext.Context.Request.Path;
                if (path.StartsWithSegments("/assets") || path.Value?.EndsWith(".svg") == true)
                {
                    responseContext.Context.Response.Headers.CacheControl = "public, max-age=3600";
                }
                else if (path.Value?.EndsWith(".html", StringComparison.OrdinalIgnoreCase) == true)
                {
                    responseContext.Context.Response.Headers.CacheControl = "no-store";
                }
            }
        });

        _app.MapCxEndpoints(options);

        State = _app.Services.GetRequiredService<AppState>();
        Hub = _app.Services.GetRequiredService<AudioHub>();
        Network = _app.Services.GetRequiredService<NetworkSelectionService>();
        Android = _app.Services.GetRequiredService<AndroidWiredService>();
        Capture = _app.Services.GetRequiredService<AudioCaptureService>();
        Settings = _app.Services.GetRequiredService<AppSettingsStore>();
        VirtualSpeaker = _app.Services.GetRequiredService<VirtualSpeakerService>();
    }

    public LaunchOptions Options { get; }
    public AppState State { get; }
    public AudioHub Hub { get; }
    public NetworkSelectionService Network { get; }
    public AndroidWiredService Android { get; }
    public AudioCaptureService Capture { get; }
    public AppSettingsStore Settings { get; }
    public VirtualSpeakerService VirtualSpeaker { get; }
    public string LocalStudioUrl => $"http://127.0.0.1:{Options.Port}/studio";
    public string LogFilePath => AppPaths.LogFile;

    public Task StartAsync(CancellationToken cancellationToken = default)
    {
        lock (_sync)
        {
            if (_started || _disposed) return Task.CompletedTask;
            return _startTask ??= StartCoreAsync(cancellationToken);
        }
    }

    private async Task StartCoreAsync(CancellationToken cancellationToken)
    {
        try
        {
            await _app.StartAsync(cancellationToken);

            lock (_sync)
            {
                _started = true;
            }

            Network.Refresh();
            await Hub.BroadcastStateAsync();
        }
        catch
        {
            lock (_sync) _started = false;
            // Kestrel can fail to bind after hosted services (including audio
            // capture) have already started. Stop them before surfacing the
            // error, otherwise the UI would look half-alive with no server.
            try
            {
                await _app.StopAsync(TimeSpan.FromSeconds(2));
            }
            catch
            {
                // Best effort.
            }

            throw;
        }
        finally
        {
            lock (_sync)
            {
                _startTask = null;
            }
        }
    }

    public async Task StopAsync(CancellationToken cancellationToken = default)
    {
        Task? startTask;
        lock (_sync) startTask = _startTask;
        if (startTask is not null)
        {
            try
            {
                await startTask.WaitAsync(cancellationToken);
            }
            catch
            {
                // Start failure or cancellation is handled by the caller that started it.
            }
        }

        bool started;
        lock (_sync)
        {
            started = _started;
            _started = false;
        }

        if (!started) return;
        try
        {
            await _app.StopAsync(TimeSpan.FromSeconds(3));
        }
        catch
        {
            // The process is closing; don't let a slow Kestrel shutdown keep the window alive.
        }
    }

    public async ValueTask DisposeAsync()
    {
        Task? startTask;
        lock (_sync)
        {
            if (_disposed) return;
            _disposed = true;
            startTask = _startTask;
        }

        if (startTask is not null)
        {
            try
            {
                await startTask;
            }
            catch
            {
                // Start failure is already reported by the UI.
            }
        }

        await StopAsync();
        try
        {
            await _app.DisposeAsync();
        }
        catch
        {
            // ignored
        }
    }
}
