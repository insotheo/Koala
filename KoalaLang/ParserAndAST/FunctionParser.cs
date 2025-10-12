using KoalaLang.Lexer;
using KoalaLang.ParserAndAST.AST;
using System;
using System.Collections.Generic;

namespace KoalaLang.ParserAndAST
{
    internal sealed class FunctionParser
    {
        private readonly ParserContext _ctx;
        private readonly StatementParser _statementParser;

        internal FunctionParser(ParserContext context, StatementParser statementParser)
        {
            _ctx = context;
            _statementParser = statementParser;
        }

        internal ASTFunction ParseFunction() //func funcName<Generic, Types>(args): return_type
        {
            ASTFunction function = new ASTFunction(_ctx.Current.Line);

            _ctx.ExpectNext(TokenType.Identifier);
            function.FunctionName = _ctx.Current.Value;
                _ctx.Next();
            if (_ctx.Current.Type == TokenType.Less)//Generic types, optional
            {
                _ctx.Next();
                List<string> genericTypes = new();
                while (!_ctx.End && _ctx.Current.Type != TokenType.More)
                {
                    if (_ctx.Current.Type != TokenType.Identifier) break;
                    string type = _ctx.Current.Value;
                    if (genericTypes.Contains(type)) throw new Exception($"Generic type '{type}' is already defined for functino '{function.FunctionName}'");
                    genericTypes.Add(type);

                    _ctx.Next();
                    if (_ctx.Current.Type != TokenType.Comma) break;
                    _ctx.Next();
                }
                function.GenericTypes = genericTypes;

                _ctx.Expect(TokenType.More);
                _ctx.ExpectNext(TokenType.LParen);
            }
            else _ctx.Expect(TokenType.LParen);
            _ctx.Next();

            Dictionary<string, string> args = new Dictionary<string, string>();
            while (!_ctx.End && _ctx.Current.Type != TokenType.RParen)
            {
                if (_ctx.Current.Type != TokenType.Identifier)
                {
                    break;
                }
                string identifier = _ctx.Current.Value;
                if (args.ContainsKey(identifier))
                {
                    throw new Exception($"Argument '{identifier}' already exists in function '{function.FunctionName}'");
                }

                _ctx.ExpectNext(TokenType.Colon);
                _ctx.ExpectNext(TokenType.Identifier);

                string typeName = _statementParser.ParseType();
                args.Add(identifier, typeName);

                if (_ctx.Current.Type != TokenType.Comma) break;
                _ctx.Next();
            }
            function.Args = args;
            _ctx.Expect(TokenType.RParen);

            _ctx.Next();
            if (_ctx.Current.Type == TokenType.Colon)
            {
                _ctx.Next();
                function.ReturnTypeName = _statementParser.ParseType();
                _ctx.Expect(TokenType.LBrace);
            }
            else if (_ctx.Current.Type == TokenType.LBrace)
            {
                function.ReturnTypeName = "void";
            }
            else _ctx.Expect(TokenType.Colon);

            function.Body = _statementParser.ParseCodeBlock();

            return function;
        }
    }
}