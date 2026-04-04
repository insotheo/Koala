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
                if (node is ASTReturn retNode)
                {
                    code.Append("return ");
                    EmitExpression(code, retNode.Ret);
                    code.Append(";\n");
                }
                else if (node is ASTFunctionCall funcCall)
                {
                    EmitExpression(code, funcCall);
                    code.Append(";\n");
                }
                else if(node is ASTBranch branch)
                {
                    code.Append("if(");
                    EmitExpression(code, branch.If.Cond);
                    code.Append("){\n");
                    EmitCodeBlock(code, branch.If.Body);
                    code.Append("}\n");

                    foreach(ASTIf elseIf in branch.ElseIfs)
                    {
                        code.Append("else if(");
                        EmitExpression(code, elseIf.Cond);
                        code.Append("){\n");
                        EmitCodeBlock(code, elseIf.Body);
                        code.Append("}\n");
                    }

                    if(branch.Else != null)
                    {
                        code.Append("else{\n");
                        EmitCodeBlock(code, branch.Else);
                        code.Append("}\n");
                    }
                }
                else if(node is ASTWhileLoop whileLoop)
                {
                    code.Append("while(");
                    EmitExpression(code, whileLoop.LoopCond);
                    code.Append("){\n");
                    EmitCodeBlock(code, whileLoop.Body);
                    code.Append("}\n");
                }
                else
                {
                    EmitExpression(code, node, insertParens: false);
                    code.Append(";\n");
                }
            }
        }

        static void EmitExpression(StringBuilder code, ASTNode expr, bool insertParens = true)
        {
            if (expr is ASTConstantInt cInt) code.Append(cInt.Value);
            if (expr is ASTConstantFloat cFloat) code.Append(cFloat.Value);
            if (expr is ASTIdentifier identifier) code.Append(identifier.Identifier); 

            if(expr is ASTBinaryOp binOp)
            {
                if(insertParens) code.Append("(");
                EmitExpression(code, binOp.LHS);
                code.Append(BinaryOpToString(binOp.Op));
                EmitExpression(code, binOp.RHS);
                if (insertParens) code.Append(")");
            }

            if(expr is ASTUnaryOp unOp)
            {
                if (insertParens) code.Append("(");
                code.Append(UnaryOpToStirng(unOp.Op));
                EmitExpression(code, unOp.HS);
                if (insertParens) code.Append(")");
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

            if(expr is ASTVariableDecl varDecl)
            {
                code.Append($"{varDecl.VarType.TypeName} {varDecl.VarName}");
            }

            if(expr is ASTAssignment assignNode)
            {
                EmitExpression(code, assignNode.LHS, insertParens: false);
                code.Append("=");
                EmitExpression(code, assignNode.RHS);
            }
        }
    }
}
