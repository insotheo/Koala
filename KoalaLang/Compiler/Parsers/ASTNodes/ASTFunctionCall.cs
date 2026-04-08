using System.Collections.Generic;

namespace KoalaLang.Compiler.Parsers.ASTNodes
{
    internal class ASTFunctionCall : ASTNode
    {
        internal string FunctionName { get; private set; }
        internal string FunctionUName { get; set; }

        internal List<ASTNode> Args { get; private set; }

        internal ASTFunctionCall(string funcName, List<ASTNode> args, ulong ln, ulong col) : base(ln, col)
        {
            FunctionName = funcName;
            Args = args;
        }
    }
}
