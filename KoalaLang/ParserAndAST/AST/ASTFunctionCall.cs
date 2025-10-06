using System.Collections.Generic;

namespace KoalaLang.ParserAndAST.AST
{
    public sealed class ASTFunctionCall(string name, List<ASTNode> args) : ASTNode
    {
        internal string FunctionName = name;
        internal List<ASTNode> Args = args;
    }
}
