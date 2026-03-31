using SkullLang.Compiler.Parsers.ASTNodes;
using System.Collections.Generic;

namespace SkullLang.Compiler.Analyzers
{
    internal static class StaticAnalyzer
    {
        static TypeInfo RecognizeType(Context ctx, ASTNode node)
        {
            if (node is ASTConstantInt) return new(null, TypeKind.Integer);
            if (node is ASTConstantFloat) return new(null, TypeKind.Float);

            if(node is ASTFunctionCall funcCall)
            {
                if (!ctx.IsFunctionInCurrentContext(funcCall.FunctionName))
                {
                    ctx.Panic($"Function {funcCall.FunctionName} is used, but never declared", funcCall.Ln, funcCall.Col);
                    return new(null, TypeKind.None);
                }
                string retTypeName = ctx.GetFunction(ctx.CurrentFileName, funcCall.FunctionName).ReturnType;
                return new(retTypeName, TypeInfo.GetKindBasedOnTypeName(retTypeName, ctx));
            }

            if (node is ASTBinaryOp binOp)
            {
                var lhsType = RecognizeType(ctx, binOp.LHS);
                var rhsType = RecognizeType(ctx, binOp.RHS);

                if (lhsType.Kind != rhsType.Kind) ctx.Panic("Cannot operate on different types!", node.Ln, node.Col);
                binOp.OpType = lhsType;

                return lhsType;
            }
            if (node is ASTUnaryOp unOp)
            {
                var hsType = RecognizeType(ctx, unOp.HS);
                unOp.OpType = hsType;

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

        internal static void AnalyzeTree(Context ctx, string fileName, IReadOnlyList<ASTNode> tree)
        {
            foreach(ASTNode node in tree)
            {
                if (node is ASTFunction funcNode)
                {
                    ctx.SetContext(fileName, ctx.GetFunction(fileName, funcNode.FuncName));
                    AnalyzeFunction(ctx, funcNode);
                }
            }
        }

        static void AnalyzeFunction(Context ctx, ASTFunction funcNode)
        {
            //TODO: arguments to locals(with ctx copy)

            foreach(ASTNode node in funcNode.Body.Nodes)
            {
                if(node is ASTReturn retNode)
                {
                    TypeInfo retType = RecognizeType(ctx, retNode);
                    if (!retType.CmpKinds(RecognizeType(ctx, funcNode)))
                        ctx.Panic($"Function('{funcNode.FuncName}') return type doesn't match with it's return value", retNode.Ln);
                }
            }
        }
    }
}
