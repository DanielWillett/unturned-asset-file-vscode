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
//[assembly: InternalsVisibleTo(
//    "DanielWillett.MathMatrix, PublicKey=" +
//    "0024000004800000940000000602000000240000525341310004000001000100210ade0a4f9d7c" +
//    "0e45858accee539ef7eee3cd54be2c12b3d3111c9e7d26cf7a016e2bbca6e6129058720c99bbe1" +
//    "19be5266b54726d0ff3a52c50370af1b8f66c23fb85ba46474dfd16574a7e120b1797ff3ce44aa" +
//    "119ab6868dca42c98f5c6d022b083986571d8e20559cd3d94d364cc344e3f9ab1fa8afee32084b2f8de6ab"
//)]
#else
[assembly: InternalsVisibleTo("UnturnedAssetSpecTests")]
//[assembly: InternalsVisibleTo("DanielWillett.MathMatrix")]
#endif

[module: SkipLocalsInit]