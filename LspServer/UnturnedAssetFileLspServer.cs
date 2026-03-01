using DanielWillett.UnturnedDataFileLspServer.Data.Spec;
using DanielWillett.UnturnedDataFileLspServer.Diagnostics;
using DanielWillett.UnturnedDataFileLspServer.Files;
using DanielWillett.UnturnedDataFileLspServer.Protocol;
using DanielWillett.UnturnedDataFileLspServer.Utility;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileSystemGlobbing;
using Microsoft.Extensions.Logging;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using OmniSharp.Extensions.LanguageServer.Protocol.Server;
using OmniSharp.Extensions.LanguageServer.Protocol.Server.WorkDone;
using OmniSharp.Extensions.LanguageServer.Server;
using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using OmniSharp.Extensions.LanguageServer.Protocol.General;

namespace DanielWillett.UnturnedDataFileLspServer;

internal sealed class UnturnedAssetFileLspServer
{
    public const string LanguageId = "unturned-dat";
    public const string ConfigurationSectionId = "unturned-data-file-lsp";
    public const string DiagnosticSource = "unturned-dat";

    private static ILogger<UnturnedAssetFileLspServer> _logger = null!;

    public static long? ClientProcessId { get; private set; }

    private static Timer? _closeTimer;

    public const string FileWatcherGlobPattern = "{**/*.dat,**/*.asset,**/Config_*Difficulty.txt}";

    public static readonly Matcher FileWatcherMatcher = new Matcher().AddInclude("**/*.asset").AddInclude("**/*.dat");

    public static readonly TextDocumentSelector AssetFileSelector = new TextDocumentSelector(new TextDocumentFilter
    {
        Language = LanguageId,
        Pattern = FileWatcherGlobPattern
    });

    private static ILanguageServer _server;

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

#if DEBUG
        if (Environment.GetEnvironmentVariable("UNTURNED_LSP_DEBUG") == "1")
        {
            Debugger.Launch();
        }
#endif

        _server = await LanguageServer.From(bldr =>
        {
#if DEBUG
            bldr.OnSetTrace(_ =>
            {
                object loggingManager = _server.GetType().GetField("_languageServerLoggingManager", BindingFlags.NonPublic | BindingFlags.Instance)!
                    .GetValue(_server)!;
                loggingManager.GetType().GetMethod("SetTrace", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)!
                    .Invoke(loggingManager, [InitializeTrace.Verbose]);
            });
#endif

            Console.InputEncoding = Encoding.UTF8;
            Console.OutputEncoding = Encoding.UTF8;
            bldr.ConfigureLogging(logr =>
                {
                    logr.AddLanguageProtocolLogging()
                        .SetMinimumLevel(LogLevel.Trace);
                })
                .WithOutput(Console.OpenStandardOutput())
                .WithInput(Console.OpenStandardInput())
                // .WithHandler<UnturnedAssetFileSyncHandler>()
                // .WithHandler<HoverHandler>()
                // .WithHandler<DocumentSymbolHandler>()
                // .WithHandler<KeyCompletionHandler>()
                // todo: .WithHandler<DiscoverAssetPropertiesHandler>()
                // .WithHandler<LspWorkspaceEnvironment>()
                // todo: .WithHandler<GetAssetPropertyAddLocationHandler>()
                // .WithHandler<CodeActionRequestHandler>()
                //.WithHandler<DocumentDiagnosticHandler>()
                .WithServerInfo(new ServerInfo
                {
                    Name = "Unturned Data File LSP",
                    Version = Assembly.GetExecutingAssembly().GetName().Version?.ToString(4)
                })
                .WithConfigurationSection(ConfigurationSectionId)
                // used to add custom json converters
                .WithSerializer(new UnturnedLspSerializer())
                .WithServices(serv =>
                {
                    //serv//.AddSingleton<UnturnedAssetFileSyncHandler>()
                        //.AddSingleton<KeyCompletionHandler>()
                        //.AddSingleton<DocumentSymbolHandler>()
                        //.AddSingleton<HoverHandler>()
                        //.AddSingleton<OpenedFileTracker>()
                        //.AddSingleton<FileEvaluationContextFactory>()
                        //.AddSingleton<IAssetSpecDatabase, LspAssetSpecDatabase>()
                        //.AddSingleton<LspWorkspaceEnvironment>()
                        //.AddSingleton<DiagnosticsManager>()
                        //.AddSingleton<GlobalCodeFixes>()
                        //.AddSingleton<LspInstallationEnvironment>()
                        //.AddSingleton<FileRelationalCacheProvider>()
                        //.AddSingleton(new InstallDirUtility("Unturned", "304930"))
                        //.AddSingleton<EnvironmentCache>()
                        //.AddTransient<ISpecDatabaseCache, EnvironmentCache>(sp => sp.GetRequiredService<EnvironmentCache>())
                        //.AddTransient<InstallationEnvironment>(sp => sp.GetRequiredService<LspInstallationEnvironment>())
                        //.AddTransient<IWorkspaceEnvironment>(sp => sp.GetRequiredService<LspWorkspaceEnvironment>())
                        //.AddTransient<IFileRelationalModelProvider>(sp => sp.GetRequiredService<FileRelationalCacheProvider>())
                        //.AddSingleton<IParsingServices, ParsingServiceProvider>(sp => new ParsingServiceProvider(sp))
                        //.AddSingleton(new JsonSerializerOptions
                        //{
                        //    WriteIndented = true
                        //})
                        //.AddSingleton(typeof(JsonReaderOptions), new JsonReaderOptions { AllowTrailingCommas = true, CommentHandling = JsonCommentHandling.Skip })
                        //.AddSingleton(typeof(JsonWriterOptions), new JsonWriterOptions { Indented = true });
                })
                .OnInitialize(async (server, request, token) =>
                {
                    _server = server;
#if DEBUG
                    typeof(InitializeParams).GetField("<Trace>k__BackingField", BindingFlags.NonPublic | BindingFlags.Instance)!
                        .SetValue(request, InitializeTrace.Verbose);
                    object loggingManager = server.GetType().GetField("_languageServerLoggingManager", BindingFlags.NonPublic | BindingFlags.Instance)!
                        .GetValue(server)!;
                    loggingManager.GetType().GetMethod("SetTrace", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)!
                        .Invoke(loggingManager, [ InitializeTrace.Verbose ]);
#endif
                    ClientProcessId = request.ProcessId;

                    _logger = server.Services.GetRequiredService<ILogger<UnturnedAssetFileLspServer>>();
                })
                .OnInitialized(async (server, _, _, _) =>
                {
                    // warmup services
                    // todo: _ = server.Services.GetRequiredService<DiagnosticsManager>();

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
                });
        });

        await OnStarted(CancellationToken.None);

        await _server.WaitForExit.ConfigureAwait(false);

        if (_closeTimer != null)
        {
            await _closeTimer.DisposeAsync();
        }
    }

    private static async Task OnStarted(CancellationToken token)
    {
        IConfigurationSection config = _server.Configuration.GetSection(ConfigurationSectionId);

        if (_server.ClientSettings.Capabilities?.Workspace?.Configuration is { IsSupported: true })
        {
            if (!config.AsEnumerable().Any())
            {
                _logger.LogWarning("Configuration not received.");
            }
            else
            {
                _logger.LogTrace("Configuration ready.");
                foreach (IConfigurationSection section in config.GetChildren())
                {
                    _logger.LogTrace($"  {section.Key}: {section.Value ?? "{ ... }"}");
                }

                await HandleConfigurationReady(config, token);
            }
        }
        else
        {
            _logger.LogWarning("Configuration not supported by client.");
        }

        if (_initObserver != null)
        {
            _initObserver.OnCompleted();
            _initObserver.Dispose();
            _initObserver = null;
        }

        _logger.LogInformation("Language server fully started.");
    }

    private static async Task HandleConfigurationReady(IConfigurationSection config, CancellationToken token)
    {
        IWorkDoneObserver workDoneManager = await _server.WorkDoneManager.Create(
            new ProgressToken(Guid.NewGuid().ToString()),
            new WorkDoneProgressBegin
            {
                Title = "Start Unturned Data File Language Server",
                Message = "Downloading Unturned Asset Specs",
                Percentage = 10
            },
            onComplete: () => new WorkDoneProgressEnd { Message = "Language Server Started." },
            cancellationToken: token
        );

        _initObserver = workDoneManager;

        //IAssetSpecDatabase db = await InitializeAssetSpecDatabaseAsync(server.Services, token);

        //workDoneManager.OnNext(new WorkDoneProgressReport
        //{
        //    Message = $"Found {db.Information.ParentTypes.Count} asset types",
        //    Percentage = 25
        //});

        await Task.Delay(500, token);

        workDoneManager.OnNext(new WorkDoneProgressReport
        {
            Message = "Discovering Existing Assets",
            Percentage = 35
        });

        //LspInstallationEnvironment env = InitializeInstallEnvironment(server.Services, token);

        //workDoneManager.OnNext(new WorkDoneProgressReport
        //{
        //    Message = $"Found {env.FileCount} file(s)",
        //    Percentage = 70
        //});

        await Task.Delay(500, token);

        bool registerFileAssociations = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) && config.GetValue<bool>("registerFileAssociations");

        workDoneManager.OnNext(new WorkDoneProgressReport
        {
            Message = "Initialized.",
            Percentage = registerFileAssociations ? 95 : 99
        });

        if (registerFileAssociations)
        {
            workDoneManager.OnNext(new WorkDoneProgressReport
            {
                Message = "Checking file associations",
                Percentage = 99
            });

            FileAssociationUtility util = ActivatorUtilities.CreateInstance<FileAssociationUtility>(_server.Services);
            try
            {
                await util.AssociateFileTypesAsync(force: Environment.GetEnvironmentVariable("UNTURNED_LSP_RESET_FILE_ASSOC") == "1");
            }
            finally
            {
                if (util is IDisposable d)
                    d.Dispose();
            }
        }
        else
        {
            _logger.LogTrace("Skipping registering file associations.");
        }
    }

    private static bool CheckClientProcessAlive()
    {
        if (!ClientProcessId.HasValue)
            return false;

        Process? process = null;

        try
        {
            process = Process.GetProcessById(checked ( (int)ClientProcessId.Value ));
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