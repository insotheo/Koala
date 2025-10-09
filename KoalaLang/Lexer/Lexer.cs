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
                        identifier == "func"
                        || identifier == "return"
                        || identifier == "let"
                        || identifier == "if"
                        || identifier == "else"
                        || identifier == "while"
                        || identifier == "do"
                        || identifier == "for"
                        || identifier == "break"
                        || identifier == "continue"
                        ) _tokens.Add(new(TokenType.Keyword, identifier, _ln, _col));

                    else if (identifier == "true" || identifier == "false") _tokens.Add(new(TokenType.BooleanValue, identifier, _ln, _col));

                    else _tokens.Add(new(TokenType.Identifier, identifier, _ln, _col));
                    
                    continue;
                }

                switch (_text[_pos])
                {
                    case ';': _tokens.Add(new(TokenType.Semicolon, "", _ln, _col)); Next(); continue;
                    case ':': _tokens.Add(new(TokenType.Colon, "", _ln, _col)); Next(); continue;
                    case ',': _tokens.Add(new(TokenType.Comma, "", _ln, _col)); Next(); continue;
                    
                    case '(': _tokens.Add(new(TokenType.LParen, "", _ln, _col)); Next(); continue;
                    case ')': _tokens.Add(new(TokenType.RParen, "", _ln, _col)); Next(); continue;

                    case '{': _tokens.Add(new(TokenType.LBrace, "", _ln, _col)); Next(); continue;
                    case '}': _tokens.Add(new(TokenType.RBrace, "", _ln, _col)); Next(); continue;

                    case '=':
                        if(_pos + 1 < _text.Length && _text[_pos + 1] == '=')
                        {
                            Next();
                            Next();
                            _tokens.Add(new(TokenType.Equal, "", _ln, _col)); 
                            continue;
                        }
                        else _tokens.Add(new(TokenType.AssignmentSign, "", _ln, _col)); 
                        Next(); 
                        continue;
                    case '%': _tokens.Add(new(TokenType.Percent, "", _ln, _col)); Next(); continue;
                    case '+': _tokens.Add(new(TokenType.Plus, "", _ln, _col)); Next(); continue;
                    case '-': _tokens.Add(new(TokenType.Minus, "", _ln, _col)); Next(); continue;
                    case '*': _tokens.Add(new(TokenType.Asterisk, "", _ln, _col)); Next(); continue;
                    case '/':
                        if (_pos + 1 < _text.Length && _text[_pos + 1] == '/') //coment
                        {
                            while (_pos < _text.Length && _text[_pos] != '\n') Next(false);
                            continue;
                        }

                        else if(_pos + 1 < _text.Length && _text[_pos + 1] == '*') //multi-line coment
                        {
                            Next(false);
                            while(_pos + 1 < _text.Length)
                            {
                                if (_text[_pos] == '*' && _text[_pos + 1] == '/')
                                {
                                    Next(false);
                                    Next(false);
                                    break;
                                }
                                Next(false);
                            }
                            continue;
                        }

                        else _tokens.Add(new(TokenType.Slash, "", _ln, _col));
                        Next();
                        continue;

                    case '!':
                        if (_pos + 1 < _text.Length && _text[_pos + 1] == '=')
                        {
                            Next();
                            Next();
                            _tokens.Add(new(TokenType.Inequal, "", _ln, _col));
                            continue;
                        }
                        _tokens.Add(new(TokenType.LogicalNot, "", _ln, _col));
                        Next();
                        continue;
                    case '~': _tokens.Add(new(TokenType.BitwiseNot, "", _ln, _col)); Next(); continue;
                    case '^': _tokens.Add(new(TokenType.Xor, "", _ln, _col)); Next(); continue;
                    case '&':
                        if (_pos + 1 < _text.Length && _text[_pos + 1] == '&')
                        {
                            Next();
                            Next();
                            _tokens.Add(new(TokenType.LogicalAnd, "", _ln, _col));
                            continue;
                        }
                        else _tokens.Add(new(TokenType.BitwiseAnd, "", _ln, _col));
                        Next();
                        continue;
                    case '|':
                        if (_pos + 1 < _text.Length && _text[_pos + 1] == '|')
                        {
                            Next();
                            Next();
                            _tokens.Add(new(TokenType.LogicalOr, "", _ln, _col));
                            continue;
                        }
                        else _tokens.Add(new(TokenType.BitwiseOr, "", _ln, _col));
                        Next();
                        continue;
                    case '<':
                        if (_pos + 1 < _text.Length && _text[_pos + 1] == '<')
                        {
                            Next();
                            Next();
                            _tokens.Add(new(TokenType.LeftShift, "", _ln, _col));
                            continue;
                        }
                        else if (_pos + 1 < _text.Length && _text[_pos + 1] == '=')
                        {
                            Next();
                            Next();
                            _tokens.Add(new(TokenType.LessOrEqual, "", _ln, _col));
                            continue;
                        }
                        else _tokens.Add(new(TokenType.Less, "", _ln, _col));
                        Next();
                        continue;
                    case '>':
                        if (_pos + 1 < _text.Length && _text[_pos + 1] == '>')
                        {
                            Next();
                            Next();
                            _tokens.Add(new(TokenType.RightShift, "", _ln, _col));
                            continue;
                        }
                        else if (_pos + 1 < _text.Length && _text[_pos + 1] == '=')
                        {
                            Next();
                            Next();
                            _tokens.Add(new(TokenType.MoreOrEqual, "", _ln, _col));
                            continue;
                        }
                        else _tokens.Add(new(TokenType.More, "", _ln, _col));
                        Next();
                        continue;
                }

                _tokens.Add(new(TokenType.Unknown, _text[_pos].ToString(), _ln, _col));
                Next();
            }
            _tokens.Add(new(TokenType.EOF, "", _ln, _col));
        }

        private void Next(bool skipNextLine = true)
        {
            _pos += 1;
            _col += 1;
            if (_pos < _text.Length)
            {
                if (_text[_pos] == '\n')
                {
                    if(skipNextLine) _pos += 1;
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
