namespace KoalaLang.Compiler.Parsers.ASTNodes
{
    internal class ASTAssignment : ASTNode
    {
        internal ASTNode LHS { get; set; }
        internal ASTNode RHS { get; set; }

        internal ASTAssignment(ASTNode lhs, ASTNode rhs, ulong ln = 0, ulong col = 0) : base(ln, col)
        {
            LHS = lhs;
            RHS = rhs;
        }
    }
}
