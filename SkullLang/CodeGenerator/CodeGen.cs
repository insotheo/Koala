using SkullLang.Compiler.Analyzers;
using SkullLang.Compiler.Parsers.ASTNodes;
using SkullLang.Tools;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;

using static SkullLang.CodeGenerator.NodesEmitting;

namespace SkullLang.CodeGenerator
{
    public sealed class CodeGen
    {
        Context _ctx;


        public CodeGen(Analyzer analyzer)
        {
            _ctx = analyzer.Output;

            CultureInfo.CurrentCulture = new CultureInfo("en-us");
        }

        public void Generate(string outputDir)
        {
            if (!Directory.Exists(outputDir))
                Directory.CreateDirectory(outputDir);

            foreach(string fileName in _ctx.Functions.Keys)
            {
                string name = Path.GetFileNameWithoutExtension(fileName);

                //header
                string headerName = "SKH_" + name.ToUpperSnakeCase() + "_H";
                StringBuilder header = new();
                header.Append($"#ifndef {headerName}\n");
                header.Append($"#define {headerName}\n");

                header.Append("#include \"SKULL_LANG_DEFAULT_DEFINITIONS.h\"\n");

                foreach(string funcName in _ctx.Functions[fileName].Keys)
                {
                    foreach (FunctionInfo funcInfo in _ctx.Functions[fileName][funcName]) //TODO: private, public modifiers
                    {
                        if (funcInfo.IsExtern) continue;

                        string argsLine = "";
                        if (funcInfo.Args.Count > 0)
                        {
                            StringBuilder argsString = new();
                            foreach (VariableInfo arg in funcInfo.Args)
                            {
                                argsString.Append($"{arg.Type.ToCType()},");
                            }
                            argsLine = argsString.ToString().TrimEnd(',').Trim();
                        }
                        else argsLine = "SKULL_VOID";

                        header.AppendLine($"{funcInfo.ReturnType.ToCType()} {funcInfo.FuncUName}({argsLine});");
                    }
                }

                header.Append("#endif");
                ///

                //code
                StringBuilder code = new();
                code.Append($"#include \"{name}.skh\"\n");
                foreach(ASTNode node in _ctx.Analyzer.Modules[fileName])
                {
                    if(node is ASTFunction funcNode)
                    {
                        List<VariableInfo> signature = new();
                        foreach (var arg in funcNode.Args)
                            signature.Add(new(arg.argName, new(arg.typeName)));

                        FunctionInfo info = _ctx.GetFunctionBySignature(fileName, funcNode.FuncName, signature).Value;

                        string argsLine = "";
                        if (info.Args.Count > 0)
                        {
                            StringBuilder argsString = new();
                            foreach (VariableInfo arg in info.Args)
                            {
                                argsString.Append($"{arg.Type.ToCType()} {arg.Name},");
                            }
                            argsLine = argsString.ToString().TrimEnd(',').Trim();
                        }
                        else argsLine = "SKULL_VOID";

                        code.AppendLine($"{info.ReturnType.ToCType()} {info.FuncUName}({argsLine}){{");
                        EmitCodeBlock(code, funcNode.Body);
                        code.AppendLine("}");
                    }
                }
                ///

                //saving files
                File.WriteAllText(Path.Combine(outputDir, name + ".skh"), header.ToString());
                File.WriteAllText(Path.Combine(outputDir, name + ".c"), code.ToString());
            }
        }
    }
}
