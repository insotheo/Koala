using System.Collections.Generic;

namespace KoalaLang.ParserAndAST.AST
{
    public sealed class ASTCodeBlock(int line) : ASTNode(line)
    {
        internal List<ASTNode> Nodes = new List<ASTNode>();
    }
}
