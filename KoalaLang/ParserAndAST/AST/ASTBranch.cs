using System.Collections.Generic;

namespace KoalaLang.ParserAndAST.AST
{
    public sealed class ASTBranch(List<ASTConditionBlock> ifs, ASTCodeBlock @else, int line) : ASTNode(line)
    {
        internal ASTConditionBlock[] Ifs = ifs.ToArray();
        internal ASTCodeBlock Else = @else;
    }
}
