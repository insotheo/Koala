using SkullLang.Compiler.Parsers.ASTNodes;

namespace SkullLang.Compiler.Parsers
{
    internal static class ParserExpression
    {
        internal static ASTNode ParseExpression(ParserContext ctx)
        {
            ulong ln = ctx.Current.Ln, col = ctx.Current.Col;

            var left = ParseTerm(ctx);
            while (ctx.NotEOF && (ctx.Current.Type == TokenType.Plus || ctx.Current.Type == TokenType.Minus))
            {
                var type = ctx.Current.Type == TokenType.Plus ? BinaryOpType.Add : BinaryOpType.Sub;
                ctx.Next();
                var right = ParseTerm(ctx);
                left = new ASTBinaryOp(left, right, type, ln, col);
            }
            return left;
        }

        internal static ASTNode ParseTerm(ParserContext ctx)
        {
            ulong ln = ctx.Current.Ln, col = ctx.Current.Col;

            var left = ParseFactor(ctx);
            ctx.Next();
            while (ctx.NotEOF && (ctx.Current.Type == TokenType.Asterisk || ctx.Current.Type == TokenType.Slash || ctx.Current.Type == TokenType.Persent))
            {
                var type = ctx.Current.Type switch
                {
                    TokenType.Asterisk => BinaryOpType.Mul,
                    TokenType.Slash => BinaryOpType.Div,
                    TokenType.Persent => BinaryOpType.Mod,
                    _ => BinaryOpType.None
                };
                ctx.Next();
                var right = ParseFactor(ctx);
                ctx.Next();
                left = new ASTBinaryOp(left, right, type, ln, col);
            }
            return left;
        }

        internal static ASTNode ParseFactor(ParserContext ctx)
        {
            ulong ln = ctx.Current.Ln, col = ctx.Current.Col;

            if (ctx.Current.Type == TokenType.NumberI) return new ASTConstantInt(ulong.Parse(ctx.Current.Value), ln, col);
            if (ctx.Current.Type == TokenType.NumberF) return new ASTConstantFloat(double.Parse(ctx.Current.Value), ln, col);

            if (ctx.Current.Type == TokenType.Identifier)
            {
                string identifier = ctx.Current.Value;
                if (ctx.Peek(1).Type == TokenType.RParen) return ParseFunctionCall(ctx, identifier);

                return new ASTIdentifier(identifier, ln, col);
            }

            if (ctx.Current.Type == TokenType.LParen)
            {
                //consume (
                ctx.Next();

                var expr = ParseExpression(ctx);

                if (!ctx.Expect(TokenType.RParen)) { ctx.Sync(TokenType.RParen, TokenType.Semicolon); return expr; }

                return expr;
            }

            ctx.Panic("Failed to parse expression");
            ctx.Sync(TokenType.Semicolon);
            return null;
        }

        internal static ASTNode ParseFunctionCall(ParserContext ctx, string fname)
        {
            ulong ln = ctx.Current.Ln, col = ctx.Current.Col;
            //consume (
            ctx.Next();

            //TODO: FUNCTION CALL
            return new ASTIdentifier(fname, ln, col);
        }
    }
}
