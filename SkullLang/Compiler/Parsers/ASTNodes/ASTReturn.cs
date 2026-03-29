namespace SkullLang.Compiler.Parsers.ASTNodes
{
    internal class ASTReturn : ASTNode
    {
        internal ASTNode Ret { get; private set; }
        
        internal ASTReturn(ASTNode ret, ulong ln, ulong col) : base(ln, col) => Ret = ret;
    }
}