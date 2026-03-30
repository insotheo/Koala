using System;
using System.Collections.Generic;
using System.IO;
using SkullLang.Compiler.Analyzers;
using SkullLang.Compiler.Parsers;
using SkullLang.Compiler.Parsers.ASTNodes;

namespace Skull
{
    internal class Program
    {
        static void Main(string[] args)
        {
            if(args.Length == 0)
            {
                Console.Error.WriteLine("No files provided!");
            }

            bool isParsingSuccess = true;
            var trees = new Dictionary<string, IReadOnlyList<ASTNode>>();

            foreach (string filePath in args)
            {
                string path = Path.GetFullPath(filePath, Directory.GetCurrentDirectory());
                if (!File.Exists(path))
                {
                    Console.Error.WriteLine($"File {path} doesn't exist!");
                }

                using(FileStream fs = File.OpenRead(path))
                {
                    Lexer lexer;
                    fs.Seek(0, SeekOrigin.Begin);
                    
                    using(StreamReader reader = new StreamReader(fs))
                    {
                        lexer = new Lexer(reader.ReadToEnd());
                        lexer.Tokenize();
                    }

                    Parser parser = new(lexer);
                    parser.Parse();

                    isParsingSuccess = isParsingSuccess && parser.IsSuccess;
                    trees.Add(path, parser.AST);
                }
            }

            if (!isParsingSuccess)
            {
                Console.Error.WriteLine("Parsing failed!");
                return;
            }

            Analyzer analyzer = new(trees);
            analyzer.Analyze();
        }
    }
}
