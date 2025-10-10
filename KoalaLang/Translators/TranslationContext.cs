using System;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace KoalaLang.Translators
{
    internal sealed class TranslationContext
    {
        internal ILGenerator IL;
        internal VariablesController Vars;
        internal Dictionary<string, GenericTypeParameterBuilder> GenericMap;
        internal Label? RegionStart;
        internal Label? RegionEnd;
        internal FunctionInfo CurrentFunction;

        private List<string> _kidsLocals = null;

        public TranslationContext(ILGenerator il, FunctionInfo funcInfo)
        {
            IL = il;
            Vars = new();
            GenericMap = funcInfo == null ? new() : funcInfo.GenericMap;
            CurrentFunction = funcInfo;
        }

        public TranslationContext CreateKid()
        {
            return new TranslationContext(IL, CurrentFunction)
            {
                RegionStart = RegionStart,
                RegionEnd = RegionEnd,
                GenericMap = GenericMap,
                Vars = Vars,
                _kidsLocals = new(),
            };
        }

        public void DeclareLocalVariable(string name, Type type, int line)
        {
            Vars.DeclareVariable(IL, name, type, line);
            if (_kidsLocals != null) _kidsLocals.Add(name);
        }

        public void Free() => FreeKidsLocals();

        public void FreeKidsLocals()
        {
            if (_kidsLocals == null) return;

            foreach(string loc in _kidsLocals)
            {
                Vars.Free(IL, loc);
            }
        }
    }
}
