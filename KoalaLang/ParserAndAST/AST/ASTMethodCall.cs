using System.Collections.Generic;

namespace KoalaLang.ParserAndAST.AST
{
    public sealed class ASTMethodCall(ASTNode target, string methodName, List<ASTNode> args, List<string> genericTypes, int line) : ASTNode(line)
    {
        internal ASTNode Target = target;
        internal string MethodName = methodName;
        internal List<ASTNode> Args = args;
        internal List<string> GenericTypes = genericTypes;
    }
}
