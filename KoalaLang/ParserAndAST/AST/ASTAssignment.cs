namespace KoalaLang.ParserAndAST.AST
{
    public sealed class ASTAssignment(string dest, ASTNode val, int line) : ASTNode(line)
    {
        internal string DestinationName = dest;
        internal ASTNode Value = val;
    }
}
