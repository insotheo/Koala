namespace KoalaLang.Lexer
{
    public enum TokenType
    {
        EOF, Identifier, Number, FloatNumber, BooleanValue, StringLiteral, CharLiteral, Unknown, Keyword,

        Plus, Minus, Asterisk, Slash, AssignmentSign, Percent,

        LogicalAnd, LogicalOr, LogicalNot, Xor,
        BitwiseAnd, BitwiseOr, BitwiseNot, LeftShift, RightShift,
        Equal, Inequal, More, MoreOrEqual, Less, LessOrEqual,
        Dot,

        Colon, Semicolon, Comma, LParen, RParen, LBrace, RBrace, LBracket, RBracket
    }
}
