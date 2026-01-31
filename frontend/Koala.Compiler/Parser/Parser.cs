using System.Collections.Generic;
using Koala.Compiler.Lexer;
using Koala.Compiler.Parser.ASTNodes;

namespace Koala.Compiler.Parser;

public class Parser
{
    private readonly ParserContext _ctx;
    public Parser(string filePath, Lexer.Lexer lexer)
    {
        _ctx = new ParserContext(filePath, lexer);
    }

    public int GetErrorsCount() => _ctx.GetErrorsCount();
    public void PrintErrors() => _ctx.PrintErrors();


    public void Parse()
    {
        while (_ctx.CurrentType != TokenType.EOF)
        {
            switch (_ctx.CurrentType)
            {
                case TokenType.Func: _ctx.Push(ParseFuncDecl()); break;

                default: _ctx.Panic($"Unexpected command: {_ctx.CurrentType}"); break;
            }
        }
    }

    INode ParseInstruction()
    {
        INode output = null;

        switch (_ctx.CurrentType)
        {
            case TokenType.Return:
                {
                    _ctx.Next();
                    //TODO: make expressions parsing
                    output = new ReturnNode(new ConstantNode(0)); //tmp
                }
                break;

            default:
                _ctx.Panic($"Unexpected token: {_ctx.CurrentType}");
                _ctx.SkipUntil(TokenType.Semicolon, TokenType.RBrace, TokenType.EOF);
                break;
        }

        if (_ctx.CurrentType == TokenType.Semicolon) _ctx.Next();
        return output;
    }

    BodyNode ParseBody()
    {
        if (_ctx.CurrentType != TokenType.LBrace)
        {
            _ctx.TryCurrent(TokenType.LBrace);
            _ctx.SkipUntil(TokenType.RBrace, TokenType.EOF);
            return null;
        }
        _ctx.Next(); //consume {

        List<INode> nodes = new();
        while (_ctx.CurrentType != TokenType.RBrace && _ctx.CurrentType != TokenType.EOF)
        {
            nodes.Add(ParseInstruction());
        }

        if (_ctx.CurrentType == TokenType.RBrace) _ctx.Next(); //consume }

        return new BodyNode(nodes);
    }

    FunctionDeclNode ParseFuncDecl()
    {
        FunctionDeclNode FunctionDeclSkip()
        {
            _ctx.SkipUntil(TokenType.LBrace, TokenType.Func, TokenType.EOF);
            return null;
        }

        if (!_ctx.TryNext(TokenType.Identifier)) return FunctionDeclSkip();
        string funcName = _ctx.Current.Val;

        if (!_ctx.TryNext(TokenType.LParen)) return FunctionDeclSkip();
        //TODO: args
        if (!_ctx.TryNext(TokenType.RParen)) return FunctionDeclSkip();

        //return type
        string returnType;
        if (_ctx.IsNext(TokenType.Colon))
        {
            if (!_ctx.TryNext(TokenType.Identifier)) return FunctionDeclSkip();
            returnType = _ctx.Current.Val;
        }
        else
        {
            _ctx.Back();
            returnType = "void";
        }

        if (!_ctx.TryNext(TokenType.LBrace)) return FunctionDeclSkip();
        BodyNode body = ParseBody();

        return new FunctionDeclNode(funcName, body, returnType);
    }

}