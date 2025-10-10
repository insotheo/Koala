using KoalaLang.Lexer;
using KoalaLang.ParserAndAST.AST;
using System;
using System.Collections.Generic;
using System.Linq;

namespace KoalaLang.ParserAndAST
{

    public sealed class Parser
    {
        private readonly ParserContext _ctx;
        private ASTNode _astRoot;

        public Parser(Lexer.Lexer lexer)
        {
            _ctx = new(lexer);
        }

        public void Parse()
        {
            foreach(Token token in _ctx.Tokens)
            {
                if (token.Type == TokenType.Unknown)
                    _ctx.Fatal($"Unknown token '{token.Value}'");
            }

            StatementParser statementParser = new(_ctx);
            _astRoot = statementParser.ParseStatementList(TokenType.EOF);
            _ctx.Expect(TokenType.EOF);
        }

        internal ASTNode GetAST() => _astRoot;
    }
}
