using System;
using System.Collections.Generic;

namespace Koala.Compiler.Lexer;

public class Lexer
{
    private string _src;
    private int _idx;
    private int _ln, _col; //_ln stands for line
    private List<Token> _tokens;

    public List<Token> Tokens => _tokens;

    public Lexer(string source)
    {
        _idx = 0;
        _col = 1;
        _ln = 1;
        _src = source;
        _tokens = new();
    }

    public void Tokenize()
    {
        char current;
        while (_idx < _src.Length)
        {
            current = _src[_idx];

            if (Char.IsWhiteSpace(current))
            {
                Next();
                continue;
            }

            if (Char.IsAsciiDigit(current))
            {
                string number = "";
                bool hasDecimalPoint = false;
                bool hasMultipleDecimalPoints = false;
                while (_idx < _src.Length && (Char.IsNumber(_src[_idx]) || _src[_idx] == '.'))
                {
                    if (_src[_idx] == '.')
                    {
                        if (hasDecimalPoint) hasMultipleDecimalPoints = true;
                        hasDecimalPoint = true;
                    }
                    number += _src[_idx];
                    Next(true);
                }
                AddToken(hasDecimalPoint ? (hasMultipleDecimalPoints ? TokenType.Unknown : TokenType.Float) : TokenType.Int, number);
                continue;
            }

            if (Char.IsAsciiLetter(current) || _src[_idx] == '_')
            {
                string identifier = String.Empty;
                while (_idx < _src.Length && (Char.IsAsciiLetterOrDigit(_src[_idx]) || _src[_idx] == '_'))
                {
                    identifier += _src[_idx];
                    Next(true);
                }

                switch (identifier)
                {
                    case "func": AddToken(TokenType.Func); break;
                    case "return": AddToken(TokenType.Return); break;

                    default: AddToken(TokenType.Identifier, identifier); break;
                }

                continue;
            }

            switch (current)
            {
                case ';': AddToken(TokenType.Semicolon); Next(); continue;
                case ':': AddToken(TokenType.Colon); Next(); continue;

                case '(': AddToken(TokenType.LParen); Next(); continue;
                case ')': AddToken(TokenType.RParen); Next(); continue;
                case '{': AddToken(TokenType.LBrace); Next(); continue;
                case '}': AddToken(TokenType.RBrace); Next(); continue;
            }

            AddToken(TokenType.Unknown, current.ToString());
            Next();
        }
        AddToken(TokenType.EOF);
    }

    void AddToken(TokenType type, string value = null) => _tokens.Add(new Token(type, value, _ln, _col));
    void Next(bool skipNextLine = false)
    {
        _idx++;
        _col++;
        if (_idx >= _src.Length) return;
        if (_src[_idx] == '\n' && !skipNextLine)
        {
            _col = 1;
            _ln++;
            Next();
        }
    }
}