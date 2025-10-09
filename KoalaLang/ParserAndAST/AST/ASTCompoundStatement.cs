namespace KoalaLang.ParserAndAST.AST
{
    public sealed class ASTCompoundStatement(ASTNode i, ASTNode ii, int line) : ASTNode(line)
    {
        internal ASTNode I = i;
        internal ASTNode II = ii;
    }
}
