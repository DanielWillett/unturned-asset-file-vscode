using DanielWillett.UnturnedDataFileLspServer.Data;
using System.Net;
// ReSharper disable InconsistentNaming

namespace UnturnedAssetSpecTests.Parsing;

[TestFixture]
public class IPv4FilterTests
{
    public static IEnumerable<TestCaseData> IPv4FilterParseCases
    {
        get
        {
            yield return new TestCaseData("123.122.121.120/30:55433-55456", new IPv4Filter(123, 122, 121, 120, 55433, 55456, 30));
            yield return new TestCaseData("123.122.121.120/30:55433", new IPv4Filter(123, 122, 121, 120, 55433, 55433, 30));
            yield return new TestCaseData("123.122.121.120:55433-55456", new IPv4Filter(123, 122, 121, 120, 55433, 55456));
            yield return new TestCaseData("123.122.121.120:55433", new IPv4Filter(123, 122, 121, 120, 55433, 55433));
            yield return new TestCaseData("123.122.121.120/30", new IPv4Filter(123, 122, 121, 120, cidrNumber: 30));
            yield return new TestCaseData("123.122.121.120", new IPv4Filter(123, 122, 121, 120));
            yield return new TestCaseData("1.2.3.4/6:8-9", new IPv4Filter(1, 2, 3, 4, 8, 9, 6));
            yield return new TestCaseData("1.2.3.4/6:8", new IPv4Filter(1, 2, 3, 4, 8, 8, 6));
            yield return new TestCaseData("1.2.3.4:8-9", new IPv4Filter(1, 2, 3, 4, 8, 9));
            yield return new TestCaseData("1.2.3.4:8", new IPv4Filter(1, 2, 3, 4, 8, 8));
            yield return new TestCaseData("1.2.3.4/6", new IPv4Filter(1, 2, 3, 4, cidrNumber: 6));
            yield return new TestCaseData("1.2.3.4", new IPv4Filter(1, 2, 3, 4));
        }
    }

    [Test]
    [TestCaseSource(nameof(IPv4FilterParseCases))]
    public void TestParse(string str, IPv4Filter expectedValue)
    {
        IPv4Filter filter = IPv4Filter.Parse(str);

        Assert.That(filter, Is.EqualTo(expectedValue));
        Assert.That(filter.ToString(), Is.EqualTo(str));
        Assert.That(filter.TryFormat(Span<char>.Empty, out int size), Is.False);
        Assert.That(size, Is.EqualTo(str.Length));
    }

    [Test]
    [TestCase("123.122.121.120.180/30:55433-55456")]
    [TestCase("256.122.121.120/30:55433-55456")]
    [TestCase("123.122.121.120/30:")]
    [TestCase("123.122.121.120/")]
    [TestCase("123.122.121.")]
    [TestCase("123.122.121/30:55433-55456")]
    [TestCase("123.122/30:55433-55456")]
    [TestCase("123/30:55433-55456")]
    [TestCase("123.../30:55433-55456")]
    [TestCase("123../30:55433-55456")]
    [TestCase("123./30:55433-55456")]
    [TestCase("...123/30:55433-55456")]
    [TestCase("..123./30:55433-55456")]
    [TestCase(".123../30:55433-55456")]
    [TestCase("..123/30:55433-55456")]
    [TestCase(".123./30:55433-55456")]
    [TestCase(".123/30:55433-55456")]
    [TestCase("123.122.121.120/:55433-55456")]
    [TestCase("123.122.121.120:55433-55456/30")]
    [TestCase("123.122.121.120/30-55456")]
    [TestCase("123.122.121.120/30:-55456")]
    [TestCase("123.122.121.120/30:55433-")]
    [TestCase("123.122.121.120:-55433")]
    [TestCase("123.122.121.120:55433-")]
    [TestCase("123.122.121.120:55433/30-55456")]
    [TestCase("55433-55456:123.122.121.120/30")]
    [TestCase("55433-55456/30:123.122.121.120")]
    [TestCase("1.2.3.4.5/6:8-9")]
    [TestCase("256.2.3.4/6:8-9")]
    [TestCase("1.2.3.4/6:")]
    [TestCase("1.2.3.4/")]
    [TestCase("1.2.3.")]
    [TestCase("1.2.3/6:8-9")]
    [TestCase("1.2/6:8-9")]
    [TestCase("1/6:8-9")]
    [TestCase("1.../6:8-9")]
    [TestCase("1../6:8-9")]
    [TestCase("1./6:8-9")]
    [TestCase("...1/6:8-9")]
    [TestCase("..1./6:8-9")]
    [TestCase(".1../6:8-9")]
    [TestCase("..1/6:8-9")]
    [TestCase(".1./6:8-9")]
    [TestCase(".1/6:8-9")]
    [TestCase("1.2.3.4/:8-9")]
    [TestCase("1.2.3.4:8-9/6")]
    [TestCase("1.2.3.4/6-9")]
    [TestCase("1.2.3.4/6:-9")]
    [TestCase("1.2.3.4/6:8-")]
    [TestCase("1.2.3.4:-8")]
    [TestCase("1.2.3.4:8-")]
    [TestCase("1.2.3.4:1/30-9")]
    [TestCase("1-2:1.2.3.4/3")]
    [TestCase("1-2/3:1.2.3.4")]
    [TestCase("")]
    [TestCase(".../:-")]
    [TestCase(".../:")]
    [TestCase("...:-")]
    [TestCase("...:")]
    [TestCase("...")]
    [TestCase("../:-")]
    [TestCase("../:")]
    [TestCase("..:-")]
    [TestCase("..:")]
    [TestCase("..")]
    [TestCase("./:-")]
    [TestCase("./:")]
    [TestCase(".:-")]
    [TestCase(".:")]
    [TestCase(".")]
    [TestCase("/:-")]
    [TestCase("/:")]
    [TestCase(":-")]
    [TestCase(":")]
    public void TestParseFailure(string str)
    {
        Assert.Throws<FormatException>(() =>
        {
            IPv4Filter.Parse(str);
        });
    }

    [Test]
    public void TestParseFailureNull()
    {
        Assert.Throws<ArgumentNullException>(() =>
        {
            IPv4Filter.Parse(null!);
        });
    }

    [Test]
    public void CheckAddressProperty()
    {
        IPv4Filter filter = new IPv4Filter(192, 168, 14, 17, 25565, 25566, cidrNumber: 24);

        Assert.That(filter.Address, Is.EqualTo(new IPv4Filter(192, 168, 14, 17)));
    }

    [Test]
    public void CheckAlignedProperty()
    {
        IPv4Filter filter = new IPv4Filter(192, 168, 14, 17, 25565, 25566, cidrNumber: 24);

        Assert.That(filter.Aligned, Is.EqualTo(new IPv4Filter(192, 168, 14, 0, 25565, 25566, cidrNumber: 24)));
    }

    [Test]
    public void CheckPortOrderedProperty()
    {
        IPv4Filter filter = new IPv4Filter(192, 168, 14, 17, 25565, 25566, cidrNumber: 24);
        Assert.That(filter.PortOrdered, Is.EqualTo(new IPv4Filter(192, 168, 14, 17, 25565, 25566, cidrNumber: 24)));

        filter = new IPv4Filter(192, 168, 14, 17, 25566, 25565, cidrNumber: 24);
        Assert.That(filter.PortOrdered, Is.EqualTo(new IPv4Filter(192, 168, 14, 17, 25565, 25566, cidrNumber: 24)));
    }

    [Test]
    [TestCase((byte)32, "255.255.255.255")]
    [TestCase((byte)31, "255.255.255.254")]
    [TestCase((byte)30, "255.255.255.252")]
    [TestCase((byte)29, "255.255.255.248")]
    [TestCase((byte)28, "255.255.255.240")]
    [TestCase((byte)27, "255.255.255.224")]
    [TestCase((byte)26, "255.255.255.192")]
    [TestCase((byte)25, "255.255.255.128")]
    [TestCase((byte)24, "255.255.255.0")]
    [TestCase((byte)23, "255.255.254.0")]
    [TestCase((byte)22, "255.255.252.0")]
    [TestCase((byte)21, "255.255.248.0")]
    [TestCase((byte)20, "255.255.240.0")]
    [TestCase((byte)19, "255.255.224.0")]
    [TestCase((byte)18, "255.255.192.0")]
    [TestCase((byte)17, "255.255.128.0")]
    [TestCase((byte)16, "255.255.0.0")]
    [TestCase((byte)15, "255.254.0.0")]
    [TestCase((byte)14, "255.252.0.0")]
    [TestCase((byte)13, "255.248.0.0")]
    [TestCase((byte)12, "255.240.0.0")]
    [TestCase((byte)11, "255.224.0.0")]
    [TestCase((byte)10, "255.192.0.0")]
    [TestCase((byte)9,  "255.128.0.0")]
    [TestCase((byte)8,  "255.0.0.0")]
    [TestCase((byte)7,  "254.0.0.0")]
    [TestCase((byte)6,  "252.0.0.0")]
    [TestCase((byte)5,  "248.0.0.0")]
    [TestCase((byte)4,  "240.0.0.0")]
    [TestCase((byte)3,  "224.0.0.0")]
    [TestCase((byte)2,  "192.0.0.0")]
    [TestCase((byte)1,  "128.0.0.0")]
    [TestCase((byte)0,  "0.0.0.0")]
    public void CheckPackedSubnetMaskMethod(byte number, string mask)
    {
        IPv4Filter filter = new IPv4Filter(0, cidrNumber: number);
        Assert.That(new IPv4Filter(filter.GetPackedSubnetMask()).ToString(), Is.EqualTo(mask));
    }

    [Test]
    public void CheckGetIPAddressMethod()
    {
        IPv4Filter filter = new IPv4Filter(192, 168, 14, 17);
        Assert.That(filter.GetIPAddress(), Is.EqualTo(IPAddress.Parse("192.168.14.17")));
    }

    [Test]
    public void CheckGetPackedIPAddressMethod()
    {
        IPv4Filter filter = new IPv4Filter(192, 168, 14, 17);
        Assert.That(filter.GetPackedIPAddress(), Is.EqualTo((192u << 24) | (168u << 16) | (14u << 8) | 17u));
    }

    [Test]
    public void CheckPortProperties()
    {
        IPv4Filter filter = new IPv4Filter(192, 168, 14, 17, 123, 456);
        Assert.That(filter.MinimumPort, Is.EqualTo((ushort)123));
        Assert.That(filter.MaximumPort, Is.EqualTo((ushort)456));
    }

    [Test]
    public void CheckSubnetMaskProperty()
    {
        IPv4Filter filter = new IPv4Filter(192, 168, 14, 17, 123, 456, cidrNumber: 13);
        Assert.That(filter.SubnetMask, Is.EqualTo(13));
    }

    [Test]
    public void CheckContainsIPAddressMethodsBasic()
    {
        IPv4Filter filter = new IPv4Filter(192, 168, 14, 0, cidrNumber: 24);
        Assert.That(filter.ContainsAddress(new IPv4Filter(192, 168, 14, 0)));
        Assert.That(filter.ContainsAddress(new IPv4Filter(192, 168, 14, 5)));
        Assert.That(filter.ContainsAddress(new IPv4Filter(192, 168, 14, 255)));
        Assert.That(filter.ContainsAddress(new IPv4Filter(192, 168, 15, 0)), Is.False);
        Assert.That(filter.ContainsAddress(new IPv4Filter(192, 168, 13, 255)), Is.False);
        Assert.That(filter.ContainsAddress(new IPv4Filter(100, 24, 8, 14)), Is.False);
    }

    [Test]
    public void CheckContainsIPAddressMethodsSingle()
    {
        IPv4Filter filter = new IPv4Filter(192, 168, 14, 0, cidrNumber: 32);
        Assert.That(filter.ContainsAddress(new IPv4Filter(192, 168, 14, 0)));
        Assert.That(filter.ContainsAddress(new IPv4Filter(192, 168, 14, 1)), Is.False);
        Assert.That(filter.ContainsAddress(new IPv4Filter(192, 168, 14, 255)), Is.False);
        Assert.That(filter.ContainsAddress(new IPv4Filter(192, 168, 15, 0)), Is.False);
        Assert.That(filter.ContainsAddress(new IPv4Filter(192, 168, 13, 255)), Is.False);
        Assert.That(filter.ContainsAddress(new IPv4Filter(100, 24, 8, 14)), Is.False);
    }

    [Test]
    public void CheckContainsIPAddressMethodsAll()
    {
        IPv4Filter filter = new IPv4Filter(192, 168, 14, 0, cidrNumber: 0);
        Assert.That(filter.ContainsAddress(new IPv4Filter(0, 0, 0, 0)));
        Assert.That(filter.ContainsAddress(new IPv4Filter(192, 168, 14, 1)));
        Assert.That(filter.ContainsAddress(new IPv4Filter(192, 168, 14, 255)));
        Assert.That(filter.ContainsAddress(new IPv4Filter(192, 168, 15, 0)));
        Assert.That(filter.ContainsAddress(new IPv4Filter(192, 168, 13, 255)));
        Assert.That(filter.ContainsAddress(new IPv4Filter(100, 24, 8, 14)));
    }

    [Test]
    public void CheckContainsIPAddressMethodsPorts()
    {
        IPv4Filter filter = new IPv4Filter(192, 168, 14, 0, 3, 6, cidrNumber: 24);
        Assert.That(filter.ContainsAddress(new IPv4Filter(192, 168, 14, 0, 2, 2)), Is.False);
        Assert.That(filter.ContainsAddress(new IPv4Filter(192, 168, 14, 0, 3, 3)));
        Assert.That(filter.ContainsAddress(new IPv4Filter(192, 168, 14, 0, 4, 4)));
        Assert.That(filter.ContainsAddress(new IPv4Filter(192, 168, 14, 0, 5, 5)));
        Assert.That(filter.ContainsAddress(new IPv4Filter(192, 168, 14, 0, 6, 6)));
        Assert.That(filter.ContainsAddress(new IPv4Filter(192, 168, 14, 0, 7, 7)), Is.False);

        filter = new IPv4Filter(192, 168, 14, 0, 6, 3, cidrNumber: 24);
        Assert.That(filter.ContainsAddress(new IPv4Filter(192, 168, 14, 0, 2, 2)), Is.False);
        Assert.That(filter.ContainsAddress(new IPv4Filter(192, 168, 14, 0, 3, 3)));
        Assert.That(filter.ContainsAddress(new IPv4Filter(192, 168, 14, 0, 4, 4)));
        Assert.That(filter.ContainsAddress(new IPv4Filter(192, 168, 14, 0, 5, 5)));
        Assert.That(filter.ContainsAddress(new IPv4Filter(192, 168, 14, 0, 6, 6)));
        Assert.That(filter.ContainsAddress(new IPv4Filter(192, 168, 14, 0, 7, 7)), Is.False);
    }

}
