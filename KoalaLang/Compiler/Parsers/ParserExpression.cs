using KoalaLang.Compiler.Parsers.ASTNodes;
using System.Collections.Generic;
using System.Text;

namespace KoalaLang.Compiler.Parsers
{
    internal static class ParserExpression
    {
        const int UNARY_PRECEDENCE = 12;

        internal static ASTNode ParseExpression(ParserContext ctx, int precedence = 0, ASTNode lhs = null)
        {
            int GetOpPrecedence(TokenType type) => type switch
            {
                TokenType.Assignment => 1,

                TokenType.AsKW => 2,

                TokenType.LogicalOr => 3,

                TokenType.LogicalAnd => 4,

                TokenType.GreaterThan => 5,
                TokenType.GreaterOrEqual => 5,
                TokenType.LessThan => 5,
                TokenType.LessOrEqual => 5,
                TokenType.Equal => 5,
                TokenType.Inequal => 5,

                TokenType.Pipe => 6,

                TokenType.Caret => 7,

                TokenType.Ampersand => 8,

                TokenType.LeftShift => 9,
                TokenType.RightShift => 9,

                TokenType.Plus => 10,
                TokenType.Minus => 10,

                TokenType.Asterisk => 11,
                TokenType.Slash => 11,
                TokenType.Percent => 11,

                _ => -1,
            };

            BinaryOpType MapBinaryOp(TokenType type) => type switch
            {
                TokenType.Plus => BinaryOpType.Add,
                TokenType.Minus => BinaryOpType.Sub,
                TokenType.Asterisk => BinaryOpType.Mul,
                TokenType.Slash => BinaryOpType.Div,
                TokenType.Percent => BinaryOpType.Mod,
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
                else if(opToken.Type == TokenType.AsKW)
                {
                    string typeName = ParseType(ctx);

                    left = new ASTCast(left, typeName, opToken.Ln, opToken.Col);
                    continue;
                }

                var right = ParseExpression(ctx, currentPrecedence);

                left = new ASTBinaryOp(left, right, MapBinaryOp(opToken.Type), opToken.Ln, opToken.Col);
            }

            return left;
        }

        internal static ASTNode ParsePrimary(ParserContext ctx, bool ignorePostfix = false)
        {
            UnaryOpType MapUnaryOp(TokenType type) => type switch
            {
                TokenType.Tilde => UnaryOpType.BitwiseNot,
                TokenType.Minus => UnaryOpType.Neg,
                TokenType.Ampersand => UnaryOpType.Reference,
                TokenType.Asterisk => UnaryOpType.DereferencingPtr,
                TokenType.Not => UnaryOpType.Not,
                TokenType.SizeOfKW => UnaryOpType.SizeOf,

                _ => UnaryOpType.None,
            };

            ulong ln = ctx.Current.Ln, col = ctx.Current.Col;
            ASTNode node;

            if (ctx.Current.Type == TokenType.NumberI)
            {
                node = new ASTConstantInt(ulong.Parse(ctx.Current.Value), ln, col);
                ctx.Next();
            }
            else if (ctx.Current.Type == TokenType.NumberF)
            {
                node = new ASTConstantFloat(double.Parse(ctx.Current.Value), ln, col);
                ctx.Next();
            }
            else if (ctx.Current.Type == TokenType.True || ctx.Current.Type == TokenType.False)
            {
                node = new ASTConstantBoolean(ctx.Current.Type == TokenType.True, ln, col);
                ctx.Next();
            }
            else if (ctx.Current.Type == TokenType.CharLiteral)
            {
                if (ctx.Current.Value.Length != 1 && ctx.Current.Value[0] != '\\')
                    ctx.Panic("Character can only be one character length");
                node = new ASTConstantChar(ctx.Current.Value, ln, col);
                ctx.Next();
            }
            else if (ctx.Current.Type == TokenType.StringLiteral)
            {
                node = new ASTConstantString(ctx.Current.Value, ln, col);
                ctx.Next();
            }

            else if (ctx.Current.Type == TokenType.Identifier)
            {
                node = new ASTIdentifier(ctx.Current.Value, ln, col);
                ctx.Next(); //identifier

                if(ctx.Current.Type == TokenType.LParen)
                    node = ParseFunctionCall(ctx, (node as ASTIdentifier).Identifier);
            }

            else if (ctx.Current.Type == TokenType.Tilde ||
                ctx.Current.Type == TokenType.Minus ||
                ctx.Current.Type == TokenType.Ampersand ||
                ctx.Current.Type == TokenType.Asterisk ||
                ctx.Current.Type == TokenType.Not ||
                ctx.Current.Type == TokenType.SizeOfKW)
            {
                UnaryOpType op = MapUnaryOp(ctx.Current.Type);
                ctx.Next();
                var expr = ParseExpression(ctx, UNARY_PRECEDENCE);
                node = new ASTUnaryOp(expr, op, ln, col);
            }

            else if (ctx.Current.Type == TokenType.LParen)
            {
                ctx.Next(); //consume (

                node = ParseExpression(ctx);

                if (!ctx.Expect(TokenType.RParen)) { ctx.Sync(TokenType.RParen, TokenType.Semicolon); return node; }
                ctx.Next(); //consume )
            }

            else
            {
                ctx.Panic("Failed to parse expression");
                ctx.Sync(TokenType.Semicolon);
                return null;
            }

            //postfix parsing
            while (ctx.NotEOF && !ignorePostfix)
            {
                if (ctx.Current.Type == TokenType.Dot)
                {
                    ctx.Next(); //.

                    if (!ctx.Expect(TokenType.Identifier)) { ctx.Sync(TokenType.Semicolon); return node; }
                    var right = ParsePrimary(ctx, ignorePostfix: true);

                    node = new ASTDotAccess(node, right, ln, col);
                }

                else if (ctx.Current.Type == TokenType.LBracket)
                {
                    ulong ln2 = ctx.Current.Ln, col2 = ctx.Current.Col;
                    ctx.Next(); //[

                    var idxExpr = ParseExpression(ctx);

                    if (!ctx.Expect(TokenType.RBracket)) { ctx.Sync(TokenType.RBracket, TokenType.Semicolon); return node; }
                    ctx.Next();

                    node = new ASTIndexing(node, idxExpr, ln2, col2);
                }

                else break;
            }

            return node;
        }

        internal static ASTFunctionCall ParseFunctionCall(ParserContext ctx, string fName)
        {
            ulong ln = ctx.Current.Ln, col = ctx.Current.Col;

            ctx.Next(); //(

            List<ASTNode> args = new();
            while (ctx.NotEOF && ctx.Current.Type != TokenType.RParen)
            {
                args.Add(ParseExpression(ctx));
                if (ctx.Current.Type == TokenType.Comma)
                    ctx.Next();
            }

            if (!ctx.Expect(TokenType.RParen)) { ctx.Sync(TokenType.RParen, TokenType.Semicolon); return null; }
            ctx.Next();//)

            return new ASTFunctionCall(fName, args, ln, col);
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
                ctx.Current.Type == TokenType.Ampersand ||
                ctx.Current.Type == TokenType.ReadonlyKW
                ))
            {
                if (ctx.Current.Type == TokenType.Asterisk) typeName += "*";
                if (ctx.Current.Type == TokenType.Ampersand) typeName += "&";

                if(ctx.Current.Type == TokenType.ReadonlyKW)
                {
                    typeName += " __readonly";
                    ctx.Next();
                    break;
                }

                ctx.Next();
            }

            return typeName;
        }
    }
}
