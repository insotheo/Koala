using System.Collections.Generic;

namespace SkullLang.Compiler.Analyzers
{
    internal sealed class Context
    {
        internal Dictionary<string, Dictionary<string, FunctionInfo>> Functions { get; private set; }
        internal AnalyzerContext Analyzer { get; private set; }

        internal string CurrentFileName;
        internal FunctionInfo CurrentFunction;

        internal Context(AnalyzerContext analyzer)
        {
            Functions = new();
            Analyzer = analyzer;
        }

        internal void SetContext(string fileName, FunctionInfo funcInfo)
        {
            CurrentFileName = fileName;
            CurrentFunction = funcInfo;
        }

        internal void DeclareFunction(string @namespace, FunctionInfo info)
        {
            if (!Functions.ContainsKey(@namespace)) Functions.Add(@namespace, new());
            Functions[@namespace].Add(info.FuncName, info);
        }
        internal FunctionInfo GetFunction(string fileName, string funcName) => Functions[fileName][funcName];
        internal bool IsFunctionInCurrentContext(string funcName) => Functions[CurrentFileName].ContainsKey(funcName);

        internal void Panic(string msg, ulong ln = 0, ulong col = 0) => Analyzer.Panic(CurrentFileName, msg, ln, col);
    }
}
