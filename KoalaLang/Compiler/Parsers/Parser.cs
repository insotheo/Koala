using System.Collections.Generic;
using KoalaLang.Compiler.Parsers.ASTNodes;

using static KoalaLang.Compiler.Parsers.ParserStatement;

namespace KoalaLang.Compiler.Parsers
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
            if (_ctx.Current.Type == TokenType.FuncKW) _ctx.PushNode(ParseFunction(_ctx));
            else if (_ctx.Current.Type == TokenType.StructKW) ParseStruct(_ctx);
            else
            {
                _ctx.Panic($"Unknown declaration: {_ctx.GetLine(_ctx.Current.Ln)}");
                _ctx.Sync(TokenType.FuncKW);
                _ctx.StepBack();
            }
        }
    }
}