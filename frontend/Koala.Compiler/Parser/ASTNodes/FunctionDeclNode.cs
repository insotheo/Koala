namespace Koala.Compiler.Parser.ASTNodes;

internal class FunctionDeclNode : INode
{
    internal string FuncName { get; private set; }
    internal BodyNode Body { get; private set; }
    internal string ReturnType { get; private set; }

    public FunctionDeclNode(string name, BodyNode body, string returnType)
    {
        FuncName = name;
        Body = body;
        ReturnType = returnType;
    }

}