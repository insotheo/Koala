using System;
using System.Collections.Generic;
using SkullLang.Compiler.Parsers.ASTNodes;

namespace SkullLang.Compiler.Parsers
{
    internal class ParserContext
    {
        Token[] _tokens;
        int _idx;

        List<ASTNode> _nodes;

        internal bool isIdxInRange => _idx < _tokens.Length;
        internal bool NotEOF => _tokens[_idx].Type != TokenType.EOF;
        internal Token Previous => _tokens[_idx - 1];
        internal Token Current => _tokens[_idx];

        internal ParserContext(Lexer lexer)
        {
            _tokens = lexer.GetTokens().ToArray();
            _idx = 0;
            _nodes = new();
        }

        internal void PushNode(ASTNode node) => _nodes.Add(node);

        internal void Next()
        {
            _idx++;
            if(_idx >= _tokens.Length) _idx = _tokens.Length - 1;
        }

        internal Token Peek(int offset = 1)
        {
            int idx = _idx + offset;
            return idx < _tokens.Length ? _tokens[idx] : _tokens[_tokens.Length - 1];
        }

        internal bool Expect(TokenType type)
        {
            if(Current.Type != type)
            {
                Panic($"Unexpected token type: expected: {type}, but got: {Current.Type}{(Current.Value == null ? "" : ": " + Current.Value)}");
                return false;
            }
            
            return true;
        }

        internal void Sync(params TokenType[] safe)
        {
            Next();

            while (NotEOF)
            {
                if (safe.Contains(Previous.Type)) return;
            }
        }


        internal void Panic(string msg) => Console.Error.WriteLine($"[PARSER ERROR] at ln: {Current.Ln}, col: {Current.Col}: {msg}");
    }
}