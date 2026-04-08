namespace KoalaLang.Compiler.Parsers.ASTNodes
{
    internal class ASTWhileLoop : ASTNode
    {
        internal ASTNode LoopCond { get; set; }
        internal ASTCodeBlock Body { get; set; }

        internal ASTWhileLoop(ASTNode loopCond, ASTCodeBlock body, ulong ln, ulong col) : base(ln, col)
        {
            LoopCond = loopCond;
            Body = body;
        }
    }
}
