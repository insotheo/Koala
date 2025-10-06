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
                        if (_tokens[_idx].Value == "func") block.Nodes.Add(ParseFunction());

                        else if (_tokens[_idx].Value == "return")
                        {
                            Next();

                            ASTNode expr = _tokens[_idx].Type == TokenType.Semicolon ? null : ParseExpression();
                            FatalCheck(TokenType.Semicolon);

                            block.Nodes.Add(new ASTReturn(expr));
                        }

                        else if( _tokens[_idx].Value == "let") //let <identifier>: <identifier>
                        {
                            FatalNext(TokenType.Identifier);
                            string varName = _tokens[_idx].Value;

                            FatalNext(TokenType.Colon);
                            FatalNext(TokenType.Identifier);
                            string typeName = _tokens[_idx].Value;

                            Next();

                            ASTVariableDeclaration varDecl = new ASTVariableDeclaration(varName, typeName);

                            if (_tokens[_idx].Type == TokenType.AssignmentSign)
                            {
                                Next();
                                ASTNode expr = ParseExpression();
                                FatalCheck(TokenType.Semicolon);

                                block.Nodes.Add(varDecl);
                                block.Nodes.Add(new ASTAssignment(varName, expr));
                            }
                            else if (_tokens[_idx].Type == TokenType.Semicolon) block.Nodes.Add(varDecl);

                            else throw new Exception("Invalid variable declaration signature");
                        }
                    }

                    else if (_tokens[_idx].Type == TokenType.Identifier)
                    {
                        string identifier = _tokens[_idx].Value;
                        Next();

                        if (_tokens[_idx].Type == TokenType.AssignmentSign)
                        {
                            Next();
                            ASTNode expr = ParseExpression();
                            FatalCheck(TokenType.Semicolon);
                            block.Nodes.Add(new ASTAssignment(identifier, expr));
                        }
                        else if (_tokens[_idx].Type == TokenType.LParen)
                        {
                            block.Nodes.Add(ParseFunctionCall(identifier));
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
            Next();

            Dictionary<string, string> args = new Dictionary<string, string>();
            while(_idx < _tokens.Count && _tokens[_idx].Type != TokenType.RParen)
            {
                if (_tokens[_idx].Type != TokenType.Identifier)
                {
                    break;
                }
                string identifier = _tokens[_idx].Value;
                if (args.ContainsKey(identifier))
                {
                    throw new Exception($"Argument {identifier} already exists at functnion {function.FunctionName}");
                }

                FatalNext(TokenType.Colon);
                FatalNext(TokenType.Identifier);

                string typeName = _tokens[_idx].Value;
                args.Add(identifier, typeName);

                Next();
                if (_tokens[_idx].Type != TokenType.Comma) break;
                Next();
            }
            function.Args = args;
            FatalCheck(TokenType.RParen);

            FatalNext(TokenType.Colon);

            FatalNext(TokenType.Identifier);
            function.ReturnTypeName = _tokens[_idx].Value;

            FatalNext(TokenType.LBrace);
            function.Body = ParseCodeBlock();

            return function;
        }

        ASTNode ParseExpression()
        {
            ASTNode left = ParseTerm();
            while(_idx < _tokens.Count && (_tokens[_idx].Type == TokenType.Plus || _tokens[_idx].Type == TokenType.Minus))
            {
                BinOperationType op = _tokens[_idx].Type == TokenType.Plus ? BinOperationType.Add : BinOperationType.Subtract;
                Next();
                ASTNode right = ParseTerm();
                left = new ASTBinOperation(left, op, right);
            }
            return left;
        }
        ASTNode ParseTerm()
        {
            ASTNode left = ParseFactor();
            while (_idx < _tokens.Count && (_tokens[_idx].Type == TokenType.Asterisk || _tokens[_idx].Type == TokenType.Slash || _tokens[_idx].Type == TokenType.Percent))
            {
                BinOperationType op = _tokens[_idx].Type switch
                {
                    TokenType.Asterisk => BinOperationType.Multiply,
                    TokenType.Slash => BinOperationType.Divide,
                    TokenType.Percent => BinOperationType.Remain,

                    _ => throw new Exception("Unknow operation in expression!")
                };

                Next();
                ASTNode right = ParseFactor();
                left = new ASTBinOperation(left, op, right);
            }
            return left;
        }
        ASTNode ParseFactor()
        {
            if (_tokens[_idx].Type == TokenType.Number)
            {
                ASTNode number = new ASTConstant<int>(int.Parse(_tokens[_idx].Value));
                Next();
                return number;
            }
            else if (_tokens[_idx].Type == TokenType.FloatNumber)
            {
                ASTNode number = new ASTConstant<float>(float.Parse(_tokens[_idx].Value));
                Next();
                return number;
            }

            else if (_tokens[_idx].Type == TokenType.Identifier)
            {
                string identifier = _tokens[_idx].Value;
                Next();

                if (_tokens[_idx].Type == TokenType.LParen)
                {
                    return ParseFunctionCall(identifier);
                }
                else return new ASTVariableUse(identifier);
            }

            else if (_tokens[_idx].Type == TokenType.Minus)
            {
                Next();
                ASTNode expr = ParseFactor();
                return new ASTUnOperation(UnaryOperationType.Negate, expr);
            }

            else if (_tokens[_idx].Type == TokenType.LParen)
            {
                Next();
                ASTNode expr = ParseExpression();
                FatalCheck(TokenType.RParen);
                Next();
                return expr;
            }

            throw new Exception("Unexpected token in expression");
        }

        //Token before call is LParen
        private ASTNode ParseFunctionCall(string identifier)
        {
            Next(); //skips LParen
            List<ASTNode> args = new();
            while (_idx < _tokens.Count && _tokens[_idx].Type != TokenType.RParen)
            {
                ASTNode expression = ParseExpression();
                args.Add(expression);
                if (_tokens[_idx].Type == TokenType.Comma)
                {
                    Next();
                    continue;
                }
                else if (_tokens[_idx].Type == TokenType.RParen) break;
                else FatalCheck(TokenType.Comma);
            }
            FatalCheck(TokenType.RParen);
            Next();

            return new ASTFunctionCall(identifier, args);
        }

        void Next()
        {
            if (_idx + 1 < _tokens.Count) _idx += 1;
        }
        void FatalCheck(TokenType type)
        {
            if (_tokens[_idx].Type != type)
            {
                string line = _lexer.GetStringLine(_tokens[_idx].Line);
                Console.Error.WriteLine($"Unexpected token({_tokens[_idx].Type}: \"{_tokens[_idx].Value}\") at ln: {_tokens[_idx].Line}, col: {_tokens[_idx].Column}; expected: {type}: {line}");
                Environment.Exit(-1);
            }
        }
        void FatalNext(TokenType type)
        {
            Next();
            FatalCheck(type);
        }

        internal ASTNode GetAST() => code;
    }
}
