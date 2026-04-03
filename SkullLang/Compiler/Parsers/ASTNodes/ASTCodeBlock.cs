using System.Collections.Generic;

namespace SkullLang.Compiler.Parsers.ASTNodes
{
    internal class ASTCodeBlock : ASTNode
    {
        List<ASTNode> _nodes;
        internal List<ASTNode> Nodes => _nodes;

        internal ASTCodeBlock(List<ASTNode> nodes, ulong ln, ulong col) : base(ln, col) => _nodes = nodes;
    }
}