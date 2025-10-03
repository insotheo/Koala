namespace KoalaLang.ParserAndAST.AST
{
    public enum UnaryOperationType
    {
        Negate, //-x
    }

    public sealed class ASTUnOperation(UnaryOperationType type, ASTNode op) : ASTNode
    {
        internal ASTNode Operand = op;
        internal UnaryOperationType OperationType = type;
    }
}
