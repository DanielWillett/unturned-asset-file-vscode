using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

[assembly: ComVisible(false)]
[assembly: Guid("26624efc-f411-43b7-a720-3e786e67081a")]
#if STRONG_NAME
[assembly: InternalsVisibleTo(
    "UnturnedAssetSpecTests, PublicKey=" +
    "00240000048000009400000006020000002400005253413100040000010001007dab5e70907895" + 
    "821562ca105e18d77ad728195896d8c6a229efe7bf551df54d8aea6b33d324c8e2c6b407265049" + 
    "4891a9f7afbc560750ff48606de05b22c74f7b48aa122465dd3a75ea33f9c7d4ddb08824c875a7" + 
    "a3a3e48452dc074189cb853c1d40d761eaa1ef942733f185e1c02a831c3de530d9db87e57082610bfc71ad"
)]
#else
[assembly: InternalsVisibleTo("UnturnedAssetSpecTests")]
#endif

[module: SkipLocalsInit]