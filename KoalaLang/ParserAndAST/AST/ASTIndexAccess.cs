namespace KoalaLang.ParserAndAST.AST
{
    internal class ASTIndexAccess(ASTNode target, ASTNode idx, int line) : ASTNode(line)
    {
        internal ASTNode Target = target;
        internal ASTNode Index = idx;
    }
}
