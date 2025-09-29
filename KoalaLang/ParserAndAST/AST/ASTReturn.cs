namespace KoalaLang.ParserAndAST.AST
{
    public sealed class ASTReturn(ASTNode @return) : ASTNode
    {
        internal ASTNode ReturnValue = @return;
    }
}
