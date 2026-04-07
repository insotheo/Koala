using SkullLang.Compiler.Parsers.ASTNodes;
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

    internal enum BuiltInType
    {
        None,
        Byte, UByte,
        Short, UShort,
        Int, UInt,
        Long, ULong,
        Float, Double,
        Bool,
        Void
    }

    internal enum TypeModifier
    {
        Pointer, Reference
    }

    internal struct TypeInfo
    {
        internal BuiltInType BuiltIn;
        internal string CustomTypeName; //later for structs

        internal List<TypeModifier> Modifiers;

        internal bool IsReadonly;
        internal bool IsLiteral;

        internal TypeKind Kind;

        internal string OriginalTypeName;

        internal bool IsPointer => Modifiers.Count > 0 && Modifiers[0] == TypeModifier.Pointer;
        internal bool IsReference => Modifiers.Count > 0 && Modifiers[0] == TypeModifier.Reference;

        internal TypeInfo(string typeName, bool isLiteral = false, Context ctx = null, ASTNode node = null)
        {
            BuiltIn = BuiltInType.None;
            CustomTypeName = null;
            Modifiers = new();

            OriginalTypeName = typeName != null ? typeName.Replace(" __readonly", "").Trim() : null;

            IsReadonly = false;
            IsLiteral = isLiteral;

            Kind = TypeKind.None;

            if (typeName == null) return;

            if(!String.IsNullOrEmpty(typeName)) Parse(ctx, node, typeName, ref this);
        }

        private static void Parse(Context ctx, ASTNode node, string typeName, ref TypeInfo info)
        {
            typeName = typeName.Trim();

            //readonly
            if (typeName.EndsWith(" __readonly"))
            {
                info.IsReadonly = true;
                typeName = typeName.Replace(" __readonly", "").Trim();
            }

            //ptr and ref
            while (typeName.EndsWith('*') || typeName.EndsWith('&'))
            {
                if (typeName.EndsWith('*'))
                    info.Modifiers.Add(TypeModifier.Pointer);
                else if (typeName.EndsWith('&'))
                    info.Modifiers.Add(TypeModifier.Reference);

                typeName = typeName.Substring(0, typeName.Length - 1).Trim();
            }

            info.BuiltIn = typeName switch
            {
                "byte" => BuiltInType.Byte,
                "ubyte" => BuiltInType.UByte,
                "short" => BuiltInType.Short,
                "ushort" => BuiltInType.UShort,
                "int" => BuiltInType.Int,
                "uint" => BuiltInType.UInt,
                "long" => BuiltInType.Long,
                "ulong" => BuiltInType.ULong,
                "float" => BuiltInType.Float,
                "double" => BuiltInType.Double,
                "bool" => BuiltInType.Bool,
                "void" => BuiltInType.Void,

                _ => BuiltInType.None,
            };

            if (info.BuiltIn == BuiltInType.None)
                info.CustomTypeName = typeName;

            if (ctx != null && node != null) Validate(ctx, node, ref info);
            DetermindKind(ctx, ref info);
        }
        
        private static void Validate(Context ctx, ASTNode node, ref TypeInfo info)
        {
            int refCnt = 0;

            for(int i = 0; i < info.Modifiers.Count; i++)
            {
                if (info.Modifiers[i] == TypeModifier.Reference)
                {
                    refCnt++;

                    if (refCnt > 1)
                        ctx.Panic("Multiple references are not allowed", node.Ln, node.Col);

                    if (i != 0)
                        ctx.Panic("Reference must be the outermost modifier", node.Ln, node.Col);
                }
            }
        }

        private static void DetermindKind(Context ctx, ref TypeInfo info)
        {
            if (info.Modifiers.Count > 0)
            {
                info.Kind = info.Modifiers[0] switch
                {
                    TypeModifier.Pointer => TypeKind.Pointer,
                    TypeModifier.Reference => TypeKind.Reference,

                    _ => TypeKind.None,
                };
                return;
            }

            if(info.BuiltIn == BuiltInType.Byte ||
                info.BuiltIn == BuiltInType.UByte ||
                info.BuiltIn == BuiltInType.Short ||
                info.BuiltIn == BuiltInType.UShort ||
                info.BuiltIn == BuiltInType.Int ||
                info.BuiltIn == BuiltInType.UInt ||
                info.BuiltIn == BuiltInType.Long ||
                info.BuiltIn == BuiltInType.ULong ||
                info.BuiltIn == BuiltInType.Bool
                )
                info.Kind = TypeKind.Integer;

            else if (info.BuiltIn == BuiltInType.Float || info.BuiltIn == BuiltInType.Double)
                info.Kind = TypeKind.Float;

            else if(info.BuiltIn == BuiltInType.Void)
                info.Kind = TypeKind.None;

            //TODO: structs and classes
        }

        internal string ToCType()
        {
            string baseType = BuiltIn switch
            {
                BuiltInType.None => CustomTypeName,

                _ => "SKULL_" + BuiltIn.ToString().ToUpper()
            };

            StringBuilder typeBuilder = new();

            if (IsReadonly)
                typeBuilder.Append("const ");

            typeBuilder.Append(baseType);

            foreach(TypeModifier mod in Modifiers)
            {
                if (mod == TypeModifier.Pointer)
                    typeBuilder.Append("*");
                else if(mod == TypeModifier.Reference)
                    typeBuilder.Append("*"); //no references in C
            }

            return typeBuilder.ToString();
        }

        internal bool Cmp(TypeInfo other)
        {
            if (this.IsLiteral || other.IsLiteral)
                return Kind == other.Kind;

            if (this.BuiltIn != other.BuiltIn ||
                this.CustomTypeName != other.CustomTypeName ||
                Modifiers.Count != other.Modifiers.Count
                )
                return false;

            for(int i = 0; i < Modifiers.Count; i++)
            {
                if (Modifiers[i] != other.Modifiers[i])
                    return false;
            }

            return true;
        }

        internal void Rebuild(Context ctx = null, ASTNode node = null)
        {
            if (ctx != null && node != null) Validate(ctx, node, ref this);
            DetermindKind(ctx, ref this);
        }

        internal TypeInfo Clone() => new TypeInfo()
        {
            BuiltIn = this.BuiltIn,
            CustomTypeName = this.CustomTypeName,
            Modifiers = new(this.Modifiers),
            IsReadonly = this.IsReadonly,
            IsLiteral = this.IsLiteral,
            Kind = this.Kind,
            OriginalTypeName = this.OriginalTypeName,
        };

        internal string ToStringOriginal() => !String.IsNullOrEmpty(OriginalTypeName) ? OriginalTypeName : Kind.ToString().ToLower();
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

        internal FunctionInfo(string funcName, string returnTypeName, List<VariableInfo> args, bool isExtern = false, Context ctx = null, ASTFunction funcNode = null)
        {
            FuncName = funcName;
            ReturnType = new TypeInfo(returnTypeName, ctx: ctx, node: funcNode);
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
            uname.Append("_sk_f");
            uname.Append(_id);
            uname.Append("_");
            uname.Append(FuncName);
            uname.Append("_");
            uname.Append(ReturnType.ToStringOriginal().ToLower());
            uname.Append("_");

            foreach(var arg in Args)
                uname.Append(arg.Type.OriginalTypeName[0].ToString());

            FuncUName = uname.ToString();
        }
    }
}
