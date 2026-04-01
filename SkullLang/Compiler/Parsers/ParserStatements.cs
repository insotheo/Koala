using System.Collections.Generic;
using SkullLang.Compiler.Parsers.ASTNodes;

using static SkullLang.Compiler.Parsers.ParserExpression;

namespace SkullLang.Compiler.Parsers
{
    internal static class ParserStatement
    {
        internal static void ParseFunction(ParserContext ctx)
        {
            ulong ln = ctx.Current.Ln, col = ctx.Current.Col;
            //consume func
            ctx.Next();

            if (!ctx.Expect(TokenType.Identifier)) { ctx.Sync(TokenType.LBrace, TokenType.Semicolon); return; }

            string fname = ctx.Current.Value;
            ctx.Next();

            if(!ctx.Expect(TokenType.LParen)) { ctx.Sync(TokenType.LBrace, TokenType.Semicolon); return; }
            ctx.Next();

            List<(string typeName, string argName)> args = new();
            while (ctx.NotEOF && ctx.Current.Type != TokenType.RParen)
            {
                if (!ctx.Expect(TokenType.Identifier)) { ctx.Sync(TokenType.RParen); }
                string argName = ctx.Current.Value;
                ctx.Next();

                if (!ctx.Expect(TokenType.Colon)) { ctx.Sync(TokenType.RParen); }
                ctx.Next();

                if (!ctx.Expect(TokenType.Identifier)) { ctx.Sync(TokenType.RParen); }
                string typeName = ctx.Current.Value;
                ctx.Next();

                args.Add((typeName, argName));

                if (ctx.Current.Type == TokenType.Comma) ctx.Next();
            }

            if (!ctx.Expect(TokenType.RParen)) { ctx.Sync(TokenType.LBrace, TokenType.Semicolon); return; }
            ctx.Next();

            string returnType = "void";
            if(ctx.Current.Type == TokenType.Colon)
            {
                ctx.Next(); //consume colon

                if(!ctx.Expect(TokenType.Identifier)) { ctx.Sync(TokenType.LBrace, TokenType.Semicolon); return; }
                returnType = ctx.Current.Value; //TODO: ADD NORMAL TYPE PARSER
                
                ctx.Next();
            }

            if(!ctx.Expect(TokenType.LBrace)) { ctx.Sync(TokenType.RBrace, TokenType.Semicolon); return; }
            var body = ParseCodeBlock(ctx);

            ctx.PushNode(new ASTFunction(fname, returnType, args, body, ln, col));
        }

        internal static ASTCodeBlock ParseCodeBlock(ParserContext ctx)
        {
            ulong ln = ctx.Current.Ln, col = ctx.Current.Col;

            //consume {
            ctx.Next();

            List<ASTNode> nodes = new();
            while(ctx.NotEOF && ctx.Current.Type != TokenType.RBrace)
            {
                if(ctx.Current.Type == TokenType.ReturnKW) nodes.Add(ParseReturn(ctx));
                else
                {
                    ctx.Panic($"Unknown statement: {ctx.GetLine(ctx.Current.Ln)}!");
                    ctx.Sync(TokenType.Semicolon, TokenType.RBrace);
                }
            }

            if(ctx.Current.Type == TokenType.RBrace) ctx.Next();

            return new ASTCodeBlock(nodes, ln, col);
        }

        internal static ASTReturn ParseReturn(ParserContext ctx)
        {
            ulong ln = ctx.Current.Ln, col = ctx.Current.Col;
            //consume return keyword
            ctx.Next();

            var expr = ParseExpression(ctx);

            ctx.SkipIfSemicolon();

            return new ASTReturn(expr, ln, col);
        }
    }
}