namespace KoalaLang.ParserAndAST.AST
{
    public sealed class ASTVariableUse(string name, int line) : ASTNode(line)
    {
        internal string VariableName = name;
    }
}
