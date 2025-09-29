using System;
using System.IO;
using System.Collections.Generic;
using System.Reflection;
using KoalaLang.Lexer;
using KoalaLang.ParserAndAST;

namespace Koala
{
    internal class Program
    {
        static void PrintHelpMsg()
        {
            Console.WriteLine($"""
                Welcome to Koala!
                Version: {Assembly.GetEntryAssembly().GetName().Version.ToString(4)}
                -----------------------

                > koala <build|make> <args>
                    -p <path> - REQUIRED, path to source file(usually *.kls)
                """);
        }

        static void Main(string[] args)
        {
            if (args.Length < 1)
            {
                PrintHelpMsg();
                return;
            }
            string baseCommand = args[0];

            Dictionary<string, string> argsFlags = new Dictionary<string, string>();

            {
                for (int i = 1; i < args.Length; i++)
                {
                    if (args[i].StartsWith("-"))
                    {
                        if (i + 1 < args.Length && !args[i + 1].StartsWith("-"))
                        {
                            argsFlags.Add(args[i], args[i + 1]);
                            i += 1;
                            continue;
                        }
                        else argsFlags.Add(args[i], args[i + 1]);
                    }
                }
            }

            if(baseCommand == "build" || baseCommand == "make")
            {
                if (!argsFlags.ContainsKey("-p"))
                {
                    Console.Error.WriteLine("Path to source file is required!\n");
                    PrintHelpMsg();
                    return;
                }
                string path = argsFlags["-p"];
                if (!File.Exists(path))
                {
                    Console.Error.WriteLine($"Path \"{path}\" doesn't exist!\n");
                    PrintHelpMsg();
                    return;
                }

                string source = String.Empty;
                using(FileStream file = File.OpenRead(path))
                {
                    using(StreamReader reader = new StreamReader(file))
                    {
                        source = reader.ReadToEnd();
                    }
                }
                source = source.Trim();

                if(source == String.Empty)
                {
                    Console.Error.WriteLine("File is empty!");
                    return;
                }

                Lexer lexer = new(source);
                lexer.Tokenize();

                Parser parser = new(lexer);
                parser.Parse();

                Console.WriteLine("Done!");
            }
            else
            {
                PrintHelpMsg();
            }
        }
    }
}
