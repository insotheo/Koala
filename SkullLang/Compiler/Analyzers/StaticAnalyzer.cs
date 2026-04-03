using SkullLang.Compiler.Parsers.ASTNodes;
using System.Collections.Generic;
using static SkullLang.Compiler.Parsers.ASTNodes.OperationsToStringStaticClass;

namespace SkullLang.Compiler.Analyzers
{
    internal static class StaticAnalyzer
    {
        static (ASTNode newNode, TypeInfo typeInfo) RecognizeType(Context ctx, ASTNode node)
        {
            if (node is ASTConstantInt) return (node, new(null, TypeKind.Integer, isLiteral: true));
            if (node is ASTConstantFloat) return (node, new(null, TypeKind.Float, isLiteral: true));

            if (node is ASTVariableDecl varDeclNode)
            {
                if (!ctx.IsVariableInScope(varDeclNode.VarName)) return (node, new(null, TypeKind.None));

                TypeInfo varDeclType = ctx.GetVariableType((node as ASTVariableDecl).VarName);
                varDeclNode.VarType = varDeclType;
                return (node, varDeclType);
            }

            if(node is ASTIdentifier identifier)
            {
                string varName = identifier.Identifier;

                if (!ctx.IsVariableInScope(varName))
                {
                    ctx.Panic($"Use of undeclared variable '{varName}'", node.Ln, node.Col);
                    return (node, new(null, TypeKind.None));
                }

                var varTypeInfo = ctx.GetVariableType(varName);

                if(varTypeInfo.Kind == TypeKind.Reference)
                {
                    ASTNode lowerNode = identifier.WasDefered ? identifier : new ASTUnaryOp(identifier, UnaryOpType.DeferencingPtr, node.Ln, node.Col);
                    identifier.WasDefered = true;

                    string deferencedTypeName = varTypeInfo.TypeName.Substring(0, varTypeInfo.TypeName.Length - 1);
                    return (lowerNode, new TypeInfo(deferencedTypeName, TypeInfo.GetKind(deferencedTypeName, ctx), refInPast: true));
                }

                return (node, varTypeInfo);
            }

            if(node is ASTFunctionCall funcCall)
            {
                if (!ctx.IsFunctionInCurrentContext(funcCall.FunctionName))
                {
                    ctx.Panic($"Call to undeclared function {funcCall.FunctionName}", funcCall.Ln, funcCall.Col);
                    return (node, new(null, TypeKind.None));
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
                            TypeInfo providedType = RecognizeType(ctx, funcCall.Args[i]).typeInfo;

                            if (!requiredType.CmpKinds(providedType))
                            {
                                ctx.Panic($"Argument at index {i} of function '{func.FuncName}' has incorrect type. Expected: '{func.Args[i].Type.ToStringOriginal()}', but got '{providedType.ToStringOriginal()}'", funcCall.Args[i].Ln, funcCall.Args[i].Col);
                            }
                        }
                    }
                }

                string retTypeName = func.ReturnType;
                return (node, new(retTypeName, TypeInfo.GetKind(retTypeName, ctx)));
            }

            if (node is ASTBinaryOp binOp)
            {
                (var lhsNode,var lhsType) = RecognizeType(ctx, binOp.LHS);
                (var rhsNode, var rhsType) = RecognizeType(ctx, binOp.RHS);

                var newBinOp = new ASTBinaryOp(lhsNode, rhsNode, binOp.Op, node.Ln, node.Col);

                if (lhsType.Kind != rhsType.Kind) ctx.Panic($"Operator '{BinaryOpToString(binOp.Op)}' cannot be applied to types '{lhsType.ToStringOriginal()}' and '{rhsType.ToStringOriginal()}'", node.Ln, node.Col);
                
                if (binOp.Op == BinaryOpType.LogicalOr ||
                    binOp.Op == BinaryOpType.LogicalAnd ||
                    binOp.Op == BinaryOpType.BitwiseAnd ||
                    binOp.Op == BinaryOpType.BitwiseOr ||
                    binOp.Op == BinaryOpType.BitwiseLShift ||
                    binOp.Op == BinaryOpType.BitwiseRShift || 
                    binOp.Op == BinaryOpType.BitwiseXor)
                {
                    if (lhsType.Kind != TypeKind.Integer || rhsType.Kind != TypeKind.Integer)
                        ctx.Panic($"Operator '{BinaryOpToString(binOp.Op)}' requires integer operands", node.Ln, node.Col);
                }

                if (binOp.Op == BinaryOpType.GreaterThan ||
                   binOp.Op == BinaryOpType.GreaterOrEqual ||
                   binOp.Op == BinaryOpType.LessThan ||
                   binOp.Op == BinaryOpType.LessOrEqual ||
                   binOp.Op == BinaryOpType.Equal ||
                   binOp.Op == BinaryOpType.Inequal ||
                   binOp.Op == BinaryOpType.LogicalOr ||
                   binOp.Op == BinaryOpType.LogicalAnd
                    ) return (newBinOp, new(null, TypeKind.Integer));

                return (newBinOp, lhsType);
            }
            if (node is ASTUnaryOp unOp)
            {
                (var hsNode, var hsType) = RecognizeType(ctx, unOp.HS);
                var unaryNode = new ASTUnaryOp(hsNode, unOp.Op, node.Ln, node.Col);

                if((unOp.Op == UnaryOpType.BitwiseNot ||
                    unOp.Op == UnaryOpType.Not)
                    && hsType.Kind != TypeKind.Integer)
                    ctx.Panic($"Operator '{UnaryOpToStirng(unOp.Op)}' requires integer operands", node.Ln, node.Col);

                if (unOp.Op == UnaryOpType.DeferencingPtr)
                {
                    if (hsType.Kind != TypeKind.Pointer && !hsType.IsRefInPast) ctx.Panic($"Operator '{UnaryOpToStirng(unOp.Op)}' requires pointer operand", node.Ln, node.Col);
                    else
                    {
                        string deferencedTypeName;
                        if (!hsType.IsRefInPast)
                            deferencedTypeName = hsType.TypeName.Substring(0, hsType.TypeName.Length - 1);
                        else deferencedTypeName = hsType.TypeName;

                        return (unaryNode, new(deferencedTypeName, TypeInfo.GetKind(deferencedTypeName), isLiteral: true));
                    }
                }

                if (unOp.Op == UnaryOpType.Reference)
                {
                    if(hsType.IsLiteral) ctx.Panic("Cannot take reference of literal", node.Ln, node.Col);
                    if(unOp.HS is ASTFunctionCall && hsType.Kind != TypeKind.Pointer) ctx.Panic("Cannot take reference of a function return value", node.Ln, node.Col);

                    hsType.Kind = TypeKind.Reference;
                    return (unaryNode, hsType);
                }

                return (unaryNode, hsType);
            }

            if(node is ASTFunction funcNode)
            {
                if (funcNode.FuncType != null)
                    return (node, (TypeInfo)funcNode.FuncType);

                TypeInfo funcType = new();
                funcType.TypeName = funcNode.RetType;
                funcType.Kind = TypeInfo.GetKind(funcType.TypeName, ctx);

                funcNode.FuncType = funcType;

                return (node, funcType);
            }

            if (node is ASTReturn retNode) return RecognizeType(ctx, retNode.Ret);

            return (node, new(null, TypeKind.None));
        }

        static void AnalyzeVariableDeclaration(Context ctx, ASTVariableDecl varDeclNode)
        {
            ctx.DeclareVariable(varDeclNode.TypeName, varDeclNode.VarName, varDeclNode.Ln, varDeclNode.Col);
            RecognizeType(ctx, varDeclNode);
        }

        static ASTNode AnalyzeNode(Context ctx, ASTNode node)
        {
            (ASTNode newNode, TypeInfo type) = RecognizeType(ctx, node);

            if (node is ASTReturn retNode)
            {
                (ASTNode retExpr, TypeInfo retType) = RecognizeType(ctx, retNode.Ret);
                retNode.Ret = retExpr;

                TypeInfo expectedType = new TypeInfo(ctx.CurrentFunction.ReturnType, TypeInfo.GetKind(ctx.CurrentFunction.ReturnType, ctx));
                if (!retType.CmpKinds(expectedType))
                    ctx.Panic($"Return type mismatch in function '{ctx.CurrentFunction.FuncName}': expected '{expectedType.ToStringOriginal()}', got '{retType.ToStringOriginal()}'", retNode.Ln);

                return retNode;
            }

            else if (node is ASTVariableDecl varDeclNode)
            {
                AnalyzeVariableDeclaration(ctx, varDeclNode);
                return varDeclNode;
            }

            else if (node is ASTAssignment assignNode)
            {
                assignNode.LHS = AnalyzeNode(ctx, assignNode.LHS);
                assignNode.RHS = AnalyzeNode(ctx, assignNode.RHS);

                TypeInfo alhsType = RecognizeType(ctx, assignNode.LHS).typeInfo;
                TypeInfo arhsType = RecognizeType(ctx, assignNode.RHS).typeInfo;

                if (!alhsType.CmpKinds(arhsType)) ctx.Panic($"Type mismatch in assignment: cannot assign '{arhsType.ToStringOriginal()}' to '{alhsType.ToStringOriginal()}'", node.Ln, node.Col);

                return assignNode;
            }

            else if(node is ASTBranch branchNode)
            {
                (var ifCondNode, var ifCondType) = RecognizeType(ctx, branchNode.If.Cond);
                branchNode.If.Cond = ifCondNode;

                if (ifCondType.Kind != TypeKind.Integer) ctx.Panic($"Type mismatch in condition: expected int, but got {ifCondType.ToStringOriginal()}", ifCondNode.Ln, ifCondNode.Col);
                AnalyzeCodeBlock(ctx, branchNode.If.Body);
                
                foreach(ASTIf elseIf in branchNode.ElseIfs)
                {
                    (var elseIfCondNode, var elseIfCondType) = RecognizeType(ctx, elseIf.Cond);
                    elseIf.Cond = elseIfCondNode;

                    if (elseIfCondType.Kind != TypeKind.Integer) ctx.Panic($"Type mismatch in condition: expected int, but got {ifCondType.ToStringOriginal()}", elseIfCondNode.Ln, elseIfCondNode.Col);
                    AnalyzeCodeBlock(ctx, elseIf.Body);
                }

                if (branchNode.Else != null) AnalyzeCodeBlock(ctx, branchNode.Else);

                return branchNode;
            }

            return newNode;
        }
        
        static void AnalyzeCodeBlock(Context ctx, ASTCodeBlock codeBlock)
        {
            Context curBlockContext = ctx.BeginScope();
            for (int i = 0; i < codeBlock.Nodes.Count; i++)
                codeBlock.Nodes[i] = AnalyzeNode(curBlockContext, codeBlock.Nodes[i]);
        }

        internal static void AnalyzeTree(Context ctx, string fileName, List<ASTNode> tree)
        {
            foreach (ASTNode node in tree)
            {
                if (node is ASTFunction funcNode)
                {
                    ctx.SetContext(fileName, ctx.GetFunction(fileName, funcNode.FuncName));
                    AnalyzeCodeBlock(ctx, funcNode.Body);
                }
            }
        }
    }
}
