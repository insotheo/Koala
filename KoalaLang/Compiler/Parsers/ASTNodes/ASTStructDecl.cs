using System.Collections.Generic;

namespace KoalaLang.Compiler.Parsers.ASTNodes
{
    internal class ASTStructDecl : ASTNode
    {
        internal string StructName { get; set; }
        internal List<ASTVariableDecl> Fields { get; set; }
        internal List<ASTFunction> Methods { get; set; }

        internal ASTStructDecl(string name, List<ASTVariableDecl> fields, List<ASTFunction> methods, ulong ln, ulong col) : base(ln, col)
        {
            StructName = name;
            Fields = fields;
            Methods = methods;
        }
    }
}
