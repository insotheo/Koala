namespace SkullLang.Compiler.Parsers.ASTNodes
{

    internal enum BinaryOpType
    {
        None,
        Add, Sub, Mul, Div, Mod
    }

    internal enum UnaryOpType
    {
        Neg,
    }

    internal class ASTBinaryOp : ASTNode
    {
        internal ASTNode LHS { get; private set; }
        internal ASTNode RHS { get; private set; }
        internal BinaryOpType Type { get; private set; }

        internal ASTBinaryOp(ASTNode lhs, ASTNode rhs, BinaryOpType op, ulong ln, ulong col) : base(ln, col)
        {
            LHS = lhs;
            RHS = rhs;
            Type = op;
        }
    }

    internal class ASTUnaryOp : ASTNode
    {
        internal ASTNode HS { get; private set; }
        internal UnaryOpType Type { get; private set; }

        internal ASTUnaryOp(ASTNode hS, UnaryOpType op, ulong ln, ulong col) : base(ln, col)
        {
            HS = hS;
            Type = op;
        }
    }
}
