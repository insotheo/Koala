namespace KoalaLang.ParserAndAST.AST
{
    public sealed class ASTAssignment(ASTNode dest, ASTNode val, int line) : ASTNode(line)
    {
        internal ASTNode Destination = dest;
        internal ASTNode Value = val;
    }
}
