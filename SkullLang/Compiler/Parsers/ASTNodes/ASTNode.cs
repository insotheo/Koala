namespace SkullLang.Compiler.Parsers.ASTNodes
{
    public abstract class ASTNode
    {
        internal ulong Ln { get; init; }
        internal ulong Col { get; init; }

        internal ASTNode(ulong ln, ulong col)
        {
            Ln = ln;
            Col = col;
        } 
    }
}
