namespace KoalaLang.ParserAndAST.AST
{
    public sealed class ASTForLoop(ASTNode decl, ASTNode cond, ASTNode iterAction, ASTCodeBlock body, int line) : ASTNode(line)
    {
        internal ASTNode VariableDeclaration = decl;
        internal ASTNode Condition = cond;
        internal ASTNode IterAction = iterAction;
        internal ASTCodeBlock Body = body;
    }
}
