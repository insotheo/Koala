using KoalaLang.ParserAndAST;
using KoalaLang.ParserAndAST.AST;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

using static KoalaLang.Translators.TypeTranslator;

namespace KoalaLang.Translators
{
    public sealed class ILTranslator(string asmName)
    {
        string _asmName = asmName;
        List<ModuleInfo> _mods = new List<ModuleInfo>();

        AssemblyBuilder _assemblyBuilder;
        ModuleBuilder _moduleBuilder;

        public void Translate(Parser parser, string moduleName, string outputPath)
        {
            AssemblyName assemblyName = new AssemblyName(_asmName);

            _assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.RunAndCollect);
            _moduleBuilder = _assemblyBuilder.DefineDynamicModule("ApplicationDynamicModule");

            ModuleInfo mod = null;
            try
            {
                DefineModule(ref _mods, moduleName, parser.GetAST() as ASTCodeBlock, null); //head module
                mod = _mods.Last();

                foreach (ASTNode node in (parser.GetAST() as ASTCodeBlock).Nodes)
                {
                    if (node is ASTFunction func)
                    {
                        FunctionInfo funcInfo = mod.Functions.FirstOrDefault(x => x.Name == func.FunctionName, null)
                            ?? throw new Exception($"[Error at line {func.Line}]: Function '{func.FunctionName}' was not declared before use");

                        ILGenerator il = (funcInfo.Info as MethodBuilder).GetILGenerator();

                        TranslationContext funcCtx = new(il, funcInfo)
                        {
                            CurrentFunction = funcInfo,
                            GenericMap = funcInfo.GenericMap,
                            CurrentModuleHandler = mod,
                        };

                        {
                            int argIdx = 0;
                            foreach ((string argName, string argType) in func.Args)
                            {
                                funcCtx.DeclareLocalVariable(argName, ResolveType(argType, funcCtx, func.Line), func.Line);
                                il.Emit(OpCodes.Ldarg, argIdx);
                                il.Emit(OpCodes.Stloc, funcCtx.Vars.GetVariable(argName));
                                argIdx += 1;
                            }
                        }

                        TranslateBody(func.Body, funcCtx);
                        il.Emit(OpCodes.Ret);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Translator error(at {(mod != null ? mod.GetFullName() : moduleName)}): {ex.Message}");
                Environment.Exit(-1);
            }

            Type[] translatedModules = _mods.Select(m => m.TypeBuilder.CreateType()).ToArray();

            //DBG
            translatedModules[0].GetMethod("Main")!.Invoke(null, null);
        }

        private void TranslateBody(ASTCodeBlock block, TranslationContext ctx)
        {
            foreach (ASTNode node in block.Nodes)
                TranslateNode(node, ctx);
        }

        private void TranslateNode(ASTNode node, TranslationContext ctx)
        {
            ILGenerator il = ctx.IL;

            switch (node)
            {
                case ASTReturn ret:
                    TranslateExpression(ret.ReturnValue, ctx);
                    il.Emit(OpCodes.Ret);
                    break;

                case ASTVariableDeclaration varDecl:
                    ctx.DeclareLocalVariable(varDecl.Name, ResolveType(varDecl.Type, ctx, varDecl.Line), varDecl.Line);
                    break;

                case ASTAssignment assign:
                    {
                        if(assign.Destination is ASTIdentifier varDest)
                        {
                            if (!ctx.Vars.VarExists(varDest.Identifier))
                                throw new Exception($"[Error at line {assign.Line}]: Cannot assign to undefined variable '{varDest.Identifier}'");

                            TranslateExpression(assign.Value, ctx);
                            il.Emit(OpCodes.Stloc, ctx.Vars.GetVariable(varDest.Identifier));
                        }
                        else if(assign.Destination is ASTIndexAccess idxAccess)
                        {
                            Type targetType = GetExpressionType(idxAccess.Target, ctx);
                            Type idxType = GetExpressionType(idxAccess.Index, ctx);
                            Type valueType = GetExpressionType(assign.Value, ctx);

                            if(targetType == typeof(string))
                            {
                                if (idxAccess.Target is not ASTIdentifier varUse)
                                    throw new Exception($"[Error at line {assign.Line}]: Cannot assign to string literal - only string variables are mutable via copy");

                                if (!ctx.Vars.VarExists(varUse.Identifier))
                                    throw new Exception($"[Error at line {assign.Line}]: Cannot assign to undefined variable '{varUse.Identifier}'");

                                il.Emit(OpCodes.Ldloc, ctx.Vars.GetVariable(varUse.Identifier));
                                il.Emit(OpCodes.Call, typeof(string).GetMethod("ToCharArray", Type.EmptyTypes));
                                il.Emit(OpCodes.Dup);

                                TranslateExpression(idxAccess.Index, ctx); //load index
                                TranslateExpression(assign.Value, ctx); // load new char
                                il.Emit(OpCodes.Stelem_I2);

                                il.Emit(OpCodes.Newobj, typeof(string).GetConstructor(new[] { typeof(char[]) }));
                                il.Emit(OpCodes.Stloc, ctx.Vars.GetVariable(varUse.Identifier));
                            }
                            else if (targetType.IsArray)
                            {
                                Type elementType = targetType.GetElementType();

                                if(idxType != typeof(int))
                                    throw new Exception($"[Error at line {idxAccess.Line}]: Array index must be type of int");
                                if (elementType != valueType)
                                    throw new Exception($"[Error at line {assign.Line}]: Cannot assign value of type '{valueType}' to array of '{elementType}'");

                                TranslateExpression(idxAccess.Target, ctx); //load array
                                TranslateExpression(idxAccess.Index, ctx); //load index
                                TranslateExpression(assign.Value, ctx); //load value
                                il.Emit(OpCodes.Stelem, elementType);
                            }
                            else throw new Exception($"[Error at line {assign.Line}]: Type '{targetType}' does not support indexed assignment");
                        }
                        else if(assign.Destination is ASTMemberAccess member)
                        {
                            Type targetType = GetExpressionType(member.Target, ctx);
                            TranslateExpression(member.Target, ctx);

                            //field
                            FieldInfo field = targetType.GetField(member.MemberName,
                                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
                            if (field != null)
                            {
                                TranslateExpression(assign.Value, ctx);
                                il.Emit(field.IsStatic ? OpCodes.Stsfld : OpCodes.Stfld, field);
                                return;
                            }

                            //prop
                            PropertyInfo prop = targetType.GetProperty(member.MemberName,
                                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
                            if (prop != null)
                            {
                                MethodInfo setter = prop.GetSetMethod(true);
                                if (setter == null)
                                    throw new Exception($"[Error at line {assign.Line}]: Property '{member.MemberName}' has no setter");
                                TranslateExpression(assign.Value, ctx);
                                il.Emit(setter.IsVirtual ? OpCodes.Callvirt : OpCodes.Call, setter);
                                return;
                            }
                        }
                        else throw new Exception($"[Error at line {assign.Line}]: Invalid assignment target");
                    }
                    break;

                case ASTBranch branch:
                    {
                        TranslationContext branchCtx = ctx.CreateKid();
                        Label endLb = il.DefineLabel();

                        for(int i = 0; i < branch.Ifs.Length; i++)
                        {
                            ASTConditionBlock ifBlock = branch.Ifs[i];
                            TranslationContext ifCtx = branchCtx.CreateKid();

                            Label nextIfLb = il.DefineLabel();

                            TranslateExpression(ifBlock.Condition, ifCtx);
                            il.Emit(OpCodes.Brfalse, nextIfLb); //if condition fails

                            TranslateBody(ifBlock.Body, ifCtx);
                            il.Emit(OpCodes.Br, endLb);
                            
                            ctx.FreeKid(ifCtx);

                            il.MarkLabel(nextIfLb);
                        }

                        if(branch.Else != null)
                        {
                            TranslationContext elseCtx = branchCtx.CreateKid();
                            TranslateBody(branch.Else, elseCtx);
                            ctx.FreeKid(elseCtx);
                        }
                        il.MarkLabel(endLb);

                        ctx.FreeKid(branchCtx);
                    }
                    break;

                case ASTWhileLoop whileLoop:
                    {
                        TranslationContext loopCtx = ctx.CreateKid();
                        loopCtx.RegionStart = il.DefineLabel();
                        loopCtx.RegionEnd = il.DefineLabel();

                        il.MarkLabel(loopCtx.RegionStart.Value);
                        TranslateExpression(whileLoop.Condition, loopCtx);
                        il.Emit(OpCodes.Brfalse, loopCtx.RegionEnd.Value);
                        TranslateBody(whileLoop.Body, loopCtx);
                        il.Emit(OpCodes.Br, loopCtx.RegionStart.Value);
                        il.MarkLabel(loopCtx.RegionEnd.Value);

                        ctx.FreeKid(loopCtx);
                    }
                    break;

                case ASTDoWhileLoop doWhileLoop:
                    {
                        TranslationContext loopCtx = ctx.CreateKid();
                        loopCtx.RegionStart = il.DefineLabel();
                        loopCtx.RegionEnd = il.DefineLabel();
                        Label loopBegin = il.DefineLabel();

                        il.MarkLabel(loopBegin);
                        TranslateBody(doWhileLoop.Body, loopCtx);
                        il.MarkLabel(loopCtx.RegionStart.Value);
                        TranslateExpression(doWhileLoop.Condition, loopCtx);
                        il.Emit(OpCodes.Brfalse, loopCtx.RegionEnd.Value);
                        il.Emit(OpCodes.Br, loopBegin);
                        il.MarkLabel(loopCtx.RegionEnd.Value);

                        ctx.FreeKid(loopCtx);
                    }
                    break;

                case ASTForLoop forLoop:
                    {
                        TranslationContext loopCtx = ctx.CreateKid();

                        ASTCodeBlock forLoopCodeBlock = new(-1);
                        forLoopCodeBlock.Nodes.Add(forLoop.VariableDeclaration);

                        ASTCodeBlock forLoopBody = forLoop.Body;
                        forLoopBody.Nodes.Add(forLoop.IterAction);

                        forLoopCodeBlock.Nodes.Add(new ASTWhileLoop(forLoop.Condition, forLoopBody, forLoop.Line));
                        TranslateBody(forLoopCodeBlock, loopCtx);

                        ctx.FreeKid(loopCtx);
                    }
                    break;

                case ASTFunctionCall funcCall:
                    TranslateFunctionCall(funcCall, ctx);
                    break;

                case ASTMethodCall methodCall:
                    TranslateFunctionCall(methodCall, ctx);
                    break;

                case ASTCodeBlock inner:
                    {
                        TranslationContext innerCtx = ctx.CreateKid();
                        TranslateBody(inner, innerCtx);
                        ctx.FreeKid(innerCtx);
                    }
                    break;

                case ASTCompoundStatement compoundStatement:
                    TranslateNode(compoundStatement.I, ctx);
                    TranslateNode(compoundStatement.II, ctx);
                    break;

                case ASTBreak:
                    if(!ctx.RegionEnd.HasValue)
                        throw new Exception($"[Error at line {node.Line}]: No enclosing loop out of which to break or continue");
                    il.Emit(OpCodes.Br, ctx.RegionEnd.Value);
                    break;

                case ASTContinue:
                    if (!ctx.RegionStart.HasValue)
                        throw new Exception($"[Error at line {node.Line}]: No enclosing loop out of which to break or continue");
                    il.Emit(OpCodes.Br, ctx.RegionStart.Value);
                    break;

                default: return;
            }
        }

        private void TranslateExpression(ASTNode expr, TranslationContext ctx)
        {
            ILGenerator il = ctx.IL;

            if (expr == null) return;

            else if (expr is ASTConstant<sbyte> bConst) { il.Emit(OpCodes.Ldc_I4, (int)bConst.Value); il.Emit(OpCodes.Conv_I1); }
            else if (expr is ASTConstant<byte> ubConst) { il.Emit(OpCodes.Ldc_I4, (int)ubConst.Value); il.Emit(OpCodes.Conv_U1); }
            else if (expr is ASTConstant<short> sConst) { il.Emit(OpCodes.Ldc_I4, (int)sConst.Value); il.Emit(OpCodes.Conv_I2); }
            else if (expr is ASTConstant<ushort> usConst) { il.Emit(OpCodes.Ldc_I4, (int)usConst.Value); il.Emit(OpCodes.Conv_U2); }
            else if (expr is ASTConstant<int> intConst) { il.Emit(OpCodes.Ldc_I4, intConst.Value); }
            else if (expr is ASTConstant<uint> uConst) { il.Emit(OpCodes.Ldc_I4, (int)uConst.Value); il.Emit(OpCodes.Conv_U4); }
            else if (expr is ASTConstant<long> lConst) { il.Emit(OpCodes.Ldc_I8, lConst.Value); }
            else if (expr is ASTConstant<ulong> ulConst) { il.Emit(OpCodes.Ldc_I8, (long)ulConst.Value); il.Emit(OpCodes.Conv_U8); }
            else if (expr is ASTConstant<float> fConst) il.Emit(OpCodes.Ldc_R4, fConst.Value);
            else if (expr is ASTConstant<double> doubleConst) il.Emit(OpCodes.Ldc_R8, doubleConst.Value);
            else if (expr is ASTConstant<bool> boolConst) il.Emit(boolConst.Value ? OpCodes.Ldc_I4_1 : OpCodes.Ldc_I4_0);
            else if (expr is ASTConstant<char> charConst) { il.Emit(OpCodes.Ldc_I4, (int)charConst.Value); il.Emit(OpCodes.Conv_U2); }
            else if (expr is ASTConstant<string> stringConst) il.Emit(OpCodes.Ldstr, stringConst.Value);

            else if (expr is ASTIdentifier varUse)
            {
                //if such type exists and it is static - pass
                Type staticType = FindClrTypeByName(varUse.Identifier, 0, ctx);
                if (staticType != null && ((staticType.IsAbstract && staticType.IsSealed) || (staticType.GetMethods().Select(m => m.IsStatic).ToArray().Length != 0) || staticType.IsEnum)) return;

                if (!ctx.Vars.VarExists(varUse.Identifier))
                    throw new Exception($"[Error at line {varUse.Line}]: Variable '{varUse.Identifier}' is used before being defined");
                il.Emit(OpCodes.Ldloc, ctx.Vars.GetVariable(varUse.Identifier));
            }

            else if (expr is ASTFunctionCall funcCall) TranslateFunctionCall(funcCall, ctx);
            else if (expr is ASTMethodCall methodCall) TranslateFunctionCall(methodCall, ctx);

            else if (expr is ASTBinOperation binOp)
            {
                Type leftType = GetExpressionType(binOp.Left, ctx);
                Type rightType = GetExpressionType(binOp.Right, ctx);

                if (leftType != rightType) throw new Exception($"[Error at line {expr.Line}]: Cannot operate with different types: {leftType}, {rightType}");

                TranslateExpression(binOp.Left, ctx);
                TranslateExpression(binOp.Right, ctx);
                
                switch (binOp.OperationType)
                {
                    case BinOperationType.Add:
                        {
                            if (leftType == typeof(string) && rightType == typeof(string))
                            {
                                il.Emit(OpCodes.Call, typeof(string).GetMethod("Concat", new[] { typeof(string), typeof(string) }));
                            }
                            else il.Emit(OpCodes.Add);
                        }
                        break;

                    case BinOperationType.Subtract: il.Emit(OpCodes.Sub); break;
                    case BinOperationType.Multiply: il.Emit(OpCodes.Mul); break;
                    case BinOperationType.Divide: il.Emit(OpCodes.Div); break;
                    case BinOperationType.Remain: il.Emit(OpCodes.Rem); break;

                    case BinOperationType.BitwiseAnd:
                    case BinOperationType.LogicalAnd: il.Emit(OpCodes.And); break;

                    case BinOperationType.BitwiseOr:
                    case BinOperationType.LogicalOr: il.Emit(OpCodes.Or); break;

                    case BinOperationType.Xor: il.Emit(OpCodes.Xor); break;

                    case BinOperationType.LeftShift: il.Emit(OpCodes.Shl); break;
                    case BinOperationType.RightShift: il.Emit(OpCodes.Shr); break;

                    case BinOperationType.CmpEqual: il.Emit(OpCodes.Ceq); break;
                    case BinOperationType.CmpInequal: il.Emit(OpCodes.Ceq); il.Emit(OpCodes.Ldc_I4_0); il.Emit(OpCodes.Ceq); break;
                    case BinOperationType.CmpLess: il.Emit(OpCodes.Clt); break;
                    case BinOperationType.CmpLessOrEq: il.Emit(OpCodes.Cgt); il.Emit(OpCodes.Ldc_I4_0); il.Emit(OpCodes.Ceq); break; //(a > b) == false -> !(a > b) -> a <= b
                    case BinOperationType.CmpMore: il.Emit(OpCodes.Cgt); break;
                    case BinOperationType.CmpMoreOrEq: il.Emit(OpCodes.Clt); il.Emit(OpCodes.Ldc_I4_0); il.Emit(OpCodes.Ceq); break; //(a < b) == false -> !(a < b) -> a >= b

                    default: throw new Exception($"[Error at line {binOp.Line}]: Unknown binary operation '{binOp.OperationType}'");
                }
            }

            else if (expr is ASTUnOperation unOp)
            {
                TranslateExpression(unOp.Operand, ctx);

                switch (unOp.OperationType)
                {
                    case UnaryOperationType.Negate: il.Emit(OpCodes.Neg); break;
                    case UnaryOperationType.LogicalNot: il.Emit(OpCodes.Ldc_I4_1); il.Emit(OpCodes.Xor); break;
                    case UnaryOperationType.BitwiseNot: il.Emit(OpCodes.Not); break;

                    default: throw new Exception($"[Error at line {unOp.Line}]: Unknown unary operation '{unOp.OperationType}'");
                }
            }

            else if (expr is ASTIndexAccess indexAccess)
            {
                TranslateExpression(indexAccess.Target, ctx);
                TranslateExpression(indexAccess.Index, ctx);

                Type targetType = GetExpressionType(indexAccess.Target, ctx);
                Type indexType = GetExpressionType(indexAccess.Index, ctx);

                if (targetType == typeof(string))
                {
                    il.Emit(OpCodes.Callvirt, typeof(string).GetProperty("Chars").GetGetMethod());
                }
                else if (targetType.IsArray)
                {
                    if (indexType != typeof(int))
                        throw new Exception($"[Error at line {expr.Line}]: Array index must be type of int");

                    Type elementType = targetType.GetElementType();
                    il.Emit(OpCodes.Ldelem, elementType);
                }
                else
                    throw new Exception($"[Error at line {expr.Line}] Type '{targetType}' does not support indexing");
            }

            else if (expr is ASTCast staticCast)
            {
                TranslateExpression(staticCast.Value, ctx);

                Type sourceType = GetExpressionType(staticCast.Value, ctx);
                Type targetType = ResolveType(staticCast.TypeName, ctx, staticCast.Line);

                EmitCast(il, sourceType, targetType);
            }

            else if (expr is ASTNew newNode)
            {
                Type newType = ResolveType(newNode.TypeName, ctx, newNode.Line);

                if (newType.IsArray)
                {
                    if (newNode.Args.Length != 1)
                        throw new Exception($"[Error at line {newNode.Line}]: Array creation requires a single length argument");

                    Type argType = GetExpressionType(newNode.Args[0], ctx);
                    if (argType != typeof(int))
                        throw new Exception($"[Error at line {newNode.Args[0].Line}]: Array index must be type of int");

                    TranslateExpression(newNode.Args[0], ctx); //push length
                    Type elType = newType.GetElementType();
                    il.Emit(OpCodes.Newarr, elType);
                }
                else
                {
                    Type[] paramTypes = new Type[newNode.Args.Length];
                    for (int i = 0; i < newNode.Args.Length; i++)
                        paramTypes[i] = GetExpressionType(newNode.Args[i], ctx);

                    ConstructorInfo ctor = newType.GetConstructor(paramTypes);

                    if (ctor == null)
                    {
                        var candidates = newType.GetConstructors().Where(c => c.GetParameters().Length == paramTypes.Length).ToArray();
                        foreach (var c in candidates)
                        {
                            var pi = c.GetParameters();
                            bool ok = true;
                            for (int i = 0; i < pi.Length; i++)
                            {
                                if (!pi[i].ParameterType.IsAssignableFrom(paramTypes[i]))
                                {
                                    if (pi[i].ParameterType.IsPrimitive && paramTypes[i].IsPrimitive)
                                        continue;
                                    ok = false;
                                    break;
                                }
                            }
                            if (ok) { ctor = c; break; }
                        }
                    }

                    if (ctor == null)
                        throw new Exception($"[Error at line {newNode.Line}]: No constructor found for '{newNode.TypeName}' with {newNode.Args.Length} arguments");

                    foreach (var arg in newNode.Args)
                        TranslateExpression(arg, ctx);
                    il.Emit(OpCodes.Newobj, ctor);
                }
            }

            else if (expr is ASTMemberAccess member)
            {
                Type targetType = null;
                bool exprWasTranslated = false;

                if(member.Target is ASTIdentifier id)
                {
                    targetType = FindClrTypeByName(id.Identifier, -1, ctx);
                }
                else
                {
                    TranslateExpression(member.Target, ctx);
                    exprWasTranslated = true;
                    targetType = GetExpressionType(member.Target, ctx);
                }

                if(targetType == null)
                {
                    TranslateExpression(member.Target, ctx);
                    exprWasTranslated = true;
                    targetType = GetExpressionType(member.Target, ctx);
                }

                //enum
                if (targetType.IsEnum)
                {
                    var enumField = targetType.GetField(member.MemberName, BindingFlags.Public | BindingFlags.Static);
                    if (enumField == null)
                        throw new Exception($"[Error at line {expr.Line}]: Enum '{targetType}' has no member '{member.MemberName}'");
                    var rawVal = enumField.GetRawConstantValue();
                    if (!exprWasTranslated)
                    {
                        TranslateExpression(Type.GetTypeCode(Enum.GetUnderlyingType(targetType)) switch
                        {
                            TypeCode.SByte => new ASTConstant<sbyte>((sbyte)rawVal, -1),
                            TypeCode.Byte => new ASTConstant<byte>((byte)rawVal, -1),
                            TypeCode.Int16 => new ASTConstant<short>((short)rawVal, -1),
                            TypeCode.UInt16 => new ASTConstant<ushort>((ushort)rawVal, -1),
                            TypeCode.Int32 => new ASTConstant<int>((int)rawVal, -1),
                            TypeCode.UInt32 => new ASTConstant<uint>((uint)rawVal, -1),
                            TypeCode.Int64 => new ASTConstant<long>((long)rawVal, -1),
                            TypeCode.UInt64 => new ASTConstant<ulong>((ulong)rawVal, -1),

                            _ => throw new Exception($"[Error at line {expr.Line}]: Unsupported enum base type")
                        }, ctx);
                    }
                    return;
                }

                //field
                FieldInfo fieldInfo = targetType.GetField(member.MemberName,
                    BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
                if (fieldInfo != null)
                {
                    il.Emit(fieldInfo.IsStatic ? OpCodes.Ldsfld : OpCodes.Ldfld, fieldInfo);
                    return;
                }

                //prop
                PropertyInfo propInfo = targetType.GetProperty(member.MemberName,
                    BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
                if (propInfo != null)
                {
                    MethodInfo getter = propInfo.GetGetMethod();
                    if (getter == null)
                        throw new Exception($"[Error at line {expr.Line}]: Property '{member.MemberName}' has no getter");
                    il.Emit(getter.IsVirtual ? OpCodes.Callvirt : OpCodes.Call, getter);
                    return;
                }

                //nested type
                Type nested = targetType.GetNestedType(member.MemberName,
                    BindingFlags.Public | BindingFlags.NonPublic);
                if (nested != null)
                {
                    return;
                }

                throw new Exception($"[Error at line {expr.Line}]: Member '{member.MemberName}' not found on type '{targetType}'");
            }

            else throw new Exception($"[Error at line {expr.Line}]: Unknown expression type '{expr.GetType().Name}'");
        }

        private void TranslateFunctionCall(ASTNode callNode, TranslationContext ctx)
        {
            ILGenerator il = ctx.IL;

            if (callNode is ASTFunctionCall funcCall)
            {

                FunctionInfo funcInfo = FindFunctionInfo(funcCall.FunctionName, -1, ctx, funcCall.Line)
                    ?? throw new Exception($"[Error at line {funcCall.Line}]: Undefined function '{funcCall.FunctionName}'");
                MethodInfo methodInfo = funcInfo.Info;

                if (methodInfo == null)
                    throw new Exception($"[Error at line {funcCall.Line}]: Cannot call undefined function '{funcCall.FunctionName}'");

                if (funcCall.GenericTypes.Count != 0 && methodInfo.IsGenericMethod)
                {
                    Type[] typeArgs = funcCall.GenericTypes.Select(t => ResolveType(t, ctx, funcCall.Line)).ToArray();
                    methodInfo = methodInfo.MakeGenericMethod(typeArgs);
                }
                else if (methodInfo.IsGenericMethod && funcCall.GenericTypes.Count == 0)
                    throw new Exception($"[Error at line {funcCall.Line}]: Function '{funcCall.FunctionName}' is generic");

                //loading args
                foreach (ASTNode arg in funcCall.Args)
                    TranslateExpression(arg, ctx);

                //call
                il.Emit(OpCodes.Call, methodInfo);
            }
            else if (callNode is ASTMethodCall methodCall)
            {
                Type targetType = GetExpressionType(methodCall.Target, ctx);

                MethodInfo method = targetType.GetMethod(
                        methodCall.MethodName,
                         BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static,
                         null,
                         methodCall.Args.Select(a => GetExpressionType(a, ctx)).ToArray(),
                         null
                ) ?? throw new Exception($"[Error at line {methodCall.Line}]: No method '{methodCall.MethodName}' found on '{targetType}'");

                if (methodCall.GenericTypes.Count != 0 && method.IsGenericMethod)
                {
                    Type[] typeArgs = methodCall.GenericTypes.Select(t => ResolveType(t, ctx, methodCall.Line)).ToArray();
                    method = method.MakeGenericMethod(typeArgs);
                }
                else if (method.IsGenericMethod && methodCall.GenericTypes.Count == 0)
                    throw new Exception($"[Error at line {methodCall.Line}]: Function '{methodCall.MethodName}' is generic");

                bool isValueType = targetType.IsValueType;
                if (isValueType)
                {
                    if (methodCall.Target is ASTIdentifier varUse && ctx.Vars.VarExists(varUse.Identifier))
                    {
                        il.Emit(OpCodes.Ldloca, ctx.Vars.GetVariable(varUse.Identifier));
                    }
                    else
                    {
                        TranslateExpression(methodCall.Target, ctx);
                        il.Emit(OpCodes.Box, targetType);
                    }

                    foreach (ASTNode arg in methodCall.Args)
                        TranslateExpression(arg, ctx);

                    il.Emit(OpCodes.Call, method);
                }
                else
                {
                    TranslateExpression(methodCall.Target, ctx);
                    foreach (ASTNode arg in methodCall.Args)
                        TranslateExpression(arg, ctx);

                    il.Emit(method.IsVirtual ? OpCodes.Callvirt : OpCodes.Call, method);
                }
            }
        }

        private void DefineModule(ref List<ModuleInfo> modules, string moduleName, ASTCodeBlock block, ModuleInfo currentModule)
        {
            ModuleInfo mod = new ModuleInfo(moduleName, currentModule, _moduleBuilder.DefineType(moduleName, TypeAttributes.Class | TypeAttributes.Public));
            foreach (ASTNode node in block.Nodes)
            {
                if (node is ASTFunction func)
                {
                    MethodBuilder methodBuilder = mod.TypeBuilder.DefineMethod(
                        func.FunctionName,
                        MethodAttributes.Public | MethodAttributes.Static
                    );

                    Dictionary<string, GenericTypeParameterBuilder> genericMap = new();
                    if (func.GenericTypes.Count != 0)
                    {
                        var genParams = methodBuilder.DefineGenericParameters(func.GenericTypes.ToArray());
                        for (int i = 0; i < genParams.Length; i++)
                        {
                            genericMap.Add(func.GenericTypes[i], genParams[i]);
                        }
                    }

                    TranslationContext tmpCtx = new(null, null)
                    {
                        GenericMap = genericMap,
                        CurrentModuleHandler = mod,
                    };

                    Type[] args = new Type[func.Args.Count];
                    {
                        int i = 0;
                        foreach (var (argName, argType) in func.Args)
                        {
                            args[i] = ResolveType(argType, tmpCtx, func.Line);
                            i += 1;
                        }
                    }

                    Type returnType = ResolveType(func.ReturnTypeName, tmpCtx, func.Line);

                    methodBuilder.SetReturnType(returnType);
                    methodBuilder.SetParameters(args);

                    FunctionInfo funcInfo = new(func.FunctionName, func.ReturnTypeName, func.Args) { GenericMap = genericMap };
                    funcInfo.Info = methodBuilder;
                    mod.Functions.Add(funcInfo);
                }
                else if (node is ASTImport import)
                {
                    mod.Imports.Add(import.Path);
                    //TODO: another modules support...
                }
            }
            modules.Add(mod);
        }

    }
}
