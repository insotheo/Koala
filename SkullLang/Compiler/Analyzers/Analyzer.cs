using SkullLang.Compiler.Parsers.ASTNodes;
using System.Collections.Generic;

using static SkullLang.Compiler.Analyzers.StaticAnalyzer;

namespace SkullLang.Compiler.Analyzers
{
    public sealed class Analyzer
    {
        AnalyzerContext _ctx;
        public bool IsSuccess => _ctx.IsAnalizingSuccess;

        internal Context Output { get; private set; }

        public Analyzer(Dictionary<string, IReadOnlyList<ASTNode>> trees)
        {
            _ctx = new(trees);
        }

        public void Analyze()
        {
            //go through all declarations
            Context ctx = new Context(_ctx);
            foreach(string fileName in ctx.Analyzer.Modules.Keys)
            {
                var tree = ctx.Analyzer.Modules[fileName];

                foreach(var node in tree)
                {
                    if(node is ASTFunction funcNode)
                        ctx.DeclareFunction(fileName, new FunctionInfo(funcNode.FuncName, TypeInfo.GetBaseCTypeName(funcNode.RetType), []));
                }
            }

            //go through each code block and verify
            foreach(string fileName in ctx.Analyzer.Modules.Keys)
            {
                var tree = ctx.Analyzer.Modules[fileName];
                AnalyzeTree(ctx, fileName, tree);
            }

            Output = ctx;
        }
    }
}
