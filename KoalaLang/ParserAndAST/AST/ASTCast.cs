namespace KoalaLang.ParserAndAST.AST
{
    public sealed class ASTCast(string type, ASTNode val, int line) : ASTNode(line)
    {
        internal string TypeName = type;
        internal ASTNode Value = val;
    }
}
