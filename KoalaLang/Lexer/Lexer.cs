using System;
using System.Collections.Generic;

namespace KoalaLang.Lexer
{
    public sealed class Lexer(string text)
    {
        private int _pos = 0;
        private int _col = 1;
        private int _ln = 1;
        private string _text = text;
        private List<Token> _tokens = new();

        public void Tokenize()
        {
            while(_pos < _text.Length)
            {
                if (Char.IsWhiteSpace(_text[_pos]))
                {
                    Next();
                    continue;
                }

                if (Char.IsNumber(_text[_pos]))
                {
                    string number = String.Empty;
                    bool hasDecimalPoint = false, hasMoreThanOneDecimalPoint = false;
                    while (_pos < _text.Length && (Char.IsNumber(_text[_pos]) || _text[_pos] == '.'))
                    {
                        if (_text[_pos] == '.')
                        {
                            if (hasDecimalPoint) hasMoreThanOneDecimalPoint = true;
                            hasDecimalPoint = true;
                        }
                        number += _text[_pos];
                        Next();
                    }

                    if (hasDecimalPoint && !hasMoreThanOneDecimalPoint) _tokens.Add(new(TokenType.FloatNumber, number, _ln, _col)); 
                    else if (!hasDecimalPoint && !hasMoreThanOneDecimalPoint) _tokens.Add(new(TokenType.Number, number, _ln, _col)); 
                    else _tokens.Add(new(TokenType.Unknown, number, _ln, _col));

                    continue;
                }

                if (Char.IsLetter(_text[_pos]) || _text[_pos] == '_')
                {
                    string identifier = String.Empty;
                    while(_pos < _text.Length && (Char.IsLetterOrDigit(_text[_pos]) || _text[_pos] == '_'))
                    {
                        identifier += _text[_pos];
                        Next();
                    }

                    if (
                        identifier == "fn"
                        || identifier == "return"
                        ) _tokens.Add(new(TokenType.Keyword, identifier, _ln, _col));
                    else _tokens.Add(new(TokenType.Identifier, identifier, _ln, _col));
                    continue;
                }

                switch (_text[_pos])
                {
                    case ';': _tokens.Add(new(TokenType.Semicolon, "", _ln, _col)); Next(); continue;
                    case ':': _tokens.Add(new(TokenType.Colon, "", _ln, _col)); Next(); continue;
                    
                    case '(': _tokens.Add(new(TokenType.LParen, "", _ln, _col)); Next(); continue;
                    case ')': _tokens.Add(new(TokenType.RParen, "", _ln, _col)); Next(); continue;

                    case '{': _tokens.Add(new(TokenType.LBrace, "", _ln, _col)); Next(); continue;
                    case '}': _tokens.Add(new(TokenType.RBrace, "", _ln, _col)); Next(); continue;

                    case '/':
                        {
                            if(_pos + 1 < _text.Length && _text[_pos] == '/') //coment
                            {
                                while (_pos < _text.Length && _text[_pos] != '\n') Next();
                                continue;
                            }
                            Next();
                            continue;
                        }
                }

                _tokens.Add(new(TokenType.Unknown, _text[_pos].ToString(), _ln, _col));
                Next();
            }
            _tokens.Add(new(TokenType.EOF, "", _ln, _col));
        }

        private void Next()
        {
            _pos += 1;
            _col += 1;
            if (_pos < _text.Length)
            {
                if (_text[_pos] == '\n')
                {
                    _pos += 1;
                    _ln += 1;
                    _col = 1;
                }
            }
        }


        //For Parser
        internal string GetStringLine(int line) => _text.Split('\n')[line - 1];

        internal List<Token> Tokens { get { return _tokens; } }
    }
}
