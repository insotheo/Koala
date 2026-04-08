using KoalaLang.Compiler.Analyzers;
using KoalaLang.Compiler.Parsers.ASTNodes;
using KoalaLang.Tools;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;

using static KoalaLang.CodeGenerator.NodesEmitting;

namespace KoalaLang.CodeGenerator
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
                StringBuilder header = new();
                StringBuilder code = new();
                code.Append($"#include \"{name}.klh\"\n");

                //header
                string headerName = "KLH_" + name.ToUpperSnakeCase() + "_H";
                header.Append($"#ifndef {headerName}\n");
                header.Append($"#define {headerName}\n");

                header.Append("#include \"KOALA_LANG_DEFAULT_DEFINITIONS.h\"\n");

                foreach(string funcName in _ctx.Functions[fileName].Keys)
                {
                    foreach (FunctionInfo funcInfo in _ctx.Functions[fileName][funcName]) //TODO: private, public modifiers; if private - move declaration to code
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
                        else argsLine = "KOALA_VOID";

                        header.AppendLine($"{funcInfo.ReturnType.ToCType()} {funcInfo.FuncUName}({argsLine});");
                    }
                }

                header.Append("#endif");
                ///

                //code
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
                        else argsLine = "KOALA_VOID";

                        code.AppendLine($"{info.ReturnType.ToCType()} {info.FuncUName}({argsLine}){{");
                        EmitCodeBlock(code, funcNode.Body);
                        code.AppendLine("}");
                    }
                }
                ///

                //saving files
                File.WriteAllText(Path.Combine(outputDir, name + ".klh"), header.ToString());
                File.WriteAllText(Path.Combine(outputDir, name + ".c"), code.ToString());
            }
        }
    }
}
