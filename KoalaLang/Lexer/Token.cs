namespace KoalaLang.Lexer
{
    public struct Token(TokenType type, string val, int ln, int col)
    {
        public string Value = val;
        public TokenType Type = type;
        public int Line = ln;
        public int Column = col;
    }
}
