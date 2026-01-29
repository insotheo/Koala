using System;
using Koala.Compiler;

namespace Koala.App;

class Project
{
    static void Main(string[] args)
    {
        Test.Hello();
        Console.WriteLine(String.Join('\n', args));
    }
}