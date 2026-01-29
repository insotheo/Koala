using System;
using System.Collections.Generic;

namespace Koala.App;

class Project
{
    private const string _version = "0.0.0-dev"; 

    static void Main(string[] cmdArgs)
    {
    
        if(cmdArgs.Length == 0 || cmdArgs[0] == "help" || cmdArgs[0] == "-h" || cmdArgs[0] == "--help") PrintHelpMsg();
        string command = cmdArgs[0];

        Dictionary<string, HashSet<string>> args = new();
        {
            int i = 1;
            while(i < cmdArgs.Length)
            {
                string token = cmdArgs[i];
                if (token.StartsWith('-'))
                {
                    string flag = token;
                    if(!args.ContainsKey(flag)) args.Add(flag, new());

                    i++;
                    while(i < cmdArgs.Length && !cmdArgs[i].StartsWith('-'))
                    {
                        args[flag].Add(cmdArgs[i]);
                        i++;
                    }
                }
                else //<empty flag> - unknown
                {
                    if(!args.ContainsKey("")) args.Add("", new());
                    args[""].Add(token);
                    i++;
                }
            }
        }

        //DBG
        Console.WriteLine($"Command: {command}");
        foreach((string key, HashSet<string> vals) in args)
        {
            Console.WriteLine($"{key}: {String.Join(' ', vals)}");
        }
    }

    static void PrintHelpMsg()
    {
        Console.WriteLine($"""
        Welcome to Koala!
        (version: {_version})

        1) help command
            koalac <|help|-h|--help>
        """);
    }
}