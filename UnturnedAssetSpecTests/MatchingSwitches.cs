using DanielWillett.UnturnedDataFileLspServer.Data.Properties;
using DanielWillett.UnturnedDataFileLspServer.Data.Types;
using DanielWillett.UnturnedDataFileLspServer.Data.Values;
using System.Collections.Immutable;
using System.Text.Json;

namespace UnturnedAssetSpecTests;

public class MatchingSwitches
{
    [Test]
    public void BasicSwitch()
    {
        TypeSwitch typeSwitch = new TypeSwitch(
            TypeOfType.Factory,
            ImmutableArray.Create<ISwitchCase<IType>>
            (
                new ComplexConditionalSwitchCase<IType>(
                    ImmutableArray.Create<IValue<bool>>
                    (
                        new Condition<bool>(new PropertyReferenceValue(PropertyReference.Parse("Uniform_Scale")),
                            DanielWillett.UnturnedDataFileLspServer.Data.Values.Operations.Equals.Instance, true, false)
                    ),
                    SpecDynamicSwitchCaseOperation.Or,
                    Value.Type(Float32Type.Instance)
                ),
                new DefaultSwitchCase<IType>(
                    Value.Type(new Vector3Type(Vector3Kind.Scale, VectorTypeOptions.Default))
                )
            )
        );

        const string json = """
                            [
                                {
                                    "And":
                                    [
                                        {
                                            "Variable": "Uniform_Scale",
                                            "Operation": "eq",
                                            "Comparand": true
                                        },
                                        {
                                            "Variable": "Something_Else",
                                            "Operation": "eq",
                                            "Comparand": true
                                        }
                                    ],
                                    "Value": 0
                                },
                                {
                                    "And":
                                    [
                                        {
                                            "Variable": "Uniform_Scale",
                                            "Operation": "eq",
                                            "Comparand": true
                                        }
                                    ],
                                    "Value": 1
                                },
                                {
                                    "Value": "0, 0, 0"
                                }
                            ]
                            """;

        using JsonDocument doc = JsonDocument.Parse(json);

        JsonElement rootElement = doc.RootElement;
        Assert.That(SwitchValue.TryRead(in rootElement, typeSwitch, out SwitchValue? valueSwitch), Is.True);

        Assert.That(valueSwitch, Is.Not.Null);
        Assert.That(valueSwitch.Cases, Has.Length.EqualTo(3));
    }
}
