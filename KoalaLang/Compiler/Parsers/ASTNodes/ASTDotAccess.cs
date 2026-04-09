namespace KoalaLang.Compiler.Parsers.ASTNodes
{
    internal class ASTDotAccess : ASTNode
    {
        internal ASTNode LHS { get; set; }
        internal ASTNode RHS { get; set; }

        internal bool IsStaticAccess { get; set; }

        internal ASTDotAccess(ASTNode lhs, ASTNode rhs, bool isStatic, ulong ln, ulong col) : base(ln, col)
        {
            LHS = lhs;
            RHS = rhs;
            IsStaticAccess = isStatic;
        }
    }
}
