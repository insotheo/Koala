using System.Collections.Generic;
using System.Reflection;

namespace KoalaLang.Translators
{
    public sealed class FunctionInfo(string name, string retType, Dictionary<string, string> args)
    {
        internal string Name = name;
        internal string ReturnType = retType;
        internal Dictionary<string, string> Args = args; //name: type
        internal MethodInfo Info;
    }
}
