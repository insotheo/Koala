namespace KoalaLang.ParserAndAST.AST
{
    public sealed class ASTConstant<T>(T val) : ASTNode
    {
        internal T Value = val;
    }
}
