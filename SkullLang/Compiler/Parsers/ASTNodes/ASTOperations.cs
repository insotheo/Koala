namespace SkullLang.Compiler.Parsers.ASTNodes
{

    internal enum BinaryOpType
    {
        None,
        Add, Sub, Mul, Div, Mod,
        BitwiseAnd, BitwiseOr, BitwiseXor, BitwiseLShift, BitwiseRShift,
    }

    internal enum UnaryOpType
    {
        Neg, BitwiseNot, Reference, DeferencingPtr,
    }

    
    internal static class OperationsToStringStaticClass
    {
        internal static string BinaryOpToString(BinaryOpType op) => op switch
        {
            BinaryOpType.Add => "+",
            BinaryOpType.Sub => "-",
            BinaryOpType.Mul => "*",
            BinaryOpType.Div => "/",
            BinaryOpType.Mod => "%",

            BinaryOpType.BitwiseAnd => "&",
            BinaryOpType.BitwiseOr => "|",
            BinaryOpType.BitwiseXor => "^",
            BinaryOpType.BitwiseLShift => "<<",
            BinaryOpType.BitwiseRShift => ">>",

            _ => " "
        };

        internal static string UnaryOpToStirng(UnaryOpType op) => op switch
        {
            UnaryOpType.Neg => "-",
            UnaryOpType.BitwiseNot => "~",
            UnaryOpType.Reference => "&",
            UnaryOpType.DeferencingPtr => "*",

            _ => " ",
        };
    }


    internal class ASTBinaryOp : ASTNode
    {
        internal ASTNode LHS { get; private set; }
        internal ASTNode RHS { get; private set; }
        internal BinaryOpType Op { get; private set; }

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

        internal ASTUnaryOp(ASTNode hS, UnaryOpType op, ulong ln, ulong col) : base(ln, col)
        {
            HS = hS;
            Op = op;
        }
    }
}
