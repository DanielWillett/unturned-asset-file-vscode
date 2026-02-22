using DanielWillett.UnturnedDataFileLspServer.Data.AssetEnvironment;
using DanielWillett.UnturnedDataFileLspServer.Data.CodeFixes;
using DanielWillett.UnturnedDataFileLspServer.Data.Parsing;
using DanielWillett.UnturnedDataFileLspServer.Data.Spec;
using DanielWillett.UnturnedDataFileLspServer.Diagnostics;
using DanielWillett.UnturnedDataFileLspServer.Files;
using DanielWillett.UnturnedDataFileLspServer.Handlers;
using DanielWillett.UnturnedDataFileLspServer.Protocol;
using DanielWillett.UnturnedDataFileLspServer.Utility;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileSystemGlobbing;
using Microsoft.Extensions.Logging;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using OmniSharp.Extensions.LanguageServer.Protocol.Server;
using OmniSharp.Extensions.LanguageServer.Protocol.Server.WorkDone;
using OmniSharp.Extensions.LanguageServer.Server;
using Serilog;
using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using DanielWillett.UnturnedDataFileLspServer.Data.Properties;
using DanielWillett.UnturnedDataFileLspServer.Data.Utility;

namespace DanielWillett.UnturnedDataFileLspServer;

internal sealed class UnturnedAssetFileLspServer
{
    public const string LanguageId = "unturned-dat";
    public const string DiagnosticSource = "unturned-dat";

    private static ILogger<UnturnedAssetFileLspServer> _logger = null!;

    public static int? ClientProcessId { get; private set; }

    private static Timer? _closeTimer;

    public const string FileWatcherGlobPattern = "{**/*.dat,**/*.asset,**/Config_*Difficulty.txt}";

    public static readonly Matcher FileWatcherMatcher = new Matcher().AddInclude("**/*.asset").AddInclude("**/*.dat");

    public static readonly TextDocumentSelector AssetFileSelector = new TextDocumentSelector(new TextDocumentFilter
    {
        Language = LanguageId,
        Pattern = FileWatcherGlobPattern
    });

    public static string DebugPath => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "UnturnedDatLSP");

    private static async Task Main()
    {
#if DEBUG
        string logPath = Path.Combine(DebugPath, "log.txt");
        if (File.Exists(logPath))
            File.WriteAllBytes(logPath, ReadOnlySpan<byte>.Empty);
        else
            Directory.CreateDirectory(DebugPath);
#endif
        Log.Logger = new LoggerConfiguration()
            .Enrich.FromLogContext()
#if DEBUG
            .WriteTo.File(logPath)
#endif
            .MinimumLevel.Debug()
            .CreateLogger();

#if DEBUG
        if (Environment.GetEnvironmentVariable("UNTURNED_LSP_DEBUG") == "1")
        {
            Debugger.Launch();
        }
#endif

        ILanguageServer server = await LanguageServer.From(bldr =>
        {
            Console.InputEncoding = Encoding.UTF8;
            Console.OutputEncoding = Encoding.UTF8;
            bldr.ConfigureLogging(logr =>
                {
                    logr.AddSerilog(Log.Logger)
                        .AddLanguageProtocolLogging()
                        .SetMinimumLevel(LogLevel.Debug);
                })
                .WithOutput(Console.OpenStandardOutput())
                .WithInput(Console.OpenStandardInput())
                .WithHandler<UnturnedAssetFileSyncHandler>()
                .WithHandler<HoverHandler>()
                .WithHandler<DocumentSymbolHandler>()
                .WithHandler<KeyCompletionHandler>()
                // todo: .WithHandler<DiscoverAssetPropertiesHandler>()
                .WithHandler<LspWorkspaceEnvironment>()
                // todo: .WithHandler<GetAssetPropertyAddLocationHandler>()
                .WithHandler<CodeActionRequestHandler>()
                //.WithHandler<DocumentDiagnosticHandler>()
                .WithServerInfo(new ServerInfo
                {
                    Name = "Unturned Data File LSP",
                    Version = Assembly.GetExecutingAssembly().GetName().Version?.ToString(4)
                })
                // used to add custom json converters
                .WithSerializer(new UnturnedLspSerializer())
                .WithServices(serv =>
                {
                    serv.AddSingleton<UnturnedAssetFileSyncHandler>()
                        .AddSingleton<KeyCompletionHandler>()
                        .AddSingleton<DocumentSymbolHandler>()
                        .AddSingleton<HoverHandler>()
                        .AddSingleton<OpenedFileTracker>()
                        .AddSingleton<FileEvaluationContextFactory>()
                        .AddSingleton<IAssetSpecDatabase, LspAssetSpecDatabase>()
                        .AddSingleton<LspWorkspaceEnvironment>()
                        .AddSingleton<DiagnosticsManager>()
                        .AddSingleton<GlobalCodeFixes>()
                        .AddSingleton<LspInstallationEnvironment>()
                        .AddSingleton<FileRelationalCacheProvider>()
                        .AddSingleton(new InstallDirUtility("Unturned", "304930"))
                        .AddSingleton<EnvironmentCache>()
                        .AddTransient<ISpecDatabaseCache, EnvironmentCache>(sp => sp.GetRequiredService<EnvironmentCache>())
                        .AddTransient<InstallationEnvironment>(sp => sp.GetRequiredService<LspInstallationEnvironment>())
                        .AddTransient<IWorkspaceEnvironment>(sp => sp.GetRequiredService<LspWorkspaceEnvironment>())
                        .AddTransient<IFileRelationalModelProvider>(sp => sp.GetRequiredService<FileRelationalCacheProvider>())
                        .AddSingleton<IParsingServices, ParsingServiceProvider>(sp => new ParsingServiceProvider(sp))
                        .AddSingleton(new JsonSerializerOptions
                        {
                            WriteIndented = true
                        })
                        .AddSingleton(typeof(JsonReaderOptions), new JsonReaderOptions { AllowTrailingCommas = true, CommentHandling = JsonCommentHandling.Skip })
                        .AddSingleton(typeof(JsonWriterOptions), new JsonWriterOptions { Indented = true });
                })
                .OnInitialize(async (server, request, token) =>
                {
                    string[] args = Environment.GetCommandLineArgs();
                    int index = Array.IndexOf(args, "--clientProcessId");
                    if (index >= 0 && index < args.Length - 1 && int.TryParse(args[index + 1], NumberStyles.Any, CultureInfo.InvariantCulture, out int pid))
                    {
                        ClientProcessId = pid;
                    }

                    if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                    {
                        FileAssociationUtility util = ActivatorUtilities.CreateInstance<FileAssociationUtility>(server.Services);
                        await util.AssociateFileTypesAsync(force: Environment.GetEnvironmentVariable("UNTURNED_LSP_RESET_FILE_ASSOC") == "1");
                        if (util is IDisposable d)
                            d.Dispose();
                    }

                    IWorkDoneObserver workDoneManager = server.WorkDoneManager.For(
                        request,
                        new WorkDoneProgressBegin
                        {
                            Message = "Downloading Unturned Asset Specs",
                            Percentage = 10
                        });

                    _initObserver = workDoneManager;

                    _logger = server.Services.GetRequiredService<ILogger<UnturnedAssetFileLspServer>>();

                    IAssetSpecDatabase db = await InitializeAssetSpecDatabaseAsync(server.Services, token);

                    workDoneManager.OnNext(new WorkDoneProgressReport
                    {
                        Message = $"Found {db.Information.ParentTypes.Count} asset types",
                        Percentage = 25
                    });

                    await Task.Delay(500, token);

                    workDoneManager.OnNext(new WorkDoneProgressReport
                    {
                        Message = "Discovering Existing Assets",
                        Percentage = 35
                    });

                    LspInstallationEnvironment env = InitializeInstallEnvironment(server.Services, token);

                    workDoneManager.OnNext(new WorkDoneProgressReport
                    {
                        Message = $"Found {env.FileCount} file(s)",
                        Percentage = 70
                    });

                    await Task.Delay(500, token);
                })
                .OnInitialized((server, _, _, _) =>
                {
                    _initObserver?.OnCompleted();
                    _initObserver = null;

                    // warmup services
                    _ = server.Services.GetRequiredService<DiagnosticsManager>();

#if DEBUG
                    _logger.LogInformation("LSP initialized, client PID: {0}, server PID: {1} ({2}).", ClientProcessId, Environment.ProcessId, Environment.CommandLine);
#else
                    _logger.LogInformation("LSP initialized.");
#endif

                    if (ClientProcessId.HasValue)
                    {
                        _closeTimer = new Timer(static state =>
                        {
                            if (CheckClientProcessAlive())
                                return;

                            ILanguageServer server = (ILanguageServer)state!;

                            // ReSharper disable once AccessToDisposedClosure
                            _closeTimer?.Dispose();
                            server.Dispose();
                            Environment.Exit(0);
                        }, server, TimeSpan.FromMinutes(1), TimeSpan.FromMinutes(1));
                    }

                    return Task.CompletedTask;
                });
        });

        await server.WaitForExit.ConfigureAwait(false);

        if (_closeTimer != null)
        {
            await _closeTimer.DisposeAsync();
        }
    }

    private static bool CheckClientProcessAlive()
    {
        if (!ClientProcessId.HasValue)
            return false;

        Process? process = null;

        try
        {
            process = Process.GetProcessById(ClientProcessId.Value);
        }
        catch (ArgumentException) { }

        return process is { HasExited: false };
    }

    private static IWorkDoneObserver? _initObserver;

    private static async Task<IAssetSpecDatabase> InitializeAssetSpecDatabaseAsync(IServiceProvider serviceProvider, CancellationToken token = default)
    {
        IAssetSpecDatabase db = serviceProvider.GetRequiredService<IAssetSpecDatabase>();

#if DEBUG
        db.UseInternet = false;
#else
        db.UseInternet = Environment.GetEnvironmentVariable("UDAT_OFFLINE_SPEC") != "1";
#endif

        _logger.LogInformation("Initializing asset specs...");
        await db.InitializeAsync(token).ConfigureAwait(false);
        _logger.LogInformation("AssetSpecDatabase initialized.");

        //_logger.LogInformation(JsonSerializer.Serialize(db.Information, db.Options));

        return db;
    }

    private static LspInstallationEnvironment InitializeInstallEnvironment(IServiceProvider serviceProvider, CancellationToken token = default)
    {
        LspInstallationEnvironment env = serviceProvider.GetRequiredService<LspInstallationEnvironment>();

        _logger.LogInformation("Initializing installation environment...");
        env.Discover(token);
        _logger.LogInformation("Installation environment initialized; {0} file(s) found.", env.FileCount);

        return env;
    }
}