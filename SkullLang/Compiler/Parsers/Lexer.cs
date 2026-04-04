using System;
using System.Collections.Generic;
using System.Text;

namespace SkullLang.Compiler.Parsers
{
    public sealed class Lexer
    {
        string _src;
        int _idx;

        ulong _ln, _col;
        List<Token> _tokens;

        internal List<Token> GetTokens() => _tokens;
        internal string GetSource() => _src;

        bool _isIdxValid => _idx < _src.Length;
        char _cur => _src[_idx];

        public Lexer(string source)
        {
            _src = source;
            _idx = 0;
            _tokens = new();
            _ln = 1;
            _col = 1;
        }

        public void Tokenize()
        {
            while (_isIdxValid)
            {
                if (Char.IsWhiteSpace(_cur))
                {
                    Next();
                    continue;
                }

                if (Char.IsAsciiDigit(_cur))
                {
                    StringBuilder number = new();
                    ulong startCol = _col;
                    bool hasDecimalPoint = false, isValid = true;

                    while(_isIdxValid && (Char.IsAsciiDigit(_cur) || _cur == '.'))
                    {
                        if(_cur == '.')
                        {
                            if(hasDecimalPoint) isValid = false;
                            hasDecimalPoint = true;
                        }
                        number.Append(_cur);
                        Next();
                    }

                    if(isValid) AddToken(hasDecimalPoint ? TokenType.NumberF : TokenType.NumberI, number.ToString(), startCol);
                    else AddToken(TokenType.Unknown, number.ToString(), startCol);

                    continue;
                }

                else if(Char.IsAsciiLetter(_cur) || _cur == '_')
                {
                    StringBuilder identifier = new();
                    ulong startCol = _col;

                    while(_isIdxValid && (Char.IsAsciiLetterOrDigit(_cur) || _cur == '_'))
                    {
                        identifier.Append(_cur);
                        Next();
                    }

                    string id = identifier.ToString();

                    switch (id)
                    {
                        case "true": AddToken(TokenType.NumberI, "1", col:startCol); break;
                        case "false": AddToken(TokenType.NumberI, "0", col:startCol); break;

                        case "func": AddToken(TokenType.FuncKW, col:startCol); break;
                        case "return": AddToken(TokenType.ReturnKW, col:startCol); break;
                        case "let": AddToken(TokenType.LetKW, col:startCol); break;
                        case "if": AddToken(TokenType.IfKW, col:startCol); break;
                        case "else": AddToken(TokenType.ElseKW, col:startCol); break;
                        case "while": AddToken(TokenType.WhileKW, col:startCol); break;
                         
                        default: AddToken(TokenType.Identifier, id, startCol); break;
                    }

                    continue;
                }

                switch (_cur)
                {
                    case ';': AddToken(TokenType.Semicolon); Next(); continue;
                    case ':': AddToken(TokenType.Colon); Next(); continue;

                    case ',': AddToken(TokenType.Comma); Next(); continue;

                    case '!':
                        {
                            if (Peek() == '=') { AddToken(TokenType.Inequal); Next(); }
                            else AddToken(TokenType.Not);

                            Next(); continue;
                        }
                    case '=':
                        {
                            if (Peek() == '=') { AddToken(TokenType.Equal); Next(); }
                            else AddToken(TokenType.Assignment);
                            
                            Next(); continue;
                        }

                    case '(': AddToken(TokenType.LParen); Next(); continue;
                    case ')': AddToken(TokenType.RParen); Next(); continue;
                    case '{': AddToken(TokenType.LBrace); Next(); continue;
                    case '}': AddToken(TokenType.RBrace); Next(); continue;

                    case '+': AddToken(TokenType.Plus); Next(); continue;
                    case '-': AddToken(TokenType.Minus); Next(); continue;
                    case '*': AddToken(TokenType.Asterisk); Next(); continue;
                    case '/':
                        {
                            if (Peek() == '/') //comment
                            {
                                while (_cur != '\n') Next();
                                Next();
                            }
                            else if(Peek() == '*') //multi-line comment
                            {
                                while (true)
                                {
                                    Next();
                                    if (_cur == '*' && Peek() == '/') break;
                                }
                                Next(); //consume *
                                Next(); //consume /
                            }
                            else
                            {
                                AddToken(TokenType.Slash);
                                Next();
                            }
                            continue;
                        }
                    case '%': AddToken(TokenType.Percent); Next(); continue;

                    case '&':
                        {
                            if(Peek() == '&') { AddToken(TokenType.LogicalAnd); Next(); }
                            else AddToken(TokenType.Ampersand); 
                            
                            Next(); continue;
                        }
                    case '|':
                        {
                            if(Peek() == '|') { AddToken(TokenType.LogicalOr); Next(); }
                            else AddToken(TokenType.Pipe);
                            
                            Next(); continue;
                        }
                    case '^': AddToken(TokenType.Caret); Next(); continue;
                    case '~': AddToken(TokenType.Tilde); Next(); continue;
                    case '<':
                        {
                            if (Peek() == '<') { AddToken(TokenType.LeftShift); Next(); }
                            else if (Peek() == '=') { AddToken(TokenType.LessOrEqual); Next(); }
                            else AddToken(TokenType.LessThan); //<

                            Next(); continue;
                        }
                    case '>':
                        {
                            if (Peek() == '>') { AddToken(TokenType.RightShift); Next(); }
                            else if (Peek() == '=') { AddToken(TokenType.GreaterOrEqual); Next(); }
                            else AddToken(TokenType.GreaterThan); //>

                            Next(); continue;
                        }

                    case '"':
                        {
                            Next();
                            StringBuilder stringLiteral = new();

                            while(_isIdxValid && _cur != '"')
                            {
                                stringLiteral.Append(_cur);
                                Next();
                            }
                            Next();

                            AddToken(TokenType.StringLiteral, stringLiteral.ToString(), _col); continue;
                        }

                }

                AddToken(TokenType.Unknown, _cur.ToString());
                Next();
            }

            AddToken(TokenType.EOF);
        }

        private void Next()
        {
            char c = _cur;
            _idx++;
            _col++;

            if(c == '\n')
            {
                _ln += 1;
                _col = 1;
            }
        }

        internal char Peek(int offset = 1)
        {
            int idx = _idx + offset;
            return idx < _src.Length ? _src[idx] : '\0';
        }

        private void AddToken(TokenType type, string val = null, ulong col = 0) => _tokens.Add(new Token(_ln, col == 0 ? _col : col, type, val));
    }
}
