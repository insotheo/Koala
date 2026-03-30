using System;
using System.IO;
using System.Collections.Generic;
using SkullLang.Compiler.Parsers.ASTNodes;

namespace SkullLang.Compiler.Analyzers
{
    internal sealed class AnalyzerContext
    {
        internal IReadOnlyDictionary<string, IReadOnlyList<ASTNode>> Modules { get; private set; }
        internal bool IsAnalizingSuccess { get; private set; } = true;


        internal AnalyzerContext(Dictionary<string, IReadOnlyList<ASTNode>> modules) => Modules = modules;

        internal void Panic(string fileName, string msg, ulong ln = 0, ulong col = 0)
        {
            Console.Error.WriteLine($"[ERORR] at {Path.GetRelativePath(Directory.GetCurrentDirectory(), fileName)}{(ln == 0 ? "" : col == 0 ? $" at ln: {ln}" : $" at ln: {ln}, col: {col}")}: {msg}");
            IsAnalizingSuccess = false;
        }
    }
}
