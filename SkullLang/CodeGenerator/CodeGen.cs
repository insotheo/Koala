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

                foreach(FunctionInfo funcInfo in _ctx.Functions[fileName].Values)
                {
                    code.AppendLine($"{funcInfo.ReturnType} {funcInfo.FuncName}();");
                }

                foreach(ASTNode node in _ctx.Analyzer.Modules[fileName])
                {
                    if(node is ASTFunction funcNode)
                    {
                        FunctionInfo info = _ctx.GetFunction(fileName, funcNode.FuncName);
                        code.AppendLine($"{info.ReturnType} {funcNode.FuncName}(){{");
                        EmitCodeBlock(code, funcNode.Body);
                        code.AppendLine("}");
                    }
                }

                File.WriteAllText(Path.Combine(outputDir, name + ".c"), code.ToString());
            }
        }
    }
}
