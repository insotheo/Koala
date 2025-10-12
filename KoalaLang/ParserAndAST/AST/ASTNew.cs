using System.Collections.Generic;

namespace KoalaLang.ParserAndAST.AST
{
    public sealed class ASTNew(string typeName, List<ASTNode> args, int line) : ASTNode(line)
    {
        internal string TypeName = typeName;
        internal ASTNode[] Args = args.ToArray();
    }
}
