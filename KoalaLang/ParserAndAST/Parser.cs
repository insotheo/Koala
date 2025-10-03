using KoalaLang.Lexer;
using KoalaLang.ParserAndAST.AST;
using System;
using System.Collections.Generic;
using System.Linq;

namespace KoalaLang.ParserAndAST
{
    public sealed class Parser(Lexer.Lexer lexer)
    {
        private Lexer.Lexer _lexer = lexer;
        private List<Token> _tokens => _lexer.Tokens;

        private ASTNode code;
        private int _idx = 0;

        public void Parse()
        {
            //Unknown tokens errors
            if (_tokens.FindAll(x => x.Type == TokenType.Unknown).Count != 0)
            {
                foreach (Token token in _tokens.Where(x => x.Type == TokenType.Unknown))
                {
                    string line = _lexer.GetStringLine(token.Line);
                    Console.Error.WriteLine($"Unknown token('{token.Value}') at ln: {token.Line}, col: {token.Column}: {line}");
                }
                Environment.Exit(-1);
            }

            code = ParseCodeBlock(initShift: false, tillTheLeftBrace: false);
        }

        ASTCodeBlock ParseCodeBlock(bool initShift = true, bool tillTheLeftBrace = true)
        {
            if (initShift) Next();
            ASTCodeBlock block = new ASTCodeBlock();

            try
            {
                while (_idx < _tokens.Count && (!tillTheLeftBrace || _tokens[_idx].Type != TokenType.RBrace) && _tokens[_idx].Type != TokenType.EOF)
                {
                    if (_tokens[_idx].Type == TokenType.Keyword)
                    {
                        if (_tokens[_idx].Value == "fn") block.Nodes.Add(ParseFunction());
                        else if (_tokens[_idx].Value == "return")
                        {
                            //TODO: parsing of expressions
                            FatalNext(TokenType.Number);
                            string val = _tokens[_idx].Value;
                            FatalNext(TokenType.Semicolon);

                            block.Nodes.Add(new ASTReturn(new ASTConstant<string>(val)));
                        }
                    }
                    else throw new Exception("Unknown token inside body");

                    Next();
                }
            }
            catch(Exception e)
            {
                Console.Error.WriteLine($"PARSER ERROR: \"{e.Message}\" at ln: {_tokens[_idx].Line}, col: {_tokens[_idx].Column}: {_lexer.GetStringLine(_tokens[_idx].Line)}");
                Environment.Exit(-1);
            }

            return block;
        }

        ASTFunction ParseFunction()
        {
            ASTFunction function = new ASTFunction();

            FatalNext(TokenType.Identifier);
            function.FunctionName = _tokens[_idx].Value;

            FatalNext(TokenType.LParen);
            //TODO: args
            FatalNext(TokenType.RParen);

            FatalNext(TokenType.Colon);

            FatalNext(TokenType.Identifier);
            function.ReturnTypeName = _tokens[_idx].Value;

            FatalNext(TokenType.LBrace);
            function.Body = ParseCodeBlock();

            return function;
        }

        void Next()
        {
            if (_idx + 1 < _tokens.Count) _idx += 1;
        }

        void FatalNext(TokenType type)
        {
            Next();
            if (_tokens[_idx].Type != type)
            {
                string line = _lexer.GetStringLine(_tokens[_idx].Line);
                Console.Error.WriteLine($"Unexpected token({_tokens[_idx].Type}: \"{_tokens[_idx].Value}\") at ln: {_tokens[_idx].Line}, col: {_tokens[_idx].Column}; expected: {type}: {line}");
                Environment.Exit(-1);
            }
        }

        internal ASTNode GetAST() => code;
    }
}
