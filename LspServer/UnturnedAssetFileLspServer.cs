using LspServer.Files;
using LspServer.Handlers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using OmniSharp.Extensions.LanguageServer.Protocol.Server;
using OmniSharp.Extensions.LanguageServer.Server;
using System.Reflection;
using System.Text;
using System.Text.Json;

namespace LspServer;

internal sealed class UnturnedAssetFileLspServer
{
    public const string LanguageId = "unturned-data-file";

    public static readonly TextDocumentSelector AssetFileSelector = new TextDocumentSelector(new TextDocumentFilter
    {
        Language = LanguageId,
        Pattern = "**/*.{dat,asset}"
    });

    private static void ConfigureServices(IServiceCollection collection)
    {
        collection.AddSingleton<OpenedFileTracker>();
    }

    private static void Main(string[] args) => new UnturnedAssetFileLspServer().RunAsync(args).GetAwaiter().GetResult();

    private async Task RunAsync(string[] args)
    {
        IServiceCollection serv = new ServiceCollection();

        ConfigureServices(serv);

        ILoggerFactory loggerFactory = LoggerFactory.Create(l => l
            .AddProvider(new FileLoggerProvider())
            //.AddLanguageProtocolLogging()
            .SetMinimumLevel(LogLevel.Trace)
        );

        serv
            .AddSingleton(loggerFactory)
            .AddTransient(typeof(ILogger<>), typeof(Logger<>))
            .AddSingleton<UnturnedAssetFileSyncHandler>()
            .AddSingleton<KeyCompletionHandler>()
            .AddSingleton<DocumentSymbolHandler>()
            .AddSingleton<HoverHandler>()
            .AddSingleton<OpenedFileTracker>()
            .AddSingleton<AssetSpecDictionary>()
            .AddSingleton(new JsonSerializerOptions
            {
                WriteIndented = true
            })
            .AddSingleton((object)new JsonReaderOptions { AllowTrailingCommas = true, CommentHandling = JsonCommentHandling.Skip })
            .AddSingleton((object)new JsonWriterOptions { Indented = true })
            .AddLanguageServer(bldr =>
            {
                Console.InputEncoding = Encoding.UTF8;
                Console.OutputEncoding = Encoding.UTF8;
                bldr.WithLoggerFactory(loggerFactory)
                    .WithOutput(Console.OpenStandardOutput())
                    .WithInput(Console.OpenStandardInput())
                    .WithHandler<UnturnedAssetFileSyncHandler>()
                    .WithHandler<HoverHandler>()
                    .WithHandler<DocumentSymbolHandler>()
                    .WithHandler<KeyCompletionHandler>();

                bldr.ServerInfo = new ServerInfo { Name = "Unturned Data File LSP", Version = Assembly.GetExecutingAssembly().GetName().Version?.ToString(4) };
            });
        
        IServiceProvider serviceProvider = serv.BuildServiceProvider(validateScopes: true);

        ILogger<UnturnedAssetFileLspServer> logger = serviceProvider.GetRequiredService<ILogger<UnturnedAssetFileLspServer>>();

        logger.LogInformation("Starting LSP server...");

        ILanguageServer languageServer = serviceProvider.GetRequiredService<ILanguageServer>();

        await languageServer.Initialize(CancellationToken.None);

        logger.LogInformation("LSP initialized...");
        await languageServer.WaitForExit.ConfigureAwait(false);
    }
}

internal class FileLoggerProvider : ILoggerProvider
{
    public readonly StreamWriter _writer;

    private class Logger(string categoryName, StreamWriter writer) : ILogger
    {
        private class NullScope : IDisposable
        {
            public static readonly NullScope Instance = new NullScope();
            public void Dispose() { }
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            lock (writer)
            {
                writer.Write(categoryName);
                writer.Write(" | ");
                writer.WriteLine(formatter(state, exception));

                if (exception != null)
                    writer.WriteLine(exception);

                writer.Flush();
            }
        }

        public bool IsEnabled(LogLevel logLevel) => true;

        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => NullScope.Instance;
    }

    public FileLoggerProvider()
    {
        _writer = new StreamWriter("log.txt", Encoding.UTF8,
            new FileStreamOptions
            {
                Mode = FileMode.Create,
                Access = FileAccess.Write,
                Share = FileShare.Read,
                Options = FileOptions.SequentialScan
            });
    }

    public void Dispose()
    {
        _writer.Close();
    }

    public ILogger CreateLogger(string categoryName) => new Logger(categoryName, _writer);
}