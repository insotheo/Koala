using KoalaLang.Compiler.Parsers.ASTNodes;
using System.Collections.Generic;

using static KoalaLang.Compiler.Analyzers.StaticAnalyzer;

namespace KoalaLang.Compiler.Analyzers
{
    public sealed class Analyzer
    {
        AnalyzerContext _ctx;
        public bool IsSuccess => _ctx.IsAnalizingSuccess;

        internal Context Output { get; private set; }

        public Analyzer(Dictionary<string, List<ASTNode>> trees)
        {
            _ctx = new(trees);
        }

        public void Analyze()
        {
            //go through all declarations
            Context ctx = new Context(_ctx);

            foreach (string fileName in ctx.Analyzer.Modules.Keys)
            {
                ctx.CurrentFileName = fileName;

                var tree = ctx.Analyzer.Modules[fileName];

                //DEFAULT FUNCTIONS
                ctx.DeclareFunction(fileName, new FunctionInfo("nprint", "int", [], [], isExtern: true, uname: "_F_KOALA_NATIVE_PRINT_F"));
                ctx.DeclareFunction(fileName, new FunctionInfo("nscan", "int", [], [], isExtern: true, uname: "_F_KOALA_NATIVE_SCAN_F"));

                foreach(var node in tree) //struct first
                {
                    if (node is ASTStructDecl structDecl)
                        ctx.DeclareStruct(fileName, structDecl);
                }

                foreach (var node in tree) //functions
                {
                    if (node is ASTFunction funcNode)
                        ctx.DeclareFunction(fileName, FunctionsHandler.ParseFunctionInfo(ctx, funcNode, funcNode.Modifiers));
                }
            }
            ctx.CurrentFileName = "";

            //go through each code block and verify
            foreach (string fileName in ctx.Analyzer.Modules.Keys)
            {
                var tree = ctx.Analyzer.Modules[fileName];
                AnalyzeTree(ctx, fileName, tree);
            }

            Output = ctx;
        }
    }
}
