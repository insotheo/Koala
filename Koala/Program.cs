using System;
using System.Collections.Generic;
using System.IO;
using KoalaLang.Compiler.Analyzers;
using KoalaLang.Compiler.Parsers;
using KoalaLang.Compiler.Parsers.ASTNodes;
using KoalaLang.CodeGenerator;
using System.Linq;

namespace Koala
{

    //libLLVM = LLVM-C
    internal class Program
    {
        static void Main(string[] args)
        {
            if(args.Length == 0)
            {
                Console.Error.WriteLine("No files provided!");
            }

            DateTime t1 = DateTime.Now;

            bool isParsingSuccess = true;
            var trees = new Dictionary<string, List<ASTNode>>();

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
                    trees.Add(path, parser.AST.ToList());
                }
            }

            if (!isParsingSuccess)
            {
                Console.Error.WriteLine("Parsing failed!");
                return;
            }

            Analyzer analyzer = new(trees);
            analyzer.Analyze();

            if (!analyzer.IsSuccess)
            {
                Console.Error.WriteLine("Analyzing failed!");
                return;
            }

            CodeGen gen = new(analyzer);
            gen.Generate("bin");
            
            DateTime t2 = DateTime.Now;
            TimeSpan t = (t2 - t1);

            Console.WriteLine($"Done!\nCompilation completed in {t.ToString()}");
        }
    }
}
