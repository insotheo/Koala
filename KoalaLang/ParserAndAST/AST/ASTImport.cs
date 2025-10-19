namespace KoalaLang.ParserAndAST.AST
{
    public sealed class ASTImport(string path, int line) : ASTNode(line)
    {
        internal string Path = path;
    }
}
