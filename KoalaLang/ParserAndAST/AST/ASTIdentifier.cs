namespace KoalaLang.ParserAndAST.AST
{
    public sealed class ASTIdentifier(string identifier, int line) : ASTNode(line)
    {
        internal string Identifier = identifier;
    }
}
