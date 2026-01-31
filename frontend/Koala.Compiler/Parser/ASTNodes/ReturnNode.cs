namespace Koala.Compiler.Parser.ASTNodes;

internal class ReturnNode : INode
{
    internal INode Expression { get; private set; }

    public ReturnNode(INode expr) => Expression = expr;

}