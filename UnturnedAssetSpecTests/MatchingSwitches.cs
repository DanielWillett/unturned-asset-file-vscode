using DanielWillett.UnturnedDataFileLspServer.Data;
using DanielWillett.UnturnedDataFileLspServer.Data.Properties;
using DanielWillett.UnturnedDataFileLspServer.Data.Spec;
using DanielWillett.UnturnedDataFileLspServer.Data.Types;
using DanielWillett.UnturnedDataFileLspServer.Data.Values;
using System.Collections.Immutable;
using System.Numerics;
using System.Text.Json;
using DanielWillett.UnturnedDataFileLspServer.Data.Utility;

namespace UnturnedAssetSpecTests;

public class MatchingSwitches
{
    [Test]
    public void BasicSwitch()
    {
        PropertyReference pref = PropertyReference.Parse("Uniform_Scale");
        AssetSpecDatabase offline = AssetSpecDatabase.FromOnline();
        TypeSwitch typeSwitch = new TypeSwitch(
            TypeOfType.Factory,
            ImmutableArray.Create<ISwitchCase<IType>>
            (
                new ComplexConditionalSwitchCase<IType>(
                    ImmutableArray.Create<IValue<bool>>
                    (
                        new Condition<bool>(new LocalPropertyReference(in pref, null!, offline),
                            DanielWillett.UnturnedDataFileLspServer.Data.Values.Operations.Equal.Instance, true, false)
                    ),
                    JointConditionOperation.Or,
                    Value.Type(Float32Type.Instance)
                ),
                new DefaultSwitchCase<IType>(
                    Value.Type(new Vector3Type(Vector3Kind.Scale, VectorTypeOptions.Default))
                )
            )
        );

        DatProperty owner = DatProperty.Create(
            "Min_Scale",
            typeSwitch,
            DatFileType.CreateFileType(
                new QualifiedType("SDG.Framework.Foliage.FoliageInfoAsset, Assembly-CSharp", true),
                true,
                default,
                null,
                null!
            ),
            default,
            SpecPropertyContext.Property
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
        Assert.That(SwitchValue.TryRead(in rootElement, typeSwitch, null!, owner, out SwitchValue? valueSwitch), Is.True);

        Assert.That(valueSwitch, Is.Not.Null);
        Assert.That(valueSwitch.Cases, Has.Length.EqualTo(3));

        Assert.That(valueSwitch.Cases[0].Value, Is.AssignableTo<IValue<float>>());
        Assert.That(valueSwitch.Cases[1].Value, Is.AssignableTo<IValue<float>>());
        Assert.That(valueSwitch.Cases[2].Value, Is.AssignableTo<IValue<Vector3>>());

        Assert.That(
            valueSwitch.Cases[0].Value.TryGetConcreteValueAs(out Optional<float> opt) ? opt : Optional<float>.Null,
            Is.EqualTo(new Optional<float>(0f))
        );
        Assert.That(
            valueSwitch.Cases[1].Value.TryGetConcreteValueAs(out opt) ? opt : Optional<float>.Null,
            Is.EqualTo(new Optional<float>(1f))
        );
        Assert.That(
            valueSwitch.Cases[2].Value.TryGetConcreteValueAs(out Optional<Vector3> opt2) ? opt2 : Optional<Vector3>.Null,
            Is.EqualTo(new Optional<Vector3>(new Vector3(0f, 0f, 0f)))
        );
    }
}
