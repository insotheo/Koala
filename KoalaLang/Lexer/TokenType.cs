namespace KoalaLang.Lexer
{
    public enum TokenType
    {
        EOF, Identifier, Number, FloatNumber, Unknown, Keyword,

        Plus, Minus, Asterisk, Slash, AssignmentSign,

        Colon, Semicolon, LParen, RParen, LBrace, RBrace
    }
}
