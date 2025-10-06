using System.Collections.Generic;

namespace KoalaLang.Translators
{
    public sealed class ModuleInfo(string name, ModuleInfo _parentModule)
    {
        internal string Name = name;
        internal List<FunctionInfo> Functions = new List<FunctionInfo>();
        internal List<ModuleInfo> Submodules = new List<ModuleInfo>();
        internal ModuleInfo ParentModule = _parentModule;

        internal string GetFullPath(bool includeSelf = true)
        {
            if(ParentModule == null)
            {
                return Name;
            }
            string parentsFullPath = ParentModule.GetFullPath(true);
            string fullPath = parentsFullPath + "." + (includeSelf ? Name : "");
            return fullPath.TrimEnd('.');
        }
    }
}
