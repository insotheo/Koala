namespace Koala.Compiler.Parser.ASTNodes;

internal enum ConstantType
{
    Unknown = 0,
    Int, Float,
}

internal class ConstantNode : INode
{
    internal object Value { get; private set; }
    internal ConstantType Type { get; private set; }

    public ConstantNode(object val)
    {
        Value = val;

        if (val is int) Type = ConstantType.Int;
        else if (val is float) Type = ConstantType.Float;
        else Type = ConstantType.Unknown;
    }
}