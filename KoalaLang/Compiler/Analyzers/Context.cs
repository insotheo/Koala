using System.Collections.Generic;
using System.Text;

namespace KoalaLang.Compiler.Analyzers
{
    internal sealed class Context
    {
        internal Dictionary<string, Dictionary<string, List<FunctionInfo>>> Functions { get; private set; }
        internal AnalyzerContext Analyzer { get; private set; }

        internal string CurrentFileName;
        internal FunctionInfo CurrentFunction;
        internal Dictionary<string, VariableInfo> CurrentVars;

        internal Context(AnalyzerContext analyzer)
        {
            Functions = new();
            Analyzer = analyzer;
            CurrentVars = new();
        }

        private Context(Context oldCtx)
        {
            Dictionary<string, VariableInfo> NewCurrentVars = new();
            foreach ((string typeName, VariableInfo varInfo) in oldCtx.CurrentVars)
                NewCurrentVars.Add(typeName, varInfo);

            Functions = oldCtx.Functions;
            Analyzer = oldCtx.Analyzer;
            CurrentFileName = oldCtx.CurrentFileName;
            CurrentFunction = oldCtx.CurrentFunction;
            CurrentVars = NewCurrentVars;
        }

        internal void SetContext(string fileName, FunctionInfo funcInfo)
        {
            CurrentFileName = fileName;
            CurrentFunction = funcInfo;
            CurrentVars.Clear();

            foreach(VariableInfo arg in funcInfo.Args)
            {
                CurrentVars.Add(arg.Name, arg);
            }
        }

        internal Context BeginScope() => new Context(this);
        

        internal bool IsVariableInScope(string varName) => CurrentVars.ContainsKey(varName);
        internal VariableInfo GetVariable(string varName) => CurrentVars[varName];
        internal TypeInfo GetVariableType(string varName) => CurrentVars[varName].Type;
        
        internal void DeclareVariable(string typeName, string varName, ulong ln, ulong col)
        {
            if (IsVariableInScope(varName))
            {
                Panic($"Variable '{varName}' is already declared in current scope", ln, col);
                return;
            }

            TypeInfo type = new TypeInfo(typeName, ctx: this, node: new(ln, col));

            CurrentVars.Add(varName, new VariableInfo(varName, type));
        }


        internal void DeclareFunction(string @namespace, FunctionInfo info)
        {
            if (!Functions.ContainsKey(@namespace))
                Functions.Add(@namespace, new());

            if (!Functions[@namespace].ContainsKey(info.FuncName))
                Functions[@namespace].Add(info.FuncName, new());

            foreach(var existing in Functions[@namespace][info.FuncName])
            {
                if(IsSameSignature(existing.Args, info.Args))
                {
                    StringBuilder signatureStr = new();
                    foreach (var arg in existing.Args)
                        signatureStr.Append($"{arg.Type.ToStringOriginal()}, ");

                    Panic($"Function '{info.FuncName}({signatureStr.ToString().TrimEnd().TrimEnd(',')})' with same signature already exists");
                    return;
                }
            }

            Functions[@namespace][info.FuncName].Add(info);
        }
        private bool IsSameSignature(List<VariableInfo> signA, List<VariableInfo> signB)
        {
            if (signA.Count != signB.Count)
                return false;

            for(int i = 0; i < signA.Count; i++)
            {
                if (!signA[i].Type.Cmp(signB[i].Type))
                    return false;
            }

            return true;
        }

        internal List<FunctionInfo> GetFunctions(string fileName, string funcName) => Functions[fileName][funcName];
        internal bool IsFunctionInCurrentContext(string funcName) => Functions[CurrentFileName].ContainsKey(funcName);

        internal FunctionInfo? GetFunctionBySignature(string fileName, string funcName, List<VariableInfo> signature)
        {
            foreach(FunctionInfo info in GetFunctions(fileName, funcName))
            {
                if (IsSameSignature(info.Args, signature))
                    return info;
            }
            return null;
        }

        internal void Panic(string msg, ulong ln = 0, ulong col = 0) => Analyzer.Panic(CurrentFileName, msg, ln, col);
    }
}
