using System;
using System.IO;
using SkullLang.Compiler.Parsers;

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
            
            foreach(string filePath in args)
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
                }
            }
        }
    }
}
