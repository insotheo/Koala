using System.Collections.Generic;

namespace KoalaLang.Compiler.Parsers.ASTNodes
{
    internal class ASTIf : ASTNode
    {
        internal ASTNode Cond { get; set; }
        internal ASTCodeBlock Body { get; set; }

        internal ASTIf(ASTNode cond, ASTCodeBlock body, ulong ln, ulong col) : base(ln, col)
        {
            Cond = cond;
            Body = body;
        }
    }

    internal class ASTBranch : ASTNode
    {
        internal ASTIf If { get; set; }
        internal List<ASTIf> ElseIfs { get; set; }
        internal ASTCodeBlock Else { get; set; }

        internal ASTBranch(ASTIf @if, List<ASTIf> elseIfs, ASTCodeBlock @else, ulong ln, ulong col) : base(ln, col)
        {
            If = @if;
            ElseIfs = elseIfs;
            Else = @else;
        }
    }
}
