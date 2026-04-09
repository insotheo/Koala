using KoalaLang.Compiler.Parsers.ASTNodes;
using System.Collections.Generic;

namespace KoalaLang.Compiler.Analyzers
{
    internal class FunctionsHandler
    {
        internal Dictionary<string, List<FunctionInfo>> Functions { get; set; }

        internal FunctionsHandler() => Functions = new();

        internal bool Contains(string funcName) => Functions.ContainsKey(funcName);
        internal void AddFunction(string funcName, FunctionInfo func)
        {
            if (!Contains(funcName)) Functions.Add(funcName, new());
            Functions[funcName].Add(func);
        }

        internal FunctionInfo? GetFunctionBySignature(string funcName, List<VariableInfo> signature)
        {
            foreach (FunctionInfo info in Functions[funcName])
            {
                if (IsSameSignature(info.Args, signature))
                    return info;
            }
            return null;
        }

        internal static FunctionInfo ParseFunctionInfo(Context ctx, ASTFunction funcNode)
        {
            List<VariableInfo> args = new();

            foreach ((string typeName, string argName) in funcNode.Args)
                args.Add(new(argName, new TypeInfo(typeName, ctx: ctx, node: funcNode)));

            if(funcNode.IsMethod)
                return new FunctionInfo(funcNode.FuncName, funcNode.RetType, args, methodOf: funcNode.MethodOf);
            return new FunctionInfo(funcNode.FuncName, funcNode.RetType, args);
        }

        internal static bool IsSameSignature(List<VariableInfo> signA, List<VariableInfo> signB)
        {
            if (signA.Count != signB.Count)
                return false;

            for (int i = 0; i < signA.Count; i++)
            {
                if (!signA[i].Type.Cmp(signB[i].Type))
                    return false;
            }

            return true;
        }
    }
}
