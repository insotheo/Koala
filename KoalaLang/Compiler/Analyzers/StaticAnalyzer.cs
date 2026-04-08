using KoalaLang.Compiler.Parsers.ASTNodes;
using System.Collections.Generic;

using static KoalaLang.Compiler.Parsers.ASTNodes.OperationsToStringStaticClass;

namespace KoalaLang.Compiler.Analyzers
{
    internal static class StaticAnalyzer
    {
        static (ASTNode newNode, TypeInfo typeInfo) RecognizeType(Context ctx, ASTNode node)
        {
            if (node is ASTConstantInt) return (node, new("int", isLiteral: true));
            if (node is ASTConstantFloat) return (node, new("float", isLiteral: true));
            if (node is ASTConstantBoolean) return (node, new("bool", isLiteral: true));
            if (node is ASTConstantString) return (node, new("byte*", isLiteral: true)); //TODO: special type for strings

            if (node is ASTVariableDecl varDeclNode)
            {
                if (!ctx.IsVariableInScope(varDeclNode.VarName))
                    return (node, new(null));

                TypeInfo varDeclType = ctx.GetVariableType((node as ASTVariableDecl).VarName);
                varDeclNode.VarType = varDeclType;
                return (node, varDeclType);
            }

            if (node is ASTIdentifier identifier)
            {
                string varName = identifier.Identifier;

                if (!ctx.IsVariableInScope(varName))
                {
                    ctx.Panic($"Use of undeclared variable '{varName}'", node.Ln, node.Col);
                    return (node, new(null));
                }

                var varTypeInfo = ctx.GetVariableType(varName);

                //ref auto deref
                if (varTypeInfo.IsReference)
                {
                    if (!identifier.WasDerefered)
                    {
                        identifier.WasDerefered = true;
                        ASTNode derefNode = new ASTUnaryOp(identifier, UnaryOpType.DereferencingPtr, node.Ln, node.Col);

                        var newType = varTypeInfo.Clone();
                        newType.Modifiers.RemoveAt(0);
                        newType.Rebuild(ctx, identifier);

                        return (derefNode, newType);
                    }
                }

                return (node, varTypeInfo);
            }

            if (node is ASTFunctionCall funcCall)
            {
                List<FunctionInfo> candidates = ctx.GetFunctions(ctx.CurrentFileName, funcCall.FunctionName);
                FunctionInfo? funcInfo = null;
                foreach(var func in candidates)
                {
                    if (func.IsExtern) //extern functions are unsafe
                    {
                        funcInfo = func;
                        break;
                    }

                    if (funcCall.Args.Count != func.Args.Count)
                        continue;

                    bool ok = true;

                    for(int i = 0; i < funcCall.Args.Count; i++)
                    {
                        var provided = RecognizeType(ctx, funcCall.Args[i]).typeInfo;

                        if (!func.Args[i].Type.Cmp(provided))
                        {
                            ok = false;
                            continue;
                        }
                    }

                    if (ok)
                    {
                        if(funcInfo != null)
                        {
                            ctx.Panic($"Ambiguous call to '{funcCall.FunctionName}'", funcCall.Ln, funcCall.Col);
                            return (node, new(null));
                        }

                        funcInfo = func;
                    }
                }
                if(funcInfo == null)
                {
                    ctx.Panic($"Call to undeclared function '{funcCall.FunctionName}'", funcCall.Ln, funcCall.Col);
                    return (node, new(null));
                }

                funcCall.FunctionUName = funcInfo.Value.FuncUName;

                foreach (ASTNode arg in funcCall.Args)
                    RecognizeType(ctx, arg);

                return (funcCall, funcInfo.Value.ReturnType.Clone());
            }

            if (node is ASTBinaryOp binOp)
            {
                (var lhsNode, var lhsType) = RecognizeType(ctx, binOp.LHS);
                (var rhsNode, var rhsType) = RecognizeType(ctx, binOp.RHS);

                var newBinOp = new ASTBinaryOp(lhsNode, rhsNode, binOp.Op, node.Ln, node.Col);

                if (lhsType.Kind != rhsType.Kind)
                    ctx.Panic($"Operator '{BinaryOpToString(binOp.Op)}' cannot be applied to types '{lhsType.ToStringOriginal()}' and '{rhsType.ToStringOriginal()}'", node.Ln, node.Col);

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
                    ) return (newBinOp, new("int"));

                return (newBinOp, lhsType);
            }

            if (node is ASTUnaryOp unOp)
            {
                (var hsNode, var hsType) = RecognizeType(ctx, unOp.HS);
                var unaryNode = new ASTUnaryOp(hsNode, unOp.Op, node.Ln, node.Col);

                if ((unOp.Op == UnaryOpType.BitwiseNot ||
                    unOp.Op == UnaryOpType.Not)
                    && hsType.Kind != TypeKind.Integer)
                    ctx.Panic($"Operator '{UnaryOpToStirng(unOp.Op)}' requires integer operands", node.Ln, node.Col);

                if (unOp.Op == UnaryOpType.DereferencingPtr)
                {
                    if (hsType.Modifiers.Count == 0 || (!hsType.IsPointer && !hsType.IsReference))
                        ctx.Panic("Cannot dereference non-pointer", node.Ln, node.Col);
                    else
                    {
                        var newType = hsType.Clone();
                        newType.Modifiers.RemoveAt(0); //deref by removing &
                        newType.Rebuild();

                        return (unaryNode, newType);
                    }
                }

                if (unOp.Op == UnaryOpType.Reference)
                {
                    if (hsType.IsLiteral)
                        ctx.Panic("Cannot take reference of literal", node.Ln, node.Col);

                    if (unOp.HS is ASTFunctionCall && !hsType.IsPointer)
                        ctx.Panic("Cannot take reference of a function return value", node.Ln, node.Col);

                    var newType = hsType.Clone();
                    newType.Modifiers.Insert(0, TypeModifier.Reference);
                    newType.Rebuild(ctx, unOp);

                    return (unaryNode, newType);
                }

                return (unaryNode, hsType);
            }

            if (node is ASTFunction funcNode)
            {
                if (funcNode.FuncType != null)
                    return (node, (TypeInfo)funcNode.FuncType);

                TypeInfo funcType = new(funcNode.RetType, ctx: ctx, node: funcNode);

                funcNode.FuncType = funcType;

                return (node, funcType);
            }

            if (node is ASTCast castNode)
            {
                if (castNode.ResultType != null)
                    return (node, castNode.ResultType.Value);

                TypeInfo castType = new TypeInfo(castNode.TypeName, ctx: ctx, node: castNode);
                RecognizeType(ctx, castNode.LHS);

                castNode.ResultType = castType;
                return (node, castType);
            }

            if (node is ASTReturn retNode)
                return RecognizeType(ctx, retNode.Ret);

            return (node, new(null));
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

                ref TypeInfo expectedType = ref ctx.CurrentFunction.ReturnType;
                if (!retType.Cmp(expectedType))
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

                if (alhsType.IsReadonly && assignNode.LHS is not ASTVariableDecl)
                    ctx.Panic("read-only variable is not assignable", node.Ln, node.Col);

                if (!alhsType.Cmp(arhsType))
                    ctx.Panic($"Type mismatch in assignment: cannot assign '{arhsType.ToStringOriginal()}' to '{alhsType.ToStringOriginal()}'", node.Ln, node.Col);

                return assignNode;
            }

            else if (node is ASTBranch branchNode)
            {
                (var ifCondNode, var ifCondType) = RecognizeType(ctx, branchNode.If.Cond);
                branchNode.If.Cond = ifCondNode;

                if (ifCondType.Kind != TypeKind.Integer) ctx.Panic($"Type mismatch in condition: expected 'integer', but got {ifCondType.ToStringOriginal()}", ifCondNode.Ln, ifCondNode.Col);
                AnalyzeCodeBlock(ctx, branchNode.If.Body);

                foreach (ASTIf elseIf in branchNode.ElseIfs)
                {
                    (var elseIfCondNode, var elseIfCondType) = RecognizeType(ctx, elseIf.Cond);
                    elseIf.Cond = elseIfCondNode;

                    if (elseIfCondType.Kind != TypeKind.Integer) ctx.Panic($"Type mismatch in condition: expected 'integer', but got {ifCondType.ToStringOriginal()}", elseIfCondNode.Ln, elseIfCondNode.Col);
                    AnalyzeCodeBlock(ctx, elseIf.Body);
                }

                if (branchNode.Else != null) AnalyzeCodeBlock(ctx, branchNode.Else);

                return branchNode;
            }

            else if (node is ASTWhileLoop whileLoopNode)
            {
                (var whileCond, var whileCondType) = RecognizeType(ctx, whileLoopNode.LoopCond);
                whileLoopNode.LoopCond = whileCond;

                if (whileCondType.Kind != TypeKind.Integer) ctx.Panic($"Type mismatch in condition: expected 'integer', but got {whileCondType.ToStringOriginal()}", whileCond.Ln, whileCond.Col);
                AnalyzeCodeBlock(ctx, whileLoopNode.Body);
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
                    List<VariableInfo> signature = new();
                    foreach (var arg in funcNode.Args)
                        signature.Add(new(arg.argName, new(arg.typeName, ctx: ctx, node: funcNode)));

                    ctx.SetContext(fileName, ctx.GetFunctionBySignature(fileName, funcNode.FuncName, signature).Value);
                    AnalyzeCodeBlock(ctx, funcNode.Body);
                }
            }
        }
    }
}
