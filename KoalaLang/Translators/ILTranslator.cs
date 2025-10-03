using KoalaLang.ParserAndAST;
using KoalaLang.ParserAndAST.AST;
using System;
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

            ModuleBuilder moduleBuilder = assemblyBuilder.DefineDynamicModule($"{_asmName}Module");

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
            foreach(ASTNode node in body.Nodes)
            {
                if(node is ASTReturn ret)
                {
                    TranslateExpression(il, ret.ReturnValue);
                    il.Emit(OpCodes.Ret);
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

        private void TranslateExpression(ILGenerator il, ASTNode expr)
        {
            if(expr is ASTConstant<string> constExpr)
            {
                if(int.TryParse(constExpr.Value, out int intVal))
                {
                    il.Emit(OpCodes.Ldc_I4, intVal);
                }
                else if(float.TryParse(constExpr.Value, out float floatVal))
                {
                    il.Emit(OpCodes.Ldc_R4, floatVal);
                }
                else
                {
                    throw new Exception($"Unknown constant type: {constExpr.Value}");
                }
            }
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
