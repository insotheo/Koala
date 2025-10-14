using System.Collections.Generic;
using System.Reflection.Emit;

namespace KoalaLang.Translators
{
    public sealed class ModuleInfo(string name, ModuleInfo _parentModule, TypeBuilder typeBuilder)
    {
        internal string Name = name;
        internal List<FunctionInfo> Functions = new List<FunctionInfo>();
        internal List<ModuleInfo> Submodules = new List<ModuleInfo>();
        internal ModuleInfo ParentModule = _parentModule;
        internal TypeBuilder @TypeBuilder = typeBuilder;
        internal List<string> Imports = new();

        internal string GetFullName(bool includeSelf = true)
        {
            if(ParentModule == null)
            {
                return Name;
            }
            string parentsFullPath = ParentModule.GetFullName(true);
            string fullPath = parentsFullPath + "." + (includeSelf ? Name : "");
            return fullPath.TrimEnd('.');
        }

        internal List<string> GetImports()
        {
            if(ParentModule != null)
            {
                return ParentModule.GetImports();
            }
            return Imports;
        }
    }
}
