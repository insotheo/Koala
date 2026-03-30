using LLVMSharp;
using LLVMSharp.Interop;
using SkullLang.Compiler.Analyzers;
using SkullLang.Compiler.Parsers.ASTNodes;
using System;
using System.Collections.Generic;
using System.IO;

using static SkullLang.Compiler.CodeGenerating.NodesEmit;

namespace SkullLang.Compiler.CodeGenerating
{
    public sealed class CodeGenerator
    {
        readonly Context _inputCtx;

        public CodeGenerator(Analyzer analyzer)
        {
            _inputCtx = analyzer.Output;

            LLVM.InitializeNativeTarget();
            LLVM.InitializeNativeAsmPrinter();
        }

        public unsafe void Generate(string saveDir)
        {
            var llvmCtx = LLVMContextRef.Create();

            foreach (string fileName in _inputCtx.Functions.Keys)
            {
                Dictionary<string, LLVMValueRef> functionMap = new();
                var module = llvmCtx.CreateModuleWithName($"skull_module_{Path.GetFileNameWithoutExtension(fileName)}");
                var builder = llvmCtx.CreateBuilder();

                foreach (FunctionInfo funcInfo in _inputCtx.Functions[fileName].Values)
                {
                    var retType = GetLLVMType(new() { TypeName = funcInfo.ReturnType });
                    var paramTypes = Array.Empty<LLVMTypeRef>();
                    var funcType = LLVMTypeRef.CreateFunction(retType, paramTypes);

                    var function = module.AddFunction(funcInfo.FuncName, funcType);

                    functionMap.Add(funcInfo.FuncName, function);
                }

                foreach(ASTNode node in _inputCtx.Analyzer.Modules[fileName])
                {
                    if(node is ASTFunction funcNode)
                    {
                        var function = functionMap[funcNode.FuncName];
                        EmitFunction(builder, function, funcNode);
                    }
                }

                //module.Verify(LLVMVerifierFailureAction.LLVMPrintMessageAction);
                module.PrintToFile(Path.Combine(saveDir, Path.GetFileNameWithoutExtension(fileName) + ".ll"));
            }
        }
    }
}
