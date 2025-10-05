namespace KoalaLang.ParserAndAST.AST
{
    public enum BinOperationType
    {
        Add,
        Subtract,
        Multiply,
        Divide,
        Remain,
    }

    public sealed class ASTBinOperation(ASTNode left, BinOperationType op, ASTNode right) : ASTNode
    {
        internal ASTNode Left = left;
        internal BinOperationType OperationType = op;
        internal ASTNode Right = right;
    }

}
