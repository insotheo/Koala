using System.Collections.Generic;

namespace Koala.Compiler.Parser.ASTNodes;

internal class BodyNode : INode
{
    internal List<INode> Body { get; private set; }

    public BodyNode(List<INode> body) => Body = body;
}