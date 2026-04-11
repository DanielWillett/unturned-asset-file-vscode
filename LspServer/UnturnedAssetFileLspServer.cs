using DanielWillett.UnturnedDataFileLspServer.Data.CodeFixes;
using DanielWillett.UnturnedDataFileLspServer.Data.Parsing;
using DanielWillett.UnturnedDataFileLspServer.Data.Project;
using DanielWillett.UnturnedDataFileLspServer.Data.Properties;
using DanielWillett.UnturnedDataFileLspServer.Data.Spec;
using DanielWillett.UnturnedDataFileLspServer.Data.Utility;
using DanielWillett.UnturnedDataFileLspServer.Diagnostics;
using DanielWillett.UnturnedDataFileLspServer.Files;
using DanielWillett.UnturnedDataFileLspServer.Handlers;
using DanielWillett.UnturnedDataFileLspServer.Handlers.AssetProperties;
using DanielWillett.UnturnedDataFileLspServer.Project;
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
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;

namespace DanielWillett.UnturnedDataFileLspServer;

internal sealed class UnturnedAssetFileLspServer
{
    public const string LanguageId = "unturned-dat";
    public const string ConfigurationSectionId = "unturned-data-file-lsp";
    public const string DiagnosticSource = "unturned-dat";

    private static ILogger<UnturnedAssetFileLspServer> _logger = null!;

    public static long? ClientProcessId { get; private set; }

    private static Timer? _closeTimer;

    public const string FileWatcherGlobPattern = "{**/*.dat,**/*.asset,**/*.udatproj,**/Config_*Difficulty.txt,**/Config.txt}";

    public static readonly Matcher FileWatcherMatcher = new Matcher().AddInclude("**/*.asset").AddInclude("**/*.dat");

    public static readonly TextDocumentSelector AssetFileSelector = new TextDocumentSelector(new TextDocumentFilter
    {
        Language = LanguageId,
        Pattern = FileWatcherGlobPattern
    });

#nullable disable

    private static ILanguageServer _server;

#if DEBUG
    public static string DebugPath { get; private set; }
#endif

#nullable restore


    private static async Task Main()
    {
#if DEBUG
        bool windows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
        if (windows || RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            DebugPath = Path.Combine(
                Environment.GetFolderPath(
                    windows
                        ? Environment.SpecialFolder.CommonApplicationData
                        : Environment.SpecialFolder.InternetCache),
                "UnturnedAssetFileLsp", "Debug"
            );
        }
        else
        {
            // Linux, FreeBSD
            DebugPath = "/var/cache/UnturnedAssetFileLsp/Debug";
        }

        Directory.CreateDirectory(DebugPath);
#endif

#if DEBUG
        if (EnvironmentHelper.ParseBooleanEnvironmentVariable("UNTURNED_LSP_DEBUG"))
        {
            Debugger.Launch();
        }
#endif

        _server = await LanguageServer.From(bldr =>
        {
#if DEBUG
            bldr.OnSetTrace(trace =>
            {
                if (trace.Value == InitializeTrace.Verbose)
                {
                    _logger.LogTrace("Trace changed to verbose.");
                }
                else
                {
                    _logger.LogWarning("Requested trace change to {0} ignored.", trace.Value);
                    object loggingManager = _server.GetType().GetField("_languageServerLoggingManager", BindingFlags.NonPublic | BindingFlags.Instance)!
                        .GetValue(_server)!;
                    loggingManager.GetType().GetMethod("SetTrace", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)!
                        .Invoke(loggingManager, [ InitializeTrace.Verbose ]);
                }
            });
#endif

            Console.InputEncoding = Encoding.UTF8;
            Console.OutputEncoding = Encoding.UTF8;
            bldr.ConfigureLogging(logr =>
                {
                    logr.AddLanguageProtocolLogging()
                        .SetMinimumLevel(LogLevel.Trace)
                        .AddFilter((s, lvl) => lvl > LogLevel.Debug || s == null || !s.StartsWith("OmniSharp"));
                })
                .WithOutput(Console.OpenStandardOutput())
                .WithInput(Console.OpenStandardInput())
                // .WithHandler<UnturnedAssetFileSyncHandler>()
                .WithHandler<HoverHandler>()
                // .WithHandler<DocumentSymbolHandler>()
                // .WithHandler<KeyCompletionHandler>()
                .WithHandler<DiscoverAssetPropertiesHandler>()
                .WithHandler<DiscoverBundleAssetsHandler>()
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
                    serv.AddSingleton<UnturnedAssetFileSyncHandler>()
                        //.AddSingleton<KeyCompletionHandler>()
                        //.AddSingleton<DocumentSymbolHandler>()
                        //.AddSingleton<HoverHandler>()
                        .AddSingleton<OpenedFileTracker>()
                        .AddSingleton<FileEvaluationContextFactory>()
                        .AddSingleton<IAssetSpecDatabase, LspAssetSpecDatabase>()
                        .AddSingleton<IProjectFileProvider, LspProjectFileProvider>()
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
                .OnInitialized((server, _, _, _) =>
                {
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

        GC.Collect(GC.MaxGeneration, GCCollectionMode.Aggressive, true, true);
        _logger.LogInformation($"Language server fully started. Memory usage: {GC.GetTotalMemory(false) / 1048576m:F2} MiB.");

        // warm up services
        _ = _server.Services.GetRequiredService<DiagnosticsManager>();

        _server.SendNotification("unturnedDataFile/ready");
    }

    private static async Task HandleConfigurationReady(IConfigurationSection config, CancellationToken token)
    {
#if DEBUG
        const bool useInternet = false;
#else
        bool useInternet = !EnvironmentHelper.ParseBooleanEnvironmentVariable("UDAT_OFFLINE_SPEC")
                         && !_server.Configuration.GetValue<bool>($"{ConfigurationSectionId}:offlineOnly");
#endif

        ApplyDirectoriesFromConfig();

        if (_server.Services.GetRequiredService<InstallDirUtility>().TryGetInstallDirectory(out GameInstallDir loc))
        {
            _logger.LogInformation("Game install directory: \"{0}\"", loc.BaseFolder);
            _logger.LogInformation("Game workshop directory: \"{0}\"", loc.WorkshopFolder);
        }
        else
        {
            _logger.LogWarning("Failed to find Unturned's installation folder. It's possible it just isn't installed. If this isn't the case, configure the install directory in your extension options.");
        }

        IWorkDoneObserver workDoneManager = await _server.WorkDoneManager.Create(
            new ProgressToken(Guid.NewGuid().ToString()),
            new WorkDoneProgressBegin
            {
                Title = "Start Unturned Data File Language Server",
                Percentage = 0
            },
            onComplete: () => new WorkDoneProgressEnd { Message = "Language server started." },
            cancellationToken: token
        );

        IAssetSpecDatabase db = _server.Services.GetRequiredService<IAssetSpecDatabase>();

        db.UseInternet = useInternet;

        _initObserver = workDoneManager;

        int previous = 0;
        await db.InitializeAsync(
            token,
            (progress, str) =>
            {
                int p = (int)Math.Round(progress * 100);
                if (p == previous)
                    return;

                previous = p;
                _initObserver.OnNext(new WorkDoneProgressReport
                {
                    Message = str,
                    Percentage = p
                });
            },
            0.25f
        );

        workDoneManager.OnNext(new WorkDoneProgressReport
        {
            Message = "Discovering existing assets",
            Percentage = 35
        });

        InitializeInstallEnvironment(token);

        workDoneManager.OnNext(new WorkDoneProgressReport
        {
            Message = "Loading workspaces",
            Percentage = 90
        });

        InitializeWorkspaceEnvironment();

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
                await util.AssociateFileTypesAsync(force: EnvironmentHelper.ParseBooleanEnvironmentVariable("UNTURNED_LSP_RESET_FILE_ASSOC"));
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

    private static void ApplyDirectoriesFromConfig()
    {
        string? u3dsDirectory = _server.Configuration.GetValue<string>($"{ConfigurationSectionId}:u3dsInstallDir");
        string? gameDirectory = _server.Configuration.GetValue<string>($"{ConfigurationSectionId}:unturnedInstallDir");
        string? wshpDirectory = _server.Configuration.GetValue<string>($"{ConfigurationSectionId}:unturnedWorkshopDirectory");

        if (!string.IsNullOrEmpty(u3dsDirectory) && !(Path.IsPathRooted(u3dsDirectory) && Directory.Exists(u3dsDirectory)))
        {
            _logger.LogError("Defined U3DS directory, \"{0}\", does not exist.", u3dsDirectory);
        }

        if (!string.IsNullOrEmpty(wshpDirectory) && !(Path.IsPathRooted(wshpDirectory) && Directory.Exists(wshpDirectory)))
        {
            _logger.LogError("Defined Unturned client workshop directory, \"{0}\", does not exist.", wshpDirectory);
            wshpDirectory = null;
        }

        if (string.IsNullOrEmpty(gameDirectory))
            return;

        if (!Path.IsPathRooted(gameDirectory) || !Directory.Exists(gameDirectory))
        {
            _logger.LogError("Defined Unturned client directory, \"{0}\", does not exist.", gameDirectory);
            return;
        }

        if (string.IsNullOrEmpty(wshpDirectory))
        {
            wshpDirectory = Path.Combine(gameDirectory, "..", "..", "workshop", "content", "304930");
            try
            {
                wshpDirectory = Path.GetFullPath(wshpDirectory);
                if (!Directory.Exists(wshpDirectory))
                {
                    _logger.LogError("Automatically determined workshop folder, \"{0}\", does not exist.", wshpDirectory);
                }
            }
            catch (ArgumentException ex)
            {
                _logger.LogError(ex, "Failed to automatically determine the workshop folder from the game folder, \"{0}\".", wshpDirectory);
                wshpDirectory = null;
            }
        }

        InstallDirUtility installDir = _server.Services.GetRequiredService<InstallDirUtility>();

        if (wshpDirectory != null && gameDirectory != null)
        {
            installDir.OverrideInstallDirectory = new GameInstallDir(gameDirectory, wshpDirectory);
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

    private static void InitializeInstallEnvironment(CancellationToken token = default)
    {
        LspInstallationEnvironment env = _server.Services.GetRequiredService<LspInstallationEnvironment>();

        _logger.LogInformation("Initializing installation environment...");
        
        env.Init();

        env.Discover(token);
        
        _logger.LogInformation("Installation environment initialized; {0} file(s) found.", env.FileCount);
    }

    private static void InitializeWorkspaceEnvironment()
    {
        LspWorkspaceEnvironment env = _server.Services.GetRequiredService<LspWorkspaceEnvironment>();

        _logger.LogInformation("Initializing workspace environment...");

        env.CreateAllProjectFiles();

        _logger.LogInformation("Workspace environment initialized; {0} workspace(s) opened.", env.WorkspaceFolders.Count);
    }
}