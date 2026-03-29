namespace SkullLang.Compiler.Parsers.ASTNodes
{
    internal class ASTConstantInt : ASTNode
    {
        long _val;
        internal long Value => _val;

        internal ASTConstantInt(long val, ulong ln, ulong col) : base(ln, col) => _val = val;
    }

    internal class ASTConstantFloat : ASTNode
    {
        double _val;
        internal double Value => _val;

        internal ASTConstantFloat(double val, ulong ln, ulong col) : base(ln, col) => _val = val;
    }
}