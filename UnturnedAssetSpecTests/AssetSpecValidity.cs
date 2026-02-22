using DanielWillett.UnturnedDataFileLspServer.Data.Spec;
using Microsoft.Extensions.Logging;
#if TEST_LSP
using DanielWillett.UnturnedDataFileLspServer.Files;
#endif

namespace UnturnedAssetSpecTests;

public class AssetSpecValidity
{
    [Test]
    public async Task CheckSpecValidity()
    {
        List<string> failed = new List<string>();
        ILoggerFactory loggerFactory = LoggerFactory.Create(
            l => l.AddConsole()
                  .AddProvider(new TestLoggerProvider(msg => failed.Add(msg)))
        );
#if TEST_LSP
        EnvironmentCache cache = new EnvironmentCache(loggerFactory.CreateLogger<EnvironmentCache>());
#else
        ISpecDatabaseCache? cache = null;
#endif
        AssetSpecDatabase db = AssetSpecDatabase.FromOffline(
            false,
            loggerFactory,
            cache: cache
        );

        await db.InitializeAsync();

        if (failed.Count > 0)
        {
            throw new AggregateException(failed.Select(x => new Exception(x)));
        }

        Assert.That(db.AllTypes,                Is.Not.Empty);
        Assert.That(db.FileTypes,               Is.Not.Empty);
        // Assert.That(db.LocalizationFileTypes,   Is.Not.Empty); requires game files
        Assert.That(db.BlueprintSkills,         Is.Not.Empty);
        // Assert.That(db.NPCAchievementIds,       Is.Not.Empty); requires game files if not online
        // Assert.That(db.ValidActionButtons,      Is.Not.Empty); requires game files if not online
    }
}

public class TestLoggerProvider : ILoggerProvider
{
    private readonly Action<string> _onLog;

    public TestLoggerProvider(Action<string> onLog)
    {
        _onLog = onLog;
    }

    /// <inheritdoc />
    public void Dispose()
    {
    }

    /// <inheritdoc />
    public ILogger CreateLogger(string categoryName)
    {
        return new TestLogger(this);
    }

    public class TestLogger : ILogger
    {
        private readonly TestLoggerProvider _provider;

        public TestLogger(TestLoggerProvider provider)
        {
            _provider = provider;
        }

        /// <inheritdoc />
        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            string message = formatter(state, exception);
            _provider._onLog?.Invoke(message);
        }

        /// <inheritdoc />
        public bool IsEnabled(LogLevel logLevel)
        {
            return true;
        }

        /// <inheritdoc />
        public IDisposable? BeginScope<TState>(TState state) where TState : notnull
        {
            return null;
        }
    }
}