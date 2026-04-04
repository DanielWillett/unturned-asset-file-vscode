using AssetsTools.NET;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Values;

public class UnityTransform
{
    private readonly UnityTransform? _parent;
    private readonly UnityObject _rootObject;
    private readonly AssetTypeValueField _transformBaseField;

    public UnityTransform(UnityTransform? parent, UnityObject rootObject, AssetTypeValueField transformBaseField)
    {
        _parent = parent;
        _rootObject = rootObject;
        _transformBaseField = transformBaseField;
    }


}
