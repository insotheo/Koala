using System.Collections.Generic;
using SkullLang.Compiler.Parsers.ASTNodes;

using static SkullLang.Compiler.Parsers.ParserStatement;

namespace SkullLang.Compiler.Parsers
{
    public sealed class Parser
    {
        ParserContext _ctx;
        public bool IsSuccess => _ctx.IsParsingSuccess;
        public IReadOnlyList<ASTNode> AST => _ctx.Nodes;

        public Parser(Lexer lexer)
        {
            _ctx = new(lexer);
        }

        public void Parse()
        {
            while(_ctx.isIdxInRange && _ctx.NotEOF)
                ParseDeclaration();
        }

        void ParseDeclaration()
        {
            if (_ctx.Current.Type == TokenType.FuncKW) ParseFunction(_ctx);
            else
            {
                _ctx.Panic($"Unknown declaration: {_ctx.GetLine(_ctx.Current.Ln)}");
                _ctx.Sync(TokenType.FuncKW);
            }
        }
    }
}