namespace KoalaLang.ParserAndAST.AST
{
    public sealed class ASTConditionBlock(ASTNode cond, ASTCodeBlock body, int line) : ASTNode(line)
    {
        internal ASTNode Condition = cond;
        internal ASTCodeBlock Body = body;
    }
}
