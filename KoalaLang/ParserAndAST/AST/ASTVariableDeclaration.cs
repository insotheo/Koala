namespace KoalaLang.ParserAndAST.AST
{
    public sealed class ASTVariableDeclaration(string name, string type, int line) : ASTNode(line)
    {
        internal string Name = name;
        internal string Type = type;
    }
}
