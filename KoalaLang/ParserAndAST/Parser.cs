using KoalaLang.Lexer;
using KoalaLang.ParserAndAST.AST;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;

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
            FatalNext(TokenType.EOF);
        }

        ASTCodeBlock ParseCodeBlock(bool initShift = true, bool tillTheLeftBrace = true)
        {
            if (initShift) Next();
            ASTCodeBlock block = new ASTCodeBlock(-1);

            try
            {
                while (_idx < _tokens.Count && (!tillTheLeftBrace || _tokens[_idx].Type != TokenType.RBrace) && _tokens[_idx].Type != TokenType.EOF)
                {
                    if (_tokens[_idx].Type == TokenType.Keyword)
                    {
                        if (_tokens[_idx].Value == "func") block.Nodes.Add(ParseFunction());

                        else if (_tokens[_idx].Value == "return")
                        {
                            int line = _tokens[_idx].Line;

                            Next();

                            ASTNode expr = _tokens[_idx].Type == TokenType.Semicolon ? null : ParseExpression(line);
                            FatalCheck(TokenType.Semicolon);

                            block.Nodes.Add(new ASTReturn(expr, line));
                        }

                        else if( _tokens[_idx].Value == "let") //let <identifier>: <identifier>
                        {
                            int line = _tokens[_idx].Line;

                            FatalNext(TokenType.Identifier);
                            string varName = _tokens[_idx].Value;

                            FatalNext(TokenType.Colon);
                            FatalNext(TokenType.Identifier);
                            string typeName = _tokens[_idx].Value;

                            Next();

                            ASTVariableDeclaration varDecl = new ASTVariableDeclaration(varName, typeName, line);

                            if (_tokens[_idx].Type == TokenType.AssignmentSign)
                            {
                                Next();
                                ASTNode expr = ParseExpression(line);
                                FatalCheck(TokenType.Semicolon);

                                block.Nodes.Add(varDecl);
                                block.Nodes.Add(new ASTAssignment(varName, expr, line));
                            }
                            else if (_tokens[_idx].Type == TokenType.Semicolon) block.Nodes.Add(varDecl);

                            else throw new Exception($"Invalid variable declaration signature");
                        }

                        else if (_tokens[_idx].Value == "if")
                        {
                            block.Nodes.Add(ParseBranch());
                        }
                    }

                    else if (_tokens[_idx].Type == TokenType.Identifier)
                    {
                        int line = _tokens[_idx].Line;

                        string identifier = _tokens[_idx].Value;
                        Next();

                        if (_tokens[_idx].Type == TokenType.AssignmentSign)
                        {
                            Next();
                            ASTNode expr = ParseExpression(line);
                            FatalCheck(TokenType.Semicolon);
                            block.Nodes.Add(new ASTAssignment(identifier, expr, line));
                        }
                        else if (_tokens[_idx].Type == TokenType.LParen)
                        {
                            block.Nodes.Add(ParseFunctionCall(identifier, line));
                        }
                    }

                    else if (_tokens[_idx].Type == TokenType.LBrace)
                    {
                        ASTCodeBlock innerBlock = ParseCodeBlock();
                        block.Nodes.Add(innerBlock);
                        FatalCheck(TokenType.RBrace);
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
            ASTFunction function = new ASTFunction(_tokens[_idx].Line);

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
                    throw new Exception($"Argument '{identifier}' already exists in function '{function.FunctionName}'");
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

            Next();
            if (_tokens[_idx].Type == TokenType.Colon)
            {
                FatalNext(TokenType.Identifier);
                function.ReturnTypeName = _tokens[_idx].Value;
                FatalNext(TokenType.LBrace);
            }
            else if (_tokens[_idx].Type == TokenType.LBrace)
            {
                function.ReturnTypeName = "void";
            }
            else FatalCheck(TokenType.Colon);
            
            function.Body = ParseCodeBlock();

            return function;
        }


        //enters with identifier "if"
        ASTBranch ParseBranch()
        {
            void ParseIF(List<ASTConditionBlock> ifs)
            {
                int line = _tokens[_idx].Line;

                FatalNext(TokenType.LParen);
                Next();
                ASTNode cond = ParseExpression(_tokens[_idx].Line);
                FatalCheck(TokenType.RParen);

                FatalNext(TokenType.LBrace);
                ASTCodeBlock body = ParseCodeBlock();
                FatalCheck(TokenType.RBrace);

                ifs.Add(new(cond, body, line));
            }

            List<ASTConditionBlock> ifs = new();
            ASTCodeBlock @else = null;
            while (_idx < _tokens.Count && (_tokens[_idx].Value == "if" || _tokens[_idx].Value == "else"))
            {
                if (_tokens[_idx].Type == TokenType.Keyword)
                {
                    if (_tokens[_idx].Value == "if")
                    {
                        ParseIF(ifs);
                    }
                    else if (_tokens[_idx].Value == "else")
                    {
                        Next();
                        if (_tokens[_idx].Type == TokenType.Keyword && _tokens[_idx].Value == "if") //else if
                        {
                            ParseIF(ifs);
                        }
                        else if (_tokens[_idx].Type == TokenType.LBrace)
                        {
                            @else = ParseCodeBlock();
                            FatalCheck(TokenType.RBrace);
                        }
                    }
                    else break;
                }
                else break;

                Next();
            }
            return new(ifs, @else, -1);
        }
       
        ASTNode ParseExpression(int line) => ParseLogicalOr(line);

        ASTNode ParseLogicalOr(int line)
        {
            ASTNode left = ParseLogicalAnd(line);
            while (_idx < _tokens.Count && _tokens[_idx].Type == TokenType.LogicalOr)
            {
                Next();
                ASTNode right = ParseLogicalAnd(line);
                left = new ASTBinOperation(left, BinOperationType.LogicalOr, right, line);
            }
            return left;
        }
        ASTNode ParseLogicalAnd(int line)
        {
            ASTNode left = ParseBitwiseOr(line);
            while (_idx < _tokens.Count && _tokens[_idx].Type == TokenType.LogicalAnd)
            {
                Next();
                ASTNode right = ParseBitwiseOr(line);
                left = new ASTBinOperation(left, BinOperationType.LogicalAnd, right, line);
            }
            return left;
        }
        ASTNode ParseBitwiseOr(int line)
        {
            ASTNode left = ParseXor(line);
            while (_idx < _tokens.Count && _tokens[_idx].Type == TokenType.BitwiseOr)
            {
                Next();
                ASTNode right = ParseXor(line);
                left = new ASTBinOperation(left, BinOperationType.BitwiseOr, right, line);
            }
            return left;
        }
        ASTNode ParseXor(int line)
        {
            ASTNode left = ParseBitwiseAnd(line);
            while (_idx < _tokens.Count && _tokens[_idx].Type == TokenType.Xor)
            {
                Next();
                ASTNode right = ParseBitwiseAnd(line);
                left = new ASTBinOperation(left, BinOperationType.Xor, right, line);
            }
            return left;
        }
        ASTNode ParseBitwiseAnd(int line)
        {
            ASTNode left = ParseShift(line);
            while (_idx < _tokens.Count && _tokens[_idx].Type == TokenType.BitwiseAnd)
            {
                Next();
                ASTNode right = ParseShift(line);
                left = new ASTBinOperation(left, BinOperationType.BitwiseAnd, right, line);
            }
            return left;
        }
        ASTNode ParseShift(int line)
        {
            ASTNode left = ParseArithmetic(line);
            while (_idx < _tokens.Count && (_tokens[_idx].Type == TokenType.LeftShift || _tokens[_idx].Type == TokenType.RightShift))
            {
                BinOperationType op = _tokens[_idx].Type == TokenType.LeftShift ? BinOperationType.LeftShift : BinOperationType.RightShift;
                Next();
                ASTNode right = ParseArithmetic(line);
                left = new ASTBinOperation(left, op, right, line);
            }
            return left;
        }
        ASTNode ParseArithmetic(int line)
        {
            ASTNode left = ParseTerm(line);
            while(_idx < _tokens.Count && (_tokens[_idx].Type == TokenType.Plus || _tokens[_idx].Type == TokenType.Minus))
            {
                BinOperationType op = _tokens[_idx].Type == TokenType.Plus ? BinOperationType.Add : BinOperationType.Subtract;
                Next();
                ASTNode right = ParseTerm(line);
                left = new ASTBinOperation(left, op, right, line);
            }
            return left;
        }
        ASTNode ParseTerm(int line)
        {
            ASTNode left = ParseFactor(line);
            while (_idx < _tokens.Count && (_tokens[_idx].Type == TokenType.Asterisk || _tokens[_idx].Type == TokenType.Slash || _tokens[_idx].Type == TokenType.Percent))
            {
                BinOperationType op = _tokens[_idx].Type switch
                {
                    TokenType.Asterisk => BinOperationType.Multiply,
                    TokenType.Slash => BinOperationType.Divide,
                    TokenType.Percent => BinOperationType.Remain,

                    _ => throw new Exception("Unknown operation in expression")
                };

                Next();
                ASTNode right = ParseFactor(line);
                left = new ASTBinOperation(left, op, right, line);
            }
            return left;
        }
        ASTNode ParseFactor(int line)
        {
            if (_tokens[_idx].Type == TokenType.Number)
            {
                ASTNode number = new ASTConstant<int>(int.Parse(_tokens[_idx].Value), line);
                Next();
                return number;
            }
            else if (_tokens[_idx].Type == TokenType.FloatNumber)
            {
                ASTNode number = new ASTConstant<float>(float.Parse(_tokens[_idx].Value), line);
                Next();
                return number;
            }
            else if (_tokens[_idx].Type == TokenType.BooleanValue)
            {
                ASTNode boolean = new ASTConstant<bool>(_tokens[_idx].Value == "true" ? true : false, line);
                Next();
                return boolean;
            }

            else if (_tokens[_idx].Type == TokenType.Identifier)
            {
                string identifier = _tokens[_idx].Value;
                Next();

                if (_tokens[_idx].Type == TokenType.LParen)
                {
                    return ParseFunctionCall(identifier, line);
                }
                else return new ASTVariableUse(identifier, line);
            }

            else if (_tokens[_idx].Type == TokenType.Minus)
            {
                Next();
                ASTNode expr = ParseFactor(line);
                return new ASTUnOperation(UnaryOperationType.Negate, expr, line);
            }
            else if (_tokens[_idx].Type == TokenType.LogicalNot)
            {
                Next();
                ASTNode expr = ParseFactor(line);
                return new ASTUnOperation(UnaryOperationType.LogicalNot, expr, line);
            }
            else if (_tokens[_idx].Type == TokenType.BitwiseNot)
            {
                Next();
                ASTNode expr = ParseFactor(line);
                return new ASTUnOperation(UnaryOperationType.BitwiseNot, expr, line);
            }

            else if (_tokens[_idx].Type == TokenType.LParen)
            {
                Next();
                ASTNode expr = ParseExpression(line);
                FatalCheck(TokenType.RParen);
                Next();
                return expr;
            }

            throw new Exception("Unexpected token in expression");
        }

        //Token before call is LParen
        private ASTNode ParseFunctionCall(string identifier, int line)
        {
            Next(); //skips LParen
            List<ASTNode> args = new();
            while (_idx < _tokens.Count && _tokens[_idx].Type != TokenType.RParen)
            {
                ASTNode expression = ParseExpression(line);
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

            return new ASTFunctionCall(identifier, args, line);
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
                Console.Error.WriteLine($"Error at line {_tokens[_idx].Line}, col {_tokens[_idx].Column}: Unexpected token{(_tokens[_idx].Value != String.Empty ? " '" + _tokens[_idx].Value + "' " : " ")}of type {_tokens[_idx].Type}; expected token of type {type}: {line}");
                Console.Error.WriteLine($"Unexpected token{(_tokens[_idx].Value != String.Empty ? " '" + _tokens[_idx].Value + "' " : " ")}of type {_tokens[_idx].Type} at line {_tokens[_idx].Line}, col {_tokens[_idx].Column}; expected token of type {type}: {line}");
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
