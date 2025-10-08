using System;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace KoalaLang.Translators
{
    public class VariablesController()
    {
        private Dictionary<string, LocalBuilder> _vars = new();
        private Dictionary<Type, Stack<LocalBuilder>> _free = new();

        internal void DeclareVariable(ILGenerator il, string name, Type type, int line = -1)
        {
            if (_vars.ContainsKey(name)) throw new Exception($"[Error at line {line}]: Variable '{name}' already exists");

            if (_free.ContainsKey(type))
            {
                LocalBuilder loc;
                _free[type].TryPop(out loc);
                if(loc != null)
                {
                    _vars.Add(name, loc);
                    return;
                }
            }

            LocalBuilder local = il.DeclareLocal(type);
            _vars.Add(name, local);
        }

        internal void Free(ILGenerator il, string name)
        {
            if (!VarExists(name)) return;

            LocalBuilder loc = GetVariable(name);
            if (loc == null) return;

            il.Emit(OpCodes.Ldnull);
            il.Emit(OpCodes.Stloc, loc);

            Type locType = loc.LocalType;
            if (!_free.ContainsKey(locType))
            {
                _free.Add(locType, new Stack<LocalBuilder>());
            }
            _free[locType].Push(loc);

            _vars.Remove(name);
        }

        internal LocalBuilder GetVariable(string name) => _vars[name];
        internal bool VarExists(string name) => _vars.ContainsKey(name);
    }
}
