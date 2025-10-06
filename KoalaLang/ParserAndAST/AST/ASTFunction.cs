using System;
using System.Collections.Generic;

namespace KoalaLang.ParserAndAST.AST
{
    public sealed class ASTFunction() : ASTNode
    {
        internal string FunctionName = String.Empty;
        internal string ReturnTypeName = String.Empty;
        internal Dictionary<string, string> Args = new Dictionary<string, string>(); //name - type
        internal ASTCodeBlock Body = new();
    }
}
