using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

namespace KoalaLang.Translators
{
    public sealed class FunctionInfo(string name, string retType, Dictionary<string, string> args)
    {
        internal string Name = name;
        internal string ReturnType = retType;
        internal Dictionary<string, string> Args = args; //name: type
        internal Dictionary<string, GenericTypeParameterBuilder> GenericMap = new();
        internal MethodInfo Info;
    }
}
