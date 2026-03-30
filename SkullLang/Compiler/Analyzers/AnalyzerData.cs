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
            TypeName = typeName;
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
            "i32" => TypeKind.Integer,

            "f32" => TypeKind.Float,
            "f64" => TypeKind.Float,

            "void" => TypeKind.None,

            //TODO: explicit type recognizion
            _ => TypeKind.None,
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
