using KoalaLang.ParserAndAST;
using KoalaLang.ParserAndAST.AST;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace KoalaLang.Translators
{
    public sealed class ILTranslator(string asmName)
    {
        string _asmName = asmName;
        List<ModuleInfo> _mods = new List<ModuleInfo>();
        ModuleInfo _currentModule = null;

        AssemblyBuilder _assemblyBuilder;
        ModuleBuilder _moduleBuilder;

        public void Translate(Parser parser, string moduleName)
        {
            AssemblyName assemblyName = new AssemblyName(_asmName);

            _assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.RunAndCollect);
            _moduleBuilder = _assemblyBuilder.DefineDynamicModule("ApplicationDynamicModule");

            try
            {
                DefineModule(ref _mods, moduleName, parser.GetAST() as ASTCodeBlock);
                _currentModule = _mods[0];

                foreach (ASTNode node in (parser.GetAST() as ASTCodeBlock).Nodes)
                {
                    if (node is ASTFunction func)
                    {
                        FunctionInfo funcInfo = _currentModule.Functions.FirstOrDefault(x => x.Name == func.FunctionName, null)
                            ?? throw new Exception($"[Error at line {func.Line}]: Function '{func.FunctionName}' was not declared before use");

                        ILGenerator il = (funcInfo.Info as MethodBuilder).GetILGenerator();

                        TranslationContext funcCtx = new(il, funcInfo)
                        {
                            CurrentFunction = funcInfo,
                            GenericMap = funcInfo.GenericMap,
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

                        funcCtx.Free();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Translator error(at {(_currentModule != null ? _currentModule.GetFullName() : moduleName)}): {ex.Message}");
                Environment.Exit(-1);
            }

            Type[] translatedModules = _mods.Select(m => m.TypeBuilder.CreateType()).ToArray();

            //DBG
            Console.WriteLine($"Function Main returned: {translatedModules[0].GetMethod("Main")!.Invoke(null, null)}");
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
                    if (!ctx.Vars.VarExists(assign.DestinationName))
                        throw new Exception($"[Error at line {assign.Line}]: Cannot assign to undefined variable '{assign.DestinationName}'");
                    TranslateExpression(assign.Value, ctx);
                    il.Emit(OpCodes.Stloc, ctx.Vars.GetVariable(assign.DestinationName));
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
                            
                            ifCtx.Free();

                            il.MarkLabel(nextIfLb);
                        }

                        if(branch.Else != null)
                        {
                            TranslationContext elseCtx = branchCtx.CreateKid();
                            TranslateBody(branch.Else, elseCtx);
                            elseCtx.Free();
                        }
                        il.MarkLabel(endLb);

                        branchCtx.Free();
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

                        loopCtx.Free();
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

                        loopCtx.Free();
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
                        TranslateNode(forLoopCodeBlock, loopCtx);

                        loopCtx.Free();
                    }
                    break;

                case ASTFunctionCall funcCall:
                    TranslateFunctionCall(funcCall, ctx);
                    break;

                case ASTCodeBlock inner:
                    {
                        TranslationContext innerCtx = ctx.CreateKid();
                        TranslateBody(inner, innerCtx);
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

            else if (expr is ASTConstant<int> intConst) il.Emit(OpCodes.Ldc_I4, intConst.Value);
            else if (expr is ASTConstant<float> floatConst) il.Emit(OpCodes.Ldc_R4, floatConst.Value);
            else if (expr is ASTConstant<bool> boolConst) il.Emit(boolConst.Value ? OpCodes.Ldc_I4_1 : OpCodes.Ldc_I4_0);

            else if (expr is ASTVariableUse varUse)
            {
                if (!ctx.Vars.VarExists(varUse.VariableName))
                {
                    throw new Exception($"[Error at line {varUse.Line}]: Variable '{varUse.VariableName}' is used before being defined");
                }
                il.Emit(OpCodes.Ldloc, ctx.Vars.GetVariable(varUse.VariableName));
            }

            else if (expr is ASTFunctionCall funcCall) TranslateFunctionCall(funcCall, ctx);

            else if (expr is ASTBinOperation binOp)
            {
                Type leftType = GetExpressionType(binOp.Left, ctx);
                Type rightType = GetExpressionType(binOp.Right, ctx);

                if (leftType != rightType) throw new Exception($"[Error at line {expr.Line}]: Cannot operate with different types: {leftType}, {rightType}");

                TranslateExpression(binOp.Left, ctx);
                TranslateExpression(binOp.Right, ctx);

                switch (binOp.OperationType)
                {
                    case BinOperationType.Add: il.Emit(OpCodes.Add); break;
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

            else if (expr is ASTCast staticCast)
            {
                TranslateExpression(staticCast.Value, ctx);

                Type sourceType = GetExpressionType(staticCast.Value, ctx);
                Type targetType = ResolveType(staticCast.TypeName, ctx, staticCast.Line);

                EmitCast(il, sourceType, targetType);
            }

            else throw new Exception($"[Error at line {expr.Line}]: Unknown expression type '{expr.GetType().Name}'");
        }

        private void TranslateFunctionCall(ASTFunctionCall funcCall, TranslationContext ctx)
        {
            ILGenerator il = ctx.IL;

            FunctionInfo funcInfo = FindFunctionInfo(funcCall.FunctionName, -1, funcCall.Line) 
                ?? throw new Exception($"[Error at line {funcCall.Line}]: Undefined function '{funcCall.FunctionName}'");
            MethodInfo methodInfo = funcInfo.Info;

            if (methodInfo == null) 
                throw new Exception($"[Error at line {funcCall.Line}]: Cannot call undefined function '{funcCall.FunctionName}'");

            if(funcCall.GenericTypes.Count != 0 && methodInfo.IsGenericMethod)
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

        private FunctionInfo FindFunctionInfo(string funcName, int argsCount, int line)
        {
            //TODO: make ability to interact with another modules
            //look for it in current module
            foreach (FunctionInfo info in _currentModule.Functions)
            {
                if (info.Name == funcName)
                {
                    if (info.Args.Count != argsCount && argsCount != -1)
                        throw new Exception($"[Error at line {line}]: Function '{funcName}' called with incorrect number of arguments (expected {info.Args.Count}, got {argsCount})");

                    return info;
                }
            }

            return null;
        }

        void EmitCast(ILGenerator il, Type sourceType, Type targetType)
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

                    var t when t == typeof(bool) => OpCodes.Conv_I4,

                    _ => throw new NotSupportedException($"Cannot convert from {sourceType} to {targetType}")
                };
                il.Emit(convOp);
            }

            //value to ref
            else if(sourceIsValue && !targetIsValue)
            {
                il.Emit(OpCodes.Box, sourceType);
                if(targetType != typeof(object))
                    il.Emit(OpCodes.Castclass, targetType);
            }

            //ref to value
            else if(!sourceIsValue && targetIsValue)
            {
                il.Emit(OpCodes.Unbox_Any, targetType);
            }

            //ref to ref
            else
            {
                il.Emit(OpCodes.Castclass, targetType);
            }
        }

        Type GetExpressionType(ASTNode expr, TranslationContext ctx)
        {
            switch (expr)
            {
                case ASTConstant<int>: return typeof(int);
                case ASTConstant<float>: return typeof(float);
                case ASTConstant<bool>: return typeof(bool);

                case ASTVariableUse varUse: return ctx.Vars.GetVariable(varUse.VariableName).LocalType;

                case ASTCast staticCast: return ResolveType(staticCast.TypeName, ctx, staticCast.Line);

                case ASTBinOperation binOp: return GetExpressionType(binOp.Left, ctx);
                case ASTUnOperation unOp: return GetExpressionType(unOp.Operand, ctx);

                case ASTFunctionCall funcCall:
                    {
                        FunctionInfo funcInfo = FindFunctionInfo(funcCall.FunctionName, funcCall.Args.Count, funcCall.Line)
                            ?? throw new Exception($"[Error at line {funcCall.Line}]: Cannot call undefined function '{funcCall.FunctionName}'");

                        int indexOfGenericType = Array.IndexOf(funcInfo.GenericMap.Keys.ToArray(), funcInfo.ReturnType);

                        if (indexOfGenericType != -1)
                            return ResolveType(funcCall.GenericTypes[indexOfGenericType], ctx, funcCall.Line);
                        else
                            return ResolveType(funcInfo.ReturnType, ctx, funcCall.Line);
                    }

                default: throw new Exception($"[Error at line {expr.Line}]: Cannot deduce expression type ({expr.GetType().Name})");
            }
        }

        Type ResolveType(string typeName, TranslationContext ctx, int line)
        {
            if (ctx.GenericMap.TryGetValue(typeName, out var type))
                return type;

            else
            {
                //TODO: Add parser for generic types!!!
                return typeName switch
                {
                    "void" => typeof(void),

                    "sbyte" => typeof(sbyte),
                    "byte" => typeof(byte),
                    "short" => typeof(short),
                    "ushort" => typeof(ushort),
                    "int" => typeof(int),
                    "uint" => typeof(uint),
                    "long" => typeof(long),
                    "ulong" => typeof(ulong),
                    "float" => typeof(float),
                    "double" => typeof(double),

                    "bool" => typeof(bool),

                    _ => throw new Exception($"[Error at line {line}]: Unknown type '{typeName}'"),
                };
            }
        }

        private void DefineModule(ref List<ModuleInfo> modules, string moduleName, ASTCodeBlock block)
        {
            ModuleInfo mod = new ModuleInfo(moduleName, _currentModule, _moduleBuilder.DefineType(moduleName, TypeAttributes.Class | TypeAttributes.Public));
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
            }
            modules.Add(mod);
        }

    }
}
