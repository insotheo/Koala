using KoalaLang.Lexer;
using KoalaLang.ParserAndAST.AST;
using System;

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

            try
            {
                StatementParser statementParser = new(_ctx);
                _astRoot = statementParser.ParseStatementList(TokenType.EOF);
                _ctx.Expect(TokenType.EOF);
            }
            catch(Exception ex)
            {
                string line = _ctx.Lexer.GetStringLine(_ctx.Current.Line);
                Console.Error.WriteLine($"Parser error: {ex.Message} at line {_ctx.Current.Line}, col {_ctx.Current.Column}: {line}");
                Environment.Exit(-1);
            }
        }

        internal ASTNode GetAST() => _astRoot;
    }
}
