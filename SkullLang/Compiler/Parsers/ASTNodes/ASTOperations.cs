using SkullLang.Compiler.Analyzers;

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
        internal BinaryOpType Op { get; private set; }
        internal TypeInfo OpType;

        internal ASTBinaryOp(ASTNode lhs, ASTNode rhs, BinaryOpType op, ulong ln, ulong col) : base(ln, col)
        {
            LHS = lhs;
            RHS = rhs;
            Op = op;
        }
    }

    internal class ASTUnaryOp : ASTNode
    {
        internal ASTNode HS { get; private set; }
        internal UnaryOpType Op { get; private set; }
        internal TypeInfo OpType;

        internal ASTUnaryOp(ASTNode hS, UnaryOpType op, ulong ln, ulong col) : base(ln, col)
        {
            HS = hS;
            Op = op;
        }
    }
}
