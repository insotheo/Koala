using LLVMSharp.Interop;
using SkullLang.Compiler.Analyzers;
using SkullLang.Compiler.Parsers.ASTNodes;
using System;

namespace SkullLang.Compiler.CodeGenerating
{
    internal static class NodesEmit
    {
        internal static LLVMTypeRef GetLLVMType(TypeInfo info) => info.TypeName switch
        {
            "bool" => LLVMTypeRef.Int1,

            "byte" => LLVMTypeRef.Int8,
            "short" => LLVMTypeRef.Int16,
            "int" => LLVMTypeRef.Int32,
            "long" => LLVMTypeRef.Int64,

            "float" => LLVMTypeRef.Float,

            _ => LLVMTypeRef.Void,
        };

        internal static void EmitFunction(LLVMBuilderRef builder, LLVMValueRef function, ASTFunction functionNode)
        {
            var entry = function.AppendBasicBlock("entry");
            builder.PositionAtEnd(entry);

            var retType = function.TypeOf.ReturnType;

            EmitCodeBlock(builder, functionNode.Body);
        }

        internal static void EmitCodeBlock(LLVMBuilderRef builder, ASTCodeBlock codeBlock)
        {
            foreach(ASTNode node in codeBlock.Nodes)
            {
                if(node is ASTReturn retNode)
                {
                    var expr = EmitExpression(builder, retNode.Ret);
                    builder.BuildRet(expr);
                }
            }
        }

        internal static LLVMValueRef EmitExpression(LLVMBuilderRef builder, ASTNode expr)
        {
            if (expr is ASTConstantInt cInt) return LLVMValueRef.CreateConstInt(LLVMTypeRef.Int32, cInt.Value);
            if (expr is ASTConstantFloat cFloat) return LLVMValueRef.CreateConstReal(LLVMTypeRef.Float, cFloat.Value);

            if (expr is ASTBinaryOp binOp)
            {
                var lhs = EmitExpression(builder, binOp.LHS);
                var rhs = EmitExpression(builder, binOp.RHS);

                switch (binOp.Op)
                {
                    case BinaryOpType.Add: return builder.BuildAdd(lhs, rhs);
                    case BinaryOpType.Sub: return builder.BuildSub(lhs, rhs);
                    case BinaryOpType.Mul:
                        {
                            if(binOp.OpType.Kind == TypeKind.Integer)
                                return builder.BuildMul(lhs, rhs);
                            else if(binOp.OpType.Kind == TypeKind.Float)
                                return builder.BuildFMul(lhs, rhs);

                            break;
                        }
                    case BinaryOpType.Div:
                        {
                            if (binOp.OpType.Kind == TypeKind.Integer)
                                return builder.BuildExactSDiv(lhs, rhs);
                            else if (binOp.OpType.Kind == TypeKind.Float)
                                return builder.BuildFDiv(lhs, rhs);

                            break;
                        }
                    case BinaryOpType.Mod:
                        {
                            if (binOp.OpType.Kind == TypeKind.Integer)
                                return builder.BuildSRem(lhs, rhs);
                            else if (binOp.OpType.Kind == TypeKind.Float)
                                return builder.BuildFRem(lhs, rhs);

                            break;
                        }
                }
            }

            if(expr is ASTUnaryOp unOp)
            {
                var hs = EmitExpression(builder, unOp.HS);

                switch (unOp.Op)
                {
                    case UnaryOpType.Neg:
                        {
                            if (unOp.OpType.Kind == TypeKind.Integer)
                                return builder.BuildNeg(hs);
                            else if (unOp.OpType.Kind == TypeKind.Float)
                                return builder.BuildFNeg(hs);

                            break;
                        }
                }
            }

            return null;
        }
    }
}
