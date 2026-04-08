using System.Collections.Generic;
using KoalaLang.Compiler.Parsers.ASTNodes;

using static KoalaLang.Compiler.Parsers.ParserExpression;

namespace KoalaLang.Compiler.Parsers
{
    internal static class ParserStatement
    {
        internal static void ParseStruct(ParserContext ctx)
        {
            ulong ln = ctx.Current.Ln, col = ctx.Current.Col;
            List<ASTVariableDecl> fields = new();
            string structName;

            //consume struct keyword
            ctx.Next();

            if(!ctx.Expect(TokenType.Identifier)) { ctx.Sync(TokenType.RBrace); return; }
            structName = ctx.Current.Value;
            ctx.Next();

            if(!ctx.Expect(TokenType.LBrace)) { ctx.Sync(TokenType.RBrace); return; }
            ctx.Next();

            while(ctx.NotEOF && ctx.Current.Type != TokenType.RBrace)
            {
                if(ctx.Current.Type == TokenType.Identifier)
                {
                    ASTNode fieldDecl = ParseVarDecl(ctx, skipConsumingKW: true);
                    if (fieldDecl is ASTAssignment)
                        ctx.Panic("Unsupported expression: cannot assign inside struct declaration");
                    else
                        fields.Add(fieldDecl as ASTVariableDecl);
                }
                else
                    ctx.Panic("Unsupported expression inside struct declaration");
            }

            if(!ctx.Expect(TokenType.RBrace)) { ctx.Sync(TokenType.FuncKW); return; }
            ctx.Next();

            ctx.PushNode(new ASTStructDecl(structName, fields, ln, col));
        }

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
                else if (ctx.Current.Type == TokenType.IfKW) nodes.Add(ParseBranch(ctx));
                else if (ctx.Current.Type == TokenType.WhileKW) nodes.Add(ParseWhileLoop(ctx));
                else if (ctx.Current.Type == TokenType.Identifier ||
                    (ctx.Current.Type == TokenType.Asterisk && ctx.Peek().Type == TokenType.Identifier))
                {
                    var expr = ParseExpression(ctx);
                    if (expr != null) nodes.Add(expr);
                    else UnknownStatementPanic();

                    ctx.SkipIfSemicolon();
                }
                else UnknownStatementPanic();
            }

            if(ctx.Current.Type == TokenType.RBrace) ctx.Next();

            return new ASTCodeBlock(nodes, ln, col);
        }

        internal static ASTNode ParseVarDecl(ParserContext ctx, bool skipConsumingKW = false)
        {
            ulong ln = ctx.Current.Ln, col = ctx.Current.Col;

            if(!skipConsumingKW) ctx.Next(); //consume let keyword

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

        internal static ASTBranch ParseBranch(ParserContext ctx)
        {
            ulong ln = ctx.Current.Ln, col = ctx.Current.Col;

            //consume if keyword
            ctx.Next();

            var ifCond = ParseExpression(ctx);

            if(!ctx.Expect(TokenType.LBrace)) { ctx.Sync(TokenType.RBrace); return null; }

            var ifBody = ParseCodeBlock(ctx);

            List<ASTIf> elseIfs = new();
            ASTCodeBlock elseBlock = null;

            while(ctx.NotEOF && ctx.Current.Type == TokenType.ElseKW)
            {
                ctx.Next();
                if(ctx.Current.Type == TokenType.IfKW)
                {
                    ulong elseIfLn = ctx.Current.Ln, elseIfCol = ctx.Current.Col;

                    //consume if keyword
                    ctx.Next();

                    var elseIfCond = ParseExpression(ctx);

                    if (!ctx.Expect(TokenType.LBrace)) { ctx.Sync(TokenType.RBrace); return null; }

                    var elseIfBody = ParseCodeBlock(ctx);

                    elseIfs.Add(new ASTIf(elseIfCond, elseIfBody, elseIfLn, elseIfCol));
                }
                else if(ctx.Current.Type == TokenType.LBrace)
                {
                    elseBlock = ParseCodeBlock(ctx);
                    break;
                }
            }

            return new ASTBranch(new ASTIf(ifCond, ifBody, ln, col), elseIfs, elseBlock, ln, col);
        }

        internal static ASTWhileLoop ParseWhileLoop(ParserContext ctx)
        {
            ulong ln = ctx.Current.Ln, col = ctx.Current.Col;

            //consume while keyword
            ctx.Next();

            var loopCond = ParseExpression(ctx);

            if (!ctx.Expect(TokenType.LBrace)) { ctx.Sync(TokenType.RBrace); return null; }

            var body = ParseCodeBlock(ctx);

            return new ASTWhileLoop(loopCond, body, ln, col);
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