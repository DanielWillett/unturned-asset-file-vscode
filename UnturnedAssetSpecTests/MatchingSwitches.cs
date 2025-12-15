using DanielWillett.UnturnedDataFileLspServer.Data.Json;
using DanielWillett.UnturnedDataFileLspServer.Data.Logic;
using DanielWillett.UnturnedDataFileLspServer.Data.Properties;
using DanielWillett.UnturnedDataFileLspServer.Data.Spec;
using DanielWillett.UnturnedDataFileLspServer.Data.Types;
using DanielWillett.UnturnedDataFileLspServer.Data.Utility;
using System.Text.Json;

namespace UnturnedAssetSpecTests;
public class MatchingSwitches
{
    [Test]
    public async Task BasicSwitch()
    {
        SpecDynamicSwitchValue typeSwitch = new SpecDynamicSwitchValue(SpecPropertyTypeType.Instance,
        new OneOrMore<SpecDynamicSwitchCaseValue>(
        [
            new SpecDynamicSwitchCaseValue(
                SpecDynamicSwitchCaseOperation.And, 
                new SpecDynamicConcreteValue<ISpecPropertyType>(KnownTypes.Float32, SpecPropertyTypeType.Instance),
                new OneOrMore<SpecDynamicSwitchCaseOrCondition>(
                [
                    new SpecDynamicSwitchCaseOrCondition(new SpecCondition(new PropertyRef("Uniform_Scale", null), ConditionOperation.Equal, true, false))
                ])),
            new SpecDynamicSwitchCaseValue(
                SpecDynamicSwitchCaseOperation.And,
                new SpecDynamicConcreteValue<ISpecPropertyType>(KnownTypes.Scale, SpecPropertyTypeType.Instance),
                OneOrMore<SpecDynamicSwitchCaseOrCondition>.Null)
        ]));

        IAssetSpecDatabase db = AssetSpecDatabase.FromOffline();

        await db.InitializeAsync();

        ReadOnlySpan<byte> json = """
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
                                  """u8;

        Utf8JsonReader reader = new Utf8JsonReader(json);
        Assert.That(reader.Read(), Is.True);

        SpecDynamicSwitchValue? valueSwitch
            = SpecDynamicSwitchValueConverter.ReadSwitch(ref reader, db.Options, new PropertyTypeOrSwitch(typeSwitch), false);

        Assert.That(valueSwitch, Is.Not.Null);
        Assert.That(valueSwitch.HasCases, Is.True);
        Assert.That(valueSwitch.Cases, Has.Length.EqualTo(3));
    }
}
