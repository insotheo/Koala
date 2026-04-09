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

                foreach(StructInfo @struct in _ctx.GetStructs(fileName))
                {
                    StringBuilder structBuilder = new();

                    structBuilder.Append($"typedef struct {@struct.Name} {@struct.Name};\n");
                    structBuilder.Append($"struct {@struct.Name}{{\n");
                    foreach (VariableInfo field in @struct.Fields.Values)
                        structBuilder.Append($"{field.Type.ToCType()} {field.Name};\n");

                    structBuilder.Append("};\n");

                    header.Append(structBuilder.ToString());

                    GenerateFunctionsDeclarations(header, @struct.Methods);
                }

                GenerateFunctionsDeclarations(header, _ctx.Functions[fileName]);

                header.Append("#endif");
                ///

                //code
                foreach(ASTNode node in _ctx.Analyzer.Modules[fileName])
                {
                    if (node is ASTFunction funcNode)
                        GenerateFunctionsDefinitions(code, _ctx.Functions[fileName], funcNode);
                    else if(node is ASTStructDecl structNode)
                    {
                        StructInfo structInfo = _ctx.GetStuct(fileName, structNode.StructName);

                        foreach (ASTFunction method in structNode.Methods)
                            GenerateFunctionsDefinitions(code, structInfo.Methods, method);
                    }
                }
                ///

                //saving files
                File.WriteAllText(Path.Combine(outputDir, name + ".klh"), header.ToString());
                File.WriteAllText(Path.Combine(outputDir, name + ".c"), code.ToString());
            }
        }

        private void GenerateFunctionsDeclarations(StringBuilder target, FunctionsHandler data)
        {
            foreach (var funcs in data.Functions.Values)
            {
                foreach (FunctionInfo func in funcs)
                {
                    if (func.IsExtern) continue;

                    string argsLine = "";
                    if (func.Args.Count > 0)
                    {
                        StringBuilder argsString = new();
                        foreach (VariableInfo arg in func.Args)
                            argsString.Append($"{arg.Type.ToCType()},");
                        argsLine = argsString.ToString().TrimEnd(',').Trim();
                    }
                    else argsLine = "KOALA_VOID";

                    target.AppendLine($"{func.ReturnType.ToCType()} {func.FuncUName}({argsLine});");
                }
            }
        }

        private void GenerateFunctionsDefinitions(StringBuilder target, FunctionsHandler data, ASTFunction node)
        {
            List<VariableInfo> signature = new();
            foreach (var arg in node.Args)
                signature.Add(new(arg.argName, new(arg.typeName)));

            FunctionInfo info = data.GetFunctionBySignature(node.FuncName, signature).Value;

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

            target.AppendLine($"{info.ReturnType.ToCType()} {info.FuncUName}({argsLine}){{");
            EmitCodeBlock(target, node.Body);
            target.AppendLine("}");
        }
    }
}
