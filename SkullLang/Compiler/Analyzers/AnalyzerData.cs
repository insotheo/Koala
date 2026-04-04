using System;
using System.Collections.Generic;
using System.Text;

namespace SkullLang.Compiler.Analyzers
{
    internal enum TypeKind
    {
        None, 
        Integer, Float, String,
        Pointer, Reference
    }

    internal struct TypeInfo
    {
        internal string OriginalTypeName;
        internal string TypeName;
        internal bool IsLiteral;
        internal bool IsRefInPast;
        internal TypeKind Kind;

        internal TypeInfo(string typeName, TypeKind kind, bool isLiteral = false, bool refInPast = false)
        {
            OriginalTypeName = typeName;
            TypeName = GetCTypeName(typeName);
            Kind = kind;
            IsLiteral = isLiteral;
            IsRefInPast = refInPast;
        }

        internal bool CmpKinds(TypeInfo other) => Kind == other.Kind;
        internal bool CmpStrict(TypeInfo other)
        {
            bool kinds = CmpKinds(other);
            if (TypeName == null || other.TypeName == null || this.IsLiteral || other.IsLiteral) return kinds;
            return TypeName == other.TypeName && kinds;
        }

        internal static TypeKind GetKind(string typeName, Context ctx = null)
        {
            if (typeName.EndsWith("*")) return TypeKind.Pointer;
            if (typeName.EndsWith("&")) return TypeKind.Reference;
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
            "llong" => TypeKind.Integer,
            "ullong" => TypeKind.Integer,

            "float" => TypeKind.Float,
            "double" => TypeKind.Float,

            "bool" => TypeKind.Integer,

            "void" => TypeKind.None,

            _ => TypeKind.None
        };


        internal static string GetBaseTypeName(string typeName)
        {
            string baseName = typeName?.TrimEnd('*', '&');
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
                "llong" => "long long int",
                "ullong" => "unsigned long long int",

                "float" => "float",
                "double" => "double",

                "bool" => "int",

                "void" => "void",

                _ => baseType
            };

            string suffix = typeName.Substring(baseType.Length);
            suffix = suffix.Replace('&', '*');

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
        private static ulong _id = 0;

        internal string FuncName;
        internal string FuncUName;
        internal TypeInfo ReturnType;
        internal List<VariableInfo> Args;
        internal bool IsExtern;

        internal FunctionInfo(string funcName, string returnTypeName, List<VariableInfo> args, bool isExtern = false)
        {
            FuncName = funcName;
            ReturnType = new TypeInfo(returnTypeName, TypeInfo.GetKind(returnTypeName));
            Args = args;
            IsExtern = isExtern;
            GenUName();
        }

        private void GenUName()
        {
            if(FuncName == "main" || IsExtern)
            {
                FuncUName = FuncName;
                return;
            }
            _id += 1;

            StringBuilder uname = new();
            uname.Append("_f");
            uname.Append(_id);
            uname.Append(FuncName);
            uname.Append("_");

            foreach(var arg in Args)
            {
                uname.Append(arg.Type.TypeName[0].ToString());
            }

            FuncUName = uname.ToString();
        }
    }
}
