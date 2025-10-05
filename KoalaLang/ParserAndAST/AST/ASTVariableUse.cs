namespace KoalaLang.ParserAndAST.AST
{
    public sealed class ASTVariableUse(string name) : ASTNode
    {
        internal string VariableName = name;
    }
}
