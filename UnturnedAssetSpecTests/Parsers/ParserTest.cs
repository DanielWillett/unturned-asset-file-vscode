using System.Globalization;
using System.Runtime.CompilerServices;
using DanielWillett.UnturnedDataFileLspServer.Data.AssetEnvironment;
using DanielWillett.UnturnedDataFileLspServer.Data.Diagnostics;
using DanielWillett.UnturnedDataFileLspServer.Data.Files;
using DanielWillett.UnturnedDataFileLspServer.Data.Parsing;
using DanielWillett.UnturnedDataFileLspServer.Data.Properties;
using DanielWillett.UnturnedDataFileLspServer.Data.Spec;
using DanielWillett.UnturnedDataFileLspServer.Data.Types;
using DanielWillett.UnturnedDataFileLspServer.Data.Utility;
using Microsoft.Extensions.Logging;

namespace UnturnedAssetSpecTests.Parsers;

public class ParserTest<T> : IDisposable, IDiagnosticSink, IReferencedPropertySink
    where T : IEquatable<T>
{
    private readonly Func<IParsingServices, ISourceFile> _sourceFileFactory;
    private readonly Action<Optional<T>, IParsingServices> _handleValue;
    private readonly string _propertyName;
    private readonly Func<IParsingServices, IType<T>> _type;
    private readonly PropertyResolutionContext _keyFilter;
    private readonly IParsingServices _parsingServices;
    private readonly TypeParserMissingValueBehavior _missingValueBahvior;
    private readonly bool _expectValue;
    private bool _requireInit;

    private readonly List<DatDiagnosticMessage> _diagnostics;
    private readonly List<IPropertySourceNode> _referencedProperties;
    private readonly List<IPropertySourceNode> _dereferencedProperties;

    public IReadOnlyList<DatDiagnosticMessage> Diagnostics { get; }
    public IReadOnlyList<IPropertySourceNode> ReferencedProperties { get; }
    public IReadOnlyList<IPropertySourceNode> DereferencedProperties { get; }

    public static ParserTest<T> CreateFromSingleProperty(string value, Optional<T> expectedValue, IType<T> type, string property = "Property", string filename = "test.dat")
    {
        return CreateFromSingleProperty(value, _ => expectedValue, _ => type, property, filename);
    }

    public static ParserTest<T> CreateFromSingleProperty(string value, Func<IParsingServices, Optional<T>> expectedValue, Func<IParsingServices, IType<T>> type, string property = "Property", string filename = "test.dat")
    {
        return new ParserTest<T>(
            sourceFileFactory:
            s => StaticSourceFile.FromOtherFile(
                filename,
                $"{property} {value}",
                s.Database).SourceFile,
            handleValue:
            (value, services) =>
            {
                Optional<T> expected = expectedValue(services);
                Assert.That(value.HasValue, Is.EqualTo(expected.HasValue));
                if (!value.HasValue)
                    return;
                
                if (typeof(T) == typeof(float))
                {
                    Assert.That(MathMatrix.As<T, float>(value.Value), Is.EqualTo(MathMatrix.As<T?, float>(expected.Value)).Within(0.00001f));
                }
                else if (typeof(T) == typeof(double))
                {
                    Assert.That(MathMatrix.As<T, double>(value.Value), Is.EqualTo(MathMatrix.As<T?, double>(expected.Value)).Within(0.0000001f));
                }
                else
                {
                    Assert.That(value, Is.EqualTo(expected));
                }
            },
            propertyName: property,
            type: type
        );
    }

    public static ParserTest<T> CreateFromSinglePropertyExpectFailure(string value, IType<T> type, string property = "Property", string filename = "test.dat")
    {
        return CreateFromSinglePropertyExpectFailure(value, _ => type, property, filename);
    }

    public static ParserTest<T> CreateFromSinglePropertyExpectFailure(string value, Func<IParsingServices, IType<T>> type, string property = "Property", string filename = "test.dat")
    {
        return new ParserTest<T>(
            sourceFileFactory:
            s => StaticSourceFile.FromOtherFile(
                filename,
                string.IsNullOrEmpty(value) ? property : $"{property} \"{value}\"",
                s.Database).SourceFile,
            handleValue: null!,
            expectValue: false,
            propertyName: property,
            type: type
        );
    }

    public ParserTest(
        Func<IParsingServices, ISourceFile> sourceFileFactory,
        Action<Optional<T>, IParsingServices> handleValue,
        string propertyName,
        Func<IParsingServices, IType<T>> type,
        PropertyResolutionContext keyFilter = PropertyResolutionContext.Unknown,
        TypeParserMissingValueBehavior missingValueBahvior = TypeParserMissingValueBehavior.ErrorIfValueOrPropertyNotProvided,
        bool expectValue = true,
        bool requireInit = true)
    {
        // avoid localization issues in diagnostics checks
        Thread.CurrentThread.CurrentCulture = CultureInfo.GetCultureInfo("en-US");

        _sourceFileFactory = sourceFileFactory;
        _handleValue = handleValue;
        _propertyName = propertyName;
        _type = type;
        _missingValueBahvior = missingValueBahvior;
        _expectValue = expectValue;
        _requireInit = requireInit;
        _keyFilter = keyFilter;

        ILoggerFactory loggerFactory = LoggerFactory.Create(l => l.AddConsole());

        AssetSpecDatabase database = AssetSpecDatabase.FromOffline(loggerFactory: loggerFactory);

        InstallationEnvironment env = new InstallationEnvironment(database);

        _parsingServices = new ParsingServiceProvider(
            database,
            loggerFactory,
            new StaticSourceFileWorkspaceEnvironment(useCache: true, database, installationEnvironment: env),
            database.UnturnedInstallDirectory,
            env
        );

        _diagnostics = new List<DatDiagnosticMessage>();
        _referencedProperties = new List<IPropertySourceNode>();
        _dereferencedProperties = new List<IPropertySourceNode>();

        Diagnostics = _diagnostics.AsReadOnly();
        ReferencedProperties = _referencedProperties.AsReadOnly();
        DereferencedProperties = _dereferencedProperties.AsReadOnly();
    }

    public async Task<IPropertySourceNode> Execute()
    {
        await _parsingServices.Database.InitializeAsync(CancellationToken.None);

        _diagnostics.Clear();
        _referencedProperties.Clear();
        _dereferencedProperties.Clear();
        ISourceFile sourceFile = _sourceFileFactory(_parsingServices);
        
        if (!sourceFile.TryGetProperty(_propertyName, out IPropertySourceNode? prop))
        {
            Assert.Fail($"Property \"{_propertyName}\" not found.");
            return null!;
        }

        TypeParserArgs<T> parserArgs = default;
        parserArgs.Type = _type(_parsingServices);
        parserArgs.KeyFilter = _keyFilter;
        parserArgs.MissingValueBehavior = _missingValueBahvior;
        parserArgs.ParentNode = prop;
        parserArgs.ValueNode = prop.Value;
        parserArgs.DiagnosticSink = this;
        parserArgs.ReferencedPropertySink = this;

        FileEvaluationContext ctx = new FileEvaluationContext(_parsingServices, sourceFile);

        if (!parserArgs.Type.Parser.TryParse(ref parserArgs, in ctx, out Optional<T> value))
        {
            if (_expectValue)
                Assert.Fail("Expected a value but parser failed.");
        }
        else if (!_expectValue)
            Assert.Fail($"Expected parser to fail but it resulted in value: {{{(value.HasValue ? value.Value.ToString() : "<null>")}}}.");
        else
            _handleValue(value, _parsingServices);

        return prop;
    }

    /// <inheritdoc />
    public void AcceptDiagnostic(DatDiagnosticMessage diagnostic)
    {
        _diagnostics.Add(diagnostic);
        Console.WriteLine($"Diagnostic: {diagnostic}.");
    }

    /// <inheritdoc />
    public void AcceptReferencedProperty(IPropertySourceNode property)
    {
        _referencedProperties.Add(property);
        Console.WriteLine($"Referenced property: {property.Key}.");
    }

    /// <inheritdoc />
    public void AcceptDereferencedProperty(IPropertySourceNode property)
    {
        _dereferencedProperties.Add(property);
        Console.WriteLine($"Dereferenced property: {property.Key}.");
    }

    public void AssertNoEmissions()
    {
        AssertNoDiagnostics();
        AssertNoReferencedProperties();
        AssertNoDereferencedProperties();
    }

    public void AssertNoDiagnostics()
    {
        Assert.That(_diagnostics, Is.Empty);
    }

    public void AssertDiagnostics(int c)
    {
        Assert.That(_diagnostics, Has.Count.EqualTo(c));
    }

    public void AssertHasDiagnostic(DatDiagnosticMessage diagnostic, bool matchMessage = false, Action<string?>? messageValidation = null)
    {
        if (matchMessage)
        {
            Assert.That(_diagnostics, Does.Contain(diagnostic));
            return;
        }

        DatDiagnosticMessage? potential = null;
        foreach (DatDiagnosticMessage diag in _diagnostics)
        {
            if (diag.Diagnostic != diagnostic.Diagnostic)
                continue;
            if (diag.Range != diagnostic.Range)
            {
                potential = diag;
                continue;
            }

            messageValidation?.Invoke(diag.Message);
            return;
        }

        if (potential.HasValue)
        {
            Assert.That(
                potential.Value.Range,
                Is.EqualTo(diagnostic.Range),
                message: $"Incorrect range for diagnostic {diagnostic.Diagnostic.ErrorId}."
            );
        }

        Assert.Fail($"Expected diagnostic {diagnostic.Diagnostic} at {diagnostic.Range}.");
    }

    public void AssertNoReferencedProperties()
    {
        Assert.That(_referencedProperties, Is.Empty);
    }

    public void AssertReferencedProperties(int c)
    {
        Assert.That(_referencedProperties, Has.Count.EqualTo(c));
    }

    public void AssertHasReferencedProperty(IPropertySourceNode property)
    {
        Assert.That(_referencedProperties, Does.Contain(property));
    }

    public void AssertNoDereferencedProperties()
    {
        Assert.That(_dereferencedProperties, Is.Empty);
    }

    public void AssertDereferencedProperties(int c)
    {
        Assert.That(_dereferencedProperties, Has.Count.EqualTo(c));
    }

    public void AssertHasDereferencedProperty(IPropertySourceNode property)
    {
        Assert.That(_dereferencedProperties, Does.Contain(property));
    }

    /// <inheritdoc />
    public void Dispose()
    {
        (_parsingServices as IDisposable)?.Dispose();
    }
}
