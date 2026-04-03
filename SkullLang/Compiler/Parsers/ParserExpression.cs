using SkullLang.Compiler.Parsers.ASTNodes;
using System.Collections.Generic;

namespace SkullLang.Compiler.Parsers
{
    internal static class ParserExpression
    {
        const int UNARY_PRECEDENCE = 11;

        internal static ASTNode ParseExpression(ParserContext ctx, int precedence = 0, ASTNode lhs = null)
        {
            int GetOpPrecedence(TokenType type) => type switch
            {
                TokenType.Assignment => 1,

                TokenType.LogicalOr => 2,

                TokenType.LogicalAnd => 3,

                TokenType.GreaterThan => 4,
                TokenType.GreaterOrEqual => 4,
                TokenType.LessThan => 4,
                TokenType.LessOrEqual => 4,
                TokenType.Equal => 4,
                TokenType.Inequal => 4,

                TokenType.Pipe => 5,

                TokenType.Caret => 6,

                TokenType.Ampersand => 7,

                TokenType.LeftShift => 8,
                TokenType.RightShift => 8,

                TokenType.Plus => 9,
                TokenType.Minus => 9,

                TokenType.Asterisk => 10,
                TokenType.Slash => 10,
                TokenType.Percent => 10,

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
                TokenType.GreaterThan => BinaryOpType.GreaterThan,
                TokenType.GreaterOrEqual => BinaryOpType.GreaterOrEqual,
                TokenType.LessThan => BinaryOpType.LessThan,
                TokenType.LessOrEqual => BinaryOpType.LessOrEqual,
                TokenType.Equal => BinaryOpType.Equal,
                TokenType.Inequal => BinaryOpType.Inequal,
                TokenType.LogicalOr => BinaryOpType.LogicalOr,
                TokenType.LogicalAnd => BinaryOpType.LogicalAnd,

                _ => BinaryOpType.None,
            };

            var left = lhs == null ? ParsePrimary(ctx) : lhs;

            while (ctx.NotEOF)
            {
                var currentPrecedence = GetOpPrecedence(ctx.Current.Type);
                if (currentPrecedence <= precedence)
                    break;

                var opToken = ctx.Current;
                ctx.Next();

                if(opToken.Type == TokenType.Assignment)
                {
                    var rhsAssignment = ParseExpression(ctx, precedence - 1); //right associative 

                    left = new ASTAssignment(left, rhsAssignment, opToken.Ln, opToken.Col);
                    continue;
                }

                var right = ParseExpression(ctx, currentPrecedence);

                left = new ASTBinaryOp(left, right, MapBinaryOp(opToken.Type), opToken.Ln, opToken.Col);
            }

            return left;
        }

        internal static ASTNode ParsePrimary(ParserContext ctx)
        {
            UnaryOpType MapUnaryOp(TokenType type) => type switch
            {
                TokenType.Tilde => UnaryOpType.BitwiseNot,
                TokenType.Minus => UnaryOpType.Neg,
                TokenType.Ampersand => UnaryOpType.Reference,
                TokenType.Asterisk => UnaryOpType.DeferencingPtr,
                TokenType.Not => UnaryOpType.Not,

                _ => UnaryOpType.None,
            };

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

            if (ctx.Current.Type == TokenType.Tilde ||
                ctx.Current.Type == TokenType.Minus ||
                ctx.Current.Type == TokenType.Ampersand ||
                ctx.Current.Type == TokenType.Asterisk ||
                ctx.Current.Type == TokenType.Not)
            {
                UnaryOpType op = MapUnaryOp(ctx.Current.Type);
                ctx.Next();
                var expr = ParseExpression(ctx, UNARY_PRECEDENCE);
                return new ASTUnaryOp(expr, op, ln, col);
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

            List<ASTNode> args = new();
            while(ctx.NotEOF && ctx.Current.Type != TokenType.RParen)
            {
                ASTNode expr = ParseExpression(ctx);

                args.Add(expr);

                if (ctx.Current.Type == TokenType.Comma) ctx.Next();
            }

            if (!ctx.Expect(TokenType.RParen)) { ctx.Sync(TokenType.RParen, TokenType.Semicolon); }
            ctx.Next(); //consume )

            return new ASTFunctionCall(fname, args, ln, col);
        }

        internal static string ParseType(ParserContext ctx)
        {
            if(!ctx.Expect(TokenType.Identifier))
            {
                ctx.Sync(TokenType.Semicolon, TokenType.RParen, TokenType.LBrace);
                return null;
            }

            string typeName = ctx.Current.Value;
            ctx.Next();

            while (ctx.NotEOF &&
                (ctx.Current.Type == TokenType.Asterisk ||
                ctx.Current.Type == TokenType.Ampersand
                ))
            {
                if (ctx.Current.Type == TokenType.Asterisk) typeName += "*";
                if (ctx.Current.Type == TokenType.Ampersand) typeName += "&";

                ctx.Next();
            }

            return typeName;
        }
    }
}
