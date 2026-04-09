using KoalaLang.Compiler.Parsers.ASTNodes;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KoalaLang.Compiler.Analyzers
{
    internal sealed class Context
    {
        internal Dictionary<string, FunctionsHandler> Functions { get; private set; }
        internal Dictionary<string, Dictionary<string, StructInfo>> Structs { get; private set; }

        internal AnalyzerContext Analyzer { get; private set; }

        internal string CurrentFileName;
        internal FunctionInfo CurrentFunction;
        internal Dictionary<string, VariableInfo> CurrentVars;

        internal Context(AnalyzerContext analyzer)
        {
            Functions = new();
            Structs = new();
            Analyzer = analyzer;
            CurrentVars = new();
        }

        private Context(Context oldCtx)
        {
            Dictionary<string, VariableInfo> NewCurrentVars = new();
            foreach ((string typeName, VariableInfo varInfo) in oldCtx.CurrentVars)
                NewCurrentVars.Add(typeName, varInfo);

            Functions = oldCtx.Functions;
            Structs = oldCtx.Structs;
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

            if (!Functions[@namespace].Contains(info.FuncName))
                Functions[@namespace].Functions[info.FuncName] = new();

            foreach(var existing in GetFunctions(@namespace, info.FuncName))
            {
                if(FunctionsHandler.IsSameSignature(existing.Args, info.Args))
                {
                    StringBuilder signatureStr = new();
                    foreach (var arg in existing.Args)
                        signatureStr.Append($"{arg.Type.ToStringOriginal()}, ");

                    Panic($"Function '{info.FuncName}({signatureStr.ToString().TrimEnd().TrimEnd(',')})' with same signature already exists");
                    return;
                }
            }

            Functions[@namespace].AddFunction(info.FuncName, info);
        }

        internal List<FunctionInfo> GetFunctions(string fileName, string funcName)
        {
            if (!Functions.ContainsKey(fileName)) return new();
            return Functions[fileName].Functions[funcName];
        }

        internal bool IsFunctionInCurrentContext(string funcName) => Functions[CurrentFileName].Contains(funcName);

        internal FunctionInfo? GetFunctionBySignature(string fileName, string funcName, List<VariableInfo> signature) => Functions[fileName].GetFunctionBySignature(funcName, signature);

        internal void DeclareStruct(string @namespace, ASTStructDecl structNode)
        {
            if (!Structs.ContainsKey(@namespace))
                Structs.Add(@namespace, new());

            if (Structs[@namespace].ContainsKey(structNode.StructName))
            {
                Panic($"Struct '{structNode.StructName}' already defined", structNode.Ln, structNode.Col);
                return;
            }

            StructInfo info = new(structNode.StructName);
            Structs[@namespace].Add(info.Name, info);

            foreach (ASTVariableDecl field in structNode.Fields)
            {
                if (info.Fields.ContainsKey(field.VarName))
                {
                    Panic($"Field '{field.VarName}' already exists in struct '{info.Name}'", field.Ln, field.Col);
                    continue;
                }

                TypeInfo type = new(field.TypeName, ctx: this, node: field);

                Structs[@namespace][info.Name].Fields.Add(field.VarName, new(field.VarName, type));
            }

            foreach(ASTFunction method in structNode.Methods)
            {
                Structs[@namespace][info.Name].DeclareMethod(method.FuncName, FunctionsHandler.ParseFunctionInfo(this, method));
            }
        }
        internal bool IsStuctDefinedInCurrentContext(string structName) => Structs[CurrentFileName].ContainsKey(structName);
        internal StructInfo GetStuct(string fileName, string structName) => Structs[fileName][structName];
        
        internal List<StructInfo> GetStructs(string fileName)
        {
            if (!Structs.ContainsKey(fileName)) return new();
            return Structs[fileName].Values.ToList();
        }

        internal void Panic(string msg, ulong ln = 0, ulong col = 0) => Analyzer.Panic(CurrentFileName, msg, ln, col);
    }
}
