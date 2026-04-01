using SkullLang.Compiler.Parsers.ASTNodes;

namespace SkullLang.Compiler.Parsers
{
    internal static class ParserExpression
    {
        const int UNARY_PRECEDENCE = 7;

        internal static ASTNode ParseExpression(ParserContext ctx, int precedence = 0)
        {
            int GetOpPrecedence(TokenType type) => type switch
            {
                TokenType.Pipe => 1,

                TokenType.Caret => 2,

                TokenType.Ampersand => 3,

                TokenType.LeftShift => 4,
                TokenType.RightShift => 4,

                TokenType.Plus => 5,
                TokenType.Minus => 5,

                TokenType.Asterisk => 6,
                TokenType.Slash => 6,
                TokenType.Percent => 6,

                _ => -1,
            };

            BinaryOpType MapBinaryOp(TokenType type) => type switch
            {
                TokenType.Plus => BinaryOpType.Add,
                TokenType.Minus => BinaryOpType.Sub,
                TokenType.Asterisk => BinaryOpType.Mul,
                TokenType.Slash => BinaryOpType.Div,
                TokenType.Ampersand => BinaryOpType.BitwiseAnd,
                TokenType.Pipe => BinaryOpType.BitwiseOr,
                TokenType.Caret => BinaryOpType.BitwiseXor,
                TokenType.LeftShift => BinaryOpType.BitwiseLShift,
                TokenType.RightShift => BinaryOpType.BitwiseRShift,

                _ => BinaryOpType.None,
            };

            var left = ParsePrimary(ctx);

            while (ctx.NotEOF)
            {
                var currentPrecedence = GetOpPrecedence(ctx.Current.Type);
                if (currentPrecedence <= precedence)
                    break;

                var opToken = ctx.Current;
                ctx.Next();

                var right = ParseExpression(ctx, currentPrecedence);

                left = new ASTBinaryOp(left, right, MapBinaryOp(opToken.Type), opToken.Ln, opToken.Col);
            }

            return left;
        }

        internal static ASTNode ParsePrimary(ParserContext ctx)
        {
            ulong ln = ctx.Current.Ln, col = ctx.Current.Col;

            if (ctx.Current.Type == TokenType.NumberI)
            {
                var node = new ASTConstantInt(ulong.Parse(ctx.Current.Value), ln, col);
                ctx.Next();
                return node;
            }
            if (ctx.Current.Type == TokenType.NumberF)
            {
                var node = new ASTConstantFloat(double.Parse(ctx.Current.Value), ln, col);
                ctx.Next();
                return node;
            }

            if (ctx.Current.Type == TokenType.Identifier)
            {
                string identifier = ctx.Current.Value;

                if (ctx.Peek(1).Type == TokenType.LParen) return ParseFunctionCall(ctx, identifier);

                ctx.Next();
                return new ASTIdentifier(identifier, ln, col);
            }

            if (ctx.Current.Type == TokenType.Tilde)
            {
                ctx.Next();
                var expr = ParseExpression(ctx, UNARY_PRECEDENCE);
                return new ASTUnaryOp(expr, UnaryOpType.BitwiseNot, ln, col);
            }

            if(ctx.Current.Type == TokenType.Minus)
            {
                ctx.Next();
                var expr = ParseExpression(ctx, UNARY_PRECEDENCE);
                return new ASTUnaryOp(expr, UnaryOpType.Neg, ln, col);
            }

            if (ctx.Current.Type == TokenType.LParen)
            {
                //consume (
                ctx.Next();

                var expr = ParseExpression(ctx);

                if (!ctx.Expect(TokenType.RParen)) { ctx.Sync(TokenType.RParen, TokenType.Semicolon); return expr; }
                ctx.Next(); //consume )

                return expr;
            }

            ctx.Panic("Failed to parse expression");
            ctx.Sync(TokenType.Semicolon);
            return null;
        }

        internal static ASTNode ParseFunctionCall(ParserContext ctx, string fname)
        {
            ulong ln = ctx.Current.Ln, col = ctx.Current.Col;
            //consume identifier and (
            ctx.Next(); ctx.Next();

            //TODO: arguments parsing

            if (!ctx.Expect(TokenType.RParen)) { ctx.Sync(TokenType.RParen, TokenType.Semicolon); }

            return new ASTFunctionCall(fname, ln, col);
        }
    }
}
