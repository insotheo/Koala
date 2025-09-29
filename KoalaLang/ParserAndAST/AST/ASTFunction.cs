using System;

namespace KoalaLang.ParserAndAST.AST
{
    public sealed class ASTFunction() : ASTNode
    {
        internal string FunctionName = String.Empty;
        internal string ReturnTypeName = String.Empty;
        internal ASTCodeBlock Body = new();
    }
}
