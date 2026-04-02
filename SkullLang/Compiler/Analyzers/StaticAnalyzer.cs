using SkullLang.Compiler.Parsers.ASTNodes;
using System.Collections.Generic;

using static SkullLang.Compiler.Parsers.ASTNodes.OperationsToStringStaticClass;

namespace SkullLang.Compiler.Analyzers
{
    internal static class StaticAnalyzer
    {
        static TypeInfo RecognizeType(Context ctx, ASTNode node)
        {
            if (node is ASTConstantInt) return new(null, TypeKind.Integer);
            if (node is ASTConstantFloat) return new(null, TypeKind.Float);

            if (node is ASTVariableDecl varDeclNode)
            {
                TypeInfo varDeclType = ctx.GetVariableType((node as ASTVariableDecl).VarName);
                varDeclNode.VarType = varDeclType;
                return varDeclType;
            }

            if(node is ASTIdentifier identifier)
            {
                string varName = identifier.Identifier;

                if (!ctx.IsVariableInScope(varName))
                {
                    ctx.Panic($"Use of undeclared variable '{varName}'", node.Ln, node.Col);
                    return new(null, TypeKind.None);
                }

                return ctx.GetVariableType(varName);
            }

            if(node is ASTFunctionCall funcCall)
            {
                if (!ctx.IsFunctionInCurrentContext(funcCall.FunctionName))
                {
                    ctx.Panic($"Call to undeclared function {funcCall.FunctionName}", funcCall.Ln, funcCall.Col);
                    return new(null, TypeKind.None);
                }

                FunctionInfo func = ctx.GetFunction(ctx.CurrentFileName, funcCall.FunctionName);

                if (funcCall.Args.Count > 0)
                {
                    if (funcCall.Args.Count != func.Args.Count) ctx.Panic($"Function '{func.FuncName}' expects {func.Args.Count} {(func.Args.Count == 1 ? "argument" : "arguments")}, but {funcCall.Args.Count} {(funcCall.Args.Count == 1 ? "was" : "were")} provided", funcCall.Ln, funcCall.Col);
                    else
                    {
                        for(int i = 0; i < funcCall.Args.Count; i++)
                        {
                            TypeInfo requiredType = func.Args[i].Type;
                            TypeInfo providedType = RecognizeType(ctx, funcCall.Args[i]);

                            if (!requiredType.CmpKinds(providedType))
                            {
                                ctx.Panic($"Argument at index {i} of function '{func.FuncName}' has incorrect type. Expected: '{func.Args[i].Type.ToStringOriginal()}', but got '{providedType.ToStringOriginal()}'", funcCall.Args[i].Ln, funcCall.Args[i].Col);
                            }
                        }
                    }
                }

                string retTypeName = func.ReturnType;
                return new(retTypeName, TypeInfo.GetKindBasedOnTypeName(retTypeName, ctx));
            }

            if (node is ASTBinaryOp binOp)
            {
                var lhsType = RecognizeType(ctx, binOp.LHS);
                var rhsType = RecognizeType(ctx, binOp.RHS);

                if (lhsType.Kind != rhsType.Kind) ctx.Panic($"Operator '{BinaryOpToString(binOp.Op)}' cannot be applied to types '{lhsType.ToStringOriginal()}' and '{rhsType.ToStringOriginal()}'", node.Ln, node.Col);
                
                if (binOp.Op == BinaryOpType.BitwiseAnd ||
                    binOp.Op == BinaryOpType.BitwiseOr ||
                    binOp.Op == BinaryOpType.BitwiseLShift ||
                    binOp.Op == BinaryOpType.BitwiseRShift || 
                    binOp.Op == BinaryOpType.BitwiseXor)
                {
                    if (lhsType.Kind != TypeKind.Integer || rhsType.Kind != TypeKind.Integer)
                        ctx.Panic($"Operator '{BinaryOpToString(binOp.Op)}' requires integer operands", node.Ln, node.Col);
                }

                return lhsType;
            }
            if (node is ASTUnaryOp unOp)
            {
                var hsType = RecognizeType(ctx, unOp.HS);

                if(unOp.Op == UnaryOpType.BitwiseNot && hsType.Kind != TypeKind.Integer) ctx.Panic($"Operator '{UnaryOpToStirng(unOp.Op)}' requires integer operands", node.Ln, node.Col);

                return hsType;
            }

            if(node is ASTFunction funcNode)
            {
                if (funcNode.FuncType != null)
                    return (TypeInfo)funcNode.FuncType;

                TypeInfo funcType = new();
                funcType.TypeName = funcNode.RetType;
                funcType.Kind = TypeInfo.GetKindBasedOnTypeName(funcType.TypeName, ctx);

                funcNode.FuncType = funcType;

                return funcType;
            }

            if (node is ASTReturn retNode) return RecognizeType(ctx, retNode.Ret);

            return new(null, TypeKind.None);
        }

        static void AnalyzeVariableDeclaration(Context ctx, ASTVariableDecl varDeclNode)
        {
            ctx.DeclareVariable(varDeclNode.TypeName, varDeclNode.VarName, varDeclNode.Ln, varDeclNode.Col);
            RecognizeType(ctx, varDeclNode);
        }

        static void AnalyzeNode(Context ctx, ASTNode node)
        {
            if (node is ASTReturn retNode)
            {
                TypeInfo retType = RecognizeType(ctx, retNode);
                TypeInfo expectedType = new TypeInfo(ctx.CurrentFunction.ReturnType, TypeInfo.GetKindBasedOnTypeName(ctx.CurrentFunction.ReturnType, ctx));
                if (!retType.CmpKinds(expectedType))
                    ctx.Panic($"Return type mismatch in function '{ctx.CurrentFunction.FuncName}': expected '{expectedType.ToStringOriginal()}', got '{retType.ToStringOriginal()}'", retNode.Ln);
            }

            else if (node is ASTVariableDecl) AnalyzeVariableDeclaration(ctx, node as ASTVariableDecl);

            else if (node is ASTAssignment assignNode)
            {
                AnalyzeNode(ctx, assignNode.LHS);

                TypeInfo alhsType = RecognizeType(ctx, assignNode.LHS);
                TypeInfo arhsType = RecognizeType(ctx, assignNode.RHS);

                if (!alhsType.CmpKinds(arhsType)) ctx.Panic($"Type mismatch in assignment: cannot assign '{arhsType.ToStringOriginal()}' to '{alhsType.ToStringOriginal()}'");
            }
        }

        static void AnalyzeFunction(Context ctx, ASTFunction funcNode)
        {
            foreach (ASTNode node in funcNode.Body.Nodes)
                AnalyzeNode(ctx, node);
        }

        internal static void AnalyzeTree(Context ctx, string fileName, IReadOnlyList<ASTNode> tree)
        {
            foreach (ASTNode node in tree)
            {
                if (node is ASTFunction funcNode)
                {
                    ctx.SetContext(fileName, ctx.GetFunction(fileName, funcNode.FuncName));
                    AnalyzeFunction(ctx, funcNode);
                }
            }
        }
    }
}
