using static SkullLang.Compiler.Parsers.ParserStatement;

namespace SkullLang.Compiler.Parsers
{
    public sealed class Parser
    {
        ParserContext _ctx;

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
            if(_ctx.Current.Type == TokenType.FuncKW) ParseFunction(_ctx);
        }
    }
}