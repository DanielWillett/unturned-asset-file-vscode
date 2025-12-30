using DanielWillett.UnturnedDataFileLspServer.Data.Properties;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Values.Expressions;

internal class PropertyReferenceExpressionNode : IPropertyReferenceExpressionNode
{
    private readonly PropertyReference _propRef;

    public ref readonly PropertyReference Reference => ref _propRef;

    public PropertyReferenceExpressionNode(PropertyReference propRef)
    {
        _propRef = propRef;
    }

    public override string ToString()
    {
        return _propRef.ToString();
    }

    PropertyReference IPropertyReferenceExpressionNode.Reference => _propRef;

    public bool Equals(IExpressionNode? other)
    {
        if ((object?)other == this)
            return true;
        if (other == null)
            return false;
        return other is IPropertyReferenceExpressionNode propRefNode
               && propRefNode.Reference.Equals(_propRef);
    }
}