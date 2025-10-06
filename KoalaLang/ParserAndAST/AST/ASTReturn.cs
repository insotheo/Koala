namespace KoalaLang.ParserAndAST.AST
{
    public sealed class ASTReturn(ASTNode @return, int line) : ASTNode(line)
    {
        internal ASTNode ReturnValue = @return;
    }
}
