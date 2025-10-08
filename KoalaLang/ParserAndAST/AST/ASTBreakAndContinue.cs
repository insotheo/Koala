namespace KoalaLang.ParserAndAST.AST
{
    public sealed class ASTBreak(int line): ASTNode(line) {}
    public sealed class ASTContinue(int line): ASTNode(line) {}
}
