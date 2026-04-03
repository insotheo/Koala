namespace SkullLang.Compiler.Parsers
{
    internal enum TokenType
    {
        EOF, Unknown,
        Identifier, NumberI, NumberF,

        //KW means keyword
        ReturnKW, FuncKW, LetKW, IfKW, ElseKW,

        Plus, Minus, Asterisk, Slash, Percent,
        GreaterThan, GreaterOrEqual, LessThan, LessOrEqual, Equal, Inequal, Not, LogicalOr, LogicalAnd,
        Ampersand, Pipe, Caret, Tilde, LeftShift, RightShift,

        LParen, RParen, LBrace, RBrace, Semicolon, Colon, Comma, Assignment,
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
