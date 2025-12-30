using DanielWillett.UnturnedDataFileLspServer.Data.Values.Expressions;

namespace UnturnedAssetSpecTests.Expressions;

[TestFixture]
public class TokenizerTests
{
    [Test]
    [TestCase("")]
    [TestCase("ABS)")]
    [TestCase("ABS(%(3)")]
    [TestCase("ABS 3)")]
    [TestCase("ABS(")]
    [TestCase("ABS(3")]
    [TestCase("ABS(=(SQRT(4)))")]
    public void TestExpectedError(string value)
    {
        Assert.Throws<FormatException>(() =>
        {
            using ExpressionTokenizer tokenizer = new ExpressionTokenizer(value.AsSpan());
            while (tokenizer.MoveNext()) ;
        });
    }

    [Test]
    public void ParseConstant()
    {
        ExpressionTokenizer tokenizer = new ExpressionTokenizer("PI");
        try
        {
            Assert.That(tokenizer.MoveNext());

            Assert.That(tokenizer.Current.GetContent(), Is.EqualTo("PI"));
            Assert.That(tokenizer.Current.Type, Is.EqualTo(ExpressionTokenType.FunctionName));

            Assert.That(tokenizer.MoveNext(), Is.False);
        }
        finally
        {
            tokenizer.Dispose();
        }
    }

    [Test]
    [TestCase(@"P\\I", @"P\I")]
    [TestCase(@"P\(", @"P(")]
    [TestCase(@"P\\(", @"P\")]
    [TestCase(@"P\\\\(", @"P\\")]
    public void ParseEscapedConstants(string c, string expected)
    {
        ExpressionTokenizer tokenizer = new ExpressionTokenizer(c);
        try
        {
            Assert.That(tokenizer.MoveNext());

            Assert.That(tokenizer.Current.GetContent(), Is.EqualTo(expected));
            Assert.That(tokenizer.Current.Type, Is.EqualTo(ExpressionTokenType.FunctionName));
        }
        finally
        {
            tokenizer.Dispose();
        }
    }

    [Test]
    public void ParseFunction1Arg_ValueImplicit()
    {
        ExpressionTokenizer tokenizer = new ExpressionTokenizer("ABS(3)");
        try
        {
            Assert.That(tokenizer.MoveNext());

            Assert.That(tokenizer.Current.GetContent(), Is.EqualTo("ABS"));
            Assert.That(tokenizer.Current.Type, Is.EqualTo(ExpressionTokenType.FunctionName));

            Assert.That(tokenizer.MoveNext());

            Assert.That(tokenizer.Current.GetContent(), Is.EqualTo("("));
            Assert.That(tokenizer.Current.Type, Is.EqualTo(ExpressionTokenType.OpenParams));

            Assert.That(tokenizer.MoveNext());

            Assert.That(tokenizer.Current.GetContent(), Is.EqualTo("3"));
            Assert.That(tokenizer.Current.Type, Is.EqualTo(ExpressionTokenType.Value));
            Assert.That(tokenizer.Current.ValueType, Is.EqualTo(ExpressionValueType.Value));

            Assert.That(tokenizer.MoveNext());

            Assert.That(tokenizer.Current.GetContent(), Is.EqualTo(")"));
            Assert.That(tokenizer.Current.Type, Is.EqualTo(ExpressionTokenType.CloseParams));

            Assert.That(tokenizer.MoveNext(), Is.False);
        }
        finally
        {
            tokenizer.Dispose();
        }
    }

    [Test]
    public void ParseFunction1Arg_ValueExplicitParenthesis()
    {
        ExpressionTokenizer tokenizer = new ExpressionTokenizer("ABS(%(3))");
        try
        {
            Assert.That(tokenizer.MoveNext());

            Assert.That(tokenizer.Current.GetContent(), Is.EqualTo("ABS"));
            Assert.That(tokenizer.Current.Type, Is.EqualTo(ExpressionTokenType.FunctionName));

            Assert.That(tokenizer.MoveNext());

            Assert.That(tokenizer.Current.GetContent(), Is.EqualTo("("));
            Assert.That(tokenizer.Current.Type, Is.EqualTo(ExpressionTokenType.OpenParams));

            Assert.That(tokenizer.MoveNext());

            Assert.That(tokenizer.Current.GetContent(), Is.EqualTo("3"));
            Assert.That(tokenizer.Current.Type, Is.EqualTo(ExpressionTokenType.Value));
            Assert.That(tokenizer.Current.ValueType, Is.EqualTo(ExpressionValueType.Value));

            Assert.That(tokenizer.MoveNext());

            Assert.That(tokenizer.Current.GetContent(), Is.EqualTo(")"));
            Assert.That(tokenizer.Current.Type, Is.EqualTo(ExpressionTokenType.CloseParams));

            Assert.That(tokenizer.MoveNext(), Is.False);
        }
        finally
        {
            tokenizer.Dispose();
        }
    }

    [Test]
    public void ParseFunction1Arg_ValueExplicit()
    {
        ExpressionTokenizer tokenizer = new ExpressionTokenizer("ABS(%3)");
        try
        {
            Assert.That(tokenizer.MoveNext());

            Assert.That(tokenizer.Current.GetContent(), Is.EqualTo("ABS"));
            Assert.That(tokenizer.Current.Type, Is.EqualTo(ExpressionTokenType.FunctionName));

            Assert.That(tokenizer.MoveNext());

            Assert.That(tokenizer.Current.GetContent(), Is.EqualTo("("));
            Assert.That(tokenizer.Current.Type, Is.EqualTo(ExpressionTokenType.OpenParams));

            Assert.That(tokenizer.MoveNext());

            Assert.That(tokenizer.Current.GetContent(), Is.EqualTo("3"));
            Assert.That(tokenizer.Current.Type, Is.EqualTo(ExpressionTokenType.Value));
            Assert.That(tokenizer.Current.ValueType, Is.EqualTo(ExpressionValueType.Value));

            Assert.That(tokenizer.MoveNext());

            Assert.That(tokenizer.Current.GetContent(), Is.EqualTo(")"));
            Assert.That(tokenizer.Current.Type, Is.EqualTo(ExpressionTokenType.CloseParams));

            Assert.That(tokenizer.MoveNext(), Is.False);
        }
        finally
        {
            tokenizer.Dispose();
        }
    }

    [Test]
    public void ParseFunction1Arg_PropRefExplicitParenthesis()
    {
        ExpressionTokenizer tokenizer = new ExpressionTokenizer("ABS(@(SDG.Unturned.ItemAsset, Assembly-CSharp::Type))");
        try
        {
            Assert.That(tokenizer.MoveNext());

            Assert.That(tokenizer.Current.GetContent(), Is.EqualTo("ABS"));
            Assert.That(tokenizer.Current.Type, Is.EqualTo(ExpressionTokenType.FunctionName));

            Assert.That(tokenizer.MoveNext());

            Assert.That(tokenizer.Current.GetContent(), Is.EqualTo("("));
            Assert.That(tokenizer.Current.Type, Is.EqualTo(ExpressionTokenType.OpenParams));

            Assert.That(tokenizer.MoveNext());

            Assert.That(tokenizer.Current.GetContent(), Is.EqualTo("SDG.Unturned.ItemAsset, Assembly-CSharp::Type"));
            Assert.That(tokenizer.Current.Type, Is.EqualTo(ExpressionTokenType.Value));
            Assert.That(tokenizer.Current.ValueType, Is.EqualTo(ExpressionValueType.PropertyRef));

            Assert.That(tokenizer.MoveNext());

            Assert.That(tokenizer.Current.GetContent(), Is.EqualTo(")"));
            Assert.That(tokenizer.Current.Type, Is.EqualTo(ExpressionTokenType.CloseParams));

            Assert.That(tokenizer.MoveNext(), Is.False);
        }
        finally
        {
            tokenizer.Dispose();
        }
    }

    [Test]
    public void ParseFunction1Arg_PropRefExplicit()
    {
        ExpressionTokenizer tokenizer = new ExpressionTokenizer("ABS(@Type)");
        try
        {
            Assert.That(tokenizer.MoveNext());

            Assert.That(tokenizer.Current.GetContent(), Is.EqualTo("ABS"));
            Assert.That(tokenizer.Current.Type, Is.EqualTo(ExpressionTokenType.FunctionName));

            Assert.That(tokenizer.MoveNext());

            Assert.That(tokenizer.Current.GetContent(), Is.EqualTo("("));
            Assert.That(tokenizer.Current.Type, Is.EqualTo(ExpressionTokenType.OpenParams));

            Assert.That(tokenizer.MoveNext());

            Assert.That(tokenizer.Current.GetContent(), Is.EqualTo("Type"));
            Assert.That(tokenizer.Current.Type, Is.EqualTo(ExpressionTokenType.Value));
            Assert.That(tokenizer.Current.ValueType, Is.EqualTo(ExpressionValueType.PropertyRef));

            Assert.That(tokenizer.MoveNext());

            Assert.That(tokenizer.Current.GetContent(), Is.EqualTo(")"));
            Assert.That(tokenizer.Current.Type, Is.EqualTo(ExpressionTokenType.CloseParams));

            Assert.That(tokenizer.MoveNext(), Is.False);
        }
        finally
        {
            tokenizer.Dispose();
        }
    }

    [Test]
    public void ParseFunction1Arg_EmptyValue()
    {
        ExpressionTokenizer tokenizer = new ExpressionTokenizer("ABS(())");
        try
        {
            Assert.That(tokenizer.MoveNext());

            Assert.That(tokenizer.Current.GetContent(), Is.EqualTo("ABS"));
            Assert.That(tokenizer.Current.Type, Is.EqualTo(ExpressionTokenType.FunctionName));

            Assert.That(tokenizer.MoveNext());

            Assert.That(tokenizer.Current.GetContent(), Is.EqualTo("("));
            Assert.That(tokenizer.Current.Type, Is.EqualTo(ExpressionTokenType.OpenParams));

            Assert.That(tokenizer.MoveNext());

            Assert.That(tokenizer.Current.GetContent(), Is.EqualTo(string.Empty));
            Assert.That(tokenizer.Current.Type, Is.EqualTo(ExpressionTokenType.Value));
            Assert.That(tokenizer.Current.ValueType, Is.EqualTo(ExpressionValueType.Value));

            Assert.That(tokenizer.MoveNext());

            Assert.That(tokenizer.Current.GetContent(), Is.EqualTo(")"));
            Assert.That(tokenizer.Current.Type, Is.EqualTo(ExpressionTokenType.CloseParams));

            Assert.That(tokenizer.MoveNext(), Is.False);
        }
        finally
        {
            tokenizer.Dispose();
        }
    }

    [Test]
    public void ParseFunction1Arg_DataRefExplicitParenthesis()
    {
        ExpressionTokenizer tokenizer = new ExpressionTokenizer("ABS(#(SDG.Unturned.ItemAsset, Assembly-CSharp::Type.IsLegacy))");
        try
        {
            Assert.That(tokenizer.MoveNext());

            Assert.That(tokenizer.Current.GetContent(), Is.EqualTo("ABS"));
            Assert.That(tokenizer.Current.Type, Is.EqualTo(ExpressionTokenType.FunctionName));

            Assert.That(tokenizer.MoveNext());

            Assert.That(tokenizer.Current.GetContent(), Is.EqualTo("("));
            Assert.That(tokenizer.Current.Type, Is.EqualTo(ExpressionTokenType.OpenParams));

            Assert.That(tokenizer.MoveNext());

            Assert.That(tokenizer.Current.GetContent(),
                Is.EqualTo("SDG.Unturned.ItemAsset, Assembly-CSharp::Type.IsLegacy"));
            Assert.That(tokenizer.Current.Type, Is.EqualTo(ExpressionTokenType.Value));
            Assert.That(tokenizer.Current.ValueType, Is.EqualTo(ExpressionValueType.DataRef));

            Assert.That(tokenizer.MoveNext());

            Assert.That(tokenizer.Current.GetContent(), Is.EqualTo(")"));
            Assert.That(tokenizer.Current.Type, Is.EqualTo(ExpressionTokenType.CloseParams));

            Assert.That(tokenizer.MoveNext(), Is.False);
        }
        finally
        {
            tokenizer.Dispose();
        }
    }

    [Test]
    public void ParseFunction1Arg_DataRefExplicit()
    {
        ExpressionTokenizer tokenizer = new ExpressionTokenizer("ABS(#Self.IsLegacy)");
        try
        {
            Assert.That(tokenizer.MoveNext());

            Assert.That(tokenizer.Current.GetContent(), Is.EqualTo("ABS"));
            Assert.That(tokenizer.Current.Type, Is.EqualTo(ExpressionTokenType.FunctionName));

            Assert.That(tokenizer.MoveNext());

            Assert.That(tokenizer.Current.GetContent(), Is.EqualTo("("));
            Assert.That(tokenizer.Current.Type, Is.EqualTo(ExpressionTokenType.OpenParams));

            Assert.That(tokenizer.MoveNext());

            Assert.That(tokenizer.Current.GetContent(), Is.EqualTo("Self.IsLegacy"));
            Assert.That(tokenizer.Current.Type, Is.EqualTo(ExpressionTokenType.Value));
            Assert.That(tokenizer.Current.ValueType, Is.EqualTo(ExpressionValueType.DataRef));

            Assert.That(tokenizer.MoveNext());

            Assert.That(tokenizer.Current.GetContent(), Is.EqualTo(")"));
            Assert.That(tokenizer.Current.Type, Is.EqualTo(ExpressionTokenType.CloseParams));

            Assert.That(tokenizer.MoveNext(), Is.False);
        }
        finally
        {
            tokenizer.Dispose();
        }
    }

    [Test]
    public void ParseFunction1Arg_ExpressionExplicit()
    {
        ExpressionTokenizer tokenizer = new ExpressionTokenizer("ABS(=PI)");
        try
        {
            Assert.That(tokenizer.MoveNext());

            Assert.That(tokenizer.Current.GetContent(), Is.EqualTo("ABS"));
            Assert.That(tokenizer.Current.Type, Is.EqualTo(ExpressionTokenType.FunctionName));

            Assert.That(tokenizer.MoveNext());

            Assert.That(tokenizer.Current.GetContent(), Is.EqualTo("("));
            Assert.That(tokenizer.Current.Type, Is.EqualTo(ExpressionTokenType.OpenParams));

            Assert.That(tokenizer.MoveNext());

            Assert.That(tokenizer.Current.GetContent(), Is.EqualTo("PI"));
            Assert.That(tokenizer.Current.Type, Is.EqualTo(ExpressionTokenType.FunctionName));

            Assert.That(tokenizer.MoveNext());

            Assert.That(tokenizer.Current.GetContent(), Is.EqualTo(")"));
            Assert.That(tokenizer.Current.Type, Is.EqualTo(ExpressionTokenType.CloseParams));

            Assert.That(tokenizer.MoveNext(), Is.False);
        }
        finally
        {
            tokenizer.Dispose();
        }
    }

    [Test]
    public void ParseFunction2Arg()
    {
        ExpressionTokenizer tokenizer = new ExpressionTokenizer("ABS(3 @Test)");
        try
        {
            Assert.That(tokenizer.MoveNext());

            Assert.That(tokenizer.Current.GetContent(), Is.EqualTo("ABS"));
            Assert.That(tokenizer.Current.Type, Is.EqualTo(ExpressionTokenType.FunctionName));

            Assert.That(tokenizer.MoveNext());

            Assert.That(tokenizer.Current.GetContent(), Is.EqualTo("("));
            Assert.That(tokenizer.Current.Type, Is.EqualTo(ExpressionTokenType.OpenParams));

            Assert.That(tokenizer.MoveNext());

            Assert.That(tokenizer.Current.GetContent(), Is.EqualTo("3"));
            Assert.That(tokenizer.Current.Type, Is.EqualTo(ExpressionTokenType.Value));

            Assert.That(tokenizer.MoveNext());

            Assert.That(tokenizer.Current.GetContent(), Is.EqualTo(" "));
            Assert.That(tokenizer.Current.Type, Is.EqualTo(ExpressionTokenType.ArgumentSeparator));

            Assert.That(tokenizer.MoveNext());

            Assert.That(tokenizer.Current.GetContent(), Is.EqualTo("Test"));
            Assert.That(tokenizer.Current.Type, Is.EqualTo(ExpressionTokenType.Value));
            Assert.That(tokenizer.Current.ValueType, Is.EqualTo(ExpressionValueType.PropertyRef));

            Assert.That(tokenizer.MoveNext());

            Assert.That(tokenizer.Current.GetContent(), Is.EqualTo(")"));
            Assert.That(tokenizer.Current.Type, Is.EqualTo(ExpressionTokenType.CloseParams));

            Assert.That(tokenizer.MoveNext(), Is.False);
        }
        finally
        {
            tokenizer.Dispose();
        }
    }

    [Test]
    public void ParseFunctionParenthesizedValue()
    {
        ExpressionTokenizer tokenizer = new ExpressionTokenizer("ABS((a b) @Test)");
        try
        {
            Assert.That(tokenizer.MoveNext());

            Assert.That(tokenizer.Current.GetContent(), Is.EqualTo("ABS"));
            Assert.That(tokenizer.Current.Type, Is.EqualTo(ExpressionTokenType.FunctionName));

            Assert.That(tokenizer.MoveNext());

            Assert.That(tokenizer.Current.GetContent(), Is.EqualTo("("));
            Assert.That(tokenizer.Current.Type, Is.EqualTo(ExpressionTokenType.OpenParams));

            Assert.That(tokenizer.MoveNext());

            Assert.That(tokenizer.Current.GetContent(), Is.EqualTo("a b"));
            Assert.That(tokenizer.Current.Type, Is.EqualTo(ExpressionTokenType.Value));

            Assert.That(tokenizer.MoveNext());

            Assert.That(tokenizer.Current.GetContent(), Is.EqualTo(" "));
            Assert.That(tokenizer.Current.Type, Is.EqualTo(ExpressionTokenType.ArgumentSeparator));

            Assert.That(tokenizer.MoveNext());

            Assert.That(tokenizer.Current.GetContent(), Is.EqualTo("Test"));
            Assert.That(tokenizer.Current.Type, Is.EqualTo(ExpressionTokenType.Value));
            Assert.That(tokenizer.Current.ValueType, Is.EqualTo(ExpressionValueType.PropertyRef));

            Assert.That(tokenizer.MoveNext());

            Assert.That(tokenizer.Current.GetContent(), Is.EqualTo(")"));
            Assert.That(tokenizer.Current.Type, Is.EqualTo(ExpressionTokenType.CloseParams));

            Assert.That(tokenizer.MoveNext(), Is.False);
        }
        finally
        {
            tokenizer.Dispose();
        }
    }

    [Test]
    public void ParseFunctionMultiArg()
    {
        ExpressionTokenizer tokenizer = new ExpressionTokenizer("ABS(3 @Test #Self.ValueType =PI)");
        try
        {
            Assert.That(tokenizer.MoveNext());

            Assert.That(tokenizer.Current.GetContent(), Is.EqualTo("ABS"));
            Assert.That(tokenizer.Current.Type, Is.EqualTo(ExpressionTokenType.FunctionName));

            Assert.That(tokenizer.MoveNext());

            Assert.That(tokenizer.Current.GetContent(), Is.EqualTo("("));
            Assert.That(tokenizer.Current.Type, Is.EqualTo(ExpressionTokenType.OpenParams));

            Assert.That(tokenizer.MoveNext());

            Assert.That(tokenizer.Current.GetContent(), Is.EqualTo("3"));
            Assert.That(tokenizer.Current.Type, Is.EqualTo(ExpressionTokenType.Value));
            Assert.That(tokenizer.Current.ValueType, Is.EqualTo(ExpressionValueType.Value));

            Assert.That(tokenizer.MoveNext());

            Assert.That(tokenizer.Current.GetContent(), Is.EqualTo(" "));
            Assert.That(tokenizer.Current.Type, Is.EqualTo(ExpressionTokenType.ArgumentSeparator));

            Assert.That(tokenizer.MoveNext());

            Assert.That(tokenizer.Current.GetContent(), Is.EqualTo("Test"));
            Assert.That(tokenizer.Current.Type, Is.EqualTo(ExpressionTokenType.Value));
            Assert.That(tokenizer.Current.ValueType, Is.EqualTo(ExpressionValueType.PropertyRef));

            Assert.That(tokenizer.MoveNext());

            Assert.That(tokenizer.Current.GetContent(), Is.EqualTo(" "));
            Assert.That(tokenizer.Current.Type, Is.EqualTo(ExpressionTokenType.ArgumentSeparator));

            Assert.That(tokenizer.MoveNext());

            Assert.That(tokenizer.Current.GetContent(), Is.EqualTo("Self.ValueType"));
            Assert.That(tokenizer.Current.Type, Is.EqualTo(ExpressionTokenType.Value));
            Assert.That(tokenizer.Current.ValueType, Is.EqualTo(ExpressionValueType.DataRef));

            Assert.That(tokenizer.MoveNext());

            Assert.That(tokenizer.Current.GetContent(), Is.EqualTo(" "));
            Assert.That(tokenizer.Current.Type, Is.EqualTo(ExpressionTokenType.ArgumentSeparator));

            Assert.That(tokenizer.MoveNext());

            Assert.That(tokenizer.Current.GetContent(), Is.EqualTo("PI"));
            Assert.That(tokenizer.Current.Type, Is.EqualTo(ExpressionTokenType.FunctionName));

            Assert.That(tokenizer.MoveNext());

            Assert.That(tokenizer.Current.GetContent(), Is.EqualTo(")"));
            Assert.That(tokenizer.Current.Type, Is.EqualTo(ExpressionTokenType.CloseParams));

            Assert.That(tokenizer.MoveNext(), Is.False);
        }
        finally
        {
            tokenizer.Dispose();
        }
    }

    [Test]
    public void NestedExpressions()
    {
        ExpressionTokenizer tokenizer = new ExpressionTokenizer(
            "MAX(=MAX(=MAX(=MAX(@Max_Instances_Insane @Max_Instances_Large) @Max_Instances_Medium) @Max_Instances_Small) @Max_Instances_Tiny)"
        );
        try
        {
            Assert.That(tokenizer.MoveNext());

            Assert.That(tokenizer.Current.GetContent(), Is.EqualTo("MAX"));
            Assert.That(tokenizer.Current.Type, Is.EqualTo(ExpressionTokenType.FunctionName));

            Assert.That(tokenizer.MoveNext());

            Assert.That(tokenizer.Current.GetContent(), Is.EqualTo("("));
            Assert.That(tokenizer.Current.Type, Is.EqualTo(ExpressionTokenType.OpenParams));

            Assert.That(tokenizer.MoveNext());

            Assert.That(tokenizer.Current.GetContent(), Is.EqualTo("MAX"));
            Assert.That(tokenizer.Current.Type, Is.EqualTo(ExpressionTokenType.FunctionName));

            Assert.That(tokenizer.MoveNext());

            Assert.That(tokenizer.Current.GetContent(), Is.EqualTo("("));
            Assert.That(tokenizer.Current.Type, Is.EqualTo(ExpressionTokenType.OpenParams));

            Assert.That(tokenizer.MoveNext());

            Assert.That(tokenizer.Current.GetContent(), Is.EqualTo("MAX"));
            Assert.That(tokenizer.Current.Type, Is.EqualTo(ExpressionTokenType.FunctionName));

            Assert.That(tokenizer.MoveNext());

            Assert.That(tokenizer.Current.GetContent(), Is.EqualTo("("));
            Assert.That(tokenizer.Current.Type, Is.EqualTo(ExpressionTokenType.OpenParams));

            Assert.That(tokenizer.MoveNext());

            Assert.That(tokenizer.Current.GetContent(), Is.EqualTo("MAX"));
            Assert.That(tokenizer.Current.Type, Is.EqualTo(ExpressionTokenType.FunctionName));

            Assert.That(tokenizer.MoveNext());

            Assert.That(tokenizer.Current.GetContent(), Is.EqualTo("("));
            Assert.That(tokenizer.Current.Type, Is.EqualTo(ExpressionTokenType.OpenParams));

            Assert.That(tokenizer.MoveNext());

            Assert.That(tokenizer.Current.GetContent(), Is.EqualTo("Max_Instances_Insane"));
            Assert.That(tokenizer.Current.Type, Is.EqualTo(ExpressionTokenType.Value));
            Assert.That(tokenizer.Current.ValueType, Is.EqualTo(ExpressionValueType.PropertyRef));

            Assert.That(tokenizer.MoveNext());

            Assert.That(tokenizer.Current.GetContent(), Is.EqualTo(" "));
            Assert.That(tokenizer.Current.Type, Is.EqualTo(ExpressionTokenType.ArgumentSeparator));

            Assert.That(tokenizer.MoveNext());

            Assert.That(tokenizer.Current.GetContent(), Is.EqualTo("Max_Instances_Large"));
            Assert.That(tokenizer.Current.Type, Is.EqualTo(ExpressionTokenType.Value));
            Assert.That(tokenizer.Current.ValueType, Is.EqualTo(ExpressionValueType.PropertyRef));

            Assert.That(tokenizer.MoveNext());

            Assert.That(tokenizer.Current.GetContent(), Is.EqualTo(")"));
            Assert.That(tokenizer.Current.Type, Is.EqualTo(ExpressionTokenType.CloseParams));

            Assert.That(tokenizer.MoveNext());

            Assert.That(tokenizer.Current.GetContent(), Is.EqualTo(" "));
            Assert.That(tokenizer.Current.Type, Is.EqualTo(ExpressionTokenType.ArgumentSeparator));

            Assert.That(tokenizer.MoveNext());

            Assert.That(tokenizer.Current.GetContent(), Is.EqualTo("Max_Instances_Medium"));
            Assert.That(tokenizer.Current.Type, Is.EqualTo(ExpressionTokenType.Value));
            Assert.That(tokenizer.Current.ValueType, Is.EqualTo(ExpressionValueType.PropertyRef));

            Assert.That(tokenizer.MoveNext());

            Assert.That(tokenizer.Current.GetContent(), Is.EqualTo(")"));
            Assert.That(tokenizer.Current.Type, Is.EqualTo(ExpressionTokenType.CloseParams));

            Assert.That(tokenizer.MoveNext());

            Assert.That(tokenizer.Current.GetContent(), Is.EqualTo(" "));
            Assert.That(tokenizer.Current.Type, Is.EqualTo(ExpressionTokenType.ArgumentSeparator));

            Assert.That(tokenizer.MoveNext());

            Assert.That(tokenizer.Current.GetContent(), Is.EqualTo("Max_Instances_Small"));
            Assert.That(tokenizer.Current.Type, Is.EqualTo(ExpressionTokenType.Value));
            Assert.That(tokenizer.Current.ValueType, Is.EqualTo(ExpressionValueType.PropertyRef));

            Assert.That(tokenizer.MoveNext());

            Assert.That(tokenizer.Current.GetContent(), Is.EqualTo(")"));
            Assert.That(tokenizer.Current.Type, Is.EqualTo(ExpressionTokenType.CloseParams));

            Assert.That(tokenizer.MoveNext());

            Assert.That(tokenizer.Current.GetContent(), Is.EqualTo(" "));
            Assert.That(tokenizer.Current.Type, Is.EqualTo(ExpressionTokenType.ArgumentSeparator));

            Assert.That(tokenizer.MoveNext());

            Assert.That(tokenizer.Current.GetContent(), Is.EqualTo("Max_Instances_Tiny"));
            Assert.That(tokenizer.Current.Type, Is.EqualTo(ExpressionTokenType.Value));
            Assert.That(tokenizer.Current.ValueType, Is.EqualTo(ExpressionValueType.PropertyRef));

            Assert.That(tokenizer.MoveNext());

            Assert.That(tokenizer.Current.GetContent(), Is.EqualTo(")"));
            Assert.That(tokenizer.Current.Type, Is.EqualTo(ExpressionTokenType.CloseParams));

            Assert.That(tokenizer.MoveNext(), Is.False);
        }
        finally
        {
            tokenizer.Dispose();
        }
    }
}
