using DanielWillett.UnturnedDataFileLspServer.Data.AssetEnvironment;
using DanielWillett.UnturnedDataFileLspServer.Data.Spec;
using DanielWillett.UnturnedDataFileLspServer.Files;
using DanielWillett.UnturnedDataFileLspServer.Handlers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using OmniSharp.Extensions.LanguageServer.Protocol.Server;
using OmniSharp.Extensions.LanguageServer.Protocol.Server.WorkDone;
using OmniSharp.Extensions.LanguageServer.Server;
using Serilog;
using System.Reflection;
using System.Text;
using System.Text.Json;

namespace DanielWillett.UnturnedDataFileLspServer;

internal sealed class UnturnedAssetFileLspServer
{
    public const string LanguageId = "unturned-data-file";

    private static ILogger<UnturnedAssetFileLspServer> _logger;

    public static readonly TextDocumentSelector AssetFileSelector = new TextDocumentSelector(new TextDocumentFilter
    {
        Language = LanguageId,
        Pattern = "**/*.{dat,asset}"
    });
    
    private static void Main(string[] args) => new UnturnedAssetFileLspServer().RunAsync(args).GetAwaiter().GetResult();

    private async Task RunAsync(string[] args)
    {
        Log.Logger = new LoggerConfiguration()
            .Enrich.FromLogContext()
            .WriteTo.File(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "log.txt"))
            .MinimumLevel.Debug()
            .CreateLogger();

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
                .WithServerInfo(new ServerInfo
                {
                    Name = "Unturned Data File LSP",
                    Version = Assembly.GetExecutingAssembly().GetName().Version?.ToString(4)
                })
                .WithServices(serv =>
                {
                    serv.AddSingleton<UnturnedAssetFileSyncHandler>()
                        .AddSingleton<KeyCompletionHandler>()
                        .AddSingleton<DocumentSymbolHandler>()
                        .AddSingleton<HoverHandler>()
                        .AddSingleton<OpenedFileTracker>()
                        .AddSingleton<LspAssetSpecDatabase>()
                        .AddTransient<IAssetSpecDatabase, LspAssetSpecDatabase>()
                        .AddSingleton<LspInstallationEnvironment>()
                        .AddSingleton<InstallationEnvironment>(sp => sp.GetRequiredService<LspInstallationEnvironment>())
                        .AddSingleton(new JsonSerializerOptions
                        {
                            WriteIndented = true
                        })
                        .AddSingleton((object)new JsonReaderOptions { AllowTrailingCommas = true, CommentHandling = JsonCommentHandling.Skip })
                        .AddSingleton((object)new JsonWriterOptions { Indented = true });
                })
                .OnInitialize(async (server, request, token) =>
                {
                    IWorkDoneObserver workDoneManager = server.WorkDoneManager.For(
                        request,
                        new WorkDoneProgressBegin
                        {
                            Message = "Downloading Unturned Asset Specs",
                            Percentage = 10
                        });

                    _initObserver = workDoneManager;

                    _logger = server.Services.GetRequiredService<ILogger<UnturnedAssetFileLspServer>>();

                    LspAssetSpecDatabase db = await InitializeAssetSpecDatabase(server.Services, token);

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
                .OnInitialized((_, _, _, _) =>
                {
                    _initObserver?.OnCompleted();
                    _initObserver = null;

                    _logger.LogInformation("LSP initialized.");

                    return Task.CompletedTask;
                });
        });

        await server.WaitForExit.ConfigureAwait(false);
    }

    private static IWorkDoneObserver? _initObserver;

    private static async Task<LspAssetSpecDatabase> InitializeAssetSpecDatabase(IServiceProvider serviceProvider, CancellationToken token = default)
    {
        LspAssetSpecDatabase db = serviceProvider.GetRequiredService<LspAssetSpecDatabase>();

        db.UseInternet = true;

        _logger.LogInformation("Initializing asset specs...");
        await db.InitializeAsync(token).ConfigureAwait(false);
        _logger.LogInformation("AssetSpecDatabase initialized.");

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