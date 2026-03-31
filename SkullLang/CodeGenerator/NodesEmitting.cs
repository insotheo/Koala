using SkullLang.Compiler.Parsers.ASTNodes;
using System.Text;

namespace SkullLang.CodeGenerator
{
    internal static class NodesEmitting
    {
        internal static void EmitCodeBlock(StringBuilder code, ASTCodeBlock codeBlock)
        {
            foreach(ASTNode node in codeBlock.Nodes)
            {
                if(node is ASTReturn retNode)
                {
                    code.Append("return ");
                    EmitExpression(code, retNode.Ret);
                    code.Append(";\n");
                }
            }
        }

        static void EmitExpression(StringBuilder code, ASTNode expr)
        {
            if (expr is ASTConstantInt cInt) code.Append(cInt.Value);
            if (expr is ASTConstantFloat cFloat) code.Append(cFloat.Value);

            if(expr is ASTBinaryOp binOp)
            {
                EmitExpression(code, binOp.LHS);
                code.Append(binOp.Op switch
                {
                    BinaryOpType.Add => "+",
                    BinaryOpType.Sub => "-",
                    BinaryOpType.Mul => "*",
                    BinaryOpType.Div => "/",
                    BinaryOpType.Mod => "%",

                    _ => " "
                });
                EmitExpression(code, binOp.RHS);
            }

            if(expr is ASTUnaryOp unOp)
            {
                code.Append(unOp.Op switch
                {
                    UnaryOpType.Neg => "-",
                    _ => " "
                });
                EmitExpression(code, unOp.HS);
            }
        }
    }
}
