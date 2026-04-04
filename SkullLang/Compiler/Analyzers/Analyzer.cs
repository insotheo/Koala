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

        public Analyzer(Dictionary<string, List<ASTNode>> trees)
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

                //DEFAULT FUNCTIONS
                ctx.DeclareFunction(fileName, new FunctionInfo("printf", "int", [], isExtern: true));
                ctx.DeclareFunction(fileName, new FunctionInfo("scanf", "int", [], isExtern: true));

                foreach(var node in tree)
                {

                    if (node is ASTFunction funcNode)
                    {
                        List<VariableInfo> args = new();

                        foreach((string typeName, string argName) in funcNode.Args)
                        {
                            VariableInfo argInfo = new();

                            argInfo.Name = argName;
                            argInfo.Type = new TypeInfo(typeName, TypeInfo.GetKind(typeName, ctx));

                            args.Add(argInfo);
                        }

                        ctx.DeclareFunction(fileName, new FunctionInfo(funcNode.FuncName, TypeInfo.GetCTypeName(funcNode.RetType), args) { ReturnType=new TypeInfo(funcNode.RetType, TypeInfo.GetKind(funcNode.RetType, ctx)) });
                    }
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
