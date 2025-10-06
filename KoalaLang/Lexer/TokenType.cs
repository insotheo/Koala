namespace KoalaLang.Lexer
{
    public enum TokenType
    {
        EOF, Identifier, Number, FloatNumber, BooleanValue, Unknown, Keyword,

        Plus, Minus, Asterisk, Slash, AssignmentSign, Percent,

        LogicalAnd, LogicalOr, LogicalNot, Xor,
        BitwiseAnd, BitwiseOr, BitwiseNot, LeftShift, RightShift,

        Colon, Semicolon, Comma, LParen, RParen, LBrace, RBrace
    }
}
