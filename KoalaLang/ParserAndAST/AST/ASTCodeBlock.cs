using System.Collections.Generic;

namespace KoalaLang.ParserAndAST.AST
{
    public sealed class ASTCodeBlock : ASTNode
    {
        internal List<ASTNode> Nodes;

        public ASTCodeBlock()
        {
            Nodes = new List<ASTNode>();
        }
    }
}
