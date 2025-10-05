using KoalaLang.ParserAndAST;
using KoalaLang.ParserAndAST.AST;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

namespace KoalaLang.Translators
{
    public sealed class ILTranslator(Parser parser, string moduleName, string asmName)
    {
        Parser _parser = parser;
        string _moduleName = moduleName;
        string _asmName = asmName;

        public void Translate()
        {
            AssemblyName assemblyName = new AssemblyName(_asmName);
            AssemblyBuilder assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.RunAndCollect);

            ModuleBuilder moduleBuilder = assemblyBuilder.DefineDynamicModule("ApplicationDynamicModule");

            TypeBuilder typeBuilder = moduleBuilder.DefineType(_moduleName, TypeAttributes.Public | TypeAttributes.Class);

            foreach(ASTNode node in (_parser.GetAST() as ASTCodeBlock).Nodes)
            {
                if (node is ASTFunction func)
                {
                    MethodBuilder methodBuilder = TranslateFunction(typeBuilder, func);
                    ILGenerator il = methodBuilder.GetILGenerator();
                    TranslateBody(il, func.Body);
                    il.Emit(OpCodes.Ret);
                }
            }

            Type module = typeBuilder.CreateType();

            //DBG
            Console.WriteLine($"Function Main returned: {module.GetMethod("Main")!.Invoke(null, null)}");
        }

        private void TranslateBody(ILGenerator il, ASTCodeBlock body)
        {
            Dictionary<string, LocalBuilder> varStack = new Dictionary<string, LocalBuilder>();

            foreach(ASTNode node in body.Nodes)
            {
                if(node is ASTReturn ret)
                {
                    TranslateExpression(il, ret.ReturnValue, varStack);
                    il.Emit(OpCodes.Ret);
                }
                else if(node is ASTVariableDeclaration varDecl)
                {
                    LocalBuilder localVariable = il.DeclareLocal(GetTypeByName(varDecl.Type));
                    varStack.Add(varDecl.Name, localVariable);
                }
                else if(node is ASTAssignment assignment)
                {
                    TranslateExpression(il, assignment.Value, varStack);
                    il.Emit(OpCodes.Stloc, varStack[assignment.DestinationName]);
                }
            }
        }

        private MethodBuilder TranslateFunction(TypeBuilder typeBuilder, ASTFunction functionInfo)
        {
            MethodBuilder methodBuilder = typeBuilder.DefineMethod(
                functionInfo.FunctionName,
                MethodAttributes.Public | MethodAttributes.Static,
                GetTypeByName(functionInfo.ReturnTypeName),
                Type.EmptyTypes
                );


            return methodBuilder;
        }

        private void TranslateExpression(ILGenerator il, ASTNode expr, Dictionary<string, LocalBuilder> varStack)
        {
            if (expr is ASTConstant<int> intConst) il.Emit(OpCodes.Ldc_I4, intConst.Value);
            else if (expr is ASTConstant<float> floatConst) il.Emit(OpCodes.Ldc_R4, floatConst.Value);

            else if(expr is ASTVariableUse varUse)
            {
                if (!varStack.ContainsKey(varUse.VariableName))
                {
                    throw new Exception($"Cannot use undefined variable '{varUse.VariableName}'!");
                }
                il.Emit(OpCodes.Ldloc, varStack[varUse.VariableName]);
            }

            else if (expr is ASTBinOperation binOp)
            {
                TranslateExpression(il, binOp.Left, varStack);
                TranslateExpression(il, binOp.Right, varStack);

                switch (binOp.OperationType)
                {
                    case BinOperationType.Add: il.Emit(OpCodes.Add); break;
                    case BinOperationType.Subtract: il.Emit(OpCodes.Sub); break;
                    case BinOperationType.Multiply: il.Emit(OpCodes.Mul); break;
                    case BinOperationType.Divide: il.Emit(OpCodes.Div); break;

                    default: throw new Exception($"Unknown binary operation: {binOp.OperationType}");
                }
            }

            else if (expr is ASTUnOperation unOp)
            {
                TranslateExpression(il, unOp.Operand, varStack);

                switch (unOp.OperationType)
                {
                    case UnaryOperationType.Negate: il.Emit(OpCodes.Neg); break;

                    default: throw new Exception($"Unknown unary operation: {unOp.OperationType}");
                }
            }

            else throw new Exception($"Unknown expression type: {expr.GetType().Name}");
        }

        private Type GetTypeByName(string typeName) {
            return typeName switch
            {
                "int" => typeof(int),
                "float" => typeof(float),
                _ => throw new Exception($"Unknown type: {typeName}"),
            };
        }
    }
}
