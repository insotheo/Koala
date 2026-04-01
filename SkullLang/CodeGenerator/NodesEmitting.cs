using SkullLang.Compiler.Parsers.ASTNodes;
using System.Text;

using static SkullLang.Compiler.Parsers.ASTNodes.OperationsToStringStaticClass;

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
                else if(node is ASTFunctionCall funcCall)
                {
                    EmitExpression(code, funcCall);
                    code.Append(";\n");
                }
            }
        }

        static void EmitExpression(StringBuilder code, ASTNode expr)
        {
            if (expr is ASTConstantInt cInt) code.Append(cInt.Value);
            if (expr is ASTConstantFloat cFloat) code.Append(cFloat.Value);
            if (expr is ASTIdentifier identifier) code.Append(identifier.Identifier); 

            if(expr is ASTBinaryOp binOp)
            {
                code.Append("(");
                EmitExpression(code, binOp.LHS);
                code.Append(BinaryOpToString(binOp.Op));
                EmitExpression(code, binOp.RHS);
                code.Append(")");
            }

            if(expr is ASTUnaryOp unOp)
            {
                code.Append("(");
                code.Append(UnaryOpToStirng(unOp.Op));
                EmitExpression(code, unOp.HS);
                code.Append(")");
            }

            if(expr is ASTFunctionCall funcCall)
            {
                code.Append($"{funcCall.FunctionName}(");

                for(int i = 0; i < funcCall.Args.Count; i++)
                {
                    EmitExpression(code, funcCall.Args[i]);
                    if (i + 1 < funcCall.Args.Count) code.Append(", ");
                }

                code.Append(")");
            }
        }
    }
}
