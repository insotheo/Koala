namespace KoalaLang.ParserAndAST.AST
{
    public sealed class ASTConstant<T>(T val, int line) : ASTNode(line)
    {
        internal T Value = val;
    }
}
