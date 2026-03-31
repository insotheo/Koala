namespace SkullLang.Compiler.Analyzers
{
    internal enum TypeKind
    {
        None, 
        Integer, Float
    }

    internal struct TypeInfo
    {
        internal string TypeName;
        internal TypeKind Kind;

        internal TypeInfo(string typeName, TypeKind kind)
        {
            TypeName = GetBaseCTypeName(typeName);
            Kind = kind;
        }

        internal bool CmpKinds(TypeInfo other) => Kind == other.Kind;
        internal bool CmpStrict(TypeInfo other)
        {
            bool kinds = CmpKinds(other);
            if (TypeName == null || other.TypeName == null) return kinds;
            return TypeName == other.TypeName && kinds;
        }

        internal static TypeKind GetKindBasedOnTypeName(string typeName, Context ctx = null) => typeName switch
        {
            "byte" => TypeKind.Integer,
            "ubyte" => TypeKind.Integer,
            "short" => TypeKind.Integer,
            "ushort" => TypeKind.Integer,
            "int" => TypeKind.Integer,
            "uint" => TypeKind.Integer,
            "long" => TypeKind.Integer,
            "ulong" => TypeKind.Integer,

            "float" => TypeKind.Float,
            "double" => TypeKind.Float,

            "bool" => TypeKind.Integer,

            "void" => TypeKind.None,

            _ => TypeKind.None
        };

        internal static string GetBaseCTypeName(string typeName) => typeName switch
        {
            "byte" => "char",
            "ubyte" => "unsigned char",
            "short" => "short int",
            "ushort" => "unsigned short int",
            "int" => "int",
            "uint" => "unsigned int",
            "long" => "long int",
            "ulong" => "unsigned long int",

            "float" => "float",
            "double" => "double",

            "bool" => "bool",

            "void" => "void",

            _ => typeName
        };
    }

    internal struct FunctionInfo
    {
        internal string FuncName;
        internal string ReturnType;
        internal string[] Args;

        internal FunctionInfo(string funcName, string returnType, string[] args)
        {
            FuncName = funcName;
            ReturnType = returnType;
            Args = args;
        }
    }
}
