using NLua;
using SomethingNeedDoing.LuaMacro.Wrappers;

namespace SomethingNeedDoing.LuaMacro.Modules;

public unsafe class AddonModule : LuaModuleBase
{
    public override string ModuleName => "Addons";

    [LuaFunction] public AddonWrapper GetAddon(string name) => new(name);
}
