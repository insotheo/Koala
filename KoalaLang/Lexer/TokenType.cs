namespace KoalaLang.Lexer
{
    public enum TokenType
    {
        EOF, Identifier, Number, FloatNumber, Unknown, Keyword,

        Plus, Minus, Asterisk, Slash, AssignmentSign, Percent,

        Colon, Semicolon, Comma, LParen, RParen, LBrace, RBrace
    }
}
