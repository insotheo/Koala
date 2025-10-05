namespace KoalaLang.ParserAndAST.AST
{
    public sealed class ASTVariableDeclaration(string name, string type) : ASTNode
    {
        internal string Name = name;
        internal string Type = type;
    }
}
