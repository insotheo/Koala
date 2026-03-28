namespace SkullLang.Compiler.Parsers
{
    internal enum TokenType
    {
        EOF, Unknown,
        Identifier, NumberI, NumberF,

        //K means keyword
        ReturnK,

        LParen, RParen, LBrace, RBrace,
    }

    internal struct Token
    {
        internal TokenType Type;
        internal string Value;
        internal ulong Ln, Col;

        internal Token(ulong ln, ulong col, TokenType type, string val)
        {
            Type = type;
            Value = val;
            Ln = ln;
            Col = col;
        }
    }
}
