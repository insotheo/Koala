using System.Collections.Generic;

namespace KoalaLang.ParserAndAST.AST
{
    public sealed class ASTFunctionCall(string name, List<ASTNode> args, int line) : ASTNode(line)
    {
        internal string FunctionName = name;
        internal List<ASTNode> Args = args;
    }
}
