using System.Collections.Generic;
using SkullLang.Compiler.Analyzers;

namespace SkullLang.Compiler.Parsers.ASTNodes
{
    internal class ASTFunction : ASTNode
    {
        internal string FuncName { get; private set; }
        internal string RetType { get; private set; }
        internal List<(string typeName, string argName)> Args { get; private set; } 
        internal ASTCodeBlock Body { get; private set; }

        internal TypeInfo? FuncType;

        internal ASTFunction(string functionName, string @return, List<(string typeName, string argName)> args, ASTCodeBlock body, ulong ln, ulong col) : base(ln, col)
        {
            FuncName = functionName;
            RetType = @return;
            Args = args;
            Body = body;
        }
    }
}