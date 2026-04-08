using KoalaLang.Compiler.Analyzers;

namespace KoalaLang.Compiler.Parsers.ASTNodes
{
    internal class ASTCast : ASTNode
    {
        internal ASTNode LHS { get; set; }
        internal string TypeName { get; set; }

        internal TypeInfo? ResultType { get; set; }

        internal ASTCast(ASTNode lhs, string typeName, ulong ln, ulong col) : base(ln, col)
        {
            LHS = lhs;
            TypeName = typeName;
        }
    }
}
