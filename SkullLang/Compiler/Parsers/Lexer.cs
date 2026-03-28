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
                        case "return": AddToken(TokenType.ReturnK, col:startCol); break;
                        
                        default: AddToken(TokenType.Identifier, id, startCol); break;
                    }

                    continue;
                }

                switch (_cur)
                {
                    case '(': AddToken(TokenType.LParen); Next(); continue;
                    case ')': AddToken(TokenType.RParen); Next(); continue;
                    case '{': AddToken(TokenType.LBrace); Next(); continue;
                    case '}': AddToken(TokenType.RBrace); Next(); continue;
                }

                AddToken(TokenType.Unknown, _cur.ToString());
                Next();
            }

            AddToken(TokenType.EOF);

            foreach(var token in _tokens)
            {
                System.Console.WriteLine($"{token.Type}: {token.Value} at ln: {token.Ln}, col: {token.Col}");
            }
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

        private void AddToken(TokenType type, string val = null, ulong col = 0) => _tokens.Add(new Token(_ln, col == 0 ? _col : col, type, val));
    }
}
