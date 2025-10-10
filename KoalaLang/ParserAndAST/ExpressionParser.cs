using KoalaLang.Lexer;
using KoalaLang.ParserAndAST.AST;
using System;
using System.Collections.Generic;

namespace KoalaLang.ParserAndAST
{
    internal sealed class ExpressionParser
    {
        private readonly ParserContext _ctx;

        internal ExpressionParser(ParserContext context)
        {
            _ctx = context;
        }
        internal ASTNode ParseExpression() => ParseLogicalOr();

        ASTNode ParseLogicalOr()
        {
            ASTNode left = ParseLogicalAnd();
            while (!_ctx.End && _ctx.Current.Type == TokenType.LogicalOr)
            {
                _ctx.Next();
                ASTNode right = ParseLogicalAnd();
                left = new ASTBinOperation(left, BinOperationType.LogicalOr, right, _ctx.Current.Line);
            }
            return left;
        }
        ASTNode ParseLogicalAnd()
        {
            ASTNode left = ParseBitwiseOr();
            while (!_ctx.End && _ctx.Current.Type == TokenType.LogicalAnd)
            {
                _ctx.Next();
                ASTNode right = ParseBitwiseOr();
                left = new ASTBinOperation(left, BinOperationType.LogicalAnd, right, _ctx.Current.Line);
            }
            return left;
        }
        ASTNode ParseBitwiseOr()
        {
            ASTNode left = ParseXor();
            while (!_ctx.End && _ctx.Current.Type == TokenType.BitwiseOr)
            {
                _ctx.Next();
                ASTNode right = ParseXor();
                left = new ASTBinOperation(left, BinOperationType.BitwiseOr, right, _ctx.Current.Line);
            }
            return left;
        }
        ASTNode ParseXor()
        {
            ASTNode left = ParseBitwiseAnd();
            while (!_ctx.End && _ctx.Current.Type == TokenType.Xor)
            {
                _ctx.Next();
                ASTNode right = ParseBitwiseAnd();
                left = new ASTBinOperation(left, BinOperationType.Xor, right, _ctx.Current.Line);
            }
            return left;
        }
        ASTNode ParseBitwiseAnd()
        {
            ASTNode left = ParseEquality();
            while (!_ctx.End && _ctx.Current.Type == TokenType.BitwiseAnd)
            {
                _ctx.Next();
                ASTNode right = ParseEquality();
                left = new ASTBinOperation(left, BinOperationType.BitwiseAnd, right, _ctx.Current.Line);
            }
            return left;
        }
        ASTNode ParseEquality()
        {
            ASTNode left = ParseRelational();
            while (!_ctx.End && (_ctx.Current.Type == TokenType.Equal || _ctx.Current.Type == TokenType.Inequal))
            {
                BinOperationType op = _ctx.Current.Type == TokenType.Equal ? BinOperationType.CmpEqual : BinOperationType.CmpInequal;
                _ctx.Next();
                ASTNode right = ParseRelational();
                left = new ASTBinOperation(left, op, right, _ctx.Current.Line);
            }
            return left;
        }
        ASTNode ParseRelational()
        {
            ASTNode left = ParseShift();
            while (!_ctx.End && (_ctx.Current.Type == TokenType.Less || _ctx.Current.Type == TokenType.LessOrEqual ||
                                            _ctx.Current.Type == TokenType.More || _ctx.Current.Type == TokenType.MoreOrEqual))
            {
                BinOperationType op = _ctx.Current.Type switch
                {
                    TokenType.Less => BinOperationType.CmpLess,
                    TokenType.LessOrEqual => BinOperationType.CmpLessOrEq,
                    TokenType.More => BinOperationType.CmpMore,
                    TokenType.MoreOrEqual => BinOperationType.CmpMoreOrEq,

                    _ => throw new Exception("Unknown operation in expression")
                };

                _ctx.Next();
                ASTNode right = ParseShift();
                left = new ASTBinOperation(left, op, right, _ctx.Current.Line);
            }
            return left;
        }
        ASTNode ParseShift()
        {
            ASTNode left = ParseArithmetic();
            while (!_ctx.End && (_ctx.Current.Type == TokenType.LeftShift || _ctx.Current.Type == TokenType.RightShift))
            {
                BinOperationType op = _ctx.Current.Type == TokenType.LeftShift ? BinOperationType.LeftShift : BinOperationType.RightShift;
                _ctx.Next();
                ASTNode right = ParseArithmetic();
                left = new ASTBinOperation(left, op, right, _ctx.Current.Line);
            }
            return left;
        }
        ASTNode ParseArithmetic()
        {
            ASTNode left = ParseTerm();
            while (!_ctx.End && (_ctx.Current.Type == TokenType.Plus || _ctx.Current.Type == TokenType.Minus))
            {
                BinOperationType op = _ctx.Current.Type == TokenType.Plus ? BinOperationType.Add : BinOperationType.Subtract;
                _ctx.Next();
                ASTNode right = ParseTerm();
                left = new ASTBinOperation(left, op, right, _ctx.Current.Line);
            }
            return left;
        }
        ASTNode ParseTerm()
        {
            ASTNode left = ParseFactor();
            while (!_ctx.End && (_ctx.Current.Type == TokenType.Asterisk || _ctx.Current.Type == TokenType.Slash || _ctx.Current.Type == TokenType.Percent))
            {
                BinOperationType op = _ctx.Current.Type switch
                {
                    TokenType.Asterisk => BinOperationType.Multiply,
                    TokenType.Slash => BinOperationType.Divide,
                    TokenType.Percent => BinOperationType.Remain,

                    _ => throw new Exception("Unknown operation in expression")
                };

                _ctx.Next();
                ASTNode right = ParseFactor();
                left = new ASTBinOperation(left, op, right, _ctx.Current.Line);
            }
            return left;
        }
        ASTNode ParseFactor()
        {
            if (_ctx.Current.Type == TokenType.Number)
            {
                ASTNode number = new ASTConstant<int>(int.Parse(_ctx.Current.Value), _ctx.Current.Line);
                _ctx.Next();
                return number;
            }
            else if (_ctx.Current.Type == TokenType.FloatNumber)
            {
                ASTNode number = new ASTConstant<float>(float.Parse(_ctx.Current.Value), _ctx.Current.Line);
                _ctx.Next();
                return number;
            }
            else if (_ctx.Current.Type == TokenType.BooleanValue)
            {
                ASTNode boolean = new ASTConstant<bool>(_ctx.Current.Value == "true" ? true : false, _ctx.Current.Line);
                _ctx.Next();
                return boolean;
            }

            else if (_ctx.Current.Type == TokenType.Identifier)
            {
                string identifier = _ctx.Current.Value;
                _ctx.Next();

                if (_ctx.Current.Type == TokenType.LParen || _ctx.Current.Type == TokenType.Less)
                {
                    return ParseFunctionCall(identifier);
                }
                else return new ASTVariableUse(identifier, _ctx.Current.Line);
            }

            else if (_ctx.Current.Type == TokenType.Minus)
            {
                _ctx.Next();
                ASTNode expr = ParseFactor();
                return new ASTUnOperation(UnaryOperationType.Negate, expr, _ctx.Current.Line);
            }
            else if (_ctx.Current.Type == TokenType.LogicalNot)
            {
                _ctx.Next();
                ASTNode expr = ParseFactor();
                return new ASTUnOperation(UnaryOperationType.LogicalNot, expr, _ctx.Current.Line);
            }
            else if (_ctx.Current.Type == TokenType.BitwiseNot)
            {
                _ctx.Next();
                ASTNode expr = ParseFactor();
                return new ASTUnOperation(UnaryOperationType.BitwiseNot, expr, _ctx.Current.Line);
            }

            else if (_ctx.Current.Type == TokenType.LParen)
            {
                int saveIdx = _ctx.Index;
                _ctx.Next();

                if (_ctx.Current.Type == TokenType.Identifier)
                {
                    string typeName = _ctx.Current.Value;
                    _ctx.Next();

                    if (_ctx.Current.Type == TokenType.RParen)
                    {
                        try
                        {
                            _ctx.Next();//skip )
                            ASTNode castExpr = ParseFactor();
                            return new ASTCast(typeName, castExpr, _ctx.Current.Line);
                        }
                        catch { _ctx.Index = saveIdx + 1; }
                    }
                    else
                    {
                        _ctx.Index = saveIdx + 1;
                    }
                }
                ASTNode expr = ParseExpression();
                _ctx.Expect(TokenType.RParen);
                _ctx.Next();
                return expr;
            }

            throw new Exception("Unexpected token in expression");
        }

        //Token before call is LParen or Less
        internal ASTFunctionCall ParseFunctionCall(string identifier)
        {
            List<string> genericTypes = new();
            if (_ctx.Current.Type == TokenType.Less)
            {
                _ctx.Next();
                while (!_ctx.End && _ctx.Current.Type != TokenType.More)
                {
                    if (_ctx.Current.Type != TokenType.Identifier) break;
                    genericTypes.Add(_ctx.Current.Value);
                    _ctx.Next();
                    if (_ctx.Current.Type != TokenType.Comma) break;
                    _ctx.Next();
                }
                _ctx.Expect(TokenType.More);
                _ctx.ExpectNext(TokenType.LParen);
            }

            _ctx.Next(); //skip LParen
            List<ASTNode> args = new();
            while (!_ctx.End && _ctx.Current.Type != TokenType.RParen)
            {
                ASTNode expression = ParseExpression();
                args.Add(expression);
                if (_ctx.Current.Type == TokenType.Comma)
                {
                    _ctx.Next();
                    continue;
                }
                else if (_ctx.Current.Type == TokenType.RParen) break;
                else _ctx.Expect(TokenType.Comma);
            }
            _ctx.Expect(TokenType.RParen);
            _ctx.Next();

            return new ASTFunctionCall(identifier, args, genericTypes, _ctx.Current.Line);
        }
    }
}