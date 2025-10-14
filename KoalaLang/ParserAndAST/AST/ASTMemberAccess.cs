namespace KoalaLang.ParserAndAST.AST
{
    public sealed class ASTMemberAccess(ASTNode target, string member, int line) : ASTNode(line)
    {
        internal ASTNode Target = target;
        internal string MemberName = member;
    }
}
