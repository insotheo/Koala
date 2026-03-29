namespace SkullLang.Compiler.Parsers.ASTNodes
{
    internal class ASTFunction : ASTNode
    {
        internal string FuncName { get; private set; }
        internal string RetType { get; private set; }
        internal ASTCodeBlock Body { get; private set; }

        internal ASTFunction(string functionName, string @return, ASTCodeBlock body, ulong ln, ulong col) : base(ln, col)
        {
            FuncName = functionName;
            RetType = @return;
            Body = body;
        }
    }
}