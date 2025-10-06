namespace KoalaLang.ParserAndAST.AST
{
    public enum UnaryOperationType
    {
        Negate, //-x
        LogicalNot,
        BitwiseNot,
    }

    public sealed class ASTUnOperation(UnaryOperationType type, ASTNode op, int line) : ASTNode(line)
    {
        internal ASTNode Operand = op;
        internal UnaryOperationType OperationType = type;
    }
}
