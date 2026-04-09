using System;
using System.Collections.Generic;
using KoalaLang.Compiler.Analyzers;

namespace KoalaLang.Compiler.Parsers.ASTNodes
{
    internal class ASTFunction : ASTNode
    {
        internal string FuncName { get; private set; }
        internal string RetType { get; private set; }

        internal string MethodOf { get; private set; }
        internal bool IsMethod => !String.IsNullOrEmpty(MethodOf);

        internal List<Modifier> Modifiers = new();

        internal List<(string typeName, string argName)> Args { get; private set; } 
        internal ASTCodeBlock Body { get; private set; }

        internal TypeInfo? FuncType;

        internal ASTFunction(string functionName, string @return, List<(string typeName, string argName)> args, ASTCodeBlock body, ulong ln, ulong col, string methodOf = null) : base(ln, col)
        {
            FuncName = functionName;
            RetType = @return;
            Args = args;
            Body = body;
            MethodOf = methodOf;
        }
    }
}