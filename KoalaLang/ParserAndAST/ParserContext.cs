using KoalaLang.Lexer;
using System;
using System.Collections.Generic;

namespace KoalaLang.ParserAndAST
{
    internal sealed class ParserContext(Lexer.Lexer lexer)
    {
        internal List<Token> Tokens = lexer.Tokens;
        internal Lexer.Lexer Lexer = lexer;
        internal int Index = 0;

        internal Token Current => Index < Tokens.Count ? Tokens[Index] : Tokens[^1];
        internal Token NextToken => Index + 1 < Tokens.Count ? Tokens[Index + 1] : Tokens[^1];
        internal bool End => Index >= Tokens.Count;

        internal void Next()
        {
            if (Index + 1 < Tokens.Count)
                Index += 1;
        }

        internal void Expect(TokenType type)
        {
            if (Current.Type != type)
                Fatal($"Expected token of type {type} but got {Current.Type}");
                
        }

        internal void ExpectNext(TokenType type)
        {
            Next();
            Expect(type);
        }

        internal void Fatal(string msg)
        {
            string line = Lexer.GetStringLine(Current.Line);
            Console.Error.WriteLine($"Parser error: {msg} at line {Current.Line}, col {Current.Column}: {line}");
            Environment.Exit(-1);
        }
    }
}
