namespace KoalaLang.ParserAndAST.AST
{
    public enum BinOperationType
    {
        Add,
        Subtract,
        Multiply,
        Divide,
        Remain,

        Xor,
        LogicalAnd,
        LogicalOr,

        BitwiseAnd,
        BitwiseOr,
        LeftShift,
        RightShift,

        CmpEqual,
        CmpInequal,
        CmpMore,
        CmpMoreOrEq,
        CmpLess,
        CmpLessOrEq,
    }

    public sealed class ASTBinOperation(ASTNode left, BinOperationType op, ASTNode right, int line) : ASTNode(line)
    {
        internal ASTNode Left = left;
        internal BinOperationType OperationType = op;
        internal ASTNode Right = right;
    }

}
