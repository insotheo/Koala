using System.Collections.Generic;

namespace KoalaLang.ParserAndAST.AST
{
    public sealed class ASTFunctionCall(string name, List<ASTNode> args, List<string> generic, int line) : ASTNode(line)
    {
        internal string FunctionName = name;
        internal List<ASTNode> Args = args;
        internal List<string> GenericTypes = generic;
    }
}
