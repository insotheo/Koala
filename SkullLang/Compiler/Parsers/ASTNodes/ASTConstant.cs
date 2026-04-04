namespace SkullLang.Compiler.Parsers.ASTNodes
{
    internal class ASTConstantInt : ASTNode
    {
        internal ulong Value { get; private set; }

        internal ASTConstantInt(ulong val, ulong ln, ulong col) : base(ln, col) => Value = val;
    }

    internal class ASTConstantFloat : ASTNode
    {
        internal double Value { get; private set; }

        internal ASTConstantFloat(double val, ulong ln, ulong col) : base(ln, col) => Value = val;
    }

    internal class ASTConstantString : ASTNode
    {
        internal string Value { get; private set; }

        internal ASTConstantString(string val, ulong ln, ulong col) : base(ln, col) => Value = val;
    }

    internal class ASTIdentifier : ASTNode
    {
        internal string Identifier { get; private set; }

        internal bool WasDefered { get; set; } = false;

        internal ASTIdentifier(string identifier, ulong ln, ulong col) : base(ln, col) => Identifier = identifier;
    }
}