using SkullLang.Compiler.Analyzers;

namespace SkullLang.Compiler.Parsers.ASTNodes
{
    internal class ASTVariableDecl : ASTNode
    {
        internal string TypeName { get; private set; }
        internal string VarName { get; private set; }

        internal TypeInfo VarType { get; set; }

        internal ASTVariableDecl(string typeName, string varName, ulong ln, ulong col) : base(ln, col)
        {
            TypeName = typeName;
            VarName = varName;
        }
    }
}
