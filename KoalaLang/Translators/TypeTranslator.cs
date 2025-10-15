using KoalaLang.ParserAndAST.AST;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace KoalaLang.Translators
{
    internal static class TypeTranslator
    {
        internal static FunctionInfo FindFunctionInfo(string functionName, int argsCount, TranslationContext ctx, int line)
        {
            //looking for local function
            if (ctx.CurrentModuleHandler != null)
            {
                foreach (FunctionInfo localFunc in ctx.CurrentModuleHandler.Functions)
                {
                    if(localFunc.Name == functionName && (localFunc.Args.Count == argsCount || argsCount == -1))
                    {
                        return localFunc;
                    }
                }
            }

            throw new Exception($"[Error at line {line}] function '{functionName}' not found");
        }

        internal static void EmitCast(ILGenerator il, Type sourceType, Type targetType)
        {
            if (sourceType == targetType) return;

            bool sourceIsValue = sourceType.IsValueType;
            bool targetIsValue = targetType.IsValueType;

            //value to value
            if (sourceIsValue && targetIsValue)
            {
                OpCode convOp = targetType switch
                {
                    var t when t == typeof(sbyte) => OpCodes.Conv_I1,
                    var t when t == typeof(byte) => OpCodes.Conv_U1,
                    var t when t == typeof(short) => OpCodes.Conv_I2,
                    var t when t == typeof(ushort) => OpCodes.Conv_U2,
                    var t when t == typeof(int) => OpCodes.Conv_I4,
                    var t when t == typeof(uint) => OpCodes.Conv_U4,
                    var t when t == typeof(long) => OpCodes.Conv_I8,
                    var t when t == typeof(ulong) => OpCodes.Conv_U8,
                    var t when t == typeof(float) => OpCodes.Conv_R4,
                    var t when t == typeof(double) => OpCodes.Conv_R8,

                    var t when t == typeof(char) => OpCodes.Conv_U2,

                    var t when t == typeof(bool) => OpCodes.Conv_I4,

                    _ => throw new NotSupportedException($"Cannot convert from {sourceType} to {targetType}")
                };
                il.Emit(convOp);
            }

            //value to ref
            else if (sourceIsValue && !targetIsValue)
            {
                il.Emit(OpCodes.Box, sourceType);
                if (targetType != typeof(object))
                    il.Emit(OpCodes.Castclass, targetType);
            }

            //ref to value
            else if (!sourceIsValue && targetIsValue)
            {
                il.Emit(OpCodes.Unbox_Any, targetType);
            }

            //ref to ref
            else
            {
                il.Emit(OpCodes.Castclass, targetType);
            }
        }

        internal static Type GetExpressionType(ASTNode expr, TranslationContext ctx)
        {
            switch (expr)
            {
                case ASTConstant<sbyte>: return typeof(sbyte);
                case ASTConstant<byte>: return typeof(byte);
                case ASTConstant<short>: return typeof(short);
                case ASTConstant<ushort>: return typeof(ushort);
                case ASTConstant<int>: return typeof(int);
                case ASTConstant<uint>: return typeof(uint);
                case ASTConstant<long>: return typeof(long);
                case ASTConstant<ulong>: return typeof(ulong);
                case ASTConstant<float>: return typeof(float);
                case ASTConstant<double>: return typeof(double);
                case ASTConstant<bool>: return typeof(bool);
                case ASTConstant<char>: return typeof(char);
                case ASTConstant<string>: return typeof(string);

                case ASTIdentifier identifier:
                    {
                        Type t = FindClrTypeByName(identifier.Identifier, -1, ctx);
                        if (t != null)
                            return t;

                        try { return ctx.Vars.GetVariable(identifier.Identifier).LocalType; }
                        catch { throw new Exception($"[Error at line {identifier.Line}]: Unknown variable or type '{identifier.Identifier}'"); }
                    }

                case ASTCast staticCast: return ResolveType(staticCast.TypeName, ctx, staticCast.Line);

                case ASTBinOperation binOp: return GetExpressionType(binOp.Left, ctx);
                case ASTUnOperation unOp: return GetExpressionType(unOp.Operand, ctx);

                case ASTFunctionCall funcCall:
                    {
                        FunctionInfo funcInfo = FindFunctionInfo(funcCall.FunctionName, funcCall.Args.Count, ctx, funcCall.Line)
                            ?? throw new Exception($"[Error at line {funcCall.Line}]: Cannot call undefined function '{funcCall.FunctionName}'");

                        int indexOfGenericType = Array.IndexOf(funcInfo.GenericMap.Keys.ToArray(), funcInfo.ReturnType);

                        if (indexOfGenericType != -1)
                            return ResolveType(funcCall.GenericTypes[indexOfGenericType], ctx, funcCall.Line);
                        else
                            return ResolveType(funcInfo.ReturnType, ctx, funcCall.Line);
                    }

                case ASTMethodCall methodCall:
                    {
                        Type targetType = GetExpressionType(methodCall.Target, ctx);

                        MethodInfo method = targetType.GetMethod(
                            methodCall.MethodName,
                             BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static,
                             null,
                             methodCall.Args.Select(a => GetExpressionType(a, ctx)).ToArray(),
                             null
                        ) ?? throw new Exception($"[Error at line {methodCall.Line}]: No method '{methodCall.MethodName}' found on '{targetType}'");

                        return method.ReturnType;
                    }

                case ASTIndexAccess indexAccess:
                    {
                        Type targetType = GetExpressionType(indexAccess.Target, ctx);
                        if (targetType == typeof(string))
                            return typeof(char);
                        else if (targetType.IsArray)
                            return targetType.GetElementType();
                        else throw new Exception($"[Error at line {expr.Line}] Type '{targetType}' does not support indexing");
                    }

                case ASTNew newNode:
                    return ResolveType(newNode.TypeName, ctx, newNode.Line);

                case ASTMemberAccess member:
                    {
                        Type targetType = GetExpressionType(member.Target, ctx);

                        //field
                        var field = targetType.GetField(member.MemberName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
                        if (field != null) return field.FieldType;

                        //prop
                        var prop = targetType.GetProperty(member.MemberName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
                        if (prop != null) return prop.PropertyType;

                        //nested
                        var nestedType = targetType.GetNestedType(member.MemberName, BindingFlags.Public | BindingFlags.NonPublic);
                        if (nestedType != null) return nestedType;

                        throw new Exception($"[Error at line {expr.Line}]: Member '{member.MemberName}' not found on '{targetType}'");
                    }

                default: throw new Exception($"[Error at line {expr.Line}]: Cannot deduce expression type ({expr.GetType().Name})");
            }
        }

        internal static Type ResolveType(string typeName, TranslationContext ctx, int line)
        {
            if (ctx.GenericMap.TryGetValue(typeName, out var mapped))
                return mapped;

            Type primitive = typeName switch
            {
                "void" => typeof(void),
                "bool" => typeof(bool),
                "byte" => typeof(byte),
                "sbyte" => typeof(sbyte),
                "short" => typeof(short),
                "ushort" => typeof(ushort),
                "int" => typeof(int),
                "uint" => typeof(uint),
                "long" => typeof(long),
                "ulong" => typeof(ulong),
                "float" => typeof(float),
                "double" => typeof(double),
                "char" => typeof(char),
                "string" => typeof(string),
                "object" => typeof(object),
                _ => null
            };
            if (primitive != null)
                return primitive;

            int arrayDepth = 0;
            while (typeName.EndsWith("[]"))
            {
                typeName = typeName.Substring(0, typeName.Length - 2);
                arrayDepth += 1;
            }

            List<Type> genericArgs = null;
            int genericStart = typeName.IndexOf('<');
            if (genericStart != -1)
            {
                int genericEnd = typeName.LastIndexOf('>');

                string genericBase = typeName.Substring(0, genericStart);
                string inside = typeName.Substring(genericStart + 1, genericEnd - genericStart - 1);
                string[] argNames = inside.Split(',');

                genericArgs = new();
                foreach (string argName in argNames)
                {
                    genericArgs.Add(ResolveType(argName.Trim(), ctx, line));
                }

                typeName = genericBase;
            }

            Type baseType = FindClrTypeByName(typeName, genericArgs == null ? 0 : genericArgs.Count, ctx);
            if (baseType == null)
                throw new Exception($"[Error at line {line}]: Unknown type '{typeName}'");

            if (genericArgs != null && baseType.IsGenericTypeDefinition)
            {
                try
                {
                    baseType = baseType.MakeGenericType(genericArgs.ToArray());
                }
                catch (Exception ex)
                {
                    throw new Exception($"[Error at line {line}]: Failed to construct generic type '{typeName}': {ex.Message}");
                }
            }

            while (arrayDepth-- > 0)
                baseType = baseType.MakeArrayType();

            return baseType;
        }

        internal static Type FindClrTypeByName(string id, int genericArity, TranslationContext ctx)
        {
            //static bool MatchesTypeName(Type t, string id, int genericArity)
            //{
            //    if (genericArity > 0)
            //    {
            //        return t.IsGenericTypeDefinition &&
            //               (t.Name == id + "`" + genericArity ||
            //                t.FullName?.EndsWith("." + id + "`" + genericArity) == true);
            //    }
            //    else
            //    {
            //        return t.Name == id || t.FullName?.EndsWith("." + id) == true;
            //    }
            //}

            Type t = id switch
            {
                "void" => typeof(void),
                "bool" => typeof(bool),
                "byte" => typeof(byte),
                "sbyte" => typeof(sbyte),
                "short" => typeof(short),
                "ushort" => typeof(ushort),
                "int" => typeof(int),
                "uint" => typeof(uint),
                "long" => typeof(long),
                "ulong" => typeof(ulong),
                "float" => typeof(float),
                "double" => typeof(double),
                "char" => typeof(char),
                "string" => typeof(string),
                "object" => typeof(object),
                _ => null
            };
            if (t != null)
                return t;

            //find in imports
            foreach (string import in ctx.CurrentModuleHandler.Imports)
            {
                string fullName = $"{import}.{id}";

                foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
                {
                    Type found = null;
                    try
                    {
                        found = asm.GetType(fullName, throwOnError: false, ignoreCase: false);
                    }
                    catch { }

                    if (found != null)
                    {
                        if (genericArity > 0 && found.IsGenericTypeDefinition && found.GetGenericArguments().Length == genericArity)
                            return found;
                        if (genericArity == 0 || genericArity == -1)
                            return found;
                    }
                }
            }

            return null;
        }
    }
}
