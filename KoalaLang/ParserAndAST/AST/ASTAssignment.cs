namespace KoalaLang.ParserAndAST.AST
{
    public sealed class ASTAssignment(string dest, ASTNode val) : ASTNode
    {
        internal string DestinationName = dest;
        internal ASTNode Value = val;
    }
}
