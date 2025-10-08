namespace KoalaLang.ParserAndAST.AST
{
    public sealed class ASTWhileLoop(ASTNode condition, ASTCodeBlock body, int line) : ASTNode(line)
    {
        internal ASTNode Condition = condition;
        internal ASTCodeBlock Body = body;
    }
}
