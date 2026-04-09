namespace KoalaLang.Compiler.Parsers.ASTNodes
{
    internal class ASTIndexing : ASTNode
    {
        internal ASTNode Source { get; set; }
        internal ASTNode Index { get; set; }

        internal ASTIndexing(ASTNode source, ASTNode index, ulong ln, ulong col) : base(ln, col)
        {
            Source = source;
            Index = index;
        }
    }
}
