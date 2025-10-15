using KoalaLang.Lexer;
using KoalaLang.ParserAndAST.AST;
using System;
using System.Collections.Generic;

namespace KoalaLang.ParserAndAST
{
    internal sealed class StatementParser
    {
        private readonly ParserContext _ctx;
        private readonly ExpressionParser _expressionParser;
        private readonly FunctionParser _functionParser;

        internal StatementParser(ParserContext context)
        {
            _ctx = context;
            _expressionParser = new(context, this);
            _functionParser = new(context, this);
        }

        internal ASTCodeBlock ParseStatementList(TokenType endToken)
        {
            ASTCodeBlock block = new ASTCodeBlock(-1);
            while (!_ctx.End && _ctx.Current.Type != endToken && _ctx.Current.Type != TokenType.EOF)
            {
                ASTNode statement = ParseStatement();
                if (statement != null) block.Nodes.Add(statement);
            }

            return block;
        }

        ASTNode ParseStatement(bool ignoreSemicolonCheck = false)
        {
            Token token = _ctx.Current;

            if (token.Type == TokenType.Keyword)
            {
                ASTNode statement = token.Value switch
                {
                    "func" => _functionParser.ParseFunction(),
                    "return" => ParseReturn(),
                    "let" => ParseVariableDeclaration(),
                    "if" => ParseBranch(),
                    "while" => ParseWhileLoop(),
                    "do" => ParseDoWhileLoop(),
                    "for" => ParseForLoop(),
                    "break" => new ASTBreak(_ctx.Current.Line),
                    "continue" => new ASTContinue(_ctx.Current.Line),

                    _ => throw new Exception($"Unknown keyword '{token.Value}'")
                };

                if (!ignoreSemicolonCheck && token.Value is "return" or "let")
                {
                    _ctx.Expect(TokenType.Semicolon);
                    _ctx.Next();
                }

                return statement;
            }

            else if (token.Type == TokenType.Identifier)
            {
                ASTNode statement = ParseIdentifierStatement();

                if (!ignoreSemicolonCheck)
                {
                    _ctx.Expect(TokenType.Semicolon);
                    _ctx.Next();
                }

                return statement;
            }

            throw new Exception($"Unexpected token '{token.Value}' of type {token.Type} in statement");
        }
        internal ASTCodeBlock ParseCodeBlock()
        {
            _ctx.Expect(TokenType.LBrace);
            _ctx.Next();
            ASTCodeBlock block = ParseStatementList(TokenType.RBrace);
            _ctx.Expect(TokenType.RBrace);
            _ctx.Next();
            return block;
        }

        ASTReturn ParseReturn()
        {
            int line = _ctx.Current.Line;

            _ctx.Next();

            ASTNode expr = _ctx.Current.Type == TokenType.Semicolon ? null : _expressionParser.ParseExpression();

            return new ASTReturn(expr, line);
        }
        ASTNode ParseVariableDeclaration()
        {
            int line = _ctx.Current.Line;

            _ctx.ExpectNext(TokenType.Identifier);
            string varName = _ctx.Current.Value;

            _ctx.ExpectNext(TokenType.Colon);
            _ctx.ExpectNext(TokenType.Identifier);
            string typeName = ParseType();

            ASTVariableDeclaration varDecl = new ASTVariableDeclaration(varName, typeName, line);

            if (_ctx.Current.Type == TokenType.AssignmentSign)
            {
                _ctx.Next();
                ASTNode expr = _expressionParser.ParseExpression();

                return new ASTCompoundStatement(varDecl, new ASTAssignment(new ASTIdentifier(varDecl.Name, line), expr, line), line);
            }
            else if (_ctx.Current.Type == TokenType.Semicolon)
            {
                return varDecl;
            }

            else throw new Exception("Invalid variable declaration signature");
        }
        ASTWhileLoop ParseWhileLoop()
        {
            int line = _ctx.Current.Line;

            _ctx.ExpectNext(TokenType.LParen);
            _ctx.Next();
            ASTNode cond = _expressionParser.ParseExpression();
            _ctx.Expect(TokenType.RParen);

            _ctx.ExpectNext(TokenType.LBrace);
            ASTCodeBlock body = ParseCodeBlock();

            return new ASTWhileLoop(cond, body, line);
        }
        ASTDoWhileLoop ParseDoWhileLoop()
        {
            int line = _ctx.Current.Line;

            _ctx.ExpectNext(TokenType.LBrace);
            ASTCodeBlock body = ParseCodeBlock();

            _ctx.ExpectNext(TokenType.Keyword);
            if (_ctx.Current.Value != "while") _ctx.Fatal("Invalid do-while loop signature!");
            _ctx.ExpectNext(TokenType.LParen);
            _ctx.Next();
            ASTNode cond = _expressionParser.ParseExpression();
            _ctx.Expect(TokenType.RParen);
            _ctx.Next();

            return new ASTDoWhileLoop(cond, body, line);
        }
        ASTForLoop ParseForLoop()
        {
            int line = _ctx.Current.Line;

            _ctx.ExpectNext(TokenType.LParen);
            _ctx.Next();

            ASTNode init = ParseStatement(true);
            _ctx.Expect(TokenType.Semicolon);
            _ctx.Next();

            ASTNode condition = _expressionParser.ParseExpression();
            _ctx.Expect(TokenType.Semicolon);
            _ctx.Next();

            ASTNode iter = ParseStatement(true);
            _ctx.Expect(TokenType.RParen);
            _ctx.Next();

            _ctx.Expect(TokenType.LBrace);
            ASTCodeBlock body = ParseCodeBlock();

            return new ASTForLoop(init, condition, iter, body, line);
        }
        ASTNode ParseIdentifierStatement()
        {
            int line = _ctx.Current.Line;

            string identifier = _ctx.Current.Value;
            ASTNode left = _expressionParser.ParseFactor();

            if (_ctx.Current.Type == TokenType.AssignmentSign)
            {
                _ctx.Next();
                ASTNode value = _expressionParser.ParseExpression();
                return new ASTAssignment(left, value, line);
            }
            return left;
        }

        //enters with identifier "if"
        ASTBranch ParseBranch()
        {
            void ParseIF(List<ASTConditionBlock> ifs)
            {
                int line = _ctx.Current.Line;

                _ctx.ExpectNext(TokenType.LParen);
                _ctx.Next();
                ASTNode cond = _expressionParser.ParseExpression();
                _ctx.Expect(TokenType.RParen);

                _ctx.ExpectNext(TokenType.LBrace);
                ASTCodeBlock body = ParseCodeBlock();

                ifs.Add(new(cond, body, line));
            }

            List<ASTConditionBlock> ifs = new();
            ASTCodeBlock @else = null;
            while (!_ctx.End && (_ctx.Current.Value == "if" || _ctx.Current.Value == "else"))
            {
                if (_ctx.Current.Type == TokenType.Keyword)
                {
                    if (_ctx.Current.Value == "if")
                    {
                        ParseIF(ifs);
                    }
                    else if (_ctx.Current.Value == "else")
                    {
                        _ctx.Next();
                        if (_ctx.Current.Type == TokenType.Keyword && _ctx.Current.Value == "if") //else if
                        {
                            ParseIF(ifs);
                        }
                        else if (_ctx.Current.Type == TokenType.LBrace)
                        {
                            @else = ParseCodeBlock();
                        }
                    }
                    else break;
                }
                else break;
            }
            return new(ifs, @else, -1);
        }

        internal string ParseType()
        {
            _ctx.Expect(TokenType.Identifier);
            string typeName = _ctx.Current.Value;
            _ctx.Next();

            while (!_ctx.End)
            {
                if (_ctx.Current.Type == TokenType.LBracket)
                {
                    _ctx.ExpectNext(TokenType.RBracket);
                    _ctx.Next();
                    typeName += "[]";
                }
                else if(_ctx.Current.Type == TokenType.Less)
                {
                    _ctx.Next();
                    string genericType = ParseType();
                    _ctx.Expect(TokenType.More);
                    _ctx.Next();
                    typeName += "<" + genericType + ">";
                }
                else break;
            }

            return typeName.Trim();
        }
    }
}
