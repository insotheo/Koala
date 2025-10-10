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

            code = ParseStatementList(TokenType.EOF);
            FatalNext(TokenType.EOF);
        }

        #region Statements
        ASTCodeBlock ParseStatementList(TokenType endToken)
        {
            ASTCodeBlock code = new(-1);

            try
            {
                while(_idx < _tokens.Count && _tokens[_idx].Type != endToken && _tokens[_idx].Type != TokenType.EOF)
                {
                    ASTNode statement = ParseStatement();
                    if(statement != null)
                        code.Nodes.Add(statement);
                }
            }
            catch(Exception ex)
            {
                Console.Error.WriteLine($"PARSER ERROR: \"{ex.Message}\" at ln: {_tokens[_idx].Line}, col: {_tokens[_idx].Column}: {_lexer.GetStringLine(_tokens[_idx].Line)}");
                Environment.Exit(-1);
            }

            return code;
        }

        ASTCodeBlock ParseCodeBlock()
        {
            FatalCheck(TokenType.LBrace);
            Next();
            ASTCodeBlock block = ParseStatementList(TokenType.RBrace);
            FatalCheck(TokenType.RBrace);
            Next();
            return block;
        }

        ASTNode ParseStatement(bool ignoreSemicolonCheck = false)
        {
            Token token = _tokens[_idx];

            if (token.Type == TokenType.Keyword)
            {
                ASTNode statement = token.Value switch
                {
                    "func" => ParseFunction(),
                    "return" => ParseReturn(),
                    "let" => ParseVariableDeclaration(),
                    "if" => ParseBranch(),
                    "while" => ParseWhileLoop(),
                    "do" => ParseDoWhileLoop(),
                    "for" => ParseForLoop(),
                    "break" => ParseBreak(),
                    "continue" => ParseContinue(),

                    _ => throw new Exception($"Unknown keyword '{token.Value}'")
                };

                if(!ignoreSemicolonCheck && token.Value is "return" or "let")
                {
                    FatalCheck(TokenType.Semicolon);
                    Next();
                }

                return statement;
            }

            else if (token.Type == TokenType.Identifier)
            {
                ASTNode statement = ParseIdentifierStatement();

                if (!ignoreSemicolonCheck)
                {
                    FatalCheck(TokenType.Semicolon);
                    Next();
                }

                return statement;
            }

            throw new Exception($"Unexpected token '{token.Value}' of type {token.Type} in statement");
        }

        ASTFunction ParseFunction() //func funcName<Generic, Types>(args): return_type
        {
            ASTFunction function = new ASTFunction(_tokens[_idx].Line);

            FatalNext(TokenType.Identifier);
            function.FunctionName = _tokens[_idx].Value;
            Next();
            if (_tokens[_idx].Type == TokenType.Less)//Generic types, optional
            {
                Next();
                List<string> genericTypes = new();
                while(_idx < _tokens.Count && _tokens[_idx].Type != TokenType.More)
                {
                    if (_tokens[_idx].Type != TokenType.Identifier) break;
                    string type = _tokens[_idx].Value;
                    if (genericTypes.Contains(type)) throw new Exception($"Generic type '{type}' is already defined for functino '{function.FunctionName}'");
                    genericTypes.Add(type);

                    Next();
                    if (_tokens[_idx].Type != TokenType.Comma) break;
                    Next();
                }
                function.GenericTypes = genericTypes;

                FatalCheck(TokenType.More);
                FatalNext(TokenType.LParen);
            }
            else FatalCheck(TokenType.LParen);
            Next();

            Dictionary<string, string> args = new Dictionary<string, string>();
            while (_idx < _tokens.Count && _tokens[_idx].Type != TokenType.RParen)
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
        ASTReturn ParseReturn()
        {
            int line = _tokens[_idx].Line;

            Next();

            ASTNode expr = _tokens[_idx].Type == TokenType.Semicolon ? null : ParseExpression(line);

            return new ASTReturn(expr, line);
        }
        ASTNode ParseVariableDeclaration()
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

                return new ASTCompoundStatement(varDecl, new ASTAssignment(varName, expr, line), line);
            }
            else if (_tokens[_idx].Type == TokenType.Semicolon)
            {
                return varDecl;
            }

            else throw new Exception("Invalid variable declaration signature");
        }
        ASTWhileLoop ParseWhileLoop()
        {
            int line = _tokens[_idx].Line;

            FatalNext(TokenType.LParen);
            Next();
            ASTNode cond = ParseExpression(_tokens[_idx].Line);
            FatalCheck(TokenType.RParen);

            FatalNext(TokenType.LBrace);
            ASTCodeBlock body = ParseCodeBlock();

            return new ASTWhileLoop(cond, body, line);
        }
        ASTDoWhileLoop ParseDoWhileLoop()
        {
            int line = _tokens[_idx].Line;

            FatalNext(TokenType.LBrace);
            ASTCodeBlock body = ParseCodeBlock();

            FatalNext(TokenType.Keyword);
            if (_tokens[_idx].Value != "while") throw new Exception("Invalid do-while loop signature!");
            FatalNext(TokenType.LParen);
            Next();
            ASTNode cond = ParseExpression(_tokens[_idx].Line);
            FatalCheck(TokenType.RParen);
            Next();

            return new ASTDoWhileLoop(cond, body, line);
        }
        ASTForLoop ParseForLoop()
        {
            int line = _tokens[_idx].Line;

            FatalNext(TokenType.LParen);
            Next();

            ASTNode init = ParseStatement(true);
            FatalCheck(TokenType.Semicolon);
            Next();

            ASTNode condition = ParseExpression(_tokens[_idx].Line);
            FatalCheck(TokenType.Semicolon);
            Next();

            ASTNode iter = ParseStatement(true);
            FatalCheck(TokenType.RParen);
            Next();

            FatalCheck(TokenType.LBrace);
            ASTCodeBlock body = ParseCodeBlock();

            return new ASTForLoop(init, condition, iter, body, line);
        }
        ASTContinue ParseContinue()
        {
            return new ASTContinue(_tokens[_idx].Line);
        }
        ASTBreak ParseBreak()
        {
            return new ASTBreak(_tokens[_idx].Line);
        }
        ASTNode ParseIdentifierStatement()
        {
            int line = _tokens[_idx].Line;

            string identifier = _tokens[_idx].Value;
            Next();

            if (_tokens[_idx].Type == TokenType.AssignmentSign)
            {
                Next();
                ASTNode expr = ParseExpression(line);
                return new ASTAssignment(identifier, expr, line);
            }
            else if (_tokens[_idx].Type == TokenType.LParen || _tokens[_idx].Type == TokenType.Less)
            {
                ASTFunctionCall call = ParseFunctionCall(identifier, line);
                return call;
            }

            throw new Exception("Unknown identifier statement");
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
                        }
                    }
                    else break;
                }
                else break;

                Next();
            }
            _idx -= 1; //step back to the } of the last cond. block
            return new(ifs, @else, -1);
        }
        #endregion

        #region Expressions
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
            ASTNode left = ParseEquality(line);
            while (_idx < _tokens.Count && _tokens[_idx].Type == TokenType.BitwiseAnd)
            {
                Next();
                ASTNode right = ParseEquality(line);
                left = new ASTBinOperation(left, BinOperationType.BitwiseAnd, right, line);
            }
            return left;
        }
        ASTNode ParseEquality(int line)
        {
            ASTNode left = ParseRelational(line);
            while (_idx < _tokens.Count && (_tokens[_idx].Type == TokenType.Equal || _tokens[_idx].Type == TokenType.Inequal))
            {
                BinOperationType op = _tokens[_idx].Type == TokenType.Equal ? BinOperationType.CmpEqual : BinOperationType.CmpInequal;
                Next();
                ASTNode right = ParseRelational(line);
                left = new ASTBinOperation(left, op, right, line);
            }
            return left;
        }
        ASTNode ParseRelational(int line)
        {
            ASTNode left = ParseShift(line);
            while (_idx < _tokens.Count && (_tokens[_idx].Type == TokenType.Less || _tokens[_idx].Type == TokenType.LessOrEqual ||
                                            _tokens[_idx].Type == TokenType.More || _tokens[_idx].Type == TokenType.MoreOrEqual))
            {
                BinOperationType op = _tokens[_idx].Type switch
                {
                    TokenType.Less => BinOperationType.CmpLess,
                    TokenType.LessOrEqual => BinOperationType.CmpLessOrEq,
                    TokenType.More => BinOperationType.CmpMore,
                    TokenType.MoreOrEqual => BinOperationType.CmpMoreOrEq,

                    _ => throw new Exception("Unknown operation in expression")
                };

                Next();
                ASTNode right = ParseShift(line);
                left = new ASTBinOperation(left, op, right, line);
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

                if (_tokens[_idx].Type == TokenType.LParen || _tokens[_idx].Type == TokenType.Less)
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
                int saveIdx = _idx;
                Next();

                if (_tokens[_idx].Type == TokenType.Identifier)
                {
                    string typeName = _tokens[_idx].Value;
                    Next();

                    if (_tokens[_idx].Type == TokenType.RParen)
                    {
                        try
                        {
                            Next();//skip )
                            ASTNode castExpr = ParseFactor(line);
                            return new ASTCast(typeName, castExpr, line);
                        }
                        catch { _idx = saveIdx + 1; }
                    }
                    else
                    {
                        _idx = saveIdx + 1;
                    }
                }
                ASTNode expr = ParseExpression(line);
                FatalCheck(TokenType.RParen);
                Next();
                return expr;
            }

            throw new Exception("Unexpected token in expression");
        }
        #endregion

        //Token before call is LParen or Less
        ASTFunctionCall ParseFunctionCall(string identifier, int line)
        {
            List<string> genericTypes = new();
            if (_tokens[_idx].Type == TokenType.Less)
            {
                Next();
                while(_idx < _tokens.Count && _tokens[_idx].Type != TokenType.More)
                {
                    if (_tokens[_idx].Type != TokenType.Identifier) break;
                    genericTypes.Add(_tokens[_idx].Value);
                    Next();
                    if (_tokens[_idx].Type != TokenType.Comma) break;
                    Next();
                }
                FatalCheck(TokenType.More);
                FatalNext(TokenType.LParen);
            }

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

            return new ASTFunctionCall(identifier, args, genericTypes, line);
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
