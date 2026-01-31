namespace Koala.Compiler.Lexer;

public enum TokenType
{
    EOF, Unknown, Identifier, Int, Float,

    LParen, RParen, LBrace, RBrace, Semicolon,

    //Keywords
    Func, Return
}

public struct Token
{
    public TokenType Type;
    public string Val;
    public int Line, Col;

    public Token(TokenType tt, string val, int line, int col)
    {
        Type = tt;
        Val = val;
        Line = line;
        Col = col;
    }
}