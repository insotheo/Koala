using System;
using System.Collections.Generic;

namespace KoalaLang.ParserAndAST.AST
{
    public sealed class ASTFunction(int line) : ASTNode(line)
    {
        internal string FunctionName = String.Empty;
        internal string ReturnTypeName = String.Empty;
        internal Dictionary<string, string> Args = new Dictionary<string, string>(); //name - type
        internal List<string> GenericTypes = new List<string>();
        internal ASTCodeBlock Body = new(line);
    }
}
