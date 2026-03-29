using System;
using System.Collections.Generic;
using SkullLang.Compiler.Parsers.ASTNodes;

namespace SkullLang.Compiler.Parsers
{
    internal class ParserContext
    {
        internal bool IsParsingSuccess { get; private set; } = true;

        Token[] _tokens;
        string _src;
        int _idx;

        List<ASTNode> _nodes;
        internal IReadOnlyList<ASTNode> Nodes => _nodes;

        internal bool isIdxInRange => _idx < _tokens.Length;
        internal bool NotEOF => _tokens[_idx].Type != TokenType.EOF;
        internal Token Previous => _tokens[_idx - 1];
        internal Token Current => _tokens[_idx];

        internal ParserContext(Lexer lexer)
        {
            _tokens = lexer.GetTokens().ToArray();
            _src = lexer.GetSource();
            _idx = 0;
            _nodes = new();
        }

        internal void PushNode(ASTNode node) => _nodes.Add(node);

        internal void Next()
        {
            _idx++;
            if (_idx >= _tokens.Length) _idx = _tokens.Length - 1;
        }

        internal Token Peek(int offset = 1)
        {
            int idx = _idx + offset;
            return idx < _tokens.Length ? _tokens[idx] : _tokens[_tokens.Length - 1];
        }

        internal bool Expect(TokenType type)
        {
            if (Current.Type != type)
            {
                Panic($"Unexpected token type: expected: {type}, but got: {Current.Type}{(Current.Value == null ? "" : ": " + Current.Value)}");
                return false;
            }

            return true;
        }

        internal void SkipIfSemicolon() { if (Current.Type == TokenType.Semicolon) Next(); }

        internal void Sync(params TokenType[] safe)
        {
            Next();

            while (NotEOF)
            {
                if (safe.Contains(Previous.Type)) return;
                Next();
            }
        }


        internal void Panic(string msg)
        {
            Console.Error.WriteLine($"[PARSER ERROR] at ln: {Current.Ln}, col: {Current.Col}: {msg}");
            IsParsingSuccess = false;
        }

        internal string GetLine(ulong ln)
        {
            ulong idx = ln - 1;
            if (idx >= (ulong)_src.Length) return "";
            return _src.Split('\n')[idx];
        }
    }
}