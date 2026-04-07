using SkullLang.Compiler.Analyzers;
using SkullLang.Compiler.Parsers.ASTNodes;
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

                StringBuilder code = new();

                code.Append("#include \"SKULL_LANG_DEFAULT_DEFINITIONS.h\"\n");

                foreach(FunctionInfo funcInfo in _ctx.Functions[fileName].Values)
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

                    code.AppendLine($"{funcInfo.ReturnType.ToCType()} {funcInfo.FuncUName}({argsLine});");
                }

                foreach(ASTNode node in _ctx.Analyzer.Modules[fileName])
                {
                    if(node is ASTFunction funcNode)
                    {
                        FunctionInfo info = _ctx.GetFunction(fileName, funcNode.FuncName);

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

                File.WriteAllText(Path.Combine(outputDir, name + ".c"), code.ToString());
            }
        }
    }
}
