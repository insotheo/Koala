using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Koala.Compiler.Lexer;

namespace Koala.App;

class Project
{
    private const string _version = "0.0.0-dev";

    static void Main(string[] cmdArgs)
    {

        if (cmdArgs.Length == 0 || cmdArgs[0] == "help" || cmdArgs[0] == "-h" || cmdArgs[0] == "--help")
        {
            PrintHelpMsg();
            return;
        }
        string command = cmdArgs[0];


        Dictionary<string, HashSet<string>> args = new();
        {
            int i = 1;
            while (i < cmdArgs.Length)
            {
                string token = cmdArgs[i];
                if (token.StartsWith('-'))
                {
                    string flag = token;
                    if (!args.ContainsKey(flag)) args.Add(flag, new());

                    i++;
                    while (i < cmdArgs.Length && !cmdArgs[i].StartsWith('-'))
                    {
                        args[flag].Add(cmdArgs[i]);
                        i++;
                    }
                }
                else //<empty flag> - unknown
                {
                    if (!args.ContainsKey("")) args.Add("", new());
                    args[""].Add(token);
                    i++;
                }
            }
        }

        switch (command)
        {
            case "build": BuildCommand(args); break;
            default: PrintHelpMsg(); break;
        }
    }

    static void BuildCommand(Dictionary<string, HashSet<string>> args)
    {
        if (!args.ContainsKey(""))
        {
            Console.Error.WriteLine("Cannot build zero files");
            PrintHelpMsg();
            return;
        }

        (string path, string content)[] files = new (string path, string content)[args[""].Count];

        {
            string[] paths = args[""].ToArray();
            bool anyDoesntExist = false;

            for (int i = 0; i < args[""].Count; i++)
            {
                files[i].path = paths[i];
                if (!File.Exists(files[i].path))
                {
                    Console.Error.WriteLine($"File \"{files[i].path}\" doesn't exist!");
                    anyDoesntExist = true;
                    continue;
                }
                using (StreamReader reader = new StreamReader(File.OpenRead(files[i].path)))
                {
                    files[i].content = reader.ReadToEnd();
                }
            }
            if (anyDoesntExist)
            {
                Console.Error.WriteLine("Compilation failed!");
                return;
            }
        }


        foreach ((string path, string content) in files)
        {
            Lexer lexer = new Lexer(content);
            lexer.Tokenize();

            //DBG
            Console.WriteLine(path);
            foreach(Token token in lexer.Tokens)
            {
                Console.WriteLine($"{token.Type}({token.Line}; {token.Col}): {(token.Val == null ? "" : token.Val)}");
            }
        }
    }

    static void PrintHelpMsg()
    {
        Console.WriteLine($"""
        Welcome to Koala!
        (version: {_version})

        1) help command
            koalac <|help|-h|--help>

        2) build command
            koalc build <source files>
        """);
    }
}