using System;
using System.Collections.Generic;

namespace SkullLang.Compiler.Analyzers
{
    internal enum TypeKind
    {
        None, 
        Integer, Float,
        Pointer
    }

    internal struct TypeInfo
    {
        internal string OriginalTypeName;
        internal string TypeName;
        internal bool IsLiteral;
        internal TypeKind Kind;

        internal TypeInfo(string typeName, TypeKind kind, bool isLiteral = false)
        {
            OriginalTypeName = typeName;
            TypeName = GetCTypeName(typeName);
            Kind = kind;
            IsLiteral = isLiteral;
        }

        internal bool CmpKinds(TypeInfo other) => Kind == other.Kind;
        internal bool CmpStrict(TypeInfo other)
        {
            bool kinds = CmpKinds(other);
            if (TypeName == null || other.TypeName == null) return kinds;
            return TypeName == other.TypeName && kinds;
        }

        internal static TypeKind GetKind(string typeName, Context ctx = null)
        {
            if (typeName.EndsWith("*")) return TypeKind.Pointer;
            return GetKindBasedOnTypeName(typeName, ctx);
        }

        internal static TypeKind GetKindBasedOnTypeName(string typeName, Context ctc = null) => typeName switch
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


        internal static string GetBaseTypeName(string typeName)
        {
            string baseName = typeName?.TrimEnd('*');
            return baseName;
        }


        internal static string GetCTypeName(string typeName)
        {
            if (typeName == null) return null;

            string baseType = GetBaseTypeName(typeName);

            string cBase = baseType switch
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

                _ => baseType
            };

            string suffix = typeName.Substring(baseType.Length);

            return cBase + suffix;
        }

        internal string ToStringOriginal() => String.IsNullOrEmpty(OriginalTypeName) ? Kind.ToString().ToLower() : OriginalTypeName;
    }

    internal struct VariableInfo
    {
        internal string Name;
        internal TypeInfo Type;
        internal bool IsConst = false;

        internal VariableInfo(string name, TypeInfo type)
        {
            Name = name;
            Type = type;
        }
    }

    internal struct FunctionInfo
    {
        internal string FuncName;
        internal string ReturnType;
        internal List<VariableInfo> Args;

        internal FunctionInfo(string funcName, string returnType, List<VariableInfo> args)
        {
            FuncName = funcName;
            ReturnType = returnType;
            Args = args;
        }
    }
}
