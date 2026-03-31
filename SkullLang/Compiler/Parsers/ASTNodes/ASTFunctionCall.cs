using SkullLang.Compiler.Analyzers;

namespace SkullLang.Compiler.Parsers.ASTNodes
{
    internal class ASTFunctionCall : ASTNode
    {
        internal string FunctionName { get; private set; }

        internal ASTFunctionCall(string funcName, ulong ln, ulong col) : base(ln, col)
        {
            FunctionName = funcName;
        }
    }
}
