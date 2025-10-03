namespace KoalaLang.Lexer
{
    public enum TokenType
    {
        EOF, Identifier, Number, FloatNumber, Unknown, Keyword,

        Plus, Minus, Asterisk, Slash,

        Colon, Semicolon, LParen, RParen, LBrace, RBrace
    }
}
