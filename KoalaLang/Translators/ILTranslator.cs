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

        public void Translate(Parser parser, string moduleName)
        {
            AssemblyName assemblyName = new AssemblyName(_asmName);
            AssemblyBuilder assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.RunAndCollect);

            ModuleBuilder moduleBuilder = assemblyBuilder.DefineDynamicModule("ApplicationDynamicModule");

            TypeBuilder typeBuilder = moduleBuilder.DefineType(moduleName, TypeAttributes.Public | TypeAttributes.Class);

            DefineModule(ref _mods, ref typeBuilder, moduleName, parser.GetAST() as ASTCodeBlock);
            _currentModule = _mods[0];

            try
            {
                foreach (ASTNode node in (parser.GetAST() as ASTCodeBlock).Nodes)
                {
                    if (node is ASTFunction func)
                    {
                        FunctionInfo funcInfo = _currentModule.Functions.FirstOrDefault(x => x.Name == func.FunctionName, null);
                        if (funcInfo == null) 
                            throw new Exception($"[Error at line {func.Line}]: Function '{func.FunctionName}' was not declared before use");

                        ILGenerator il = (funcInfo.Info as MethodBuilder).GetILGenerator();
                        TranslateBody(il, func.Body, func);
                        il.Emit(OpCodes.Ret);
                    }
                }
            }
            catch(Exception ex)
            {
                Console.Error.WriteLine($"Translator error(at {_currentModule.GetFullName()}): {ex.Message}");
                Environment.Exit(-1);
            }

            Type module = typeBuilder.CreateType();

            //DBG
            Console.WriteLine($"Function Main returned: {module.GetMethod("Main")!.Invoke(null, null)}");
        }

        private void TranslateBody(ILGenerator il, ASTCodeBlock body, ASTFunction func = null, VariablesController baseVarController = null, Label? regionStartLabel = null, Label? regionEndLabel = null)
        {
            VariablesController varController = baseVarController != null ? baseVarController : new();
            List<string> localVariablesNames = new();

            if(func != null && func.Args.Count != 0)
            {
                int index = 0;
                foreach((string argName, string argType) in func.Args)
                {
                    Type type = GetTypeByName(argType, func.Line);

                    varController.DeclareVariable(il, argName, type, func.Line);
                    localVariablesNames.Add(argName);

                    il.Emit(OpCodes.Ldarg, index);
                    il.Emit(OpCodes.Stloc, varController.GetVariable(argName));

                    index += 1;
                }
            }

            foreach(ASTNode node in body.Nodes)
            {
                if (node is ASTReturn ret)
                {
                    TranslateExpression(il, ret.ReturnValue, varController);
                    il.Emit(OpCodes.Ret);
                }

                else if (node is ASTVariableDeclaration varDecl)
                {
                    varController.DeclareVariable(il, varDecl.Name, GetTypeByName(varDecl.Type, varDecl.Line), varDecl.Line);
                    localVariablesNames.Add(varDecl.Name);
                }

                else if (node is ASTAssignment assignment)
                {
                    if (!varController.VarExists(assignment.DestinationName))
                    {
                        throw new Exception($"[Error at line {assignment.Line}]: Cannot assign to undefined variable '{assignment.DestinationName}'");
                    }
                    TranslateExpression(il, assignment.Value, varController);
                    il.Emit(OpCodes.Stloc, varController.GetVariable(assignment.DestinationName));
                }

                else if (node is ASTBranch branch)
                {
                    Label endLb = il.DefineLabel();

                    for(int i = 0; i < branch.Ifs.Length; i++)
                    {
                        ASTConditionBlock ifBlock = branch.Ifs[i];
                        Label nextIfLb = il.DefineLabel();

                        TranslateExpression(il, ifBlock.Condition, varController);
                        il.Emit(OpCodes.Brfalse, nextIfLb); //if conditions is false

                        TranslateBody(il, ifBlock.Body, baseVarController: varController);
                        il.Emit(OpCodes.Br, endLb);

                        il.MarkLabel(nextIfLb);
                    }

                    if(branch.Else != null)
                    {
                        TranslateBody(il, branch.Else, baseVarController: varController);
                    }
                    il.MarkLabel(endLb);
                }

                else if(node is ASTWhileLoop whileLoop)
                {
                    Label loopStart = il.DefineLabel();
                    Label loopEnd = il.DefineLabel();
                    Label loopConditionCheck = il.DefineLabel();

                    il.MarkLabel(loopStart);
                    TranslateBody(il, whileLoop.Body, baseVarController: varController, regionStartLabel: loopConditionCheck, regionEndLabel: loopEnd);

                    il.MarkLabel(loopConditionCheck);
                    TranslateExpression(il, whileLoop.Condition, varController);
                    
                    il.Emit(OpCodes.Brfalse, loopEnd);
                    il.Emit(OpCodes.Br, loopStart);
                    il.MarkLabel(loopEnd);
                }

                else if (node is ASTFunctionCall funcCall) TranslateFunctionCall(il, funcCall, varController);

                else if (node is ASTCodeBlock block) TranslateBody(il, block, baseVarController: varController);

                else if(node is ASTBreak)
                {
                    if (!regionEndLabel.HasValue) throw new Exception($"[Error at line {node.Line}]: No enclosing loop out of which to break or continue");
                    il.Emit(OpCodes.Br, regionEndLabel.Value);
                }
                else if (node is ASTContinue)
                {
                    if (!regionStartLabel.HasValue) throw new Exception($"[Error at line {node.Line}]: No enclosing loop out of which to break or continue");
                    il.Emit(OpCodes.Br, regionStartLabel.Value);
                }
            }

            foreach (string localVar in localVariablesNames)
                varController.Free(il, localVar);
        }

        private void TranslateExpression(ILGenerator il, ASTNode expr, VariablesController varController)
        {
            if (expr == null) return;

            else if (expr is ASTConstant<int> intConst) il.Emit(OpCodes.Ldc_I4, intConst.Value);
            else if (expr is ASTConstant<float> floatConst) il.Emit(OpCodes.Ldc_R4, floatConst.Value);
            else if (expr is ASTConstant<bool> boolConst) il.Emit(boolConst.Value ? OpCodes.Ldc_I4_1 : OpCodes.Ldc_I4_0);

            else if (expr is ASTVariableUse varUse)
            {
                if (!varController.VarExists(varUse.VariableName))
                {
                    throw new Exception($"[Error at line {varUse.Line}]: Variable '{varUse.VariableName}' is used before being defined");
                }
                il.Emit(OpCodes.Ldloc, varController.GetVariable(varUse.VariableName));
            }

            else if (expr is ASTFunctionCall funcCall)
            {
                TranslateFunctionCall(il, funcCall, varController);
            }

            else if (expr is ASTBinOperation binOp)
            {
                TranslateExpression(il, binOp.Left, varController);
                TranslateExpression(il, binOp.Right, varController);

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
                TranslateExpression(il, unOp.Operand, varController);

                switch (unOp.OperationType)
                {
                    case UnaryOperationType.Negate: il.Emit(OpCodes.Neg); break;
                    case UnaryOperationType.LogicalNot: il.Emit(OpCodes.Ldc_I4_1); il.Emit(OpCodes.Xor); break;
                    case UnaryOperationType.BitwiseNot: il.Emit(OpCodes.Not); break;

                    default: throw new Exception($"[Error at line {unOp.Line}]: Unknown unary operation '{unOp.OperationType}'");
                }
            }

            else throw new Exception($"[Error at line {expr.Line}]: Unknown expression type '{expr.GetType().Name}'");
        }

        private void TranslateFunctionCall(ILGenerator il, ASTFunctionCall funcCall, VariablesController varController)
        {
            MethodInfo methodInfo = null;
            string shortName = funcCall.FunctionName;

            //TODO: make ability to interact with another modules
            //look for it in current module
            foreach(FunctionInfo info in _currentModule.Functions)
            {
                if (info.Name == shortName)
                {
                    if(info.Args.Count != funcCall.Args.Count)
                    {
                        throw new Exception($"[Error at line {funcCall.Line}]: Function '{shortName}' called with incorrect number of arguments (expected {info.Args.Count}, got {funcCall.Args.Count})");
                    }

                    methodInfo = info.Info;
                    break;
                }
            }

            if (methodInfo == null)
            {
                throw new Exception($"[Error at line {funcCall.Line}]: Cannot call undefined function '{shortName}'");
            }

            //loading args
            foreach (ASTNode arg in funcCall.Args)
            {
                TranslateExpression(il, arg, varController);
            }

            //call
            il.Emit(OpCodes.Call, methodInfo);
        }

        private void DefineModule(ref List<ModuleInfo> modules, ref TypeBuilder typeBuilder, string moduleName, ASTCodeBlock block)
        {
            ModuleInfo mod = new ModuleInfo(moduleName, _currentModule);
            foreach (ASTNode node in block.Nodes)
            {
                if(node is ASTFunction func)
                {
                    Type[] args = new Type[func.Args.Count];
                    {
                        int i = 0;
                        foreach (var (argName, argValue) in func.Args)
                        {
                            args[i] = GetTypeByName(argValue, func.Line);
                            i += 1;
                        }
                    }

                    MethodBuilder methodBuilder = typeBuilder.DefineMethod(
                        func.FunctionName,
                        MethodAttributes.Public | MethodAttributes.Static,
                        GetTypeByName(func.ReturnTypeName, func.Line),
                        func.Args.Count == 0 ? Type.EmptyTypes : args
                    );

                    FunctionInfo funcInfo = new(func.FunctionName, func.ReturnTypeName, func.Args);
                    funcInfo.Info = methodBuilder;
                    mod.Functions.Add(funcInfo);
                }
            }
            modules.Add(mod);
        }

        private Type GetTypeByName(string typeName, int line) {
            return typeName switch
            {
                "void" => typeof(void),

                "int" => typeof(int),
                "float" => typeof(float),

                "bool" => typeof(bool),

                _ => throw new Exception($"[Error at line {line}]: Unknown type '{typeName}'"),
            };
        }

    }
}
