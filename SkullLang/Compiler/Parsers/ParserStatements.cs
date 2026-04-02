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

                string typeName = ParseType(ctx);

                args.Add((typeName, argName));

                if (ctx.Current.Type == TokenType.Comma) ctx.Next();
            }

            if (!ctx.Expect(TokenType.RParen)) { ctx.Sync(TokenType.LBrace, TokenType.Semicolon); return; }
            ctx.Next();

            string returnType = "void";
            if(ctx.Current.Type == TokenType.Colon)
            {
                ctx.Next(); //consume colon
                returnType = ParseType(ctx);
            }

            if(!ctx.Expect(TokenType.LBrace)) { ctx.Sync(TokenType.RBrace, TokenType.Semicolon); return; }
            var body = ParseCodeBlock(ctx);

            ctx.PushNode(new ASTFunction(fname, returnType, args, body, ln, col));
        }

        internal static ASTCodeBlock ParseCodeBlock(ParserContext ctx)
        {
            void UnknownStatementPanic()
            {
                ctx.Panic($"Unknown statement: {ctx.GetLine(ctx.Current.Ln)}!");
                ctx.Sync(TokenType.Semicolon, TokenType.RBrace);
            }

            ulong ln = ctx.Current.Ln, col = ctx.Current.Col;

            //consume {
            ctx.Next();

            List<ASTNode> nodes = new();
            while(ctx.NotEOF && ctx.Current.Type != TokenType.RBrace)
            {
                if (ctx.Current.Type == TokenType.ReturnKW) nodes.Add(ParseReturn(ctx));
                else if (ctx.Current.Type == TokenType.LetKW) nodes.Add(ParseVarDecl(ctx));
                else if (ctx.Current.Type == TokenType.Identifier ||
                    (ctx.Current.Type == TokenType.Asterisk && ctx.Peek().Type == TokenType.Identifier))
                {
                    var expr = ParseExpression(ctx);
                    if (expr != null) nodes.Add(expr);
                    else UnknownStatementPanic();
                }
                else UnknownStatementPanic();
            }

            if(ctx.Current.Type == TokenType.RBrace) ctx.Next();

            return new ASTCodeBlock(nodes, ln, col);
        }

        internal static ASTNode ParseVarDecl(ParserContext ctx)
        {
            ulong ln = ctx.Current.Ln, col = ctx.Current.Col;

            ctx.Next(); //consume let keyword

            if (!ctx.Expect(TokenType.Identifier)) ctx.Sync(TokenType.Semicolon);
            string varName = ctx.Current.Value;
            ctx.Next();

            if (!ctx.Expect(TokenType.Colon)) ctx.Sync(TokenType.Semicolon);
            ctx.Next();

            string typeName = ParseType(ctx);

            ASTNode varDecl = new ASTVariableDecl(typeName, varName, ln, col);

            if(ctx.Current.Type == TokenType.Assignment) varDecl = ParseExpression(ctx, lhs: varDecl);

            ctx.SkipIfSemicolon();

            return varDecl;
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